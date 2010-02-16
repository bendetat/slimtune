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
	partial class ConnectDialog
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
			this.m_frontendOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.SQLiteMemoryRadio = new System.Windows.Forms.RadioButton();
			this.SQLiteRadio = new System.Windows.Forms.RadioButton();
			this.m_visualizerLabel = new System.Windows.Forms.Label();
			this.m_visualizerCombo = new System.Windows.Forms.ComboBox();
			this.m_browseDbButton = new System.Windows.Forms.Button();
			this.m_resultsFileTextBox = new System.Windows.Forms.TextBox();
			this.m_saveAsLabel = new System.Windows.Forms.Label();
			this.m_connectOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.m_testConnectionButton = new System.Windows.Forms.Button();
			this.m_portTextBox = new System.Windows.Forms.TextBox();
			this.m_portLabel = new System.Windows.Forms.Label();
			this.m_hostNameTextBox = new System.Windows.Forms.TextBox();
			this.m_executableLabel = new System.Windows.Forms.Label();
			this.m_connectButton = new System.Windows.Forms.Button();
			this.m_saveResultsDialog = new System.Windows.Forms.SaveFileDialog();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_frontendOptionsGroupBox.SuspendLayout();
			this.m_connectOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_frontendOptionsGroupBox
			// 
			this.m_frontendOptionsGroupBox.Controls.Add(this.SQLiteMemoryRadio);
			this.m_frontendOptionsGroupBox.Controls.Add(this.SQLiteRadio);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_visualizerLabel);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_visualizerCombo);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_browseDbButton);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_resultsFileTextBox);
			this.m_frontendOptionsGroupBox.Controls.Add(this.m_saveAsLabel);
			this.m_frontendOptionsGroupBox.Location = new System.Drawing.Point(13, 97);
			this.m_frontendOptionsGroupBox.Name = "m_frontendOptionsGroupBox";
			this.m_frontendOptionsGroupBox.Size = new System.Drawing.Size(562, 97);
			this.m_frontendOptionsGroupBox.TabIndex = 18;
			this.m_frontendOptionsGroupBox.TabStop = false;
			this.m_frontendOptionsGroupBox.Text = "Front-end Options";
			// 
			// SQLiteMemoryRadio
			// 
			this.SQLiteMemoryRadio.AutoSize = true;
			this.SQLiteMemoryRadio.Location = new System.Drawing.Point(76, 20);
			this.SQLiteMemoryRadio.Name = "SQLiteMemoryRadio";
			this.SQLiteMemoryRadio.Size = new System.Drawing.Size(109, 17);
			this.SQLiteMemoryRadio.TabIndex = 22;
			this.SQLiteMemoryRadio.Text = "SQLite In-Memory";
			this.SQLiteMemoryRadio.UseVisualStyleBackColor = true;
			this.SQLiteMemoryRadio.CheckedChanged += new System.EventHandler(this.EngineChanged);
			// 
			// SQLiteRadio
			// 
			this.SQLiteRadio.AutoSize = true;
			this.SQLiteRadio.Checked = true;
			this.SQLiteRadio.Location = new System.Drawing.Point(12, 20);
			this.SQLiteRadio.Name = "SQLiteRadio";
			this.SQLiteRadio.Size = new System.Drawing.Size(57, 17);
			this.SQLiteRadio.TabIndex = 21;
			this.SQLiteRadio.TabStop = true;
			this.SQLiteRadio.Text = "SQLite";
			this.SQLiteRadio.UseVisualStyleBackColor = true;
			this.SQLiteRadio.CheckedChanged += new System.EventHandler(this.EngineChanged);
			// 
			// m_visualizerLabel
			// 
			this.m_visualizerLabel.AutoSize = true;
			this.m_visualizerLabel.Location = new System.Drawing.Point(9, 72);
			this.m_visualizerLabel.Name = "m_visualizerLabel";
			this.m_visualizerLabel.Size = new System.Drawing.Size(54, 13);
			this.m_visualizerLabel.TabIndex = 20;
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
			this.m_visualizerCombo.TabIndex = 19;
			this.m_toolTip.SetToolTip(this.m_visualizerCombo, "The visualizer to open when the front-end connects.");
			// 
			// m_browseDbButton
			// 
			this.m_browseDbButton.Location = new System.Drawing.Point(482, 41);
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
			this.m_resultsFileTextBox.Location = new System.Drawing.Point(76, 43);
			this.m_resultsFileTextBox.Name = "m_resultsFileTextBox";
			this.m_resultsFileTextBox.Size = new System.Drawing.Size(400, 20);
			this.m_resultsFileTextBox.TabIndex = 14;
			this.m_toolTip.SetToolTip(this.m_resultsFileTextBox, "The filename of the database to store profiling data to.");
			// 
			// m_saveAsLabel
			// 
			this.m_saveAsLabel.AutoSize = true;
			this.m_saveAsLabel.Location = new System.Drawing.Point(6, 46);
			this.m_saveAsLabel.Name = "m_saveAsLabel";
			this.m_saveAsLabel.Size = new System.Drawing.Size(64, 13);
			this.m_saveAsLabel.TabIndex = 13;
			this.m_saveAsLabel.Text = "Results File:";
			// 
			// m_connectOptionsGroupBox
			// 
			this.m_connectOptionsGroupBox.Controls.Add(this.m_testConnectionButton);
			this.m_connectOptionsGroupBox.Controls.Add(this.m_portTextBox);
			this.m_connectOptionsGroupBox.Controls.Add(this.m_portLabel);
			this.m_connectOptionsGroupBox.Controls.Add(this.m_hostNameTextBox);
			this.m_connectOptionsGroupBox.Controls.Add(this.m_executableLabel);
			this.m_connectOptionsGroupBox.Location = new System.Drawing.Point(12, 12);
			this.m_connectOptionsGroupBox.Name = "m_connectOptionsGroupBox";
			this.m_connectOptionsGroupBox.Size = new System.Drawing.Size(563, 79);
			this.m_connectOptionsGroupBox.TabIndex = 17;
			this.m_connectOptionsGroupBox.TabStop = false;
			this.m_connectOptionsGroupBox.Text = "Connection Options";
			// 
			// m_testConnectionButton
			// 
			this.m_testConnectionButton.Location = new System.Drawing.Point(451, 48);
			this.m_testConnectionButton.Name = "m_testConnectionButton";
			this.m_testConnectionButton.Size = new System.Drawing.Size(105, 23);
			this.m_testConnectionButton.TabIndex = 27;
			this.m_testConnectionButton.Text = "Test Connection";
			this.m_testConnectionButton.UseVisualStyleBackColor = true;
			this.m_testConnectionButton.Click += new System.EventHandler(this.m_testConnectionButton_Click);
			// 
			// m_portTextBox
			// 
			this.m_portTextBox.Location = new System.Drawing.Point(69, 50);
			this.m_portTextBox.Name = "m_portTextBox";
			this.m_portTextBox.Size = new System.Drawing.Size(100, 20);
			this.m_portTextBox.TabIndex = 26;
			this.m_portTextBox.Text = "3000";
			this.m_toolTip.SetToolTip(this.m_portTextBox, "The port to connect on. The default will usually be fine.");
			// 
			// m_portLabel
			// 
			this.m_portLabel.AutoSize = true;
			this.m_portLabel.Location = new System.Drawing.Point(6, 53);
			this.m_portLabel.Name = "m_portLabel";
			this.m_portLabel.Size = new System.Drawing.Size(64, 13);
			this.m_portLabel.TabIndex = 25;
			this.m_portLabel.Text = "Profiler Port:";
			// 
			// m_hostNameTextBox
			// 
			this.m_hostNameTextBox.Location = new System.Drawing.Point(69, 24);
			this.m_hostNameTextBox.Name = "m_hostNameTextBox";
			this.m_hostNameTextBox.Size = new System.Drawing.Size(487, 20);
			this.m_hostNameTextBox.TabIndex = 16;
			this.m_hostNameTextBox.Text = "localhost";
			this.m_toolTip.SetToolTip(this.m_hostNameTextBox, "The name of the machine running the application to be profiled.");
			// 
			// m_executableLabel
			// 
			this.m_executableLabel.AutoSize = true;
			this.m_executableLabel.Location = new System.Drawing.Point(6, 27);
			this.m_executableLabel.Name = "m_executableLabel";
			this.m_executableLabel.Size = new System.Drawing.Size(61, 13);
			this.m_executableLabel.TabIndex = 15;
			this.m_executableLabel.Text = "Host name:";
			this.m_executableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_connectButton
			// 
			this.m_connectButton.Location = new System.Drawing.Point(253, 200);
			this.m_connectButton.Name = "m_connectButton";
			this.m_connectButton.Size = new System.Drawing.Size(75, 23);
			this.m_connectButton.TabIndex = 19;
			this.m_connectButton.Text = "Connect";
			this.m_connectButton.UseVisualStyleBackColor = true;
			this.m_connectButton.Click += new System.EventHandler(this.m_connectButton_Click);
			// 
			// m_saveResultsDialog
			// 
			this.m_saveResultsDialog.DefaultExt = "sdf";
			this.m_saveResultsDialog.Filter = "Results (*.sqlite)|*.sqlite";
			this.m_saveResultsDialog.Title = "Save Results...";
			// 
			// m_toolTip
			// 
			this.m_toolTip.IsBalloon = true;
			// 
			// ConnectDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(580, 227);
			this.Controls.Add(this.m_connectButton);
			this.Controls.Add(this.m_frontendOptionsGroupBox);
			this.Controls.Add(this.m_connectOptionsGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConnectDialog";
			this.Text = "Connect to Application";
			this.m_frontendOptionsGroupBox.ResumeLayout(false);
			this.m_frontendOptionsGroupBox.PerformLayout();
			this.m_connectOptionsGroupBox.ResumeLayout(false);
			this.m_connectOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_frontendOptionsGroupBox;
		private System.Windows.Forms.Button m_browseDbButton;
		private System.Windows.Forms.TextBox m_resultsFileTextBox;
		private System.Windows.Forms.Label m_saveAsLabel;
		private System.Windows.Forms.GroupBox m_connectOptionsGroupBox;
		private System.Windows.Forms.TextBox m_portTextBox;
		private System.Windows.Forms.Label m_portLabel;
		private System.Windows.Forms.TextBox m_hostNameTextBox;
		private System.Windows.Forms.Label m_executableLabel;
		private System.Windows.Forms.Button m_connectButton;
		private System.Windows.Forms.SaveFileDialog m_saveResultsDialog;
		private System.Windows.Forms.Label m_visualizerLabel;
		private System.Windows.Forms.ComboBox m_visualizerCombo;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.Button m_testConnectionButton;
		private System.Windows.Forms.RadioButton SQLiteMemoryRadio;
		private System.Windows.Forms.RadioButton SQLiteRadio;
	}
}