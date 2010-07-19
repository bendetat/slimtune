#ifndef PERFCOUNTER_H
#define PERFCOUNTER_H
#pragma once

#include <pdh.h>

class PerfCounter
{
public:
	PerfCounter();

	unsigned int AddCounterRaw(const std::wstring& counterPath);
	unsigned int AddInstanceCounter(const std::wstring& objectName, const std::wstring& counterName);
	unsigned int AddProcessCounter(const std::wstring& counterName) { return AddInstanceCounter(L"Process", counterName); }

	void Update();

	size_t GetCounterCount() { return m_counters.size(); }
	__int64 GetRawValue(int id);
	double GetDouble(int id);
	__int64 GetLong(int id);

private:
	PDH_HQUERY m_query;
	typedef std::vector<PDH_HCOUNTER> CounterList;
	CounterList m_counters;
	std::wstring m_processName;
	std::wstring m_instName;

	std::wstring GetInstanceName();
};

#endif
