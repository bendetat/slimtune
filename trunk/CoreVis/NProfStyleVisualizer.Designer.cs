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
			this.m_splitter = new System.Windows.Forms.SplitContainer();
			this.m_topDown = new Aga.Controls.Tree.TreeViewAdv();
			this.m_topDownIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_topDownThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_topDownNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_topDownTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_topDownPercentColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_topDownIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_topDownThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_topDownNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_topDownTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_topDownPercentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_bottomUp = new Aga.Controls.Tree.TreeViewAdv();
			this.m_bottomUpIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_bottomUpThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_bottomUpNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_bottomUpTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_bottomUpPercentColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_bottomUpIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_bottomUpThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_bottomUpNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_bottomUpTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_bottomUpPercentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_splitter.Panel1.SuspendLayout();
			this.m_splitter.Panel2.SuspendLayout();
			this.m_splitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_splitter
			// 
			this.m_splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_splitter.Location = new System.Drawing.Point(0, 0);
			this.m_splitter.Name = "m_splitter";
			this.m_splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// m_splitter.Panel1
			// 
			this.m_splitter.Panel1.Controls.Add(this.m_topDown);
			// 
			// m_splitter.Panel2
			// 
			this.m_splitter.Panel2.Controls.Add(this.m_bottomUp);
			this.m_splitter.Size = new System.Drawing.Size(884, 595);
			this.m_splitter.SplitterDistance = 280;
			this.m_splitter.TabIndex = 0;
			// 
			// m_topDown
			// 
			this.m_topDown.BackColor = System.Drawing.SystemColors.Window;
			this.m_topDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_topDown.Columns.Add(this.m_topDownIdColumn);
			this.m_topDown.Columns.Add(this.m_topDownThreadIdColumn);
			this.m_topDown.Columns.Add(this.m_topDownNameColumn);
			this.m_topDown.Columns.Add(this.m_topDownTimeColumn);
			this.m_topDown.Columns.Add(this.m_topDownPercentColumn);
			this.m_topDown.DefaultToolTipProvider = null;
			this.m_topDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_topDown.DragDropMarkColor = System.Drawing.Color.Black;
			this.m_topDown.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle) ((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.m_topDown.LineColor = System.Drawing.SystemColors.ControlDark;
			this.m_topDown.LoadOnDemand = true;
			this.m_topDown.Location = new System.Drawing.Point(0, 0);
			this.m_topDown.Model = null;
			this.m_topDown.Name = "m_topDown";
			this.m_topDown.NodeControls.Add(this.m_topDownIdTextBox);
			this.m_topDown.NodeControls.Add(this.m_topDownThreadTextBox);
			this.m_topDown.NodeControls.Add(this.m_topDownNameTextBox);
			this.m_topDown.NodeControls.Add(this.m_topDownTimeTextBox);
			this.m_topDown.NodeControls.Add(this.m_topDownPercentTextBox);
			this.m_topDown.SelectedNode = null;
			this.m_topDown.Size = new System.Drawing.Size(884, 280);
			this.m_topDown.TabIndex = 0;
			this.m_topDown.UnloadCollapsedOnReload = true;
			this.m_topDown.UseColumns = true;
			this.m_topDown.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this.ColumnClicked);
			// 
			// m_topDownIdColumn
			// 
			this.m_topDownIdColumn.Header = "Id";
			this.m_topDownIdColumn.MinColumnWidth = 100;
			this.m_topDownIdColumn.Sortable = true;
			this.m_topDownIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_topDownIdColumn.TooltipText = null;
			this.m_topDownIdColumn.Width = 200;
			// 
			// m_topDownThreadIdColumn
			// 
			this.m_topDownThreadIdColumn.Header = "Thread";
			this.m_topDownThreadIdColumn.MinColumnWidth = 50;
			this.m_topDownThreadIdColumn.Sortable = true;
			this.m_topDownThreadIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_topDownThreadIdColumn.TooltipText = null;
			this.m_topDownThreadIdColumn.Width = 80;
			// 
			// m_topDownNameColumn
			// 
			this.m_topDownNameColumn.Header = "Function Calls (top-down)";
			this.m_topDownNameColumn.MinColumnWidth = 200;
			this.m_topDownNameColumn.Sortable = true;
			this.m_topDownNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_topDownNameColumn.TooltipText = null;
			this.m_topDownNameColumn.Width = 420;
			// 
			// m_topDownTimeColumn
			// 
			this.m_topDownTimeColumn.Header = "Time (Inclusive)";
			this.m_topDownTimeColumn.MinColumnWidth = 60;
			this.m_topDownTimeColumn.Sortable = true;
			this.m_topDownTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_topDownTimeColumn.TooltipText = null;
			this.m_topDownTimeColumn.Width = 90;
			// 
			// m_topDownPercentColumn
			// 
			this.m_topDownPercentColumn.Header = "% of Parent";
			this.m_topDownPercentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_topDownPercentColumn.TooltipText = null;
			this.m_topDownPercentColumn.Width = 80;
			// 
			// m_topDownIdTextBox
			// 
			this.m_topDownIdTextBox.DataPropertyName = "Id";
			this.m_topDownIdTextBox.IncrementalSearchEnabled = true;
			this.m_topDownIdTextBox.LeftMargin = 3;
			this.m_topDownIdTextBox.ParentColumn = this.m_topDownIdColumn;
			// 
			// m_topDownThreadTextBox
			// 
			this.m_topDownThreadTextBox.DataPropertyName = "Thread";
			this.m_topDownThreadTextBox.IncrementalSearchEnabled = true;
			this.m_topDownThreadTextBox.LeftMargin = 3;
			this.m_topDownThreadTextBox.ParentColumn = this.m_topDownThreadIdColumn;
			// 
			// m_topDownNameTextBox
			// 
			this.m_topDownNameTextBox.DataPropertyName = "Name";
			this.m_topDownNameTextBox.IncrementalSearchEnabled = true;
			this.m_topDownNameTextBox.LeftMargin = 3;
			this.m_topDownNameTextBox.ParentColumn = this.m_topDownNameColumn;
			// 
			// m_topDownTimeTextBox
			// 
			this.m_topDownTimeTextBox.DataPropertyName = "Time";
			this.m_topDownTimeTextBox.IncrementalSearchEnabled = true;
			this.m_topDownTimeTextBox.LeftMargin = 3;
			this.m_topDownTimeTextBox.ParentColumn = this.m_topDownTimeColumn;
			// 
			// m_topDownPercentTextBox
			// 
			this.m_topDownPercentTextBox.DataPropertyName = "PercentTime";
			this.m_topDownPercentTextBox.IncrementalSearchEnabled = true;
			this.m_topDownPercentTextBox.LeftMargin = 3;
			this.m_topDownPercentTextBox.ParentColumn = this.m_topDownPercentColumn;
			// 
			// m_bottomUp
			// 
			this.m_bottomUp.BackColor = System.Drawing.SystemColors.Window;
			this.m_bottomUp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_bottomUp.Columns.Add(this.m_bottomUpIdColumn);
			this.m_bottomUp.Columns.Add(this.m_bottomUpThreadIdColumn);
			this.m_bottomUp.Columns.Add(this.m_bottomUpNameColumn);
			this.m_bottomUp.Columns.Add(this.m_bottomUpTimeColumn);
			this.m_bottomUp.Columns.Add(this.m_bottomUpPercentColumn);
			this.m_bottomUp.DefaultToolTipProvider = null;
			this.m_bottomUp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_bottomUp.DragDropMarkColor = System.Drawing.Color.Black;
			this.m_bottomUp.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle) ((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.m_bottomUp.LineColor = System.Drawing.SystemColors.ControlDark;
			this.m_bottomUp.LoadOnDemand = true;
			this.m_bottomUp.Location = new System.Drawing.Point(0, 0);
			this.m_bottomUp.Model = null;
			this.m_bottomUp.Name = "m_bottomUp";
			this.m_bottomUp.NodeControls.Add(this.m_bottomUpIdTextBox);
			this.m_bottomUp.NodeControls.Add(this.m_bottomUpThreadTextBox);
			this.m_bottomUp.NodeControls.Add(this.m_bottomUpNameTextBox);
			this.m_bottomUp.NodeControls.Add(this.m_bottomUpTimeTextBox);
			this.m_bottomUp.NodeControls.Add(this.m_bottomUpPercentTextBox);
			this.m_bottomUp.SelectedNode = null;
			this.m_bottomUp.Size = new System.Drawing.Size(884, 311);
			this.m_bottomUp.TabIndex = 1;
			this.m_bottomUp.UseColumns = true;
			this.m_bottomUp.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this.ColumnClicked);
			// 
			// m_bottomUpIdColumn
			// 
			this.m_bottomUpIdColumn.Header = "Id";
			this.m_bottomUpIdColumn.MinColumnWidth = 100;
			this.m_bottomUpIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_bottomUpIdColumn.TooltipText = null;
			this.m_bottomUpIdColumn.Width = 200;
			// 
			// m_bottomUpThreadIdColumn
			// 
			this.m_bottomUpThreadIdColumn.Header = "Thread";
			this.m_bottomUpThreadIdColumn.MinColumnWidth = 50;
			this.m_bottomUpThreadIdColumn.Sortable = true;
			this.m_bottomUpThreadIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_bottomUpThreadIdColumn.TooltipText = null;
			this.m_bottomUpThreadIdColumn.Width = 80;
			// 
			// m_bottomUpNameColumn
			// 
			this.m_bottomUpNameColumn.Header = "Function Parents (bottom-up)";
			this.m_bottomUpNameColumn.MinColumnWidth = 200;
			this.m_bottomUpNameColumn.Sortable = true;
			this.m_bottomUpNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_bottomUpNameColumn.TooltipText = null;
			this.m_bottomUpNameColumn.Width = 420;
			// 
			// m_bottomUpTimeColumn
			// 
			this.m_bottomUpTimeColumn.Header = "Time (Exclusive)";
			this.m_bottomUpTimeColumn.MinColumnWidth = 50;
			this.m_bottomUpTimeColumn.Sortable = true;
			this.m_bottomUpTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_bottomUpTimeColumn.TooltipText = null;
			this.m_bottomUpTimeColumn.Width = 90;
			// 
			// m_bottomUpPercentColumn
			// 
			this.m_bottomUpPercentColumn.Header = "% of Calls";
			this.m_bottomUpPercentColumn.MinColumnWidth = 50;
			this.m_bottomUpPercentColumn.Sortable = true;
			this.m_bottomUpPercentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_bottomUpPercentColumn.TooltipText = null;
			this.m_bottomUpPercentColumn.Width = 90;
			// 
			// m_bottomUpIdTextBox
			// 
			this.m_bottomUpIdTextBox.DataPropertyName = "Id";
			this.m_bottomUpIdTextBox.IncrementalSearchEnabled = true;
			this.m_bottomUpIdTextBox.LeftMargin = 3;
			this.m_bottomUpIdTextBox.ParentColumn = this.m_bottomUpIdColumn;
			// 
			// m_bottomUpThreadTextBox
			// 
			this.m_bottomUpThreadTextBox.DataPropertyName = "Thread";
			this.m_bottomUpThreadTextBox.IncrementalSearchEnabled = true;
			this.m_bottomUpThreadTextBox.LeftMargin = 3;
			this.m_bottomUpThreadTextBox.ParentColumn = this.m_bottomUpThreadIdColumn;
			// 
			// m_bottomUpNameTextBox
			// 
			this.m_bottomUpNameTextBox.DataPropertyName = "Name";
			this.m_bottomUpNameTextBox.IncrementalSearchEnabled = true;
			this.m_bottomUpNameTextBox.LeftMargin = 3;
			this.m_bottomUpNameTextBox.ParentColumn = this.m_bottomUpNameColumn;
			// 
			// m_bottomUpTimeTextBox
			// 
			this.m_bottomUpTimeTextBox.DataPropertyName = "Time";
			this.m_bottomUpTimeTextBox.IncrementalSearchEnabled = true;
			this.m_bottomUpTimeTextBox.LeftMargin = 3;
			this.m_bottomUpTimeTextBox.ParentColumn = this.m_bottomUpTimeColumn;
			// 
			// m_bottomUpPercentTextBox
			// 
			this.m_bottomUpPercentTextBox.DataPropertyName = "PercentTime";
			this.m_bottomUpPercentTextBox.IncrementalSearchEnabled = true;
			this.m_bottomUpPercentTextBox.LeftMargin = 3;
			this.m_bottomUpPercentTextBox.ParentColumn = this.m_bottomUpPercentColumn;
			// 
			// NProfStyleVisualizer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_splitter);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.Name = "NProfStyleVisualizer";
			this.Size = new System.Drawing.Size(884, 595);
			this.m_splitter.Panel1.ResumeLayout(false);
			this.m_splitter.Panel2.ResumeLayout(false);
			this.m_splitter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer m_splitter;
		private Aga.Controls.Tree.TreeViewAdv m_topDown;
		private Aga.Controls.Tree.TreeViewAdv m_bottomUp;
		private Aga.Controls.Tree.TreeColumn m_topDownIdColumn;
		private Aga.Controls.Tree.TreeColumn m_topDownThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_topDownNameColumn;
		private Aga.Controls.Tree.TreeColumn m_topDownTimeColumn;
		private Aga.Controls.Tree.TreeColumn m_bottomUpIdColumn;
		private Aga.Controls.Tree.TreeColumn m_bottomUpThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_bottomUpNameColumn;
		private Aga.Controls.Tree.TreeColumn m_bottomUpTimeColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_topDownIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_topDownThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_topDownNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_bottomUpIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_bottomUpThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_bottomUpNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_bottomUpTimeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_topDownTimeTextBox;
		private Aga.Controls.Tree.TreeColumn m_bottomUpPercentColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_bottomUpPercentTextBox;
		private Aga.Controls.Tree.TreeColumn m_topDownPercentColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_topDownPercentTextBox;
	}
}