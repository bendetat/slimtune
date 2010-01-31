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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace UICore
{
	public enum ProfilerMode
	{
		Disabled = 0,

		Sampling = 0x01,
		Tracing = 0x02,

		Hybrid = Sampling | Tracing,
	}

	public class ThreadInfo
	{
		public int ThreadId;
		public string Name;
		public bool Alive;

		public Stack<Pair<int, long>> ShadowStack = new Stack<Pair<int, long>>();

		public ThreadInfo(int threadId, string name, bool alive)
		{
			ThreadId = threadId;
			Name = name;
			Alive = alive;
		}
	}

	public class ProfilerClient : IDisposable
	{
		TcpClient m_client;
		NetworkStream m_stream;
		BufferedStream m_bufferedStream;
		BinaryReader m_reader;
		BinaryWriter m_writer;
		Dictionary<int, FunctionInfo> m_functions = new Dictionary<int, FunctionInfo>();
		Dictionary<int, ClassInfo> m_classes = new Dictionary<int, ClassInfo>();
		Dictionary<int, ThreadInfo> m_threads = new Dictionary<int, ThreadInfo>();

		IStorageEngine m_storage;

		internal TcpClient Socket
		{
			get { return m_client; }
		}

		public string HostName { get; private set; }
		public int Port { get; private set; }

		public ProfilerClient(string host, int port, IStorageEngine storage)
		{
			m_client = new TcpClient();
			m_client.Connect(host, port);
			m_client.ReceiveBufferSize = 64 * 1024;
			m_stream = m_client.GetStream();
#if DEBUG
			m_stream.ReadTimeout = 120000;
#else
			m_stream.ReadTimeout = 30000;
#endif
			m_bufferedStream = new BufferedStream(m_stream, 64 * 1024);
			m_reader = new BinaryReader(m_bufferedStream, Encoding.Unicode);
			m_writer = new BinaryWriter(m_stream, Encoding.Unicode);
			m_storage = storage;

			m_classes.Add(0, new ClassInfo(0, "$INVALID$"));
			m_functions.Add(0, new FunctionInfo(0, 0, false, "$INVALID$", string.Empty));

			HostName = host;
			Port = port;

			Debug.WriteLine("Successfully connected.");
		}

		public string Receive()
		{
			try
			{
				if(m_stream == null)
					return string.Empty;

				MessageId messageId = (MessageId) m_reader.ReadByte();
				//Debug.WriteLine(string.Format("Message: {0}", messageId));
				switch(messageId)
				{
					case MessageId.MID_MapFunction:
						var mapFunc = Messages.MapFunction.Read(m_reader);
						MapFunction(mapFunc);
						break;

					case MessageId.MID_MapClass:
						var mapClass = Messages.MapClass.Read(m_reader);
						MapClass(mapClass);
						break;

					case MessageId.MID_MapThread:
						var mapThread = Messages.MapThread.Read(m_reader);
						MapThread(mapThread);
						break;

					case MessageId.MID_EnterFunction:
						var funcEvent = Messages.FunctionEvent.Read(m_reader);
						FunctionEvent(messageId, funcEvent);
						break;
					case MessageId.MID_LeaveFunction:
					case MessageId.MID_TailCall:
						var funcEvent2 = Messages.FunctionEvent.Read(m_reader);
						FunctionEvent(messageId, funcEvent2);
						break;

					case MessageId.MID_CreateThread:
					case MessageId.MID_DestroyThread:
						var threadEvent = Messages.CreateThread.Read(m_reader);
						UpdateThread(threadEvent.ThreadId, messageId == MessageId.MID_CreateThread ? true : false, null);
						break;

					case MessageId.MID_NameThread:
						var nameThread = Messages.NameThread.Read(m_reader);
						//asume that dead threads can't be renamed
						UpdateThread(nameThread.ThreadId, true, nameThread.Name);
						break;

					case MessageId.MID_Sample:
						var sample = Messages.Sample.Read(m_reader, m_functions);
						ParseSample(sample);
						break;

					case MessageId.MID_KeepAlive:
						//don't really need to do anything
						Debug.WriteLine("Keep alive.");
						break;

					default:
#if DEBUG
						Debugger.Break();
#endif
						throw new InvalidOperationException();
					//break;
				}

				return string.Empty;
			}
			catch(IOException)
			{
				return null;
			}
		}

		private void MapFunction(Messages.MapFunction mapFunc)
		{
			if(m_functions.ContainsKey(mapFunc.FunctionId))
				return;

			FunctionInfo funcInfo = new FunctionInfo();
			funcInfo.FunctionId = mapFunc.FunctionId;
			funcInfo.ClassId = mapFunc.ClassId;
			funcInfo.Name = mapFunc.Name;
			funcInfo.Signature = mapFunc.Signature;
			funcInfo.IsNative = mapFunc.IsNative;
			m_functions.Add(funcInfo.FunctionId, funcInfo);

			if(!m_classes.ContainsKey(funcInfo.ClassId))
				RequestClassMapping(funcInfo.ClassId);

			m_storage.MapFunction(funcInfo);

			Debug.WriteLine(string.Format("Mapped {0} to {1}.", mapFunc.Name, mapFunc.FunctionId));
		}

		private void MapClass(Messages.MapClass mapClass)
		{
			if(m_classes.ContainsKey(mapClass.ClassId))
				return;

			ClassInfo classInfo = new ClassInfo();
			classInfo.ClassId = mapClass.ClassId;
			classInfo.Name = mapClass.Name;
			m_classes.Add(classInfo.ClassId, classInfo);

			m_storage.MapClass(classInfo);
		}

		private void MapThread(Messages.MapThread mapThread)
		{
			ThreadInfo threadInfo = null;
			if(!m_threads.ContainsKey(mapThread.ThreadId))
			{
				threadInfo = new ThreadInfo(mapThread.ThreadId, mapThread.Name, mapThread.IsAlive);
				m_threads.Add(threadInfo.ThreadId, threadInfo);
			}
			else
			{
				threadInfo = m_threads[mapThread.ThreadId];
				threadInfo.Name = mapThread.Name;
				threadInfo.Alive = mapThread.IsAlive;
			}

			m_storage.UpdateThread(threadInfo.ThreadId, threadInfo.Alive, threadInfo.Name);
		}

		private void FunctionEvent(MessageId id, Messages.FunctionEvent funcEvent)
		{
			ThreadInfo info;
			if(!m_threads.ContainsKey(funcEvent.ThreadId))
			{
				info = new ThreadInfo(funcEvent.ThreadId, "", true);
				m_threads.Add(funcEvent.ThreadId, info);
				RequestThreadMapping(funcEvent.ThreadId);
			}
			else
			{
				info = m_threads[funcEvent.ThreadId];
			}

			if(id == MessageId.MID_EnterFunction)
			{
				info.ShadowStack.Push(new Pair<int, long>(funcEvent.FunctionId, funcEvent.TimeStamp));
			}
			else if(info.ShadowStack.Count > 0)
			{
				Debug.Assert(info.ShadowStack.Peek().First == funcEvent.FunctionId);
				Pair<int, long> funcStart = info.ShadowStack.Pop();
				m_storage.FunctionTiming(funcEvent.FunctionId, funcEvent.TimeStamp - funcStart.Second);
			}
		}

		private void ParseSample(Messages.Sample sample)
		{
			if(!m_threads.ContainsKey(sample.ThreadId))
			{
				ThreadInfo info = new ThreadInfo(sample.ThreadId, "", true);
				m_threads.Add(sample.ThreadId, info);
				RequestThreadMapping(sample.ThreadId);
			}

			foreach(var id in sample.Functions)
			{
				if(!m_functions.ContainsKey(id))
				{
					RequestFunctionMapping(id);
				}
			}

			m_storage.ParseSample(sample);
		}

		private void UpdateThread(int threadId, bool alive, string name)
		{
			ThreadInfo info;
			if(!m_threads.ContainsKey(threadId))
			{
				info = new ThreadInfo(threadId, name != null ? name : string.Empty, alive);
				m_threads.Add(threadId, info);
			}
			else
			{
				info = m_threads[threadId];
				if(name != null)
					info.Name = name;
				info.Alive = alive;
			}

			m_storage.UpdateThread(threadId, alive, name);
		}

		private void RequestFunctionMapping(int functionId)
		{
			var request = new Requests.GetFunctionMapping(functionId);
			request.Write(m_writer);
		}

		private void RequestClassMapping(int classId)
		{
			var request = new Requests.GetClassMapping(classId);
			request.Write(m_writer);
		}

		private void RequestThreadMapping(int threadId)
		{
			var request = new Requests.GetThreadMapping(threadId);
			request.Write(m_writer);
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_stream.Dispose();
		}

		#endregion
	}
}
