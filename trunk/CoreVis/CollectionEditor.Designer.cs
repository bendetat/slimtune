namespace SlimTuneUI.CoreVis
{
	partial class CollectionEditor
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
			this.ItemsList = new System.Windows.Forms.ListBox();
			this.AddButton = new System.Windows.Forms.Button();
			this.RemoveButton = new System.Windows.Forms.Button();
			this.CancelEditingButton = new System.Windows.Forms.Button();
			this.SaveButton = new System.Windows.Forms.Button();
			this.DownButton = new System.Windows.Forms.Button();
			this.UpButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ItemsList
			// 
			this.ItemsList.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemsList.FormattingEnabled = true;
			this.ItemsList.IntegralHeight = false;
			this.ItemsList.Location = new System.Drawing.Point(12, 12);
			this.ItemsList.Name = "ItemsList";
			this.ItemsList.Size = new System.Drawing.Size(341, 173);
			this.ItemsList.TabIndex = 0;
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.Location = new System.Drawing.Point(363, 12);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = new System.Drawing.Size(56, 22);
			this.AddButton.TabIndex = 1;
			this.AddButton.Text = "Add";
			this.AddButton.UseVisualStyleBackColor = true;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveButton.Location = new System.Drawing.Point(363, 40);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = new System.Drawing.Size(56, 22);
			this.RemoveButton.TabIndex = 2;
			this.RemoveButton.Text = "Remove";
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// CancelEditingButton
			// 
			this.CancelEditingButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CancelEditingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelEditingButton.Location = new System.Drawing.Point(93, 191);
			this.CancelEditingButton.Name = "CancelEditingButton";
			this.CancelEditingButton.Size = new System.Drawing.Size(75, 23);
			this.CancelEditingButton.TabIndex = 3;
			this.CancelEditingButton.Text = "Cancel";
			this.CancelEditingButton.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = new System.Drawing.Point(12, 191);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 4;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DownButton.Location = new System.Drawing.Point(363, 163);
			this.DownButton.Name = "DownButton";
			this.DownButton.Size = new System.Drawing.Size(56, 22);
			this.DownButton.TabIndex = 5;
			this.DownButton.Text = "Down";
			this.DownButton.UseVisualStyleBackColor = true;
			this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// UpButton
			// 
			this.UpButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpButton.Location = new System.Drawing.Point(363, 135);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = new System.Drawing.Size(56, 22);
			this.UpButton.TabIndex = 6;
			this.UpButton.Text = "Up";
			this.UpButton.UseVisualStyleBackColor = true;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// CollectionEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(427, 219);
			this.Controls.Add(this.UpButton);
			this.Controls.Add(this.DownButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CancelEditingButton);
			this.Controls.Add(this.RemoveButton);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.ItemsList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CollectionEditor";
			this.ShowInTaskbar = false;
			this.Text = "Edit";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox ItemsList;
		private System.Windows.Forms.Button AddButton;
		private System.Windows.Forms.Button RemoveButton;
		private System.Windows.Forms.Button CancelEditingButton;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Button DownButton;
		private System.Windows.Forms.Button UpButton;
	}
}