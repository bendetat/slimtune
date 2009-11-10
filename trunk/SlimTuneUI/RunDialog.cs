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

namespace SlimTuneUI
{
	public partial class RunDialog : Form
	{
		private const string ProfilerGuid = "{38A7EA35-B221-425a-AD07-D058C581611D}";

		MainWindow m_mainWindow;

		public RunDialog(MainWindow mainWindow)
		{
			InitializeComponent();
			m_mainWindow = mainWindow;

			foreach(var vis in Utilities.GetVisualizerList(true))
			{
				m_visualizerCombo.Items.Add(vis);
			}
			m_visualizerCombo.SelectedIndex = 0;
		}

		private bool LaunchLocal()
		{
			string exe = m_executableTextBox.Text;
			string args = m_argumentsTextBox.Text;
			int port = int.Parse(m_portTextBox.Text);
			string dbFile = m_resultsFileTextBox.Text;
			ProfilerMode mode = ProfilerMode.PM_Disabled;

			if(!File.Exists(exe))
			{
				MessageBox.Show("Executable does not exist.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			//connect to storage before launching the process -- we don't want to launch if this fails
			IStorageEngine storage = null;
			if(m_connectCheckBox.Checked)
			{
				try
				{
					storage = new SqlServerCompactEngine(dbFile, true);
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			if(m_samplingRadio.Checked)
				mode = ProfilerMode.PM_Sampling;
			else if(m_tracingRadio.Checked)
				mode = ProfilerMode.PM_Tracing;
			else if(m_hybridRadio.Checked)
				mode = ProfilerMode.PM_Hybrid;

			string config = string.Empty;

			config += string.Format("Mode={0};", (int) mode);
			config += string.Format("Port={0};", port);
			config += string.Format("Wait={0};", m_waitConnectCheckBox.Checked  ? 1 : 0);
			config += string.Format("SampleUnmanaged={0};", m_sampleNativeCheckBox.Checked ? 1 : 0);

			var psi = new ProcessStartInfo(exe, args);
			psi.RedirectStandardOutput = false;
			psi.RedirectStandardError = false;
			psi.RedirectStandardInput = false;
			psi.UseShellExecute = false;
			psi.WorkingDirectory = string.IsNullOrEmpty(m_workingDirTextBox.Text) ?
				Path.GetDirectoryName(exe) : m_workingDirTextBox.Text;

			psi.EnvironmentVariables["COR_ENABLE_PROFILING"] = "1";
			psi.EnvironmentVariables["COR_PROFILER"] = ProfilerGuid;
			psi.EnvironmentVariables["SLIMTUNE_CONFIG"] = config;

			try
			{
				Process.Start(psi);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Launch Error");
				storage.Dispose();
				return false;
			}

			//connect, if we're asked to
			if(m_connectCheckBox.Checked)
			{
				ConnectProgress progress = new ConnectProgress("localhost", port, storage, 10);
				progress.ShowDialog(this);

				if(progress.Client != null)
				{
					Connection conn = new Connection(storage);
					conn.Executable = psi.FileName;
					conn.RunClient(progress.Client);
					//TODO: set options
					conn.SetAutoSnapshots(10000, false);
					m_mainWindow.ConnectionList.AddConnection(conn);

					VisualizerEntry visEntry = m_visualizerCombo.SelectedItem as VisualizerEntry;
					if(visEntry != null && visEntry.Type != null)
					{
						IVisualizer visualizer = Activator.CreateInstance(visEntry.Type) as IVisualizer;
						visualizer.Initialize(m_mainWindow, conn);
						visualizer.Show(m_mainWindow.DockPanel);
					}
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
			if(m_executableTextBox.Text == string.Empty)
			{
				MessageBox.Show("You must enter an executable file to run.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			int port = 0;
			int.TryParse(m_portTextBox.Text, out port);
			if(port < 1 || port > ushort.MaxValue)
			{
				MessageBox.Show("Port must be between 1 and 65535.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

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

		private void m_browseExeButton_Click(object sender, EventArgs e)
		{
			m_openExeDialog.FileName = m_executableTextBox.Text;
			var result = m_openExeDialog.ShowDialog(this);
			if(result == DialogResult.OK)
				m_executableTextBox.Text = m_openExeDialog.FileName;
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
			m_resultsFileTextBox.Enabled = m_connectCheckBox.Checked;
			m_browseDbButton.Enabled = m_connectCheckBox.Checked;
			m_visualizerCombo.Enabled = m_connectCheckBox.Checked;
		}

		private void m_portTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == (char) Keys.Back)
				return;
			if(e.KeyChar < '0' || e.KeyChar > '9')
				e.Handled = true;
		}

		private void m_browseWorkingDirButton_Click(object sender, EventArgs e)
		{
			m_dirBrowser.SelectedPath = m_workingDirTextBox.Text;
			DialogResult result = m_dirBrowser.ShowDialog(this);
			if(result != DialogResult.Cancel)
				m_workingDirTextBox.Text = m_dirBrowser.SelectedPath;
		}
	}
}
