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

		ListBox m_rightMost;

		//drawing stuff
		Font m_functionFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
		Font m_objectFont = new Font(SystemFonts.DefaultFont.FontFamily, 9, FontStyle.Regular);
		double m_totalHotspotsTime = 0;

		public HotSpots()
		{
			InitializeComponent();
			m_rightMost = HotspotsList;
		}

		public string DisplayName
		{
			get { return "Hotspots"; }
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;

			UpdateHotspots();
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

		private void UpdateHotspots()
		{
			HotspotsList.Items.Clear();
			using(var session = m_mainWindow.OpenActiveSnapshot())
			{
				//find the functions that consumed the most time-exclusive. These are hotspots.
				var query = session.CreateQuery("from Call c where c.ChildId = 0 order by c.Time desc inner join fetch c.Parent");
				query.SetMaxResults(20);
				var hotspots = query.List<Call>();
				foreach(var call in hotspots)
				{
					var func = call.Parent;
					var parentName = func.Name;
					HotspotsList.Items.Add(call);
				}

				var totalQuery = session.CreateQuery("select sum(call.Time) from Call call where call.ChildId = 0");
				m_totalHotspotsTime = totalQuery.UniqueResult<double>();
			}
		}

		private bool UpdateParents(Call child, ListBox box)
		{
			using(var session = m_mainWindow.OpenActiveSnapshot())
			{
				session.Lock(child.Parent, NHibernate.LockMode.None);
				var parents = child.Parent.CallsAsChild;
				foreach(var call in parents)
				{
					if(call.ParentId == 0)
						return false;

					var func = call.Parent;
					var parentName = func.Name;
					box.Items.Add(call);
				}
			}

			return true;
		}

		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			//UpdateHotspots();
		}

		private void RemoveList(ListBox list)
		{
			if(list.Tag != null)
				RemoveList(list.Tag as ListBox);

			ScrollPanel.Controls.Remove(list);
		}

		private void CallList_SelectedIndexChanged(object sender, EventArgs e)
		{
			ListBox list = sender as ListBox;
			if(list.Tag != null)
				RemoveList(list.Tag as ListBox);

			//create a new listbox to the right
			ListBox lb = new ListBox();
			lb.Size = list.Size;
			lb.Location = new Point(list.Right + 4, 4);
			lb.IntegralHeight = false;
			lb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			lb.FormattingEnabled = true;
			lb.Format += new ListControlConvertEventHandler(CallList_Format);
			lb.SelectedIndexChanged += new EventHandler(CallList_SelectedIndexChanged);

			if(UpdateParents(list.SelectedItem as Call, lb))
			{
				ScrollPanel.Controls.Add(lb);
				ScrollPanel.ScrollControlIntoView(lb);
				m_rightMost.Tag = lb;
				m_rightMost = lb;
			}
		}

		private void CallList_Format(object sender, ListControlConvertEventArgs e)
		{
			Call call = e.ListItem as Call;
			e.Value = call.Parent.Name;
		}

		private void HotspotsList_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox list = sender as ListBox;
			Call item = list.Items[e.Index] as Call;

			int splitIndex = item.Parent.Name.LastIndexOf('.');
			string functionName = item.Parent.Name.Substring(splitIndex + 1);
			string objectName = "- " + item.Parent.Name.Substring(0, splitIndex);
			double percent = 100 * item.Time / m_totalHotspotsTime;
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
