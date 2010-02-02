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
	public partial class ProfilerWindow : ProfilerWindowBase
	{
		SlimTune m_mainWindow;

		public ProfilerWindow(SlimTune mainWindow, Connection conn)
			: base(conn)
		{
			InitializeComponent();
			m_mainWindow = mainWindow;
			m_mainWindow.AddWindow(this);
		}

		private void ProfilerWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			Connection.Dispose();
			Connection = null;
		}

		private void ProfilerWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_mainWindow.IsClosing)
			{
				if(!Connection.StorageEngine.InMemory)
					return;

				DialogResult result = MessageBox.Show("Save before exiting?", "Save?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if(result == DialogResult.Yes)
				{
					SaveFileDialog dlg = new SaveFileDialog();
					DialogResult saveResult = dlg.ShowDialog(this);
					if(saveResult == DialogResult.OK)
					{
						Connection.StorageEngine.Save(dlg.FileName);
					}
				}
			}
			else
			{
				DialogResult result = MessageBox.Show("Close this connection?", "Close Connection",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
