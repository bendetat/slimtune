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
	[Serializable,
	DisplayName("CLR Application (Microsoft .NET 2.0, 4.0)")]
	public class ClrLauncher : ClrLauncherBase
	{
		//NOTE: These are all in order for the property grid
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor)),
		Category("Application"),
		Description("The path of the executable to launch for profiling.")]
		public string Executable { get; set; }

		[Browsable(false)]
		public override string Name
		{
			get { return Executable; }
		}

		[Browsable(false)]
		public override bool RequiresAdmin
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

		public ClrLauncher()
		{
			ListenPort = 3000;
			SamplingInterval = 5;
		}

		public override bool CheckParams()
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

			return base.CheckParams();
		}

		public override bool Launch()
		{
			string config = LauncherCommon.CreateConfigString(ProfilingMode, ListenPort, WaitForConnection, IncludeNative, SamplingInterval, CounterInterval, AllowMethodInlining);
			var psi = new ProcessStartInfo(Executable, Arguments);
			LauncherCommon.SetProcessOptions(psi, config, LauncherCommon.GetCounterString(PerformanceCounters));
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
