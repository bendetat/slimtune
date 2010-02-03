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

namespace SlimTuneUI
{
	[DisplayName("SQLite Engine (cross platform)")]
	class SQLiteEngine : StorageEngineBase
	{
		SQLiteDatabase m_database;

		SQLiteStatement m_mapFunctionCmd;
		SQLiteStatement m_mapClassCmd;
		SQLiteStatement m_insertThreadCmd;
		SQLiteStatement m_updateThreadAliveCmd;
		SQLiteStatement m_updateThreadNameCmd;
		SQLiteStatement m_insertCallerCmd;
		SQLiteStatement m_updateCallerCmd;
		SQLiteStatement m_insertSampleCmd;
		SQLiteStatement m_updateSampleCmd;

		public override string Extension
		{
			get { return "sqlite"; }
		}

		public SQLiteEngine()
			: base("memory")
		{
			//create the database in-memory
			m_database = new SQLiteDatabase(":memory:");
			CreateSchema();
			PrepareCommands();
		}

		public SQLiteEngine(string name, bool createNew)
			: base(name)
		{
			if(createNew)
			{
				if(File.Exists(name))
					File.Delete(name);

				m_database = new SQLiteDatabase(name);
				CreateSchema();
			}
			else
			{
				if(!File.Exists(name))
					throw new InvalidOperationException();

				m_database = new SQLiteDatabase(name);
			}

			PrepareCommands();
		}

		~SQLiteEngine()
		{
			Dispose();
		}

		public override void MapFunction(FunctionInfo funcInfo)
		{
			m_mapFunctionCmd.Reset();
			m_mapFunctionCmd.BindInt(1, funcInfo.FunctionId);
			m_mapFunctionCmd.BindInt(2, funcInfo.ClassId);
			m_mapFunctionCmd.BindInt(3, funcInfo.IsNative ? 1 : 0);
			m_mapFunctionCmd.BindText(4, funcInfo.Name);
			m_mapFunctionCmd.BindText(5, funcInfo.Signature);
			m_mapFunctionCmd.Step();
		}

		public override void MapClass(ClassInfo classInfo)
		{
			m_mapClassCmd.Reset();
			m_mapClassCmd.BindInt(1, classInfo.ClassId);
			m_mapClassCmd.BindText(2, classInfo.Name);
			m_mapClassCmd.Step();
		}

		public override void UpdateThread(int threadId, bool? alive, string name)
		{
			bool insert = false;
			if(alive.HasValue)
			{
				m_updateThreadAliveCmd.Reset();
				m_updateThreadAliveCmd.BindInt(1, threadId);
				m_updateThreadAliveCmd.BindInt(2, alive.Value ? 1 : 0);
				m_updateThreadAliveCmd.Step();
				if(m_updateThreadAliveCmd.GetInt(0) == 0)
					insert = true;
			}

			if(!insert && name != null)
			{
				m_updateThreadNameCmd.Reset();
				m_updateThreadNameCmd.BindInt(1, threadId);
				m_updateThreadNameCmd.BindText(2, name);
				m_updateThreadNameCmd.Step();
				if(m_updateThreadNameCmd.GetInt(0) == 0)
					insert = true;
			}

			if(insert)
			{
				bool aliveValue = alive.HasValue ? alive.Value : true;
				string nameValue = name ?? string.Empty;
				m_insertThreadCmd.Reset();
				m_insertThreadCmd.BindInt(1, threadId);
				m_insertThreadCmd.BindInt(2, aliveValue ? 1 : 0);
				m_insertThreadCmd.BindText(3, nameValue);
				m_insertThreadCmd.Step();
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

				FlushCallers();
				FlushSamples();

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

				var cmd = string.Format("INSERT INTO Snapshots (Name, DateTime) VALUES ({0}, {1})", name, DateTime.Now.ToFileTime());
				m_database.Execute(cmd);

				int id = (int) QueryScalar("SELECT MAX(Id) FROM Snapshots");
				m_database.Execute(string.Format("CREATE TABLE Callers_{0} {1}", id, kCallersSchema));
				m_database.Execute(string.Format("INSERT INTO Callers_{0} SELECT * FROM Callers", id));
				m_database.Execute(string.Format("CREATE TABLE Samples_{0} {1}", id, kSamplesSchema));
				m_database.Execute(string.Format("INSERT INTO Samples_{0} SELECT * FROM Samples", id));
			}
		}

		public override DataSet Query(string query)
		{
			using(SQLiteStatement cmd = new SQLiteStatement(m_database, query))
			{
				var ds = new DataSet();
				var table = new DataTable("Query");

				int columnCount = cmd.Columns;
				while(cmd.Step())
				{
					//populate columns if necessary
					if(table.Columns.Count == 0)
					{
						for(int i = 0; i < columnCount; ++i)
						{
							Type type = typeof(string);
							SQLiteType columnType = cmd.GetColumnType(i);
							switch(columnType)
							{
								case SQLiteType.Integer:
									type = typeof(long);
									break;
								case SQLiteType.Float:
									type = typeof(double);
									break;
								case SQLiteType.Text:
								default:
									type = typeof(string);
									break;
							}

							table.Columns.Add(cmd.GetColumnName(i), type);
						}
					}

					//get row data
					var row = table.NewRow();
					for(int i = 0; i < columnCount; ++i)
					{
						switch(cmd.GetColumnType(i))
						{
							case SQLiteType.Integer:
								row[i] = cmd.GetLong(i);
								break;
							case SQLiteType.Float:
								row[i] = cmd.GetDouble(i);
								break;
							case SQLiteType.Text:
							default:
								row[i] = cmd.GetText(i);
								break;
						}
					}
					table.Rows.Add(row);
				}

				ds.Tables.Add(table);
				return ds;
			}
		}

		public override object QueryScalar(string query)
		{
			using(SQLiteStatement cmd = new SQLiteStatement(m_database, query))
			{
				if(!cmd.Step())
					return null;

				switch(cmd.GetColumnType(0))
				{
					case SQLiteType.Integer:
						return cmd.GetLong(0);
					case SQLiteType.Float:
						return cmd.GetDouble(0);
					case SQLiteType.Text:
						return cmd.GetText(0);
				}

				return null;
			}
		}

		protected override void DoClearData()
		{
			QueryScalar("UPDATE Callers SET HitCount = 0");
			QueryScalar("UPDATE Samples SET HitCount = 0");
		}

		private void CreateSchema()
		{
			m_database.Execute("PRAGMA count_changes=TRUE");
			m_database.Execute("PRAGMA synchronous=OFF");
			m_database.Execute("PRAGMA journal_mode=MEMORY");

			m_database.Execute("CREATE TABLE Snapshots (Id INT PRIMARY KEY, Name TEXT (256), DateTime INTEGER)");
			m_database.Execute("CREATE TABLE Functions (Id INT PRIMARY KEY, ClassId INT, IsNative INT NOT NULL, Name TEXT (1024), Signature TEXT (2048))");
			m_database.Execute("CREATE TABLE Classes (Id INT PRIMARY KEY, Name TEXT (1024))");

			m_database.Execute("CREATE TABLE Threads (Id INT PRIMARY KEY, IsAlive INT, Name TEXT(256))");

			//We will look up results in CallerId order when updating this table
			m_database.Execute("CREATE TABLE Callers " + kCallersSchema);
			m_database.Execute("CREATE INDEX Callers_CallerIndex ON Callers(CallerId);");
			m_database.Execute("CREATE INDEX Callers_CalleeIndex ON Callers(CalleeId);");
			m_database.Execute("CREATE INDEX Callers_Compound ON Callers(ThreadId, CallerId, CalleeId);");

			m_database.Execute("CREATE TABLE Samples " + kSamplesSchema);
			m_database.Execute("CREATE INDEX Samples_FunctionIndex ON Samples(FunctionId);");
			m_database.Execute("CREATE INDEX Samples_Compound ON Samples(ThreadId, FunctionId);");

			m_database.Execute("CREATE TABLE Timings (FunctionId INT, RangeMin INT, RangeMax INT, HitCount INT)");
			m_database.Execute("CREATE INDEX Timings_FunctionIndex ON Timings(FunctionId);");
			m_database.Execute("CREATE INDEX Timings_Compound ON Timings(FunctionId, RangeMin);");
		}

		private void PrepareCommands()
		{
			m_mapFunctionCmd = new SQLiteStatement(m_database, "INSERT INTO Functions (Id, ClassId, IsNative, Name, Signature) VALUES (?1, ?2, ?3, ?4, ?5)");
			m_mapClassCmd = new SQLiteStatement(m_database, "INSERT INTO Classes (Id, Name) VALUES (?1, ?2)");

			m_insertThreadCmd = new SQLiteStatement(m_database, "INSERT INTO Threads (Id, IsAlive, Name) VALUES (?1, ?2, ?3)");
			m_updateThreadAliveCmd = new SQLiteStatement(m_database, "UPDATE Threads SET IsAlive = ?2 WHERE Id=?1");
			m_updateThreadNameCmd = new SQLiteStatement(m_database, "UPDATE Threads SET Name = ?2 WHERE Id=?1");

			m_insertCallerCmd = new SQLiteStatement(m_database, "INSERT INTO Callers (ThreadId, CallerId, CalleeId, HitCount) VALUES (?1, ?2, ?3, ?4)");
			m_updateCallerCmd = new SQLiteStatement(m_database, "UPDATE Callers SET HitCount = HitCount + ?4 WHERE ThreadId=?1 AND CallerId=?2 AND CalleeId=?3");

			m_insertSampleCmd = new SQLiteStatement(m_database, "INSERT INTO Samples (ThreadId, FunctionId, HitCount) VALUES (?1, ?2, ?3)");
			m_updateSampleCmd = new SQLiteStatement(m_database, "UPDATE Samples SET HitCount = HitCount + ?3 WHERE ThreadId=?1 AND FunctionId=?2");
		}

		private void FlushCallers()
		{
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

						m_updateCallerCmd.Reset();
						m_updateCallerCmd.BindInt(1, threadId);
						m_updateCallerCmd.BindInt(2, callerId);
						m_updateCallerCmd.BindInt(3, calleeId);
						m_updateCallerCmd.BindInt(4, hits);
						m_updateCallerCmd.Step();

						if(m_updateCallerCmd.GetInt(0) == 0)
						{
							m_insertCallerCmd.Reset();
							m_insertCallerCmd.BindInt(1, threadId);
							m_insertCallerCmd.BindInt(2, callerId);
							m_insertCallerCmd.BindInt(3, calleeId);
							m_insertCallerCmd.BindInt(4, hits);
							m_insertCallerCmd.Step();
						}
					}
					callerKvp.Value.Clear();
				}
			}
		}

		private void FlushSamples()
		{
			//now to update the samples table
			foreach(KeyValuePair<int, SortedList<int, int>> sampleKvp in m_samples)
			{
				if(sampleKvp.Value.Count == 0)
					continue;

				foreach(KeyValuePair<int, int> threadKvp in sampleKvp.Value)
				{
					m_updateSampleCmd.Reset();
					m_updateSampleCmd.BindInt(1, threadKvp.Key);
					m_updateSampleCmd.BindInt(2, sampleKvp.Key);
					m_updateSampleCmd.BindInt(3, threadKvp.Key);
					m_updateSampleCmd.Step();

					if(m_updateSampleCmd.GetInt(0) == 0)
					{
						m_insertSampleCmd.Reset();
						m_insertSampleCmd.BindInt(1, threadKvp.Key);
						m_insertSampleCmd.BindInt(2, sampleKvp.Key);
						m_insertSampleCmd.BindInt(3, threadKvp.Key);
						m_insertSampleCmd.Step();
					}
				}

				sampleKvp.Value.Clear();
			}
		}

		public override void Dispose()
		{
			m_mapFunctionCmd.Dispose();
			m_mapClassCmd.Dispose();
			m_insertThreadCmd.Dispose();
			m_updateThreadAliveCmd.Dispose();
			m_updateThreadNameCmd.Dispose();
			m_insertCallerCmd.Dispose();
			m_updateCallerCmd.Dispose();
			m_insertSampleCmd.Dispose();
			m_updateSampleCmd.Dispose();

			m_database.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
