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
using System.Runtime.InteropServices;

namespace SlimTuneUI
{
	delegate int SQLiteExecCallback(IntPtr param, int argc, IntPtr argv, IntPtr column);
	delegate void SQLiteDestructor(IntPtr pointer);

	enum SQLiteError
	{
		OK = 0,   /* Successful result */

		/* beginning-of-error-codes */
		ERROR = 1,   /* SQL error or missing database */
		INTERNAL = 2,   /* Internal logic error in SQLite */
		PERM = 3,   /* Access permission denied */
		ABORT = 4,   /* Callback routine requested an abort */
		BUSY = 5,   /* The database file is locked */
		LOCKED = 6,   /* A table in the database is locked */
		NOMEM = 7,   /* A malloc() failed */
		READONLY = 8,   /* Attempt to write a readonly database */
		INTERRUPT = 9,   /* Operation terminated by sqlite3_interrupt()*/
		IOERR = 10,   /* Some kind of disk I/O error occurred */
		CORRUPT = 11,   /* The database disk image is malformed */
		NOTFOUND = 12,   /* NOT USED. Table or record not found */
		FULL = 13,   /* Insertion failed because database is full */
		CANTOPEN = 14,   /* Unable to open the database file */
		PROTOCOL = 15,   /* NOT USED. Database lock protocol error */
		EMPTY = 16,   /* Database is empty */
		SCHEMA = 17,   /* The database schema changed */
		TOOBIG = 18,   /* String or BLOB exceeds size limit */
		CONSTRAINT = 19,   /* Abort due to constraint violation */
		MISMATCH = 20,   /* Data type mismatch */
		MISUSE = 21,   /* Library used incorrectly */
		NOLFS = 22,   /* Uses OS features not supported on host */
		AUTH = 23,   /* Authorization denied */
		FORMAT = 24,   /* Auxiliary database format error */
		RANGE = 25,   /* 2nd parameter to sqlite3_bind out of range */
		NOTADB = 26,   /* File opened that is not a database file */
		ROW = 100,  /* sqlite3_step() has another row ready */
		DONE = 101  /* sqlite3_step() has finished executing */
	}

	enum SQLiteType
	{
		Integer = 1,
		Float = 2,
		Text = 3,
		Blob = 4,
		Null = 5
	}

	class SQLiteException : Exception
	{
		int m_code;

		public SQLiteException(int code)
			: base()
		{
			m_code = code;
		}

		public SQLiteException(int code, string message)
			: base(message)
		{
			m_code = code;
		}

		public int Code
		{
			get { return m_code; }
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	class SQLiteDatabase : IDisposable
	{
		IntPtr m_database;

		public SQLiteDatabase(string name)
		{
			int result = SQLiteFunctions.Open(name, out m_database);
			SQLiteFunctions.CheckError(result);
		}

		~SQLiteDatabase()
		{
			Dispose();
		}

		public IntPtr InternalPointer
		{
			get { return m_database; }
		}

		public void Execute(string query)
		{
			IntPtr errorMsg;
			int result = SQLiteFunctions.Exec(m_database, query, null, IntPtr.Zero, out errorMsg);
			string error = string.Empty;
			if(errorMsg != IntPtr.Zero)
			{
				error = Marshal.PtrToStringAnsi(errorMsg);
				SQLiteFunctions.Free(errorMsg);
			}
			SQLiteFunctions.CheckError(result, error);
		}

		public void Backup(string file)
		{
			IntPtr dest;
			var result = SQLiteFunctions.Open(file, out dest);
			SQLiteFunctions.CheckError(result);

			IntPtr error;
			SQLiteFunctions.Exec(dest, "PRAGMA count_changes=TRUE", null, IntPtr.Zero, out error);
			SQLiteFunctions.Exec(dest, "PRAGMA journal_mode=MEMORY", null, IntPtr.Zero, out error);
			SQLiteFunctions.Exec(dest, "PRAGMA synchronous=OFF", null, IntPtr.Zero, out error);

			try
			{
				var backup = SQLiteFunctions.BackupInit(dest, "main", m_database, "main");
				if(backup == IntPtr.Zero)
					throw new InvalidOperationException();

				do
				{
					result = SQLiteFunctions.BackupStep(backup, 5);
					if(result == (int) SQLiteError.BUSY)
						System.Threading.Thread.Sleep(250);
				} while(result == (int) SQLiteError.OK || result == (int) SQLiteError.BUSY || result == (int) SQLiteError.LOCKED);

				result = SQLiteFunctions.BackupFinish(backup);
				SQLiteFunctions.CheckError(result);
			}
			finally
			{
				SQLiteFunctions.Close(dest);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			int result = SQLiteFunctions.Close(m_database);
			SQLiteFunctions.CheckError(result);
			GC.SuppressFinalize(this);
		}

		#endregion
	}

	class SQLiteStatement : IDisposable
	{
		private IntPtr m_statement;

		public SQLiteStatement(SQLiteDatabase database, string query)
		{
			int result = SQLiteFunctions.Prepare16(database.InternalPointer, query, query.Length * 2 + 2, out m_statement, IntPtr.Zero);
			SQLiteFunctions.CheckError(result);
		}

		public bool Step()
		{
			int result = SQLiteFunctions.Step(m_statement);
			if(result == (int) SQLiteError.ROW)
				return true;
			if(result == (int) SQLiteError.DONE)
				return false;

			//should throw at this point
			SQLiteFunctions.CheckError(result);
			return false;
		}

		public void Reset()
		{
			int result = SQLiteFunctions.Reset(m_statement);
			SQLiteFunctions.CheckError(result);
		}

		public string GetColumnName(int column)
		{
			IntPtr strPtr = SQLiteFunctions.ColumnName(m_statement, column);
			return Marshal.PtrToStringUni(strPtr);
		}

		public SQLiteType GetColumnType(int column)
		{
			return (SQLiteType) SQLiteFunctions.ColumnType(m_statement, column);
		}

		public int GetInt(int column)
		{
			return SQLiteFunctions.ColumnInt(m_statement, column);
		}

		public long GetLong(int column)
		{
			return SQLiteFunctions.ColumnInt64(m_statement, column);
		}

		public double GetDouble(int column)
		{
			return SQLiteFunctions.ColumnDouble(m_statement, column);
		}

		public string GetText(int column)
		{
			IntPtr strPtr = SQLiteFunctions.ColumnText(m_statement, column);
			return Marshal.PtrToStringUni(strPtr);
		}

		public void BindInt(int param, int value)
		{
			int result = SQLiteFunctions.BindInt(m_statement, param, value);
			SQLiteFunctions.CheckError(result);
		}

		public void BindLong(int param, long value)
		{
			int result = SQLiteFunctions.BindLong(m_statement, param, value);
			SQLiteFunctions.CheckError(result);
		}

		public void BindDouble(int param, double value)
		{
			int result = SQLiteFunctions.BindDouble(m_statement, param, value);
			SQLiteFunctions.CheckError(result);
		}

		public void BindText(int param, string value)
		{
			int result = SQLiteFunctions.BindText(m_statement, param, value, value.Length * 2 + 2, IntPtr.Zero);
			GC.KeepAlive(value);
			SQLiteFunctions.CheckError(result);
		}

		public int Columns
		{
			get
			{
				return SQLiteFunctions.ColumnCount(m_statement);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			int result = SQLiteFunctions.Finalize(m_statement);
			SQLiteFunctions.CheckError(result);
		}

		#endregion
	}

	static class SQLiteFunctions
	{
		const string Dll = "sqlite3.dll";

		public static void CheckError(int resultCode)
		{
			CheckError(resultCode, string.Empty);
		}

		public static void CheckError(int resultCode, string message)
		{
			if(resultCode != (int) SQLiteError.OK)
				throw new SQLiteException(resultCode, message);
		}

		[DllImport(Dll, EntryPoint = "sqlite3_errmsg16")]
		public static extern IntPtr GetErrorMessage(IntPtr database);

		[DllImport(Dll, EntryPoint = "sqlite3_open")]
		public static extern int Open([MarshalAs(UnmanagedType.LPStr)] string name, out IntPtr database);

		[DllImport(Dll, EntryPoint = "sqlite3_close")]
		public static extern int Close(IntPtr database);

		[DllImport(Dll, EntryPoint = "sqlite3_exec")]
		public static extern int Exec(IntPtr database, [MarshalAs(UnmanagedType.LPStr)] string query,
			[MarshalAs(UnmanagedType.FunctionPtr)] SQLiteExecCallback callback, IntPtr callbackArg,
			out IntPtr errorMsg);

		[DllImport(Dll, EntryPoint = "sqlite3_malloc")]
		public static extern IntPtr Malloc(int bytes);

		[DllImport(Dll, EntryPoint = "sqlite3_free")]
		public static extern void Free(IntPtr ptr);

		[DllImport(Dll, EntryPoint = "sqlite3_prepare16_v2")]
		public static extern int Prepare16(IntPtr database, [MarshalAs(UnmanagedType.LPWStr)] string query,
			int bytes, out IntPtr statement, IntPtr tail);

		[DllImport(Dll, EntryPoint = "sqlite3_finalize")]
		public static extern int Finalize(IntPtr statement);

		[DllImport(Dll, EntryPoint = "sqlite3_step")]
		public static extern int Step(IntPtr statement);

		[DllImport(Dll, EntryPoint = "sqlite3_reset")]
		public static extern int Reset(IntPtr statement);

		[DllImport(Dll, EntryPoint = "sqlite3_column_count")]
		public static extern int ColumnCount(IntPtr statement);

		[DllImport(Dll, EntryPoint = "sqlite3_column_name16")]
		public static extern IntPtr ColumnName(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_origin_name16")]
		public static extern IntPtr ColumnOriginName(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_int")]
		public static extern int ColumnInt(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_int64")]
		public static extern long ColumnInt64(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_double")]
		public static extern double ColumnDouble(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_text16")]
		public static extern IntPtr ColumnText(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_column_type")]
		public static extern IntPtr ColumnType(IntPtr statement, int iCol);

		[DllImport(Dll, EntryPoint = "sqlite3_bind_int")]
		public static extern int BindInt(IntPtr statement, int param, int value);

		[DllImport(Dll, EntryPoint = "sqlite3_bind_int64")]
		public static extern int BindLong(IntPtr statement, int param, long value);

		[DllImport(Dll, EntryPoint = "sqlite3_bind_double")]
		public static extern int BindDouble(IntPtr statement, int param, double value);

		[DllImport(Dll, EntryPoint = "sqlite3_bind_text16")]
		public static extern int BindText(IntPtr statement, int param, [MarshalAs(UnmanagedType.LPWStr)] string value, int bytes, IntPtr dtor);

		[DllImport(Dll, EntryPoint = "sqlite3_backup_init")]
		public static extern IntPtr BackupInit(IntPtr destDb, [MarshalAs(UnmanagedType.LPStr)] string destName,
			IntPtr sourceDb, [MarshalAs(UnmanagedType.LPStr)] string sourceName);

		[DllImport(Dll, EntryPoint = "sqlite3_backup_step")]
		public static extern int BackupStep(IntPtr backup, int page);

		[DllImport(Dll, EntryPoint = "sqlite3_backup_finish")]
		public static extern int BackupFinish(IntPtr backup);
	}
}
