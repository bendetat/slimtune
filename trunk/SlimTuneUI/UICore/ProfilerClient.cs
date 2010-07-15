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
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace UICore
{
	public enum ProfilerMode
	{
		Disabled = 0,

		Sampling = 0x01,
		Tracing = 0x02,

		Hybrid = Sampling | Tracing,
	}

	public class ThreadContext
	{
		public int ThreadId;
		public string Name;
		public bool Alive;

		public Stack<Pair<int, long>> ShadowStack = new Stack<Pair<int, long>>();

		public ThreadContext(int threadId, string name, bool alive)
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
		Guid? m_sessionId;
		Dictionary<int, FunctionInfo> m_functions = new Dictionary<int, FunctionInfo>();
		Dictionary<int, ClassInfo> m_classes = new Dictionary<int, ClassInfo>();
		Dictionary<int, ThreadContext> m_threads = new Dictionary<int, ThreadContext>();
		Dictionary<int, string> m_counters = new Dictionary<int, string>();

		IDataEngine m_data;

		internal TcpClient Socket
		{
			get { return m_client; }
		}

		public string HostName { get; private set; }
		public int Port { get; private set; }

		public ProfilerClient(string host, int port, IDataEngine data)
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
			m_data = data;

			m_classes.Add(0, new ClassInfo { Id = 0, Name = "$INVALID$" });
			m_functions.Add(0, new FunctionInfo { Id = 0, ClassId = 0, IsNative = false, Name = "$INVALID$", Signature = string.Empty });

			HostName = host;
			Port = port;

			Debug.WriteLine("Successfully connected.");
		}

		public void SuspendTarget()
		{
			m_writer.Write((byte) ClientRequest.CR_Suspend);
		}

		public void ResumeTarget()
		{
			m_writer.Write((byte) ClientRequest.CR_Resume);
		}

		public void SetSamplerActive(bool active)
		{
			m_writer.Write((byte) ClientRequest.CR_SetSamplerActive);
			m_writer.Write((byte) (active ? 1 : 0));
		}

		public bool Receive()
		{
			try
			{
				if(m_stream == null)
					return true;

				if(m_sessionId == null)
				{
					byte[] guidBytes = m_reader.ReadBytes(16);
					m_sessionId = new Guid(guidBytes);
					OnSessionId();
					return true;
				}

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

					case MessageId.MID_PerfCounter:
						var counter = Messages.PerfCounter.Read(m_reader);
						ParseCounter(counter);
						break;

					case MessageId.MID_CounterName:
						var counterName = Messages.CounterName.Read(m_reader);
						NameCounter(counterName);
						break;

					case MessageId.MID_ObjectAllocated:
						var objectAllocated = Messages.ObjectAllocated.Read(m_reader);
						//Console.WriteLine("Allocated class #{0} of size {1} in function {2}.", objectAllocated.ClassId, objectAllocated.Size, objectAllocated.FunctionId);
						break;

					case MessageId.MID_GarbageCollected:
						var garbageCollected = Messages.GarbageCollected.Read(m_reader);
						GarbageCollection(garbageCollected);
						break;

					case MessageId.MID_GenerationSizes:
						var generationSizes = Messages.GenerationSizes.Read(m_reader);
						Console.WriteLine("Gen0 size: {0}", generationSizes.Sizes[0]);
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

				return true;
			}
			catch(IOException)
			{
				return false;
			}
		}

		private void OnSessionId()
		{
			Debug.Assert(m_sessionId != null);
			m_data.WriteProperty("SessionId", m_sessionId.Value.ToString());
		}

		private void MapFunction(Messages.MapFunction mapFunc)
		{
			if(m_functions.ContainsKey(mapFunc.FunctionId))
				return;

			FunctionInfo funcInfo = new FunctionInfo { Id = mapFunc.FunctionId };
			funcInfo.ClassId = mapFunc.ClassId;
			funcInfo.Name = mapFunc.Name;
			funcInfo.Signature = mapFunc.Signature;
			funcInfo.IsNative = mapFunc.IsNative;
			m_functions.Add(funcInfo.Id, funcInfo);

			if(!m_classes.ContainsKey(funcInfo.ClassId))
				RequestClassMapping(funcInfo.ClassId);

			m_data.MapFunction(funcInfo);

			Debug.WriteLine(string.Format("Mapped {0} to {1}.", mapFunc.Name, mapFunc.FunctionId));
		}

		private void MapClass(Messages.MapClass mapClass)
		{
			if(m_classes.ContainsKey(mapClass.ClassId))
				return;

			ClassInfo classInfo = new ClassInfo { Id = mapClass.ClassId };
			classInfo.Name = mapClass.Name;
			m_classes.Add(classInfo.Id, classInfo);

			m_data.MapClass(classInfo);
		}

		private void MapThread(Messages.MapThread mapThread)
		{
			ThreadContext threadInfo = null;
			if(!m_threads.ContainsKey(mapThread.ThreadId))
			{
				threadInfo = new ThreadContext(mapThread.ThreadId, mapThread.Name, mapThread.IsAlive);
				m_threads.Add(threadInfo.ThreadId, threadInfo);
			}
			else
			{
				threadInfo = m_threads[mapThread.ThreadId];
				threadInfo.Name = mapThread.Name;
				threadInfo.Alive = mapThread.IsAlive;
			}

			m_data.UpdateThread(threadInfo.ThreadId, threadInfo.Alive, threadInfo.Name);
		}

		private void FunctionEvent(MessageId id, Messages.FunctionEvent funcEvent)
		{
			ThreadContext info;
			if(!m_threads.ContainsKey(funcEvent.ThreadId))
			{
				info = new ThreadContext(funcEvent.ThreadId, "", true);
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
				m_data.FunctionTiming(funcEvent.FunctionId, funcEvent.TimeStamp - funcStart.Second);
			}
		}

		private void ParseSample(Messages.Sample sample)
		{
			if(!m_threads.ContainsKey(sample.ThreadId))
			{
				ThreadContext info = new ThreadContext(sample.ThreadId, "", true);
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

			m_data.ParseSample(sample);
		}

		private void UpdateThread(int threadId, bool alive, string name)
		{
			ThreadContext info;
			if(!m_threads.ContainsKey(threadId))
			{
				info = new ThreadContext(threadId, name != null ? name : string.Empty, alive);
				m_threads.Add(threadId, info);
			}
			else
			{
				info = m_threads[threadId];
				if(name != null)
					info.Name = name;
				info.Alive = alive;
			}

			m_data.UpdateThread(threadId, alive, info.Name);
		}

		private void NameCounter(Messages.CounterName counterName)
		{
			if(!m_counters.ContainsKey(counterName.CounterId))
				m_counters.Add(counterName.CounterId, counterName.Name);
			else
				m_counters[counterName.CounterId] = counterName.Name;

			m_data.CounterName(counterName.CounterId, counterName.Name);
		}

		private void ParseCounter(Messages.PerfCounter counter)
		{
			if(!m_counters.ContainsKey(counter.CounterId))
			{
				m_counters.Add(counter.CounterId, string.Empty);
				RequestCounterName(counter.CounterId);
			}

			m_data.PerfCounter(counter.CounterId, counter.TimeStamp, counter.Value);
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

		private void RequestCounterName(int counterId)
		{
			var request = new Requests.GetCounterName(counterId);
			request.Write(m_writer);
		}

		private void GarbageCollection(Messages.GarbageCollected gc)
		{
			m_data.GarbageCollection(gc.Generation, gc.TimeStamp);
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_stream.Dispose();
		}

		#endregion
	}
}
