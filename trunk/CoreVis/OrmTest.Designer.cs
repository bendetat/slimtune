namespace SlimTuneUI.CoreVis
{
	partial class OrmTest
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
			this.FunctionList = new System.Windows.Forms.ListBox();
			this.RefreshButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// FunctionList
			// 
			this.FunctionList.FormattingEnabled = true;
			this.FunctionList.Location = new System.Drawing.Point(4, 4);
			this.FunctionList.Name = "FunctionList";
			this.FunctionList.Size = new System.Drawing.Size(278, 407);
			this.FunctionList.TabIndex = 0;
			// 
			// RefreshButton
			// 
			this.RefreshButton.Location = new System.Drawing.Point(289, 4);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = new System.Drawing.Size(75, 23);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.Text = "Refresh";
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// OrmTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.RefreshButton);
			this.Controls.Add(this.FunctionList);
			this.Name = "OrmTest";
			this.Size = new System.Drawing.Size(610, 427);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox FunctionList;
		private System.Windows.Forms.Button RefreshButton;
	}
}
