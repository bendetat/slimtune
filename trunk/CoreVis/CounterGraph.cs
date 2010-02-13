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
SELECT *
FROM Counters
ORDER BY Id
";

		const string kValuesQuery = @"
SELECT Time, Value
FROM CounterValues
WHERE CounterId = {0}
ORDER BY Time DESC
LIMIT 3600
";

		ProfilerWindowBase m_mainWindow;
		Connection m_connection;
		ColorRotator m_colors = new ColorRotator();
		bool m_redrawGraph = true;

		public string DisplayName
		{
			get
			{
				return "Performance Counters";
			}
		}

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

			Graph.GraphPane.Title.Text = "Performance Counters";
			Graph.GraphPane.XAxis.Title.Text = "Time";
			Graph.GraphPane.YAxis.Title.Text = "Value";

			UpdateCounters();
		}

		public void Show(Control.ControlCollection parent)
		{
			this.Dock = DockStyle.Fill;
			parent.Add(this);
		}

		private void UpdateCounters()
		{
			for(int i = 0; i < m_counterListBox.Items.Count; ++i)
			{
				var entry = m_counterListBox.Items[i] as CounterEntry;
				entry.Tagged = false;
			}

			using(var transact = new TransactionHandle(m_connection.StorageEngine))
			{
				var data = m_connection.StorageEngine.Query(kCountersQuery);
				foreach(DataRow row in data.Tables[0].Rows)
				{
					int id = Convert.ToInt32(row["Id"]);
					string name = Convert.ToString(row["Name"]);
					if(string.IsNullOrEmpty(name))
						name = string.Format("Counter #{0}", id);

					var newEntry = new CounterEntry(id, name);
					int existingEntryIndex = m_counterListBox.Items.IndexOf(newEntry);
					if(existingEntryIndex >= 0)
					{
						var existingEntry = m_counterListBox.Items[existingEntryIndex] as CounterEntry;
						existingEntry.Name = name;
						existingEntry.Tagged = true;
					}
					else
					{
						m_counterListBox.Items.Add(new CounterEntry(id, name));
					}
				}
			}

			int index = 0;
			while(index < m_counterListBox.Items.Count)
			{
				var entry = m_counterListBox.Items[index] as CounterEntry;
				if(!entry.Tagged)
				{
					//this is a stale entry, so remove it
					m_counterListBox.Items.RemoveAt(index);
				}
				else
				{
					//it's fine, keep going
					++index;
				}
			}
		}

		private void UpdateGraph()
		{
			m_redrawGraph = false;
			for(int i = 0; i < m_counterListBox.Items.Count; ++i)
			{
				if(m_counterListBox.GetItemChecked(i))
				{
					//cheat and simply toggle the check
					m_counterListBox.SetItemChecked(i, false);
					m_counterListBox.SetItemChecked(i, true);
				}
			}
			m_redrawGraph = true;
			Graph.AxisChange();
			Graph.Refresh();
		}

		private void m_counterListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			var entry = m_counterListBox.Items[e.Index] as CounterEntry;
			if(e.NewValue == CheckState.Checked)
			{
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

				Graph.GraphPane.AddCurve(entry.Name, points, m_colors.ColorForIndex(entry.Id), SymbolType.None);
			}
			else if(e.NewValue == CheckState.Unchecked)
			{
				Graph.GraphPane.CurveList.Remove(Graph.GraphPane.CurveList[entry.Name]);
			}

			if(m_redrawGraph)
			{
				Graph.AxisChange();
				Graph.Refresh();
			}
		}

		private void m_refreshTimer_Tick(object sender, EventArgs e)
		{
			UpdateCounters();
			UpdateGraph();
		}
	}

	class CounterEntry
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Tagged { get; set; }

		public CounterEntry(int id, string name)
		{
			Id = id;
			Name = name;
			Tagged = true;
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CounterEntry;
			if(other == null)
				return false;

			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id;
		}
	}
}
