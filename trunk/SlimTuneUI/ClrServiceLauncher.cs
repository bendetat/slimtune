using System;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Runtime.InteropServices;

using UICore;

namespace SlimTuneUI
{
	[Serializable,
	DisplayName("CLR Service (Microsoft .NET 2.0)")]
	class ClrServiceLauncher : ILauncher
	{
		[Category("Service"),
		DisplayName("Service name"),
		Description("The name of the service to be profiled.")]
		public string Name { get; set; }

		[Category("Service"),
		DisplayName("Start command"),
		Description("The command used to start the service.")]
		public string StartCommand { get; set; }

		[Category("Service"),
		DisplayName("Stop command"),
		Description("The command used to stop the service.")]
		public string StopCommand { get; set; }

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

		public ClrServiceLauncher()
		{
			ListenPort = 3000;
		}

		public bool CheckParams()
		{
			return false;
		}

		public bool Launch()
		{
			string serviceAccountSid = string.Empty;
			string serviceAccountName = GetServiceAccountName(Name);
			if(serviceAccountName.StartsWith(@".\"))
				serviceAccountName = Environment.MachineName + serviceAccountName.Substring(1);
			if(serviceAccountName != null && serviceAccountName != "LocalSystem")
			{
				serviceAccountSid = GetAccountSid(serviceAccountName);
			}

			if(serviceAccountSid != null)
			{
				//set environment for target account
			}

			return false;
		}

		//Essentially a replica of the CLRProfiler code
		[DllImport("Advapi32.dll")]
		private static extern bool LookupAccountName(string machineName, string accountName, byte[] sid,
								 ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);

		[DllImport("Kernel32.dll")]
		private static extern bool LocalFree(IntPtr ptr);

		[DllImport("Advapi32.dll")]
		private static extern bool ConvertSidToStringSidW(byte[] sid, out IntPtr stringSid);

		private static Microsoft.Win32.RegistryKey GetServiceKey(string serviceName)
		{
			return Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + serviceName, true);
		}

		private static string GetServiceAccountName(string serviceName)
		{
			var key = GetServiceKey(serviceName);
			if(key == null)
				return null;

			return key.GetValue("ObjectName") as string;
		}

		private static string GetAccountSid(string accountName)
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
            if (LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse))
            {
                IntPtr stringSidPtr;
                if (ConvertSidToStringSidW(sid, out stringSidPtr))
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
	}
}
