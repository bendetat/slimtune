using System;
using System.Runtime.InteropServices;

namespace SlimTuneApi
{
	/// <summary>
	/// The main static class for the SLimTune API.
	/// </summary>
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

		/// <summary>
		/// Gets the version of the underlying profiler backend.
		/// </summary>
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

		/// <summary>
		/// Gets the version of the underlying profiler backend, as a string.
		/// </summary>
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

		/// <summary>
		/// True if the profiling API is available, false otherwise.
		/// </summary>
		/// <remarks>
		/// Always check this property before calling into the SlimTune API.
		/// If false is returned, other methods will crash.
		/// </remarks>
		public static bool Available
		{
			get
			{
				return m_available;
			}
		}

		[DllImport(DllName)]
		private static extern int IsProfilerConnected();

		/// <summary>
		/// True if a profiler frontend is currently connected and listening, false otherwise.
		/// </summary>
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
		private static extern void SetSamplerActive(int active);

		/// <summary>
		/// Gets or sets whether the sampling profiler is currently active.
		/// Can be used to control where the profiler actually takes measurements.
		/// </summary>
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

		/// <summary>
		/// Sets the name of a performance counter.
		/// </summary>
		/// <param name="counterId">The unique integer ID of the counter.</param>
		/// <param name="name">The name to use for this counter ID.</param>
		[DllImport(DllName)]
		public static extern void SetCounterName(int counterId, [MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport(DllName)]
		private static extern void WritePerfCounterInt(int counterId, long value);

		[DllImport(DllName)]
		private static extern void WritePerfCounterFloat(int counterId, double value);

		/// <summary>
		/// Writes an integer value for a custom performance counter.
		/// </summary>
		/// <param name="counterId">The unique integer ID of the counter.</param>
		/// <param name="value">The value to write for the counter.</param>
		/// <remarks>
		/// The value is stored as a fixed point value with three decimal places.
		/// Large values may be truncated.
		/// </remarks>
		public static void WritePerfCounter(int counterId, long value)
		{
			WritePerfCounterInt(counterId, value);
		}

		/// <summary>
		/// Writes an floating point value for a custom performance counter.
		/// </summary>
		/// <param name="counterId">The unique integer ID of the counter.</param>
		/// <param name="value">The value to write for the counter.</param>
		/// <remarks>
		/// The value is stored as a fixed point value with three decimal places.
		/// Values beyond the first five decimal places will be truncated, and large values may be truncated.
		/// </remarks>
		public static void WritePerfCounter(int counterId, double value)
		{
			WritePerfCounterFloat(counterId, value);
		}
	}
}
