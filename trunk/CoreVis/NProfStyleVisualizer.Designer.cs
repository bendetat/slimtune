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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NProfStyleVisualizer));
			this.m_splitter = new System.Windows.Forms.SplitContainer();
			this.m_callees = new Aga.Controls.Tree.TreeViewAdv();
			this.m_parentsIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_parentsThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_parentsNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_parentsTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_parentsPercentColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_parentsIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_parentsThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_parentsNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_parentsTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_parentsPercentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callers = new Aga.Controls.Tree.TreeViewAdv();
			this.m_callersIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersThreadIdColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersPercentColumn = new Aga.Controls.Tree.TreeColumn();
			this.m_callersIdTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersThreadTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_callersPercentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.m_refreshButton = new System.Windows.Forms.Button();
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
			this.m_callees.Columns.Add(this.m_parentsIdColumn);
			this.m_callees.Columns.Add(this.m_parentsThreadIdColumn);
			this.m_callees.Columns.Add(this.m_parentsNameColumn);
			this.m_callees.Columns.Add(this.m_parentsTimeColumn);
			this.m_callees.Columns.Add(this.m_parentsPercentColumn);
			this.m_callees.DefaultToolTipProvider = null;
			this.m_callees.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_callees.DragDropMarkColor = System.Drawing.Color.Black;
			this.m_callees.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle) ((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.m_callees.LineColor = System.Drawing.SystemColors.ControlDark;
			this.m_callees.LoadOnDemand = true;
			this.m_callees.Location = new System.Drawing.Point(0, 0);
			this.m_callees.Model = null;
			this.m_callees.Name = "m_callees";
			this.m_callees.NodeControls.Add(this.m_parentsIdTextBox);
			this.m_callees.NodeControls.Add(this.m_parentsThreadTextBox);
			this.m_callees.NodeControls.Add(this.m_parentsNameTextBox);
			this.m_callees.NodeControls.Add(this.m_parentsTimeTextBox);
			this.m_callees.NodeControls.Add(this.m_parentsPercentTextBox);
			this.m_callees.SelectedNode = null;
			this.m_callees.Size = new System.Drawing.Size(884, 268);
			this.m_callees.TabIndex = 0;
			this.m_callees.UnloadCollapsedOnReload = true;
			this.m_callees.UseColumns = true;
			this.m_callees.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this.ColumnClicked);
			// 
			// m_parentsIdColumn
			// 
			this.m_parentsIdColumn.Header = "Id";
			this.m_parentsIdColumn.MinColumnWidth = 100;
			this.m_parentsIdColumn.Sortable = true;
			this.m_parentsIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_parentsIdColumn.TooltipText = null;
			this.m_parentsIdColumn.Width = 200;
			// 
			// m_parentsThreadIdColumn
			// 
			this.m_parentsThreadIdColumn.Header = "Thread";
			this.m_parentsThreadIdColumn.MinColumnWidth = 50;
			this.m_parentsThreadIdColumn.Sortable = true;
			this.m_parentsThreadIdColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_parentsThreadIdColumn.TooltipText = null;
			this.m_parentsThreadIdColumn.Width = 80;
			// 
			// m_parentsNameColumn
			// 
			this.m_parentsNameColumn.Header = "Parents";
			this.m_parentsNameColumn.MinColumnWidth = 200;
			this.m_parentsNameColumn.Sortable = true;
			this.m_parentsNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_parentsNameColumn.TooltipText = null;
			this.m_parentsNameColumn.Width = 420;
			// 
			// m_parentsTimeColumn
			// 
			this.m_parentsTimeColumn.Header = "Time";
			this.m_parentsTimeColumn.MinColumnWidth = 60;
			this.m_parentsTimeColumn.Sortable = true;
			this.m_parentsTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_parentsTimeColumn.TooltipText = null;
			this.m_parentsTimeColumn.Width = 70;
			// 
			// m_parentsPercentColumn
			// 
			this.m_parentsPercentColumn.Header = "% of Parent";
			this.m_parentsPercentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_parentsPercentColumn.TooltipText = null;
			this.m_parentsPercentColumn.Width = 80;
			// 
			// m_parentsIdTextBox
			// 
			this.m_parentsIdTextBox.DataPropertyName = "Id";
			this.m_parentsIdTextBox.IncrementalSearchEnabled = true;
			this.m_parentsIdTextBox.LeftMargin = 3;
			this.m_parentsIdTextBox.ParentColumn = this.m_parentsIdColumn;
			// 
			// m_parentsThreadTextBox
			// 
			this.m_parentsThreadTextBox.DataPropertyName = "Thread";
			this.m_parentsThreadTextBox.IncrementalSearchEnabled = true;
			this.m_parentsThreadTextBox.LeftMargin = 3;
			this.m_parentsThreadTextBox.ParentColumn = this.m_parentsThreadIdColumn;
			// 
			// m_parentsNameTextBox
			// 
			this.m_parentsNameTextBox.DataPropertyName = "Name";
			this.m_parentsNameTextBox.IncrementalSearchEnabled = true;
			this.m_parentsNameTextBox.LeftMargin = 3;
			this.m_parentsNameTextBox.ParentColumn = this.m_parentsNameColumn;
			// 
			// m_parentsTimeTextBox
			// 
			this.m_parentsTimeTextBox.DataPropertyName = "Time";
			this.m_parentsTimeTextBox.IncrementalSearchEnabled = true;
			this.m_parentsTimeTextBox.LeftMargin = 3;
			this.m_parentsTimeTextBox.ParentColumn = this.m_parentsTimeColumn;
			// 
			// m_parentsPercentTextBox
			// 
			this.m_parentsPercentTextBox.DataPropertyName = "PercentTime";
			this.m_parentsPercentTextBox.IncrementalSearchEnabled = true;
			this.m_parentsPercentTextBox.LeftMargin = 3;
			this.m_parentsPercentTextBox.ParentColumn = this.m_parentsPercentColumn;
			// 
			// m_callers
			// 
			this.m_callers.BackColor = System.Drawing.SystemColors.Window;
			this.m_callers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_callers.Columns.Add(this.m_callersIdColumn);
			this.m_callers.Columns.Add(this.m_callersThreadIdColumn);
			this.m_callers.Columns.Add(this.m_callersNameColumn);
			this.m_callers.Columns.Add(this.m_callersTimeColumn);
			this.m_callers.Columns.Add(this.m_callersPercentColumn);
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
			this.m_callers.NodeControls.Add(this.m_callersTimeTextBox);
			this.m_callers.NodeControls.Add(this.m_callersPercentTextBox);
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
			// m_callersTimeColumn
			// 
			this.m_callersTimeColumn.Header = "% Time";
			this.m_callersTimeColumn.MinColumnWidth = 50;
			this.m_callersTimeColumn.Sortable = true;
			this.m_callersTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersTimeColumn.TooltipText = null;
			this.m_callersTimeColumn.Width = 90;
			// 
			// m_callersPercentColumn
			// 
			this.m_callersPercentColumn.Header = "% of Calls";
			this.m_callersPercentColumn.MinColumnWidth = 50;
			this.m_callersPercentColumn.Sortable = true;
			this.m_callersPercentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.m_callersPercentColumn.TooltipText = null;
			this.m_callersPercentColumn.Width = 90;
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
			// m_callersTimeTextBox
			// 
			this.m_callersTimeTextBox.DataPropertyName = "Time";
			this.m_callersTimeTextBox.IncrementalSearchEnabled = true;
			this.m_callersTimeTextBox.LeftMargin = 3;
			this.m_callersTimeTextBox.ParentColumn = this.m_callersTimeColumn;
			// 
			// m_callersPercentTextBox
			// 
			this.m_callersPercentTextBox.DataPropertyName = "PercentTime";
			this.m_callersPercentTextBox.IncrementalSearchEnabled = true;
			this.m_callersPercentTextBox.LeftMargin = 3;
			this.m_callersPercentTextBox.ParentColumn = this.m_callersPercentColumn;
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
			// NProfStyleVisualizer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_refreshButton);
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
		private Aga.Controls.Tree.TreeViewAdv m_callees;
		private Aga.Controls.Tree.TreeViewAdv m_callers;
		private Aga.Controls.Tree.TreeColumn m_parentsIdColumn;
		private Aga.Controls.Tree.TreeColumn m_parentsThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_parentsNameColumn;
		private Aga.Controls.Tree.TreeColumn m_parentsTimeColumn;
		private Aga.Controls.Tree.TreeColumn m_callersIdColumn;
		private Aga.Controls.Tree.TreeColumn m_callersThreadIdColumn;
		private Aga.Controls.Tree.TreeColumn m_callersNameColumn;
		private Aga.Controls.Tree.TreeColumn m_callersTimeColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_parentsIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_parentsThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_parentsNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersIdTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersThreadTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersNameTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersTimeTextBox;
		private System.Windows.Forms.Button m_refreshButton;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_parentsTimeTextBox;
		private Aga.Controls.Tree.TreeColumn m_callersPercentColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_callersPercentTextBox;
		private Aga.Controls.Tree.TreeColumn m_parentsPercentColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox m_parentsPercentTextBox;
	}
}