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
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class RunDialog : Form
	{
		const string ConfigFile = "launcherConfig";

		SlimTune m_mainWindow;

		//saved configuration
		static ILauncher m_launcher;
		static int m_launcherIndex = 0;
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

			//select an initial launcher
			m_appTypeCombo.SelectedIndex = m_launcherIndex;
			if(m_launcher == null)
			{
				LoadLauncherConfig();
			}

			//add the handler AFTER setting the correct selected index
			m_appTypeCombo.SelectedIndexChanged += new EventHandler(m_appTypeCombo_SelectedIndexChanged);

			//run the index changed handler if we didn't get a launcher from file
			if(m_launcher == null)
			{
				m_launcherIndex = -1;
				m_appTypeCombo_SelectedIndexChanged(m_appTypeCombo, EventArgs.Empty);
			}

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

			//connect to data engine before launching the process -- we don't want to launch if this fails
			IDataEngine data = null;
			if(m_connectCheckBox.Checked)
			{
				try
				{
					//storage = new SqlServerCompactEngine(dbFile, true);
					if(m_sqliteRadio.Checked)
						//data = new SQLiteEngine(dbFile, true);
						data = new SQLiteEngine(dbFile, true);
					else if(m_sqliteMemoryRadio.Checked)
						data = new SQLiteMemoryEngine();
					else
						throw new NotImplementedException();
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			if(!m_launcher.Launch())
			{
				if(data != null)
					data.Dispose();
				return false;
			}

			//connect, if we're asked to
			if(m_connectCheckBox.Checked)
			{
				ConnectProgress progress = new ConnectProgress("localhost", m_launcher.ListenPort, data, 10);
				progress.ShowDialog(this);

				if(progress.Client != null)
				{
					Connection conn = new Connection(data);
					conn.Executable = m_launcher.Name;
					conn.RunClient(progress.Client);
					//TODO: set options like auto snapshot frequency
					conn.SetAutoSnapshots(10000, false);

					var profilerWindow = new ProfilerWindow(m_mainWindow, conn);
					profilerWindow.Show();

					TypeEntry visEntry = m_visualizerCombo.SelectedItem as TypeEntry;
					if(visEntry != null && visEntry.Type != null)
						profilerWindow.AddVisualizer(visEntry.Type);

					profilerWindow.BringToFront();
				}
				else
				{
					//connection failed, shut down the storage
					data.Dispose();
				}
			}

			return true;
		}

		private void LoadLauncherConfig()
		{
			try
			{
				//try and load a launcher configuration from isolated storage
				using(var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
				{
					var configFile = new IsolatedStorageFileStream(ConfigFile, FileMode.Open, FileAccess.Read, isoStore);
					using(var sr = new StreamReader(configFile))
					{
						//read the concrete type to deserialize
						var launcherTypeName = sr.ReadLine();
						var launcherType = Type.GetType(launcherTypeName, true);

						//read the actual object
						XmlSerializer serializer = new XmlSerializer(launcherType);
						m_launcher = (ILauncher) serializer.Deserialize(sr);

						//select the correct item in the combo box
						foreach(TypeEntry item in m_appTypeCombo.Items)
						{
							if(item.Type == launcherType)
							{
								m_appTypeCombo.SelectedItem = item;
								break;
							}
						}
					}
					isoStore.Close();
				}
			}
			catch
			{
				//couldn't load the launcher, for whatever reason
			}
		}

		private void m_runButton_Click(object sender, EventArgs e)
		{
			if(m_connectCheckBox.Checked && m_resultsFileTextBox.Enabled && m_resultsFileTextBox.Text == string.Empty)
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
			m_sqliteRadio.Enabled = m_connectCheckBox.Checked;
			m_sqliteMemoryRadio.Enabled = m_connectCheckBox.Checked;
			m_resultsFileTextBox.Enabled = m_connectCheckBox.Checked;
			m_browseDbButton.Enabled = m_connectCheckBox.Checked;
			m_visualizerCombo.Enabled = m_connectCheckBox.Checked;
		}

		private void m_appTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			//this only happens when we detect admin required and revert the change
			if(m_appTypeCombo.SelectedIndex == m_launcherIndex)
				return;

			//not sure this CAN happen
			if(m_appTypeCombo.SelectedItem == null)
			{
				m_launcher = null;
				m_launchPropGrid.SelectedObject = null;
				return;
			}

			Type launcherType = (m_appTypeCombo.SelectedItem as TypeEntry).Type;
			ILauncher launcher = (ILauncher) Activator.CreateInstance(launcherType);
			//Don't allow the user to select a launcher that will just fail due to admin privileges required
			if(launcher.RequiresAdmin && !LauncherCommon.UserIsAdmin())
			{
				MessageBox.Show("You must run SlimTune as an administrator to profile the selected application type.");
				m_appTypeCombo.SelectedIndex = m_launcherIndex;
				return;
			}

			m_launcher = launcher;
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

		private void EngineChanged(object sender, EventArgs e)
		{
			if(m_sqliteRadio.Checked)
			{
				m_resultsFileTextBox.Enabled = true;
			}
			else if(m_sqliteMemoryRadio.Checked)
			{
				m_resultsFileTextBox.Enabled = false;
			}
		}

		private void RunDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				//save the launcher configuration to isolated storage
				using(var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
				{
					var configFile = new IsolatedStorageFileStream(ConfigFile, FileMode.Create, FileAccess.Write, isoStore);
					using(var sw = new StreamWriter(configFile))
					{
						var launcherType = m_launcher.GetType();
						//write the concrete type so we know what to deserialize
						string launcherTypeName = launcherType.AssemblyQualifiedName;
						sw.WriteLine(launcherTypeName);

						//write the object itself
						var serializer = new XmlSerializer(launcherType);
						serializer.Serialize(sw, m_launcher);
					}
					isoStore.Close();
				}
			}
			catch(Exception ex)
			{
				//not catching a problem here will cause the window not to close, which sucks
				MessageBox.Show("Please report a bug: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
