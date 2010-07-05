using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

using UICore;

namespace Universe
{
	[DisplayName("Universe")]
	public partial class Host : UserControl, IVisualizer
	{
		Universe m_universe;
		ElementHost m_host;

		public Host()
		{
			InitializeComponent();
		}

		private void Host_Load(object sender, EventArgs e)
		{
		}

		public string DisplayName
		{
			get { return "Universe"; }
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			m_host = new ElementHost();
			m_host.Dock = DockStyle.Fill;
			this.Controls.Add(m_host);

			m_universe = new Universe(connection, m_host);
			m_host.Child = m_universe;
			this.Text = Utilities.GetStandardCaption(connection);
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
	}
}
