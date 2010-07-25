#include "stdafx.h"
#include "Profiler.h"
#include "IProfilerServer.h"
#include "Timer.h"
#include "ThreadIterator.h"
#include <iostream>

//A few RAII containers for common tasks.
Profiler::ScopedSuspendLock::ScopedSuspendLock(Profiler * p)
	:prof(p)
{
	InterlockedIncrement(&prof->m_instrutmentationDepth);
}

Profiler::ScopedSuspendLock::~ScopedSuspendLock()
{
	InterlockedDecrement(&prof->m_instrutmentationDepth);
}

namespace 
{
	//The IO Runner. Runs IO tasks for the server, used to spawn a thread.
	struct IORunner
	{
		IORunner(IProfilerServer& server)
			:m_target(server)
		{}

		void operator()()
		{
			m_target.Run();
		}

		IProfilerServer& m_target;
	};

	//Stops an input timer.
	void StopTimer(HANDLE timer)
	{
		//Terminate the counter timer.
		int errorCode = ERROR_SUCCESS;
		BOOL success = true;
		do 
		{
			success = DeleteTimerQueueTimer(NULL, timer, INVALID_HANDLE_VALUE);
			if (!success)
			{
				errorCode = GetLastError();
				Sleep(100);
			}
		} while (!success && errorCode != ERROR_IO_PENDING);
	}
}

Profiler::Profiler(HANDLE process)
	:m_instrutmentationDepth(0)
	,m_targetProcess(process)
	,m_samplerActive(false)
	,m_connected(false)
	,m_counters(process)
	,m_suspended(0)
{
	//Public symbols only so we can undecorate for parameter and return types.
	//This is relevant in case overloads do significantly different tasks.
	SymSetOptions(SYMOPT_DEBUG | SYMOPT_LOAD_LINES | SYMOPT_PUBLICS_ONLY );
	SymInitialize(m_targetProcess, NULL, TRUE);

	HRESULT hr = CoCreateGuid(&m_sesssion);
	if (FAILED(hr))
	{
		//Something really weird happened.
	}

	m_configuration.LoadConfiguration();

	//Create a set of invalid functions as dummy objects
	const wchar_t * invalidName = L"$INVALID$";

	FunctionInfo * invalidFunction = new FunctionInfo(this, 0, 0);
	invalidFunction->Name = invalidName;
	m_functionInfos.push_back(invalidFunction);

	ClassInfo * invalidClass = new ClassInfo(0, 0);
	invalidClass->Name = invalidName;
	m_classInfos.push_back(invalidClass);

	ThreadInfo * invalidThread = new ThreadInfo();
	invalidThread->Name = invalidName;

	unsigned int zero = 0;
	m_threadInfos.insert(zero, invalidThread);

	//Startup the server and IO threads
	m_server.reset(IProfilerServer::CreateSocketServer(*this, m_configuration.ListenPort, m_lock));
	m_server->SetCallbacks(boost::bind(&Profiler::OnConnect, this),boost::bind(&Profiler::OnDisconnect, this));
	m_server->Start();

	//Setup the timers for timestamps
	InitializeTimer(false);

	for (size_t i = 0; i < m_configuration.Counters.size(); ++i)
	{
		PerformanceCounterDescriptor desc = m_configuration.Counters[i];

		if (desc.ObjectName.size() == 0)
		{
			unsigned int id = m_counters.AddRawCounter(desc.CounterName);
			SetCounterName(id, desc.CounterName);
		} else 
		{
			unsigned int id = m_counters.AddInstanceCounter(desc.ObjectName, desc.CounterName);
			SetCounterName(id, str(boost::wformat(L"%1%: %2%") % desc.ObjectName % desc.CounterName));
		}
	}

	if (m_configuration.CounterInterval < 50)
	{
		m_configuration.CounterInterval = 50;
	}

	CreateTimerQueueTimer(
		&m_counterTimer, 
		NULL, 
		&Profiler::OnCounterTimerCallback, 
		this, 
		m_configuration.CounterInterval, 
		m_configuration.CounterInterval, 
		WT_EXECUTEDEFAULT);

	m_samplerActive = (m_configuration.Mode & PM_Sampling) != 0;
	if (m_configuration.WaitForConnection)
	{
		m_server->WaitForConnection();
	}

	//Start the IO Threads.
	IORunner runner(*m_server);
	m_ioThread.reset(new boost::thread(runner));
}

Profiler::~Profiler()
{
	SymCleanup(m_targetProcess);
	StopTimer(m_counterTimer);
}

void Profiler::OnConnect()
{
	if ((m_configuration.Mode & PM_Sampling) != 0)
	{
		timeBeginPeriod(1);
		ResetSamplerTimer(200);
	}

	m_connected = true;

	if (m_configuration.SuspendOnConnection)
	{
		SuspendTarget();
	}
}

void Profiler::OnDisconnect()
{
	m_connected = false;
	timeEndPeriod(1);
}

void Profiler::ResetSamplerTimer(DWORD time)
{
	DWORD milliseconds = time;
	if (milliseconds == USE_CONFIGURATION_WAIT)
	{
		milliseconds = static_cast<DWORD>(m_configuration.SampleInterval);
	}

	CreateTimerQueueTimer(&m_samplerTimer, NULL, &Profiler::OnSampleTimerCallback, this, milliseconds, 0, WT_EXECUTEDEFAULT);
}

void Profiler::StopSamplerTimer()
{
	StopTimer(m_samplerTimer);
}

bool Profiler::SuspendTarget()
{
	LONG oldValue = InterlockedExchangeAdd(&m_suspended, 1);
	if (oldValue < 0)
	{
		//Weird.
		return false;
	}

	Mutex::scoped_lock EnterLock(m_lock);

	ThreadIterator it(m_targetProcess);
	ThreadIterator end;

	for (; it != end; ++it)
	{
		HANDLE hThread = it.GetThreadHandle();
		SuspendThread(hThread);
		CloseHandle(hThread);
	}
	return true;
}

bool Profiler::ResumeTarget()
{
	LONG oldValue = InterlockedExchangeAdd(&m_suspended, -1);
	if (oldValue < 1)
	{
		InterlockedExchangeAdd(&m_suspended, 1);
		return false;
	}

	Mutex::scoped_lock EnterLock(m_lock);

	ThreadIterator it(m_targetProcess);
	ThreadIterator end;

	for (; it != end; ++it)
	{
		HANDLE hThread = it.GetThreadHandle();
		ResumeThread(hThread);
		CloseHandle(hThread);
	}

	return true;
}

const GUID * Profiler::GetSessionId() 
{
	return &m_sesssion;
}

void Profiler::ThreadCreated(DWORD threadID)
{
	Mutex::scoped_lock EnterLock(m_lock);
	//Create a mapping for this thread.
	unsigned int id = m_threadRemapper.Alloc();
	m_threadRemapper[threadID] = id;

	ThreadContext * context = new ThreadContext();
	context->Id = id;

	ThreadInfo * info = new ThreadInfo(id, threadID, context);
	{
		Mutex::scoped_lock EnterLock(m_lock);
		m_threadInfos.insert(id, info);
	}

	Messages::CreateThread msg;
	msg.ThreadId = id;

	ScopedSuspendLock lock(this);
	//msg.Write(*m_server, MID_CreateThread);
}

void Profiler::ThreadDestroyed(DWORD threadID)
{
	Mutex::scoped_lock EnterLock(m_lock);

	unsigned int& id = m_threadRemapper[threadID];
	if (id == 0)
	{
		id = m_threadRemapper.Alloc();
		ThreadInfo * info = new ThreadInfo(id, threadID, NULL);
		info->Destroyed = true;
		m_threadInfos.insert(id, info);
	} else 
	{
		m_threadInfos[id].Destroyed = true;
	}

	Messages::CreateThread msg;
	msg.ThreadId = id;

	ScopedSuspendLock lock(this);
	msg.Write(*m_server, MID_DestroyThread);
}

unsigned int Profiler::MapThread(HANDLE threadHandle)
{
	Mutex::scoped_lock EnterLock(m_lock);
	DWORD threadID = GetThreadId(threadHandle);
	unsigned int id = m_threadRemapper[threadID];
	if (id == 0)
	{
		//Weird that we've never seen it before...
		ThreadCreated(threadID);
		return m_threadRemapper[threadID];
	}
	return id;
}

unsigned int Profiler::MapFunction(DWORD64 address)
{
	Mutex::scoped_lock EnterLock(m_lock);
	//The structure below uses a length 1 array to place a data buffer right after the header.
	//That is why we are going to cast a byte buffer into a pointer into said structure.
	static const size_t NAME_BUFFER_SIZE = MAX_SYM_NAME * sizeof(wchar_t);
	static const size_t SYMBOL_BUFFER_SIZE = sizeof(SYMBOL_INFO) + NAME_BUFFER_SIZE;

	//Allocate a block of memory for the symbol info.
	BYTE symbolBuffer[SYMBOL_BUFFER_SIZE];
	wchar_t undecorBuffer[MAX_SYM_NAME];

	SYMBOL_INFO  * symbol = reinterpret_cast<SYMBOL_INFO  *>(&symbolBuffer[0]);
	symbol->SizeOfStruct = sizeof(SYMBOL_INFO);
	symbol->MaxNameLen = MAX_SYM_NAME; //Note this is the number of characters.

	DWORD64 displacement = 0; 
	BOOL lookupResult = SymFromAddr(m_targetProcess, address, &displacement, symbol);

	UINT_PTR casted = static_cast<UINT_PTR>(symbol->Index);
	unsigned int& id = m_functionRemapper[casted];
	if (id == 0)
	{
		id = m_functionRemapper.Alloc();

		//Attempt to resolve a symbol from the input address.
		
		if (lookupResult)
		{
			UnDecorateSymbolName(symbol->Name, undecorBuffer, MAX_SYM_NAME, UNDNAME_COMPLETE);
			
			FunctionInfo * info = new FunctionInfo(this, id, casted);
			info->IsNative = true;
			info->ClassId = 0;

			/*
				The result from UndecorateSymbolName is either a C function, or an undecorated C++ name.
				C functions do not have any decodable signature.
				C++ functions are of the form:   access: return_type calling_convention Namespaces::Classes::Name(arguments) modifiers
					Signatures are everything after the first open paren.
					The name is Namespaces::Classes::Name.
			*/
			//Attempt to find the signature.
			const wchar_t * sigStart = wcschr(undecorBuffer, L'(');
			if (sigStart)
			{
				//The rest of the string should be a valid signature.
				info->Signature = sigStart;
			} else 
			{
				info->Signature = L"";	
				
			}

			//From the signature, find the name.
			if (sigStart)
			{
				//Find the first space before sigStart.
				const wchar_t * nameStart = sigStart;
				bool inQuotes = false;
				while (inQuotes || (*nameStart != L' ' && nameStart != undecorBuffer))
				{
					//Wow, this is a major hack. the backtick (`) starts 'vector deleting destructor', while a single quote (') ends it. 
					if (*nameStart == L'\'' || *nameStart == L'`')
					{
						inQuotes = !inQuotes;
					}
					nameStart--;
				}
				info->Name = std::wstring(nameStart + 1, sigStart);
			} else
			{
				info->Name = symbol->Name;
			}

			m_functionInfos.push_back(info);

			std::wcout << L"Mapped function: " << info->Name << info->Signature << L"\n";


			Messages::MapFunction msg;
			msg.FunctionId = id;
			msg.IsNative = true;
			wcsncpy_s(msg.Name, Messages::MapFunction::MaxNameSize, info->Name.c_str(), _TRUNCATE);
			wcsncpy_s(msg.Signature, Messages::MapFunction::MaxSignatureSize, info->Signature.c_str(), _TRUNCATE);

			ScopedSuspendLock lock(this);
			msg.Write(*m_server, info->Name.length(), info->Signature.length());
		}
	}
	return id;
}

VOID CALLBACK Profiler::OnSampleTimerCallback(LPVOID param, BOOLEAN wait)
{
	assert(wait);
	Profiler * target = reinterpret_cast<Profiler *>(param);
	target->OnSampleTimer();
}

size_t g_SampleCount = 0;
void Profiler::OnSampleTimer()
{
	Mutex::scoped_lock EnterLock(m_lock);

	if (!IsSamplerActive() || !IsConnected())
	{
		if (IsConnected())
		{
			ResetSamplerTimer(USE_CONFIGURATION_WAIT);
		}
		return;
	}

	if (m_instrutmentationDepth > 0)
	{
		ResetSamplerTimer(USE_CONFIGURATION_WAIT);
		return;
	}

	if (m_suspended || !SuspendTarget())
	{
		//Try again later~
		ResetSamplerTimer(20);
		return;
	}

	//We can now attempt to iterate the call stack.

	//TODO: Fail gracefully on things less than XP
	ThreadIterator it(m_targetProcess);
	ThreadIterator end;

	for (; it != end; ++it)
	{
		HANDLE hThread = it.GetThreadHandle();

		CONTEXT context;
		context.ContextFlags = CONTEXT_ALL;

		if (!GetThreadContext(hThread, &context))
		{
			//Urg; skip this thread.
			continue; 
		}

		STACKFRAME64 stackFrame;
		ZeroMemory(&stackFrame, sizeof(stackFrame));
		
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

		std::vector<DWORD64> frames;
		do 
		{
			frames.push_back(stackFrame.AddrPC.Offset);
		} while(StackWalk64(
			machineType,
			m_targetProcess, 
			hThread,
			&stackFrame, 
			reinterpret_cast<void *>(&context),
			NULL,
			SymFunctionTableAccess64,
			SymGetModuleBase64,
			NULL));

		Messages::Sample sample;
		sample.ThreadId = MapThread(hThread);

		//Its honestly safe to restart the thread starting from here.
		//But now we will attempt to resolve the stack frames
		for (size_t i = 0; i < frames.size(); ++i)
		{
			unsigned int id = MapFunction(frames[i]);
			sample.Functions.push_back(id);
		}
		
		if (sample.Functions.size() > 0)
		{
			
			ScopedSuspendLock lock(this);
			sample.Write(*m_server);
			std::cout << "Sample written: " << g_SampleCount << "\n";
			g_SampleCount++;
		}

		CloseHandle(hThread);
	}

	boost::singleton_pool<boost::pool_allocator_tag, sizeof(unsigned int)>::purge_memory();
	ResumeTarget();
	if (IsConnected())
	{
		ResetSamplerTimer(USE_CONFIGURATION_WAIT);
	}
}

VOID CALLBACK Profiler::OnCounterTimerCallback(LPVOID param, BOOLEAN waitTerminated)
{
	assert(waitTerminated);
	Profiler * target = reinterpret_cast<Profiler *>(param);
	target->OnCounterTimer();
}

void Profiler::OnCounterTimer()
{
	//Query the performance counters, and then send them all as doubles.
	m_counters.Update();
	for (unsigned int i = 1; i <= m_counters.GetCounterCount(); ++i)
	{
		WritePerfCounter(i, m_counters.GetDouble(i));
	}
}

void Profiler::SetCounterName(unsigned int id, const std::wstring& name)
{
	//Multiple calls to this overwrite the previous calls.
	m_counterNames[id] = name;

	Messages::CounterName msg;
	msg.CounterId = id;
	std::copy(name.begin(), name.end(), msg.Name);

	ScopedSuspendLock lock(this);
	msg.Write(*m_server, name.size());
}

const std::wstring& Profiler::GetCounterName(unsigned int id)
{
	return m_counterNames[id];
}

void Profiler::WritePerfCounter(unsigned int id, double value)
{
	Messages::PerfCounter msg;
	msg.CounterId = id;
	msg.Value = value;
	QueryTimer(msg.TimeStamp);

	ScopedSuspendLock lock(this);
	msg.Write(*m_server);
}

void Profiler::SetEventName(unsigned int id, const std::wstring& name)
{
	//Multiple calls to this overwrite the previous calls.
	m_eventNames[id] = name;

	Messages::EventName msg;
	msg.EventId = id;
	std::copy(name.begin(), name.end(), msg.Name);

	ScopedSuspendLock lock(this);
	msg.Write(*m_server, name.size());
}

const std::wstring& Profiler::GetEventName(unsigned int id)
{
	return m_eventNames[id];
}

void Profiler::BeginEvent(unsigned int id)
{
	Messages::Event msg;
	msg.EventId = id;
	QueryTimer(msg.TimeStamp);

	ScopedSuspendLock lock(this);
	msg.Write(*m_server, MID_BeginEvent);
}

void Profiler::EndEvent(unsigned int id)
{
	Messages::Event msg;
	msg.EventId = id;
	QueryTimer(msg.TimeStamp);

	ScopedSuspendLock lock(this);
	msg.Write(*m_server, MID_EndEvent);
}

const ClassInfo * Profiler::GetClass(unsigned int id)
{
	//Not sure if native code can actually do this.
	return NULL;
}

const FunctionInfo * Profiler::GetFunction(unsigned int id)
{
	if (!IsConnected())
	{
		return NULL;
	}

	Mutex::scoped_lock EnterLock(m_lock);
	if (id >= m_functionInfos.size())
	{
		return NULL;
	}
	return &m_functionInfos[id];
}

const ThreadInfo * Profiler::GetThread(unsigned int id)
{
	if (!IsConnected())
	{
		return NULL;
	}

	Mutex::scoped_lock EnterLock(m_lock);
	if (id >= m_threadInfos.size())
	{
		return NULL;
	}
	return &m_threadInfos[id];
}

void Profiler::SetSamplerActive(bool value)
{
	m_samplerActive = value;
}

void Profiler::SetInstrument(unsigned int, bool)
{
	//Not implemented.
	assert(false && "NOT IMPLEMENTED YET");
}

bool Profiler::IsSamplerActive() const
{
	return m_samplerActive;
}

bool Profiler::IsConnected() const
{
	return m_connected;
}