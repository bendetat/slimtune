#include "stdafx.h"
#include "Config.h"


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

void ParseVar(ProfilerConfiguration& config, wchar_t* var)
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
	else if(_wcsicmp(var, L"sampleinterval") == 0)
		ParseRef(valueStr, config.SampleInterval);
	else if(_wcsicmp(var, L"counterinterval") == 0)
		ParseRef(valueStr, config.CounterInterval);
}

void ParseCounter(ProfilerConfiguration& config, wchar_t* counter)
{
	if(counter[0] == L'@')
	{
		PerformanceCounterDescriptor desc;
		desc.ObjectName = L"";
		desc.CounterName = counter;

		config.Counters.push_back(desc);
	}
	else
	{
		const wchar_t* split = wcsstr(counter, L"\\");
		if(split)
		{
			PerformanceCounterDescriptor desc;
			desc.ObjectName = std::wstring(counter, split - counter);
			desc.CounterName = std::wstring(split + 1);

			config.Counters.push_back(desc);
		}
		else
		{
			PerformanceCounterDescriptor desc;
			desc.ObjectName = L"Process";
			desc.CounterName = counter;

			config.Counters.push_back(desc);
		}
	}
}


ProfilerConfiguration::ProfilerConfiguration()
	:Mode(PM_Sampling)
	,ListenPort(3000)
	,WaitForConnection(false)
	,SuspendOnConnection(false)
	,SampleInterval(40)
	,CounterInterval(1000)
{
	OSVersion.dwOSVersionInfoSize = sizeof(OSVersion);
	GetVersionEx(&OSVersion);
}



bool ProfilerConfiguration::LoadConfiguration()
{
	//Use as much of the options from SLIMTUNE_CONFIG as applicable.
	size_t length = GetEnvironmentVariable(L"SLIMTUNE_CONFIG", NULL, 0);
	if(length == 0)
		return false;

	std::vector<wchar_t> buffer(length);
	size_t obtainedLength = GetEnvironmentVariable(L"SLIMTUNE_CONFIG", &buffer[0], buffer.size());
	if (obtainedLength == 0 || obtainedLength > buffer.size())
	{
		return false;
	}

	wchar_t * begin = &buffer[0];
	wchar_t * end = NULL;
	while (*begin)
	{
		end = wcsstr(begin, L";");
		if (!end)
		{
			break;
		}

		*end = NULL;
		ParseVar(*this, begin);
		begin = end + 1;
	}

	length = GetEnvironmentVariable(L"SLIMTUNE_COUNTERS", NULL, 0);
	buffer.resize(length);
	obtainedLength = GetEnvironmentVariable(L"SLIMTUNE_COUNTERS", &buffer[0], 0);
	if (obtainedLength == 0 || obtainedLength > buffer.size())
	{
		return false;
	}

	begin = &buffer[0];
	end = NULL;
	
	while(*begin)
	{
		end = wcsstr(begin, L";");
		if (!end)
		{
			break;
		}

		*end = 0;
		ParseCounter(*this, begin);
		begin = end + 1;
	}

	return true;
}
