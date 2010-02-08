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
#include "Config.h"

ProfilerConfig::ProfilerConfig()
{
	Mode = PM_Sampling;
	ListenPort = 3000;
	WaitForConnection = false;
	SuspendOnConnection = false;

	TrackMemory = true;
	AllowInlining = false;

	CycleTiming = false;
	InstrumentSmallFunctions = false;

	SampleInterval = 3;
	SampleUnmanaged = false;

	Version.dwOSVersionInfoSize = sizeof(Version);
	GetVersionEx(&Version);
}

template<typename T>
T Parse(const wchar_t* str, T defaultValue)
{
	try
	{
		return boost::lexical_cast<T>(str);
	}
	catch(const std::bad_cast&)
	{
		return defaultValue;
	}
}

template<typename T>
void ParseRef(const wchar_t* str, T& value)
{
	value = Parse<T>(str, value);
}

void ParseVar(ProfilerConfig& config, wchar_t* var)
{
	wchar_t* equals = wcsstr(var, L"=");
	if(equals == NULL)
		return;

	*equals = 0;
	wchar_t* valueStr = equals + 1;
	//hard coded parse for recognized variables
	if(_wcsicmp(var, L"mode") == 0)
		config.Mode = (ProfilerMode) Parse<int>(valueStr, PM_Disabled);
	else if(_wcsicmp(var, L"port") == 0)
		ParseRef(valueStr, config.ListenPort);
	else if(_wcsicmp(var, L"wait") == 0)
		ParseRef(valueStr, config.WaitForConnection);
	else if(_wcsicmp(var, L"suspendonconnection") == 0)
		ParseRef(valueStr, config.SuspendOnConnection);
	else if(_wcsicmp(var, L"trackmemory") == 0)
		ParseRef(valueStr, config.AllowInlining);
	else if(_wcsicmp(var, L"allowinlining") == 0)
		ParseRef(valueStr, config.AllowInlining);
	else if(_wcsicmp(var, L"cycletiming") == 0)
		ParseRef(valueStr, config.CycleTiming);
	else if(_wcsicmp(var, L"sampleinterval") == 0)
		ParseRef(valueStr, config.SampleInterval);
	else if(_wcsicmp(var, L"sampleunmanaged") == 0)
		ParseRef(valueStr, config.SampleUnmanaged);
}

bool ProfilerConfig::LoadEnv()
{
	const int kBufferSize = 2048;
	wchar_t buffer[kBufferSize];
	size_t length = 0;
	length = GetEnvironmentVariable(L"SLIMTUNE_CONFIG", buffer, kBufferSize);
	if(length == 0 || length > kBufferSize)
		return false;

	wchar_t* beginVar = buffer;
	wchar_t* endVar = NULL;
	while(*beginVar)
	{
		endVar = wcsstr(beginVar, L";");
		if(endVar == NULL)
			break;
		
		*endVar = 0;
		ParseVar(*this, beginVar);
		beginVar = endVar + 1;
	}

	return true;
}
