using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class ConnectionList : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		MainWindow m_mainWindow;
		BindingSource m_source = new BindingSource();
		BindingList<Connection> m_connections = new BindingList<Connection>();
		public IEnumerable<Connection> Connections
		{
			get
			{
				foreach(Connection conn in m_source)
				{
					yield return conn;
				}
			}
		}

		public int ConnectionsCount
		{
			get { return m_connections.Count; }
		}

		public ConnectionList(MainWindow mainWindow)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");

			InitializeComponent();

			m_mainWindow = mainWindow;
			m_source.DataSource = m_connections;
			m_connectionList.DataSource = m_source;
			m_connectionList.DisplayMember = "Name";

			foreach(var vis in Utilities.GetVisualizerList(false))
			{
				m_visualizersCombo.Items.Add(vis);
			}
			m_visualizersCombo.SelectedIndex = 0;
		}

		public void AddConnection(Connection connection)
		{
			m_source.Add(connection);
			connection.Disconnected += new EventHandler(Connection_Disconnected);
			m_visualizersCombo.Enabled = true;
			m_openVisualizerButton.Enabled = true;
			m_connectionList.SelectedIndex = m_connectionList.Items.Count - 1;
			m_connectionList_SelectedIndexChanged(this, EventArgs.Empty);
			m_closeButton.Enabled = true;
			m_clearDataButton.Enabled = true;
		}

		void Connection_Disconnected(object sender, EventArgs e)
		{
			//HACK HACK HACK
			try
			{
				Connection conn = sender as Connection;
				this.Invoke((Action) delegate
				{
					if(conn == m_connectionList.SelectedItem)
					{
						m_statusLabel.Text = "Status: Stopped";
						m_disconnectButton.Enabled = false;
					}

					if(m_connectionList.Items.Count > 0)
					{
						m_connectionList.SelectedIndex = m_connectionList.Items.Count - 1;
						m_connectionList_SelectedIndexChanged(this, EventArgs.Empty);
						m_closeButton.Enabled = true;
					}
					else
					{
						m_disconnectButton.Enabled = false;
						m_closeButton.Enabled = false;
						m_visualizersCombo.Enabled = false;
						m_openVisualizerButton.Enabled = false;
						m_clearDataButton.Enabled = false;
					}
				});
			}
			catch
			{
				//probably shutting down, don't bother
			}
		}

		private void m_connectionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			Connection conn = m_connectionList.SelectedItem as Connection;
			if(conn != null && conn.Client != null)
			{
				m_statusLabel.Text = "Status: Running";
				m_disconnectButton.Enabled = true;
				m_clearDataButton.Enabled = true;
			}
			else
			{
				m_statusLabel.Text = "Status: Stopped";
				m_disconnectButton.Enabled = false;
				m_clearDataButton.Enabled = false;
			}
		}

		private void m_closeButton_Click(object sender, EventArgs e)
		{
			Connection conn = m_connectionList.SelectedItem as Connection;
			if(conn != null)
			{
				DialogResult result = MessageBox.Show(this, "Visualizers using this connection will also close. Close anyway?",
					"Close Connection", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.Yes)
				{
					m_source.Remove(conn);
					conn.Dispose();

					if(m_connections.Count == 0)
					{
						m_closeButton.Enabled = false;
						m_disconnectButton.Enabled = false;
						m_clearDataButton.Enabled = false;
						m_visualizersCombo.Enabled = false;
						m_openVisualizerButton.Enabled = false;
					}
				}
			}
		}

		private void m_openVisualizerButton_Click(object sender, EventArgs e)
		{
			Connection conn = m_connectionList.SelectedItem as Connection;
			var visEntry = m_visualizersCombo.SelectedItem as TypeEntry;
			if(conn == null || visEntry == null || visEntry.Type == null)
				return;

			IVisualizer visualizer = Activator.CreateInstance(visEntry.Type) as IVisualizer;
			visualizer.Initialize(m_mainWindow, conn);
			visualizer.Show(m_mainWindow.DockPanel);
		}

		private void m_disconnectButton_Click(object sender, EventArgs e)
		{
			Connection conn = m_connectionList.SelectedItem as Connection;
			if(conn != null && conn.Client != null)
			{
				conn.DisconnectClient();
				m_statusLabel.Text = "Status: Stopped";
				m_disconnectButton.Enabled = false;
			}
		}

		private void ConnectionList_FormClosed(object sender, FormClosedEventArgs e)
		{
			foreach(Connection conn in m_connections)
			{
				conn.Dispose();
			}
		}

		private void m_clearDataButton_Click(object sender, EventArgs e)
		{
			Connection conn = m_connectionList.SelectedItem as Connection;
			if(conn != null)
			{
				DialogResult result = MessageBox.Show("Do you want to clear all of the collected runtime data for this connection?",
					"Clear Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.Yes)
				{
					conn.StorageEngine.ClearData();
				}
			}
		}
	}
}
