#include "stdafx.h"
#include "PublicInterface.h"
#include "Version.h"
#include "Profiler.h"

extern "C" {

void STDCALL GetProfilerVersion(int* major, int* minor, int* revision, int* build)
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

const char* STDCALL GetProfilerVersionString()
{
	return VERSION_STRING;
}

int STDCALL GetProfilerMode()
{
	return g_Profiler->GetMode();
}

int STDCALL IsProfilerAvailable()
{
	return g_Profiler != NULL;
}

int STDCALL IsProfilerConnected()
{
	return g_Profiler && g_Profiler->IsConnected();
}

int STDCALL IsSamplerActive()
{
	return g_Profiler && g_Profiler->IsSamplerActive();
}

void STDCALL SetSamplerActive(int active)
{
	if(g_Profiler)
		g_Profiler->SetSamplerActive(active != 0);
}

void STDCALL SetInstrument(unsigned int id, int enable)
{
	g_Profiler->SetInstrument(id, enable != 0);
}

//CounterId less than 32 is reserved for internal use
void STDCALL SetCounterName(unsigned int counterId, const wchar_t* name)
{
	if(counterId < 32)
		return;

	g_Profiler->SetCounterName(counterId, name);
}

void STDCALL WritePerfCounterInt(unsigned int counterId, __int64 value)
{
	if(counterId < 32)
		return;

	g_Profiler->WritePerfCounter(counterId, value * 1000);
}

void STDCALL WritePerfCounterFloat(unsigned int counterId, double value)
{
	if(counterId < 32)
		return;

	g_Profiler->WritePerfCounter(counterId, static_cast<__int64>(value * 1000));
}

}
