#include <ObjBase.h>
#include "PerformanceCounters.h"
#include "IProfilerData.h"
#include "IdRemapper.h"
#include "Config.h"

#ifndef PROFILER_H
#define PROFILER_H
#pragma once

class Profiler : public IProfilerData
{
	class ScopedSuspendLock
	{
	public:
		ScopedSuspendLock(Profiler * prof);
		~ScopedSuspendLock();
	private:
		Profiler * prof;
	};
public:
	explicit Profiler(HANDLE targetProcess);
	~Profiler();

	const GUID * GetSessionId();
	const FunctionInfo * GetFunction(unsigned int id);
	const ClassInfo * GetClass(unsigned int id);
	const ThreadInfo * GetThread(unsigned int id);

	void SetCounterName(unsigned int id, const std::wstring& name);
	const std::wstring& GetCounterName(unsigned int id);
	void WritePerfCounter(unsigned int id, double value);

	void SetEventName(unsigned int id, const std::wstring& name);
	const std::wstring& GetEventName(unsigned int id);
	void BeginEvent(unsigned int id);
	void EndEvent(unsigned int id);

	void SetInstrument(unsigned int id, bool enable);

	//These are used to signal thread creation and destruction.
	void ThreadCreated(DWORD threadID);
	void ThreadDestroyed(DWORD threadID);

	bool SuspendTarget();
	bool ResumeTarget();

	void SetSamplerActive(bool b);

	//Some properties
	bool IsConnected() const;
	bool IsSamplerActive() const;

private:
	//Noncopyable
	Profiler(const Profiler&);
	Profiler& operator=(const Profiler&);

	void OnConnect();
	void OnDisconnect();

	static VOID CALLBACK OnCounterTimerCallback(LPVOID param, BOOLEAN wait);
	void OnCounterTimer();

	static VOID CALLBACK OnSampleTimerCallback(LPVOID param, BOOLEAN wait);
	void OnSampleTimer();

	//A zero millisecond request will use the wait in the configuration file.
	static const DWORD USE_CONFIGURATION_WAIT = 0;
	void ResetSamplerTimer(DWORD millisecondWait);
	void StopSamplerTimer();

	//Data Mapping
	unsigned int MapFunction(DWORD64 address);
	unsigned int MapThread(HANDLE threadHandle);

	//Some important member data.
	ProfilerConfiguration m_configuration;
	PerformanceCounters m_counters;

	//Synchronization
	Mutex m_lock;
	volatile long m_instrutmentationDepth; //Stops program suspension if we're instrutmenting. 

	//Session data.
	GUID m_sesssion;
	boost::scoped_ptr<IProfilerServer> m_server;
	boost::scoped_ptr<boost::thread> m_ioThread;

	//Flags
	volatile bool m_samplerActive;
	volatile bool m_connected;
	volatile long m_suspended;
	
	//Function, Class and Thread Information containers and remappers
	IdRemapper m_functionRemapper;
	boost::ptr_vector<FunctionInfo> m_functionInfos;

	IdRemapper m_classRemapper;
	boost::ptr_vector<ClassInfo> m_classInfos;

	IdRemapper m_threadRemapper;
	typedef boost::ptr_map<unsigned int, ThreadInfo> ThreadMap;
	ThreadMap m_threadInfos;

	//Counters and Events
	typedef std::map<unsigned int, std::wstring> CounterMap;
	CounterMap m_counterNames;
	HANDLE m_counterTimer;

	typedef std::map<unsigned int, std::wstring> EventMap;
	EventMap m_eventNames;
	HANDLE m_eventTimer;

	//The Sampler
	HANDLE m_samplerTimer;

	//Target Process information
	HANDLE m_targetProcess;
};

#endif