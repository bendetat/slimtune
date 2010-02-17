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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class SlimTune : Form
	{
		private List<ProfilerWindow> m_windows = new List<ProfilerWindow>();

		public bool IsClosing { get; private set; }

		public SlimTune()
		{
			InitializeComponent();
			Plugins.Load(Path.Combine(Application.StartupPath, "Plugins"));
		}

		public void AddWindow(ProfilerWindow window)
		{
			m_windows.Add(window);
			window.FormClosed += new FormClosedEventHandler(window_FormClosed);
			WindowList.Items.Add(window);
		}

		void window_FormClosed(object sender, FormClosedEventArgs e)
		{
			int index = m_windows.IndexOf(sender as ProfilerWindow);
			m_windows[index] = null;
			WindowList.Items.Remove(sender);
		}

		private void RunButton_Click(object sender, EventArgs e)
		{
			var runner = new RunDialog(this);
			runner.Show();
		}

		private void ConnectButton_Click(object sender, EventArgs e)
		{
			var connect = new ConnectDialog(this);
			connect.Show();
		}

		private void RemoveDeadWindows()
		{
			m_windows.RemoveAll((Predicate<ProfilerWindow>)
				delegate(ProfilerWindow window)
				{
					return window == null;
				}
			);
		}

		private void SlimTune_FormClosing(object sender, FormClosingEventArgs e)
		{
			RemoveDeadWindows();
			if(m_windows.Count > 0)
			{
				DialogResult result = MessageBox.Show("Close all active connections?", "Closing",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.No)
				{
					e.Cancel = true;
					return;
				}
			}

			IsClosing = true;

			for(int i = 0; i < m_windows.Count; ++i)
			{
				if(m_windows[i] == null)
					continue;

				m_windows[i].Close();
			}

			RemoveDeadWindows();
			e.Cancel = m_windows.Count > 0;
			IsClosing = false;
		}

		private void SlimTune_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if(keyData == (Keys.Control | Keys.O))
				OpenButton_Click(OpenButton, EventArgs.Empty);
			else if(keyData == (Keys.Control | Keys.R))
				RunButton_Click(RunButton, EventArgs.Empty);
			else if(keyData == (Keys.Control | Keys.N))
				ConnectButton_Click(ConnectButton, EventArgs.Empty);

			return false;
		}

		private void WindowList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool enable = WindowList.SelectedItem != null;
			ShowHideButton.Enabled = enable;
			if(enable)
			{
				Form form = WindowList.SelectedItem as Form;
				if(form.Visible)
					ShowHideButton.Text = "Hide";
				else
					ShowHideButton.Text = "Show";
			}
		}

		private void OpenButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.Title = "Open Fesults File";			
			dialog.Filter = string.Format("SlimTune results ({0})|{0}", GetOpenFilter());

			DialogResult result = dialog.ShowDialog(this);
			if(result == DialogResult.OK)
			{
				OpenFile(dialog.FileName);
			}
		}

		private void OpenFile(string file)
		{
			Type engineType = Utilities.FindEngine(file);
			if(engineType == null)
			{
				//this should NOT happen.
				MessageBox.Show("Unable to find an engine for the selected file. This is a bug and should be reported.",
					"Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				//arguments are filename, createNew
				IStorageEngine engine = (IStorageEngine) Activator.CreateInstance(engineType, file, false);

				var conn = new Connection(engine);
				var window = new ProfilerWindow(this, conn);
				window.Show();
			}
			catch(System.Reflection.TargetInvocationException ex)
			{
				MessageBox.Show("Unable to create engine: " + ex.InnerException.Message,
					"Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch(Exception ex)
			{
				MessageBox.Show("Unknown error: " + ex.Message, "Error Opening File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static string GetOpenFilter()
		{
			//put together the filter string
			string extFilter = string.Empty;
			foreach(Type t in Plugins.GetEngines())
			{
				var handles = t.GetCustomAttributes(typeof(HandlesExtensionAttribute), false);
				foreach(var ext in handles)
				{
					string extString = (ext as HandlesExtensionAttribute).Extension;
					extFilter += "*.";
					extFilter += extString;
					extFilter += ";";
				}
			}
			if(extFilter.EndsWith(";"))
				extFilter = extFilter.Remove(extFilter.Length - 1);
			return extFilter;
		}

		private void ShowHideButton_Click(object sender, EventArgs e)
		{
			if(WindowList.SelectedItem == null)
				return;
			Form form = WindowList.SelectedItem as Form;
			if(form.Visible)
			{
				form.Hide();
				ShowHideButton.Text = "Show";
			}
			else
			{
				form.Show();
				ShowHideButton.Text = "Hide";
			}
		}
	}
}
