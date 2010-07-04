using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("ORM Test Visualizer")]
	public partial class OrmTest : UserControl, IVisualizer
	{
		ProfilerWindowBase m_mainWindow;
		Connection m_connection;

		public OrmTest()
		{
			InitializeComponent();
		}

		public string DisplayName
		{
			get { return "ORM Test"; }
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;

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

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			FunctionList.Items.Clear();
			FunctionList.BeginUpdate();
			using(var session = m_connection.DataEngine.OpenSession())
			{
				var functions = session.CreateCriteria<FunctionInfo>().List<FunctionInfo>();
				foreach(var f in functions)
				{
					FunctionList.Items.Add(f.Name);
				}
			}
			FunctionList.EndUpdate();
		}
	}
}
