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

int IsProfilerConnected()
{
	return g_Profiler && g_Profiler->IsConnected();
}

int IsSamplerActive()
{
	return g_Profiler && g_Profiler->IsSamplerActive();
}

int SetSamplerActive(int active)
{
	if(g_Profiler)
		g_Profiler->SetSamplerActive(active != 0);
}

void SetInstrument(unsigned int id, int enable)
{
	g_Profiler->SetInstrument(id, enable != 0);
}

//CounterId less than 32 is reserved for internal use
void SetCounterName(unsigned int counterId, const wchar_t* name)
{
	if(counterId < 32)
		return;

	g_Profiler->SetCounterName(counterId, name);
}

void WritePerfCounterInt(unsigned int counterId, __int64 value)
{
	if(counterId < 32)
		return;

	g_Profiler->WritePerfCounter(counterId, value * 1000);
}

void WritePerfCounterFloat(unsigned int counterId, double value)
{
	if(counterId < 32)
		return;

	g_Profiler->WritePerfCounter(counterId, static_cast<__int64>(value * 1000));
}

}
