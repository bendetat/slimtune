
#ifndef CONFIG_H
#define CONFIG_H
#pragma once

enum ProfilerMode
{
	PM_Disabled = 0 ,
	PM_Sampling = 1 << 0, 
	PM_Tracing = 1 << 1,

	PM_Hybrid = PM_Sampling |  PM_Tracing,
};

struct PerformanceCounterDescriptor
{
	std::wstring ObjectName;
	std::wstring CounterName;
};

struct ProfilerConfiguration
{
	OSVERSIONINFO OSVersion;

	//General properties
	ProfilerMode Mode;
	unsigned int ListenPort;
	bool WaitForConnection;
	bool SuspendOnConnection;


	//Intervals specified in milliseconds.
	unsigned int SampleInterval;

	//Performance Counter Properties
	unsigned int CounterInterval;
	std::vector<PerformanceCounterDescriptor> Counters;

	ProfilerConfiguration();
	bool LoadConfiguration();
};

#endif