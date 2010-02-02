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
			this.VisualizerHost = new System.Windows.Forms.TabControl();
			this.SuspendLayout();
			// 
			// VisualizerHost
			// 
			this.VisualizerHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisualizerHost.Location = new System.Drawing.Point(0, 0);
			this.VisualizerHost.Name = "VisualizerHost";
			this.VisualizerHost.SelectedIndex = 0;
			this.VisualizerHost.Size = new System.Drawing.Size(799, 507);
			this.VisualizerHost.TabIndex = 0;
			// 
			// ProfilerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(799, 507);
			this.Controls.Add(this.VisualizerHost);
			this.Name = "ProfilerWindow";
			this.Text = "ProfilerWindow";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProfilerWindow_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfilerWindow_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.TabControl VisualizerHost;
	}
}