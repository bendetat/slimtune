/*
* Copyright (c) 2007-2009 SlimDX Group
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
	partial class SqlVisualizer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlVisualizer));
			this.m_queryButton = new System.Windows.Forms.Button();
			this.m_dataGrid = new System.Windows.Forms.DataGridView();
			this.m_queryTextBox = new System.Windows.Forms.TextBox();
			this.m_clearDataButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize) (this.m_dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// m_queryButton
			// 
			this.m_queryButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_queryButton.Location = new System.Drawing.Point(12, 448);
			this.m_queryButton.Name = "m_queryButton";
			this.m_queryButton.Size = new System.Drawing.Size(75, 23);
			this.m_queryButton.TabIndex = 0;
			this.m_queryButton.Text = "Query";
			this.m_queryButton.UseVisualStyleBackColor = true;
			this.m_queryButton.Click += new System.EventHandler(this.m_queryButton_Click);
			// 
			// m_dataGrid
			// 
			this.m_dataGrid.AllowUserToAddRows = false;
			this.m_dataGrid.AllowUserToDeleteRows = false;
			this.m_dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGrid.Location = new System.Drawing.Point(424, 13);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.ReadOnly = true;
			this.m_dataGrid.Size = new System.Drawing.Size(385, 458);
			this.m_dataGrid.TabIndex = 2;
			// 
			// m_queryTextBox
			// 
			this.m_queryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.m_queryTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_queryTextBox.Location = new System.Drawing.Point(12, 12);
			this.m_queryTextBox.Multiline = true;
			this.m_queryTextBox.Name = "m_queryTextBox";
			this.m_queryTextBox.Size = new System.Drawing.Size(406, 429);
			this.m_queryTextBox.TabIndex = 3;
			this.m_queryTextBox.Text = "SELECT * FROM Functions";
			// 
			// m_clearDataButton
			// 
			this.m_clearDataButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_clearDataButton.Location = new System.Drawing.Point(343, 448);
			this.m_clearDataButton.Name = "m_clearDataButton";
			this.m_clearDataButton.Size = new System.Drawing.Size(75, 23);
			this.m_clearDataButton.TabIndex = 4;
			this.m_clearDataButton.Text = "Clear Data";
			this.m_clearDataButton.UseVisualStyleBackColor = true;
			this.m_clearDataButton.Click += new System.EventHandler(this.m_clearDataButton_Click);
			// 
			// SqlVisualizer
			// 
			this.ClientSize = new System.Drawing.Size(821, 483);
			this.Controls.Add(this.m_clearDataButton);
			this.Controls.Add(this.m_queryTextBox);
			this.Controls.Add(this.m_dataGrid);
			this.Controls.Add(this.m_queryButton);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.Name = "SqlVisualizer";
			this.Text = "Profiling Run";
			((System.ComponentModel.ISupportInitialize) (this.m_dataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_queryButton;
		private System.Windows.Forms.DataGridView m_dataGrid;
		private System.Windows.Forms.TextBox m_queryTextBox;
		private System.Windows.Forms.Button m_clearDataButton;
	}
}
