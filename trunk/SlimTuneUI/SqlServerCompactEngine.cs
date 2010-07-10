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

using System.Data.SQLite;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;

namespace SlimTuneUI
{
	[Obsolete,
	DisplayName("SQL Server Compact"),
	HandlesExtension("sdf")]
	public class SqlServerCompactEngine : DataEngineBase
	{
		private const int kTimingBuckets = 20;

		Dictionary<int, List<long>> m_timings;
		//int m_cachedTimings;

		SqlCeConnection m_sqlConn;

		SqlCeCommand m_callsCmd;
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
			}

			m_sqlConn = new SqlCeConnection(connStr);
			m_sqlConn.Open();

			var config = Fluently.Configure().Database(MsSqlCeConfiguration.Standard.ConnectionString(connStr));
			FinishConstruct(createNew, config);
			m_timings = new Dictionary<int, List<long>>(2048);
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

		public override void Flush()
		{
			if(!AllowFlush)
				return;

			lock(m_lock)
			{
				Stopwatch timer = new Stopwatch();
				timer.Start();

				using(var callsSet = m_callsCmd.ExecuteResultSet(ResultSetOptions.Updatable | ResultSetOptions.Scrollable))
				{
					FlushCalls(callsSet);
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

				int id = (int) RawQueryScalar("SELECT MAX(Id) FROM Snapshots");
				ExecuteNonQuery(string.Format("CREATE TABLE Calls_{0} {1}", id, kCallsSchema));
				ExecuteNonQuery(string.Format("INSERT INTO Calls_{0} SELECT * FROM Calls", id));
				ExecuteNonQuery(string.Format("CREATE TABLE Samples_{0} {1}", id, kSamplesSchema));
				ExecuteNonQuery(string.Format("INSERT INTO Samples_{0} SELECT * FROM Samples", id));
			}
		}

		public override NHibernate.ISession OpenSession()
		{
			return m_sessionFactory.OpenSession(m_sqlConn);
		}

		public override DataSet RawQuery(string query, int limit)
		{
			query = query.Substring(query.IndexOf("SELECT") + 6);
			query = string.Format("SELECT TOP ({0}) {1}", limit.ToString(), query);
			return RawQuery(query);
		}

		public override DataSet RawQuery(string query)
		{
			var command = new SqlCeCommand(query, m_sqlConn);
			var adapter = new SqlCeDataAdapter(command);
			var ds = new DataSet();
			adapter.Fill(ds, "Query");
			return ds;
		}

		public override object RawQueryScalar(string query)
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
			new SqlCeCommand("UPDATE Calls SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
			new SqlCeCommand("UPDATE Samples SET HitCount = 0", m_sqlConn).ExecuteNonQuery();
		}

		private int ExecuteNonQuery(string query)
		{
			using(var command = new SqlCeCommand(query, m_sqlConn))
			{
				return command.ExecuteNonQuery();
			}
		}

		protected override void PrepareCommands()
		{
			m_callsCmd = m_sqlConn.CreateCommand();
			m_callsCmd.CommandType = CommandType.TableDirect;
			m_callsCmd.CommandText = "Calls";
			//m_callsCmd.IndexName = "Compound";

			m_samplesCmd = m_sqlConn.CreateCommand();
			m_samplesCmd.CommandType = CommandType.TableDirect;
			m_samplesCmd.CommandText = "Samples";
			//m_samplesCmd.IndexName = "Compound";

			m_timingsCmd = m_sqlConn.CreateCommand();
			m_timingsCmd.CommandType = CommandType.TableDirect;
			m_timingsCmd.CommandText = "Timings";
			//m_timingsCmd.IndexName = "Compound";

			m_threadsCmd = m_sqlConn.CreateCommand();
			m_threadsCmd.CommandType = CommandType.TableDirect;
			m_threadsCmd.CommandText = "Threads";
			m_threadsCmd.IndexName = "pk_Id";
		}

		private void FlushCalls(SqlCeResultSet resultSet)
		{
			//a lock is already taken at this point
			int hitsOrdinal = resultSet.GetOrdinal("HitCount");
			int childOrdinal = resultSet.GetOrdinal("ChildId");
			int parentOrdinal = resultSet.GetOrdinal("ParentId");
			int threadOrdinal = resultSet.GetOrdinal("ThreadId");

			foreach(KeyValuePair<int, SortedDictionary<int, SortedList<int, int>>> threadKvp in m_calls.Graph)
			{
				int threadId = threadKvp.Key;
				foreach(KeyValuePair<int, SortedList<int, int>> parentKvp in threadKvp.Value)
				{
					int parentId = parentKvp.Key;
					foreach(KeyValuePair<int, int> hitsKvp in parentKvp.Value)
					{
						int childId = hitsKvp.Key;
						int hits = hitsKvp.Value;

						bool result = resultSet.Seek(DbSeekOptions.FirstEqual, threadId, parentId, childId);
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
							CreateRecord(resultSet, threadId, parentId, childId, hits);
						}
					}
					parentKvp.Value.Clear();
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

				#if FALSE
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
				#endif
			}
		}

		private void CreateRecord(SqlCeResultSet resultSet, int threadId, int parentId, int childId, int hits)
		{
			//a lock is not needed
			var row = resultSet.CreateRecord();
			row["ThreadId"] = threadId;
			row["ParentId"] = parentId;
			row["ChildId"] = childId;
			row["HitCount"] = hits;
			resultSet.Insert(row, DbInsertOptions.PositionOnInsertedRow);
		}
	}
}
