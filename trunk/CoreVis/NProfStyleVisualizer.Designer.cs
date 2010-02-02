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
	partial class NProfStyleVisualizer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NProfStyleVisualizer));
			this.m_splitter = new System.Windows.Forms.SplitContainer();
			this.m_callees = new Aga.Controls.Tree.TreeViewAdv();
			this.m_calleesIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_calleesThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_calleesNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_calleesPercentParentColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_calleesPercentCallsColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_calleesIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_calleesThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_calleesNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_calleesPercentParentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_calleesPercentCallsTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callers = new Aga.Controls.Tree.TreeViewAdv();
			this.m_callersIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersPercentTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersPercentCallsColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersPercentTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_refreshButton = new System.Windows.Forms.Button();
			this.m_callersPercentCallsTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_splitter.Panel1.SuspendLayout();
			this.m_splitter.Panel2.SuspendLayout();
			this.m_splitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_splitter
			// 
			this.m_splitter.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.m_splitter.Location = new System.Drawing.Point(0, 26);
			this.m_splitter.Name = "m_splitter";
			this.m_splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// m_splitter.Panel1
			// 
			this.m_splitter.Panel1.Controls.Add(this.m_callees);
			// 
			// m_splitter.Panel2
			// 
			this.m_splitter.Panel2.Controls.Add(this.m_callers);
			this.m_splitter.Size = new System.Drawing.Size(884, 569);
			this.m_splitter.SplitterDistance = 268;
			this.m_splitter.TabIndex = 0;
			// 
			// m_callees
			// 
			this.m_callees.BackColor = System.Drawing.SystemColors.Window;
			this.m_callees.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_callees.Columns.Add(this.m_calleesIdColumn);
			this.m_callees.Columns.Add(this.m_calleesThreadIdColumn);
			this.m_callees.Columns.Add(this.m_calleesNameColumn);
			this.m_callees.Columns.Add(this.m_calleesPercentParentColumn);
			this.m_callees.Columns.Add(this.m_calleesPercentCallsColumn);
			this.m_callees.DefaultToolTipProvider = null;
			this.m_callees.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_callees.DragDropMarkColor = System.Drawing.Color.Black;
			this.m_callees.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle) ((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.m_callees.LineColor = System.Drawing.SystemColors.ControlDark;
			this.m_callees.LoadOnDemand = true;
			this.m_callees.Location = new System.Drawing.Point(0, 0);
			this.m_callees.Model = null;
			this.m_callees.Name = "m_callees";
			this.m_callees.NodeControls.Add(this.m_calleesIdTextBox);
			this.m_callees.NodeControls.Add(this.m_calleesThreadTextBox);
			this.m_callees.NodeControls.Add(this.m_calleesNameTextBox);
			this.m_callees.NodeControls.Add(this.m_calleesPercentParentTextBox);
			this.m_callees.NodeControls.Add(this.m_calleesPercentCallsTextBox);
			this.m_callees.SelectedNode = null;
			this.m_callees.Size = new System.Drawing.Size(884, 268);
			this.m_callees.TabIndex = 0;
			this.m_callees.UnloadCollapsedOnReload = true;
			this.m_callees.UseColumns = true;
			this.m_callees.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this.ColumnClicked);
			// 
			// m_calleesIdColumn
			// 
			this.m_calleesIdColumn.Header = "Id";
			this.m_calleesIdColumn.MinColumnWidth = 100;
			this.m_calleesIdColumn.Sortable = true;
			this.m_calleesIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_calleesIdColumn.TooltipText = null;
			this.m_calleesIdColumn.Width = 200;
			// 
			// m_calleesThreadIdColumn
			// 
			this.m_calleesThreadIdColumn.Header = "Thread";
			this.m_calleesThreadIdColumn.MinColumnWidth = 50;
			this.m_calleesThreadIdColumn.Sortable = true;
			this.m_calleesThreadIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_calleesThreadIdColumn.TooltipText = null;
			this.m_calleesThreadIdColumn.Width = 80;
			// 
			// m_calleesNameColumn
			// 
			this.m_calleesNameColumn.Header = "Callees";
			this.m_calleesNameColumn.MinColumnWidth = 200;
			this.m_calleesNameColumn.Sortable = true;
			this.m_calleesNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_calleesNameColumn.TooltipText = null;
			this.m_calleesNameColumn.Width = 420;
			// 
			// m_calleesPercentParentColumn
			// 
			this.m_calleesPercentParentColumn.Header = "% of Parent";
			this.m_calleesPercentParentColumn.MinColumnWidth = 60;
			this.m_calleesPercentParentColumn.Sortable = true;
			this.m_calleesPercentParentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_calleesPercentParentColumn.TooltipText = null;
			this.m_calleesPercentParentColumn.Width = 90;
			// 
			// m_calleesPercentCallsColumn
			// 
			this.m_calleesPercentCallsColumn.Header = "% Calls";
			this.m_calleesPercentCallsColumn.MinColumnWidth = 60;
			this.m_calleesPercentCallsColumn.Sortable = true;
			this.m_calleesPercentCallsColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_calleesPercentCallsColumn.TooltipText = null;
			this.m_calleesPercentCallsColumn.Width = 90;
			// 
			// m_calleesIdTextBox
			// 
			this.m_calleesIdTextBox.DataPropertyName = "Id";
			this.m_calleesIdTextBox.IncrementalSearchEnabled = true;
			this.m_calleesIdTextBox.LeftMargin = 3;
			this.m_calleesIdTextBox.ParentColumn = this.m_calleesIdColumn;
			// 
			// m_calleesThreadTextBox
			// 
			this.m_calleesThreadTextBox.DataPropertyName = "Thread";
			this.m_calleesThreadTextBox.IncrementalSearchEnabled = true;
			this.m_calleesThreadTextBox.LeftMargin = 3;
			this.m_calleesThreadTextBox.ParentColumn = this.m_calleesThreadIdColumn;
			// 
			// m_calleesNameTextBox
			// 
			this.m_calleesNameTextBox.DataPropertyName = "Name";
			this.m_calleesNameTextBox.IncrementalSearchEnabled = true;
			this.m_calleesNameTextBox.LeftMargin = 3;
			this.m_calleesNameTextBox.ParentColumn = this.m_calleesNameColumn;
			// 
			// m_calleesPercentParentTextBox
			// 
			this.m_calleesPercentParentTextBox.DataPropertyName = "PercentTime";
			this.m_calleesPercentParentTextBox.IncrementalSearchEnabled = true;
			this.m_calleesPercentParentTextBox.LeftMargin = 3;
			this.m_calleesPercentParentTextBox.ParentColumn = this.m_calleesPercentParentColumn;
			// 
			// m_calleesPercentCallsTextBox
			// 
			this.m_calleesPercentCallsTextBox.DataPropertyName = "PercentCalls";
			this.m_calleesPercentCallsTextBox.IncrementalSearchEnabled = true;
			this.m_calleesPercentCallsTextBox.LeftMargin = 3;
			this.m_calleesPercentCallsTextBox.ParentColumn = this.m_calleesPercentCallsColumn;
			// 
			// m_callers
			// 
			this.m_callers.BackColor = System.Drawing.SystemColors.Window;
			this.m_callers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_callers.Columns.Add(this.m_callersIdColumn);
			this.m_callers.Columns.Add(this.m_callersThreadIdColumn);
			this.m_callers.Columns.Add(this.m_callersNameColumn);
			this.m_callers.Columns.Add(this.m_callersPercentTimeColumn);
			this.m_callers.Columns.Add(this.m_callersPercentCallsColumn);
			this.m_callers.DefaultToolTipProvider = null;
			this.m_callers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_callers.DragDropMarkColor = System.Drawing.Color.Black;
			this.m_callers.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle) ((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.m_callers.LineColor = System.Drawing.SystemColors.ControlDark;
			this.m_callers.LoadOnDemand = true;
			this.m_callers.Location = new System.Drawing.Point(0, 0);
			this.m_callers.Model = null;
			this.m_callers.Name = "m_callers";
			this.m_callers.NodeControls.Add(this.m_callersIdTextBox);
			this.m_callers.NodeControls.Add(this.m_callersThreadTextBox);
			this.m_callers.NodeControls.Add(this.m_callersNameTextBox);
			this.m_callers.NodeControls.Add(this.m_callersPercentTimeTextBox);
			this.m_callers.NodeControls.Add(this.m_callersPercentCallsTextBox);
			this.m_callers.SelectedNode = null;
			this.m_callers.Size = new System.Drawing.Size(884, 297);
			this.m_callers.TabIndex = 1;
			this.m_callers.UseColumns = true;
			this.m_callers.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this.ColumnClicked);
			// 
			// m_callersIdColumn
			// 
			this.m_callersIdColumn.Header = "Id";
			this.m_callersIdColumn.MinColumnWidth = 100;
			this.m_callersIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersIdColumn.TooltipText = null;
			this.m_callersIdColumn.Width = 200;
			// 
			// m_callersThreadIdColumn
			// 
			this.m_callersThreadIdColumn.Header = "Thread";
			this.m_callersThreadIdColumn.MinColumnWidth = 50;
			this.m_callersThreadIdColumn.Sortable = true;
			this.m_callersThreadIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersThreadIdColumn.TooltipText = null;
			this.m_callersThreadIdColumn.Width = 80;
			// 
			// m_callersNameColumn
			// 
			this.m_callersNameColumn.Header = "Callers";
			this.m_callersNameColumn.MinColumnWidth = 200;
			this.m_callersNameColumn.Sortable = true;
			this.m_callersNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersNameColumn.TooltipText = null;
			this.m_callersNameColumn.Width = 420;
			// 
			// m_callersPercentTimeColumn
			// 
			this.m_callersPercentTimeColumn.Header = "% Time";
			this.m_callersPercentTimeColumn.MinColumnWidth = 50;
			this.m_callersPercentTimeColumn.Sortable = true;
			this.m_callersPercentTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersPercentTimeColumn.TooltipText = null;
			this.m_callersPercentTimeColumn.Width = 90;
			// 
			// m_callersPercentCallsColumn
			// 
			this.m_callersPercentCallsColumn.Header = "% Calls";
			this.m_callersPercentCallsColumn.MinColumnWidth = 50;
			this.m_callersPercentCallsColumn.Sortable = true;
			this.m_callersPercentCallsColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersPercentCallsColumn.TooltipText = null;
			this.m_callersPercentCallsColumn.Width = 90;
			// 
			// m_callersIdTextBox
			// 
			this.m_callersIdTextBox.DataPropertyName = "Id";
			this.m_callersIdTextBox.IncrementalSearchEnabled = true;
			this.m_callersIdTextBox.LeftMargin = 3;
			this.m_callersIdTextBox.ParentColumn = this.m_callersIdColumn;
			// 
			// m_callersThreadTextBox
			// 
			this.m_callersThreadTextBox.DataPropertyName = "Thread";
			this.m_callersThreadTextBox.IncrementalSearchEnabled = true;
			this.m_callersThreadTextBox.LeftMargin = 3;
			this.m_callersThreadTextBox.ParentColumn = this.m_callersThreadIdColumn;
			// 
			// m_callersNameTextBox
			// 
			this.m_callersNameTextBox.DataPropertyName = "Name";
			this.m_callersNameTextBox.IncrementalSearchEnabled = true;
			this.m_callersNameTextBox.LeftMargin = 3;
			this.m_callersNameTextBox.ParentColumn = this.m_callersNameColumn;
			// 
			// m_callersPercentTimeTextBox
			// 
			this.m_callersPercentTimeTextBox.DataPropertyName = "PercentTime";
			this.m_callersPercentTimeTextBox.IncrementalSearchEnabled = true;
			this.m_callersPercentTimeTextBox.LeftMargin = 3;
			this.m_callersPercentTimeTextBox.ParentColumn = this.m_callersPercentTimeColumn;
			// 
			// m_refreshButton
			// 
			this.m_refreshButton.Image = ((System.Drawing.Image) (resources.GetObject("m_refreshButton.Image")));
			this.m_refreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_refreshButton.Location = new System.Drawing.Point(0, 2);
			this.m_refreshButton.Name = "m_refreshButton";
			this.m_refreshButton.Size = new System.Drawing.Size(75, 23);
			this.m_refreshButton.TabIndex = 1;
			this.m_refreshButton.Text = "Refresh";
			this.m_refreshButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.m_refreshButton.UseVisualStyleBackColor = true;
			this.m_refreshButton.Click += new System.EventHandler(this.m_refreshButton_Click);
			// 
			// m_callersPercentCallsTextBox
			// 
			this.m_callersPercentCallsTextBox.DataPropertyName = "PercentCalls";
			this.m_callersPercentCallsTextBox.IncrementalSearchEnabled = true;
			this.m_callersPercentCallsTextBox.LeftMargin = 3;
			this.m_callersPercentCallsTextBox.ParentColumn = this.m_callersPercentCallsColumn;
			// 
			// NProfStyleVisualizer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(884, 595);
			this.Controls.Add(this.m_refreshButton);
			this.Controls.Add(this.m_splitter);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.Name = "NProfStyleVisualizer";
			this.Text = "NProfStyleVisualizer";
			this.m_splitter.Panel1.ResumeLayout(false);
			this.m_splitter.Panel2.ResumeLayout(false);
			this.m_splitter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer m_splitter;
		private Aga.Controls.Tree.TreeViewAdv m_callees;
		private Aga.Controls.Tree.TreeViewAdv m_callers;
		private Aga.Controls.Tree.TreeColumn m_calleesIdColumn;
		private Aga.Controls.Tree.TreeColumn m_calleesThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_calleesNameColumn;
		private Aga.Controls.Tree.TreeColumn m_calleesPercentParentColumn;
		private Aga.Controls.Tree.TreeColumn m_callersIdColumn;
		private Aga.Controls.Tree.TreeColumn m_callersThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_callersNameColumn;
		private Aga.Controls.Tree.TreeColumn m_callersPercentTimeColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_calleesIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_calleesThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_calleesNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_calleesPercentParentTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersPercentTimeTextBox;
		private System.Windows.Forms.Button m_refreshButton;
		private Aga.Controls.Tree.TreeColumn m_calleesPercentCallsColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_calleesPercentCallsTextBox;
		private Aga.Controls.Tree.TreeColumn m_callersPercentCallsColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersPercentCallsTextBox;
	}
}