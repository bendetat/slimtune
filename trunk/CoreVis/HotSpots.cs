using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("Hotspots")]
	public partial class HotSpots : UserControl, IVisualizer
	{
		ProfilerWindowBase m_mainWindow;
		Connection m_connection;
		Snapshot m_snapshot;

		//drawing stuff
		Font m_functionFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
		Font m_objectFont = new Font(SystemFonts.DefaultFont.FontFamily, 9, FontStyle.Regular);

		class ListTag
		{
			public ListBox Right;
			public double TotalTime;
		}

		public string DisplayName
		{
			get { return "Hotspots"; }
		}

		public UserControl Control
		{
			get { return this; }
		}

		public HotSpots()
		{
			InitializeComponent();
			HotspotsList.Tag = new ListTag();
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection, Snapshot snapshot)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;
			m_snapshot = snapshot;

			UpdateHotspots();
			return true;
		}

		public void OnClose()
		{
		}

		private void UpdateHotspots()
		{
			HotspotsList.Items.Clear();
			using(var session = m_connection.DataEngine.OpenSession(m_snapshot.Id))
			using(var tx = session.BeginTransaction())
			{
				//find the total time consumed
				var totalQuery = session.CreateQuery("select sum(call.Time) from Call call where call.ChildId = 0");
				var totalTimeFuture = totalQuery.FutureValue<double>();

				//find the functions that consumed the most time-exclusive. These are hotspots.
				var query = session.CreateQuery("from Call c inner join fetch c.Parent where c.ChildId = 0 order by c.Time desc");
				query.SetMaxResults(20);
				var hotspots = query.Future<Call>();

				var totalTime = totalTimeFuture.Value;
				(HotspotsList.Tag as ListTag).TotalTime = totalTime;
				foreach(var call in hotspots)
				{
					if(call.Time / totalTime < 0.01f)
					{
						//less than 1% is not a hotspot, and since we're ordered by Time we can exit
						break;
					}

					HotspotsList.Items.Add(call);
				}

				tx.Commit();
			}
		}

		private bool UpdateParents(Call child, ListBox box)
		{
			using(var session = m_connection.DataEngine.OpenSession(m_snapshot.Id))
			using(var tx = session.BeginTransaction())
			{
				var query = session.CreateQuery("from Call c inner join fetch c.Parent where c.ChildId = :funcId order by c.Time desc")
					.SetInt32("funcId", child.Parent.Id);
				var parents = query.List<Call>();
				double totalTime = 0;
				foreach(var call in parents)
				{
					if(call.ParentId == 0)
						return false;

					totalTime += call.Time;
					box.Items.Add(call);
				}
				(box.Tag as ListTag).TotalTime = totalTime;

				tx.Commit();
			}

			return true;
		}

		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			//UpdateHotspots();
		}

		private void RemoveList(ListBox list)
		{
			if(list == null)
				return;

			RemoveList((list.Tag as ListTag).Right);
			ScrollPanel.Controls.Remove(list);
		}

		private void CallList_SelectedIndexChanged(object sender, EventArgs e)
		{
			ListBox list = sender as ListBox;
			RemoveList((list.Tag as ListTag).Right);

			//create a new listbox to the right
			ListBox lb = new ListBox();
			lb.Size = list.Size;
			lb.Location = new Point(list.Right + 4, 4);
			lb.IntegralHeight = false;
			lb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			lb.FormattingEnabled = true;
			lb.DrawMode = DrawMode.OwnerDrawFixed;
			lb.ItemHeight = HotspotsList.ItemHeight;
			lb.Tag = new ListTag();
			lb.Format += new ListControlConvertEventHandler(CallList_Format);
			lb.SelectedIndexChanged += new EventHandler(CallList_SelectedIndexChanged);
			lb.DrawItem += new DrawItemEventHandler(CallList_DrawItem);

			if(UpdateParents(list.SelectedItem as Call, lb))
			{
				ScrollPanel.Controls.Add(lb);
				ScrollPanel.ScrollControlIntoView(lb);
				(list.Tag as ListTag).Right = lb;
			}
		}

		private void CallList_Format(object sender, ListControlConvertEventArgs e)
		{
			Call call = e.ListItem as Call;
			e.Value = call.Parent.Name;
		}

		private void CallList_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox list = sender as ListBox;
			Call item = list.Items[e.Index] as Call;

			int splitIndex = item.Parent.Name.LastIndexOf('.');
			string functionName = item.Parent.Name.Substring(splitIndex + 1);
			string objectName = "- " + item.Parent.Name.Substring(0, splitIndex);
			double percent = 100 * item.Time / (list.Tag as ListTag).TotalTime;
			string functionString = string.Format("{0:0.##}%: {1}", percent, functionName);

			Brush brush = Brushes.Black;
			if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				brush = Brushes.White;

			e.DrawBackground();
			e.Graphics.DrawString(functionString, m_functionFont, brush, new PointF(e.Bounds.X, e.Bounds.Y));
			e.Graphics.DrawString(objectName, m_objectFont, brush, new PointF(e.Bounds.X + 4, e.Bounds.Y + 18));
			e.DrawFocusRectangle();
		}
	}
}
