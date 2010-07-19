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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;

using UICore;

using System.Data.SQLite;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;

namespace SlimTuneUI
{
	[DisplayName("SQLite"),
	HandlesExtension("sqlite")]
	public class SQLiteEngine : DataEngineBase
	{
		SQLiteConnection m_database;

		SQLiteCommand m_insertCallerCmd;
		SQLiteCommand m_updateCallerCmd;
		SQLiteCommand m_insertSampleCmd;
		SQLiteCommand m_updateSampleCmd;

		SQLiteCommand m_insertAllocCmd;
		SQLiteCommand m_updateAllocCmd;

		public override string Extension
		{
			get { return "sqlite"; }
		}

		public override string Engine
		{
			get { return "SQLite"; }
		}

		public override IDbConnection Connection
		{
			get { return m_database; }
		}

		public SQLiteEngine()
			: base("memory")
		{
			string connStr = "Data Source=:memory:;";
			m_database = new SQLiteConnection(connStr);
			m_database.Open();

			var dbconfig = SQLiteConfiguration.Standard.ConnectionString(connStr);
			FinishConstruct(true, dbconfig);
		}

		public SQLiteEngine(string name, bool createNew)
			: base(name)
		{
			string connStr = string.Format("Data Source={0}; Synchronous=Off;", Name);
			if(createNew)
			{
				if(File.Exists(name))
					File.Delete(name);
			}
			else
			{
				if(!File.Exists(name))
					throw new InvalidOperationException();
			}

			m_database = new SQLiteConnection(connStr);
			m_database.Open();

			var dbconfig = SQLiteConfiguration.Standard.ConnectionString(connStr);
			//dbconfig.ShowSql();
			FinishConstruct(createNew, dbconfig);
		}

		~SQLiteEngine()
		{
			Dispose();
		}

		protected override void DoFlush()
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			int queryCount = 0;

			using(SQLiteTransaction transact = m_database.BeginTransaction())
			{
				queryCount += FlushCalls();
				queryCount += FlushSamples();
				queryCount += FlushAllocations();
				transact.Commit();
			}

			m_cachedSamples = 0;
			//m_cachedTimings = 0;
			timer.Stop();
			Debug.WriteLine(string.Format("Database update took {0} milliseconds for {1} queries.", timer.ElapsedMilliseconds, queryCount));
		}

		public override void Save(string file)
		{
			/*lock(m_lock)
			{
				Flush();
				m_database.Backup(file);
			}*/
		}

		public override void Snapshot(string name)
		{
			lock(m_lock)
			{
				Flush();

				var cmd = CreateCommand("INSERT INTO Snapshots (Name, DateTime) VALUES (?, ?)", 2);
				cmd.Parameters[0].Value = name;
				cmd.Parameters[1].Value = DateTime.Now.ToFileTime();
				cmd.ExecuteNonQuery();

				int id = Convert.ToInt32(RawQueryScalar("SELECT MAX(Id) FROM Snapshots"));
				/*Command(string.Format("CREATE TABLE Calls_{0} {1}", id, kCallsSchema));
				Command(string.Format("INSERT INTO Calls_{0} SELECT * FROM Callers", id));
				Command(string.Format("CREATE TABLE Samples_{0} {1}", id, kSamplesSchema));
				Command(string.Format("INSERT INTO Samples_{0} SELECT * FROM Samples", id));*/
				Command(string.Format("CREATE TABLE Calls_{0} AS SELECT * FROM Calls", id));
				Command(string.Format("CREATE TABLE Samples_{0} AS SELECT * FROM Samples", id));
			}
		}

		public override DataSet RawQuery(string query, int limit)
		{
			query += "\nLIMIT " + limit.ToString();
			return RawQuery(query);
		}

		public static DataSet ConvertDataReaderToDataSet(SQLiteDataReader reader)
		{
			DataSet dataSet = new DataSet();
			do
			{
				// Create new data table
				DataTable schemaTable = reader.GetSchemaTable();
				DataTable dataTable = new DataTable("Query");
				if(schemaTable != null)
				{
					// A query returning records was executed
					for(int i = 0; i < schemaTable.Rows.Count; i++)
					{
						DataRow dataRow = schemaTable.Rows[i];
						// Create a column name that is unique in the data table
						string columnName = (string) dataRow["ColumnName"];
						// Add the column definition to the data table
						DataColumn column = new DataColumn(columnName, (Type) dataRow["DataType"]);
						dataTable.Columns.Add(column);
					}
					dataSet.Tables.Add(dataTable);
					// Fill the data table we just created
					while(reader.Read())
					{
						DataRow dataRow = dataTable.NewRow();
						for(int i = 0; i < reader.FieldCount; i++)
							dataRow[i] = reader.GetValue(i);
						dataTable.Rows.Add(dataRow);
					}
				}
				else
				{
					// No records were returned
					DataColumn column = new DataColumn("RowsAffected");
					dataTable.Columns.Add(column);
					dataSet.Tables.Add(dataTable);
					DataRow dataRow = dataTable.NewRow();
					dataRow[0] = reader.RecordsAffected;
					dataTable.Rows.Add(dataRow);
				}
			} while(reader.NextResult());

			return dataSet;
		}
		
		public override DataSet RawQuery(string query)
		{
			using(SQLiteCommand cmd = new SQLiteCommand(query, m_database))
			{
				var reader = cmd.ExecuteReader();
				return ConvertDataReaderToDataSet(reader);
			}
		}

		public override object RawQueryScalar(string query)
		{
			using(SQLiteCommand cmd = new SQLiteCommand(query, m_database))
			{
				return cmd.ExecuteScalar();
			}
		}

		protected override void DoClearData()
		{
			Command("UPDATE Calls SET HitCount = 0");
			Command("UPDATE Samples SET HitCount = 0");
			Command("DELETE FROM CounterValues");
			Command("DELETE FROM Counters");
		}

		private int Command(string command)
		{
			using(SQLiteCommand cmd = new SQLiteCommand(command, m_database))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		protected override void PreCreateSchema()
		{
			//Command("PRAGMA count_changes=TRUE");
			Command("PRAGMA synchronous=OFF");
			Command("PRAGMA journal_mode=MEMORY");
		}

		private SQLiteCommand CreateCommand(string commandText, int paramCount)
		{
			SQLiteCommand command = new SQLiteCommand(commandText, m_database);
			for(int i = 0; i < paramCount; ++i)
				command.Parameters.Add(new SQLiteParameter());
			return command;
		}

		protected override void PrepareCommands()
		{
			m_insertCallerCmd = CreateCommand("INSERT INTO Calls (ThreadId, ParentId, ChildId, HitCount) VALUES (?, ?, ?, ?)", 4);
			m_updateCallerCmd = CreateCommand("UPDATE Calls SET HitCount = HitCount + ? WHERE ThreadId=? AND ParentId=? AND ChildId=?", 4);

			m_insertSampleCmd = CreateCommand("INSERT INTO Samples (ThreadId, FunctionId, HitCount) VALUES (?, ?, ?)", 3);
			m_updateSampleCmd = CreateCommand("UPDATE Samples SET HitCount = HitCount + ? WHERE ThreadId=? AND FunctionId=?", 3);

			m_insertAllocCmd = CreateCommand("INSERT INTO Allocations (Count, Size, ClassId, FunctionId) VALUES (?, ?, ?, ?)", 4);
			m_updateAllocCmd = CreateCommand("UPDATE Allocations SET Count = Count + ?, Size = Size + ? WHERE ClassId=? AND FunctionId=?", 4);
		}

		private int FlushCalls()
		{
			int queryCount = 0;
			foreach(KeyValuePair<int, SortedDictionary<int, SortedList<int, int>>> threadKvp in m_calls.Graph)
			{
				int threadId = threadKvp.Key;
				foreach(KeyValuePair<int, SortedList<int, int>> callerKvp in threadKvp.Value)
				{
					int parentId = callerKvp.Key;
					foreach(KeyValuePair<int, int> hitsKvp in callerKvp.Value)
					{
						int childId = hitsKvp.Key;
						int hits = hitsKvp.Value;

						m_updateCallerCmd.Parameters[0].Value = hits;
						m_updateCallerCmd.Parameters[1].Value = threadId;
						m_updateCallerCmd.Parameters[2].Value = parentId;
						m_updateCallerCmd.Parameters[3].Value = childId;
						int count = m_updateCallerCmd.ExecuteNonQuery();
						++queryCount;

						if(count == 0)
						{
							m_insertCallerCmd.Parameters[0].Value = threadId;
							m_insertCallerCmd.Parameters[1].Value = parentId;
							m_insertCallerCmd.Parameters[2].Value = childId;
							m_insertCallerCmd.Parameters[3].Value = hits;
							m_insertCallerCmd.ExecuteNonQuery();
							++queryCount;
						}
					}
					callerKvp.Value.Clear();
				}
			}
			return queryCount;
		}

		private int FlushSamples()
		{
			int queryCount = 0;
			//now to update the samples table
			foreach(KeyValuePair<int, SortedList<int, int>> sampleKvp in m_samples)
			{
				if(sampleKvp.Value.Count == 0)
					continue;

				foreach(KeyValuePair<int, int> threadKvp in sampleKvp.Value)
				{
					m_updateSampleCmd.Parameters[0].Value = threadKvp.Value;
					m_updateSampleCmd.Parameters[1].Value = threadKvp.Key;
					m_updateSampleCmd.Parameters[2].Value = sampleKvp.Key;
					int count = m_updateSampleCmd.ExecuteNonQuery();
					++queryCount;

					if(count == 0)
					{
						m_insertSampleCmd.Parameters[0].Value = threadKvp.Key;
						m_insertSampleCmd.Parameters[1].Value = sampleKvp.Key;
						m_insertSampleCmd.Parameters[2].Value = threadKvp.Value;
						m_insertSampleCmd.ExecuteNonQuery();
						++queryCount;
					}
				}

				sampleKvp.Value.Clear();
			}
			return queryCount;
		}

		private int FlushAllocations()
		{
			int queryCount = 0;

			foreach(KeyValuePair<int, SortedDictionary<int, AllocData>> classKvp in m_allocs)
			{
				foreach(KeyValuePair<int, AllocData> funcKvp in classKvp.Value)
				{
					m_updateAllocCmd.Parameters[0].Value = funcKvp.Value.Count;
					m_updateAllocCmd.Parameters[1].Value = funcKvp.Value.Size;
					m_updateAllocCmd.Parameters[2].Value = classKvp.Key;
					m_updateAllocCmd.Parameters[3].Value = funcKvp.Key;
					int count = m_updateAllocCmd.ExecuteNonQuery();
					++queryCount;

					if(count == 0)
					{
						m_insertAllocCmd.Parameters[0].Value = funcKvp.Value.Count;
						m_insertAllocCmd.Parameters[1].Value = funcKvp.Value.Size;
						m_insertAllocCmd.Parameters[2].Value = classKvp.Key;
						m_insertAllocCmd.Parameters[3].Value = funcKvp.Key;
						m_insertAllocCmd.ExecuteNonQuery();
						++queryCount;
					}
				}
			}

			m_allocs.Clear();

			return queryCount;
		}

		public override void Dispose()
		{
			base.Dispose();

			//and this is why C# could really use some RAII constructs
			Utilities.Dispose(m_insertCallerCmd);
			Utilities.Dispose(m_updateCallerCmd);
			Utilities.Dispose(m_insertSampleCmd);
			Utilities.Dispose(m_updateSampleCmd);
			Utilities.Dispose(m_insertAllocCmd);
			Utilities.Dispose(m_updateAllocCmd);

			m_database.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
