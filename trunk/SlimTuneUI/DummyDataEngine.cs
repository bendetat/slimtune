using System;

namespace SlimTuneUI
{
	public class DummyDataEngine : UICore.IDataEngine
	{
		#region IDataEngine Members

#pragma warning disable 0067
		public event EventHandler DataFlush;
		public event EventHandler DataClear;

		public string Name
		{
			get { return "Dummy Engine"; }
		}

		public string Extension
		{
			get { return "*"; }
		}

		public string Engine
		{
			get { return "Dummy"; }
		}

		public bool InMemory
		{
			get { return true; }
		}

		public System.Data.IDbConnection Connection
		{
			get { return null; }
		}

		public string GetProperty(string name)
		{
			throw new NotImplementedException();
		}

		public void WriteProperty(string name, string value)
		{
		}

		public void MapFunction(UICore.FunctionInfo funcInfo)
		{
		}

		public void MapClass(UICore.ClassInfo classInfo)
		{
		}

		public void ParseSample(UICore.Messages.Sample sample)
		{
		}

		public void ClearData()
		{
		}

		public void UpdateThread(int threadId, bool alive, string name)
		{
		}

		public void FunctionTiming(int functionId, long time)
		{
		}

		public void MapCounter(UICore.Counter counter)
		{
		}

		public void PerfCounter(int counterId, long time, double value)
		{
		}

		public void Flush()
		{
		}

		public void Snapshot(string name)
		{
		}

		public void Save(string file)
		{
		}

		public NHibernate.ISession OpenSession()
		{
			throw new NotImplementedException();
		}

		public NHibernate.ISession OpenSession(int snapshot)
		{
			throw new NotImplementedException();
		}

		public NHibernate.IStatelessSession OpenStatelessSession()
		{
			throw new NotImplementedException();
		}

		public System.Data.IDataReader SqlQuery(string query)
		{
			throw new NotImplementedException();
		}

		public object SqlScalar(string query)
		{
			throw new NotImplementedException();
		}

		public void SqlCommand(string query)
		{
			throw new NotImplementedException();
		}

		public void GarbageCollection(int generation, int function, long time)
		{
			throw new NotImplementedException();
		}

		public void ObjectAllocated(int classId, long size, int functionId, long time)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}
