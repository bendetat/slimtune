/*****************************************************************************
 * DotNetProfiler
 * 
 * Copyright (c) 2006 Scott Hackett
 * 
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author be held liable for any damages arising from the
 * use of this software. Permission to use, copy, modify, distribute and sell
 * this software for any purpose is hereby granted without fee, provided that
 * the above copyright notice appear in all copies and that both that
 * copyright notice and this permission notice appear in supporting
 * documentation.
 * 
 * Scott Hackett (code@scotthackett.com)
 *****************************************************************************/
/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#ifndef PROFILER_H
#define PROFILER_H
#pragma once

#include "IProfilerServer.h"
#include "IProfilerData.h"
#include "IdRemapper.h"
#include "Messages.h"
#include "Config.h"
#include "PerfCounter.h"
#include "lockfree_list.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

#define ASSERT_HR(x) _ASSERT(SUCCEEDED(x))

inline bool operator< (const GUID& lhs, const GUID& rhs)
{
	//hacktacular!
	return memcmp(&lhs, &rhs, sizeof(GUID)) < 0;
}

// ClrProfiler
class ATL_NO_VTABLE ClrProfiler :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<ClrProfiler, &CLSID_Profiler>,
	public ProfilerBase,
	public IProfilerData
{
public:
	ClrProfiler();
	virtual ~ClrProfiler();

	DECLARE_REGISTRY_RESOURCEID(IDR_PROFILER)
	BEGIN_COM_MAP(ClrProfiler)
		COM_INTERFACE_ENTRY(ICorProfilerCallback)
		COM_INTERFACE_ENTRY(ICorProfilerCallback2)
	END_COM_MAP()
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	// overridden implementations of FinalConstruct and FinalRelease
	HRESULT FinalConstruct();
	void FinalRelease();

	//public interface functions

	bool IsSamplerActive() const { return m_samplerActive; }
	bool IsConnected() const { return m_connected; }
	void SetSamplerActive(bool active) { m_samplerActive = active; }
	ProfilerMode GetMode() const { return m_config.Mode; }
	
	const FunctionInfo* GetFunction(unsigned int id);
	const ClassInfo* GetClass(unsigned int id);
	const ThreadInfo* GetThread(unsigned int id);
	const std::wstring& GetCounterName(unsigned int id);
	void SetInstrument(unsigned int id, bool enable);

	bool SuspendTarget();
	bool ResumeTarget();

	void SetCounterName(unsigned int counterId, std::wstring name);
	void WritePerfCounter(unsigned int counterId, __int64 value);

    // STARTUP/SHUTDOWN EVENTS
    STDMETHOD(Initialize)(IUnknown* pICorProfilerInfoUnk);
    STDMETHOD(Shutdown)();

	// callback functions
	void Enter(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO* argumentInfo);
	void Leave(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE* argumentRange);
	void Tailcall(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo);

	static UINT_PTR _stdcall StaticFunctionMapper(FunctionID functionId, BOOL* pbHookFunction);

	//overriden interface functions
	STDMETHOD(ObjectAllocated)(ObjectID objectId, ClassID classId);

	STDMETHOD(ThreadCreated)(ThreadID threadId);
	STDMETHOD(ThreadDestroyed)(ThreadID threadId);
	STDMETHOD(ThreadNameChanged)(ThreadID threadId, ULONG nameLen, WCHAR name[]);
	STDMETHOD(ThreadAssignedToOSThread)(ThreadID managedThreadId, DWORD osThreadId);

	STDMETHOD(ModuleLoadFinished)(ModuleID moduleId, HRESULT hrStatus);

	STDMETHOD(JITCachedFunctionSearchStarted)(FunctionID functionId, BOOL* pbUseCachedFunction);

private:
	HRESULT GetMethodInfo(FunctionID functionID,
		LPWSTR functionName, ULONG& maxFunctionLength,
		unsigned int& classId,
		LPWSTR signature, ULONG& maxSignatureLength);
	HRESULT SetInitialEventMask();

	unsigned int MapModule(ModuleID moduleId);
	unsigned int MapClass(mdTypeDef classDef, IMetaDataImport* metadata);
	UINT_PTR MapFunction(FunctionID, bool deferNameLookup);
	unsigned int MapUnmanaged(UINT_PTR address);

	void LeaveImpl(FunctionID functionId, FunctionInfo* info, MessageId message);

	//COM Interface pointers
	CComPtr<ICorProfilerInfo> m_ProfilerInfo;
	CComPtr<ICorProfilerInfo2> m_ProfilerInfo2;
	volatile LONG m_eventMask;
	ProfilerConfig m_config;

	boost::scoped_ptr<IProfilerServer> m_server;
	boost::scoped_ptr<boost::thread> m_ioThread;
	volatile bool m_samplerActive;
	volatile bool m_connected;
	volatile LONG m_suspended;
	volatile LONG m_instDepth;

	//Mainly used as a data compression trick for the communication layer
	IdRemapper m_moduleRemapper;
	IdRemapper m_classRemapper;
	IdRemapper m_threadRemapper;
	IdRemapper m_functionRemapper;

	mutable CRITICAL_SECTION m_lock;
	std::vector<ModuleInfo*> m_modules;
	std::vector<ClassInfo*> m_classes;
	std::vector<FunctionInfo*> m_functions;

	typedef std::tr1::unordered_map<unsigned int, ThreadInfo*> ThreadMap;
	ThreadMap m_threads;

	typedef lockfree_list<ThreadID, ThreadContext> ContextList;
	ContextList m_threadContexts;

	typedef std::map<GUID, ModuleInfo*> ModuleLookup;
	ModuleLookup m_moduleLookup;

	typedef std::map<unsigned int, std::wstring> CounterMap;
	CounterMap m_counters;

	HANDLE m_sampleTimer;
	HANDLE m_counterTimer;

	boost::scoped_ptr<PerfCounter> m_counter;

	void OnConnect();
	void OnDisconnect();

	static void CALLBACK OnSampleTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired);
	void OnSampleTimer();
	void StartSampleTimer(DWORD duration);
	void StopSampleTimer();

	static void CALLBACK OnCounterTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired);
	void OnCounterTimer();

	struct WalkData
	{
		ClrProfiler* profiler;
		std::vector<unsigned int, UIntPoolAlloc>* functions;
		HANDLE hProcess;
		HANDLE hThread;
	};

	static HRESULT CALLBACK StackWalkGlobal(FunctionID funcId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE context[], void *clientData);
};

OBJECT_ENTRY_AUTO(__uuidof(Profiler), ClrProfiler)

extern ClrProfiler* g_Profiler;

#endif
