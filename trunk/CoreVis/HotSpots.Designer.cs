namespace SlimTuneUI.CoreVis
{
	partial class HotSpots
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
			this.ScrollPanel = new System.Windows.Forms.Panel();
			this.HotspotsList = new System.Windows.Forms.ListBox();
			this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
			this.ScrollPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ScrollPanel
			// 
			this.ScrollPanel.AutoScroll = true;
			this.ScrollPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ScrollPanel.Controls.Add(this.HotspotsList);
			this.ScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScrollPanel.Location = new System.Drawing.Point(0, 0);
			this.ScrollPanel.Name = "ScrollPanel";
			this.ScrollPanel.Size = new System.Drawing.Size(710, 463);
			this.ScrollPanel.TabIndex = 0;
			// 
			// HotspotsList
			// 
			this.HotspotsList.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.HotspotsList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.HotspotsList.FormattingEnabled = true;
			this.HotspotsList.IntegralHeight = false;
			this.HotspotsList.ItemHeight = 40;
			this.HotspotsList.Location = new System.Drawing.Point(4, 4);
			this.HotspotsList.Name = "HotspotsList";
			this.HotspotsList.Size = new System.Drawing.Size(279, 446);
			this.HotspotsList.TabIndex = 0;
			this.HotspotsList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.CallList_DrawItem);
			this.HotspotsList.SelectedIndexChanged += new System.EventHandler(this.CallList_SelectedIndexChanged);
			this.HotspotsList.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.CallList_Format);
			// 
			// RefreshTimer
			// 
			this.RefreshTimer.Interval = 5000;
			this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
			// 
			// HotSpots
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ScrollPanel);
			this.Name = "HotSpots";
			this.Size = new System.Drawing.Size(710, 463);
			this.ScrollPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel ScrollPanel;
		private System.Windows.Forms.ListBox HotspotsList;
		private System.Windows.Forms.Timer RefreshTimer;
	}
}
