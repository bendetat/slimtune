namespace SlimTuneUI.CoreVis
{
	partial class FunctionDetails
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
			this.components = new System.ComponentModel.Container();
			this.Splitter = new System.Windows.Forms.SplitContainer();
			this.FunctionList = new System.Windows.Forms.ListBox();
			this.SearchBox = new System.Windows.Forms.TextBox();
			this.DetailsGraph = new ZedGraph.ZedGraphControl();
			this.m_refreshTimer = new System.Windows.Forms.Timer(this.components);
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Splitter.Location = new System.Drawing.Point(0, 0);
			this.Splitter.Name = "Splitter";
			// 
			// Splitter.Panel1
			// 
			this.Splitter.Panel1.Controls.Add(this.FunctionList);
			this.Splitter.Panel1.Controls.Add(this.SearchBox);
			// 
			// Splitter.Panel2
			// 
			this.Splitter.Panel2.Controls.Add(this.DetailsGraph);
			this.Splitter.Size = new System.Drawing.Size(812, 483);
			this.Splitter.SplitterDistance = 248;
			this.Splitter.TabIndex = 0;
			// 
			// FunctionList
			// 
			this.FunctionList.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FunctionList.DisplayMember = "Name";
			this.FunctionList.FormattingEnabled = true;
			this.FunctionList.IntegralHeight = false;
			this.FunctionList.Location = new System.Drawing.Point(0, 26);
			this.FunctionList.Name = "FunctionList";
			this.FunctionList.Size = new System.Drawing.Size(248, 457);
			this.FunctionList.TabIndex = 1;
			this.FunctionList.SelectedIndexChanged += new System.EventHandler(this.FunctionList_SelectedIndexChanged);
			// 
			// SearchBox
			// 
			this.SearchBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SearchBox.Location = new System.Drawing.Point(0, 0);
			this.SearchBox.Name = "SearchBox";
			this.SearchBox.Size = new System.Drawing.Size(248, 20);
			this.SearchBox.TabIndex = 0;
			this.SearchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
			// 
			// DetailsGraph
			// 
			this.DetailsGraph.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGraph.IsEnableHPan = false;
			this.DetailsGraph.IsEnableHZoom = false;
			this.DetailsGraph.IsEnableVPan = false;
			this.DetailsGraph.IsEnableVZoom = false;
			this.DetailsGraph.IsEnableWheelZoom = false;
			this.DetailsGraph.IsShowPointValues = true;
			this.DetailsGraph.Location = new System.Drawing.Point(0, 0);
			this.DetailsGraph.Name = "DetailsGraph";
			this.DetailsGraph.ScrollGrace = 0;
			this.DetailsGraph.ScrollMaxX = 0;
			this.DetailsGraph.ScrollMaxY = 0;
			this.DetailsGraph.ScrollMaxY2 = 0;
			this.DetailsGraph.ScrollMinX = 0;
			this.DetailsGraph.ScrollMinY = 0;
			this.DetailsGraph.ScrollMinY2 = 0;
			this.DetailsGraph.Size = new System.Drawing.Size(560, 483);
			this.DetailsGraph.TabIndex = 0;
			// 
			// m_refreshTimer
			// 
			this.m_refreshTimer.Enabled = true;
			this.m_refreshTimer.Interval = 2000;
			this.m_refreshTimer.Tick += new System.EventHandler(this.m_refreshTimer_Tick);
			// 
			// FunctionDetails
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Splitter);
			this.Name = "FunctionDetails";
			this.Size = new System.Drawing.Size(812, 483);
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel1.PerformLayout();
			this.Splitter.Panel2.ResumeLayout(false);
			this.Splitter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer Splitter;
		private System.Windows.Forms.ListBox FunctionList;
		private System.Windows.Forms.TextBox SearchBox;
		private ZedGraph.ZedGraphControl DetailsGraph;
		private System.Windows.Forms.Timer m_refreshTimer;
	}
}
