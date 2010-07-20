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

namespace Memory
{
	volatile HANDLE g_HeapHandle = NULL;

	void Initialize()
	{
		//not at all thread safe
		if(g_HeapHandle != NULL)
			return;

		g_HeapHandle = HeapCreate(0, 4 * 1024 * 1024, 0);
	}

	void* Allocate(size_t size)
	{
		Initialize();

		if(size == 0)
			size = 4;

		void* pointer = HeapAlloc(g_HeapHandle, 0, size);
		if(pointer != NULL)
			return pointer;

		throw std::bad_alloc();
	}

	void Free(void* pointer)
	{
		if(pointer == NULL)
			return;

		Initialize();
		HeapFree(g_HeapHandle, 0, pointer);
	}
}