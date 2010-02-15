#ifndef PUBLICINTERFACE_H
#define PUBLICINTERFACE_H
#pragma once

#define DLLEXPORT __declspec(dllexport)
#ifndef STDCALL
#define STDCALL __stdcall
#endif

extern "C" {

DLLEXPORT void STDCALL GetProfilerVersion(int* major, int* minor, int* revision, int* build);
DLLEXPORT const char* STDCALL GetProfilerVersionString();
DLLEXPORT int STDCALL GetProfilerMode();
DLLEXPORT int STDCALL IsProfilerAvailable();
DLLEXPORT int STDCALL IsProfilerConnected();
DLLEXPORT int STDCALL IsSamplerActive();
DLLEXPORT void STDCALL SetSamplerActive(int active);
DLLEXPORT void STDCALL SetInstrument(unsigned int id, int enable);
DLLEXPORT void STDCALL SetCounterName(unsigned int counterId, const wchar_t* name);
DLLEXPORT void STDCALL WritePerfCounterInt(unsigned int counterId, __int64 value);
DLLEXPORT void STDCALL WritePerfCounterFloat(unsigned int counterId, double value);

}


#endif
