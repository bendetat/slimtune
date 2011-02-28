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
using System.Data;
using System.Collections.Generic;
using System.Text;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

namespace UICore
{
	public struct AllocData
	{
		public long Size;
		public int Count;

		public void Add(long size)
		{
			++Count;
			Size += size;
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
		protected Dictionary<long, Call> m_calls = new Dictionary<long, Call>();
		//this is: FunctionId, ThreadId, HitCount
		protected Dictionary<long, Sample> m_samples = new Dictionary<long, Sample>();

		//this is: ClassId, FunctionId, AllocData
		protected SortedDictionary<int, SortedDictionary<int, AllocData>> m_allocs = new SortedDictionary<int, SortedDictionary<int, AllocData>>();

		private DateTime m_lastFlush = DateTime.Now;
		//we use this so we don't have to check DateTime.Now on every single sample
		protected int m_cachedSamples;

		private ISessionFactory m_sessionFactory;
		private IPersistenceConfigurer m_configurer;
		private Configuration m_config;
		private IStatelessSession m_statelessSession;
		private Dictionary<int, ISessionFactory> m_snapshotFactories = new Dictionary<int, ISessionFactory>(8);

		protected object m_lock = new object();

		public event EventHandler DataFlush;
		public event EventHandler DataClear;

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

		private bool ShouldFlush
		{
			get
			{
				var span = DateTime.Now - m_lastFlush;
				if(span.TotalSeconds > 5.0)
					return true;
				return false;
			}
		}

		protected virtual void PreCreateSchema() { }
		protected abstract void PrepareCommands();
		protected abstract void DoFlush();
		public abstract void Save(string file);

		public IDataReader SqlQuery(string query)
		{
			using(var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query;
				return cmd.ExecuteReader();
			}
		}

		public object SqlScalar(string query)
		{
			using(var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query;
				return cmd.ExecuteScalar();
			}
		}

		public void SqlCommand(string query)
		{
			using(var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query;
				cmd.ExecuteNonQuery();
			}
		}

		public DataEngineBase(string name)
		{
			Name = name;
		}

		protected void FinishConstruct(bool createNew, IPersistenceConfigurer configurer)
		{
			m_configurer = configurer;
			var fluentConfig = Fluently.Configure().Database(configurer)
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<DataEngineBase>());
			m_config = fluentConfig.BuildConfiguration();
			m_sessionFactory = m_config.BuildSessionFactory();
			m_statelessSession = OpenStatelessSession();

			if(createNew)
			{
				PreCreateSchema();
				var export = new SchemaExport(m_config);
				export.Execute(true, true, false, Connection, Console.Out);

				WriteCoreProperties();
				//create an entry for the first snapshot in the db
				var firstSnapshot = m_statelessSession.CreateSQLQuery("INSERT INTO Snapshots (Id, Name, DateTime) VALUES (:id, :name, :datetime)")
					.SetInt32("id", 0)
					.SetString("name", "Current")
					.SetInt64("datetime", DateTime.Now.ToFileTime());
				firstSnapshot.ExecuteUpdate();
			}
			else
			{
				using(var session = OpenSession())
				using(var tx = session.BeginTransaction())
				{
					var applicationProp = session.Get<Property>("Application");
					if(applicationProp == null || applicationProp.Value != System.Windows.Forms.Application.ProductName)
					{
						throw new System.IO.InvalidDataException("Wrong or missing application name.");
					}

					var versionProp = session.Get<Property>("FileVersion");
					if(versionProp == null || int.Parse(versionProp.Value) != 3)
					{
						throw new System.IO.InvalidDataException("Wrong file version.");
					}

					tx.Commit();
				}
			}

			PrepareCommands();
		}

		public virtual NHibernate.ISession OpenSession()
		{
			return m_sessionFactory.OpenSession(Connection);
		}

		public virtual NHibernate.ISession OpenSession(int snapshot)
		{
			var session = m_sessionFactory.OpenSession(Connection);
			session.EnableFilter("Snapshot").SetParameter("snapshotId", snapshot);
			return session;
		}

		public virtual NHibernate.IStatelessSession OpenStatelessSession()
		{
			return m_sessionFactory.OpenStatelessSession(Connection);
		}

		public void Flush()
		{
			lock(m_lock)
			{
				using(var tx = m_statelessSession.BeginTransaction())
				{
					//update the timestamp on the current snapshot
					m_statelessSession.CreateQuery("update Snapshot set TimeStamp = :timeStamp where Id = 0")
						.SetInt64("timeStamp", DateTime.Now.ToFileTime())
						.ExecuteUpdate();

					//flush functions
					foreach(var f in m_functionCache)
					{
						try
						{
							m_statelessSession.Insert(f);
						}
						catch(NHibernate.Exceptions.GenericADOException)
						{
							//this can happen with duplicates, which should not be possible with the caching in ProfilerClient
							//that may change, though, and we want to be robust to this particular failure
						}
					}
					m_functionCache.Clear();

					//flush classes
					foreach(var c in m_classCache)
					{
						try
						{
							m_statelessSession.Insert(c);
						}
						catch(NHibernate.Exceptions.GenericADOException)
						{
							//see above
						}
					}
					m_classCache.Clear();

					tx.Commit();
				}

				DoFlush();
				m_lastFlush = DateTime.Now;

				Utilities.FireEvent(this, DataFlush);
			}
		}

		public void ParseSample(Messages.Sample sample)
		{
			lock(m_lock)
			{
				//Update calls
				Increment(sample.ThreadId, sample.Functions[0], 0, m_calls, sample.Time);
				for(int f = 1; f < sample.Functions.Count; ++f)
				{
					Increment(sample.ThreadId, sample.Functions[f], sample.Functions[f - 1], m_calls, sample.Time);
				}
				Increment(sample.ThreadId, 0, sample.Functions[sample.Functions.Count - 1], m_calls, sample.Time);

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
						long key = Sample.ComputeKey(sample.ThreadId, functionId);
						//add the function if we don't have it yet
						if(!m_samples.ContainsKey(key))
							m_samples.Add(key, new Sample { ThreadId = sample.ThreadId, FunctionId = functionId, Time = sample.Time });
						else
							m_samples[key].Time += sample.Time;
					}
				}

				++m_cachedSamples;
				if(m_cachedSamples > 2000 || ShouldFlush)
				{
					Flush();
				}
			}
		}

		public void Snapshot(string name)
		{
			lock(m_lock)
			{
				Flush();

				using(var session = OpenSession())
				using(var tx = session.BeginTransaction(IsolationLevel.Serializable))
				{
					Snapshot snapshot = new Snapshot();
					snapshot.Name = name;
					snapshot.TimeStamp = DateTime.Now.ToFileTime();
					session.Save(snapshot);

					string sampleQuery = string.Format("insert into Sample (ThreadId, FunctionId, Time, SnapshotId) select s.ThreadId, s.FunctionId, s.Time, {0} from Sample s where SnapshotId = 0", snapshot.Id);
					session.CreateQuery(sampleQuery).ExecuteUpdate();
					string callQuery = string.Format("insert into Call (ThreadId, ParentId, ChildId, Time, SnapshotId) select ThreadId, ParentId, ChildId, Time, {0} from Call where SnapshotId = 0", snapshot.Id);
					session.CreateQuery(callQuery).ExecuteUpdate();

					tx.Commit();
				}
			}
		}

		public void ClearData()
		{
			lock(m_lock)
			{
				m_calls.Clear();
				m_lastFlush = DateTime.Now;
				m_samples.Clear();
				m_cachedSamples = 0;

				using(var session = OpenSession())
				using(var tx = session.BeginTransaction(IsolationLevel.Serializable))
				{
					session.CreateQuery("delete from Call where SnapshotId = 0").ExecuteUpdate();
					session.CreateQuery("delete from Sample where SnapshotId = 0").ExecuteUpdate();
					tx.Commit();
				}

				Utilities.FireEvent(this, DataClear);
			}
		}

		public virtual void WriteProperty(string name, string value)
		{
			using(var session = OpenSession())
			using(var tx = session.BeginTransaction())
			{
				WriteProperty(session, name, value);
				tx.Commit();
			}
		}

		public virtual void WriteProperty(ISession session, string name, string value)
		{
			var prop = new Property() { Name = name, Value = value };
			session.SaveOrUpdateCopy(prop);
		}

		public virtual string GetProperty(string name)
		{
			using(var session = OpenSession())
			{
				var prop = session.Get<Property>(name);
				if(prop == null)
					return null;

				return prop.Value;
			}
		}

		protected void WriteCoreProperties()
		{
			using(var session = OpenSession(0))
			using(var tx = session.BeginTransaction(IsolationLevel.Serializable))
			{
				WriteProperty(session, "Application", System.Windows.Forms.Application.ProductName);
				WriteProperty(session, "Version", System.Windows.Forms.Application.ProductVersion);
				WriteProperty(session, "FileVersion", "3");
				WriteProperty(session, "FileName", Name);

				tx.Commit();
			}
		}

		public virtual void FunctionTiming(int functionId, long time)
		{
			throw new NotImplementedException();
		}

		protected static void Increment(int threadId, int parentId, int childId, Dictionary<long, Call> container, double time)
		{
			long key = Call.ComputeKey(threadId, parentId, childId);
			if(container.ContainsKey(key))
				container[key].Time += time;
			else
				container.Add(key, new Call { ThreadId = threadId, ParentId = parentId, ChildId = childId, Time = time });
		}

		public virtual void MapFunction(FunctionInfo funcInfo)
		{
			m_functionCache.Add(funcInfo);
			if(m_functionCache.Count >= kFunctionCacheSize || ShouldFlush)
				Flush();
		}

		public virtual void MapClass(ClassInfo classInfo)
		{
			m_classCache.Add(classInfo);
			if(m_classCache.Count >= kClassCacheSize || ShouldFlush)
				Flush();
		}

		public virtual void UpdateThread(int threadId, bool alive, string name)
		{
			var ti = new ThreadInfo() { Id = threadId, IsAlive = alive, Name = name };
			using(var session = OpenSession(0))
			using(var tx = session.BeginTransaction())
			{
				session.SaveOrUpdateCopy(ti, ti.Id);
				tx.Commit();
			}
		}

		public virtual void MapCounter(Counter counter)
		{
			using(var session = OpenSession(0))
			using(var tx = session.BeginTransaction())
			{
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

		public virtual void ObjectAllocated(int classId, long size, int functionId, long time)
		{
			SortedDictionary<int, AllocData> byFunc;
			if(m_allocs.ContainsKey(classId))
			{
				byFunc = m_allocs[classId];
			}
			else
			{
				byFunc = new SortedDictionary<int, AllocData>();
				m_allocs.Add(classId, byFunc);
			}

			if(byFunc.ContainsKey(functionId))
				byFunc[functionId].Add(size);
			else
				byFunc.Add(functionId, new AllocData() { Count = 1, Size = size });
		}

		public virtual void GarbageCollection(int generation, int function, long time)
		{
			using(var tx = m_statelessSession.BeginTransaction())
			{
				var gc = new GarbageCollection()
				{
					Generation = generation,
					FunctionId = function,
					Time = time
				};
				m_statelessSession.Insert(gc);
				tx.Commit();
			}

			Flush();
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
