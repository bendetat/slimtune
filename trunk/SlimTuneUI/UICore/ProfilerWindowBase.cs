using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UICore
{
	public class ProfilerWindowBase : Form
	{
		public List<IVisualizer> Visualizers
		{
			get;
			private set;
		}

		public Connection Connection
		{
			get;
			protected set;
		}

		public ProfilerWindowBase(Connection conn)
		{
			Visualizers = new List<IVisualizer>();
			Connection = conn;
		}
	}
}
