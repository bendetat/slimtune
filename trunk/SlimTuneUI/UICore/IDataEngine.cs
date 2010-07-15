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

using NHibernate;

namespace UICore
{
	public class TransactionHandle : IDisposable
	{
		IDataEngine m_engine;

		public TransactionHandle(IDataEngine engine)
		{
			if(engine == null)
				throw new ArgumentNullException("engine");

			m_engine = engine;
			m_engine.AllowFlush = false;
		}

		public void Dispose()
		{
			m_engine.AllowFlush = true;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class HandlesExtensionAttribute : Attribute
	{
		public string Extension { get; private set; }

		public HandlesExtensionAttribute(string ext)
		{
			Extension = ext;
		}
	}

	public interface IDataEngineCreator
	{
		bool CheckParams();
		IDataEngine CreateEngine();

		string Name { get; }
	}

	public interface IDataEngine : IDisposable
	{
		string Name { get; }
		string Extension { get; }
		string Engine { get; }
		bool InMemory { get; }
		IDbConnection Connection { get; }

		void WriteProperty(string name, string value);

		void MapFunction(FunctionInfo funcInfo);
		void MapClass(ClassInfo classInfo);

		void ParseSample(Messages.Sample sample);
		void ClearData();
		void UpdateThread(int threadId, bool alive, string name);

		void FunctionTiming(int functionId, long time);

		void CounterName(int counterId, string name);
		void PerfCounter(int counterId, long time, double value);

		void GarbageCollection(int generation, long time);

		bool AllowFlush { get; set; }
		void Flush();
		void Snapshot(string name);

		void Save(string file);

		ISession OpenSession();
		IStatelessSession OpenStatelessSession();
		ISession OpenSnapshot(int snapshot);

		DataSet RawQuery(string query);
		DataSet RawQuery(string query, int limit);
		object RawQueryScalar(string query);
	}
}
