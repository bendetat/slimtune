#include "stdafx.h"
#include "PublicInterface.h"
#include "Version.h"
#include "Profiler.h"

extern "C" {

void GetProfilerVersion(int* major, int* minor, int* revision, int* build)
{
	if(major)
		*major = MAJOR_VERSION;
	if(minor)
		*minor = MINOR_VERSION;
	if(revision)
		*revision = REVISION_VERSION;
	if(build)
		*build = BUILD_VERSION;
}

const char* GetProfilerVersionString()
{
	return VERSION_STRING;
}

int GetProfilerMode()
{
	return g_Profiler->GetMode();
}

int IsProfilerAvailable()
{
	return g_Profiler != NULL;
}

int IsProfilerActive()
{
	return g_Profiler && g_Profiler->IsActive();
}

void SetInstrument(unsigned int id, int enable)
{
	g_Profiler->SetInstrument(id, enable != 0);
}

void SetCounterName(unsigned int counterId, const wchar_t* name)
{
	g_Profiler->SetCounterName(counterId, name);
}

void WritePerfCounter(unsigned int counterId, __int64 value)
{
	g_Profiler->WritePerfCounter(counterId, value);
}

}
