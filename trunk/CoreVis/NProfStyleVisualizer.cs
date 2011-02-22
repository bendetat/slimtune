﻿/*
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;

using UICore;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("NProf-Style TreeViews")]
	public partial class NProfStyleVisualizer : UserControl, IVisualizer
	{
		ProfilerWindowBase m_mainWindow;
		Connection m_connection;

		ParentsModel m_calleesModel;
		CallersModel m_callersModel;

		public string DisplayName
		{
			get { return "Tree Views"; }
		}

		public NProfStyleVisualizer()
		{
			InitializeComponent();
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;

			m_calleesModel = new ParentsModel(connection.DataEngine, m_mainWindow);
			m_callersModel = new CallersModel(connection.DataEngine, m_mainWindow);
			m_callees.Model = new SortedTreeModel(m_calleesModel);
			m_callers.Model = new SortedTreeModel(m_callersModel);

			//set the sort orders
			ColumnClicked(m_callees, new TreeColumnEventArgs(m_parentsTimeColumn));
			ColumnClicked(m_callees, new TreeColumnEventArgs(m_parentsTimeColumn));
			ColumnClicked(m_callers, new TreeColumnEventArgs(m_callersTimeColumn));
			ColumnClicked(m_callers, new TreeColumnEventArgs(m_callersTimeColumn));

			return true;
		}

		public void Show(Control.ControlCollection parent)
		{
			this.Dock = DockStyle.Fill;
			parent.Add(this);
		}

		public void OnClose()
		{
		}

		private void m_refreshButton_Click(object sender, EventArgs e)
		{
			m_calleesModel.Refresh();
			m_callersModel.Refresh();
		}

		private void ColumnClicked(object sender, TreeColumnEventArgs e)
		{
			TreeColumn clicked = e.Column;

			if(clicked.SortOrder == SortOrder.None)
				clicked.SortOrder = SortOrder.Descending;
			else if(clicked.SortOrder == SortOrder.Descending)
				clicked.SortOrder = SortOrder.Ascending;
			else if(clicked.SortOrder == SortOrder.Ascending)
				clicked.SortOrder = SortOrder.None;

			var tree = sender as TreeViewAdv;
			(tree.Model as SortedTreeModel).Comparer = new FunctionComparer(this, clicked, clicked.SortOrder);
		}

		class FunctionComparer : System.Collections.IComparer, IComparer<FunctionItem>
		{
			//this is incredibly stupid
			public NProfStyleVisualizer Parent;
			public TreeColumn Column;
			public SortOrder Order;

			public FunctionComparer(NProfStyleVisualizer parent, TreeColumn column, SortOrder order)
			{
				this.Parent = parent;
				this.Column = column;
				this.Order = order;
			}

			private static int Compare(double? x, double? y)
			{
				if(x.HasValue && y.HasValue)
					return x.Value.CompareTo(y.Value);
				else
					return x.HasValue.CompareTo(y.HasValue);
			}

			public int Compare(FunctionItem x, FunctionItem y)
			{
				//yeah, this is awful
				int result = 0;
				if(Column == Parent.m_parentsIdColumn || Column == Parent.m_callersIdColumn)
					result = x.Id.CompareTo(y.Id);
				else if(Column == Parent.m_parentsThreadIdColumn || Column == Parent.m_callersThreadIdColumn)
					result = x.Thread.CompareTo(y.Thread);
				else if(Column == Parent.m_parentsNameColumn || Column == Parent.m_callersNameColumn)
					result = x.Name.CompareTo(y.Name);
				else if(Column == Parent.m_parentsTimeColumn || Column == Parent.m_callersTimeColumn)
					result = x.Time.CompareTo(y.Time);
				else if(Column == Parent.m_parentsPercentColumn || Column == Parent.m_callersPercentColumn)
					result = Compare(x.PercentTime, y.PercentTime);

				//if primary sort is not differentiating, go to secondary sort criteria (hard coded for now)
				if(result == 0)
					result = x.Thread.CompareTo(y.Thread);
				if(result == 0)
					result = -Compare(x.PercentTime, y.PercentTime);
				if(result == 0 && x.Name != null)
					result = -x.Name.CompareTo(y.Name);
				if(result == 0)
					result = x.Id.CompareTo(y.Id);

				if(Order == SortOrder.Ascending)
					result = -result;
				return result;
			}

			public int Compare(object x, object y)
			{
				return Compare(x as FunctionItem, y as FunctionItem);
			}
		}
	}

	class FunctionItem
	{
		public int Id { get; set; }
		public int Thread { get; set; }
		public string Name { get; set; }
		public double Time { get; set; }
		public double? PercentTime { get; set; }
	}

	class ParentsModel : ITreeModel
	{
		//find out what functions took the most time inclusive
		const string kTopLevelQuery = @"
select s, max(s2.Time)
from Sample s
	join s.Thread.Samples s2
	left join fetch s.Function
where s2.SnapshotId = :snapshotId
group by s.Id
order by s.Time desc
";

		const string kChildQuery = @"
select c, sum(c2.Time)
from Call c
	join c.Parent.CallsAsParent c2
	left join fetch c.Child
where c.ParentId = :parentId and c.ThreadId = :threadId
	and c2.ThreadId = :threadId and c2.SnapshotId = :snapshotId
group by c.Id
";

		IDataEngine m_data;
		ProfilerWindowBase m_mainWindow;

		public ParentsModel(IDataEngine data, ProfilerWindowBase mainWindow)
		{
			m_data = data;
			m_mainWindow = mainWindow;
		}

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			using(var session = m_mainWindow.OpenActiveSnapshot())
			{
				if(treePath.IsEmpty())
				{
					//top level queries
					var data = session.CreateQuery(kTopLevelQuery)
						.SetInt32("snapshotId", m_mainWindow.ActiveSnapshot.Id)
						.SetMaxResults(200)
						.List<object[]>();
					foreach(var row in data)
					{
						Sample s = row[0] as Sample;
						double totalTime = Convert.ToDouble(row[1]);
						var item = new FunctionItem();
						item.Id = s.FunctionId;
						item.Thread = s.ThreadId;
						item.Name = FunctionInfo.GetFullSignature(s.Function);
						if(s.Function != null)
							item.Name = s.Function.Name + s.Function.Signature;
						else
							item.Name = "(unknown)";
						item.Time = s.Time;
						item.PercentTime = Math.Round(100 * s.Time / totalTime, 3);
						yield return item;
					}
				}
				else
				{
					var parentNode = treePath.LastNode as FunctionItem;
					var data = session.CreateQuery(kChildQuery)
						.SetInt32("parentId", parentNode.Id)
						.SetInt32("threadId", parentNode.Thread)
						.SetInt32("snapshotId", m_mainWindow.ActiveSnapshot.Id)
						.List<object[]>();

					foreach(var row in data)
					{
						var c = row[0] as Call;
						var parentTime = Convert.ToDouble(row[1]);
						var item = new FunctionItem();
						item.Thread = parentNode.Thread;
						item.Id = c.ChildId;
						item.Name = FunctionInfo.GetFullSignature(c.Child);
						item.Time = c.Time;
						if(parentTime == 0)
							item.PercentTime = 0;
						else
							item.PercentTime = Math.Round(100 * item.Time / parentTime, 3);
						yield return item;
					}
				}

				yield break;
			}
		}

		public bool IsLeaf(TreePath treePath)
		{
			return false;
		}

		public void Refresh()
		{
			StructureChanged(this, new TreePathEventArgs());
		}

#pragma warning disable 67
		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;
#pragma warning restore
	}

	class CallersModel : ITreeModel
	{
		//find the methods that account for the most time exclusive
		//that means high time in Call.ChildId = 0
		const string kTopLevelQuery = @"
from Call c
	left join fetch c.Parent
where c.Child.Id = 0
order by Time desc
";

		const string kChildQuery = @"
select c, sum(c2.Time)
from Call c
	join c.Child.CallsAsChild c2
	left join fetch c.Parent
where c.Child.Id = :childId and c.Thread.Id = :threadId
	and c2.Thread.Id = :threadId and c2.Snapshot.Id = :snapshotId
group by c.Id
";

		IDataEngine m_data;
		ProfilerWindowBase m_mainWindow;

		public CallersModel(IDataEngine data, ProfilerWindowBase mainWindow)
		{
			m_data = data;
			m_mainWindow = mainWindow;
		}

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			using(var session = m_mainWindow.OpenActiveSnapshot())
			{
				if(treePath.IsEmpty())
				{
					//top level queries
					var calls = session.CreateQuery(kTopLevelQuery).List<Call>();
					foreach(var c in calls)
					{
						var item = new FunctionItem();
						item.Id = c.ParentId;
						item.Thread = c.Thread.Id;
						item.Name = FunctionInfo.GetFullSignature(c.Parent);
						item.Time = c.Time;
						yield return item;
					}
				}
				else
				{
					var parentNode = treePath.LastNode as FunctionItem;
					var data = session.CreateQuery(kChildQuery)
						.SetInt32("childId", parentNode.Id)
						.SetInt32("threadId", parentNode.Thread)
						.SetInt32("snapshotId", m_mainWindow.ActiveSnapshot.Id)
						.List<object[]>();

					foreach(var row in data)
					{
						Call c = row[0] as Call;
						var parentTime = Convert.ToDouble(row[1]);
						var item = new FunctionItem();
						item.Thread = parentNode.Thread;
						item.Id = c.ParentId;
						item.Name = FunctionInfo.GetFullSignature(c.Parent);
						item.Time = c.Time;
						if(parentTime == 0)
							item.PercentTime = 0;
						else
							item.PercentTime = Math.Round(100 * item.Time / parentTime, 3);
						yield return item;
					}
				}

				yield break;
			}
		}

		public bool IsLeaf(TreePath treePath)
		{
			return false;
		}

		public void Refresh()
		{
			StructureChanged(this, new TreePathEventArgs());
		}

#pragma warning disable 67
		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;
#pragma warning restore
	}
}
