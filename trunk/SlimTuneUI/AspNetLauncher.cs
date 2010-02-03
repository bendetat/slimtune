using System;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using UICore;

namespace SlimTuneUI
{
	[Serializable,
	DisplayName("ASP.NET 2.0 Server")]
	public class AspNetLauncher : ILauncher
	{
		[Browsable(false)]
		public string Name
		{
			get { return "ASP.NET"; }
		}

		[Browsable(false)]
		public bool RequiresAdmin
		{
			get { return true; }
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
				if(m_samplingInterval < 1)
					throw new ArgumentOutOfRangeException("SamplingInterval", value, "Sampling interval must be at least 1ms.");
				m_samplingInterval = value;
			}
		}

		public AspNetLauncher()
		{
			ListenPort = 3000;
			SamplingInterval = 10;
		}

		public bool CheckParams()
		{
			var key = LauncherCommon.GetServiceKey("IISADMIN") ?? LauncherCommon.GetServiceKey("W3SVC") ?? LauncherCommon.GetServiceKey("WAS");
			if(key == null)
			{
				MessageBox.Show("Unable to find a compatible ASP.NET installation.", "ASP.NET Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(ListenPort < 1)
			{
				MessageBox.Show("Listen port must be between 1 and 65535.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if(SamplingInterval < 1)
			{
				MessageBox.Show("Sampling interval must be at least 1.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public bool Launch()
		{
			StopIIS();

			string config = LauncherCommon.CreateConfigString(ProfilingMode, ListenPort, false, IncludeNative, SamplingInterval);
			string[] profilerEnv = LauncherCommon.CreateProfilerEnvironment(config);
			string[] baseEnv = LauncherCommon.GetServicesEnvironment();
			baseEnv = LauncherCommon.ReplaceTempDir(baseEnv, Path.GetTempPath());
			string[] env = LauncherCommon.CombineEnvironments(baseEnv, profilerEnv);

			LauncherCommon.SetEnvironmentVariables("IISADMIN", env);
			LauncherCommon.SetEnvironmentVariables("W3SVC", env);
			LauncherCommon.SetEnvironmentVariables("WAS", env);

			string accountName = GetASP_NETaccountName();
			string accountSid = null;
			if(accountName != null)
			{
				accountSid = LauncherCommon.GetAccountSid(accountName);
				if(accountSid != null)
					LauncherCommon.SetAccountEnvironment(accountSid, profilerEnv);
			}

			bool returnVal = StartIIS();
			if(returnVal)
			{
				Thread.Sleep(1000);

				//we need to create a connection in order to know when CLR has been loaded
				IStorageEngine engine = new SQLiteMemoryEngine();
				ConnectProgress progress = new ConnectProgress("localhost", ListenPort, engine, 20);
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
			}

			LauncherCommon.DeleteEnvironmentVariables("IISADMIN");
			LauncherCommon.DeleteEnvironmentVariables("W3SVC");
			LauncherCommon.DeleteEnvironmentVariables("WAS");

			if(accountSid != null)
				LauncherCommon.ResetAccountEnvironment(accountSid, profilerEnv);

			return returnVal;
		}

		//Taken from CLRProfiler
		private static void StopIIS()
		{
			// stop IIS
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
			if(Environment.OSVersion.Version.Major >= 6/*Vista*/)
				processStartInfo.Arguments = "/c net stop was /y";
			else
				processStartInfo.Arguments = "/c net stop iisadmin /y";
			Process process = Process.Start(processStartInfo);
			while(!process.HasExited)
			{
				Thread.Sleep(100);
				Application.DoEvents();
			}
		}

		private static bool StartIIS()
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
			processStartInfo.Arguments = "/c net start w3svc";
			Process process = Process.Start(processStartInfo);
			while(!process.HasExited)
			{
				Thread.Sleep(100);
				Application.DoEvents();
			}
			if(process.ExitCode != 0)
			{
				return false;
			}
			return true;
		}

		private static string GetASP_NETaccountName()
		{
			try
			{
				XmlDocument machineConfig = new XmlDocument();
				string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();
				string configPath = Path.Combine(runtimePath, @"CONFIG\machine.config");
				machineConfig.Load(configPath);
				XmlNodeList elemList = machineConfig.GetElementsByTagName("processModel");
				for(int i = 0; i < elemList.Count; i++)
				{
					XmlAttributeCollection attributes = elemList[i].Attributes;
					XmlAttribute userNameAttribute = attributes["userName"];
					if(userNameAttribute != null)
					{
						string userName = userNameAttribute.InnerText;
						if(userName == "machine")
							return "ASPNET";
						else if(userName == "SYSTEM")
							return null;
						else
							return userName;
					}
				}
			}
			catch
			{
				// swallow all exceptions here
			}
			return "ASPNET";
		}
	}
}
