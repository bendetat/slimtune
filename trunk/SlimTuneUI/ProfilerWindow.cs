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
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class ProfilerWindow : ProfilerWindowBase
	{
		SlimTune m_mainWindow;

		public ProfilerWindow(SlimTune mainWindow, Connection conn)
			: base(conn)
		{
			InitializeComponent();
			this.Text = Utilities.GetStandardCaption(conn);
			m_mainWindow = mainWindow;
			m_mainWindow.AddWindow(this);

			string host = string.IsNullOrEmpty(conn.HostName) ? "(file)" : conn.HostName;
			HostLabel.Text = "Host: " + host;
			string port = conn.Port == 0 ? "(file)" : conn.Port.ToString();
			PortLabel.Text = "Port: " + port;
			EngineLabel.Text = "Engine: " + conn.DataEngine.Engine;
			NameLabel.Text = "Name: " + conn.DataEngine.Name;

			string status;
			if(conn.Port == 0)
				status = "Opened From File";
			else if(conn.IsConnected)
				status = "Running";
			else
				status = "Stopped";
			StatusLabel.Text = "Status: " + status;

			//SnapshotButton.Enabled = conn.IsConnected;
			Connection.Disconnected += new EventHandler(Connection_Disconnected);

			foreach(var vis in Utilities.GetVisualizerList(false))
			{
				m_visualizerCombo.Items.Add(vis);
			}
			m_visualizerCombo.SelectedIndex = 0;
		}

		void Connection_Disconnected(object sender, EventArgs e)
		{
			if(!this.IsDisposed)
			{
				this.Invoke((System.Action) delegate
				{
					StatusLabel.Text = "Status: Stopped";
					SnapshotButton.Enabled = false;
				});
			}
		}

		private void ProfilerWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			foreach(var vis in Visualizers)
			{
				vis.OnClose();
			}

			Connection.Dispose();
			Connection = null;
		}

		private void PromptSave()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = string.Format("Results file (*.{0})|*.{0}", Connection.DataEngine.Extension);
			dlg.AddExtension = true;

			while(true)
			{
				DialogResult saveResult = dlg.ShowDialog(this);
				if(saveResult == DialogResult.OK)
				{
					try
					{
						Connection.DataEngine.Save(dlg.FileName);
						return;
					}
					catch
					{
						MessageBox.Show("Unable to save results file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
				{
					return;
				}
			}
		}

		private void ProfilerWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_mainWindow.IsClosing && !Connection.DataEngine.InMemory)
				return;

			//TODO: Saving of SQLite in-memory databases does not currently work for some reason
			/*if(Connection.StorageEngine.InMemory)
			{
				DialogResult result = MessageBox.Show("Save before exiting?", "Save?",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
				if(result == DialogResult.Yes)
				{
					PromptSave();
				}
				else if(result == DialogResult.Cancel)
				{
					e.Cancel = true;
				}
			}
			else*/
			{
				DialogResult result = MessageBox.Show("Close this connection?", "Close Connection",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}

		private void m_openVisualizerButton_Click(object sender, EventArgs e)
		{
			TypeEntry visEntry = m_visualizerCombo.SelectedItem as TypeEntry;
			if(visEntry != null && visEntry.Type != null)
			{
				AddVisualizer(visEntry.Type);
			}
		}

		public void AddVisualizer(Type visType)
		{
			IVisualizer visualizer = (IVisualizer) Activator.CreateInstance(visType);
			if(!visualizer.Initialize(this, Connection))
				return;

			Visualizers.Add(visualizer);
			TabPage page = new TabPage(visualizer.DisplayName);
			page.Tag = visualizer;
			visualizer.Show(page.Controls);
			VisualizerHost.TabPages.Add(page);
			VisualizerHost.SelectedTab = page;
			m_closeVisualizerButton.Enabled = true;
		}

		private void ClearDataButton_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show("WARNING: This will clear all collected data, and cannot be reversed. Continue?",
				"Clear All Data", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
			if(result == DialogResult.Yes)
				Connection.DataEngine.ClearData();
		}

		private void SnapshotButton_Click(object sender, EventArgs e)
		{
			if(!Connection.IsConnected)
			{
				MessageBox.Show("Target must be running in order to take a snapshot.", "Take Snapshot");
				return;
			}

			Connection.DataEngine.Snapshot("User snapshot");
			MessageBox.Show("Snapshot saved", "Take Snapshot");
		}

		private void CloseTab(int index)
		{
			var tab = VisualizerHost.TabPages[index];

			if(tab == VisualizerHost.SelectedTab)
			{
				//Select the next tab to the right, or the rightmost tab
				if(VisualizerHost.TabPages.Count > index + 1)
					VisualizerHost.SelectedIndex = index + 1;
				else if(VisualizerHost.TabPages.Count > 1)
					VisualizerHost.SelectedIndex = VisualizerHost.TabPages.Count - 2;
			}

			//close down the visualizer and its associated tab
			var vis = (IVisualizer) tab.Tag;
			vis.OnClose();
			Visualizers.Remove(vis);
			VisualizerHost.TabPages.Remove(tab);

			Debug.Assert(Visualizers.Count == VisualizerHost.TabPages.Count);

			if(VisualizerHost.TabPages.Count == 0)
			{
				m_closeVisualizerButton.Enabled = false;
			}
		}

		private void m_closeVisualizerButton_Click(object sender, EventArgs e)
		{
			var tab = VisualizerHost.SelectedTab;
			int index = VisualizerHost.SelectedIndex;
			if(tab != null)
			{
				CloseTab(index);
			}
		}

		private void VisualizerHost_MouseClick(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
			{
				Point pos = e.Location;
				for(int i = 0; i < VisualizerHost.TabPages.Count; ++i)
				{
					if(VisualizerHost.GetTabRect(i).Contains(e.Location))
					{
						CloseTab(i);
						break;
					}
				}
			}
		}

		/*private void SuspendButton_Click(object sender, EventArgs e)
		{
			if(Connection.Client != null)
				Connection.Client.SuspendTarget();
		}

		private void ResumeButton_Click(object sender, EventArgs e)
		{
			if(Connection.Client != null)
				Connection.Client.ResumeTarget();
		}*/
	}
}
