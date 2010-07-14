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
using System.Text;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Cfg;

namespace UICore
{
	public struct CallGraph<T>
	{
		//this is: ThreadId, ParentId, ChildId, HitCount
		public SortedList<int, SortedDictionary<int, SortedList<int, T>>> Graph;

		public static CallGraph<T> Create()
		{
			CallGraph<T> cg = new CallGraph<T>();
			cg.Graph = new SortedList<int, SortedDictionary<int, SortedList<int, T>>>(8);
			return cg;
		}
	}

	public abstract class DataEngineBase : IDataEngine
	{
		protected const string kCallsSchema = "(ThreadId INT NOT NULL, ParentId INT NOT NULL, ChildId INT NOT NULL, HitCount INT NOT NULL)";
		protected const string kSamplesSchema = "(ThreadId INT NOT NULL, FunctionId INT NOT NULL, HitCount INT NOT NULL)";

		private const int kFunctionCacheSize = 64;
		private const int kClassCacheSize = 32;

		private List<FunctionInfo> m_functionCache = new List<FunctionInfo>(kFunctionCacheSize);
		private List<ClassInfo> m_classCache = new List<ClassInfo>(kClassCacheSize);

		//Everything is stored sorted so that we can sprint through the database quickly
		protected CallGraph<int> m_calls = CallGraph<int>.Create();
		//this is: FunctionId, ThreadId, HitCount
		protected SortedDictionary<int, SortedList<int, int>> m_samples = new SortedDictionary<int, SortedList<int, int>>();

		protected volatile bool m_allowFlush = true;
		protected DateTime m_lastFlush = DateTime.Now;
		//we use this so we don't have to check DateTime.Now on every single sample
		protected int m_cachedSamples;

		protected ISessionFactory m_sessionFactory;
		protected Configuration m_config;
		protected IStatelessSession m_statelessSession;

		protected object m_lock = new object();

		public string Name
		{
			get;
			private set;
		}

		public abstract string Engine
		{
			get;
		}

		public abstract string Extension
		{
			get;
		}

		public abstract System.Data.IDbConnection Connection
		{
			get;
		}

		public virtual bool InMemory
		{
			get { return false; }
		}

		public bool AllowFlush
		{
			get { return m_allowFlush; }
			set
			{
				lock(m_lock)
				{
					m_allowFlush = value;
				}
			}
		}

		protected virtual void PreCreateSchema() { }
		protected abstract void PrepareCommands();
		protected abstract void DoFlush();
		public abstract void Save(string file);
		public abstract void Snapshot(string name);
		public abstract System.Data.DataSet RawQuery(string query);
		public abstract System.Data.DataSet RawQuery(string query, int limit);
		public abstract object RawQueryScalar(string query);
		protected abstract void DoClearData();

		public DataEngineBase(string name)
		{
			Name = name;
		}

		protected void FinishConstruct(bool createNew, FluentConfiguration config)
		{
			CreateSessionFactory(config);
			if(createNew)
			{
				PreCreateSchema();
				var export = new SchemaExport(m_config);
				export.Execute(true, true, false, Connection, Console.Out);
			}
			else
			{
				using(var session = OpenSession())
				{
					var crit = session.CreateCriteria<Property>();
					var versionProp = session.Get<Property>("FileVersion");
					if(versionProp == null || int.Parse(versionProp.Value) != 3)
					{
						throw new System.IO.InvalidDataException("Wrong file version.");
					}
				}
			}

			m_statelessSession = OpenStatelessSession();
			PrepareCommands();
			WriteCoreProperties();
		}

		private void CreateSessionFactory(FluentConfiguration config)
		{
			config.Mappings(m => m.FluentMappings.AddFromAssemblyOf<DataEngineBase>());
			m_config = config.BuildConfiguration();
			m_sessionFactory = config.BuildSessionFactory();
		}

		public virtual NHibernate.ISession OpenSession()
		{
			return m_sessionFactory.OpenSession(Connection);
		}

		public virtual NHibernate.IStatelessSession OpenStatelessSession()
		{
			return m_sessionFactory.OpenStatelessSession(Connection);
		}

		public void Flush()
		{
			lock(m_lock)
			{
				if(!m_allowFlush)
					return;

				using(var tx = m_statelessSession.BeginTransaction())
				{
					//flush functions
					foreach(var f in m_functionCache)
						m_statelessSession.Insert(f);
					m_functionCache.Clear();
					//flush classes
					foreach(var c in m_classCache)
						m_statelessSession.Insert(c);
					m_classCache.Clear();

					tx.Commit();
				}

				DoFlush();
			}
		}

		public void ParseSample(Messages.Sample sample)
		{
			lock(m_lock)
			{
				//Update calls
				SortedDictionary<int, SortedList<int, int>> perThread;
				bool foundThread = m_calls.Graph.TryGetValue(sample.ThreadId, out perThread);
				if(!foundThread)
				{
					perThread = new SortedDictionary<int, SortedList<int, int>>();
					m_calls.Graph.Add(sample.ThreadId, perThread);
				}

				Increment(sample.Functions[0], 0, perThread);
				for(int f = 1; f < sample.Functions.Count; ++f)
				{
					Increment(sample.Functions[f], sample.Functions[f - 1], perThread);
				}
				Increment(0, sample.Functions[sample.Functions.Count - 1], perThread);

				//Update overall samples count
				for(int s = 0; s < sample.Functions.Count; ++s)
				{
					int functionId = sample.Functions[s];
					bool first = true;

					//scan backwards to see if this was elsewhere in the stack and therefore already counted
					for(int r = s - 1; r >= 0; --r)
					{
						if(functionId == sample.Functions[r])
						{
							//yep, it was
							first = false;
							break;
						}
					}

					if(first)
					{
						//add the function if we don't have it yet
						if(!m_samples.ContainsKey(functionId))
							m_samples.Add(functionId, new SortedList<int, int>());

						//add this thread if we don't have it, else just increment
						if(!m_samples[functionId].ContainsKey(sample.ThreadId))
							m_samples[functionId].Add(sample.ThreadId, 1);
						else
							++m_samples[sample.Functions[s]][sample.ThreadId];
					}
				}

				++m_cachedSamples;
				if(m_cachedSamples > 2000)
				{
					Flush();
				}
			}
		}

		public void ClearData()
		{
			lock(m_lock)
			{
				foreach(KeyValuePair<int, SortedDictionary<int, SortedList<int, int>>> threadKvp in m_calls.Graph)
				{
					int threadId = threadKvp.Key;
					foreach(KeyValuePair<int, SortedList<int, int>> callerKvp in threadKvp.Value)
					{
						callerKvp.Value.Clear();
					}
				}
				m_lastFlush = DateTime.Now;
				m_samples.Clear();
				m_cachedSamples = 0;

				//m_timings.Clear();
				//m_cachedTimings = 0;

				DoClearData();
			}
		}

		public virtual void WriteProperty(string name, string value)
		{
			using(var session = OpenSession())
			using(var tx = session.BeginTransaction())
			{
				var prop = new Property() { Name = name, Value = value };
				session.SaveOrUpdateCopy(prop);
				tx.Commit();
			}
		}

		protected void WriteCoreProperties()
		{
			WriteProperty("Application", "SlimTune Profiler");
			WriteProperty("Version", System.Windows.Forms.Application.ProductVersion);
			WriteProperty("FileVersion", "3");
			WriteProperty("FileName", Name);
		}

		public virtual void FunctionTiming(int functionId, long time)
		{
			throw new NotImplementedException();
		}

		protected static void Increment(int key1, int key2, SortedDictionary<int, SortedList<int, int>> container)
		{
			SortedList<int, int> key1Table;
			bool foundKey1Table = container.TryGetValue(key1, out key1Table);
			if(!foundKey1Table)
			{
				key1Table = new SortedList<int, int>();
				container.Add(key1, key1Table);
			}

			if(!key1Table.ContainsKey(key2))
			{
				key1Table.Add(key2, 1);
			}
			else
			{
				++key1Table[key2];
			}
		}

		public virtual void MapFunction(FunctionInfo funcInfo)
		{
			m_functionCache.Add(funcInfo);
			if(m_functionCache.Count >= kFunctionCacheSize)
				Flush();
		}

		public virtual void MapClass(ClassInfo classInfo)
		{
			m_classCache.Add(classInfo);
			if(m_classCache.Count >= kClassCacheSize)
				Flush();
		}

		public virtual void UpdateThread(int threadId, bool alive, string name)
		{
			var ti = new ThreadInfo() { Id = threadId, IsAlive = alive, Name = name };
			using(var session = OpenSession())
			using(var tx = session.BeginTransaction())
			{
				session.SaveOrUpdateCopy(ti, ti.Id);
				tx.Commit();
			}
		}

		public virtual void CounterName(int counterId, string name)
		{
			using(var session = OpenSession())
			using(var tx = session.BeginTransaction())
			{
				var counter = new Counter() { Id = counterId, Name = name };
				session.SaveOrUpdateCopy(counter);
				tx.Commit();
			}
		}

		public virtual void PerfCounter(int counterId, long time, double value)
		{
			using(var tx = m_statelessSession.BeginTransaction())
			{
				var cv = new CounterValue()
				{
					CounterId = counterId,
					Time = time,
					Value = value
				};
				m_statelessSession.Insert(cv);
				tx.Commit();
			}
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			if(m_statelessSession != null)
				m_statelessSession.Dispose();
			if(m_sessionFactory != null)
				m_sessionFactory.Dispose();
		}

		#endregion
	}
}
