#include "stdafx.h"
#include "PerformanceCounters.h"
#include <iostream>

//TODO: Make error handling useful.
PerformanceCounters::PerformanceCounters(HANDLE process)
	:m_instanceName(L"")
{
	PdhOpenQuery(NULL, NULL, &m_query);

	std::wstring query = L"\\Process(*)\\ID Process";

	//Attempt to expand the wild card path
	DWORD size = 0;
	PDH_STATUS result = PdhExpandWildCardPath(NULL, query.c_str(), NULL, &size, 0);
	if (result != PDH_MORE_DATA)
	{
		//Do something meaningful here.
	}

	//Add one to the required size, since XP and 2000 require it.
	size += 1;

	std::vector<wchar_t> buffer(size);
	result = PdhExpandWildCardPath(NULL, query.c_str(), &buffer[0], &size, 0);
	if (result != ERROR_SUCCESS)
	{
		//TODO: Do something meaningful here.
		return;
	}

	DWORD id = GetProcessId(process);
	//For every process, check if its PID is equal to ours.
	wchar_t * path = &buffer[0];
	size_t len = wcslen(path);

	wchar_t * found = NULL;
	while (len != 0)
	{
		PDH_HCOUNTER counter;
		PdhAddCounter(m_query, path, NULL, &counter);
		PdhCollectQueryData(m_query);

		PDH_RAW_COUNTER value;
		PdhGetRawCounterValue(counter, NULL, &value);

		PdhRemoveCounter(counter);

		if (value.FirstValue == id)
		{
			found = path;
			break;
		}

		path += len + 1;
		len = wcslen(path);
	}

	//Parse out the instance name from the path that we have. This is the value between the parenthesis
	assert(found);

	const wchar_t * nameStart = wcschr(found, L'(') + 1;
	const wchar_t * nameEnd = wcschr(found, L')'); //This points one past the end of the string.
	m_instanceName = std::wstring(nameStart, nameEnd);
	m_processName = m_instanceName;

	//Now; strip off everything behind the #, that is our process name.
	const wchar_t * processEnd = wcschr(found, L'#');
	if (processEnd)
	{
		m_processName = std::wstring(nameStart, processEnd);
	}
}

unsigned int PerformanceCounters::GetCounterCount() const
{
	return m_counters.size();
}

unsigned int PerformanceCounters::AddRawCounter(const std::wstring& path)
{
	PDH_HCOUNTER counter;
	PDH_STATUS status = PdhAddCounter(m_query, path.c_str(), NULL, &counter);
	if (status != ERROR_SUCCESS)
	{
		//Do something meaningful here.
	}
	m_counters.push_back(counter);
	return static_cast<unsigned int>(m_counters.size());
}

unsigned int PerformanceCounters::AddInstanceCounter(const std::wstring& objectName, const std::wstring& counterName)
{
	std::wstring fullPath = str(boost::wformat(L"\\%1%(%2%)\\%3%") % objectName % m_instanceName % counterName);
	return AddRawCounter(fullPath);
}

unsigned int PerformanceCounters::AddProcessCounter(const std::wstring& counter)
{
	return AddInstanceCounter(L"Process", counter);
}

void PerformanceCounters::Update()
{
	PdhCollectQueryData(m_query);
}

__int64 PerformanceCounters::GetRawValue(unsigned int id)
{
	if (id == 0)
	{
		return 0;
	}

	PDH_RAW_COUNTER value;
	PdhGetRawCounterValue(m_counters[id - 1], NULL, &value);
	return value.FirstValue;
}

double PerformanceCounters::GetDouble(unsigned int id)
{
	if (id == 0)
	{
		return 0.0;
	}

	PDH_FMT_COUNTERVALUE value;
	PdhGetFormattedCounterValue(m_counters[id - 1], PDH_FMT_DOUBLE, NULL, &value);
	return value.doubleValue;
}

__int64 PerformanceCounters::GetLong(unsigned int id)
{
	if (id == 0)
	{
		return 0;
	}

	PDH_FMT_COUNTERVALUE value;
	PdhGetFormattedCounterValue(m_counters[id - 1], PDH_FMT_LARGE, NULL, &value);
	return value.longValue;
}
