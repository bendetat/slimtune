namespace SlimTuneUI
{
	partial class ConnectionList
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionList));
			this.m_splitter = new System.Windows.Forms.SplitContainer();
			this.m_connectionList = new System.Windows.Forms.ListBox();
			this.m_clearDataButton = new System.Windows.Forms.Button();
			this.m_disconnectButton = new System.Windows.Forms.Button();
			this.m_openVisualizerButton = new System.Windows.Forms.Button();
			this.m_visualizersCombo = new System.Windows.Forms.ComboBox();
			this.m_visualizersLabel = new System.Windows.Forms.Label();
			this.m_closeButton = new System.Windows.Forms.Button();
			this.m_statusLabel = new System.Windows.Forms.Label();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_splitter.Panel1.SuspendLayout();
			this.m_splitter.Panel2.SuspendLayout();
			this.m_splitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_splitter
			// 
			this.m_splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_splitter.Location = new System.Drawing.Point(0, 0);
			this.m_splitter.Name = "m_splitter";
			this.m_splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// m_splitter.Panel1
			// 
			this.m_splitter.Panel1.Controls.Add(this.m_connectionList);
			// 
			// m_splitter.Panel2
			// 
			this.m_splitter.Panel2.Controls.Add(this.m_clearDataButton);
			this.m_splitter.Panel2.Controls.Add(this.m_disconnectButton);
			this.m_splitter.Panel2.Controls.Add(this.m_openVisualizerButton);
			this.m_splitter.Panel2.Controls.Add(this.m_visualizersCombo);
			this.m_splitter.Panel2.Controls.Add(this.m_visualizersLabel);
			this.m_splitter.Panel2.Controls.Add(this.m_closeButton);
			this.m_splitter.Panel2.Controls.Add(this.m_statusLabel);
			this.m_splitter.Size = new System.Drawing.Size(182, 494);
			this.m_splitter.SplitterDistance = 181;
			this.m_splitter.TabIndex = 0;
			// 
			// m_connectionList
			// 
			this.m_connectionList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_connectionList.FormattingEnabled = true;
			this.m_connectionList.Location = new System.Drawing.Point(0, 0);
			this.m_connectionList.Name = "m_connectionList";
			this.m_connectionList.Size = new System.Drawing.Size(182, 173);
			this.m_connectionList.TabIndex = 0;
			this.m_connectionList.SelectedIndexChanged += new System.EventHandler(this.m_connectionList_SelectedIndexChanged);
			// 
			// m_clearDataButton
			// 
			this.m_clearDataButton.Enabled = false;
			this.m_clearDataButton.Image = ((System.Drawing.Image)(resources.GetObject("m_clearDataButton.Image")));
			this.m_clearDataButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_clearDataButton.Location = new System.Drawing.Point(6, 182);
			this.m_clearDataButton.Name = "m_clearDataButton";
			this.m_clearDataButton.Size = new System.Drawing.Size(84, 23);
			this.m_clearDataButton.TabIndex = 6;
			this.m_clearDataButton.Text = "Clear Data";
			this.m_clearDataButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.m_toolTip.SetToolTip(this.m_clearDataButton, "Deletes all profiling data collected in this database.");
			this.m_clearDataButton.UseVisualStyleBackColor = true;
			this.m_clearDataButton.Click += new System.EventHandler(this.m_clearDataButton_Click);
			// 
			// m_disconnectButton
			// 
			this.m_disconnectButton.Enabled = false;
			this.m_disconnectButton.Image = ((System.Drawing.Image)(resources.GetObject("m_disconnectButton.Image")));
			this.m_disconnectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_disconnectButton.Location = new System.Drawing.Point(6, 30);
			this.m_disconnectButton.Name = "m_disconnectButton";
			this.m_disconnectButton.Size = new System.Drawing.Size(84, 23);
			this.m_disconnectButton.TabIndex = 5;
			this.m_disconnectButton.Text = "Disconnect";
			this.m_disconnectButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.m_toolTip.SetToolTip(this.m_disconnectButton, "Stops data collection and disconnects from the target.");
			this.m_disconnectButton.UseVisualStyleBackColor = true;
			this.m_disconnectButton.Click += new System.EventHandler(this.m_disconnectButton_Click);
			// 
			// m_openVisualizerButton
			// 
			this.m_openVisualizerButton.Enabled = false;
			this.m_openVisualizerButton.Image = ((System.Drawing.Image)(resources.GetObject("m_openVisualizerButton.Image")));
			this.m_openVisualizerButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_openVisualizerButton.Location = new System.Drawing.Point(6, 119);
			this.m_openVisualizerButton.Name = "m_openVisualizerButton";
			this.m_openVisualizerButton.Size = new System.Drawing.Size(107, 23);
			this.m_openVisualizerButton.TabIndex = 4;
			this.m_openVisualizerButton.Text = "Open Visualizer";
			this.m_openVisualizerButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.m_toolTip.SetToolTip(this.m_openVisualizerButton, "Opens the selected visualizer in a new tab.");
			this.m_openVisualizerButton.UseVisualStyleBackColor = true;
			this.m_openVisualizerButton.Click += new System.EventHandler(this.m_openVisualizerButton_Click);
			// 
			// m_visualizersCombo
			// 
			this.m_visualizersCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_visualizersCombo.DisplayMember = "Name";
			this.m_visualizersCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_visualizersCombo.Enabled = false;
			this.m_visualizersCombo.FormattingEnabled = true;
			this.m_visualizersCombo.Location = new System.Drawing.Point(6, 92);
			this.m_visualizersCombo.Name = "m_visualizersCombo";
			this.m_visualizersCombo.Size = new System.Drawing.Size(173, 21);
			this.m_visualizersCombo.Sorted = true;
			this.m_visualizersCombo.TabIndex = 3;
			this.m_visualizersCombo.ValueMember = "Type";
			// 
			// m_visualizersLabel
			// 
			this.m_visualizersLabel.AutoSize = true;
			this.m_visualizersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_visualizersLabel.Location = new System.Drawing.Point(3, 73);
			this.m_visualizersLabel.Name = "m_visualizersLabel";
			this.m_visualizersLabel.Size = new System.Drawing.Size(136, 16);
			this.m_visualizersLabel.TabIndex = 2;
			this.m_visualizersLabel.Text = "Available Visualizers:";
			// 
			// m_closeButton
			// 
			this.m_closeButton.Enabled = false;
			this.m_closeButton.Image = ((System.Drawing.Image)(resources.GetObject("m_closeButton.Image")));
			this.m_closeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_closeButton.Location = new System.Drawing.Point(95, 30);
			this.m_closeButton.Name = "m_closeButton";
			this.m_closeButton.Size = new System.Drawing.Size(75, 23);
			this.m_closeButton.TabIndex = 1;
			this.m_closeButton.Text = "Close";
			this.m_toolTip.SetToolTip(this.m_closeButton, "Closes this database and any visualizers using it.");
			this.m_closeButton.UseVisualStyleBackColor = true;
			this.m_closeButton.Click += new System.EventHandler(this.m_closeButton_Click);
			// 
			// m_statusLabel
			// 
			this.m_statusLabel.AutoSize = true;
			this.m_statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_statusLabel.Location = new System.Drawing.Point(3, 12);
			this.m_statusLabel.Name = "m_statusLabel";
			this.m_statusLabel.Size = new System.Drawing.Size(44, 15);
			this.m_statusLabel.TabIndex = 0;
			this.m_statusLabel.Text = "Status:";
			// 
			// m_toolTip
			// 
			this.m_toolTip.IsBalloon = true;
			// 
			// ConnectionList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(182, 494);
			this.Controls.Add(this.m_splitter);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.Document)));
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.HideOnClose = true;
			this.Name = "ConnectionList";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
			this.Text = "Connections";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConnectionList_FormClosed);
			this.m_splitter.Panel1.ResumeLayout(false);
			this.m_splitter.Panel2.ResumeLayout(false);
			this.m_splitter.Panel2.PerformLayout();
			this.m_splitter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer m_splitter;
		private System.Windows.Forms.ListBox m_connectionList;
		private System.Windows.Forms.Button m_closeButton;
		private System.Windows.Forms.Label m_statusLabel;
		private System.Windows.Forms.ComboBox m_visualizersCombo;
		private System.Windows.Forms.Label m_visualizersLabel;
		private System.Windows.Forms.Button m_openVisualizerButton;
		private System.Windows.Forms.Button m_disconnectButton;
		private System.Windows.Forms.Button m_clearDataButton;
		private System.Windows.Forms.ToolTip m_toolTip;
	}
}