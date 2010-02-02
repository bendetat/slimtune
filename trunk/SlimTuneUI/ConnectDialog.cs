/*
* Copyright (c) 2007-2009 SlimDX Group
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class ConnectDialog : Form
	{
		SlimTune m_mainWindow;

		public ConnectDialog(SlimTune mainWindow)
		{
			InitializeComponent();
			m_mainWindow = mainWindow;

			foreach(var vis in Utilities.GetVisualizerList(true))
			{
				m_visualizerCombo.Items.Add(vis);
			}
			m_visualizerCombo.SelectedIndex = 0;
		}

		private void m_browseDbButton_Click(object sender, EventArgs e)
		{
			var result = m_saveResultsDialog.ShowDialog(this);
			if(result == DialogResult.OK)
				m_resultsFileTextBox.Text = m_saveResultsDialog.FileName;
		}

		private bool Connect(string host, int port)
		{
			string dbFile = m_resultsFileTextBox.Text;

			//connect to storage before launching the process -- we don't want to launch if this fails
			IStorageEngine storage = null;
			try
			{
				//storage = new SqlServerCompactEngine(dbFile, true);
				storage = new SQLiteEngine(dbFile, true);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			ConnectProgress progress = new ConnectProgress(host, port, storage, 10);
			progress.ShowDialog(this);

			if(progress.Client != null)
			{
				Connection conn = new Connection(storage);
				conn.RunClient(progress.Client);
				conn.SetAutoSnapshots(10000, false);

				var profilerWindow = new ProfilerWindow(m_mainWindow, conn);
				profilerWindow.Show();

				TypeEntry visEntry = m_visualizerCombo.SelectedItem as TypeEntry;
				if(visEntry != null && visEntry.Type != null)
				{
					IVisualizer visualizer = Activator.CreateInstance(visEntry.Type) as IVisualizer;
					visualizer.Initialize(profilerWindow, conn);
					visualizer.Show(profilerWindow.VisualizerHost);
				}
				profilerWindow.BringToFront();
			}
			else
			{
				storage.Dispose();
				return false;
			}

			return true;
		}

		private void m_connectButton_Click(object sender, EventArgs e)
		{
			if(m_hostNameTextBox.Text == string.Empty)
			{
				MessageBox.Show("You must enter a hostname to connect to.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			int port = 0;
			int.TryParse(m_portTextBox.Text, out port);
			if(port < 1 || port > ushort.MaxValue)
			{
				MessageBox.Show("Port must be between 1 and 65535.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if(m_resultsFileTextBox.Text == string.Empty)
			{
				MessageBox.Show("You must enter a file to save the results to.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			bool result = Connect(m_hostNameTextBox.Text, int.Parse(m_portTextBox.Text));
			if(!result)
				return;

			this.Close();
		}
	}
}
