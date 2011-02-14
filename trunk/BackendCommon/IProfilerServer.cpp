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
#include "IProfilerServer.h"
#include "SocketServer.h"
#include "IProfilerData.h"

IProfilerServer* IProfilerServer::CreateSocketServer(IProfilerData& profiler, unsigned short port, Mutex& lock)
{
	return new SocketServer(profiler, port, lock);
}

char* WriteFloat(char* buffer, float value)
{
	memcpy(buffer, &value, sizeof(float));
	buffer += 4;
	return buffer;
}

char* Write7BitEncodedInt(char* buffer, unsigned int value)
{
	while(value >= 128)
	{
		*buffer++ = static_cast<char>(value | 0x80);
		value >>= 7;
	}
	*buffer++ = static_cast<char>(value);

	return buffer;
}

char* Write7BitEncodedInt64(char* buffer, unsigned __int64 value)
{
	while(value >= 128)
	{
		*buffer++ = static_cast<char>(value | 0x80);
		value >>= 7;
	}
	*buffer++ = static_cast<char>(value);

	return buffer;
}

char* WriteString(char* buffer, const wchar_t* string, size_t count)
{
	size_t size = count * sizeof(wchar_t);
	buffer = Write7BitEncodedInt(buffer, (unsigned int) size);
	memcpy(buffer, string, size);
	return buffer + size;
}

char* Read7BitEncodedInt(char* buffer, unsigned int& value)
{
	int byteval;
	int shift = 0;

	value = 0;
	while(((byteval = *buffer++) & 0x80) != 0)
	{
		value |= ((byteval & 0x7F) << shift);
		shift += 7;
	}

	return buffer;
}
