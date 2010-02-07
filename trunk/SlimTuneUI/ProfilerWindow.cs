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
