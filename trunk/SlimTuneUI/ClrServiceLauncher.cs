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
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;
using System.IO;
using System.Threading;

using UICore;

namespace SlimTuneUI
{
	[Serializable,
	DisplayName("CLR Service (Microsoft .NET 2.0)")]
	public class ClrServiceLauncher : ILauncher
	{
		private string m_name = string.Empty;
		[Category("\tService"),
		DisplayName("Service name"),
		Description("The name of the service to be profiled.")]
		public string Name
		{
			get { return m_name; }
			set
			{
				if(StartCommand == kDefaultStart + m_name)
					StartCommand = kDefaultStart + value;
				if(StopCommand == kDefaultStop + m_name)
					StopCommand = kDefaultStop + value;
				m_name = value;
			}
		}

		[Browsable(false)]
		public bool RequiresAdmin
		{
			get { return true; }
		}

		[Category("\tService"),
		DisplayName("Start command"),
		Description("The command used to start the service.")]
		public string StartCommand { get; set; }

		[Category("\tService"),
		DisplayName("Stop command"),
		Description("The command used to stop the service.")]
		public string StopCommand { get; set; }

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

		private const string kDefaultStart = "net start ";
		private const string kDefaultStop = "net stop ";

		public ClrServiceLauncher()
		{
			ListenPort = 3000;
			StartCommand = kDefaultStart;
			StopCommand = kDefaultStop;
		}

		public bool CheckParams()
		{
			return true;
		}

		public bool Launch()
		{
			StopService(Name, StopCommand);

			string config = LauncherCommon.CreateConfigString(ProfilingMode, ListenPort, WaitForConnection, IncludeNative);
			string[] profEnv = LauncherCommon.CreateProfilerEnvironment(config);

			string serviceAccountSid = null;
			string serviceAccountName = LauncherCommon.GetServiceAccountName(Name);
			if(serviceAccountName != null && serviceAccountName.StartsWith(@".\"))
				serviceAccountName = Environment.MachineName + serviceAccountName.Substring(1);
			if(serviceAccountName != null && serviceAccountName != "LocalSystem")
			{
				serviceAccountSid = LauncherCommon.GetAccountSid(serviceAccountName);
			}

			if(serviceAccountSid != null)
			{
				//set environment for target account
				LauncherCommon.SetAccountEnvironment(serviceAccountSid, profEnv);
			}
			else
			{
				string[] baseEnv = LauncherCommon.GetServicesEnvironment();
				baseEnv = LauncherCommon.ReplaceTempDir(baseEnv, Path.GetTempPath());
				string[] combinedEnv = LauncherCommon.CombineEnvironments(baseEnv, profEnv);
				LauncherCommon.SetEnvironmentVariables(Name, combinedEnv);
			}

			bool returnVal = true;
			StartService(Name, StartCommand);

			Thread.Sleep(1000);
			var engine = new SQLiteMemoryEngine();
			var progress = new ConnectProgress("localhost", ListenPort, engine, 10);
			progress.ShowDialog();
			if(progress.Client != null)
			{
				progress.Client.Dispose();
			}
			else
			{
				returnVal = false;
			}
			engine.Dispose();

			if(serviceAccountSid != null)
			{
				LauncherCommon.ResetAccountEnvironment(serviceAccountSid, profEnv);
			}
			else
			{
				LauncherCommon.DeleteEnvironmentVariables(Name);
			}

			return returnVal;
		}

		private static void StopService(string serviceName, string stopCommand)
		{
			// stop service
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
			processStartInfo.Arguments = "/c " + stopCommand;
			Process process = Process.Start(processStartInfo);
			while(!process.HasExited)
			{
				Thread.Sleep(1000);
			}
		}

		private static Process StartService(string serviceName, string startCommand)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
			processStartInfo.Arguments = "/c " + startCommand;
			Process process = Process.Start(processStartInfo);
			return process;
		}
	}
}
