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
namespace SlimTuneUI
{
	partial class RunDialog
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
			this.m_runButton = new System.Windows.Forms.Button();
			this.m_openExeDialog = new System.Windows.Forms.OpenFileDialog();
			this.m_saveResultsDialog = new System.Windows.Forms.SaveFileDialog();
			this.m_frontendOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.m_visualizerLabel = new System.Windows.Forms.Label();
			this.m_visualizerCombo = new System.Windows.Forms.ComboBox();
			this.m_connectCheckBox = new System.Windows.Forms.CheckBox();
			this.m_browseDbButton = new System.Windows.Forms.Button();
			this.m_resultsFileTextBox = new System.Windows.Forms.TextBox();
			this.m_saveAsLabel = new System.Windows.Forms.Label();
			this.m_dirBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_launchPropGrid = new System.Windows.Forms.PropertyGrid();
			this.m_appTypeLabel = new System.Windows.Forms.Label();
			this.m_appTypeCombo = new System.Windows.Forms.ComboBox();
			this.m_frontendOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_runButton
			// 
			this.m_runButton.Location = new System.Drawing.Point(256, 471);
			this.m_runButton.Name = "m_runButton";
			this.m_runButton.Size = new System.Drawing.Size(75, 23);
			this.m_runButton.TabIndex = 9;
			this.m_runButton.Text = "Run";
			this.m_toolTip.SetToolTip(this.m_runButton, "Launches the application.");
			this.m_runButton.UseVisualStyleBackColor = true;
			this.m_runButton.Click += new System.EventHandler(this.m_runButton_Click);
			// 
			// m_openExeDialog
			// 
			this.m_openExeDialog.Filter = "Applications (*.exe)|*.exe";
			// 
			// m_saveResultsDialog
			// 
			this.m_saveResultsDialog.DefaultExt = "sdf";
			this.m_saveResultsDialog.Filter = "Results (*.sdf)|*.sdf";
			this.m_saveResultsDialog.Title = "Save Results...";
			// 
			// m_frontendOptionsGroupBox
			// 
			this.m_frontendOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_visualizerLabel);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_visualizerCombo);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_connectCheckBox);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_browseDbButton);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_resultsFileTextBox);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_saveAsLabel);
			this.m_frontendOptionsGroupBox.Location = new System.Drawing.Point(6, 366);
			this.m_frontendOptionsGroupBox.Name = "m_frontendOptionsGroupBox";
			this.m_frontendOptionsGroupBox.Size = new System.Drawing.Size(563, 99);
			this.m_frontendOptionsGroupBox.TabIndex = 16;
			this.m_frontendOptionsGroupBox.TabStop = false;
			this.m_frontendOptionsGroupBox.Text = "Front-end Options";
			// 
			// m_visualizerLabel
			// 
			this.m_visualizerLabel.AutoSize = true;
			this.m_visualizerLabel.Location = new System.Drawing.Point(9, 72);
			this.m_visualizerLabel.Name = "m_visualizerLabel";
			this.m_visualizerLabel.Size = new System.Drawing.Size(54, 13);
			this.m_visualizerLabel.TabIndex = 18;
			this.m_visualizerLabel.Text = "Visualizer:";
			// 
			// m_visualizerCombo
			// 
			this.m_visualizerCombo.DisplayMember = "Name";
			this.m_visualizerCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_visualizerCombo.FormattingEnabled = true;
			this.m_visualizerCombo.Location = new System.Drawing.Point(76, 69);
			this.m_visualizerCombo.Name = "m_visualizerCombo";
			this.m_visualizerCombo.Size = new System.Drawing.Size(400, 21);
			this.m_visualizerCombo.Sorted = true;
			this.m_visualizerCombo.TabIndex = 17;
			this.m_toolTip.SetToolTip(this.m_visualizerCombo, "The visualizer to open when the front-end connects.");
			this.m_visualizerCombo.SelectedIndexChanged += new System.EventHandler(this.m_visualizerCombo_SelectedIndexChanged);
			// 
			// m_connectCheckBox
			// 
			this.m_connectCheckBox.AutoSize = true;
			this.m_connectCheckBox.Checked = true;
			this.m_connectCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_connectCheckBox.Location = new System.Drawing.Point(9, 19);
			this.m_connectCheckBox.Name = "m_connectCheckBox";
			this.m_connectCheckBox.Size = new System.Drawing.Size(108, 17);
			this.m_connectCheckBox.TabIndex = 16;
			this.m_connectCheckBox.Text = "Connect to target";
			this.m_toolTip.SetToolTip(this.m_connectCheckBox, "If checked, the front-end will connect and begin gathering data immediately. ");
			this.m_connectCheckBox.UseVisualStyleBackColor = true;
			this.m_connectCheckBox.CheckedChanged += new System.EventHandler(this.m_connectCheckBox_CheckedChanged);
			// 
			// m_browseDbButton
			// 
			this.m_browseDbButton.Location = new System.Drawing.Point(482, 40);
			this.m_browseDbButton.Name = "m_browseDbButton";
			this.m_browseDbButton.Size = new System.Drawing.Size(75, 23);
			this.m_browseDbButton.TabIndex = 15;
			this.m_browseDbButton.Text = "Browse...";
			this.m_toolTip.SetToolTip(this.m_browseDbButton, "Opens a file browser to select a database file.");
			this.m_browseDbButton.UseVisualStyleBackColor = true;
			this.m_browseDbButton.Click += new System.EventHandler(this.m_browseDbButton_Click);
			// 
			// m_resultsFileTextBox
			// 
			this.m_resultsFileTextBox.Location = new System.Drawing.Point(76, 42);
			this.m_resultsFileTextBox.Name = "m_resultsFileTextBox";
			this.m_resultsFileTextBox.Size = new System.Drawing.Size(400, 20);
			this.m_resultsFileTextBox.TabIndex = 14;
			this.m_toolTip.SetToolTip(this.m_resultsFileTextBox, "The filename of the database to store profiling data to.");
			this.m_resultsFileTextBox.TextChanged += new System.EventHandler(this.m_resultsFileTextBox_TextChanged);
			// 
			// m_saveAsLabel
			// 
			this.m_saveAsLabel.AutoSize = true;
			this.m_saveAsLabel.Location = new System.Drawing.Point(9, 45);
			this.m_saveAsLabel.Name = "m_saveAsLabel";
			this.m_saveAsLabel.Size = new System.Drawing.Size(64, 13);
			this.m_saveAsLabel.TabIndex = 13;
			this.m_saveAsLabel.Text = "Results File:";
			// 
			// m_dirBrowser
			// 
			this.m_dirBrowser.Description = "Select the working directory to be used when launching the application.";
			// 
			// m_toolTip
			// 
			this.m_toolTip.IsBalloon = true;
			// 
			// m_launchPropGrid
			// 
			this.m_launchPropGrid.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_launchPropGrid.Location = new System.Drawing.Point(7, 37);
			this.m_launchPropGrid.Name = "m_launchPropGrid";
			this.m_launchPropGrid.Size = new System.Drawing.Size(562, 323);
			this.m_launchPropGrid.TabIndex = 17;
			// 
			// m_appTypeLabel
			// 
			this.m_appTypeLabel.AutoSize = true;
			this.m_appTypeLabel.Location = new System.Drawing.Point(6, 13);
			this.m_appTypeLabel.Name = "m_appTypeLabel";
			this.m_appTypeLabel.Size = new System.Drawing.Size(123, 13);
			this.m_appTypeLabel.TabIndex = 18;
			this.m_appTypeLabel.Text = "Target Application Type:";
			// 
			// m_appTypeCombo
			// 
			this.m_appTypeCombo.DisplayMember = "Name";
			this.m_appTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_appTypeCombo.FormattingEnabled = true;
			this.m_appTypeCombo.Location = new System.Drawing.Point(135, 10);
			this.m_appTypeCombo.Name = "m_appTypeCombo";
			this.m_appTypeCombo.Size = new System.Drawing.Size(433, 21);
			this.m_appTypeCombo.TabIndex = 19;
			this.m_appTypeCombo.ValueMember = "Type";
			// 
			// RunDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(586, 506);
			this.Controls.Add(this.m_appTypeCombo);
			this.Controls.Add(this.m_appTypeLabel);
			this.Controls.Add(this.m_launchPropGrid);
			this.Controls.Add(this.m_frontendOptionsGroupBox);
			this.Controls.Add(this.m_runButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RunDialog";
			this.Text = "RunDialog";
			this.m_frontendOptionsGroupBox.ResumeLayout(false);
			this.m_frontendOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_runButton;
		private System.Windows.Forms.OpenFileDialog m_openExeDialog;
		private System.Windows.Forms.SaveFileDialog m_saveResultsDialog;
		private System.Windows.Forms.GroupBox m_frontendOptionsGroupBox;
		private System.Windows.Forms.Button m_browseDbButton;
		private System.Windows.Forms.TextBox m_resultsFileTextBox;
		private System.Windows.Forms.Label m_saveAsLabel;
		private System.Windows.Forms.CheckBox m_connectCheckBox;
		private System.Windows.Forms.Label m_visualizerLabel;
		private System.Windows.Forms.ComboBox m_visualizerCombo;
		private System.Windows.Forms.FolderBrowserDialog m_dirBrowser;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.PropertyGrid m_launchPropGrid;
		private System.Windows.Forms.Label m_appTypeLabel;
		private System.Windows.Forms.ComboBox m_appTypeCombo;
	}
}