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
			System.Windows.Forms.GroupBox VisualizersGroupBox;
			this.label1 = new System.Windows.Forms.Label();
			this.m_openVisualizerButton = new System.Windows.Forms.Button();
			this.m_visualizerCombo = new System.Windows.Forms.ComboBox();
			this.MainSplitter = new System.Windows.Forms.SplitContainer();
			this.TasksGroupBox = new System.Windows.Forms.GroupBox();
			this.ClearDataButton = new System.Windows.Forms.Button();
			this.SnapshotButton = new System.Windows.Forms.Button();
			this.TasksLabel = new System.Windows.Forms.Label();
			this.InfoGroupBox = new System.Windows.Forms.GroupBox();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.NameLabel = new System.Windows.Forms.Label();
			this.EngineLabel = new System.Windows.Forms.Label();
			this.PortLabel = new System.Windows.Forms.Label();
			this.HostLabel = new System.Windows.Forms.Label();
			this.m_closeVisualizerButton = new System.Windows.Forms.Button();
			this.VisualizerHost = new System.Windows.Forms.TabControl();
			VisualizersGroupBox = new System.Windows.Forms.GroupBox();
			VisualizersGroupBox.SuspendLayout();
			this.MainSplitter.Panel1.SuspendLayout();
			this.MainSplitter.Panel2.SuspendLayout();
			this.MainSplitter.SuspendLayout();
			this.TasksGroupBox.SuspendLayout();
			this.InfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// VisualizersGroupBox
			// 
			VisualizersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			VisualizersGroupBox.Controls.Add(this.label1);
			VisualizersGroupBox.Controls.Add(this.m_openVisualizerButton);
			VisualizersGroupBox.Controls.Add(this.m_visualizerCombo);
			VisualizersGroupBox.Location = new System.Drawing.Point(3, 136);
			VisualizersGroupBox.Name = "VisualizersGroupBox";
			VisualizersGroupBox.Size = new System.Drawing.Size(195, 98);
			VisualizersGroupBox.TabIndex = 4;
			VisualizersGroupBox.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label1.Location = new System.Drawing.Point(7, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(139, 14);
			this.label1.TabIndex = 7;
			this.label1.Text = "Available Visualizers:";
			// 
			// m_openVisualizerButton
			// 
			this.m_openVisualizerButton.Location = new System.Drawing.Point(6, 64);
			this.m_openVisualizerButton.Name = "m_openVisualizerButton";
			this.m_openVisualizerButton.Size = new System.Drawing.Size(91, 23);
			this.m_openVisualizerButton.TabIndex = 6;
			this.m_openVisualizerButton.Text = "Open Visualizer";
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
			this.m_visualizerCombo.Location = new System.Drawing.Point(6, 37);
			this.m_visualizerCombo.Name = "m_visualizerCombo";
			this.m_visualizerCombo.Size = new System.Drawing.Size(183, 21);
			this.m_visualizerCombo.Sorted = true;
			this.m_visualizerCombo.TabIndex = 5;
			this.m_visualizerCombo.ValueMember = "Name";
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
			this.MainSplitter.Panel1.Controls.Add(this.TasksGroupBox);
			this.MainSplitter.Panel1.Controls.Add(this.InfoGroupBox);
			this.MainSplitter.Panel1.Controls.Add(VisualizersGroupBox);
			// 
			// MainSplitter.Panel2
			// 
			this.MainSplitter.Panel2.Controls.Add(this.m_closeVisualizerButton);
			this.MainSplitter.Panel2.Controls.Add(this.VisualizerHost);
			this.MainSplitter.Size = new System.Drawing.Size(968, 501);
			this.MainSplitter.SplitterDistance = 200;
			this.MainSplitter.TabIndex = 1;
			// 
			// TasksGroupBox
			// 
			this.TasksGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TasksGroupBox.Controls.Add(this.ClearDataButton);
			this.TasksGroupBox.Controls.Add(this.SnapshotButton);
			this.TasksGroupBox.Controls.Add(this.TasksLabel);
			this.TasksGroupBox.Location = new System.Drawing.Point(3, 240);
			this.TasksGroupBox.Name = "TasksGroupBox";
			this.TasksGroupBox.Size = new System.Drawing.Size(194, 67);
			this.TasksGroupBox.TabIndex = 6;
			this.TasksGroupBox.TabStop = false;
			// 
			// ClearDataButton
			// 
			this.ClearDataButton.Location = new System.Drawing.Point(92, 38);
			this.ClearDataButton.Name = "ClearDataButton";
			this.ClearDataButton.Size = new System.Drawing.Size(75, 23);
			this.ClearDataButton.TabIndex = 2;
			this.ClearDataButton.Text = "Clear Data";
			this.ClearDataButton.UseVisualStyleBackColor = true;
			this.ClearDataButton.Click += new System.EventHandler(this.ClearDataButton_Click);
			// 
			// SnapshotButton
			// 
			this.SnapshotButton.Enabled = false;
			this.SnapshotButton.Location = new System.Drawing.Point(10, 38);
			this.SnapshotButton.Name = "SnapshotButton";
			this.SnapshotButton.Size = new System.Drawing.Size(75, 23);
			this.SnapshotButton.TabIndex = 1;
			this.SnapshotButton.Text = "Snapshot";
			this.SnapshotButton.UseVisualStyleBackColor = true;
			this.SnapshotButton.Click += new System.EventHandler(this.SnapshotButton_Click);
			// 
			// TasksLabel
			// 
			this.TasksLabel.AutoSize = true;
			this.TasksLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.TasksLabel.Location = new System.Drawing.Point(7, 20);
			this.TasksLabel.Name = "TasksLabel";
			this.TasksLabel.Size = new System.Drawing.Size(48, 14);
			this.TasksLabel.TabIndex = 0;
			this.TasksLabel.Text = "Tasks:";
			// 
			// InfoGroupBox
			// 
			this.InfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoGroupBox.Controls.Add(this.StatusLabel);
			this.InfoGroupBox.Controls.Add(this.NameLabel);
			this.InfoGroupBox.Controls.Add(this.EngineLabel);
			this.InfoGroupBox.Controls.Add(this.PortLabel);
			this.InfoGroupBox.Controls.Add(this.HostLabel);
			this.InfoGroupBox.Location = new System.Drawing.Point(3, 3);
			this.InfoGroupBox.Name = "InfoGroupBox";
			this.InfoGroupBox.Size = new System.Drawing.Size(195, 127);
			this.InfoGroupBox.TabIndex = 5;
			this.InfoGroupBox.TabStop = false;
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
			this.VisualizerHost.Size = new System.Drawing.Size(764, 501);
			this.VisualizerHost.TabIndex = 1;
			// 
			// ProfilerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(968, 501);
			this.Controls.Add(this.MainSplitter);
			this.Name = "ProfilerWindow";
			this.Text = "ProfilerWindow";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProfilerWindow_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfilerWindow_FormClosing);
			VisualizersGroupBox.ResumeLayout(false);
			VisualizersGroupBox.PerformLayout();
			this.MainSplitter.Panel1.ResumeLayout(false);
			this.MainSplitter.Panel2.ResumeLayout(false);
			this.MainSplitter.ResumeLayout(false);
			this.TasksGroupBox.ResumeLayout(false);
			this.TasksGroupBox.PerformLayout();
			this.InfoGroupBox.ResumeLayout(false);
			this.InfoGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer MainSplitter;
		internal System.Windows.Forms.TabControl VisualizerHost;
		private System.Windows.Forms.Button m_openVisualizerButton;
		private System.Windows.Forms.ComboBox m_visualizerCombo;
		private System.Windows.Forms.GroupBox InfoGroupBox;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.Label EngineLabel;
		private System.Windows.Forms.Label PortLabel;
		private System.Windows.Forms.Label HostLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label StatusLabel;
		private System.Windows.Forms.GroupBox TasksGroupBox;
		private System.Windows.Forms.Label TasksLabel;
		private System.Windows.Forms.Button ClearDataButton;
		private System.Windows.Forms.Button SnapshotButton;
		private System.Windows.Forms.Button m_closeVisualizerButton;

	}
}