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
#include "stdafx.h"
#include "Profiler.h"
#include "NativeHooks.h"
#include "Timer.h"
#include "SigFormat.h"
#include "dbghelp.h"

// global reference to the profiler object (ie this) used by the static functions
ClrProfiler* g_Profiler = NULL;

struct IoThreadFunc
{
	IoThreadFunc(IProfilerServer& server) : m_server(server)
	{ }

	void operator()()
	{
		m_server.Run();
	}

private:
	IProfilerServer& m_server;

	void operator=(const IoThreadFunc&) { }
};

//This is used to block StackWalk64 from trying to go to disk for symbols, which can cause deadlocks
void* CALLBACK FunctionTableAccess(HANDLE hProcess, DWORD64 AddrBase)
{
	return NULL;
}

ClrProfiler::ClrProfiler()
: m_server(NULL),
m_suspended(0),
m_instDepth(0)
{
#ifdef _DEBUG
	__debugbreak();
#endif

	refCount = 1;
	InterlockedIncrement((volatile LONG *)&ComServerLocks);

	m_modules.reserve(16);
	m_classes.reserve(512);
	m_functions.reserve(4096);

	const wchar_t* invalidName = L"$INVALID$";

	FunctionInfo* invalidFunc = new FunctionInfo(this, 0, 0);
	invalidFunc->Name = invalidName;
	m_functions.push_back(invalidFunc);

	ClassInfo* invalidClass = new ClassInfo(0, 0);
	invalidClass->Name = invalidName;
	m_classes.push_back(invalidClass);

	ModuleInfo* invalidMod = new ModuleInfo(0);
	invalidMod->Name = invalidName;
	m_modules.push_back(invalidMod);

	m_allocatedPending = false;
	m_collectionInProgress = 0;
}

ClrProfiler::~ClrProfiler()
{
	InterlockedDecrement((volatile LONG *)&ComServerLocks);
}

HRESULT ClrProfiler::QueryInterface(REFIID riid, void **ppvObject)
{
	if(riid == IID_IUnknown)
	{
		*ppvObject = (IUnknown *)this;
	}
	else if(riid == IID_ICorProfilerCallback)
	{
		*ppvObject = (ICorProfilerCallback *)this;
	}
	else if(riid == IID_ICorProfilerCallback2)
	{
		*ppvObject = (ICorProfilerCallback2 *)this;
	}
	else if(riid == IID_ICorProfilerCallback3)
	{
		*ppvObject = (ICorProfilerCallback3 *)this;
	}
	else
	{
		*ppvObject = NULL;
		return E_NOINTERFACE;
	}

	AddRef();
	return S_OK;
}

ULONG ClrProfiler::AddRef()
{
	return InterlockedIncrement((volatile LONG *)&refCount);
}

ULONG ClrProfiler::Release()
{
	int newRefCount = InterlockedDecrement((volatile LONG *)&refCount);

	if(newRefCount == 0)
	{
		delete this;
		return 0;
	}
	else
	{
		return newRefCount;
	}
}

STDMETHODIMP ClrProfiler::Initialize(IUnknown *pICorProfilerInfoUnk)
{
	//Get the COM interfaces
	HRESULT hr = pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo, (void**) &m_ProfilerInfo);
	if (FAILED(hr))
		return E_FAIL;

	hr = pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo2, (void**) &m_ProfilerInfo2);
	if (FAILED(hr))
	{
		//we've decided not to support .NET before 2.0.
		return E_FAIL;
	}

	//Query for .NET 4.0 (not required)
	pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo3, (void**) &m_ProfilerInfo3);

	m_config.LoadEnv();
	//m_config.SampleUnmanaged = true;
#ifdef X64
	//nothing else supported right now
	m_config.Mode = PM_Sampling;
#endif

	//set up basic profiler info
	hr = SetInitialEventMask();
	assert(SUCCEEDED(hr));

	if(m_ProfilerInfo3 != 0)
		hr = m_ProfilerInfo3->SetFunctionIDMapper2(StaticFunctionMapper2, this);
	else
		hr = m_ProfilerInfo2->SetFunctionIDMapper(StaticFunctionMapper);
	assert(SUCCEEDED(hr));

#ifdef X86
	if(m_config.Mode & PM_Tracing)
	{
		hr = m_ProfilerInfo2->SetEnterLeaveFunctionHooks2(FunctionEnterNaked, FunctionLeaveNaked, FunctionTailcallNaked);
		assert(SUCCEEDED(hr));
	}
#endif

	//set up dbghelp
	if(!SymInitializeLocal())
		return E_FAIL;
	SymSetOptionsPtr(SYMOPT_UNDNAME);
	SymInitializePtr(GetCurrentProcess(), NULL, TRUE);

	//CONFIG: Server type?
	m_samplerActive = false;
	CoCreateGuid(&m_sessionId);
	m_server.reset(IProfilerServer::CreateSocketServer(*this, static_cast<unsigned short>(m_config.ListenPort), m_lock));
	m_server->SetCallbacks(boost::bind(&ClrProfiler::OnConnect, this), boost::bind(&ClrProfiler::OnDisconnect, this));
	m_server->Start();

	//initialize timing (and use cycle timing if enabled and on Vista+)
	InitializeTimer(/*m_config.CycleTiming && m_config.Version.dwMajorVersion >= 6*/ false);

	//initialize counters
	m_counter.reset(new PerfCounter());
	for(size_t i = 0; i < m_config.Counters.size(); ++i)
	{
		int id = 0;
		std::wstring& objectName = m_config.Counters[i].first;
		std::wstring& counterName = m_config.Counters[i].second;
		if(objectName.size() == 0)
			id = m_counter->AddCounterRaw(counterName);
		else
			id = m_counter->AddInstanceCounter(objectName, counterName);

		if(objectName.size() > 0)
			SetCounterName(id, (boost::wformat(L"%1%: %2%") % objectName % counterName).str());
		else
			SetCounterName(id, counterName);
	}
	if(m_config.CounterInterval < 50)
		m_config.CounterInterval = 50;
	CreateTimerQueueTimer(&m_counterTimer, NULL, &ClrProfiler::OnCounterTimerGlobal, this,
		m_config.CounterInterval, m_config.CounterInterval, WT_EXECUTEDEFAULT);

	// set up our global access pointer
	g_Profiler = this;

	m_samplerActive = (m_config.Mode & PM_Sampling) != 0;
	if(m_config.WaitForConnection)
		m_server->WaitForConnection();

	//kick off the IO thread
	IoThreadFunc threadFunc(*m_server);
	m_ioThread.reset(new boost::thread(threadFunc));

	return S_OK;
}

STDMETHODIMP ClrProfiler::Shutdown()
{
	//terminate the sampler
	StopSampleTimer();

	//if we hold the lock when the server is going down, we can deadlock
	{
		//force everything else to finish
		Mutex::scoped_lock EnterLock(m_lock);

		//shut off profiling (in case we're unfortunate enough to get an activate request right here)
		g_Profiler = NULL;
		m_config.Mode = PM_Disabled;
		m_samplerActive = false;

		timeEndPeriod(1);
	}

	//take down the IO system
	m_server->Stop();
	m_ioThread->join();

	return S_OK;
}

void ClrProfiler::OnConnect()
{
	//Hybrid will pass this conditional
	if(m_config.Mode & PM_Sampling)
	{
		timeBeginPeriod(1);
		StartSampleTimer(200);
	}

	m_connected = true;

	if(m_config.SuspendOnConnection)
		SuspendTarget();
}

void ClrProfiler::OnDisconnect()
{
	m_connected = false;
}

HRESULT ClrProfiler::SetInitialEventMask()
{
	//COR_PRF_MONITOR_NONE	= 0,
	//COR_PRF_MONITOR_FUNCTION_UNLOADS	= 0x1,
	//COR_PRF_MONITOR_CLASS_LOADS	= 0x2,
	//COR_PRF_MONITOR_MODULE_LOADS	= 0x4,
	//COR_PRF_MONITOR_ASSEMBLY_LOADS	= 0x8,
	//COR_PRF_MONITOR_APPDOMAIN_LOADS	= 0x10,
	//COR_PRF_MONITOR_JIT_COMPILATION	= 0x20,
	//COR_PRF_MONITOR_EXCEPTIONS	= 0x40,
	//COR_PRF_MONITOR_GC	= 0x80,
	//COR_PRF_MONITOR_OBJECT_ALLOCATED	= 0x100,
	//COR_PRF_MONITOR_THREADS	= 0x200,
	//COR_PRF_MONITOR_REMOTING	= 0x400,
	//COR_PRF_MONITOR_CODE_TRANSITIONS	= 0x800,
	//COR_PRF_MONITOR_ENTERLEAVE	= 0x1000,
	//COR_PRF_MONITOR_CCW	= 0x2000,
	//COR_PRF_MONITOR_REMOTING_COOKIE	= 0x4000 | COR_PRF_MONITOR_REMOTING,
	//COR_PRF_MONITOR_REMOTING_ASYNC	= 0x8000 | COR_PRF_MONITOR_REMOTING,
	//COR_PRF_MONITOR_SUSPENDS	= 0x10000,
	//COR_PRF_MONITOR_CACHE_SEARCHES	= 0x20000,
	//COR_PRF_MONITOR_CLR_EXCEPTIONS	= 0x1000000,
	//COR_PRF_MONITOR_ALL	= 0x107ffff,
	//COR_PRF_ENABLE_REJIT	= 0x40000,
	//COR_PRF_ENABLE_INPROC_DEBUGGING	= 0x80000,
	//COR_PRF_ENABLE_JIT_MAPS	= 0x100000,
	//COR_PRF_DISABLE_INLINING	= 0x200000,
	//COR_PRF_DISABLE_OPTIMIZATIONS	= 0x400000,
	//COR_PRF_ENABLE_OBJECT_ALLOCATED	= 0x800000,
	// New in VS2005
	//	COR_PRF_ENABLE_FUNCTION_ARGS	= 0x2000000,
	//	COR_PRF_ENABLE_FUNCTION_RETVAL	= 0x4000000,
	//  COR_PRF_ENABLE_FRAME_INFO	= 0x8000000,
	//  COR_PRF_ENABLE_STACK_SNAPSHOT	= 0x10000000,
	//  COR_PRF_USE_PROFILE_IMAGES	= 0x20000000,
	// End New in VS2005
	//COR_PRF_ALL	= 0x3fffffff,
	//COR_PRF_MONITOR_IMMUTABLE	= COR_PRF_MONITOR_CODE_TRANSITIONS | COR_PRF_MONITOR_REMOTING | COR_PRF_MONITOR_REMOTING_COOKIE | COR_PRF_MONITOR_REMOTING_ASYNC | COR_PRF_MONITOR_GC | COR_PRF_ENABLE_REJIT | COR_PRF_ENABLE_INPROC_DEBUGGING | COR_PRF_ENABLE_JIT_MAPS | COR_PRF_DISABLE_OPTIMIZATIONS | COR_PRF_DISABLE_INLINING | COR_PRF_ENABLE_OBJECT_ALLOCATED | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_USE_PROFILE_IMAGES

	if(m_config.Mode == PM_Disabled)
		return m_ProfilerInfo->SetEventMask(COR_PRF_MONITOR_NONE);

	//Definitely need config for flags, although GC and ObjectAllocated might be one control.

	//initialize with the required flags 
	DWORD eventMask = 0;
	eventMask |= COR_PRF_USE_PROFILE_IMAGES;
	//We want to know what threads are active
	eventMask |= COR_PRF_MONITOR_THREADS;
	//Need this to be able to map modules properly
	eventMask |= COR_PRF_MONITOR_MODULE_LOADS;
	//Inlining can result in rather confusing traces
	if(!m_config.AllowInlining)
		eventMask |= COR_PRF_DISABLE_INLINING;

	if(m_config.TrackGarbageCollections)
	{
		eventMask |= COR_PRF_MONITOR_GC;
	}

	if(m_config.TrackObjectAllocations)
	{
		eventMask |= COR_PRF_MONITOR_CLASS_LOADS;
		eventMask |= COR_PRF_MONITOR_OBJECT_ALLOCATED | COR_PRF_ENABLE_OBJECT_ALLOCATED;
	}

	//enabling stack snapshots causes instrumentation to switch to slow mode sadly
	if(m_config.Mode & PM_Sampling)
	{
		eventMask |= COR_PRF_ENABLE_STACK_SNAPSHOT;
	}

	if(m_config.Mode & PM_Tracing)
	{
		eventMask |= COR_PRF_MONITOR_ENTERLEAVE;
		eventMask |= COR_PRF_MONITOR_CODE_TRANSITIONS;
	}

	m_eventMask = eventMask;
	HRESULT hr = m_ProfilerInfo->SetEventMask(m_eventMask);
	assert(SUCCEEDED(hr));
	return hr;
}

const FunctionInfo* ClrProfiler::GetFunction(unsigned int id)
{
	if(!IsConnected())
		return 0;

	Mutex::scoped_lock EnterLock(m_lock);

	if(id >= m_functions.size())
		return NULL;

	FunctionInfo* info = m_functions[id];
	if(info->Name.size() == 0 && !m_suspended)
	{
		//we're not currently suspended, try to map this function
		if(info->IsNative)
			MapUnmanaged(info->NativeId);
		else
			MapFunction(info->NativeId, false);
	}

	return info;
}

const ClassInfo* ClrProfiler::GetClass(unsigned int id)
{
	if(!IsConnected())
		return 0;

	Mutex::scoped_lock EnterLock(m_lock);

	if(id >= m_classes.size())
		return NULL;

	ClassInfo* info = m_classes[id];
	if(info->Name.size() == 0 && !m_suspended)
	{
		MapClass(info->NativeId, NULL);
	}

	return m_classes[id];
}

const ThreadInfo* ClrProfiler::GetThread(unsigned int id)
{
	if(!IsConnected())
		return 0;

	Mutex::scoped_lock EnterLock(m_lock);

	ThreadMap::iterator infoIt = m_threads.find(id);
	if(infoIt == m_threads.end())
		return NULL;

	return infoIt->second;
}

void ClrProfiler::SetInstrument(unsigned int id, bool enable)
{
	if(!IsConnected())
		return;

	Mutex::scoped_lock EnterLock(m_lock);

	if(id >= m_functions.size())
		return;

	m_functions[id]->TriggerInstrumentation = enable;
}

UINT_PTR ClrProfiler::StaticFunctionMapper2(FunctionID functionId, void* clientData, BOOL* pbHookFunction)
{
	UINT_PTR retVal = functionId;
	ClrProfiler* profiler = static_cast<ClrProfiler*>(clientData);
	if(profiler == NULL)
		return functionId;

	retVal = profiler->MapFunction(functionId, true);

	if(profiler->GetMode() & PM_Tracing)
	{
		*pbHookFunction = TRUE;

		if(!profiler->m_config.InstrumentSmallFunctions)
		{
			ModuleID moduleId;
			mdToken token;
			ULONG methodSize;

			HRESULT hr = profiler->m_ProfilerInfo->GetFunctionInfo(functionId, NULL, &moduleId, &token);
			if(FAILED(hr))
				return retVal;

			hr = profiler->m_ProfilerInfo->GetILFunctionBody(moduleId, token, NULL, &methodSize);
			if(FAILED(hr))
				return retVal;

			if(methodSize < 64)
				*pbHookFunction = FALSE;
		}
	}
	else
	{
		*pbHookFunction = FALSE;
	}

	return retVal;
}

// this function is called by the CLR when a function has been mapped to an ID
UINT_PTR ClrProfiler::StaticFunctionMapper(FunctionID functionId, BOOL* pbHookFunction)
{
	return StaticFunctionMapper2(functionId, g_Profiler, pbHookFunction);
}

unsigned int ClrProfiler::MapModule(ModuleID moduleId)
{
	return 0;
}

unsigned int ClrProfiler::ClassIdFromTypeDefAndModule(mdTypeDef classDef, ModuleID module)
{
	unsigned short classDefLow = classDef & 0xffff;
	unsigned short moduleLow = module & 0xffff;
	unsigned int fullClassId = (moduleLow << 16) | (classDefLow);

	return fullClassId;
}

unsigned int ClrProfiler::MapClass(mdTypeDef classDef, IMetaDataImport* metadata)
{
	assert(classDef != NULL);
	Mutex::scoped_lock EnterLock(m_lock);

	//a TypeDef is only unique within its module, so we'll combine the module and class
	//if metadata is NULL, assume classDef is already fully combined
	UINT_PTR fullClassId = (UINT_PTR) classDef;
	ModuleInfo* moduleInfo = NULL;
	if(metadata != NULL)
	{
		//Get the MVID first
		GUID mvid;
		HRESULT hr = metadata->GetScopeProps(NULL, 0, NULL, &mvid);
		if(FAILED(hr))
			return 0;

		//look up the ModuleInfo using the MVID key
		moduleInfo = m_moduleLookup[mvid];
		if(moduleInfo == NULL)
			return 0;

		//put together a unique class identifier
		fullClassId = ClassIdFromTypeDefAndModule(classDef, moduleInfo->Id);
	}
	else
	{
		//decode the classDef to get the module id
		unsigned int moduleId = (fullClassId & 0xffff0000) >> 16;
		moduleInfo = m_modules[moduleId];
	}

	//Look up the ClassInfo, or create a new one
	ClassInfo* info;
	unsigned int& newId = m_classRemapper[fullClassId];
	if(newId == 0)
	{
		newId = m_classRemapper.Alloc();
		info = new ClassInfo(newId, fullClassId);
		m_classes.push_back(info);
	}
	else
	{
		info = m_classes[newId];
	}

	//Get the class name if we don't have it
	//This is unsafe if we're currently suspended
	if(info->Name.size() == 0 && !m_suspended)
	{
		//get a metadata if we don't have one
		if(metadata == NULL)
		{
			SlimComPtr<IMetaDataImport> freshMetadata;
			HRESULT hr = m_ProfilerInfo->GetModuleMetaData(moduleInfo->NativeId, ofRead, IID_IMetaDataImport, (IUnknown**) &freshMetadata);
			if(FAILED(hr))
				return newId;
			metadata = freshMetadata;
		}

		//Get the actual class name
		ULONG classLength = Messages::MapClass::MaxNameSize;
		wchar_t className[Messages::MapClass::MaxNameSize];
		HRESULT hr = metadata->GetTypeDefProps(classDef, className, classLength, &classLength, 0, 0);
		if(FAILED(hr))
			return newId;

		if(hr == S_FALSE)
		{
			//This happens with a few types, like <Module> and <CrtImplementationDetails>
			info->Name = L"[none]";
		}
		else
		{
			info->Name = std::wstring(&className[0], &className[classLength - 1]);
			assert(wcslen(info->Name.c_str()) == info->Name.size());
		}
	}

	return newId;
}

UINT_PTR ClrProfiler::MapFunction(FunctionID functionId, bool deferNameLookup)
{
	Mutex::scoped_lock EnterLock(m_lock);

	//Look up the FunctionInfo or create a new one
	FunctionInfo* info;
	unsigned int& newId = m_functionRemapper[functionId];
	if(newId == 0)
	{
		newId = m_functionRemapper.Alloc();
		info = new FunctionInfo(this, newId, functionId);
		info->IsNative = FALSE;
		m_functions.push_back(info);
	}
	else
	{
		info = m_functions[newId];
	}

	//Get the name and signature if we don't have it
	//this is unsafe if we're currently suspended
	if(!deferNameLookup && info->Name.size() == 0 && !m_suspended)
	{
		Messages::MapFunction mapFunction;
		mapFunction.FunctionId = info->Id;
		mapFunction.IsNative = FALSE;

		//get the method name and class
		ULONG nameLength = Messages::MapFunction::MaxNameSize;
		ULONG signatureLength = Messages::MapFunction::MaxSignatureSize;
		HRESULT hr = GetMethodInfo(functionId, mapFunction.Name, nameLength, mapFunction.ClassId, mapFunction.Signature, signatureLength);
		if(FAILED(hr))
		{
			//Unable to look up the name; not entirely sure why this can happen but it can.
			//We just write some placeholders and continue.
			wchar_t placeholder[] = L"$Unknown$";
			nameLength = sizeof(placeholder) / sizeof(wchar_t) - 1;
			wcscpy_s(mapFunction.Name, Messages::MapFunction::MaxNameSize, placeholder);
			mapFunction.Signature[0] = 0;
			signatureLength = 0;
			mapFunction.ClassId = 0;
		}

		info->Name = std::wstring(&mapFunction.Name[0], &mapFunction.Name[nameLength - 1]);
		info->ClassId = mapFunction.ClassId;
		if(signatureLength > 0)
			info->Signature = std::wstring(&mapFunction.Signature[0], &mapFunction.Signature[signatureLength - 1]);
	}

	return (UINT_PTR) info;
}

unsigned int ClrProfiler::MapUnmanaged(UINT_PTR address)
{
	Mutex::scoped_lock EnterLock(m_lock);

	//copied from http://msdn.microsoft.com/en-us/library/ms680578%28VS.85%29.aspx
	ULONG64 buffer[(sizeof(SYMBOL_INFO) +
		MAX_SYM_NAME * sizeof(TCHAR) +
		sizeof(ULONG64) - 1) /
		sizeof(ULONG64)];
	PSYMBOL_INFO pSymbol = (PSYMBOL_INFO) buffer;
	pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
	pSymbol->MaxNameLen = MAX_SYM_NAME;

	DWORD64 displacement;
	BOOL symResult = SymFromAddrPtr(GetCurrentProcess(), address, &displacement, pSymbol);
	if(!symResult)
		return 0;

	address -= (DWORD) displacement;
	unsigned int& id = m_functionRemapper[address];
	if(id == 0)
	{
		id = m_functionRemapper.Alloc();
		FunctionInfo* info = new FunctionInfo(this, id, address);
		m_functions.push_back(info);

		Messages::MapFunction mapFunction = {0};
		mapFunction.FunctionId = id;
		mapFunction.IsNative = TRUE;
		mapFunction.Signature[0] = 0;

		wcsncpy_s(mapFunction.Name, Messages::MapFunction::MaxNameSize, pSymbol->Name, _TRUNCATE);

		//send the map message
		mapFunction.Write(*m_server, pSymbol->NameLen, 0);
		info->Name = std::wstring(mapFunction.Name, pSymbol->NameLen);
		info->IsNative = TRUE;
		info->ClassId = 0;
	}

	return id;
}

//The returned lengths INCLUDE the null terminator on the string
//They are buffer sizes, not string lengths
HRESULT ClrProfiler::GetMethodInfo(FunctionID functionId, LPWSTR functionName, ULONG& maxFunctionLength,
								   unsigned int& classId, LPWSTR signature, ULONG& maxSignatureLength)
{
	HRESULT hr = S_OK;
	mdToken funcToken = 0;
	const ULONG originalMaxFunctionLength = maxFunctionLength;

	SlimComPtr<IMetaDataImport> metaData;
	SlimComPtr<IMetaDataImport2> metaData2;

	hr = m_ProfilerInfo->GetTokenAndMetaDataFromFunction(functionId, IID_IMetaDataImport, (IUnknown**) &metaData, &funcToken);
	CHECK_HR(hr);

	hr = metaData->QueryInterface(IID_IMetaDataImport2, (void**) &metaData2);
	CHECK_HR(hr);

	//Get the method name
	mdTypeDef classTypeDef;
	DWORD methodAttribs = 0;
	PCCOR_SIGNATURE sigBlob = NULL;
	ULONG sigBlobSize = 0;
	hr = metaData->GetMethodProps(funcToken, &classTypeDef, functionName, maxFunctionLength,
		&maxFunctionLength, &methodAttribs, &sigBlob, &sigBlobSize, 0, 0);
	CHECK_HR(hr);

	//Map the class, and prepend the class name to the method name
	classId = MapClass(classTypeDef, metaData2);
	if(classId != 0)
	{
		ClassInfo* classInfo = m_classes[classId];

#pragma warning(push)
#pragma warning(disable:4996)
		if(classInfo->Name.size() > 0)
		{
			std::copy(functionName, functionName + maxFunctionLength, functionName + classInfo->Name.size() + 1);
			std::copy(classInfo->Name.begin(), classInfo->Name.end(), functionName);
			functionName[classInfo->Name.size()] = L'.';
			maxFunctionLength += static_cast<ULONG>(classInfo->Name.size() + 1);

			assert(wcslen(functionName) + 1 == maxFunctionLength);
		}
#pragma warning(pop)
	}

	//Find any generic parameters and fill them into the name
	HCORENUM hEnum = 0;
	mdGenericParam genericParams[32] = {0};
	ULONG genericParamCount = 32;
	//returns S_FALSE if there are no generic params
	hr = metaData2->EnumGenericParams(&hEnum, funcToken, genericParams, 32, &genericParamCount);
	if(FAILED(hr))
		return hr;
	if(genericParamCount > 0)
	{
		wchar_t* end = functionName + maxFunctionLength - 1;
		*end++ = L'<';
		*end = 0;

		wchar_t genericParamName[512];
		ULONG genericNameLength = 512;
		for(ULONG g = 0; g < genericParamCount; ++g)
		{
			hr = metaData2->GetGenericParamProps(genericParams[g], NULL, NULL, NULL, NULL, genericParamName, genericNameLength, &genericNameLength);
			if(FAILED(hr))
				return hr;

			wcscpy_s(end, originalMaxFunctionLength - (end - functionName), genericParamName);
			end += genericNameLength - 1;
		}
		*end++ = L'>';
		*end = 0;
		maxFunctionLength = static_cast<ULONG>(end - functionName + 1);
		assert(wcslen(functionName) == maxFunctionLength - 1);
	}

	//Set up the signature parser
	signature[0] = 0;
	SigFormat formatter(signature, maxSignatureLength, funcToken, metaData, metaData2);
	formatter.Parse((sig_byte*) sigBlob, sigBlobSize);
	maxSignatureLength = static_cast<ULONG>(formatter.GetLength() + 1);
	assert(wcslen(signature) + 1 == maxSignatureLength);

	//TODO: Extract Method
	//Look up the debug symbol for this thing and try to figure out where it lives
	ULONG64 buffer[(sizeof(SYMBOL_INFO) +
		MAX_SYM_NAME * sizeof(TCHAR) +
		sizeof(ULONG64) - 1) /
		sizeof(ULONG64)];
	PSYMBOL_INFO pSymbol = (PSYMBOL_INFO) buffer;
	pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
	pSymbol->MaxNameLen = MAX_SYM_NAME;

	/*ModuleID moduleId = 0;
	m_ProfilerInfo->GetFunctionInfo(functionId, NULL, &moduleId, NULL);
	LPCBYTE moduleAddr = 0;
	m_ProfilerInfo->GetModuleInfo(moduleId, &moduleAddr, 0, 0, 0, 0);
	BOOL tokenResult = SymFromToken(GetCurrentProcess(), reinterpret_cast<DWORD64>(moduleAddr), funcToken, pSymbol);
	if(tokenResult)
	{
		IMAGEHLP_LINE64 lineInfo;
		lineInfo.SizeOfStruct = sizeof(IMAGEHLP_LINE64);
		BOOL lineResult = SymGetLineFromAddr64(GetCurrentProcess(), pSymbol->Address, NULL, &lineInfo);
		if(!lineResult)
		{
			DWORD err = GetLastError();
			err = 0;
		}
	}*/
	
	return S_OK;
}

bool ClrProfiler::SuspendTarget()
{
	LONG oldValue = InterlockedExchangeAdd(&m_suspended, 1);
	if(oldValue < 0)
	{
		//something got seriously screwed up
		return false;
	}

	Mutex::scoped_lock EnterLock(m_lock);

	for(ThreadMap::iterator it = m_threads.begin(); it != m_threads.end(); ++it)
	{
		if(it->second->Destroyed)
			continue;

		DWORD threadId = it->second->SystemId;
		HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME | THREAD_QUERY_INFORMATION | THREAD_GET_CONTEXT, FALSE, threadId);
		if(hThread == NULL)
		{
			//Couldn't access the thread for whatever reason
			continue;
		}

		SuspendThread(hThread);
		CloseHandle(hThread);
	}

	return true;
}

bool ClrProfiler::ResumeTarget()
{
	LONG oldValue = InterlockedExchangeAdd(&m_suspended, -1);
	if(oldValue < 1)
	{
		//totally bogus call
		InterlockedExchangeAdd(&m_suspended, 1);
		return false;
	}

	Mutex::scoped_lock EnterLock(m_lock);

	for(ThreadMap::iterator it = m_threads.begin(); it != m_threads.end(); ++it)
	{
		if(it->second->Destroyed)
			continue;

		DWORD threadId = it->second->SystemId;
		HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME | THREAD_QUERY_INFORMATION | THREAD_GET_CONTEXT, FALSE, threadId);
		if(hThread == NULL)
		{
			//Couldn't access the thread for whatever reason
			continue;
		}

		ResumeThread(hThread);
		CloseHandle(hThread);
	}

	return true;
}


void ClrProfiler::Enter(FunctionID functionId, FunctionInfo* info, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
{
	ThreadID thread;
	m_ProfilerInfo->GetCurrentThreadID(&thread);

	//look up the thread context
	ContextList::iterator it = m_threadContexts.find(thread);
	if(it == m_threadContexts.end())
	{
		//we can't do anything at all without a context!
		assert(it != m_threadContexts.end());
		it = m_threadContexts.find(thread);
		return;
	}

	ThreadContext& context = it->second;

	//Disables override triggers, and the top level function that disables is still traced
	//Disables are functional in instrumentation AND hybrid mode, triggers in hybrid only
	if(info->TriggerInstrumentation)
	{
		//this is a trigger function, we'll need to increment the count
		++context.InstCount;
	}

	if(info->DisableInstrumentation)
	{
		//increment the disable count, we're not supposed to be tracing past here
		++context.DisableCount;
	}

	if(context.InstCount == 0 && m_config.Mode != PM_Tracing)
	{
		//instrumentation isn't currently active
		return;
	}

	if(context.DisableCount > 0)
	{
		//if this is the disabling function, we still want to trace it
		//count >1 means someone above disabled, same if we didn't set the count to 1
		if(context.DisableCount > 1 || !info->DisableInstrumentation)
		{
			//instrumentation is currently disabled by a higher level function
			return;
		}
	}

	Messages::FunctionELT enterMsg;
	enterMsg.ThreadId = context.Id;
	enterMsg.FunctionId = info->Id;
	QueryTimer(enterMsg.TimeStamp);

	//We are in danger of colliding with our sampler from here onwards, so flag m_instDepth
	InterlockedIncrement(&m_instDepth);

	//update the shadow stack
	context.ShadowStack.push_back(info->Id);
	//write the message
	if(IsConnected())
		enterMsg.Write(*m_server, MID_EnterFunction);
	//Safe again
	InterlockedDecrement(&m_instDepth);
}

void ClrProfiler::LeaveImpl(FunctionID functionId, FunctionInfo* info, MessageId message)
{
	ThreadID thread;
	m_ProfilerInfo->GetCurrentThreadID(&thread);

	//look up the thread context
	ContextList::iterator it = m_threadContexts.find(thread);
	if(it == m_threadContexts.end())
	{
		//we can't do anything at all without a context!
		assert(it != m_threadContexts.end());
		return;
	}

	ThreadContext& context = it->second;

	if(info->TriggerInstrumentation && context.InstCount > 0)
	{
		//leaving a trigger function, restore the trigger count
		--context.InstCount;
	}

	if(info->DisableInstrumentation && context.DisableCount > 0)
	{
		//this is a disable function, restore the disable count
		--context.DisableCount;
	}

	if(context.DisableCount > 0)
	{
		//still disabled
		return;
	}

	if(context.InstCount == 0 && !info->TriggerInstrumentation && m_config.Mode != PM_Tracing)
	{
		//instrumentation isn't active and this leave didn't change that
		return;
	}

	Messages::FunctionELT leaveMsg;
	leaveMsg.ThreadId = context.Id;
	leaveMsg.FunctionId = info->Id;
	QueryTimer(leaveMsg.TimeStamp);

	//We are in danger of colliding with our sampler from here onwards, so flag m_instDepth
	InterlockedIncrement(&m_instDepth);

	//update shadow stack
	context.ShadowStack.pop_back();
	//write the message
	if(IsConnected())
		leaveMsg.Write(*m_server, message);
	//Safe again
	InterlockedDecrement(&m_instDepth);
}

void ClrProfiler::Leave(FunctionID functionId, FunctionInfo* info, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE *argumentRange)
{
	LeaveImpl(functionId, info, MID_LeaveFunction);
}

void ClrProfiler::Tailcall(FunctionID functionId, FunctionInfo* info, COR_PRF_FRAME_INFO frameInfo)
{
	LeaveImpl(functionId, info, MID_TailCall);
}

HRESULT ClrProfiler::StackWalkGlobal(FunctionID funcId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE contextBytes[], void *clientData)
{
	if(funcId == 0)
		return S_OK;

	WalkData* data = static_cast<WalkData*>(clientData);
	//CONTEXT* context = reinterpret_cast<CONTEXT*>(contextBytes);
	FunctionInfo* info = reinterpret_cast<FunctionInfo*>(data->profiler->MapFunction(funcId, true));
	data->functions->push_back(info->Id);
	return S_OK;
}

HRESULT ClrProfiler::StackWalkGlobal_OneShot(FunctionID funcId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE contextBytes[], void *clientData)
{
	if(funcId == 0)
		return S_OK;

	WalkData* data = static_cast<WalkData*>(clientData);
	FunctionInfo* info = reinterpret_cast<FunctionInfo*>(data->profiler->MapFunction(funcId, true));
	data->functions->push_back(info->Id);
	return S_FALSE;
}

void ClrProfiler::OnSampleTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired)
{
	ClrProfiler* profiler = static_cast<ClrProfiler*>(lpParameter);
	profiler->OnSampleTimer();
}

void ClrProfiler::StartSampleTimer(DWORD duration)
{
	if(duration == 0)
		duration = m_config.SampleInterval;

	CreateTimerQueueTimer(&m_sampleTimer, NULL, &ClrProfiler::OnSampleTimerGlobal, this, duration, 0, WT_EXECUTEDEFAULT);
}

void ClrProfiler::StopSampleTimer()
{
	BOOL result = DeleteTimerQueueTimer(NULL, m_sampleTimer, INVALID_HANDLE_VALUE);
	if(!result)
	{
		//wait a little while just in case the timer is currently running
		Sleep(100);
	}
}

void ClrProfiler::OnSampleTimer()
{
	Mutex::scoped_lock EnterLock(m_lock);

	//don't sample if we're not connected or not active
	if(!IsSamplerActive() || !IsConnected())
	{
		//but keep the timer alive if we are connected
		if(IsConnected())
			StartSampleTimer(0);
		return;
	}

	//in case we get activated during start up, wait for function mapping to start
	if(m_functions.size() == 0)
	{
		StartSampleTimer(0);
		return;
	}

	if(m_suspended || !SuspendTarget())
	{
		//already suspended or can't suspend, try again later
		StartSampleTimer(20);
		return;
	}

	if(m_instDepth > 0)
	{
		//We're inside the instrumenting bits of the profiler, try again later
		ResumeTarget();
		StartSampleTimer(0);
		return;
	}

	HANDLE hProcess = GetCurrentProcess();

	for(ThreadMap::iterator it = m_threads.begin(); it != m_threads.end(); ++it)
	{
		ThreadInfo* threadInfo = it->second;
		if(threadInfo->Destroyed)
			continue;
		DWORD threadId = threadInfo->SystemId;

		ContextList::iterator contextIt = m_threadContexts.find(threadId);
		if(contextIt != m_threadContexts.end())
		{
			//if this thread is suspended and we're not supposed to sample suspended threads, move on
			if(!m_config.SampleSuspended && contextIt->second.Suspended)
				continue;
		}

		Messages::Sample sample;
		sample.ThreadId = it->first;

		HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME | THREAD_QUERY_INFORMATION | THREAD_GET_CONTEXT, false, threadId);
		if(hThread == NULL)
		{
			//Couldn't access the thread for whatever reason
			continue;
		}

		CONTEXT context;
		context.ContextFlags = CONTEXT_FULL;
		GetThreadContext(hThread, &context);

		std::vector<unsigned int, UIntPoolAlloc>* functions = &sample.Functions;
		functions->reserve(32);

		//Attempt an initial stackwalk with what we've got
		WalkData dataFirst = { this, functions, hProcess, hThread };
		HRESULT walkResult = m_ProfilerInfo2->DoStackSnapshot(threadInfo->NativeId, StackWalkGlobal, COR_PRF_SNAPSHOT_REGISTER_CONTEXT,
			&dataFirst, (BYTE*) &context, sizeof(CONTEXT));
		if(SUCCEEDED(walkResult) && functions->size() > 0)
		{
			//it worked, let's note the correct thread entry point and move on
			threadInfo->EntryPoint = *(functions->end() - 1);
			sample.Write(*m_server);
			CloseHandle(hThread);
			continue;
		}

		if(walkResult == CORPROF_E_STACKSNAPSHOT_UNSAFE)
		{
			//severe deadlock risk, get out of the sampler
			CloseHandle(hThread);
			break;
		}

		//Initial failed, time for the complicated version
		//See http://msdn.microsoft.com/en-us/library/bb264782.aspx
		if(functions->size() != 0)
			functions->clear();

		if(m_config.SampleUnmanaged)
		{
#ifdef X64
			unsigned int id = MapUnmanaged(context.Rip);
#else
			unsigned int id = MapUnmanaged(context.Eip);
#endif
			if(id != 0)
				functions->push_back(id);
		}

		//Stack walk to find the managed stack
		bool inManagedCode = false;
		STACKFRAME64 stackFrame = {0};
		stackFrame.AddrPC.Mode = AddrModeFlat;
		stackFrame.AddrFrame.Mode = AddrModeFlat;
		stackFrame.AddrStack.Mode = AddrModeFlat;

#ifdef X64
		stackFrame.AddrPC.Offset = context.Rip;
		stackFrame.AddrFrame.Offset = context.Rbp;
		stackFrame.AddrStack.Offset = context.Rsp;
		DWORD machineType = IMAGE_FILE_MACHINE_AMD64;
#else
		stackFrame.AddrPC.Offset = context.Eip;
		stackFrame.AddrFrame.Offset = context.Ebp;
		stackFrame.AddrStack.Offset = context.Esp;
		DWORD machineType = IMAGE_FILE_MACHINE_I386;
#endif

		FunctionID funcId = 0;
		while(StackWalk64Ptr(machineType, hProcess, hThread, &stackFrame,
			&context, NULL, FunctionTableAccess, SymGetModuleBase64Ptr, NULL))
		{
			if(stackFrame.AddrPC.Offset == stackFrame.AddrReturn.Offset)
				break;

			HRESULT funcResult = m_ProfilerInfo->GetFunctionFromIP((BYTE*) stackFrame.AddrPC.Offset, &funcId);
			if(SUCCEEDED(funcResult) && funcId != 0)
			{
#ifdef X64
				//It's unnecessary to call DoStackSnapshot on x64 -- we'll just walk ourselves
				inManagedCode = false;
				FunctionInfo* funcInfo = reinterpret_cast<FunctionInfo*>(MapFunction(funcId, true));
				functions->push_back(funcInfo->Id);
				continue;
#else
				//we found our managed stack
				inManagedCode = true;
				break;
#endif
			}
			else
			{
				//still an unmanaged function
				if(m_config.SampleUnmanaged)
				{
					unsigned int id = MapUnmanaged(static_cast<UINT_PTR>(stackFrame.AddrPC.Offset));
					if(id != 0)
						functions->push_back(id);
				}
			}
		}

		if(inManagedCode)
		{
			WalkData data = { this, functions, hProcess, hThread };
			HRESULT snapshotResult = m_ProfilerInfo2->DoStackSnapshot(threadInfo->NativeId, StackWalkGlobal, COR_PRF_SNAPSHOT_REGISTER_CONTEXT,
				&data, (BYTE*) &context, sizeof(context));
			if(FAILED(snapshotResult))
			{
				functions->clear();
			}
		}

		if(functions->size() > 0)
		{
			//we double check that the entry point makes sense with this stack
			//CLR still gives us cut-off stacks in a few cases
			unsigned int top = *(functions->end() - 1);
			if(threadInfo->EntryPoint == top)
				sample.Write(*m_server);
		}

		CloseHandle(hThread);
	}

	boost::singleton_pool<boost::pool_allocator_tag, sizeof(unsigned int)>::purge_memory();

	ResumeTarget();
	//keep the timer going if connected, regardless of whether profiling is active
	if(IsConnected())
		StartSampleTimer(0);
}

HRESULT ClrProfiler::ThreadCreated(ThreadID threadId)
{
	//Map the new thread
	unsigned int& id = m_threadRemapper[threadId];
	id = m_threadRemapper.Alloc();

	//create a context for the thread
	std::pair<ContextList::iterator, bool> contextPair = m_threadContexts.insert(threadId, ThreadContext());
	ThreadContext& context = contextPair.first->second;
	context.Id = id;
	if(m_config.Mode == PM_Tracing)
	{
		context.InstCount = 1;
	}

	ThreadInfo* info = new ThreadInfo(id, threadId, &context);
	{
		Mutex::scoped_lock EnterLock(m_lock);
		m_threads[id] = info;
	}

	Messages::CreateThread msg = { id };
	msg.Write(*m_server, MID_CreateThread);

	return S_OK;
}

HRESULT ClrProfiler::ThreadDestroyed(ThreadID threadId)
{
	//taking the lock prevents this from intersecting with the stack walk
	Mutex::scoped_lock EnterLock(m_lock);

	//it's possible we've never actually seen the thread before and have to map it
	unsigned int& id = m_threadRemapper[threadId];
	if(id == 0)
	{
		id = m_threadRemapper.Alloc();
		ThreadInfo* info = new ThreadInfo(id, threadId, NULL);
		info->Destroyed = true;
		m_threads.insert(ThreadMap::value_type(id, info));
	}
	else
	{
		m_threads[id]->Destroyed = true;
	}

	//destroy the context
	ContextList::iterator it = m_threadContexts.remove(threadId);
	//deletion is known to be safe at this point
	delete it;

	Messages::CreateThread msg = { id };
	msg.Write(*m_server, MID_DestroyThread);

	return S_OK;
}

HRESULT ClrProfiler::ThreadNameChanged(ThreadID threadId, ULONG nameLen, WCHAR name[])
{
	//it's possible we've never actually seen the thread before and have to map it
	ThreadInfo* info = NULL;
	unsigned int& id = m_threadRemapper[threadId];
	if(id == 0)
	{
		id = m_threadRemapper.Alloc();
		//create a context for the thread
		std::pair<ContextList::iterator, bool> contextPair = m_threadContexts.insert(threadId, ThreadContext());
		ThreadContext& context = contextPair.first->second;
		context.Id = id;
		if(m_config.Mode == PM_Tracing)
			context.InstCount = 1;

		//fill in the thread info
		info = new ThreadInfo(id, threadId, &contextPair.first->second);
		m_threads.insert(ThreadMap::value_type(id, info));
	}
	else
	{
		info = m_threads[id];
	}

	info->Name = std::wstring(&name[0], &name[nameLen]);
	assert(wcslen(info->Name.c_str()) == info->Name.size());

	Messages::NameThread msg;
	msg.ThreadId = id;
	wcsncpy_s(msg.Name, Messages::NameThread::MaxNameSize, name, nameLen);
	msg.Write(*m_server, nameLen);

	return S_OK;
}

HRESULT ClrProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId)
{
	Mutex::scoped_lock EnterLock(m_lock);
	unsigned int& id = m_threadRemapper[managedThreadId];
	assert(id != 0);
	m_threads[id]->SystemId = osThreadId;

	//if a thread is going past while we're supposed to be suspended, stop it
	if(m_suspended)
	{
		HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME | THREAD_QUERY_INFORMATION | THREAD_GET_CONTEXT, FALSE, osThreadId);
		if(hThread != NULL)
		{
			SuspendThread(hThread);
			CloseHandle(hThread);
		}
	}

	return S_OK;
}

HRESULT ClrProfiler::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus)
{
	if(FAILED(hrStatus))
		return S_OK;

	Mutex::scoped_lock EnterLock(m_lock);

	SlimComPtr<IMetaDataImport> metadata;
	HRESULT hr = m_ProfilerInfo2->GetModuleMetaData(moduleId, ofRead, IID_IMetaDataImport, (IUnknown**) &metadata);
	CHECK_HR(hr);
	if(metadata == NULL)
		return S_OK;

	GUID mvid;
	hr = metadata->GetScopeProps(NULL, 0, NULL, &mvid);
	CHECK_HR(hr);

	//add an entry for this module to the remapper and in the modules list
	unsigned int& newId = m_moduleRemapper[moduleId];
	newId = m_moduleRemapper.Alloc();
	ModuleInfo* info = new ModuleInfo(newId);
	info->NativeId = moduleId;
	m_modules.push_back(info);

	m_moduleLookup[mvid] = info;
	return S_OK;
}

HRESULT ClrProfiler::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL* pbUseCachedFunction)
{
	*pbUseCachedFunction = TRUE;
	return S_OK;
}

void ClrProfiler::SetCounterName(unsigned int counterId, const std::wstring& name)
{
	m_counters[counterId] = name;

	Messages::CounterName counterName;
	counterName.CounterId = counterId;
	std::copy(name.begin(), name.end(), counterName.Name);

	//flag the sampler not to suspend during write
	InterlockedIncrement(&m_instDepth);
	counterName.Write(*m_server, name.size());
	InterlockedDecrement(&m_instDepth);
}

const std::wstring& ClrProfiler::GetCounterName(unsigned int id)
{
	return m_counters[id];
}

void ClrProfiler::WritePerfCounter(unsigned int counterId, double value)
{
	Messages::PerfCounter counter;
	counter.CounterId = counterId;
	counter.Value = value;
	QueryTimer(counter.TimeStamp);

	//flag the sampler not to suspend during write
	InterlockedIncrement(&m_instDepth);
	counter.Write(*m_server);
	InterlockedDecrement(&m_instDepth);
}

void ClrProfiler::OnCounterTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired)
{
	ClrProfiler* profiler = static_cast<ClrProfiler*>(lpParameter);
	profiler->OnCounterTimer();
}

void ClrProfiler::OnCounterTimer()
{
	m_counter->Update();
	for(unsigned int i = 1; i <= m_counter->GetCounterCount(); ++i)
	{
		double value = m_counter->GetDouble(i);
		WritePerfCounter(i, value);
	}
}

void ClrProfiler::SetEventName(unsigned int eventId, const std::wstring& name)
{
	m_events[eventId] = name;

	Messages::EventName eventName;
	eventName.EventId = eventId;
	std::copy(name.begin(), name.end(), eventName.Name);

	InterlockedIncrement(&m_instDepth);
	eventName.Write(*m_server, name.size());
	InterlockedDecrement(&m_instDepth);
}

const std::wstring& ClrProfiler::GetEventName(unsigned int id)
{
	return m_events[id];
}

void ClrProfiler::BeginEvent(unsigned int id)
{
	Messages::Event beginEvent;
	beginEvent.EventId = id;
	QueryTimer(beginEvent.TimeStamp);

	InterlockedIncrement(&m_instDepth);
	beginEvent.Write(*m_server, MID_BeginEvent);
	InterlockedDecrement(&m_instDepth);
}

void ClrProfiler::EndEvent(unsigned int id)
{
	Messages::Event endEvent;
	endEvent.EventId = id;
	QueryTimer(endEvent.TimeStamp);

	InterlockedIncrement(&m_instDepth);
	endEvent.Write(*m_server, MID_EndEvent);
	InterlockedDecrement(&m_instDepth);
}

HRESULT ClrProfiler::RuntimeThreadSuspended(ThreadID threadId)
{
	__debugbreak();
	ContextList::iterator it = m_threadContexts.find(threadId);
	if(it != m_threadContexts.end())
	{
		ThreadContext& context = it->second;
		context.Suspended = true;
	}

	return S_OK;
}

HRESULT ClrProfiler::RuntimeThreadResumed(ThreadID threadId)
{
	ContextList::iterator it = m_threadContexts.find(threadId);
	if(it != m_threadContexts.end())
	{
		ThreadContext& context = it->second;
		context.Suspended = false;
	}

	return S_OK;
}

HRESULT ClrProfiler::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
	ModuleID moduleId;
	mdTypeDef classDef;
	ClassID parentClass;

	HRESULT hr = m_ProfilerInfo2->GetClassIDInfo2(classId, &moduleId, &classDef, &parentClass, 0, 0, 0);
	CHECK_HR(hr);

	SlimComPtr<IMetaDataImport2> metaData;
	hr = m_ProfilerInfo->GetModuleMetaData(moduleId, ofRead, IID_IMetaDataImport2, (IUnknown**) &metaData);
	CHECK_HR(hr);

	hr = MapClass(classDef, metaData);
	CHECK_HR(hr);

	if(m_allocatedPending)
	{
		ObjectAllocated(m_allocatedObject, m_allocatedClass);
	}

	return S_OK;
}
