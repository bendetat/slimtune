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
using System.Data;

namespace SlimTuneUI
{
	public class FunctionInfo
	{
		public int FunctionId;
		public int ClassId;
		public bool IsNative;
		public string Name;
		public string Signature;

		public FunctionInfo()
		{
		}

		public FunctionInfo(int funcId, int classId, bool isNative, string name, string signature)
		{
			FunctionId = funcId;
			ClassId = classId;
			IsNative = isNative;
			Name = name;
			Signature = signature;
		}
	}

	public class ClassInfo
	{
		public int ClassId;
		public string Name;

		public ClassInfo()
		{
		}

		public ClassInfo(int classId, string name)
		{
			ClassId = classId;
			Name = name;
		}
	}

	public class TransactionHandle : IDisposable
	{
		IStorageEngine m_engine;

		public TransactionHandle(IStorageEngine engine)
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

	public interface IStorageEngine : IDisposable
	{
		string Name { get; }
		string Extension { get; }

		void MapFunction(FunctionInfo funcInfo);
		void MapClass(ClassInfo classInfo);

		void ParseSample(Messages.Sample sample);
		void ClearData();
		void UpdateThread(int threadId, bool? alive, string name);

		void FunctionTiming(int functionId, long time);

		bool AllowFlush { get; set; }
		void Flush();
		void Snapshot(string name);

		DataSet Query(string query);
		object QueryScalar(string query);
	}
}
