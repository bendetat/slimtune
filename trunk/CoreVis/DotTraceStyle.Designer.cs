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
namespace SlimTuneUI.CoreVis
{
	partial class DotTraceStyle
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DotTraceStyle));
			this.m_treeView = new System.Windows.Forms.TreeView();
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_refreshButton = new System.Windows.Forms.ToolStripButton();
			this.m_filterButton = new System.Windows.Forms.ToolStripSplitButton();
			this.m_filterSystemMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.m_filterMicrosoftMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.SnapshotCombo = new System.Windows.Forms.ToolStripComboBox();
			this.m_functionsToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_extraInfoTextBox = new System.Windows.Forms.TextBox();
			this.m_toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_treeView
			// 
			this.m_treeView.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_treeView.BackColor = System.Drawing.SystemColors.Window;
			this.m_treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.m_treeView.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_treeView.FullRowSelect = true;
			this.m_treeView.Location = new System.Drawing.Point(0, 28);
			this.m_treeView.Name = "m_treeView";
			this.m_treeView.Size = new System.Drawing.Size(722, 429);
			this.m_treeView.TabIndex = 0;
			this.m_treeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.m_treeView_AfterCollapse);
			this.m_treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.m_treeView_BeforeExpand);
			this.m_treeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.m_treeView_DrawNode);
			this.m_treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.m_treeView_BeforeSelect);
			this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.m_treeView_AfterSelect);
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_refreshButton,
            this.m_filterButton,
            this.SnapshotCombo});
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Size = new System.Drawing.Size(722, 25);
			this.m_toolStrip.TabIndex = 1;
			this.m_toolStrip.Text = "toolStrip1";
			// 
			// m_refreshButton
			// 
			this.m_refreshButton.Image = ((System.Drawing.Image) (resources.GetObject("m_refreshButton.Image")));
			this.m_refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_refreshButton.Name = "m_refreshButton";
			this.m_refreshButton.Size = new System.Drawing.Size(66, 22);
			this.m_refreshButton.Text = "Refresh";
			this.m_refreshButton.ToolTipText = "Updates the view to show the latest data.";
			this.m_refreshButton.Click += new System.EventHandler(this.m_refreshButton_Click);
			// 
			// m_filterButton
			// 
			this.m_filterButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_filterSystemMenu,
            this.m_filterMicrosoftMenu});
			this.m_filterButton.Image = ((System.Drawing.Image) (resources.GetObject("m_filterButton.Image")));
			this.m_filterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_filterButton.Name = "m_filterButton";
			this.m_filterButton.Size = new System.Drawing.Size(65, 22);
			this.m_filterButton.Text = "Filter";
			this.m_filterButton.ToolTipText = "Clicking this WOULD open a Filters dialog, if I had written one.";
			// 
			// m_filterSystemMenu
			// 
			this.m_filterSystemMenu.Checked = true;
			this.m_filterSystemMenu.CheckOnClick = true;
			this.m_filterSystemMenu.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_filterSystemMenu.Name = "m_filterSystemMenu";
			this.m_filterSystemMenu.Size = new System.Drawing.Size(133, 22);
			this.m_filterSystemMenu.Text = "System.*";
			this.m_filterSystemMenu.ToolTipText = "Check this option to gray out System functions.";
			this.m_filterSystemMenu.Click += new System.EventHandler(this.FilterMenu_Click);
			// 
			// m_filterMicrosoftMenu
			// 
			this.m_filterMicrosoftMenu.Checked = true;
			this.m_filterMicrosoftMenu.CheckOnClick = true;
			this.m_filterMicrosoftMenu.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_filterMicrosoftMenu.Name = "m_filterMicrosoftMenu";
			this.m_filterMicrosoftMenu.Size = new System.Drawing.Size(133, 22);
			this.m_filterMicrosoftMenu.Text = "Microsoft.*";
			this.m_filterMicrosoftMenu.ToolTipText = "Check this option to gray out Microsoft functions.";
			this.m_filterMicrosoftMenu.Click += new System.EventHandler(this.FilterMenu_Click);
			// 
			// SnapshotCombo
			// 
			this.SnapshotCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SnapshotCombo.Items.AddRange(new object[] {
            "Current"});
			this.SnapshotCombo.Name = "SnapshotCombo";
			this.SnapshotCombo.Size = new System.Drawing.Size(121, 25);
			this.SnapshotCombo.Click += new System.EventHandler(this.SnapshotCombo_Click);
			// 
			// m_functionsToolTip
			// 
			this.m_functionsToolTip.AutoPopDelay = 5000;
			this.m_functionsToolTip.InitialDelay = 3000;
			this.m_functionsToolTip.IsBalloon = true;
			this.m_functionsToolTip.ReshowDelay = 100;
			this.m_functionsToolTip.ToolTipTitle = "Function Details";
			// 
			// m_extraInfoTextBox
			// 
			this.m_extraInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_extraInfoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.m_extraInfoTextBox.Location = new System.Drawing.Point(0, 457);
			this.m_extraInfoTextBox.Multiline = true;
			this.m_extraInfoTextBox.Name = "m_extraInfoTextBox";
			this.m_extraInfoTextBox.ReadOnly = true;
			this.m_extraInfoTextBox.Size = new System.Drawing.Size(722, 46);
			this.m_extraInfoTextBox.TabIndex = 2;
			// 
			// DotTraceStyle
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_extraInfoTextBox);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_treeView);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.Name = "DotTraceStyle";
			this.Size = new System.Drawing.Size(722, 503);
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView m_treeView;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripButton m_refreshButton;
		private System.Windows.Forms.ToolStripSplitButton m_filterButton;
		private System.Windows.Forms.ToolStripMenuItem m_filterSystemMenu;
		private System.Windows.Forms.ToolStripMenuItem m_filterMicrosoftMenu;
		private System.Windows.Forms.ToolTip m_functionsToolTip;
		private System.Windows.Forms.TextBox m_extraInfoTextBox;
		private System.Windows.Forms.ToolStripComboBox SnapshotCombo;
	}
}