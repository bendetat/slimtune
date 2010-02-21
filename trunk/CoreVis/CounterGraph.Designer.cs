namespace SlimTuneUI.CoreVis
{
	partial class CounterGraph
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
			this.Graph = new ZedGraph.ZedGraphControl();
			this.m_refreshTimer = new System.Windows.Forms.Timer(this.components);
			this.m_counterListBox = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// Graph
			// 
			this.Graph.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.Graph.Location = new System.Drawing.Point(206, 0);
			this.Graph.Name = "Graph";
			this.Graph.ScrollGrace = 0;
			this.Graph.ScrollMaxX = 0;
			this.Graph.ScrollMaxY = 0;
			this.Graph.ScrollMaxY2 = 0;
			this.Graph.ScrollMinX = 0;
			this.Graph.ScrollMinY = 0;
			this.Graph.ScrollMinY2 = 0;
			this.Graph.Size = new System.Drawing.Size(558, 432);
			this.Graph.TabIndex = 0;
			// 
			// m_refreshTimer
			// 
			this.m_refreshTimer.Interval = 1000;
			this.m_refreshTimer.Tick += new System.EventHandler(this.m_refreshTimer_Tick);
			// 
			// m_counterListBox
			// 
			this.m_counterListBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.m_counterListBox.CheckOnClick = true;
			this.m_counterListBox.FormattingEnabled = true;
			this.m_counterListBox.IntegralHeight = false;
			this.m_counterListBox.Location = new System.Drawing.Point(0, 0);
			this.m_counterListBox.Name = "m_counterListBox";
			this.m_counterListBox.Size = new System.Drawing.Size(200, 432);
			this.m_counterListBox.TabIndex = 1;
			this.m_counterListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.m_counterListBox_ItemCheck);
			// 
			// CounterGraph
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_counterListBox);
			this.Controls.Add(this.Graph);
			this.Name = "CounterGraph";
			this.Size = new System.Drawing.Size(764, 432);
			this.ResumeLayout(false);

		}

		#endregion

		private ZedGraph.ZedGraphControl Graph;
		private System.Windows.Forms.Timer m_refreshTimer;
		private System.Windows.Forms.CheckedListBox m_counterListBox;
	}
}
