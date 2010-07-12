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
#include "RingBuffer.h"
#include <cassert>

RingBuffer::RingBuffer(LONG size)
: m_bufferRoot(new char[size]),
m_size(size),
m_cursor(0)
{
}

RingBuffer::~RingBuffer()
{
}

char* RingBuffer::Alloc(LONG size)
{
	assert(size >= 0);
	assert(size < m_size);

#ifdef LOCKLESS
	//Move the buffer pointer up and get the old value, which we need
	LONG offset = InterlockedExchangeAdd(&m_cursor, (LONG) size);
	//this is inverted since the latter check will almost always pass
	if(offset + size > m_size && offset <= m_size)
	{
		//we pushed the cursor past the end of the buffer, so we move it back to the beginning
		//(known because old offset is inside, new offset is outside)
		InterlockedExchange(&m_cursor, size);
		offset = 0;
	}
	else if(offset >= m_size)
	{
		//allocation is past the end and we didn't do it (both old and new offset are outside)
		//we will spin trying to allocate until it's corrected
		//I may regret this, it kind of seems like a recipe for priority inversion
		//At least it's a race condition, so it should be very rare
		do
		{
			Sleep(0);
			offset = InterlockedExchangeAdd(&m_cursor, (LONG) size);
		} while(offset > m_size);
	}
#else
	//Simplified implementation for testing purposes only!
	LONG offset = m_cursor;
	m_cursor += size;
	if(m_cursor > m_size)
	{
		m_cursor = size;
		offset = 0;
	}
#endif

	return m_bufferRoot.get() + offset;
}

void RingBuffer::Free(LONG size)
{
}
