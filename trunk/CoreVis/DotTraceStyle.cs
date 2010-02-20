/*
* Copyright (c) 2007-2010 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("Per-Thread Call Trees")]
	public partial class DotTraceStyle : UserControl, IVisualizer
	{
		const string kParentHits = @"
SELECT SUM(HitCount)
FROM Callers
WHERE CallerId = {0} AND ThreadId = {1}
";

		const string kTopLevelQuery = @"
SELECT F.Id, C.ThreadId, C.HitCount, T.Name AS ""ThreadName"", F.Name AS ""Function"", F.Signature AS ""Signature""
FROM Callers C
JOIN Functions F
	ON C.CalleeId = F.Id
LEFT OUTER JOIN Threads T
	ON T.Id = C.ThreadId
WHERE C.CallerId = 0
ORDER BY C.HitCount DESC
";

		const string kChildQuery = @"
SELECT C1.CalleeId AS ""Id"", HitCount, Name AS ""Function"", Signature, CASE TotalCalls
	WHEN 0 THEN 0
	ELSE (1.0 * C1.HitCount / TotalCalls)
	END AS ""Percent""
FROM Callers C1
JOIN Functions
	ON C1.CalleeId = Id
JOIN (SELECT CallerId, SUM(HitCount) AS ""TotalCalls"" FROM Callers WHERE ThreadId = {1} GROUP BY CallerId) C2
	ON C1.CallerId = C2.CallerId
WHERE C1.CallerId = {0} AND ThreadId = {1}
ORDER BY HitCount DESC
";

		const string kExclusiveQuery = @"
SELECT HitCount, CASE TotalCalls
	WHEN 0 THEN 0
	ELSE (1.0 * C1.HitCount / TotalCalls)
	END AS ""Percent""
FROM Callers C1
JOIN (
	SELECT CallerId, SUM(HitCount) AS ""TotalCalls""
	FROM Callers
	WHERE ThreadId = {1}
	GROUP BY CallerId
) C2
	ON C1.CallerId = C2.CallerId
WHERE C1.CallerId = {0} AND C1.CalleeId = 0 AND C1.ThreadId = {1}
";

		class NodeData
		{
			public int Id;
			public int ThreadId;
			public string Name;
			public decimal Percent;
			public string FormattedText;
			public string TipText;

			public NodeData(int id, int threadId, string name, decimal percent, string formattedText, string tipText)
			{
				Id = id;
				ThreadId = threadId;
				Name = name;
				Percent = percent;
				FormattedText = formattedText;
				TipText = tipText;
			}
		}

		struct FontSet
		{
			public List<Font> Fonts;
			public List<Brush> Brushes;

			public static FontSet Create()
			{
				FontSet fs = new FontSet();
				fs.Fonts = new List<Font>();
				fs.Brushes = new List<Brush>();
				return fs;
			}

			public void Add(Font font, Brush brush)
			{
				Fonts.Add(font);
				Brushes.Add(brush);
			}
		}

		Regex m_regex;
		FontSet m_normalFonts;
		FontSet m_filteredFonts;

		ProfilerWindowBase m_mainWindow;
		Connection m_connection;

		public string DisplayName
		{
			get { return "Thread Calls"; }
		}

		public DotTraceStyle()
		{
			InitializeComponent();

			m_regex = new Regex(@"\\([0-9])");
			const string fontName = "Arial";
			const int fontSize = 9;

			m_normalFonts = FontSet.Create();
			m_normalFonts.Add(new Font(fontName, fontSize, FontStyle.Regular), Brushes.Black);
			m_normalFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.Blue);
			m_normalFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.Black);
			m_normalFonts.Add(new Font(fontName, fontSize, FontStyle.Regular), Brushes.Blue);
			m_normalFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.Green);

			m_filteredFonts = FontSet.Create();
			m_filteredFonts.Add(new Font(fontName, fontSize, FontStyle.Regular), Brushes.DimGray);
			m_filteredFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.DimGray);
			m_filteredFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.DimGray);
			m_filteredFonts.Add(new Font(fontName, fontSize, FontStyle.Regular), Brushes.DimGray);
			m_filteredFonts.Add(new Font(fontName, fontSize, FontStyle.Bold), Brushes.DimGray);
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;

			UpdateTopLevel();
			return true;
		}

		public void Show(Control.ControlCollection parent)
		{
			this.Dock = DockStyle.Fill;
			parent.Add(this);
		}

		public void OnClose()
		{
		}

		private static void BreakName(string name, out string signature, out string funcName, out string classAndFunc, out string baseName)
		{
			int sigIndex = name.IndexOf('(');
			signature = sigIndex >= 0 ? name.Substring(sigIndex) : string.Empty;
			if(sigIndex < 0)
				sigIndex = name.Length - 1;

			int funcNameIndex = name.LastIndexOf('.', sigIndex) + 1;
			if(funcNameIndex <= 1)
			{
				//special case for ..ctor mainly
				funcName = name.Substring(0, sigIndex);
				classAndFunc = funcName;
				baseName = string.Empty;
				return;
			}
			funcName = funcNameIndex >= 0 ? name.Substring(funcNameIndex, sigIndex - funcNameIndex) : string.Empty;

			int classIndex = name.LastIndexOf('.', funcNameIndex - 2) + 1;
			classAndFunc = classIndex >= 1 ? name.Substring(classIndex, sigIndex - classIndex) : funcName;
			if(classIndex <= 1)
				classIndex = 0;

			baseName = name.Substring(0, classIndex);
		}

		private bool IsFiltered(string name)
		{
			if(string.IsNullOrEmpty(name))
				return false;

			//TODO: Replace with proper filters
			if(m_filterSystemMenu.Checked && name.StartsWith("System."))
				return true;
			if(m_filterMicrosoftMenu.Checked && name.StartsWith("Microsoft."))
				return true;

			return false;
		}

		private void UpdateTopLevel()
		{
			m_extraInfoTextBox.Text = string.Empty;

			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				var data = m_connection.StorageEngine.Query(kTopLevelQuery);

				int totalHits = 0;
				foreach(DataRow row in data.Tables[0].Rows)
				{
					totalHits += Convert.ToInt32(row["HitCount"]);
				}

				foreach(DataRow row in data.Tables[0].Rows)
				{
					string name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);

					string rawString = @"{0:P2} Thread {1} - {2}{3}{4}";
					string tipString = "{0:P2} - Thread {1}\r\nEntry point: {2}{3}{4}";
					string niceString = @"\1{0:P3} \2Thread {1} \0- {2}\2{3}\0{4}";

					string signature, funcName, classAndFunc, baseName;
					BreakName(name, out signature, out funcName, out classAndFunc, out baseName);
					decimal percent = totalHits == 0 ? 0 : Convert.ToInt32(row["HitCount"]) / (decimal) totalHits;
					int threadId = Convert.ToInt32(row["ThreadId"]);
					string threadName = Convert.ToString(row["ThreadName"]);
					if(string.IsNullOrEmpty(threadName))
						threadName = "#" + threadId;

					string nodeText = string.Format(rawString, percent, threadName, baseName, classAndFunc, signature);
					string tipText = string.Format(tipString, percent, threadName, baseName, classAndFunc, signature);
					string formatText = string.Format(niceString, percent, threadName, baseName, classAndFunc, signature);

					TreeNode newNode = new TreeNode(nodeText, new TreeNode[] { new TreeNode("dummy") });
					newNode.Tag = new NodeData(Convert.ToInt32( row["Id"]), threadId, string.Empty, 1, formatText, tipText);
					m_treeView.Nodes.Add(newNode);
				}
			}
		}

		private void UpdateChildren(TreeNode node)
		{
			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				var parent = (NodeData) node.Tag;
				var data = m_connection.StorageEngine.Query(string.Format(kChildQuery, parent.Id, parent.ThreadId));

				//find out what the current number of calls by the parent is
				//var parentHits = Convert.ToInt32(m_connection.StorageEngine.QueryScalar(string.Format(kParentHits, parent.Id, parent.ThreadId)));
				foreach(DataRow row in data.Tables[0].Rows)
				{
					string name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);
					string rawString = @"{0:P2} {1} - {2:P2} - {3}{4}{5}";
					string tipString = "[Id {6}] {3}{4}{5}\r\n{0:P3} of thread - {1} samples - {2:P3} of parent\r\n{7:P3} outside children - {8} samples";
					string niceString = @"\1{0:P2} \2{1} \0- \3{2:P2} \0- {3}\2{4}\0{5}";

					string signature, funcName, classAndFunc, baseName;
					BreakName(name, out signature, out funcName, out classAndFunc, out baseName);
					decimal percentOfParent = Convert.ToDecimal(row["Percent"]);
					decimal percent = percentOfParent * parent.Percent;
					int id = Convert.ToInt32(row["Id"]);

					//find out how much time was spent exclusive of children
					//we don't have children at this point so we have to query separately
					var excl = m_connection.StorageEngine.Query(string.Format(kExclusiveQuery, id, parent.ThreadId));
					var hasExcl = excl.Tables[0].Rows.Count > 0;
					var exclRow = hasExcl ? excl.Tables[0].Rows[0] : null;
					double exclPercent = exclRow != null ? Convert.ToDouble(exclRow["Percent"]) : 0.0;
					int exclHits = exclRow != null ? Convert.ToInt32(exclRow["HitCount"]) : 0;

					string nodeText = string.Format(rawString, percent, funcName, percentOfParent, baseName, classAndFunc, signature);
					string tipText = string.Format(tipString, percent, Convert.ToInt32(row["HitCount"]), percentOfParent,
						baseName, classAndFunc, signature, id, exclPercent, exclHits);
					string formatText = string.Format(niceString, percent, funcName, percentOfParent,
						baseName, classAndFunc, signature);

					TreeNode newNode = new TreeNode(nodeText, new TreeNode[] { new TreeNode() });
					newNode.Tag = new NodeData(id, (int) parent.ThreadId, name, percent, formatText, tipText);
					node.Nodes.Add(newNode);
				}
			}
		}

		private void m_treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			e.Node.Nodes.Clear();
			UpdateChildren(e.Node);
			if(e.Node.Nodes.Count == 0)
				e.Node.Collapse();
			else if(e.Node.Nodes.Count == 1)
				e.Node.Nodes[0].Expand();
		}

		private void m_treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			NodeData data = e.Node.Tag as NodeData;
			if(data == null)
			{
				e.DrawDefault = true;
				return;
			}

			string text = data.FormattedText;
			var graphics = e.Graphics;
			Font currentFont = e.Node.NodeFont ?? e.Node.TreeView.Font;
			Brush currentBrush = (e.State & TreeNodeStates.Selected) != 0 ? Brushes.White : Brushes.Black;
			FontSet fontSet = IsFiltered(data.Name) ? m_filteredFonts : m_normalFonts;

			var matches = m_regex.Matches(text);
			if(matches.Count == 0)
			{
				e.DrawDefault = true;
				return;
			}

			bool parentSelected = e.Node.Parent != null && e.Node.Parent.IsSelected;
			int rectX = e.Bounds.X > 0 ? e.Bounds.X : 0;
			if(parentSelected)
				graphics.FillRectangle(Brushes.AliceBlue, rectX, e.Bounds.Y, m_treeView.Width, e.Bounds.Height);
			else if((e.State & TreeNodeStates.Selected) != 0)
				graphics.FillRectangle(SystemBrushes.Highlight, rectX, e.Bounds.Y, m_treeView.Width, e.Bounds.Height);

			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0].First = 0;
			StringFormat format = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);

			int offset = 0;
			float drawPos = e.Bounds.X;
			foreach(Match m in matches)
			{
				if(m.Index != offset)
				{
					string substr = text.Substring(offset, m.Index - offset);
					graphics.DrawString(substr, currentFont, currentBrush, drawPos, e.Bounds.Y);

					ranges[0].Length = substr.Length;
					format.SetMeasurableCharacterRanges(ranges);
					var regions = graphics.MeasureCharacterRanges(substr, currentFont, new RectangleF(0, 0, 1000, 1000), format);
					var rect = regions[0].GetBounds(graphics);
					drawPos += rect.Width;
				}

				offset = m.Index + m.Length;
				int index = int.Parse(m.Groups[1].Value);
				currentFont = fontSet.Fonts[index];
				if((e.State & TreeNodeStates.Selected) == 0)
					currentBrush = fontSet.Brushes[index];
			}
			string final = text.Substring(offset);
			graphics.DrawString(final, currentFont, currentBrush, drawPos, e.Bounds.Y);
		}

		private void m_treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			foreach(TreeNode child in e.Node.Nodes)
			{
				int rectX = child.Bounds.X > 0 ? child.Bounds.X : 0;
				Rectangle rect = new Rectangle(rectX, child.Bounds.Y, m_treeView.Width, child.Bounds.Height);
				m_treeView.Invalidate(rect);
			}

			//Update the extra info text
			var node = e.Node;
			if(node != null && node.Tag != null)
			{
				NodeData data = node.Tag as NodeData;
				m_extraInfoTextBox.Text = data.TipText;
			}
			else
			{
				m_extraInfoTextBox.Text = string.Empty;
			}
		}

		private void m_treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if(m_treeView.SelectedNode == null)
				return;

			foreach(TreeNode child in m_treeView.SelectedNode.Nodes)
			{
				int rectX = child.Bounds.X > 0 ? child.Bounds.X : 0;
				Rectangle rect = new Rectangle(rectX, child.Bounds.Y, m_treeView.Width, child.Bounds.Height);
				m_treeView.Invalidate(rect);
			}
		}

		private void m_treeView_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			//clear the node's children back to dummy
			e.Node.Nodes.Clear();
			e.Node.Nodes.Add(new TreeNode());
		}

		private void m_refreshButton_Click(object sender, EventArgs e)
		{
			m_treeView.Nodes.Clear();
			UpdateTopLevel();
		}

		private void FilterMenu_Click(object sender, EventArgs e)
		{
			m_treeView.Invalidate();
		}

		private void SnapshotCombo_Click(object sender, EventArgs e)
		{
			SnapshotCombo.Items.Clear();
			SnapshotCombo.Items.Add("Current");
			//get a list of snapshots from the storage engine -- this can fail on 0.1.x files
			try
			{
				var snapshots = m_connection.StorageEngine.Query("SELECT * FROM Snapshots");
				foreach(DataRow row in snapshots.Tables[0].Rows)
				{
					string text = string.Format("{0} - {1}", row[1], row[2]);
					SnapshotCombo.Items.Add(text);
				}
			}
			catch
			{
				//no need to do anything
			}
		}
	}
}
