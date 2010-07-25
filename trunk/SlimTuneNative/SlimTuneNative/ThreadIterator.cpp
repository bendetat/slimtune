#include "stdafx.h"
#include "ThreadIterator.h"

ThreadIterator::ThreadIterator(HANDLE process)
	:m_process(process)
	,m_isAlive(true)
	,m_snapshot(INVALID_HANDLE_VALUE)
{
	assert(process != INVALID_HANDLE_VALUE);

	m_snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	assert(m_snapshot != INVALID_HANDLE_VALUE);

	m_threadEntry.dwSize = sizeof(m_threadEntry);
	Thread32First(m_snapshot, &m_threadEntry);
}

ThreadIterator::ThreadIterator()
	:m_process(INVALID_HANDLE_VALUE)
	,m_isAlive(false)
	,m_snapshot(INVALID_HANDLE_VALUE)
{
	ZeroMemory(&m_threadEntry, sizeof(m_threadEntry));
}

ThreadIterator::~ThreadIterator()
{
	if (m_snapshot)
	{
		CloseHandle(m_snapshot);
	}
}

bool ThreadIterator::operator ==(const ThreadIterator& other) const
{
	if (other.m_isAlive && m_isAlive)
	{
		//If we're both alive thread iterators; check the process and thread ID.
		return
			m_process == other.m_process &&
			m_threadEntry.th32ThreadID == other.m_threadEntry.th32ThreadID;
	} else 
	{
		//If one of them is the end iterator; compare the alive signals only.
		return other.m_isAlive == m_isAlive;
	}
}

bool ThreadIterator::operator !=(const ThreadIterator& other) const
{
	return !(*this == other);
}

ThreadIterator::const_reference_type ThreadIterator::operator*() const
{
	return m_threadEntry.th32ThreadID;
}

HANDLE ThreadIterator::GetThreadHandle() const
{
	DWORD requiredAccess = THREAD_SUSPEND_RESUME | THREAD_GET_CONTEXT | THREAD_QUERY_INFORMATION;
	HANDLE result = OpenThread(requiredAccess, false, m_threadEntry.th32ThreadID);
	return result;
}


ThreadIterator& ThreadIterator::operator++()
{
	AdvanceThread();
	return *this;
}

ThreadIterator ThreadIterator::operator++(int)
{
	ThreadIterator temp(*this);
	AdvanceThread();
	return temp;
}

void ThreadIterator::AdvanceThread()
{
	DWORD processID = GetProcessId(m_process);
	while (Thread32Next(m_snapshot, &m_threadEntry))
	{
		//Check for field existence. If we're sure that we're in the same process, then we have successfully 
		//advanced to the next valid thread.
		if (m_threadEntry.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(m_threadEntry.th32OwnerProcessID) &&
			processID == m_threadEntry.th32OwnerProcessID)
		{
			m_isAlive = true;
			return;
		}
	}
	m_isAlive = false;
}