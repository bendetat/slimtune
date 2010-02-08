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
	partial class SlimTune
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
			System.Windows.Forms.Label RunLabel;
			System.Windows.Forms.Label ConnectLabel;
			System.Windows.Forms.Label OpenLabel;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SlimTune));
			this.WindowList = new System.Windows.Forms.ListBox();
			this.ShowHideButton = new System.Windows.Forms.Button();
			this.OpenFilePanel = new System.Windows.Forms.Panel();
			this.CompareButton = new System.Windows.Forms.Button();
			this.OpenButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.RunButton = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.ConnectButton = new System.Windows.Forms.Button();
			RunLabel = new System.Windows.Forms.Label();
			ConnectLabel = new System.Windows.Forms.Label();
			OpenLabel = new System.Windows.Forms.Label();
			this.OpenFilePanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// RunLabel
			// 
			RunLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			RunLabel.Location = new System.Drawing.Point(85, 21);
			RunLabel.Name = "RunLabel";
			RunLabel.Size = new System.Drawing.Size(328, 43);
			RunLabel.TabIndex = 2;
			RunLabel.Text = "Run an application, service, or web server with SlimTune profiling enabled. Optio" +
				"nally connects to the application as well. Start here!";
			// 
			// ConnectLabel
			// 
			ConnectLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			ConnectLabel.Location = new System.Drawing.Point(85, 21);
			ConnectLabel.Name = "ConnectLabel";
			ConnectLabel.Size = new System.Drawing.Size(328, 43);
			ConnectLabel.TabIndex = 3;
			ConnectLabel.Text = "Connect to a running application that was launched with profiling enabled. The ta" +
				"rget can be local or remote, with a configured firewall.";
			// 
			// OpenLabel
			// 
			OpenLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			OpenLabel.Location = new System.Drawing.Point(88, 21);
			OpenLabel.Name = "OpenLabel";
			OpenLabel.Size = new System.Drawing.Size(325, 43);
			OpenLabel.TabIndex = 2;
			OpenLabel.Text = "Open a previously saved set of results, or compare multiple sets of results. See " +
				"previous runs any time, with any visualizer.";
			// 
			// WindowList
			// 
			this.WindowList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.WindowList.DisplayMember = "Text";
			this.WindowList.FormattingEnabled = true;
			this.WindowList.HorizontalScrollbar = true;
			this.WindowList.Location = new System.Drawing.Point(439, 12);
			this.WindowList.Name = "WindowList";
			this.WindowList.Size = new System.Drawing.Size(205, 251);
			this.WindowList.TabIndex = 2;
			this.WindowList.ValueMember = "Text";
			this.WindowList.SelectedIndexChanged += new System.EventHandler(this.WindowList_SelectedIndexChanged);
			// 
			// ShowHideButton
			// 
			this.ShowHideButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowHideButton.Enabled = false;
			this.ShowHideButton.Location = new System.Drawing.Point(504, 266);
			this.ShowHideButton.Name = "ShowHideButton";
			this.ShowHideButton.Size = new System.Drawing.Size(75, 23);
			this.ShowHideButton.TabIndex = 4;
			this.ShowHideButton.Text = "Hide";
			this.ShowHideButton.UseVisualStyleBackColor = true;
			this.ShowHideButton.Click += new System.EventHandler(this.ShowHideButton_Click);
			// 
			// OpenFilePanel
			// 
			this.OpenFilePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OpenFilePanel.Controls.Add(OpenLabel);
			this.OpenFilePanel.Controls.Add(this.CompareButton);
			this.OpenFilePanel.Controls.Add(this.OpenButton);
			this.OpenFilePanel.Location = new System.Drawing.Point(13, 13);
			this.OpenFilePanel.Name = "OpenFilePanel";
			this.OpenFilePanel.Size = new System.Drawing.Size(420, 88);
			this.OpenFilePanel.TabIndex = 5;
			// 
			// CompareButton
			// 
			this.CompareButton.Enabled = false;
			this.CompareButton.Location = new System.Drawing.Point(3, 45);
			this.CompareButton.Name = "CompareButton";
			this.CompareButton.Size = new System.Drawing.Size(75, 23);
			this.CompareButton.TabIndex = 1;
			this.CompareButton.Text = "Compare...";
			this.CompareButton.UseVisualStyleBackColor = true;
			// 
			// OpenButton
			// 
			this.OpenButton.Location = new System.Drawing.Point(3, 16);
			this.OpenButton.Name = "OpenButton";
			this.OpenButton.Size = new System.Drawing.Size(75, 23);
			this.OpenButton.TabIndex = 0;
			this.OpenButton.Text = "Open...";
			this.OpenButton.UseVisualStyleBackColor = true;
			this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(RunLabel);
			this.panel1.Controls.Add(this.RunButton);
			this.panel1.Location = new System.Drawing.Point(13, 107);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(420, 88);
			this.panel1.TabIndex = 6;
			// 
			// RunButton
			// 
			this.RunButton.Location = new System.Drawing.Point(3, 31);
			this.RunButton.Name = "RunButton";
			this.RunButton.Size = new System.Drawing.Size(75, 23);
			this.RunButton.TabIndex = 1;
			this.RunButton.Text = "Run...";
			this.RunButton.UseVisualStyleBackColor = true;
			this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Controls.Add(ConnectLabel);
			this.panel2.Controls.Add(this.ConnectButton);
			this.panel2.Location = new System.Drawing.Point(13, 201);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(420, 88);
			this.panel2.TabIndex = 7;
			// 
			// ConnectButton
			// 
			this.ConnectButton.Location = new System.Drawing.Point(3, 31);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(75, 23);
			this.ConnectButton.TabIndex = 2;
			this.ConnectButton.Text = "Connect...";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
			// 
			// SlimTune
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(656, 295);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.OpenFilePanel);
			this.Controls.Add(this.ShowHideButton);
			this.Controls.Add(this.WindowList);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SlimTune";
			this.Text = "SlimTune Profiler";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SlimTune_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SlimTune_FormClosing);
			this.OpenFilePanel.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox WindowList;
		private System.Windows.Forms.Button ShowHideButton;
		private System.Windows.Forms.Panel OpenFilePanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button RunButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button ConnectButton;
		private System.Windows.Forms.Button CompareButton;
		private System.Windows.Forms.Button OpenButton;
	}
}