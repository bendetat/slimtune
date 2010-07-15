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
#include "stdafx.h"
#include "Messages.h"
#include "IProfilerServer.h"

namespace Messages
{
	typedef char MessageIdStorageType;

	void MapFunction::Write(IProfilerServer& server, size_t nameCount, size_t signatureCount)
	{
		char* const buffer = (char*) _alloca(32 + sizeof(wchar_t) * (nameCount + signatureCount));
		char* bufPtr = buffer;

		*bufPtr++ = MID_MapFunction;
		bufPtr = Write7BitEncodedInt(bufPtr, this->FunctionId);
		bufPtr = Write7BitEncodedInt(bufPtr, this->ClassId);
		bufPtr = Write7BitEncodedInt(bufPtr, this->IsNative);
		bufPtr = WriteString(bufPtr, this->Name, nameCount);
		bufPtr = WriteString(bufPtr, this->Signature, signatureCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void MapClass::Write(IProfilerServer& server, size_t nameCount)
	{
		char* const buffer = (char*) _alloca(32 + sizeof(wchar_t) * (nameCount));
		char* bufPtr = buffer;

		*bufPtr++ = MID_MapClass;
		bufPtr = Write7BitEncodedInt(bufPtr, this->ClassId);
		bufPtr = WriteString(bufPtr, this->Name, nameCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void MapThread::Write(IProfilerServer& server, size_t nameCount)
	{
		char* const buffer = (char*) _alloca(32 + sizeof(wchar_t) * (nameCount));
		char* bufPtr = buffer;

		*bufPtr++ = MID_MapThread;
		bufPtr = Write7BitEncodedInt(bufPtr, this->ThreadId);
		bufPtr = Write7BitEncodedInt(bufPtr, this->IsAlive);
		bufPtr = WriteString(bufPtr, this->Name, nameCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void FunctionELT::Write(IProfilerServer& server, MessageId id)
	{
		//comfortably big enough
		char buffer[32];
		char* bufPtr = buffer;

		*bufPtr++ = (char) id;
		bufPtr = Write7BitEncodedInt(bufPtr, ThreadId);
		bufPtr = Write7BitEncodedInt(bufPtr, FunctionId);
		bufPtr = Write7BitEncodedInt64(bufPtr, TimeStamp);

		server.Write(buffer, bufPtr - buffer);
	}

	void CreateThread::Write(IProfilerServer& server, MessageId id)
	{
		char buffer[16];
		char* bufPtr = buffer;

		*bufPtr++ = (char) id;
		bufPtr = Write7BitEncodedInt(bufPtr, ThreadId);

		server.Write(buffer, bufPtr - buffer);
	}

	void NameThread::Write(IProfilerServer& server, size_t nameCount)
	{
		char* const buffer = (char*) _alloca(16 + sizeof(wchar_t) * nameCount);
		char* bufPtr = buffer;

		*bufPtr++ = MID_NameThread;
		bufPtr = Write7BitEncodedInt(bufPtr, ThreadId);
		bufPtr = WriteString(bufPtr, this->Name, nameCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void Sample::Write(IProfilerServer& server)
	{
		char* const buffer = (char*) _alloca(24 + 10 * Functions.size());
		char* bufPtr = buffer;

		*bufPtr++ = MID_Sample;
		bufPtr = Write7BitEncodedInt(bufPtr, ThreadId);
		bufPtr = Write7BitEncodedInt(bufPtr, (unsigned int) Functions.size());
		for(size_t i = 0; i < Functions.size(); ++i)
		{
			bufPtr = Write7BitEncodedInt(bufPtr, Functions[i]);
		}

		server.Write(buffer, bufPtr - buffer);
	}

	void ObjectAllocated::Write(IProfilerServer& server)
	{
		char buffer[32];
		char* bufPtr = buffer;

		*bufPtr++ = MID_ObjectAllocated;
		bufPtr = Write7BitEncodedInt(bufPtr, ClassId);
		bufPtr = Write7BitEncodedInt64(bufPtr, Size);
		bufPtr = Write7BitEncodedInt(bufPtr, FunctionId);
		bufPtr = Write7BitEncodedInt64(bufPtr, TimeStamp);

		server.Write(buffer, bufPtr - buffer);
	}

	void PerfCounter::Write(IProfilerServer& server)
	{
		char buffer[24];
		char* bufPtr = buffer + 1;
		unsigned char* bufTmp = (unsigned char*) buffer;

		*bufTmp = MID_PerfCounter;
		bufPtr = Write7BitEncodedInt(bufPtr, CounterId);
		bufPtr = Write7BitEncodedInt64(bufPtr, TimeStamp);
		*(double*)bufPtr = Value;
		assert(sizeof(double) == 8);
		bufPtr += 8;

		server.Write(buffer, bufPtr - buffer);
	}

	void CounterName::Write(IProfilerServer& server, size_t nameCount)
	{
		char* const buffer = (char*) _alloca(16 + sizeof(wchar_t) * nameCount);
		char* bufPtr = buffer + 1;
		unsigned char* bufTmp = (unsigned char*) buffer;

		*bufTmp = MID_CounterName;
		bufPtr = Write7BitEncodedInt(bufPtr, CounterId);
		bufPtr = WriteString(bufPtr, Name, nameCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void EventName::Write(IProfilerServer& server, size_t nameCount)
	{
		char* const buffer = (char*) _alloca(16 + sizeof(wchar_t) * nameCount);
		char* bufPtr = buffer + 1;
		unsigned char* bufTmp = (unsigned char*) buffer;

		*bufTmp = MID_EventName;
		bufPtr = Write7BitEncodedInt(bufPtr, EventId);
		bufPtr = WriteString(bufPtr, Name, nameCount);

		server.Write(buffer, bufPtr - buffer);
	}

	void Event::Write(IProfilerServer& server, MessageId id)
	{
		char buffer[32];
		char* bufPtr = buffer + 1;
		unsigned char* bufTmp = (unsigned char*) buffer;

		*bufTmp = (unsigned char) id;
		bufPtr = Write7BitEncodedInt(bufPtr, EventId);
		bufPtr = Write7BitEncodedInt64(bufPtr, TimeStamp);

		server.Write(buffer, bufPtr - buffer);
	}
}

namespace Requests
{
	GetFunctionMapping GetFunctionMapping::Read(char* buffer, size_t& bytesRead)
	{
		GetFunctionMapping result;
		result.FunctionId = *(int*) buffer;
		bytesRead += sizeof(int);
		return result;
	}

	GetClassMapping GetClassMapping::Read(char* buffer, size_t& bytesRead)
	{
		GetClassMapping result;
		result.ClassId = *(int*) buffer;
		bytesRead += sizeof(int);
		return result;
	}

	GetThreadMapping GetThreadMapping::Read(char* buffer, size_t& bytesRead)
	{
		GetThreadMapping result;
		result.ThreadId = *(int*) buffer;
		bytesRead += sizeof(int);
		return result;
	}

	GetCounterName GetCounterName::Read(char* buffer, size_t& bytesRead)
	{
		GetCounterName result;
		result.CounterId = *(int*) buffer;
		bytesRead += sizeof(int);
		return result;
	}

	GetEventName GetEventName::Read(char* buffer, size_t& bytesRead)
	{
		GetEventName result;
		result.EventId = *(int*) buffer;
		bytesRead += sizeof(int);
		return result;
	}

	SetFunctionFlags SetFunctionFlags::Read(char* buffer, size_t& bytesRead)
	{
		SetFunctionFlags result;
		result.FunctionId = *(unsigned int*) buffer;
		buffer += sizeof(unsigned int);
		result.Flags = *buffer;

		bytesRead += sizeof(unsigned int) + 1;
		return result;
	}
}