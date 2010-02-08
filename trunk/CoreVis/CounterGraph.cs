using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using UICore;
using ZedGraph;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("Performance Counters")]
	public partial class CounterGraph : UserControl, IVisualizer
	{
		const string kCountersQuery = @"
SELECT * FROM Counters
";

		const string kValuesQuery = @"
SELECT Time, Value
FROM CounterValues
WHERE CounterId = {0}
ORDER BY Time
";

		ProfilerWindowBase m_mainWindow;
		Connection m_connection;

		public CounterGraph()
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

			mainWindow.Visualizers.Add(this);

			Graph.GraphPane.Title.Text = "Counter";
			Graph.GraphPane.XAxis.Title.Text = "Time";
			Graph.GraphPane.YAxis.Title.Text = "Value";

			UpdateCounters();
		}

		public void Show(TabControl parent)
		{
			var page = new TabPage("Counters");
			this.Dock = DockStyle.Fill;
			page.Controls.Add(this);
			parent.TabPages.Add(page);
			parent.SelectedTab = page;
		}

		private void UpdateCounters()
		{
			CounterCombo.Items.Clear();
			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				var data = m_connection.StorageEngine.Query(kCountersQuery);
				foreach(DataRow row in data.Tables[0].Rows)
				{
					int id = Convert.ToInt32(row["Id"]);
					string name = Convert.ToString(row["Name"]);
					if(string.IsNullOrEmpty(name))
						name = string.Format("Counter #{0}", id);

					CounterCombo.Items.Add(new CounterEntry(id, name));
				}
			}
			if(CounterCombo.Items.Count > 0)
				CounterCombo.SelectedIndex = 0;
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			UpdateCounters();
		}

		private void CounterCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(CounterCombo.SelectedItem == null)
				return;

			var entry = CounterCombo.SelectedItem as CounterEntry;
			var points = new PointPairList();
			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				var data = m_connection.StorageEngine.Query(string.Format(kValuesQuery, entry.Id));
				foreach(DataRow row in data.Tables[0].Rows)
				{
					long time = Convert.ToInt64(row["Time"]);
					long value = Convert.ToInt64(row["Value"]);
					points.Add(time / 1000.0, value / 1000.0);
				}
			}

			Graph.GraphPane.CurveList.Clear();
			Graph.GraphPane.AddCurve("Values", points, Color.Blue, SymbolType.None);
			Graph.GraphPane.Title.Text = entry.Name;
			Graph.AxisChange();
			Graph.Refresh();
		}
	}

	class CounterEntry
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public CounterEntry(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
