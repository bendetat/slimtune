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

namespace SlimTuneUI
{
	[DisplayName("SQLite"),
	HandlesExtension("sqlite")]
	public class SQLiteEngine : DataEngineBase
	{
		SQLiteConnection m_database;

		SQLiteCommand m_mapFunctionCmd;
		SQLiteCommand m_mapClassCmd;
		SQLiteCommand m_insertThreadCmd;
		SQLiteCommand m_updateThreadAliveCmd;
		SQLiteCommand m_updateThreadNameCmd;
		SQLiteCommand m_insertCallerCmd;
		SQLiteCommand m_updateCallerCmd;
		SQLiteCommand m_insertSampleCmd;
		SQLiteCommand m_updateSampleCmd;

		SQLiteCommand m_insertCounterCmd;
		SQLiteCommand m_counterNameCmd;

		public override string Extension
		{
			get { return "sqlite"; }
		}

		public override string Engine
		{
			get { return "SQLite"; }
		}

		public SQLiteEngine()
			: base(Path.GetTempFileName())
		{
			string connStr = string.Format("Data Source={0}; Synchronous=Off;", Name);
			m_database = new SQLiteConnection(connStr);
			m_database.Open();
			CreateSchema();

			var config = Fluently.Configure().Database(SQLiteConfiguration.Standard.ConnectionString(connStr));
			CreateSessionFactory(config);

			PrepareCommands();
		}

		public SQLiteEngine(string name, bool createNew)
			: base(name)
		{
			string connStr = string.Format("Data Source={0}; Synchronous=Off;", Name);
			if(createNew)
			{
				if(File.Exists(name))
					File.Delete(name);

				m_database = new SQLiteConnection(connStr);
				m_database.Open();
				CreateSchema();
			}
			else
			{
				if(!File.Exists(name))
					throw new InvalidOperationException();

				m_database = new SQLiteConnection(connStr);
				m_database.Open();
			}


			var config = Fluently.Configure().Database(SQLiteConfiguration.Standard.ConnectionString(connStr));
			CreateSessionFactory(config);

			PrepareCommands();
		}

		~SQLiteEngine()
		{
			Dispose();
		}

		public override void MapFunction(FunctionInfo funcInfo)
		{
			lock(m_lock)
			{
				m_mapFunctionCmd.Parameters[0].Value = funcInfo.Id;
				m_mapFunctionCmd.Parameters[1].Value = funcInfo.ClassId;
				m_mapFunctionCmd.Parameters[2].Value = funcInfo.IsNative ? 1 : 0;
				m_mapFunctionCmd.Parameters[3].Value = funcInfo.Name;
				m_mapFunctionCmd.Parameters[4].Value = funcInfo.Signature;
				m_mapFunctionCmd.ExecuteNonQuery();
			}
		}

		public override void MapClass(ClassInfo classInfo)
		{
			lock(m_lock)
			{
				m_mapClassCmd.Parameters[0].Value = classInfo.Id;
				m_mapClassCmd.Parameters[1].Value = classInfo.Name;
				m_mapClassCmd.ExecuteNonQuery();
			}
		}

		public override void UpdateThread(int threadId, bool? alive, string name)
		{
			lock(m_lock)
			{
				bool insert = false;
				if(alive.HasValue)
				{
					m_updateThreadAliveCmd.Parameters[0].Value = alive.Value ? 1 : 0;
					m_updateThreadAliveCmd.Parameters[1].Value = threadId;
					int count = m_updateThreadAliveCmd.ExecuteNonQuery();
					if(count == 0)
						insert = true;
				}

				if(!insert && name != null)
				{
					m_updateThreadNameCmd.Parameters[0].Value = name;
					m_updateThreadNameCmd.Parameters[1].Value = threadId;
					int count = m_updateThreadNameCmd.ExecuteNonQuery();
					if(count == 0)
						insert = true;
				}

				if(insert)
				{
					bool aliveValue = alive.HasValue ? alive.Value : true;
					string nameValue = name ?? string.Empty;
					m_insertThreadCmd.Parameters[0].Value = threadId;
					m_insertThreadCmd.Parameters[1].Value = aliveValue ? 1 : 0;
					m_insertThreadCmd.Parameters[2].Value = nameValue;
					m_insertThreadCmd.ExecuteNonQuery();
				}
			}
		}

		public override void CounterName(int counterId, string name)
		{
			lock(m_lock)
			{
				m_counterNameCmd.Parameters[0].Value = counterId;
				m_counterNameCmd.Parameters[1].Value = name;
				m_counterNameCmd.ExecuteNonQuery();
			}
		}

		public override void PerfCounter(int counterId, long time, double value)
		{
			lock(m_lock)
			{
				m_insertCounterCmd.Parameters[0].Value = counterId;
				m_insertCounterCmd.Parameters[1].Value = time;
				m_insertCounterCmd.Parameters[2].Value = value;
				m_insertCounterCmd.ExecuteNonQuery();
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
				int queryCount = 0;

				using(SQLiteTransaction transact = m_database.BeginTransaction())
				{
					queryCount += FlushCallers();
					queryCount += FlushSamples();
					transact.Commit();
				}

				m_lastFlush = DateTime.Now;
				m_cachedSamples = 0;
				//m_cachedTimings = 0;
				timer.Stop();
				Debug.WriteLine(string.Format("Database update took {0} milliseconds for {1} queries.", timer.ElapsedMilliseconds, queryCount));
			}
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

				var cmd = string.Format("INSERT INTO Snapshots (Name, DateTime) VALUES ({0}, {1})", name, DateTime.Now.ToFileTime());
				Command(cmd);

				int id = (int) RawQueryScalar("SELECT MAX(Id) FROM Snapshots");
				Command(string.Format("CREATE TABLE Callers_{0} {1}", id, kCallersSchema));
				Command(string.Format("INSERT INTO Callers_{0} SELECT * FROM Callers", id));
				Command(string.Format("CREATE TABLE Samples_{0} {1}", id, kSamplesSchema));
				Command(string.Format("INSERT INTO Samples_{0} SELECT * FROM Samples", id));
			}
		}

		public override NHibernate.ISession OpenSession()
		{
			return m_sessionFactory.OpenSession(m_database);
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
			}
			while(reader.NextResult());
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
			RawQueryScalar("UPDATE Callers SET HitCount = 0");
			RawQueryScalar("UPDATE Samples SET HitCount = 0");
			RawQueryScalar("DELETE FROM CounterValues");
			RawQueryScalar("DELETE FROM Counters");
		}

		private void Command(string command)
		{
			using(SQLiteCommand cmd = new SQLiteCommand(command, m_database))
			{
				cmd.ExecuteNonQuery();
			}
		}

		private void CreateSchema()
		{
			Command("PRAGMA count_changes=TRUE");
			Command("PRAGMA synchronous=OFF");
			Command("PRAGMA journal_mode=MEMORY");

			Command("CREATE TABLE Properties (Name TEXT (256), Value TEXT (256))");
			WriteProperties();

			Command("CREATE TABLE Snapshots (Id INT PRIMARY KEY, Name TEXT (256), DateTime INTEGER)");
			Command("CREATE TABLE Functions (Id INT PRIMARY KEY, ClassId INT, IsNative INT NOT NULL, Name TEXT (1024), Signature TEXT (2048))");
			Command("CREATE TABLE Classes (Id INT PRIMARY KEY, Name TEXT (1024))");

			Command("CREATE TABLE Threads (Id INT PRIMARY KEY, IsAlive INT, Name TEXT(256))");

			//We will look up results in CallerId order when updating this table
			Command("CREATE TABLE Callers " + kCallersSchema);
			Command("CREATE INDEX Callers_CallerIndex ON Callers(CallerId);");
			Command("CREATE INDEX Callers_CalleeIndex ON Callers(CalleeId);");
			Command("CREATE INDEX Callers_Compound ON Callers(ThreadId, CallerId, CalleeId);");

			Command("CREATE TABLE Samples " + kSamplesSchema);
			Command("CREATE INDEX Samples_FunctionIndex ON Samples(FunctionId);");
			Command("CREATE INDEX Samples_Compound ON Samples(ThreadId, FunctionId);");

			Command("CREATE TABLE Timings (FunctionId INT, RangeMin INT, RangeMax INT, HitCount INT)");
			Command("CREATE INDEX Timings_FunctionIndex ON Timings(FunctionId);");
			Command("CREATE INDEX Timings_Compound ON Timings(FunctionId, RangeMin);");

			Command("CREATE TABLE Counters (Id INT PRIMARY KEY, Name TEXT(256))");
			Command("CREATE TABLE CounterValues (CounterId INT, Time INT, Value REAL)");
			Command("CREATE INDEX CounterValues_IdIndex ON Counters(Id);");
		}

		private void WriteProperty(string name, string value)
		{
			Command(string.Format("INSERT INTO Properties (Name, Value) VALUES ('{0}', '{1}')", name, value));
		}

		private void WriteProperties()
		{
			WriteProperty("Application", "SlimTune Profiler");
			WriteProperty("Version", System.Windows.Forms.Application.ProductVersion);
			WriteProperty("FileVersion", "2");
		}

		private SQLiteCommand CreateCommand(string commandText, int paramCount)
		{
			SQLiteCommand command = new SQLiteCommand(commandText, m_database);
			for(int i = 0; i < paramCount; ++i)
				command.Parameters.Add(new SQLiteParameter());
			return command;
		}

		private void PrepareCommands()
		{
			m_mapFunctionCmd = CreateCommand("INSERT INTO Functions (Id, ClassId, IsNative, Name, Signature) VALUES (?, ?, ?, ?, ?)", 5);
			m_mapClassCmd = CreateCommand("INSERT INTO Classes (Id, Name) VALUES (?, ?)", 2);

			m_insertThreadCmd = CreateCommand("INSERT INTO Threads (Id, IsAlive, Name) VALUES (?, ?, ?)", 3);
			m_updateThreadAliveCmd = CreateCommand("UPDATE Threads SET IsAlive = ? WHERE Id=?", 2);
			m_updateThreadNameCmd = CreateCommand("UPDATE Threads SET Name = ? WHERE Id=?", 2);

			m_insertCallerCmd = CreateCommand("INSERT INTO Callers (ThreadId, CallerId, CalleeId, HitCount) VALUES (?, ?, ?, ?)", 4);
			m_updateCallerCmd = CreateCommand("UPDATE Callers SET HitCount = HitCount + ? WHERE ThreadId=? AND CallerId=? AND CalleeId=?", 4);

			m_insertSampleCmd = CreateCommand("INSERT INTO Samples (ThreadId, FunctionId, HitCount) VALUES (?, ?, ?)", 3);
			m_updateSampleCmd = CreateCommand("UPDATE Samples SET HitCount = HitCount + ? WHERE ThreadId=? AND FunctionId=?", 3);

			m_insertCounterCmd = CreateCommand("INSERT INTO CounterValues (CounterId, Time, Value) VALUES (?, ?, ?)", 3);
			m_counterNameCmd = CreateCommand("REPLACE INTO Counters (Id, Name) VALUES (?, ?)", 2);
		}

		private int FlushCallers()
		{
			int queryCount = 0;
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

						m_updateCallerCmd.Parameters[0].Value = hits;
						m_updateCallerCmd.Parameters[1].Value = threadId;
						m_updateCallerCmd.Parameters[2].Value = callerId;
						m_updateCallerCmd.Parameters[3].Value = calleeId;
						int count = Convert.ToInt32(m_updateCallerCmd.ExecuteScalar());
						++queryCount;

						if(count == 0)
						{
							m_insertCallerCmd.Parameters[0].Value = threadId;
							m_insertCallerCmd.Parameters[1].Value = callerId;
							m_insertCallerCmd.Parameters[2].Value = calleeId;
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
					int count = Convert.ToInt32(m_updateSampleCmd.ExecuteScalar());
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

		public override void Dispose()
		{
			//and this is why C# could really use some RAII constructs
			m_mapFunctionCmd.Dispose();
			m_mapClassCmd.Dispose();
			m_insertThreadCmd.Dispose();
			m_updateThreadAliveCmd.Dispose();
			m_updateThreadNameCmd.Dispose();
			m_insertCallerCmd.Dispose();
			m_updateCallerCmd.Dispose();
			m_insertSampleCmd.Dispose();
			m_updateSampleCmd.Dispose();
			m_insertCounterCmd.Dispose();
			m_counterNameCmd.Dispose();

			m_database.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
