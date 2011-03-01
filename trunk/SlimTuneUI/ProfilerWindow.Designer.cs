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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfilerWindow));
			this.MainSplitter = new System.Windows.Forms.SplitContainer();
			this.SnapshotsGroupBox = new System.Windows.Forms.GroupBox();
			this.RenameSnapshotButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_openVisualizerButton = new System.Windows.Forms.Button();
			this.m_visualizerCombo = new System.Windows.Forms.ComboBox();
			this.DeleteSnapshotButton = new System.Windows.Forms.Button();
			this.SnapshotButton = new System.Windows.Forms.Button();
			this.SnapshotsListBox = new System.Windows.Forms.CheckedListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.InfoGroupBox = new System.Windows.Forms.GroupBox();
			this.ReconnectButton = new System.Windows.Forms.Button();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.NameLabel = new System.Windows.Forms.Label();
			this.EngineLabel = new System.Windows.Forms.Label();
			this.PortLabel = new System.Windows.Forms.Label();
			this.HostLabel = new System.Windows.Forms.Label();
			this.m_closeVisualizerButton = new System.Windows.Forms.Button();
			this.VisualizerHost = new System.Windows.Forms.TabControl();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.MainSplitter.Panel1.SuspendLayout();
			this.MainSplitter.Panel2.SuspendLayout();
			this.MainSplitter.SuspendLayout();
			this.SnapshotsGroupBox.SuspendLayout();
			this.InfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainSplitter
			// 
			this.MainSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitter.Location = new System.Drawing.Point(0, 0);
			this.MainSplitter.Name = "MainSplitter";
			// 
			// MainSplitter.Panel1
			// 
			this.MainSplitter.Panel1.Controls.Add(this.SnapshotsGroupBox);
			this.MainSplitter.Panel1.Controls.Add(this.InfoGroupBox);
			// 
			// MainSplitter.Panel2
			// 
			this.MainSplitter.Panel2.Controls.Add(this.m_closeVisualizerButton);
			this.MainSplitter.Panel2.Controls.Add(this.VisualizerHost);
			this.MainSplitter.Size = new System.Drawing.Size(968, 594);
			this.MainSplitter.SplitterDistance = 200;
			this.MainSplitter.TabIndex = 1;
			// 
			// SnapshotsGroupBox
			// 
			this.SnapshotsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SnapshotsGroupBox.Controls.Add(this.RenameSnapshotButton);
			this.SnapshotsGroupBox.Controls.Add(this.label1);
			this.SnapshotsGroupBox.Controls.Add(this.m_openVisualizerButton);
			this.SnapshotsGroupBox.Controls.Add(this.m_visualizerCombo);
			this.SnapshotsGroupBox.Controls.Add(this.DeleteSnapshotButton);
			this.SnapshotsGroupBox.Controls.Add(this.SnapshotButton);
			this.SnapshotsGroupBox.Controls.Add(this.SnapshotsListBox);
			this.SnapshotsGroupBox.Controls.Add(this.label2);
			this.SnapshotsGroupBox.Location = new System.Drawing.Point(4, 161);
			this.SnapshotsGroupBox.Name = "SnapshotsGroupBox";
			this.SnapshotsGroupBox.Size = new System.Drawing.Size(194, 211);
			this.SnapshotsGroupBox.TabIndex = 11;
			this.SnapshotsGroupBox.TabStop = false;
			// 
			// RenameSnapshotButton
			// 
			this.RenameSnapshotButton.Enabled = false;
			this.RenameSnapshotButton.Image = ((System.Drawing.Image) (resources.GetObject("RenameSnapshotButton.Image")));
			this.RenameSnapshotButton.Location = new System.Drawing.Point(5, 98);
			this.RenameSnapshotButton.Name = "RenameSnapshotButton";
			this.RenameSnapshotButton.Size = new System.Drawing.Size(30, 23);
			this.RenameSnapshotButton.TabIndex = 11;
			this.m_toolTip.SetToolTip(this.RenameSnapshotButton, "Rename the selected snapshot.");
			this.RenameSnapshotButton.UseVisualStyleBackColor = true;
			this.RenameSnapshotButton.Click += new System.EventHandler(this.RenameSnapshotButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label1.Location = new System.Drawing.Point(2, 137);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(139, 14);
			this.label1.TabIndex = 10;
			this.label1.Text = "Available Visualizers:";
			// 
			// m_openVisualizerButton
			// 
			this.m_openVisualizerButton.Location = new System.Drawing.Point(5, 181);
			this.m_openVisualizerButton.Name = "m_openVisualizerButton";
			this.m_openVisualizerButton.Size = new System.Drawing.Size(91, 23);
			this.m_openVisualizerButton.TabIndex = 9;
			this.m_openVisualizerButton.Text = "Open Visualizer";
			this.m_toolTip.SetToolTip(this.m_openVisualizerButton, "Open the selected visualizer, viewing the selected snapshot.");
			this.m_openVisualizerButton.UseVisualStyleBackColor = true;
			this.m_openVisualizerButton.Click += new System.EventHandler(this.m_openVisualizerButton_Click);
			// 
			// m_visualizerCombo
			// 
			this.m_visualizerCombo.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_visualizerCombo.DisplayMember = "Name";
			this.m_visualizerCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_visualizerCombo.FormattingEnabled = true;
			this.m_visualizerCombo.Location = new System.Drawing.Point(5, 154);
			this.m_visualizerCombo.Name = "m_visualizerCombo";
			this.m_visualizerCombo.Size = new System.Drawing.Size(183, 21);
			this.m_visualizerCombo.Sorted = true;
			this.m_visualizerCombo.TabIndex = 8;
			this.m_visualizerCombo.ValueMember = "Name";
			// 
			// DeleteSnapshotButton
			// 
			this.DeleteSnapshotButton.Image = ((System.Drawing.Image) (resources.GetObject("DeleteSnapshotButton.Image")));
			this.DeleteSnapshotButton.Location = new System.Drawing.Point(5, 69);
			this.DeleteSnapshotButton.Name = "DeleteSnapshotButton";
			this.DeleteSnapshotButton.Size = new System.Drawing.Size(30, 23);
			this.DeleteSnapshotButton.TabIndex = 3;
			this.m_toolTip.SetToolTip(this.DeleteSnapshotButton, "Delete the selected snapshot.");
			this.DeleteSnapshotButton.UseVisualStyleBackColor = true;
			this.DeleteSnapshotButton.Click += new System.EventHandler(this.DeleteSnapshotButton_Click);
			// 
			// SnapshotButton
			// 
			this.SnapshotButton.Enabled = false;
			this.SnapshotButton.Image = ((System.Drawing.Image) (resources.GetObject("SnapshotButton.Image")));
			this.SnapshotButton.Location = new System.Drawing.Point(5, 40);
			this.SnapshotButton.Name = "SnapshotButton";
			this.SnapshotButton.Size = new System.Drawing.Size(30, 23);
			this.SnapshotButton.TabIndex = 2;
			this.m_toolTip.SetToolTip(this.SnapshotButton, "Create a new snapshot.");
			this.SnapshotButton.UseVisualStyleBackColor = true;
			this.SnapshotButton.Click += new System.EventHandler(this.SnapshotButton_Click);
			// 
			// SnapshotsListBox
			// 
			this.SnapshotsListBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SnapshotsListBox.FormattingEnabled = true;
			this.SnapshotsListBox.Location = new System.Drawing.Point(41, 40);
			this.SnapshotsListBox.Name = "SnapshotsListBox";
			this.SnapshotsListBox.Size = new System.Drawing.Size(147, 94);
			this.SnapshotsListBox.TabIndex = 1;
			this.SnapshotsListBox.SelectedIndexChanged += new System.EventHandler(this.SnapshotsListBox_SelectedIndexChanged);
			this.SnapshotsListBox.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.SnapshotsListBox_Format);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label2.Location = new System.Drawing.Point(7, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 14);
			this.label2.TabIndex = 0;
			this.label2.Text = "Snapshots:";
			// 
			// InfoGroupBox
			// 
			this.InfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoGroupBox.Controls.Add(this.ReconnectButton);
			this.InfoGroupBox.Controls.Add(this.StatusLabel);
			this.InfoGroupBox.Controls.Add(this.NameLabel);
			this.InfoGroupBox.Controls.Add(this.EngineLabel);
			this.InfoGroupBox.Controls.Add(this.PortLabel);
			this.InfoGroupBox.Controls.Add(this.HostLabel);
			this.InfoGroupBox.Location = new System.Drawing.Point(3, 3);
			this.InfoGroupBox.Name = "InfoGroupBox";
			this.InfoGroupBox.Size = new System.Drawing.Size(195, 152);
			this.InfoGroupBox.TabIndex = 5;
			this.InfoGroupBox.TabStop = false;
			// 
			// ReconnectButton
			// 
			this.ReconnectButton.Enabled = false;
			this.ReconnectButton.Location = new System.Drawing.Point(6, 124);
			this.ReconnectButton.Name = "ReconnectButton";
			this.ReconnectButton.Size = new System.Drawing.Size(75, 22);
			this.ReconnectButton.TabIndex = 9;
			this.ReconnectButton.Text = "Reconnect";
			this.m_toolTip.SetToolTip(this.ReconnectButton, "Reconnect to the application this results file profiled previously.");
			this.ReconnectButton.UseVisualStyleBackColor = true;
			this.ReconnectButton.Click += new System.EventHandler(this.ReconnectButton_Click);
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.StatusLabel.Location = new System.Drawing.Point(7, 107);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(40, 13);
			this.StatusLabel.TabIndex = 8;
			this.StatusLabel.Text = "Status:";
			// 
			// NameLabel
			// 
			this.NameLabel.AutoSize = true;
			this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.NameLabel.Location = new System.Drawing.Point(7, 85);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = new System.Drawing.Size(38, 13);
			this.NameLabel.TabIndex = 7;
			this.NameLabel.Text = "Name:";
			// 
			// EngineLabel
			// 
			this.EngineLabel.AutoSize = true;
			this.EngineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.EngineLabel.Location = new System.Drawing.Point(7, 62);
			this.EngineLabel.Name = "EngineLabel";
			this.EngineLabel.Size = new System.Drawing.Size(43, 13);
			this.EngineLabel.TabIndex = 6;
			this.EngineLabel.Text = "Engine:";
			// 
			// PortLabel
			// 
			this.PortLabel.AutoSize = true;
			this.PortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.PortLabel.Location = new System.Drawing.Point(7, 39);
			this.PortLabel.Name = "PortLabel";
			this.PortLabel.Size = new System.Drawing.Size(29, 13);
			this.PortLabel.TabIndex = 5;
			this.PortLabel.Text = "Port:";
			// 
			// HostLabel
			// 
			this.HostLabel.AutoSize = true;
			this.HostLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.HostLabel.Location = new System.Drawing.Point(7, 16);
			this.HostLabel.Name = "HostLabel";
			this.HostLabel.Size = new System.Drawing.Size(32, 13);
			this.HostLabel.TabIndex = 4;
			this.HostLabel.Text = "Host:";
			// 
			// m_closeVisualizerButton
			// 
			this.m_closeVisualizerButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_closeVisualizerButton.Enabled = false;
			this.m_closeVisualizerButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_closeVisualizerButton.Location = new System.Drawing.Point(744, 0);
			this.m_closeVisualizerButton.Name = "m_closeVisualizerButton";
			this.m_closeVisualizerButton.Size = new System.Drawing.Size(20, 20);
			this.m_closeVisualizerButton.TabIndex = 2;
			this.m_closeVisualizerButton.Text = "X";
			this.m_closeVisualizerButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.m_closeVisualizerButton.UseVisualStyleBackColor = true;
			this.m_closeVisualizerButton.Click += new System.EventHandler(this.m_closeVisualizerButton_Click);
			// 
			// VisualizerHost
			// 
			this.VisualizerHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisualizerHost.Location = new System.Drawing.Point(0, 0);
			this.VisualizerHost.Name = "VisualizerHost";
			this.VisualizerHost.SelectedIndex = 0;
			this.VisualizerHost.Size = new System.Drawing.Size(764, 594);
			this.VisualizerHost.TabIndex = 1;
			this.VisualizerHost.MouseClick += new System.Windows.Forms.MouseEventHandler(this.VisualizerHost_MouseClick);
			// 
			// m_toolTip
			// 
			this.m_toolTip.IsBalloon = true;
			// 
			// ProfilerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(968, 594);
			this.Controls.Add(this.MainSplitter);
			this.Name = "ProfilerWindow";
			this.Text = "ProfilerWindow";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfilerWindow_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProfilerWindow_FormClosed);
			this.MainSplitter.Panel1.ResumeLayout(false);
			this.MainSplitter.Panel2.ResumeLayout(false);
			this.MainSplitter.ResumeLayout(false);
			this.SnapshotsGroupBox.ResumeLayout(false);
			this.SnapshotsGroupBox.PerformLayout();
			this.InfoGroupBox.ResumeLayout(false);
			this.InfoGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer MainSplitter;
		internal System.Windows.Forms.TabControl VisualizerHost;
		private System.Windows.Forms.GroupBox InfoGroupBox;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.Label EngineLabel;
		private System.Windows.Forms.Label PortLabel;
		private System.Windows.Forms.Label HostLabel;
		private System.Windows.Forms.Label StatusLabel;
		private System.Windows.Forms.Button m_closeVisualizerButton;
		private System.Windows.Forms.GroupBox SnapshotsGroupBox;
		private System.Windows.Forms.CheckedListBox SnapshotsListBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ReconnectButton;
		private System.Windows.Forms.Button SnapshotButton;
		private System.Windows.Forms.Button DeleteSnapshotButton;
		private System.Windows.Forms.Button RenameSnapshotButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button m_openVisualizerButton;
		private System.Windows.Forms.ComboBox m_visualizerCombo;
		private System.Windows.Forms.ToolTip m_toolTip;

	}
}