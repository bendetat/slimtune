using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace UICore
{
	[Serializable,
	DisplayName("CLR Application (Microsoft .NET 2.0)")]
	public class ClrLauncher : ILauncher
	{
		public const string ProfilerGuid = "{38A7EA35-B221-425a-AD07-D058C581611D}";

		//NOTE: These are all in order for the property grid
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor)),
		Category("Application"),
		Description("The path of the executable to launch for profiling.")]
		public string Executable { get; set; }

		[Browsable(false)]
		public string Name
		{
			get { return Executable; }
		}

		[Category("Application"),
		Description("The command line that should be passed to the executable when launched.")]
		public string Arguments { get; set; }

		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor)),
		Category("Application"),
		DisplayName("Working directory"),
		Description("The working directory to use when launching the executable. If left blank, the executable's directory will be used.")]
		public string WorkingDir { get; set; }

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
		DisplayName("Listen port"),
		Description("The TCP port that the profiler should use. Only change this if you are profiling multiple applications at once.")]
		public ushort ListenPort { get; set; }

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

		public ClrLauncher()
		{
			ListenPort = 3000;
		}

		public bool CheckParams()
		{
			if(Executable == string.Empty)
			{
				MessageBox.Show("You must enter an executable file to run.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(!File.Exists(Executable))
			{
				MessageBox.Show("Executable does not exist.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public bool Launch()
		{
			string config = string.Empty;
			config += string.Format("Mode={0};", (int) ProfilingMode);
			config += string.Format("Port={0};", ListenPort);
			config += string.Format("Wait={0};", WaitForConnection ? 1 : 0);
			config += string.Format("SampleUnmanaged={0};", IncludeNative ? 1 : 0);

			var psi = new ProcessStartInfo(Executable, Arguments);
			psi.RedirectStandardOutput = false;
			psi.RedirectStandardError = false;
			psi.RedirectStandardInput = false;
			psi.UseShellExecute = false;
			psi.WorkingDirectory = string.IsNullOrEmpty(WorkingDir) ?
				Path.GetDirectoryName(Executable) : WorkingDir;

			psi.EnvironmentVariables["COR_ENABLE_PROFILING"] = "1";
			psi.EnvironmentVariables["COR_PROFILER"] = ProfilerGuid;
			psi.EnvironmentVariables["SLIMTUNE_CONFIG"] = config;

			try
			{
				Process.Start(psi);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Launch Error");
				return false;
			}

			return true;
		}
	}
}
