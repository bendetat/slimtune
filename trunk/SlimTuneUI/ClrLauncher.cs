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

		[Browsable(false)]
		public bool RequiresAdmin
		{
			get { return false; }
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

		private ushort m_listenPort;
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

		private int m_samplingInterval;
		[Category("Profiling"),
		DisplayName("Sampling interval"),
		Description("The amount of time between stack samples, in milliseconds. Raising this value reduces how much data is collected, but improves application performance.")]
		public int SamplingInterval
		{
			get { return m_samplingInterval; }
			set
			{
				if(value < 1)
					throw new ArgumentOutOfRangeException("SamplingInterval", value, "Sampling interval must be at least 1ms.");
				m_samplingInterval = value;
			}
		}

		public ClrLauncher()
		{
			ListenPort = 3000;
			SamplingInterval = 5;
		}

		public bool CheckParams()
		{
			if(Executable == string.Empty)
			{
				MessageBox.Show("You must enter an executable file to run.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(!File.Exists(Executable))
			{
				MessageBox.Show("Executable does not exist.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(ListenPort < 1)
			{
				MessageBox.Show("Listen port must be between 1 and 65535.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(SamplingInterval < 1)
			{
				MessageBox.Show("Sampling interval must be at least 1ms.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public bool Launch()
		{
			string config = LauncherCommon.CreateConfigString(ProfilingMode, ListenPort, WaitForConnection, IncludeNative, SamplingInterval);
			var psi = new ProcessStartInfo(Executable, Arguments);
			LauncherCommon.SetProcessOptions(psi, config);
			psi.WorkingDirectory = string.IsNullOrEmpty(WorkingDir) ?
				Path.GetDirectoryName(Executable) : WorkingDir;

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
