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
using System.IO;
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
	public partial class RunDialog : Form
	{
		SlimTune m_mainWindow;

		//saved configuration
		static ILauncher m_launcher;
		static int m_launcherIndex;
		static bool m_connect = true;
		static string m_resultsFile;
		static int m_visIndex;

		public RunDialog(SlimTune mainWindow)
		{
			InitializeComponent();
			m_mainWindow = mainWindow;

			//enumerate all the launchers
			foreach(var launcher in Utilities.GetLauncherList())
			{
				m_appTypeCombo.Items.Add(launcher);
			}
			m_appTypeCombo.SelectedIndex = m_launcherIndex;
			//add the handler AFTER setting the correct selected index
			m_appTypeCombo.SelectedIndexChanged += new EventHandler(m_appTypeCombo_SelectedIndexChanged);
			//run the handler if necessary
			if(m_launcher == null)
				m_appTypeCombo_SelectedIndexChanged(m_appTypeCombo, EventArgs.Empty);
			m_launchPropGrid.SelectedObject = m_launcher;

			//enumerate all the visualizers
			foreach(var vis in Utilities.GetVisualizerList(true))
			{
				m_visualizerCombo.Items.Add(vis);
			}
			m_visualizerCombo.SelectedIndex = m_visIndex;
			m_connectCheckBox.Checked = m_connect;
			m_resultsFileTextBox.Text = m_resultsFile;
		}

		private bool LaunchLocal()
		{
			string dbFile = m_resultsFileTextBox.Text;

			if(!m_launcher.CheckParams())
				return false;

			//connect to storage before launching the process -- we don't want to launch if this fails
			IStorageEngine storage = null;
			if(m_connectCheckBox.Checked)
			{
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
			}

			if(!m_launcher.Launch())
			{
				storage.Dispose();
				return false;
			}

			//connect, if we're asked to
			if(m_connectCheckBox.Checked)
			{
				ConnectProgress progress = new ConnectProgress("localhost", m_launcher.ListenPort, storage, 10);
				progress.ShowDialog(this);

				if(progress.Client != null)
				{
					Connection conn = new Connection(storage);
					conn.Executable = m_launcher.Name;
					conn.RunClient(progress.Client);
					//TODO: set options
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
					//connection failed, shut down the storage
					storage.Dispose();
				}
			}

			return true;
		}

		private void m_runButton_Click(object sender, EventArgs e)
		{
			if(m_connectCheckBox.Checked && m_resultsFileTextBox.Text == string.Empty)
			{
				MessageBox.Show("You must enter a file to save the results to.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			bool result = LaunchLocal();
			if(!result)
				return;

			this.Close();
		}

		private void m_browseDbButton_Click(object sender, EventArgs e)
		{
			m_saveResultsDialog.FileName = m_resultsFileTextBox.Text;
			var result = m_saveResultsDialog.ShowDialog(this);
			if(result == DialogResult.OK)
				m_resultsFileTextBox.Text = m_saveResultsDialog.FileName;
		}

		private void m_connectCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			m_connect = m_connectCheckBox.Checked;
			m_resultsFileTextBox.Enabled = m_connectCheckBox.Checked;
			m_browseDbButton.Enabled = m_connectCheckBox.Checked;
			m_visualizerCombo.Enabled = m_connectCheckBox.Checked;
		}

		private void m_appTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_appTypeCombo.SelectedItem == null)
			{
				m_launcher = null;
				m_launchPropGrid.SelectedObject = null;
				return;
			}

			Type launcherType = (m_appTypeCombo.SelectedItem as TypeEntry).Type;
			m_launcher = (ILauncher) Activator.CreateInstance(launcherType);
			m_launchPropGrid.SelectedObject = m_launcher;
			m_launcherIndex = m_appTypeCombo.SelectedIndex;
		}

		private void m_visualizerCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_visIndex = m_visualizerCombo.SelectedIndex;
		}

		private void m_resultsFileTextBox_TextChanged(object sender, EventArgs e)
		{
			m_resultsFile = m_resultsFileTextBox.Text;
		}
	}
}
