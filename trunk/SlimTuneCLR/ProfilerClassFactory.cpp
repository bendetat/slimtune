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
#include "resource.h"
#include "SlimTuneProfiler.h"
#include "SlimTuneProfilerModule.h"
#include "ProfilerClassFactory.h"
#include "Profiler.h"

ProfilerClassFactory::ProfilerClassFactory()
{
	refCount = 1;
	InterlockedIncrement((volatile LONG *)&ComServerLocks);
}

ProfilerClassFactory::~ProfilerClassFactory()
{
	InterlockedDecrement((volatile LONG *)&ComServerLocks);
}

	//IUnknown Methods
HRESULT ProfilerClassFactory::QueryInterface(REFIID riid, void **ppvObject)
{
	if(riid == IID_IUnknown)
	{
		*ppvObject = (IUnknown *)this;
	}
	else if(riid == IID_IClassFactory)
	{
		*ppvObject = (IClassFactory *)this;
	}
	else
	{
		*ppvObject = NULL;
		return E_NOINTERFACE;
	}

	AddRef();

	return S_OK;
}

ULONG ProfilerClassFactory::AddRef()
{
	return InterlockedIncrement((volatile LONG *)&refCount);
}

ULONG ProfilerClassFactory::Release()
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

	//IClassFactory Methods
HRESULT ProfilerClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
	if(pUnkOuter != NULL)
	{
		*ppvObject = NULL;
		return CLASS_E_NOAGGREGATION;
	}

	ClrProfiler* newProfiler = new ClrProfiler();

	if(!SUCCEEDED(newProfiler->QueryInterface(riid, ppvObject)))
	{
		newProfiler->Release();
		newProfiler = NULL;

		*ppvObject = NULL;
		return E_NOINTERFACE;
	}

	//We held a ref to the profiler before we called QueryInterface()...
	newProfiler->Release();
	newProfiler = NULL;

	return S_OK;
}

HRESULT ProfilerClassFactory::LockServer(BOOL fLock)
{
	if(fLock)
		InterlockedIncrement((volatile LONG *)&ComServerLocks);
	else
		InterlockedDecrement((volatile LONG *)&ComServerLocks);

	return S_OK;
}