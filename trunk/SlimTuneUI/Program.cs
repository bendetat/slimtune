/*
* Copyright (c) 2007-2009 SlimDX Group
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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SlimTuneUI
{
	static class Program
	{
		public static void LoadPlugins()
		{
			string pluginsDir = Application.StartupPath + "\\Plugins\\";
			var plugins = Directory.GetFiles(pluginsDir, "*.dll", SearchOption.AllDirectories);
			foreach(var file in plugins)
			{
				try
				{
					Assembly.LoadFrom(file);
				}
				catch
				{
					//okay nevermind then
					continue;
				}
			}
		}

		private static IEnumerable<Type> GetTypeList(Type baseType)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach(var assembly in assemblies)
			{
				if(assembly.ReflectionOnly)
					continue;
				if(assembly.GlobalAssemblyCache)
					continue;

				foreach(var type in assembly.GetExportedTypes())
				{
					if(type == baseType)
						continue;
					if(baseType.IsAssignableFrom(type))
						yield return type;
				}
			}
		}

		public static IEnumerable<Type> GetVisualizers()
		{
			return GetTypeList(typeof(IVisualizer));
		}

		public static IEnumerable<Type> GetLaunchers()
		{
			return GetTypeList(typeof(ILauncher));
		}

        static void TimingsTest()
        {
            SqlServerCompactEngine engine = new SqlServerCompactEngine(Path.GetTempFileName(), true);
            
            //start generating random timings
            Random rand = new Random(5);
            for(int i = 0; i < 5000; ++i)
            {
                int id = rand.Next(10);
                long time = rand.Next(1000);
                engine.FunctionTiming(id, time);
            }
            engine.Flush();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static void Main()
		{
            //TimingsTest();
            //return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
		}
	}
}
