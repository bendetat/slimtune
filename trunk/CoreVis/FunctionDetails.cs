using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using ZedGraph;
using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("Function Details")]
	public partial class FunctionDetails : UserControl, IVisualizer
	{
		private const string kSearchQuery = @"
SELECT Id, Name
FROM Functions F
JOIN (
	SELECT CallerId, SUM(HitCount) ""HitCount""
	FROM Callers
	WHERE CalleeId = 0
	GROUP BY CallerId
) C
ON F.Id = C.CallerId
WHERE Name LIKE '%{0}%'
ORDER BY HitCount DESC
";

		const string kInFunctionQuery = @"
SELECT HitCount
FROM Callers
WHERE CallerId = {0} AND CalleeId = 0
";
		const string kCalleesQuery = @"
SELECT C.CalleeId, Name, HitCount
FROM Functions
JOIN (
	SELECT CalleeId, SUM(HitCount) HitCount
	FROM Callers
	WHERE CallerId = {0}
	GROUP BY CalleeId
) C
ON Id = CalleeId
ORDER BY HitCount DESC
";

		ProfilerWindowBase m_mainWindow;
		Connection m_connection;
		ColorRotator m_colors = new ColorRotator();

		public string DisplayName
		{
			get { return Utilities.GetDisplayName(typeof(FunctionDetails)); }
		}

		public FunctionDetails()
		{
			InitializeComponent();
		}

		public void Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;
			UpdateFunctionList();

			mainWindow.Visualizers.Add(this);
		}

		public void Show(Control.ControlCollection parent)
		{
			this.Dock = DockStyle.Fill;
			parent.Add(this);
		}

		private void UpdateFunctionList()
		{
			FunctionList.Items.Clear();
			var data = m_connection.StorageEngine.Query(string.Format(kSearchQuery, SearchBox.Text), 250);
			foreach(DataRow row in data.Tables[0].Rows)
			{
				int id = Convert.ToInt32(row["Id"]);
				string name = Convert.ToString(row["Name"]);
				FunctionList.Items.Add(new FunctionEntry(id, name));
			}
		}

		private void SearchBox_TextChanged(object sender, EventArgs e)
		{
			UpdateFunctionList();
		}

		private void FunctionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			var entry = FunctionList.SelectedItem as FunctionEntry;
			if(entry == null)
				return;

			GraphPane pane = DetailsGraph.GraphPane;
			pane.CurveList.Clear();
			pane.Title.Text = "Function Breakdown (samples)";

			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				//find time in function
				//SQLite can't do RIGHT OUTER JOIN and I can't figure out how to get this with a LEFT OUTER JOIN.
				//so I'm just sending two queries instead
				var inFunc = Convert.ToInt32(m_connection.StorageEngine.QueryScalar(string.Format(kInFunctionQuery, entry.Id)));
				if(inFunc > 0)
					pane.AddPieSlice((double) inFunc, m_colors.ColorForIndex(0), 0.0, "(self)");

				var data = m_connection.StorageEngine.Query(string.Format(kCalleesQuery, entry.Id));
				int index = 1;
				foreach(DataRow row in data.Tables[0].Rows)
				{
					int id = Convert.ToInt32(row["CalleeId"]);
					string name = Convert.ToString(row["Name"]);
					double hitCount = Convert.ToDouble(row["HitCount"]);

					pane.AddPieSlice(hitCount, m_colors.ColorForIndex(index++), 0.0, name);
				}
			}

			pane.AxisChange();
			DetailsGraph.Refresh();
		}
	}

	class FunctionEntry
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public FunctionEntry(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
