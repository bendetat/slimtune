#include <Pdh.h>

#ifndef PERFORMANCECOUNTERS_H
#define PERFORMANCECOUNTERS_H
#pragma once

class PerformanceCounters
{
public:
	PerformanceCounters(HANDLE process);

	unsigned int AddRawCounter(const std::wstring& counter);
	unsigned int AddInstanceCounter(const std::wstring& object, const std::wstring& counter);
	unsigned int AddProcessCounter(const std::wstring& counter);

	void Update();

	unsigned int GetCounterCount() const;

	__int64 GetRawValue(unsigned int);
	double GetDouble(unsigned int id);
	__int64 GetLong(unsigned int id);


private:
	PDH_HQUERY m_query;

	typedef std::vector<PDH_HCOUNTER> CounterList;
	CounterList m_counters;

	std::wstring m_processName;
	std::wstring m_instanceName;
};



#endif