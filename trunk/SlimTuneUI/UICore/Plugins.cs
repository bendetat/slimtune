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
