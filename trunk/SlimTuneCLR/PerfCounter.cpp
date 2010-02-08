#include "stdafx.h"
#include "PerfCounter.h"

PerfCounter::PerfCounter()
{
	PdhOpenQuery(NULL, NULL, &m_query);

	HMODULE hProcess = GetModuleHandle(NULL);
	wchar_t fileNameBuf[MAX_PATH];
	GetModuleFileName((HMODULE) hProcess, fileNameBuf, MAX_PATH);
	wchar_t baseNameBuf[MAX_PATH];
	_wsplitpath(fileNameBuf, NULL, NULL, baseNameBuf, NULL);
	m_processName = baseNameBuf;
	DWORD processId = GetProcessId(GetCurrentProcess());

	//we need to figure out the correct instance name
	//start by asking for all the counters which match the process name
	std::wstring wildcard = (boost::wformat(L"\\Process(%1%*)\\ID Process") % m_processName).str();
	wchar_t paths[1024];
	DWORD size = 1024;
	PdhExpandWildCardPath(NULL, wildcard.c_str(), paths, &size, 0);

	//create each ProcessID counter, check its value to see if it's the one we want
	size_t len = wcslen(paths);
	wchar_t* pathPtr = paths;
	bool found = false;
	while(len != 0)
	{
		PDH_HCOUNTER counter;
		PdhAddCounter(m_query, pathPtr, NULL, &counter);
		PdhCollectQueryData(m_query);
		PDH_RAW_COUNTER counterValue;
		PdhGetRawCounterValue(counter, 0, &counterValue);
		PdhRemoveCounter(counter);

		if(counterValue.FirstValue == processId)
		{
			found = true;
			break;
		}

		pathPtr += len + 1;
		len = wcslen(pathPtr);
	}

	if(found)
	{
		//we've got our desired instance name, which is ProcessName#N
		const wchar_t* instStart = wcschr(wildcard.c_str(), L'(') + 1;
		const wchar_t* instEnd = wcschr(wildcard.c_str(), L')') - 1;
		m_instName = std::wstring(instStart, instEnd - instStart);
	}
	else
	{
		//oh well, just roll with what we've got
		m_instName = m_processName;
	}
}

unsigned int PerfCounter::AddCounterRaw(const std::wstring& counterPath)
{
	PDH_HCOUNTER counter;
	PdhAddCounter(m_query, counterPath.c_str(), NULL, &counter);
	m_counters.push_back(counter);
	return m_counters.size();
}

unsigned int PerfCounter::AddInstanceCounter(const std::wstring& objectName, const std::wstring& counterName)
{
	PDH_HCOUNTER counter;
	std::wstring fullCounterName = (boost::wformat(L"\\%1%(%2%)\\%3%") % objectName % m_instName % counterName).str();
	PdhAddCounter(m_query, fullCounterName.c_str(), NULL, &counter);
	m_counters.push_back(counter);
	return m_counters.size();
}

void PerfCounter::Update()
{
	PdhCollectQueryData(m_query);
}

__int64 PerfCounter::GetRawValue(int id)
{
	if(id == 0)
		return 0;

	PDH_RAW_COUNTER value;
	PdhGetRawCounterValue(m_counters[id - 1], NULL, &value);
	return value.FirstValue;
}

double PerfCounter::GetDouble(int id)
{
	if(id == 0)
		return 0.0;

	PDH_FMT_COUNTERVALUE value;
	PdhGetFormattedCounterValue(m_counters[id - 1], PDH_FMT_DOUBLE, NULL, &value);
	return value.doubleValue;
}

__int64 PerfCounter::GetLong(int id)
{
	if(id == 0)
		return 0;

	PDH_FMT_COUNTERVALUE value;
	PdhGetFormattedCounterValue(m_counters[id - 1], PDH_FMT_LARGE, NULL, &value);
	return value.longValue;
}
