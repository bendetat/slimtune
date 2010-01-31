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

using System.Data.SqlServerCe;

using UICore;

namespace SlimTuneUI
{
	public partial class MainWindow : SlimTuneWindowBase
	{
		public ConnectionList ConnectionList { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			Plugins.Load();

			ConnectionList = new ConnectionList(this);
			ConnectionList.Show(DockPanel);
		}

		private void m_profilerRunMenu_Click(object sender, EventArgs e)
		{
			var runner = new RunDialog(this);
			runner.ShowDialog(this);
		}

		private void m_fileOpenMenu_Click(object sender, EventArgs e)
		{
			DialogResult result = m_openDialog.ShowDialog(this);
			if(result == DialogResult.OK)
			{
				try
				{
					IStorageEngine engine = new SQLiteEngine(m_openDialog.FileName, false);

					Connection conn = new Connection(engine);
					ConnectionList.AddConnection(conn);
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void m_fileExitMenu_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void m_profilerConnectMenu_Click(object sender, EventArgs e)
		{
			var connect = new ConnectDialog(this);
			connect.Show(this);
		}

		private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			ConnectionList.Close();
		}

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(ConnectionList.ConnectionsCount > 0)
			{
				DialogResult result = MessageBox.Show("There are still active connections open. Close anyway?",
					"Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if(result == DialogResult.No)
					e.Cancel = true;
			}
		}

		private void m_helpContentsMenu_Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, "SlimTune.chm");
		}

		private void m_helpIndexMenu_Click(object sender, EventArgs e)
		{
			Help.ShowHelpIndex(this, "SlimTune.chm");
		}

		private void m_viewConnectionsMenu_Click(object sender, EventArgs e)
		{
			ConnectionList.Show(DockPanel);
		}
	}
}
