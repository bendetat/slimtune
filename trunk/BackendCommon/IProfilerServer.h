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
#ifndef IPROFILERSERVER_H
#define IPROFILERSERVER_H
#pragma once

struct IProfilerData;
typedef boost::function<void()> ServerCallback;

//TODO: Separate protocol from communication layer
class IProfilerServer
{
public:
	static IProfilerServer* CreateSocketServer(IProfilerData& profiler, unsigned short port, Mutex& lock);

	virtual void Start() = 0;
	virtual void Run() = 0;
	virtual void Stop() = 0;

	virtual void WaitForConnection() = 0;
	virtual bool Connected() = 0;
	virtual void SetCallbacks(ServerCallback onConnect, ServerCallback onDisconnect) = 0;

	virtual void Write(const void* data, size_t sizeBytes) = 0;
};

char* Write7BitEncodedInt(char* buffer, unsigned int value);
char* Write7BitEncodedInt64(char* buffer, unsigned __int64 value);
char* WriteString(char* buffer, const wchar_t* string, size_t count);
char* WriteFloat(char* buffer, float value);

char* Read7BitEncodedInt(char* buffer, unsigned int& value);

#endif
