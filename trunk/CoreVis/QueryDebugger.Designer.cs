namespace SlimTuneUI.CoreVis
{
	partial class QueryDebugger
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Splitter = new System.Windows.Forms.SplitContainer();
			this.SqlRadioButton = new System.Windows.Forms.RadioButton();
			this.HqlRadioButton = new System.Windows.Forms.RadioButton();
			this.QueryButton = new System.Windows.Forms.Button();
			this.QueryEditor = new ScintillaNet.Scintilla();
			this.DataViewer = new System.Windows.Forms.DataGridView();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) (this.QueryEditor)).BeginInit();
			((System.ComponentModel.ISupportInitialize) (this.DataViewer)).BeginInit();
			this.SuspendLayout();
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.Splitter.Location = new System.Drawing.Point(0, 0);
			this.Splitter.Name = "Splitter";
			// 
			// Splitter.Panel1
			// 
			this.Splitter.Panel1.Controls.Add(this.SqlRadioButton);
			this.Splitter.Panel1.Controls.Add(this.HqlRadioButton);
			this.Splitter.Panel1.Controls.Add(this.QueryButton);
			this.Splitter.Panel1.Controls.Add(this.QueryEditor);
			// 
			// Splitter.Panel2
			// 
			this.Splitter.Panel2.Controls.Add(this.DataViewer);
			this.Splitter.Size = new System.Drawing.Size(801, 485);
			this.Splitter.SplitterDistance = 300;
			this.Splitter.SplitterWidth = 3;
			this.Splitter.TabIndex = 0;
			// 
			// SqlRadioButton
			// 
			this.SqlRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SqlRadioButton.AutoSize = true;
			this.SqlRadioButton.Checked = true;
			this.SqlRadioButton.Location = new System.Drawing.Point(84, 442);
			this.SqlRadioButton.Name = "SqlRadioButton";
			this.SqlRadioButton.Size = new System.Drawing.Size(126, 17);
			this.SqlRadioButton.TabIndex = 3;
			this.SqlRadioButton.TabStop = true;
			this.SqlRadioButton.Text = "SQL (Raw Database)";
			this.SqlRadioButton.UseVisualStyleBackColor = true;
			// 
			// HqlRadioButton
			// 
			this.HqlRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.HqlRadioButton.AutoSize = true;
			this.HqlRadioButton.Location = new System.Drawing.Point(84, 465);
			this.HqlRadioButton.Name = "HqlRadioButton";
			this.HqlRadioButton.Size = new System.Drawing.Size(110, 17);
			this.HqlRadioButton.TabIndex = 2;
			this.HqlRadioButton.Text = "HQL (NHibernate)";
			this.HqlRadioButton.UseVisualStyleBackColor = true;
			// 
			// QueryButton
			// 
			this.QueryButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.QueryButton.Location = new System.Drawing.Point(3, 451);
			this.QueryButton.Name = "QueryButton";
			this.QueryButton.Size = new System.Drawing.Size(75, 23);
			this.QueryButton.TabIndex = 1;
			this.QueryButton.Text = "Query";
			this.QueryButton.UseVisualStyleBackColor = true;
			this.QueryButton.Click += new System.EventHandler(this.QueryButton_Click);
			// 
			// QueryEditor
			// 
			this.QueryEditor.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.QueryEditor.Location = new System.Drawing.Point(0, 0);
			this.QueryEditor.Name = "QueryEditor";
			this.QueryEditor.Size = new System.Drawing.Size(297, 436);
			this.QueryEditor.Styles.BraceBad.FontName = "Verdana";
			this.QueryEditor.Styles.BraceLight.FontName = "Verdana";
			this.QueryEditor.Styles.ControlChar.FontName = "Verdana";
			this.QueryEditor.Styles.Default.FontName = "Verdana";
			this.QueryEditor.Styles.IndentGuide.FontName = "Verdana";
			this.QueryEditor.Styles.LastPredefined.FontName = "Verdana";
			this.QueryEditor.Styles.LineNumber.FontName = "Verdana";
			this.QueryEditor.Styles.Max.FontName = "Verdana";
			this.QueryEditor.TabIndex = 0;
			// 
			// DataViewer
			// 
			this.DataViewer.AllowUserToAddRows = false;
			this.DataViewer.AllowUserToDeleteRows = false;
			this.DataViewer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataViewer.Location = new System.Drawing.Point(0, 0);
			this.DataViewer.Name = "DataViewer";
			this.DataViewer.ReadOnly = true;
			this.DataViewer.Size = new System.Drawing.Size(498, 485);
			this.DataViewer.TabIndex = 0;
			// 
			// QueryDebugger
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Splitter);
			this.Name = "QueryDebugger";
			this.Size = new System.Drawing.Size(801, 485);
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel1.PerformLayout();
			this.Splitter.Panel2.ResumeLayout(false);
			this.Splitter.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) (this.QueryEditor)).EndInit();
			((System.ComponentModel.ISupportInitialize) (this.DataViewer)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer Splitter;
		private ScintillaNet.Scintilla QueryEditor;
		private System.Windows.Forms.DataGridView DataViewer;
		private System.Windows.Forms.RadioButton SqlRadioButton;
		private System.Windows.Forms.RadioButton HqlRadioButton;
		private System.Windows.Forms.Button QueryButton;
	}
}
