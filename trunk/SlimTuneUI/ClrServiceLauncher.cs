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
using System.IO;
using System.Threading;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI
{
	[Serializable,
	DisplayName("CLR Service (Microsoft .NET 2.0, 4.0)")]
	public class ClrServiceLauncher : ClrLauncherBase
	{
		[Browsable(false)]
		public override string Name
		{
			get { return ServiceName; }
		}

		private string m_serviceName = string.Empty;
		[Category("\tService"),
		DisplayName("Service name"),
		Description("The name of the service to be profiled.")]
		public string ServiceName
		{
			get { return m_serviceName; }
			set
			{
				if(StartCommand == kDefaultStart + m_serviceName)
					StartCommand = kDefaultStart + value;
				if(StopCommand == kDefaultStop + m_serviceName)
					StopCommand = kDefaultStop + value;
				m_serviceName = value;
			}
		}

		[Browsable(false)]
		public override bool RequiresAdmin
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

		private const string kDefaultStart = "net start ";
		private const string kDefaultStop = "net stop ";

		public ClrServiceLauncher()
		{
			ListenPort = 3000;
			SamplingInterval = 10;
			StartCommand = kDefaultStart;
			StopCommand = kDefaultStop;
		}

		public override bool CheckParams()
		{
			if(ServiceName == string.Empty)
			{
				MessageBox.Show("You must enter a service name to run.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			var key = LauncherCommon.GetServiceKey(ServiceName);
			if(key == null)
			{
				MessageBox.Show("Unable to find a service with the specified name.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return base.CheckParams();
		}

		public override bool Launch()
		{
			StopService(ServiceName, StopCommand);

			string config = LauncherCommon.CreateConfigString(ProfilingMode, ListenPort, WaitForConnection, IncludeNative, SamplingInterval, CounterInterval, AllowMethodInlining);
			string[] profEnv = LauncherCommon.CreateProfilerEnvironment(config, LauncherCommon.GetCounterString(PerformanceCounters));

			string serviceAccountSid = null;
			string serviceAccountName = LauncherCommon.GetServiceAccountName(ServiceName);
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
				LauncherCommon.SetEnvironmentVariables(ServiceName, combinedEnv);
			}

			bool returnVal = true;
			StartService(ServiceName, StartCommand);

			Thread.Sleep(1000);
			using(var engine = new DummyDataEngine())
			{
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
			}

			if(serviceAccountSid != null)
			{
				LauncherCommon.ResetAccountEnvironment(serviceAccountSid, profEnv);
			}
			else
			{
				LauncherCommon.DeleteEnvironmentVariables(ServiceName);
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
