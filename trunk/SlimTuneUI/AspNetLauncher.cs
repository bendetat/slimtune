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
	public class AspNetLauncher : ClrLauncherBase
	{
		[Browsable(false)]
		public override string Name
		{
			get { return "ASP.NET"; }
		}

		[Browsable(false)]
		public override bool RequiresAdmin
		{
			get { return true; }
		}

		public AspNetLauncher()
		{
			ListenPort = 3000;
			SamplingInterval = 10;
		}

		public override bool CheckParams()
		{
			var key = LauncherCommon.GetServiceKey("IISADMIN") ?? LauncherCommon.GetServiceKey("W3SVC") ?? LauncherCommon.GetServiceKey("WAS");
			if(key == null)
			{
				MessageBox.Show("Unable to find a compatible ASP.NET installation.", "ASP.NET Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			return base.CheckParams();
		}

		public override bool Launch()
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
