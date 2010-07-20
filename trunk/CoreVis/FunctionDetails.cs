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

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;
			m_refreshTimer.Enabled = m_connection.IsConnected;

			UpdateFunctionList();
			return true;
		}

		public void Show(Control.ControlCollection parent)
		{
			this.Dock = DockStyle.Fill;
			parent.Add(this);
		}

		public void OnClose()
		{
			m_refreshTimer.Enabled = false;
		}

		private void UpdateFunctionList()
		{
			FunctionList.Items.Clear();
			using(var session = m_connection.DataEngine.OpenSession())
			{
				var list = session.CreateQuery("from FunctionInfo where Name like :search order by Name")
					.SetMaxResults(250)
					.SetString("search", "%" + SearchBox.Text + "%")
					.List<FunctionInfo>();
				foreach(var entry in list)
					FunctionList.Items.Add(new FunctionEntry(entry.Id, entry.Name));
			}

			if(FunctionList.Items.Count > 0)
			{
				FunctionList.SelectedIndex = 0;
				DetailsGraph.Visible = true;
			}
			else
			{
				DetailsGraph.Visible = false;
			}
		}

		private void RefreshGraph()
		{
			var entry = FunctionList.SelectedItem as FunctionEntry;
			if(entry == null)
				return;

			GraphPane pane = DetailsGraph.GraphPane;
			pane.CurveList.Clear();
			pane.Title.Text = "Function Breakdown (samples)";

			using(var transact = new TransactionHandle(m_connection.DataEngine))
			using(var session = m_connection.DataEngine.OpenSession(0))
			{
				double totalHits = session.CreateQuery("select sum(c.HitCount) from Call c where c.ParentId = :parentId")
					.SetInt32("parentId", entry.Id)
					.UniqueResult<long>();
				double inFunc = session.CreateQuery("select sum(c.HitCount) from Call c where c.ParentId = :parentId and c.ChildId = 0")
					.SetInt32("parentId", entry.Id)
					.UniqueResult<long>();
				var children = session.CreateQuery("select c from Call c inner join fetch c.Child where c.ParentId = :parentId order by c.HitCount desc")
					.SetInt32("parentId", entry.Id)
					.List<Call>();

				int index = 1;
				double pieTotal = 0;
				int otherCount = 0;
				string otherName = null;

				const double Significant = 0.01;
				var inFuncFraction = inFunc / totalHits;
				if(inFunc > 0 && inFuncFraction >= Significant)
				{
					//add a slice for self if it is significant
					pane.AddPieSlice(inFunc, m_colors.ColorForIndex(0), 0.0, "(self)");
					pieTotal += inFunc;
				}
				else
				{
					//otherwise just add it to the other pile
					++otherCount;
					otherName = "(self)";
				}

				foreach(var call in children)
				{
					double fraction = call.HitCount / totalHits;
					if(index < 8 && fraction > 0.02)
					{
						var slice = pane.AddPieSlice(call.HitCount, m_colors.ColorForIndex(1 + index++), 0.0, call.Child.Name);
						pieTotal += call.HitCount;
						if(fraction < 0.03)
							slice.LabelType = PieLabelType.None;
					}
					else
					{
						++otherCount;
						otherName = call.Child.Name;
					}
				}

				//If we only found one "other" function, no sense marking it as other
				double otherTotal = totalHits - pieTotal;
				if(otherCount == 1)
				{
					var slice = pane.AddPieSlice(otherTotal, m_colors.ColorForIndex(index + 1), 0.0, otherName);
					slice.LabelType = PieLabelType.None;
				}
				else if(otherCount > 1)
				{
					pane.AddPieSlice(otherTotal, m_colors.ColorForIndex(1), 0.0, string.Format("Other: {0} functions", otherCount));
				}
			}

			pane.Title.Text = entry.Name;
			pane.AxisChange();
			DetailsGraph.Refresh();
		}

		private void SearchBox_TextChanged(object sender, EventArgs e)
		{
			UpdateFunctionList();
		}

		private void FunctionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshGraph();
		}

		private void m_refreshTimer_Tick(object sender, EventArgs e)
		{
			if(FunctionList.Items.Count < 1)
				UpdateFunctionList();
			else
				RefreshGraph();
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
