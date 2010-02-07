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
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;

using UICore;

namespace SlimTuneUI
{
	[Obsolete,
	DisplayName("SQL Server Compact"),
	HandlesExtension("sdf")]
	public class SqlServerCompactEngine : StorageEngineBase
	{
		private const int kTimingBuckets = 20;

		Dictionary<int, List<long>> m_timings;
		//int m_cachedTimings;

		SqlCeConnection m_sqlConn;

		SqlCeCommand m_addFunctionCmd;
		SqlCeCommand m_addClassCmd;

		SqlCeCommand m_callersCmd;
		SqlCeCommand m_samplesCmd;
		SqlCeCommand m_threadsCmd;
		SqlCeCommand m_timingsCmd;

		public override string Extension
		{
			get { return "sdf"; }
		}

		public override string Engine
		{
			get { return "SQL Server Compact"; }
		}

		public SqlServerCompactEngine(string dbFile, bool createNew)
			: base(dbFile)
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

			CreateCommands();
			m_callers = CallGraph<int>.Create();
			m_samples = new SortedDictionary<int, SortedList<int, int>>();
			m_timings = new Dictionary<int, List<long>>(2048);
			m_lastFlush = DateTime.Now;
		}

		public override void MapFunction(FunctionInfo funcInfo)
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

		public override void MapClass(ClassInfo classInfo)
		{
			var resultSet = m_addClassCmd.ExecuteResultSet(ResultSetOptions.Updatable);
			var row = resultSet.CreateRecord();
			row["Id"] = classInfo.ClassId;
			row["Name"] = classInfo.Name;
			resultSet.Insert(row);
		}

		public override void UpdateThread(int threadId, bool? alive, string name)
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

		public override void FunctionTiming(int functionId, long time)
		{
			lock(m_lock)
			{
				if(!m_timings.ContainsKey(functionId))
					m_timings.Add(functionId, new List<long>(16));
				m_timings[functionId].Add(time);
			}
		}

		public override void CounterName(int counterId, string name)
		{
		}

		public override void PerfCounter(int counterId, long time, long value)
		{
		}

		public override void Flush()
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

				using(var timingsSet = m_timingsCmd.ExecuteResultSet(ResultSetOptions.Updatable | ResultSetOptions.Scrollable))
				{
					FlushTimings(timingsSet);
				}

				m_lastFlush = DateTime.Now;
				m_cachedSamples = 0;
				//m_cachedTimings = 0;
				timer.Stop();
				Debug.WriteLine(string.Format("Database update took {0} milliseconds.", timer.ElapsedMilliseconds));
			}
		}

		public override void Save(string file)
		{
			throw new NotImplementedException();
		}

		public override void Snapshot(string name)
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

		public override DataSet Query(string query)
		{
			var command = new SqlCeCommand(query, m_sqlConn);
			var adapter = new SqlCeDataAdapter(command);
			var ds = new DataSet();
			adapter.Fill(ds, "Query");
			return ds;
		}

		public override object QueryScalar(string query)
		{
			var command = new SqlCeCommand(query, m_sqlConn);
			return command.ExecuteScalar();
		}

		public override void Dispose()
		{
			if(m_sqlConn != null)
				m_sqlConn.Dispose();
		}

		protected override void DoClearData()
		{
			new SqlCeCommand("UPDATE Callers SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
			new SqlCeCommand("UPDATE Samples SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
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

			ExecuteNonQuery("CREATE TABLE Threads (Id INT NOT NULL, IsAlive INT, Name NVARCHAR(256))");
			ExecuteNonQuery("ALTER TABLE Threads ADD CONSTRAINT pk_Id PRIMARY KEY (Id)");

			//We will look up results in CallerId order when updating this table
			ExecuteNonQuery("CREATE TABLE Callers " + kCallersSchema);
			ExecuteNonQuery("CREATE INDEX CallerIndex ON Callers(CallerId);");
			ExecuteNonQuery("CREATE INDEX CalleeIndex ON Callers(CalleeId);");
			ExecuteNonQuery("CREATE INDEX Compound ON Callers(ThreadId, CallerId, CalleeId);");

			ExecuteNonQuery("CREATE TABLE Samples " + kSamplesSchema);
			ExecuteNonQuery("CREATE INDEX FunctionIndex ON Samples(FunctionId);");
			ExecuteNonQuery("CREATE INDEX Compound ON Samples(ThreadId, FunctionId);");

			ExecuteNonQuery("CREATE TABLE Timings (FunctionId INT, RangeMin BIGINT, RangeMax BIGINT, HitCount INT)");
			ExecuteNonQuery("CREATE INDEX FunctionIndex ON Timings(FunctionId);");
			ExecuteNonQuery("CREATE INDEX Compound ON Timings(FunctionId, RangeMin);");
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
			m_timingsCmd.IndexName = "Compound";

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

		private void FlushTimings(SqlCeResultSet resultSet)
		{
			foreach(KeyValuePair<int, List<long>> timingKvp in m_timings)
			{
				if(timingKvp.Value.Count == 0)
					continue;

				int funcOrdinal = resultSet.GetOrdinal("FunctionId");
				int minOrdinal = resultSet.GetOrdinal("RangeMin");
				int maxOrdinal = resultSet.GetOrdinal("RangeMax");
				int hitsOrdinal = resultSet.GetOrdinal("HitCount");

				for(int t = 0; t < timingKvp.Value.Count; ++t)
				{
					bool foundBin = true;
					long time = timingKvp.Value[t];
					if(!resultSet.Seek(DbSeekOptions.BeforeEqual, timingKvp.Key, time))
					{
						foundBin = false;
					}

					if(foundBin)
					{
						resultSet.Read();
						var id = resultSet.GetInt32(funcOrdinal);
						if(id != timingKvp.Key)
						{
							if(!resultSet.Read())
							{
								foundBin = false;
							}
						}

						if(foundBin)
						{
							var min = resultSet.GetInt64(minOrdinal);
							var max = resultSet.GetInt64(maxOrdinal);
							if(id != timingKvp.Key || time < min || time > max)
								foundBin = false;
						}
					}

					if(foundBin)
					{
						//we've got a usable bin, increment and move on
						var hits = resultSet.GetInt32(hitsOrdinal);
						resultSet.SetInt32(hitsOrdinal, hits + 1);
						resultSet.Update();
						continue;
					}

					//didn't find a bin, create a new one for this entry
					var row = resultSet.CreateRecord();
					row[funcOrdinal] = timingKvp.Key;
					row[minOrdinal] = time;
					row[maxOrdinal] = time;
					row[hitsOrdinal] = 1;
					resultSet.Insert(row, DbInsertOptions.KeepCurrentPosition);

					//we need to bin-merge

					//start by seeking to the first record for this function
					if(!resultSet.Seek(DbSeekOptions.BeforeEqual, timingKvp.Key, 0.0f))
						resultSet.ReadFirst();
					else
						resultSet.Read();

					var mergeId = resultSet.GetInt32(funcOrdinal);
					if(mergeId != timingKvp.Key)
						resultSet.Read();
					mergeId = resultSet.GetInt32(funcOrdinal);
					//we know at least one exists, cause we just inserted one
					Debug.Assert(mergeId == timingKvp.Key);

					//Search for the merge that produces the smallest merged bucket
					long lastMin = resultSet.GetInt64(minOrdinal);
					int lastHits = resultSet.GetInt32(hitsOrdinal);
					bool shouldMerge = resultSet.Read();
					//these store all the data about the best merge so far
					long smallestRange = long.MaxValue;
					long bestMin = 0;
					long bestMax = 0;
					int mergedHits = 0;
					for(int b = 0; b < kTimingBuckets && shouldMerge; ++b)
					{
						long max = resultSet.GetInt64(maxOrdinal);
						long range = max - lastMin;
						if(range < smallestRange)
						{
							smallestRange = range;
							bestMin = lastMin;
							bestMax = max;
							mergedHits = lastHits + resultSet.GetInt32(hitsOrdinal);
						}
						lastMin = resultSet.GetInt64(minOrdinal);
						lastHits = resultSet.GetInt32(hitsOrdinal);
						//if this read fails, we have insufficient buckets to bother merging
						shouldMerge = resultSet.Read();
					}

					if(shouldMerge)
					{
						//seek to the first (lower) bin
						resultSet.Seek(DbSeekOptions.FirstEqual, timingKvp.Key, bestMin);
						resultSet.Read();
						//expand this bin to include the next one
						resultSet.SetInt64(maxOrdinal, bestMax);
						resultSet.SetInt32(hitsOrdinal, mergedHits);
						//go to the now redundant bin
						resultSet.Update();
						resultSet.Read();
						//delete the bin
						resultSet.Delete();
					}
				}

				/*#if DEBUG
								//DEBUG ONLY HACK: display buckets
								if(!resultSet.Seek(DbSeekOptions.BeforeEqual, timingKvp.Key, 0.0f))
									resultSet.ReadFirst();
								else
									resultSet.Read();

								var tempId = resultSet.GetInt32(funcOrdinal);
								if(tempId != timingKvp.Key)
									resultSet.Read();

								Console.WriteLine("Buckets for function {0}:", timingKvp.Key);
								for(int b = 0; b < kTimingBuckets; ++b)
								{
									long min = resultSet.GetInt64(minOrdinal);
									long max = resultSet.GetInt64(maxOrdinal);
									int hits = resultSet.GetInt32(hitsOrdinal);
									Console.WriteLine("[{0}, {1}]: {2}", min, max, hits);
									resultSet.Read();
								}
				#endif*/
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
