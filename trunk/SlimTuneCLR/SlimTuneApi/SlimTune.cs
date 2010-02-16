using System;
using System.Runtime.InteropServices;

namespace SlimTuneApi
{
	public static class SlimTune
	{
		const string DllName = "SlimTuneCLR";

		static bool m_available;

		static SlimTune()
		{
			try
			{
				m_available = IsProfilerAvailable() != 0;
			}
			catch
			{
				m_available = false;
			}
		}

		[DllImport(DllName)]
		private static extern void GetProfilerVersion(out int major, out int minor, out int revision);

		public static Version Version
		{
			get
			{
				int major, minor, revision;
				GetProfilerVersion(out major, out minor, out revision);
				//uhhh...not too sure about this
				return new Version(major, minor, revision, 0);
			}
		}

		[DllImport(DllName)]
		private static extern IntPtr GetProfilerVersionString();

		public static string VersionString
		{
			get
			{
				return Marshal.PtrToStringAnsi(GetProfilerVersionString());
			}
		}

		//called in static ctor and cached
		[DllImport(DllName)]
		private static extern int IsProfilerAvailable();

		public static bool Available
		{
			get
			{
				return m_available;
			}
		}

		[DllImport(DllName)]
		private static extern int IsProfilerConnected();

		public static bool Connected
		{
			get
			{
				return IsProfilerConnected() != 0;
			}
		}

		/*[DllImport(DllName)]
		public static extern int GetProfilerMode();*/

		[DllImport(DllName)]
		private static extern int IsSamplerActive();

		[DllImport(DllName)]
		public static extern void SetSamplerActive(int active);

		public static bool SamplerActive
		{
			get
			{
				return IsSamplerActive() != 0;
			}

			set
			{
				SetSamplerActive(value ? 1 : 0);
			}
		}

		/*[DllImport(DllName)]
		public static extern void SetInstrument(int id, int enable);*/

		[DllImport(DllName)]
		public static extern void SetCounterName(int counterId, [MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport(DllName)]
		private static extern void WritePerfCounterInt(int counterId, long value);

		[DllImport(DllName)]
		private static extern void WritePerfCounterFloat(int counterId, double value);

		public static void WritePerfCounter(int counterId, long value)
		{
			WritePerfCounterInt(counterId, value);
		}

		public static void WritePerfCounter(int counterId, double value)
		{
			WritePerfCounterFloat(counterId, value);
		}
	}
}
