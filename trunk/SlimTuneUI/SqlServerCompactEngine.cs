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
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;

namespace SlimTuneUI
{
	struct CallGraph<T>
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

	class SqlServerCompactEngine : IStorageEngine
	{
		private const string kCallersSchema = "(ThreadId INT NOT NULL, CallerId INT NOT NULL, CalleeId INT NOT NULL, HitCount INT NOT NULL)";
		private const string kSamplesSchema = "(ThreadId INT NOT NULL, FunctionId INT NOT NULL, HitCount INT NOT NULL)";
		//Everything is stored sorted so that we can sprint through the database quickly
		CallGraph<int> m_callers;
		//this is: FunctionId, ThreadId, HitCount
		SortedDictionary<int, SortedList<int, int>> m_samples;
		
		//List<Pair<int, long>> m_timings;

		volatile bool m_allowFlush = true;
		DateTime m_lastFlush;
		//we use this so we don't have to check DateTime.Now on every single sample
		int m_cachedSamples;
		//int m_cachedTimings;

		SqlCeConnection m_sqlConn;

		SqlCeCommand m_addFunctionCmd;
		SqlCeCommand m_addClassCmd;

		SqlCeCommand m_callersCmd;
		SqlCeCommand m_samplesCmd;
		SqlCeCommand m_threadsCmd;
		SqlCeCommand m_timingsCmd;

		object m_lock = new object();

		public string Name { get; private set; }

		public bool AllowFlush
		{
			get { return m_allowFlush; }
			set { m_allowFlush = value; }
		}

		public SqlServerCompactEngine(string dbFile, bool createNew)
		{
			string connStr = "Data Source='" + dbFile + "'; LCID=1033;";
			if(createNew)
			{
				if(File.Exists(dbFile))
					File.Delete(dbFile);

				using(SqlCeEngine engine = new SqlCeEngine(connStr))
				{
					engine.CreateDatabase();
				}

				m_sqlConn = new SqlCeConnection(connStr);
				m_sqlConn.Open();
				CreateSchema();
			}
			else
			{
				m_sqlConn = new SqlCeConnection(connStr);
				m_sqlConn.Open();
			}

			Name = dbFile;

			CreateCommands();
			m_callers = CallGraph<int>.Create();
			m_samples = new SortedDictionary<int, SortedList<int, int>>();
			//m_timings = new List<Pair<int, long>>(8192);
			m_lastFlush = DateTime.Now;
		}

		public void MapFunction(FunctionInfo funcInfo)
		{
			var resultSet = m_addFunctionCmd.ExecuteResultSet(ResultSetOptions.Updatable);
			var row = resultSet.CreateRecord();
			row["Id"] = funcInfo.FunctionId;
			row["ClassId"] = funcInfo.ClassId != 0 ? (object) funcInfo.ClassId : null;
			row["IsNative"] = funcInfo.IsNative ? 1 : 0;
			row["Name"] = funcInfo.Name;
			row["Signature"] = funcInfo.Signature;
			resultSet.Insert(row);
		}

		public void MapClass(ClassInfo classInfo)
		{
			var resultSet = m_addClassCmd.ExecuteResultSet(ResultSetOptions.Updatable);
			var row = resultSet.CreateRecord();
			row["Id"] = classInfo.ClassId;
			row["Name"] = classInfo.Name;
			resultSet.Insert(row);
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

				new SqlCeCommand("UPDATE Callers SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
				new SqlCeCommand("UPDATE Samples SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
			}
		}

		public void UpdateThread(int threadId, bool? alive, string name)
		{
			lock(m_lock)
			{
				using(var resultSet = m_threadsCmd.ExecuteResultSet(ResultSetOptions.Updatable))
				{
					int isAliveOrdinal = resultSet.GetOrdinal("IsAlive");
					int nameOrdinal = resultSet.GetOrdinal("Name");

					if(!resultSet.Seek(DbSeekOptions.FirstEqual, threadId))
					{
						var threadRow = resultSet.CreateRecord();
						threadRow["Id"] = threadId;
						threadRow[nameOrdinal] = name;
						if(alive.HasValue)
							threadRow[isAliveOrdinal] = alive.Value ? 1 : 0;
						else
							threadRow[isAliveOrdinal] = null;

						resultSet.Insert(threadRow);
						return;
					}

					if(!resultSet.Read())
						return;

					if(alive.HasValue)
					{
						resultSet.SetInt32(isAliveOrdinal, alive.Value ? 1 : 0);
						resultSet.Update();
					}

					if(name != null)
					{
						resultSet.SetString(nameOrdinal, name);
						resultSet.Update();
					}
				}
			}
		}

		public void FunctionTiming(int functionId, long time)
		{
			/*m_timings.Add(new Pair<int, long>(functionId, time));

			++m_cachedTimings;
			if(m_cachedTimings > 5000)
				Flush();*/
		}

		public void Flush()
		{
			if(!AllowFlush)
				return;

			lock(m_lock)
			{
				Stopwatch timer = new Stopwatch();
				timer.Start();

				using(var callersSet = m_callersCmd.ExecuteResultSet(ResultSetOptions.Updatable | ResultSetOptions.Scrollable))
				{
					FlushCallers(callersSet);
				}

				using(var samplesSet = m_samplesCmd.ExecuteResultSet(ResultSetOptions.Updatable | ResultSetOptions.Scrollable))
				{
					FlushSamples(samplesSet);
				}

				m_lastFlush = DateTime.Now;
				m_cachedSamples = 0;
				//m_cachedTimings = 0;
				timer.Stop();
				Debug.WriteLine(string.Format("Database update took {0} milliseconds.", timer.ElapsedMilliseconds));
			}
		}

		public void Snapshot(string name)
		{
			lock(m_lock)
			{
				Flush();

				var cmd = new SqlCeCommand("INSERT INTO Snapshots (Name, DateTime) VALUES (@Name, @DateTime)", m_sqlConn);
				cmd.Parameters.Add(new SqlCeParameter("@Name", name));
				cmd.Parameters.Add(new SqlCeParameter("@DateTime", DateTime.Now));
				cmd.ExecuteNonQuery();

				int id = (int) QueryScalar("SELECT MAX(Id) FROM Snapshots");
				ExecuteNonQuery(string.Format("CREATE TABLE Callers_{0} {1}", id, kCallersSchema));
				ExecuteNonQuery(string.Format("INSERT INTO Callers_{0} SELECT * FROM Callers", id));
				ExecuteNonQuery(string.Format("CREATE TABLE Samples_{0} {1}", id, kSamplesSchema));
				ExecuteNonQuery(string.Format("INSERT INTO Samples_{0} SELECT * FROM Samples", id));
			}
		}

		public DataSet Query(string query)
		{
			var command = new SqlCeCommand(query, m_sqlConn);
			var adapter = new SqlCeDataAdapter(command);
			var ds = new DataSet();
			adapter.Fill(ds, "Query");
			return ds;
		}

		public object QueryScalar(string query)
		{
			var command = new SqlCeCommand(query, m_sqlConn);
			return command.ExecuteScalar();
		}

		public void Dispose()
		{
			if(m_sqlConn != null)
				m_sqlConn.Dispose();
		}

		private void ExecuteNonQuery(string query)
		{
			using(var command = new SqlCeCommand(query, m_sqlConn))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateSchema()
		{
			ExecuteNonQuery("CREATE TABLE Snapshots (Id INT PRIMARY KEY IDENTITY, Name NVARCHAR (256), DateTime DATETIME)");
			ExecuteNonQuery("CREATE TABLE Functions (Id INT PRIMARY KEY, ClassId INT, IsNative INT NOT NULL, Name NVARCHAR (1024), Signature NVARCHAR (2048))");
			ExecuteNonQuery("CREATE TABLE Classes (Id INT PRIMARY KEY, Name NVARCHAR (1024))");

			//We will look up results in CallerId order when updating this table
			ExecuteNonQuery("CREATE TABLE Callers " + kCallersSchema);
			ExecuteNonQuery("CREATE INDEX CallerIndex ON Callers(CallerId);");
			ExecuteNonQuery("CREATE INDEX CalleeIndex ON Callers(CalleeId);");
			ExecuteNonQuery("CREATE INDEX Compound ON Callers(ThreadId, CallerId, CalleeId);");

			ExecuteNonQuery("CREATE TABLE Samples " + kSamplesSchema);
			ExecuteNonQuery("CREATE INDEX FunctionIndex ON Samples(FunctionId);");
			ExecuteNonQuery("CREATE INDEX Compound ON Samples(ThreadId, FunctionId);");

			ExecuteNonQuery("CREATE TABLE Threads (Id INT NOT NULL, IsAlive INT, Name NVARCHAR(256))");
			ExecuteNonQuery("ALTER TABLE Threads ADD CONSTRAINT pk_Id PRIMARY KEY (Id)");
		}

		private void CreateCommands()
		{
			m_addFunctionCmd = m_sqlConn.CreateCommand();
			m_addFunctionCmd.CommandType = CommandType.TableDirect;
			m_addFunctionCmd.CommandText = "Functions";

			m_addClassCmd = m_sqlConn.CreateCommand();
			m_addClassCmd.CommandType = CommandType.TableDirect;
			m_addClassCmd.CommandText = "Classes";

			m_callersCmd = m_sqlConn.CreateCommand();
			m_callersCmd.CommandType = CommandType.TableDirect;
			m_callersCmd.CommandText = "Callers";
			m_callersCmd.IndexName = "Compound";

			m_samplesCmd = m_sqlConn.CreateCommand();
			m_samplesCmd.CommandType = CommandType.TableDirect;
			m_samplesCmd.CommandText = "Samples";
			m_samplesCmd.IndexName = "Compound";

			m_timingsCmd = m_sqlConn.CreateCommand();
			m_timingsCmd.CommandType = CommandType.TableDirect;
			m_timingsCmd.CommandText = "Timings";

			m_threadsCmd = m_sqlConn.CreateCommand();
			m_threadsCmd.CommandType = CommandType.TableDirect;
			m_threadsCmd.CommandText = "Threads";
			m_threadsCmd.IndexName = "pk_Id";
		}

		private void FlushCallers(SqlCeResultSet resultSet)
		{
			//a lock is already taken at this point
			int hitsOrdinal = resultSet.GetOrdinal("HitCount");
			int calleeOrdinal = resultSet.GetOrdinal("CalleeId");
			int callerOrdinal = resultSet.GetOrdinal("CallerId");
			int threadOrdinal = resultSet.GetOrdinal("ThreadId");

			foreach(KeyValuePair<int, SortedDictionary<int, SortedList<int, int>>> threadKvp in m_callers.Graph)
			{
				int threadId = threadKvp.Key;
				foreach(KeyValuePair<int, SortedList<int, int>> callerKvp in threadKvp.Value)
				{
					int callerId = callerKvp.Key;
					foreach(KeyValuePair<int, int> hitsKvp in callerKvp.Value)
					{
						int calleeId = hitsKvp.Key;
						int hits = hitsKvp.Value;

						bool result = resultSet.Seek(DbSeekOptions.FirstEqual, threadId, callerId, calleeId);
						if(result && resultSet.Read())
						{
							//found it, update the hit count and move on
							hits += (int) resultSet[hitsOrdinal];
							resultSet.SetInt32(hitsOrdinal, hits);
							resultSet.Update();
						}
						else
						{
							//not in the db, create a new record
							CreateRecord(resultSet, threadId, callerId, calleeId, hits);
						}
					}
					callerKvp.Value.Clear();
				}
			}
		}

		private void FlushSamples(SqlCeResultSet resultSet)
		{
			//now to update the samples table
			foreach(KeyValuePair<int, SortedList<int, int>> sampleKvp in m_samples)
			{
				if(sampleKvp.Value.Count == 0)
					continue;

				int threadOrdinal = resultSet.GetOrdinal("ThreadId");
				int functionOrdinal = resultSet.GetOrdinal("FunctionId");
				int hitsOrdinal = resultSet.GetOrdinal("HitCount");

				foreach(KeyValuePair<int, int> threadKvp in sampleKvp.Value)
				{
					if(!resultSet.Seek(DbSeekOptions.FirstEqual, threadKvp.Key, sampleKvp.Key))
					{
						//doesn't exist in the table, we need to add it
						var row = resultSet.CreateRecord();
						row[threadOrdinal] = threadKvp.Key;
						row[functionOrdinal] = sampleKvp.Key;
						row[hitsOrdinal] = threadKvp.Value;
						resultSet.Insert(row, DbInsertOptions.PositionOnInsertedRow);
					}
					else
					{
						resultSet.Read();
						resultSet.SetValue(hitsOrdinal, (int) resultSet[hitsOrdinal] + threadKvp.Value);
						resultSet.Update();
					}
				}

				sampleKvp.Value.Clear();
			}
		}

		private static void Increment(int key1, int key2, SortedDictionary<int, SortedList<int, int>> container)
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

		private void CreateRecord(SqlCeResultSet resultSet, int threadId, int callerId, int calleeId, int hits)
		{
			//a lock is not needed
			var row = resultSet.CreateRecord();
			row["ThreadId"] = threadId;
			row["CallerId"] = callerId;
			row["CalleeId"] = calleeId;
			row["HitCount"] = hits;
			resultSet.Insert(row, DbInsertOptions.PositionOnInsertedRow);
		}
	}
}
