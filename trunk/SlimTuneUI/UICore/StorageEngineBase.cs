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

namespace UICore
{
	public struct CallGraph<T>
	{
		//this is: ThreadId, CallerId, CalleeId, HitCount
		public SortedList<int, SortedDictionary<int, SortedList<int, T>>> Graph;

		public static CallGraph<T> Create()
		{
			CallGraph<T> cg = new CallGraph<T>();
			cg.Graph = new SortedList<int, SortedDictionary<int, SortedList<int, T>>>(8);
			return cg;
		}
	}

	public abstract class StorageEngineBase : IStorageEngine
	{
		protected const string kCallersSchema = "(ThreadId INT NOT NULL, CallerId INT NOT NULL, CalleeId INT NOT NULL, HitCount INT NOT NULL)";
		protected const string kSamplesSchema = "(ThreadId INT NOT NULL, FunctionId INT NOT NULL, HitCount INT NOT NULL)";

		//Everything is stored sorted so that we can sprint through the database quickly
		protected CallGraph<int> m_callers;
		//this is: FunctionId, ThreadId, HitCount
		protected SortedDictionary<int, SortedList<int, int>> m_samples;

		protected volatile bool m_allowFlush = true;
		protected DateTime m_lastFlush;
		//we use this so we don't have to check DateTime.Now on every single sample
		protected int m_cachedSamples;

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

		public virtual bool InMemory
		{
			get { return false; }
		}

		public bool AllowFlush
		{
			get { return m_allowFlush; }
			set { m_allowFlush = value; }
		}

		public abstract void MapFunction(FunctionInfo funcInfo);
		public abstract void MapClass(ClassInfo classInfo);
		public abstract void UpdateThread(int threadId, bool? alive, string name);
		public abstract void CounterName(int counterId, string name);
		public abstract void PerfCounter(int counterId, long time, long value);
		public abstract void Flush();
		public abstract void Save(string file);
		public abstract void Snapshot(string name);
		public abstract System.Data.DataSet Query(string query);
		public abstract object QueryScalar(string query);
		protected abstract void DoClearData();

		public StorageEngineBase(string name)
		{
			Name = name;
			m_callers = CallGraph<int>.Create();
			m_samples = new SortedDictionary<int, SortedList<int, int>>();
			m_lastFlush = DateTime.Now;
		}

		public void ParseSample(Messages.Sample sample)
		{
			lock(m_lock)
			{
				//Update callers
				SortedDictionary<int, SortedList<int, int>> perThread;
				bool foundThread = m_callers.Graph.TryGetValue(sample.ThreadId, out perThread);
				if(!foundThread)
				{
					perThread = new SortedDictionary<int, SortedList<int, int>>();
					m_callers.Graph.Add(sample.ThreadId, perThread);
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
				foreach(KeyValuePair<int, SortedDictionary<int, SortedList<int, int>>> threadKvp in m_callers.Graph)
				{
					int threadId = threadKvp.Key;
					foreach(KeyValuePair<int, SortedList<int, int>> callerKvp in threadKvp.Value)
					{
						callerKvp.Value.Clear();
					}
				}
				m_lastFlush = DateTime.Now;
				m_cachedSamples = 0;

				//m_timings.Clear();
				//m_cachedTimings = 0;

				DoClearData();
			}
		}

		public virtual void FunctionTiming(int functionId, long time)
		{
			throw new NotImplementedException();
		}

		protected static void Increment(int key1, int key2, SortedDictionary<int, SortedList<int, int>> container)
		{
			//a lock is already taken at this point
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

		#region IDisposable Members

		public abstract void Dispose();

		#endregion
	}
}
