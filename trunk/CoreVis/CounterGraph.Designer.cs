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
			this.CounterCombo = new System.Windows.Forms.ComboBox();
			this.RefreshButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Graph
			// 
			this.Graph.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.Graph.Location = new System.Drawing.Point(0, 32);
			this.Graph.Name = "Graph";
			this.Graph.ScrollGrace = 0;
			this.Graph.ScrollMaxX = 0;
			this.Graph.ScrollMaxY = 0;
			this.Graph.ScrollMaxY2 = 0;
			this.Graph.ScrollMinX = 0;
			this.Graph.ScrollMinY = 0;
			this.Graph.ScrollMinY2 = 0;
			this.Graph.Size = new System.Drawing.Size(582, 338);
			this.Graph.TabIndex = 0;
			// 
			// CounterCombo
			// 
			this.CounterCombo.DisplayMember = "Name";
			this.CounterCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CounterCombo.FormattingEnabled = true;
			this.CounterCombo.Location = new System.Drawing.Point(84, 5);
			this.CounterCombo.Name = "CounterCombo";
			this.CounterCombo.Size = new System.Drawing.Size(179, 21);
			this.CounterCombo.TabIndex = 1;
			this.CounterCombo.ValueMember = "Name";
			this.CounterCombo.SelectedIndexChanged += new System.EventHandler(this.CounterCombo_SelectedIndexChanged);
			// 
			// RefreshButton
			// 
			this.RefreshButton.Location = new System.Drawing.Point(3, 3);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = new System.Drawing.Size(75, 23);
			this.RefreshButton.TabIndex = 2;
			this.RefreshButton.Text = "Refresh";
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// CounterGraph
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.RefreshButton);
			this.Controls.Add(this.CounterCombo);
			this.Controls.Add(this.Graph);
			this.Name = "CounterGraph";
			this.Size = new System.Drawing.Size(582, 370);
			this.ResumeLayout(false);

		}

		#endregion

		private ZedGraph.ZedGraphControl Graph;
		private System.Windows.Forms.ComboBox CounterCombo;
		private System.Windows.Forms.Button RefreshButton;
	}
}
