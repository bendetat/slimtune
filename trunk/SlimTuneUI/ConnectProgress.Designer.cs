/*
* Copyright (c) 2007-2010 SlimDX Group
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
	partial class ConnectProgress
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectProgress));
			this.m_progress = new System.Windows.Forms.ProgressBar();
			this.m_connectingToLabel = new System.Windows.Forms.Label();
			this.m_attemptsLabel = new System.Windows.Forms.Label();
			this.m_statusLabel = new System.Windows.Forms.Label();
			this.m_cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// m_progress
			// 
			this.m_progress.Location = new System.Drawing.Point(12, 13);
			this.m_progress.Name = "m_progress";
			this.m_progress.Size = new System.Drawing.Size(456, 23);
			this.m_progress.Step = 1;
			this.m_progress.TabIndex = 0;
			// 
			// m_connectingToLabel
			// 
			this.m_connectingToLabel.AutoSize = true;
			this.m_connectingToLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_connectingToLabel.Location = new System.Drawing.Point(12, 39);
			this.m_connectingToLabel.Name = "m_connectingToLabel";
			this.m_connectingToLabel.Size = new System.Drawing.Size(104, 18);
			this.m_connectingToLabel.TabIndex = 1;
			this.m_connectingToLabel.Text = "Connecting to:";
			this.m_connectingToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_attemptsLabel
			// 
			this.m_attemptsLabel.AutoSize = true;
			this.m_attemptsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_attemptsLabel.Location = new System.Drawing.Point(12, 57);
			this.m_attemptsLabel.Name = "m_attemptsLabel";
			this.m_attemptsLabel.Size = new System.Drawing.Size(132, 18);
			this.m_attemptsLabel.TabIndex = 2;
			this.m_attemptsLabel.Text = "Attempt 1 out of 10";
			// 
			// m_statusLabel
			// 
			this.m_statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_statusLabel.Location = new System.Drawing.Point(12, 75);
			this.m_statusLabel.Name = "m_statusLabel";
			this.m_statusLabel.Size = new System.Drawing.Size(456, 56);
			this.m_statusLabel.TabIndex = 3;
			this.m_statusLabel.Text = "Status: Attempting connection...";
			// 
			// m_cancelButton
			// 
			this.m_cancelButton.Image = ((System.Drawing.Image) (resources.GetObject("m_cancelButton.Image")));
			this.m_cancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_cancelButton.Location = new System.Drawing.Point(205, 134);
			this.m_cancelButton.Name = "m_cancelButton";
			this.m_cancelButton.Size = new System.Drawing.Size(70, 23);
			this.m_cancelButton.TabIndex = 4;
			this.m_cancelButton.Text = "Cancel";
			this.m_cancelButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.m_cancelButton.UseVisualStyleBackColor = true;
			this.m_cancelButton.Click += new System.EventHandler(this.m_cancelButton_Click);
			// 
			// ConnectProgress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(480, 166);
			this.ControlBox = false;
			this.Controls.Add(this.m_cancelButton);
			this.Controls.Add(this.m_statusLabel);
			this.Controls.Add(this.m_attemptsLabel);
			this.Controls.Add(this.m_connectingToLabel);
			this.Controls.Add(this.m_progress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ConnectProgress";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Connecting to Target";
			this.Shown += new System.EventHandler(this.ConnectProgress_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar m_progress;
		private System.Windows.Forms.Label m_connectingToLabel;
		private System.Windows.Forms.Label m_attemptsLabel;
		private System.Windows.Forms.Label m_statusLabel;
		private System.Windows.Forms.Button m_cancelButton;
	}
}