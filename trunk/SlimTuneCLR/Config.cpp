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
#include "Logger.h"

ProfilerConfig::ProfilerConfig()
{
	Mode = PM_Sampling;
	ListenPort = 3000;
	WaitForConnection = false;
	SuspendOnConnection = false;

	AllowInlining = false;
	TrackGarbageCollections = false;
	TrackObjectAllocations = false;

	WeightedSampling = true;
	InstrumentSmallFunctions = false;

	SampleInterval = 3;
	SampleUnmanaged = false;
	SampleSuspended = true;

	CounterInterval = 1000;

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

void ParseVar(ProfilerConfig& config, wchar_t* var, Logger* logger)
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
	else if(_wcsicmp(var, L"trackgarbagecollections") == 0)
		ParseRef(valueStr, config.TrackGarbageCollections);
	else if(_wcsicmp(var, L"trackobjectallocations") == 0)
		ParseRef(valueStr, config.TrackObjectAllocations);
	else if(_wcsicmp(var, L"allowinlining") == 0)
		ParseRef(valueStr, config.AllowInlining);
	else if(_wcsicmp(var, L"weightedsampling") == 0)
		ParseRef(valueStr, config.WeightedSampling);
	else if(_wcsicmp(var, L"sampleinterval") == 0)
		ParseRef(valueStr, config.SampleInterval);
	else if(_wcsicmp(var, L"sampleunmanaged") == 0)
		ParseRef(valueStr, config.SampleUnmanaged);
	else if(_wcsicmp(var, L"samplesuspended") == 0)
		ParseRef(valueStr, config.SampleSuspended);
	else if(_wcsicmp(var, L"counterinterval") == 0)
		ParseRef(valueStr, config.CounterInterval);
	else
		logger->WriteEvent(Logger::WARNING, "Unknown configuration option: %s", valueStr);
}

void ParseCounter(ProfilerConfig& config, wchar_t* counter, Logger* logger)
{
	if(counter[0] == L'@')
	{
		config.Counters.push_back(std::make_pair(std::wstring(L""), std::wstring(counter)));
	}
	else
	{
		const wchar_t* split = wcsstr(counter, L"\\");
		if(split)
		{
			std::wstring object(counter, split - counter);
			std::wstring counterStr(split + 1);
			config.Counters.push_back(std::make_pair(object, counterStr));
			logger->WriteEvent(Logger::INFO, "Added counter: %s", counterStr.c_str());
		}
		else
		{
			config.Counters.push_back(std::make_pair(std::wstring(L"Process"), std::wstring(counter)));
			logger->WriteEvent(Logger::INFO, "Added counter: %s", counter);
		}
	}
}

bool ProfilerConfig::LoadEnv(Logger* logger)
{
	//load config variables
	const int kBufferSize = 2048;
	wchar_t buffer[kBufferSize];
	size_t length = 0;
	length = GetEnvironmentVariable(L"SLIMTUNE_CONFIG", buffer, kBufferSize);
	if(length == 0 || length > kBufferSize)
	{
		logger->WriteEvent(Logger::FAIL, "Could not read SLIMTUNE_CONFIG environment variable.");
		return false;
	}

	logger->WriteEvent(Logger::INFO, L"SLIMTUNE_CONFIG = %s", buffer);

	wchar_t* beginVar = buffer;
	wchar_t* endVar = NULL;
	while(*beginVar)
	{
		endVar = wcsstr(beginVar, L";");
		if(endVar == NULL)
			break;
		
		*endVar = 0;
		ParseVar(*this, beginVar, logger);
		beginVar = endVar + 1;
	}

	//load counters
	length = GetEnvironmentVariable(L"SLIMTUNE_COUNTERS", buffer, kBufferSize);
	if(length == 0 || length > kBufferSize)
	{
		logger->WriteEvent(Logger::WARNING, "Could not read SLIMTUNE_COUNTERS environment variable.");
		return true;
	}

	logger->WriteEvent(Logger::INFO, "SLIMTUNE_COUNTERS = %s", buffer);

	beginVar = buffer;
	endVar = NULL;
	while(*beginVar)
	{
		endVar = wcsstr(beginVar, L";");
		if(endVar == NULL)
			break;

		*endVar = 0;
		ParseCounter(*this, beginVar, logger);
		beginVar = endVar + 1;
	}

	return true;
}

const char* StringForBool(bool value)
{
	return value ? "YES" : "NO";
}

void ProfilerConfig::VerifySettings(Logger* logger)
{
	logger->WriteEvent(Logger::INFO, "Operating system version: %d.%d.%d %s", Version.dwMajorVersion, Version.dwMinorVersion, Version.dwBuildNumber, Version.szCSDVersion);

	logger->WriteEvent(Logger::INFO, "Listening on port %d.", ListenPort);
	logger->WriteEvent(Logger::INFO, "Wait for connection: %s", StringForBool(WaitForConnection));
	logger->WriteEvent(Logger::INFO, "Suspend connection: %s", StringForBool(SuspendOnConnection));
	logger->WriteEvent(Logger::INFO, "Wait for connection: %s", StringForBool(WaitForConnection));
	logger->WriteEvent(Logger::INFO, "Sampling interval: %d ms", SampleInterval);
	logger->WriteEvent(Logger::INFO, "Performance counter interval: %d ms", CounterInterval);

	if(Version.dwMajorVersion < 6)
	{
		logger->WriteEvent(Logger::WARNING, "Weighted sampling not available before Windows Vista.");
		WeightedSampling = false;
	}
}