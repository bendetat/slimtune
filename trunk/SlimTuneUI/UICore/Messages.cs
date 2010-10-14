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
using System.Text;
using System.Collections.Generic;

namespace UICore
{
	public enum MessageId : byte
	{
		MID_MapFunction = 0x01,
		MID_MapClass,
		MID_MapModule,
		MID_MapAssembly,
		MID_MapThread,

		MID_EnterFunction = 0x10,
		MID_LeaveFunction,
		MID_TailCall,
		MID_TransitionOut,				//From managed to unmanaged (out of the runtime)
		MID_TransitionIn,				//From unmanaged to managed (back into the runtime)

		MID_ObjectAllocated = 0x20,
		MID_GarbageCollected,
		MID_GenerationSizes,

		MID_CreateThread = 0x40,
		MID_DestroyThread,
		MID_NameThread,

		MID_Sample = 0x50,

		MID_PerfCounter = 0xe0,
		MID_CounterName,

		MID_BeginEvent = 0xf0,
		MID_EndEvent,

		MID_KeepAlive = 0xff,
	};

	public enum ClientRequest : byte
	{
		CR_GetFunctionMapping = 0x01,
		CR_GetClassMapping,
		CR_GetModuleMapping,
		CR_GetAssemblyMapping,
		CR_GetThreadMapping,
		CR_GetCounterName,

		CR_GetThreadInfo = 0x10,

		CR_SetSamplerActive = 0x60,

		CR_Suspend = 0x70,
		CR_Resume,

		CR_SetFunctionFlags = 0x80,
	};

	namespace Messages
	{
		public struct MapFunction
		{
			public const int MaxNameSize = 1024;
			public const int MaxSignatureSize = 2048;

			public int FunctionId;
			public int ClassId;
			public bool IsNative;
			public string Name;
			public string Signature;

			public static MapFunction Read(BinaryReader reader)
			{
				MapFunction result = new MapFunction();

				result.FunctionId = Utilities.Read7BitEncodedInt(reader);
				result.ClassId = Utilities.Read7BitEncodedInt(reader);
				result.IsNative = Utilities.Read7BitEncodedInt(reader) != 0;
				result.Name = reader.ReadString();
				result.Signature = reader.ReadString();

				return result;
			}
		}

		public struct MapClass
		{
			public const int MaxNameSize = 1024;

			public int ClassId;
			public bool IsValueType;
			public string Name;

			public static MapClass Read(BinaryReader reader)
			{
				MapClass result = new MapClass();

				result.ClassId = Utilities.Read7BitEncodedInt(reader);
				result.IsValueType = Utilities.Read7BitEncodedInt(reader) != 0;
				result.Name = reader.ReadString();

				return result;
			}
		}

		public struct MapThread
		{
			public const int MaxNameSize = 256;

			public int ThreadId;
			public bool IsAlive;
			public string Name;

			public static MapThread Read(BinaryReader reader)
			{
				MapThread result = new MapThread();

				result.ThreadId = Utilities.Read7BitEncodedInt(reader);
				result.IsAlive = Utilities.Read7BitEncodedInt(reader) != 0;
				result.Name = reader.ReadString();

				return result;
			}
		}

		public struct FunctionEvent
		{
			public int ThreadId;
			public int FunctionId;
			public long TimeStamp;

			public static FunctionEvent Read(BinaryReader reader)
			{
				FunctionEvent result = new FunctionEvent();

				result.ThreadId = Utilities.Read7BitEncodedInt(reader);
				result.FunctionId = Utilities.Read7BitEncodedInt(reader);
				result.TimeStamp = Utilities.Read7BitEncodedInt64(reader);

				return result;
			}
		}

		public struct CreateThread
		{
			public int ThreadId;

			public static CreateThread Read(BinaryReader reader)
			{
				CreateThread result = new CreateThread();
				result.ThreadId = Utilities.Read7BitEncodedInt(reader);
				return result;
			}
		}

		public struct NameThread
		{
			public int ThreadId;
			public string Name;

			public static NameThread Read(BinaryReader reader)
			{
				NameThread result = new NameThread();
				result.ThreadId = Utilities.Read7BitEncodedInt(reader);
				result.Name = reader.ReadString();

				return result;
			}
		}

		public struct Sample
		{
			public int ThreadId;
			public List<int> Functions;

			public static Sample Read(BinaryReader reader, Dictionary<int, FunctionInfo> funcDict)
			{
				Sample result = new Sample();

				result.ThreadId = Utilities.Read7BitEncodedInt(reader);
				int count = Utilities.Read7BitEncodedInt(reader);
				result.Functions = new List<int>(count);
				for(int i = 0; i < count; ++i)
				{
					int id = Utilities.Read7BitEncodedInt(reader);
					if(id > 0)
						result.Functions.Add(id);
				}

				return result;
			}
		}

		public struct PerfCounter
		{
			public int CounterId;
			public long TimeStamp;
			public double Value;

			public static PerfCounter Read(BinaryReader reader)
			{
				PerfCounter result = new PerfCounter();
				result.CounterId = Utilities.Read7BitEncodedInt(reader);
				result.TimeStamp = Utilities.Read7BitEncodedInt64(reader);
				result.Value = reader.ReadDouble();

				return result;
			}
		}

		public struct CounterName
		{
			public int CounterId;
			public string Name;

			public static CounterName Read(BinaryReader reader)
			{
				CounterName result = new CounterName();
				result.CounterId = Utilities.Read7BitEncodedInt(reader);
				result.Name = reader.ReadString();

				return result;
			}
		}

		public struct ObjectAllocated
		{
			public int ClassId;
			public long Size;
			public int FunctionId;
			public long TimeStamp;

			public static ObjectAllocated Read(BinaryReader reader)
			{
				var result = new ObjectAllocated();
				result.ClassId = Utilities.Read7BitEncodedInt(reader);
				result.Size = Utilities.Read7BitEncodedInt64(reader);
				result.FunctionId = Utilities.Read7BitEncodedInt(reader);
				result.TimeStamp = Utilities.Read7BitEncodedInt64(reader);
				return result;
			}
		}

		public struct GarbageCollected
		{
			public int Generation;
			public int FunctionId;
			public long TimeStamp;

			public static GarbageCollected Read(BinaryReader reader)
			{
				var result = new GarbageCollected();
				result.Generation = Utilities.Read7BitEncodedInt(reader);
				result.FunctionId = Utilities.Read7BitEncodedInt(reader);
				result.TimeStamp = Utilities.Read7BitEncodedInt64(reader);
				return result;
			}
		}

		public struct GenerationSizes
		{
			public int Generations;
			public long[] Sizes;
			public long TimeStamp;

			public static GenerationSizes Read(BinaryReader reader)
			{
				var result = new GenerationSizes();
				result.Generations = Utilities.Read7BitEncodedInt(reader);
				result.Sizes = new long[result.Generations];
				for(int i = 0; i < result.Sizes.Length; ++i)
					result.Sizes[i] = Utilities.Read7BitEncodedInt64(reader);
				result.TimeStamp = Utilities.Read7BitEncodedInt64(reader);
				return result;
			}
		}
	}

	namespace Requests
	{
		struct GetFunctionMapping
		{
			public int FunctionId;

			public GetFunctionMapping(int functionId)
			{
				FunctionId = functionId;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((byte) ClientRequest.CR_GetFunctionMapping);
				writer.Write(FunctionId);
			}
		}

		struct SetFunctionFlags
		{
			public int FunctionId;
			public bool Enable;

			public SetFunctionFlags(int functionId, bool enable)
			{
				FunctionId = functionId;
				Enable = enable;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((byte) ClientRequest.CR_SetFunctionFlags);
				writer.Write(FunctionId);
				writer.Write(Enable);
			}
		}

		struct GetClassMapping
		{
			public int ClassId;

			public GetClassMapping(int classId)
			{
				ClassId = classId;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((byte) ClientRequest.CR_GetClassMapping);
				writer.Write(ClassId);
			}
		}

		struct GetThreadMapping
		{
			public int ThreadId;

			public GetThreadMapping(int classId)
			{
				ThreadId = classId;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((byte) ClientRequest.CR_GetThreadMapping);
				writer.Write(ThreadId);
			}
		}

		struct GetCounterName
		{
			public int CounterId;

			public GetCounterName(int counterId)
			{
				CounterId = counterId;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((byte) ClientRequest.CR_GetCounterName);
				writer.Write(CounterId);
			}
		}
	}
}
