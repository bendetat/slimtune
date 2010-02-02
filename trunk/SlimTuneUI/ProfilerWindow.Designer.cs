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
namespace SlimTuneUI
{
	partial class ProfilerWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.VisualizerHost = new System.Windows.Forms.TabControl();
			this.SuspendLayout();
			// 
			// VisualizerHost
			// 
			this.VisualizerHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisualizerHost.Location = new System.Drawing.Point(0, 0);
			this.VisualizerHost.Name = "VisualizerHost";
			this.VisualizerHost.SelectedIndex = 0;
			this.VisualizerHost.Size = new System.Drawing.Size(799, 507);
			this.VisualizerHost.TabIndex = 0;
			// 
			// ProfilerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(799, 507);
			this.Controls.Add(this.VisualizerHost);
			this.Name = "ProfilerWindow";
			this.Text = "ProfilerWindow";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProfilerWindow_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfilerWindow_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.TabControl VisualizerHost;
	}
}