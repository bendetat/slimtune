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
	public partial class SlimTune : Form
	{
		private List<ProfilerWindow> m_windows = new List<ProfilerWindow>();

		public bool IsClosing { get; private set; }

		public SlimTune()
		{
			InitializeComponent();
			Plugins.Load(Application.StartupPath + "\\Plugins\\");
		}

		public void AddWindow(ProfilerWindow window)
		{
			m_windows.Add(window);
			window.FormClosed += new FormClosedEventHandler(window_FormClosed);
		}

		void window_FormClosed(object sender, FormClosedEventArgs e)
		{
			int index = m_windows.IndexOf(sender as ProfilerWindow);
			m_windows[index] = null;
		}

		private void RunButton_Click(object sender, EventArgs e)
		{
			var runner = new RunDialog(this);
			runner.ShowDialog();
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
	}
}
