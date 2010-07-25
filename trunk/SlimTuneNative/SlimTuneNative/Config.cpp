#include "stdafx.h"
#include "Config.h"

ProfilerConfiguration::ProfilerConfiguration()
	:Mode(PM_Sampling)
	,ListenPort(3000)
	,WaitForConnection(true)
	,SuspendOnConnection(false)
	,SampleInterval(50)
	,CounterInterval(1000)
{
	OSVersion.dwOSVersionInfoSize = sizeof(OSVersion);
	GetVersionEx(&OSVersion);
}

bool ProfilerConfiguration::LoadConfiguration()
{
	//TODO: FIll this out.
	return true;
}