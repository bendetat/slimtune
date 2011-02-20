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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using UICore;

namespace SlimTuneUI
{
	public struct NativeConfig
	{
		public ProfilerMode ProfilingMode;
		public ushort ListenPort;
		public int SamplingInterval;
		public int CounterInterval;

		public string CreateString()
		{
			string config = string.Empty;
			config += string.Format("Mode={0};", (int) ProfilingMode);
			config += string.Format("Port={0};", ListenPort);
			config += string.Format("SampleInterval={0};", SamplingInterval);
			config += string.Format("CounterInterval={0};", CounterInterval);

			return config;
		}
	}

	[Serializable,
	DisplayName("Native code application (Visual C++)")]
	public class NativeLauncher : ILauncher
	{
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor)),
		Category("Application"),
		Description("The path of the executable to launch for profiling.")]
		public string Executable { get; set; }

		[Browsable(false)]
		public string Name
		{
			get { return Executable; }
		}

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
				if(value < 1)
					throw new ArgumentOutOfRangeException("SamplingInterval", value, "Sampling interval must be at least 1ms.");
				m_samplingInterval = value;
			}
		}

		[Category("Application"),
		Description("The command line that should be passed to the executable when launched.")]
		public string Arguments { get; set; }

		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor)),
		Category("Application"),
		DisplayName("Working directory"),
		Description("The working directory to use when launching the executable. If left blank, the executable's directory will be used.")]
		public string WorkingDir { get; set; }

		public bool RequiresAdmin
		{
			get { return false; }
		}

		public NativeLauncher()
		{
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

			using(var tempEngine = new DummyDataEngine())
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

			return true;
		}

		public bool Launch()
		{
			NativeConfig config = new NativeConfig
			{
				ProfilingMode = ProfilerMode.Sampling,
				ListenPort = ListenPort,
				SamplingInterval = SamplingInterval,
				CounterInterval = 1000
			};

			string configString = config.CreateString();
			string argStr = Executable + " " + Arguments;
			var psi = new ProcessStartInfo("Backends\\SlimTuneNative_x86.exe", argStr);
			LauncherCommon.SetProcessOptions(psi, configString, string.Empty, false);
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
