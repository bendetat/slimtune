using System;
using System.ComponentModel;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	public abstract class ClrLauncherBase : ILauncher
	{
		public abstract string Name
		{
			get;
		}

		private ProfilerMode m_profMode = ProfilerMode.Sampling;
		[Category("Profiling"),
		DisplayName("Profiler mode"),
		Description("The profiling method to use. Sampling is recommended.")]
		public ProfilerMode ProfilingMode
		{
			get { return m_profMode; }
			set
			{
				if(value == ProfilerMode.Disabled)
					throw new ArgumentOutOfRangeException("value");
				m_profMode = value;
			}
		}

		[Category("Profiling"),
		DisplayName("Include native functions"),
		Description("Include native code profiling. Generally speaking, this isn't helpful at all.")]
		public bool IncludeNative { get; set; }

		[Category("Profiling"),
		DisplayName("Wait for connection"),
		Description("If enabled, the executable will be prevented from launching until a profiler front-end connects. Not recommended (deadlock risk).")]
		public bool WaitForConnection { get; set; }

		[Category("Profiling"),
		DisplayName("Suspend on connect"),
		Description("Causes the target process to suspend when a profiler connects.")]
		public bool SuspendOnConnect { get; set; }

		private ushort m_listenPort = 3000;
		[Category("Profiling"),
		DisplayName("Listen port"),
		Description("The TCP port that the profiler should use. Only change this if you are profiling multiple applications at once.")]
		public ushort ListenPort
		{
			get { return m_listenPort; }
			set
			{
				if(value < 1)
					throw new ArgumentOutOfRangeException("ListenPort", value, "Listen port must be at least 1.");
				m_listenPort = value;
			}
		}

		private int m_samplingInterval = 5;
		[Category("Profiling"),
		DisplayName("Sampling interval"),
		Description("The amount of time between stack samples, in milliseconds. Raising this value reduces how much data is collected, but improves application performance.")]
		public int SamplingInterval
		{
			get { return m_samplingInterval; }
			set
			{
				if(m_samplingInterval < 1)
					throw new ArgumentOutOfRangeException("SamplingInterval", value, "Sampling interval must be at least 1ms.");
				m_samplingInterval = value;
			}
		}

		public abstract bool RequiresAdmin
		{
			get;
		}

		public virtual bool CheckParams()
		{
			if(ListenPort < 1)
			{
				MessageBox.Show("Listen port must be between 1 and 65535.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			using(var tempEngine = new SQLiteMemoryEngine())
			{
				bool used = LauncherCommon.TestConnection("localhost", ListenPort, tempEngine);
				if(used)
				{
					DialogResult result = MessageBox.Show("This port appears to be in use already. Continue anyway?",
						"Port In Use", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
					if(result == DialogResult.No)
						return false;
				}
			}

			if(SamplingInterval < 1)
			{
				MessageBox.Show("Sampling interval must be at least 1ms.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public abstract bool Launch();
	}
}
