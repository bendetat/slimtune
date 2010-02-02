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
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace UICore
{
	public static class Plugins
	{
		public static void Load(string pluginsDir)
		{
			var plugins = Directory.GetFiles(pluginsDir, "*.dll", SearchOption.AllDirectories);
			foreach(var file in plugins)
			{
				try
				{
					Assembly.LoadFrom(file);
				}
				catch(Exception ex)
				{
					//okay nevermind then
					Console.WriteLine("Encountered exception while loading plugin library '{0}':", file);
					Console.WriteLine(ex.ToString());
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
	}
}
