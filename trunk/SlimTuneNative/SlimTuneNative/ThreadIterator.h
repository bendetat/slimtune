
#ifndef THREADITERATOR_H
#define THREADITERATOR_H
#pragma once

class ThreadIterator
{
public:
	typedef DWORD value_type;
	typedef DWORD& reference_type;
	typedef const DWORD& const_reference_type;

	//The default constructor creates the end iterator.
	ThreadIterator(); 
	explicit ThreadIterator(HANDLE process);
	~ThreadIterator();

	bool operator==(const ThreadIterator& other) const;
	bool operator!=(const ThreadIterator& other) const;

	HANDLE GetThreadHandle() const;
	const_reference_type operator*() const;

	ThreadIterator& operator++();
	ThreadIterator operator++(int);
private:
	void AdvanceThread();

	THREADENTRY32 m_threadEntry;
	HANDLE m_process;
	HANDLE m_snapshot;
	bool m_isAlive;
};



#endif THREADITERATOR_H