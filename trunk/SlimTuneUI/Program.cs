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
using System.Windows.Forms;
using System.Runtime.InteropServices;

using UICore;
using NHibernate;
using NHibernate.Cfg;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;

namespace SlimTuneUI
{
	static class Program
	{
		const string DbFile = @"D:\Promit\Documents\Projects\SlimTune\trunk\SlimTuneUI\test.sqlite";
		private static ISessionFactory CreateSessionFactory()
		{
			return Fluently.Configure()
				.Database(SQLiteConfiguration.Standard.UsingFile(DbFile))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<FunctionInfo>())
				//.ExposeConfiguration(BuildSchema)
				.BuildSessionFactory();				
		}

		private static void BuildSchema(Configuration config)
		{
			if(File.Exists(DbFile))
				File.Delete(DbFile);

			new SchemaExport(config).Create(false, true);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if FALSE
			var factory = CreateSessionFactory();
			using(var session = factory.OpenSession())
			{
				//using(var transact = session.BeginTransaction())
				{
					var functions = session.CreateCriteria<FunctionInfo>().List<FunctionInfo>();
					/*var classes = session.CreateCriteria(typeof(ClassInfo)).List<ClassInfo>();
					foreach(var c in classes)
					{
						Console.WriteLine(c.Name);
						foreach(var f in c.Functions)
						{
							Console.WriteLine("\t" + f.Name);
						}
					}
					var threads = session.CreateCriteria<ThreadInfo>().List<ThreadInfo>();
					foreach(var t in threads)
					{
						Console.WriteLine("Thread #{0} has {1} samples.", t.Id, t.Samples.Count);
					}*/
				}
			}
#endif

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new SlimTune());
		}
	}
}
