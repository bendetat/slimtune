using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public partial class VisualizerToolbar : UserControl
	{
		private Snapshot m_snapshot;
		private IVisualizer m_visualizer;

		public VisualizerToolbar()
		{
			InitializeComponent();
		}

		private void UpdateSnapshotText()
		{
			if(m_snapshot.Id == 0)
			{
				ActiveSnapshotLabel.Text = string.Format("Viewing current data as of {0}", DateTime.Now);
			}
			else
			{
				ActiveSnapshotLabel.Text = string.Format("Viewing snapshot '{0}' from {1}", m_snapshot.Name, m_snapshot.DateTime);
			}
		}

		public VisualizerToolbar(Snapshot snapshot, IVisualizer visualizer)
		{
			if(snapshot == null)
				throw new ArgumentNullException("snapshot");
			if(visualizer == null)
				throw new ArgumentNullException("visualizer");

			m_snapshot = snapshot;
			m_visualizer = visualizer;

			InitializeComponent();

			m_visualizer.Refreshed += new EventHandler(m_visualizer_Refreshed);
			if(m_snapshot.Id == 0 && m_visualizer.SupportsRefresh)
			{
				RefreshButton.Visible = true;
			}
			else
			{
				RefreshButton.Visible = false;
			}

			UpdateSnapshotText();
		}

		void m_visualizer_Refreshed(object sender, EventArgs e)
		{
			this.Invoke(new UICore.Action(UpdateSnapshotText));
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			m_visualizer.RefreshView();
		}
	}
}
