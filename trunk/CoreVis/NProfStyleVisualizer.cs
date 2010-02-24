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
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;

using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("NProf-Style TreeViews")]
	public partial class NProfStyleVisualizer : UserControl, IVisualizer
	{
		ProfilerWindowBase m_mainWindow;
		Connection m_connection;

		CalleesModel m_calleesModel;
		CallersModel m_callersModel;

		public string DisplayName
		{
			get { return "Tree Views"; }
		}

		public NProfStyleVisualizer()
		{
			InitializeComponent();
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;

			m_calleesModel = new CalleesModel(connection.DataEngine);
			m_callersModel = new CallersModel(connection.DataEngine);
			m_callees.Model = new SortedTreeModel(m_calleesModel);
			m_callers.Model = new SortedTreeModel(m_callersModel);

			//set the sort orders
			ColumnClicked(m_callees, new TreeColumnEventArgs(m_calleesPercentParentColumn));
			ColumnClicked(m_callees, new TreeColumnEventArgs(m_calleesPercentParentColumn));
			ColumnClicked(m_callers, new TreeColumnEventArgs(m_callersPercentTimeColumn));
			ColumnClicked(m_callers, new TreeColumnEventArgs(m_callersPercentTimeColumn));

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

		private void m_refreshButton_Click(object sender, EventArgs e)
		{
			m_calleesModel.Refresh();
			m_callersModel.Refresh();
		}

		private void ColumnClicked(object sender, TreeColumnEventArgs e)
		{
			TreeColumn clicked = e.Column;

			if(clicked.SortOrder == SortOrder.None)
				clicked.SortOrder = SortOrder.Descending;
			else if(clicked.SortOrder == SortOrder.Descending)
				clicked.SortOrder = SortOrder.Ascending;
			else if(clicked.SortOrder == SortOrder.Ascending)
				clicked.SortOrder = SortOrder.None;

			var tree = sender as TreeViewAdv;
			(tree.Model as SortedTreeModel).Comparer = new FunctionComparer(this, clicked, clicked.SortOrder);
		}

		class FunctionComparer : System.Collections.IComparer, IComparer<FunctionItem>
		{
			//this is incredibly stupid
			public NProfStyleVisualizer Parent;
			public TreeColumn Column;
			public SortOrder Order;

			public FunctionComparer(NProfStyleVisualizer parent, TreeColumn column, SortOrder order)
			{
				this.Parent = parent;
				this.Column = column;
				this.Order = order;
			}

			private static int Compare(decimal? x, decimal? y)
			{
				if(x.HasValue && y.HasValue)
					return x.Value.CompareTo(y.Value);
				else
					return x.HasValue.CompareTo(y.HasValue);
			}

			public int Compare(FunctionItem x, FunctionItem y)
			{
				//yeah, this is awful
				int result = 0;
				if(Column == Parent.m_calleesIdColumn || Column == Parent.m_callersIdColumn)
					result = x.Id.CompareTo(y.Id);
				else if(Column == Parent.m_calleesThreadIdColumn || Column == Parent.m_callersThreadIdColumn)
					result = x.Thread.CompareTo(y.Thread);
				else if(Column == Parent.m_calleesNameColumn || Column == Parent.m_callersNameColumn)
					result = x.Name.CompareTo(y.Name);
				else if(Column == Parent.m_calleesPercentParentColumn || Column == Parent.m_callersPercentTimeColumn)
					result = Compare(x.PercentTime, y.PercentTime);
				else if(Column == Parent.m_calleesPercentCallsColumn || Column == Parent.m_callersPercentCallsColumn)
					result = Compare(x.PercentCalls, y.PercentCalls);

				//if primary sort is not differentiating, go to secondary sort criteria (hard coded for now)
				if(result == 0)
					result = x.Thread.CompareTo(y.Thread);
				if(result == 0)
					result = -Compare(x.PercentTime, y.PercentTime);
				if(result == 0)
					result = -Compare(x.PercentCalls, y.PercentCalls);
				if(result == 0)
					result = -x.Name.CompareTo(y.Name);
				if(result == 0)
					result = x.Id.CompareTo(y.Id);

				if(Order == SortOrder.Ascending)
					result = -result;
				return result;
			}

			public int Compare(object x, object y)
			{
				return Compare(x as FunctionItem, y as FunctionItem);
			}
		}
	}

	class FunctionItem
	{
		public int Id { get; set; }
		public int Thread { get; set; }
		public string Name { get; set; }
		public int HitCount { get; set; }
		public decimal? PercentTime { get; set; }
		public decimal? PercentCalls { get; set; }
	}

	class CalleesModel : ITreeModel
	{
		const string kParentHits = @"
SELECT SUM(HitCount)
FROM Callers
WHERE CallerId = {0} AND ThreadId = {1}
";

		const string kTopLevelQuery = @"
SELECT Samples.ThreadId, Id, HitCount, Name AS ""Function"", Signature, CASE TotalHits
	WHEN 0 THEN 0
	ELSE (1.0 * HitCount / TotalHits)
	END AS ""Percent""
FROM Samples
JOIN Functions
	ON Id = FunctionId
JOIN (SELECT ThreadId, MAX(HitCount) AS ""TotalHits"" FROM Samples GROUP BY ThreadId) AS ""Totals""
	ON Samples.ThreadId = Totals.ThreadId
ORDER BY HitCount DESC
";

		const string kChildQuery = @"
SELECT C1.CalleeId, HitCount, Name AS ""Function"", Signature, CASE TotalCalls
	WHEN 0 THEN 0
	ELSE (1.0 * C1.HitCount / TotalCalls)
	END AS ""% Calls""
FROM Callers AS ""C1""
JOIN Functions
	ON C1.CalleeId = Id
JOIN (
	SELECT CalleeId, SUM(HitCount) AS ""TotalCalls""
	FROM Callers
	GROUP BY CalleeId
) AS ""C2""
	ON C1.CalleeId = C2.CalleeId
WHERE C1.CallerId = {0} AND ThreadId = {1}
ORDER BY HitCount DESC
";

		IDataEngine m_data;

		public CalleesModel(IDataEngine data)
		{
			m_data = data;
		}

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			using(var transact = new TransactionHandle(m_data))
			{
				if(treePath.IsEmpty())
				{
					//top level queries
					var data = m_data.Query(kTopLevelQuery);

					foreach(DataRow row in data.Tables[0].Rows)
					{
						var item = new FunctionItem();
						item.Id = Convert.ToInt32(row["Id"]);
						item.Thread = Convert.ToInt32(row["ThreadId"]);
						item.Name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);
						item.HitCount = Convert.ToInt32(row["HitCount"]);
						item.PercentTime = Math.Round(100 * Convert.ToDecimal(row["Percent"]), 3);
						yield return item;
					}
				}
				else
				{
					var parent = treePath.LastNode as FunctionItem;
					var data = m_data.Query(string.Format(kChildQuery, parent.Id, parent.Thread));

					//find out what the current number of calls by the parent is
					var parentHits = Convert.ToInt32(m_data.QueryScalar(string.Format(kParentHits, parent.Id, parent.Thread)));
					foreach(DataRow row in data.Tables[0].Rows)
					{
						var item = new FunctionItem();
						item.Thread = Convert.ToInt32(parent.Thread);
						item.Id = Convert.ToInt32(row["CalleeId"]);
						item.Name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);
						item.HitCount = Convert.ToInt32(row["HitCount"]);
						if(parentHits == 0)
							item.PercentTime = 0;
						else
							item.PercentTime = Math.Round(100 * (decimal) item.HitCount / (decimal) parentHits, 3);
						item.PercentCalls = Math.Round(100 * Convert.ToDecimal(row["% Calls"]), 3);
						yield return item;
					}
				}

				yield break;
			}
		}

		public bool IsLeaf(TreePath treePath)
		{
			return false;
		}

		public void Refresh()
		{
			StructureChanged(this, new TreePathEventArgs());
		}

#pragma warning disable 67
		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;
#pragma warning restore
	}

	class CallersModel : ITreeModel
	{
		const string kTopLevelQuery = @"
SELECT Callers.ThreadId, Id, Name AS ""Function"", Signature, HitCount, CASE TotalHits
	WHEN 0 THEN 0
	ELSE (1.0 * HitCount / TotalHits)
	END AS ""Percent""
FROM Callers
JOIN Functions
	ON Id = CallerId
JOIN (SELECT ThreadId, SUM(HitCount) AS ""TotalHits"" FROM Callers WHERE CalleeId = 0 GROUP BY ThreadId) AS ""Totals""
	ON Callers.ThreadId = Totals.ThreadId
WHERE CalleeId = 0
ORDER BY HitCount DESC
";

		const string kChildQuery = @"
SELECT Id, HitCount, Name AS ""Function"", Signature, CASE TotalCalls
	WHEN 0 THEN 0
	ELSE (1.0 * HitCount / TotalCalls)
	END AS ""Percent""
FROM Callers
JOIN Functions
	ON Id = CallerId
JOIN (
	SELECT CalleeId, SUM(HitCount) AS ""TotalCalls""
	FROM Callers
	WHERE CalleeId = {0} AND ThreadId = {1}
) AS ""Totals""
	ON Callers.CalleeId = Totals.CalleeId
WHERE Callers.ThreadId = {1}
ORDER BY HitCount DESC
";

		IDataEngine m_data;

		public CallersModel(IDataEngine data)
		{
			m_data = data;
		}

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			using(var transact = new TransactionHandle(m_data))
			{
				if(treePath.IsEmpty())
				{
					//top level queries
					var data = m_data.Query(kTopLevelQuery);

					foreach(DataRow row in data.Tables[0].Rows)
					{
						var item = new FunctionItem();
						item.Id = Convert.ToInt32(row["Id"]);
						item.Thread = Convert.ToInt32(row["ThreadId"]);
						item.Name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);
						item.HitCount = Convert.ToInt32(row["HitCount"]);
						item.PercentTime = Math.Round(100 * Convert.ToDecimal(row["Percent"]), 3);
						yield return item;
					}
				}
				else
				{
					var parent = treePath.LastNode as FunctionItem;
					var data = m_data.Query(string.Format(kChildQuery, parent.Id, parent.Thread));

					foreach(DataRow row in data.Tables[0].Rows)
					{
						var item = new FunctionItem();
						item.Thread = Convert.ToInt32(parent.Thread);
						item.Id = Convert.ToInt32(row["Id"]);
						item.Name = Convert.ToString(row["Function"]) + Convert.ToString(row["Signature"]);
						item.HitCount = Convert.ToInt32(row["HitCount"]);
						item.PercentCalls = Math.Round(100 * Convert.ToDecimal(row["Percent"]), 3);
						yield return item;
					}
				}

				yield break;
			}
		}

		public bool IsLeaf(TreePath treePath)
		{
			return false;
		}

		public void Refresh()
		{
			StructureChanged(this, new TreePathEventArgs());
		}

#pragma warning disable 67
		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;
#pragma warning restore
	}
}
