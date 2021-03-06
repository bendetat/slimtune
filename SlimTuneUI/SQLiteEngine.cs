﻿/*
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
			//dbconfig.ShowSql();
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

		protected override void PreCreateSchema()
		{
			SqlCommand("PRAGMA synchronous=OFF");
			SqlCommand("PRAGMA journal_mode=MEMORY");
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
			m_insertCallerCmd = CreateCommand("INSERT INTO Calls (Time, ThreadId, ParentId, ChildId) VALUES (?, ?, ?, ?)", 4);
			m_updateCallerCmd = CreateCommand("UPDATE Calls SET Time = Time + ? WHERE ThreadId=? AND ParentId=? AND ChildId=? AND SnapshotId=0", 4);

			m_insertSampleCmd = CreateCommand("INSERT INTO Samples (Time, ThreadId, FunctionId) VALUES (?, ?, ?)", 3);
			m_updateSampleCmd = CreateCommand("UPDATE Samples SET Time = Time + ? WHERE ThreadId=? AND FunctionId=? AND SnapshotId=0", 3);

			m_insertAllocCmd = CreateCommand("INSERT INTO Allocations (Count, Size, ClassId, FunctionId) VALUES (?, ?, ?, ?)", 4);
			m_updateAllocCmd = CreateCommand("UPDATE Allocations SET Count = Count + ?, Size = Size + ? WHERE ClassId=? AND FunctionId=?", 4);
		}

		private int FlushCalls()
		{
			int queryCount = 0;
			foreach(KeyValuePair<long, Call> kvp in m_calls)
			{
				var call = kvp.Value;
				if(call.Time == 0)
					continue;

				m_updateCallerCmd.Parameters[0].Value = call.Time;
				m_updateCallerCmd.Parameters[1].Value = call.ThreadId;
				m_updateCallerCmd.Parameters[2].Value = call.ParentId;
				m_updateCallerCmd.Parameters[3].Value = call.ChildId;
				int count = m_updateCallerCmd.ExecuteNonQuery();
				++queryCount;

				if(count == 0)
				{
					m_insertCallerCmd.Parameters[0].Value = call.Time;
					m_insertCallerCmd.Parameters[1].Value = call.ThreadId;
					m_insertCallerCmd.Parameters[2].Value = call.ParentId;
					m_insertCallerCmd.Parameters[3].Value = call.ChildId;
					m_insertCallerCmd.ExecuteNonQuery();
					++queryCount;
				}

				call.Time = 0;
			}
			return queryCount;
		}

		private int FlushSamples()
		{
			int queryCount = 0;
			//now to update the samples table
			foreach(KeyValuePair<long, Sample> kvp in m_samples)
			{
				var sample = kvp.Value;
				if(sample.Time == 0)
					continue;

				m_updateSampleCmd.Parameters[0].Value = sample.Time;
				m_updateSampleCmd.Parameters[1].Value = sample.ThreadId;
				m_updateSampleCmd.Parameters[2].Value = sample.FunctionId;
				int count = m_updateSampleCmd.ExecuteNonQuery();
				++queryCount;

				if(count == 0)
				{
					m_insertSampleCmd.Parameters[0].Value = sample.Time;
					m_insertSampleCmd.Parameters[1].Value = sample.ThreadId;
					m_insertSampleCmd.Parameters[2].Value = sample.FunctionId;
					m_insertSampleCmd.ExecuteNonQuery();
					++queryCount;
				}

				sample.Time = 0;
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

			//and this is why C# could really use some real RAII constructs
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
