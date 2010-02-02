namespace SlimTuneUI
{
	partial class SlimTune
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
			this.RunButton = new System.Windows.Forms.Button();
			this.ConnectButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// RunButton
			// 
			this.RunButton.Location = new System.Drawing.Point(93, 68);
			this.RunButton.Name = "RunButton";
			this.RunButton.Size = new System.Drawing.Size(75, 23);
			this.RunButton.TabIndex = 0;
			this.RunButton.Text = "Run...";
			this.RunButton.UseVisualStyleBackColor = true;
			this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
			// 
			// ConnectButton
			// 
			this.ConnectButton.Location = new System.Drawing.Point(93, 155);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(75, 23);
			this.ConnectButton.TabIndex = 1;
			this.ConnectButton.Text = "Connect...";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
			// 
			// SlimTune
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(656, 324);
			this.Controls.Add(this.ConnectButton);
			this.Controls.Add(this.RunButton);
			this.Name = "SlimTune";
			this.Text = "SlimTune";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SlimTune_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SlimTune_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button RunButton;
		private System.Windows.Forms.Button ConnectButton;
	}
}