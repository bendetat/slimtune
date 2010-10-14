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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace UICore
{
	public interface ILauncher
	{
		bool CheckParams();
		bool Launch();

		string Name { get; }
		ushort ListenPort { get; set; }
		bool RequiresAdmin { get; }
	}

	public class LauncherCommon
	{
		public const string ClrProfilerGuid = "{38A7EA35-B221-425a-AD07-D058C581611D}";

		//A lot of this is taken from the CLRProfiler code
		[DllImport("Advapi32.dll")]
		private static extern bool LookupAccountName(string machineName, string accountName, byte[] sid,
								 ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);

		[DllImport("Kernel32.dll")]
		private static extern bool LocalFree(IntPtr ptr);

		[DllImport("Advapi32.dll")]
		private static extern bool ConvertSidToStringSidW(byte[] sid, out IntPtr stringSid);

		[DllImport("Kernel32.dll")]
		private static extern IntPtr OpenProcess(
			uint dwDesiredAccess,  // access flag
			bool bInheritHandle,    // handle inheritance option
			int dwProcessId       // process identifier
			);

		[DllImport("Advapi32.dll")]
		private static extern bool OpenProcessToken(
			IntPtr ProcessHandle,
			uint DesiredAccess,
			ref IntPtr TokenHandle
			);

		[DllImport("UserEnv.dll")]
		private static extern bool CreateEnvironmentBlock(
				out IntPtr lpEnvironment,
				IntPtr hToken,
				bool bInherit);

		[DllImport("UserEnv.dll")]
		private static extern bool DestroyEnvironmentBlock(
				IntPtr lpEnvironment);

		public static string CreateConfigStringCLR(ProfilerMode profilingMode, int listenPort, bool waitForConnection, bool includeNative,
			int samplingInterval, int counterInterval, bool allowMethodInlining, bool trackGC, bool trackAllocs)
		{
			string config = string.Empty;
			config += string.Format("Mode={0};", (int) profilingMode);
			config += string.Format("Port={0};", listenPort);
			config += string.Format("Wait={0};", waitForConnection ? 1 : 0);
			config += string.Format("SampleUnmanaged={0};", includeNative ? 1 : 0);
			config += string.Format("SampleInterval={0};", samplingInterval);
			config += string.Format("CounterInterval={0};", counterInterval);
			config += string.Format("AllowInlining={0};", allowMethodInlining ? 1 : 0);
			config += string.Format("TrackGarbageCollections={0};", trackGC ? 1 : 0);
			config += string.Format("TrackObjectAllocations={0};", trackAllocs ? 1 : 0);

			return config;
		}

		public static string CreateConfigStringNative(ProfilerMode profilingMode, int listenPort, int samplingInterval, int counterInterval)
		{
			string config = string.Empty;
			config += string.Format("Mode={0};", (int) profilingMode);
			config += string.Format("Port={0};", listenPort);
			config += string.Format("SampleInterval={0};", samplingInterval);
			config += string.Format("CounterInterval={0};", counterInterval);

			return config;
		}

		public static void SetProcessOptions(ProcessStartInfo psi, string config, string counters, bool clr)
		{
			psi.RedirectStandardOutput = false;
			psi.RedirectStandardError = false;
			psi.RedirectStandardInput = false;
			psi.UseShellExecute = false;

			if(clr)
			{
				psi.EnvironmentVariables["COR_ENABLE_PROFILING"] = "1";
				psi.EnvironmentVariables["COR_PROFILER"] = ClrProfilerGuid;
			}

			psi.EnvironmentVariables["SLIMTUNE_CONFIG"] = config;
			psi.EnvironmentVariables["SLIMTUNE_COUNTERS"] = counters;
		}

		public static string[] CreateProfilerEnvironment(string config, string counters, bool clr)
		{
			if(clr)
			{
				return new string[]
				{
					"COR_ENABLE_PROFILING=1",
					"COR_PROFILER=" + ClrProfilerGuid,
					"SLIMTUNE_CONFIG=" + config,
					"SLIMTUNE_COUNTERS=" + counters,
				};
			}

			return new string[]
			{
				"SLIMTUNE_CONFIG=" + config,
				"SLIMTUNE_COUNTERS=" + counters,
			};
		}

		public static string[] CombineEnvironments(string[] envA, string[] envB)
		{
			string[] result = new string[envA.Length + envB.Length];
			Array.Copy(envA, result, envA.Length);
			Array.Copy(envB, 0, result, envA.Length, envB.Length);
			return result;
		}

		public static bool TestConnection(string host, ushort port, IDataEngine tempEngine)
		{
			try
			{
				var client = new ProfilerClient(host, port, tempEngine);
				return true;
			}
			catch(System.Net.Sockets.SocketException)
			{
				return false;
			}
		}

		public static ProcessStartInfo StartService(string serviceName, string startCommand)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
			processStartInfo.Arguments = "/c " + startCommand;
			return processStartInfo;
		}

		public static Microsoft.Win32.RegistryKey GetServiceKey(string serviceName)
		{
			try
			{
				return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName, true);
			}
			catch(Exception)
			{
				return null;
			}
		}

		public static void SetEnvironmentVariables(string serviceName, string[] environment)
		{
			Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
			if(key != null)
				key.SetValue("Environment", environment);
		}

		public static void DeleteEnvironmentVariables(string serviceName)
		{
			Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
			if(key != null)
				key.DeleteValue("Environment");
		}

		public static string GetServiceAccountName(string serviceName)
		{
			var key = GetServiceKey(serviceName);
			if(key == null)
				return null;

			return key.GetValue("ObjectName") as string;
		}

		public static string GetAccountSid(string accountName)
		{
			int sidLen = 0;
			byte[] sid = new byte[sidLen];
			int domainNameLen = 0;
			int peUse;
			StringBuilder domainName = new StringBuilder();
			LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse);

			sid = new byte[sidLen];
			domainName = new StringBuilder(domainNameLen);
			string stringSid = null;
			if(LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse))
			{
				IntPtr stringSidPtr;
				if(ConvertSidToStringSidW(sid, out stringSidPtr))
				{
					try
					{
						stringSid = Marshal.PtrToStringUni(stringSidPtr);
					}
					finally
					{
						LocalFree(stringSidPtr);
					}
				}
			}
			return stringSid;
		}

		public static void SetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
		{
			Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
			if(key != null)
			{
				foreach(string envVariable in profilerEnvironment)
				{
					key.SetValue(EnvKey(envVariable), EnvValue(envVariable));
				}
			}
		}

		public static void ResetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
		{
			Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
			if(key != null)
			{
				foreach(string envVariable in profilerEnvironment)
				{
					key.DeleteValue(EnvKey(envVariable));
				}
			}
		}

		private static Microsoft.Win32.RegistryKey GetAccountEnvironmentKey(string serviceAccountSid)
		{
			Microsoft.Win32.RegistryKey users = Microsoft.Win32.Registry.Users;
			return users.OpenSubKey(serviceAccountSid + @"\Environment", true);
		}

		private static string EnvKey(string envVariable)
		{
			int index = envVariable.IndexOf('=');
			Debug.Assert(index >= 0);
			return envVariable.Substring(0, index);
		}

		private static string EnvValue(string envVariable)
		{
			int index = envVariable.IndexOf('=');
			Debug.Assert(index >= 0);
			return envVariable.Substring(index + 1);
		}

		/// <summary>
		/// I honestly have no idea why this is necessary.
		/// </summary>
		/// <returns>The environment from services.exe, I guess?</returns>
		public static string[] GetServicesEnvironment()
		{
			Process[] servicesProcesses = Process.GetProcessesByName("services");
			if(servicesProcesses == null || servicesProcesses.Length != 1)
			{
				servicesProcesses = Process.GetProcessesByName("services.exe");
				if(servicesProcesses == null || servicesProcesses.Length != 1)
					return new string[0];
			}
			Process servicesProcess = servicesProcesses[0];
			IntPtr processHandle = OpenProcess(0x20400, false, servicesProcess.Id);
			if(processHandle == IntPtr.Zero)
				return new string[0];
			IntPtr tokenHandle = IntPtr.Zero;
			if(!OpenProcessToken(processHandle, 0x20008, ref tokenHandle))
				return new string[0];
			IntPtr environmentPtr = IntPtr.Zero;
			if(!CreateEnvironmentBlock(out environmentPtr, tokenHandle, false))
				return new String[0];

			List<string> envStrings = new List<string>();
			while(true)
			{
				string str = Marshal.PtrToStringUni(environmentPtr);
				if(str.Length == 0)
					break;

				envStrings.Add(str);
				long ptrVal = environmentPtr.ToInt64();
				ptrVal += str.Length * 2 + 2;
				environmentPtr = (IntPtr) ptrVal;
			}

			return envStrings.ToArray();
		}

		public static string[] ReplaceTempDir(string[] env, string newTempDir)
		{
			for(int i = 0; i < env.Length; i++)
			{
				if(env[i].StartsWith("TEMP="))
					env[i] = "TEMP=" + newTempDir;
				else if(env[i].StartsWith("TMP="))
					env[i] = "TMP=" + newTempDir;
			}
			return env;
		}

		public static bool UserIsAdmin()
		{
			bool isAdmin;
			try
			{
				WindowsIdentity user = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(user);
				isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch(Exception ex)
			{
				isAdmin = false;
				Console.WriteLine(ex.Message);
			}
			return isAdmin;
		}

		public static string GetCounterString(List<string> counters)
		{
			if(counters.Count == 0)
				return string.Empty;

			return string.Join(";", counters.ToArray()) + ";";
		}
	}
}
