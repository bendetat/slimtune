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

unsigned int ClrProfiler::GetParentFunction()
{
	unsigned int parentId = 0;

	PooledUIntVector functions;
	functions.reserve(1);
	WalkData data = { this, &functions, 0, 0 };
	HRESULT snapshotResult = m_ProfilerInfo2->DoStackSnapshot(NULL, StackWalkGlobal_OneShot, COR_PRF_SNAPSHOT_DEFAULT,
		&data, NULL, 0);
	if(SUCCEEDED(snapshotResult) || snapshotResult == CORPROF_E_STACKSNAPSHOT_ABORTED)
	{
		if(functions.size() > 0)
			parentId = functions[0];
	}

	return parentId;
}

HRESULT ClrProfiler::GenerationBounds()
{
	assert(m_server->Connected());

	ULONG rangeCount = 0;
	HRESULT hr = m_ProfilerInfo2->GetGenerationBounds(static_cast<ULONG>(m_genRanges.size()), &rangeCount, &m_genRanges[0]);
	CHECK_HR(hr);
	assert(rangeCount <= 5);

	Messages::GenerationSizes gsMsg;
	gsMsg.GenerationCount = rangeCount;
	QueryTimer(gsMsg.TimeStamp);
	for(size_t i = 0; i < rangeCount; ++i)
	{
		unsigned int gen = m_genRanges[i].generation;
		gsMsg.Sizes[gen] = m_genRanges[i].rangeLength;
	}

	gsMsg.Write(*m_server);
	return S_OK;
}

HRESULT ClrProfiler::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason)
{
	if(!IsConnected())
		return S_OK;

	Mutex::scoped_lock EnterLock(m_lock);

	GenerationBounds();

	int maxGen = 0;
	for(int g = 0; g < cGenerations; ++g)
	{
		if(generationCollected[g])
			maxGen = g;
	}

	Messages::GarbageCollection gcMsg;
	gcMsg.Generation = maxGen;
	gcMsg.FunctionId = GetParentFunction();
	QueryTimer(gcMsg.TimeStamp);

	gcMsg.Write(*m_server);

	return S_OK;
}

HRESULT ClrProfiler::GarbageCollectionFinished()
{
	if(!IsConnected())
		return S_OK;

	Mutex::scoped_lock EnterLock(m_lock);

	GenerationBounds();
	return S_OK;
}

HRESULT ClrProfiler::ObjectAllocated(ObjectID objectId, ClassID classId)
{
	if(!IsConnected())
		return S_OK;

	Mutex::scoped_lock EnterLock(m_lock);

	//look up basic class info
	ModuleID moduleId;
	mdTypeDef token;
	HRESULT hr = m_ProfilerInfo->GetClassIDInfo(classId, &moduleId, &token);
	CHECK_HR(hr);

	int mappedModuleId = m_moduleRemapper[moduleId];
	ModuleInfo* moduleInfo = m_modules[mappedModuleId];

	//get the internal id we're using for this class
	unsigned int nativeId = ClassIdFromTypeDefAndModule(token, mappedModuleId);

	//if an alloc is pending, this is a re-run on mostly acquired data
	//if not, we go through and fill out the structure
	if(!m_allocatedPending)
	{
		ULONG size = 0;
		m_ProfilerInfo->GetObjectSize(objectId, &size);
		CHECK_HR(hr);

		m_allocMsg.Size = size;
		m_allocMsg.FunctionId = GetParentFunction();
		QueryTimer(m_allocMsg.TimeStamp);

		if(nativeId == 0)
		{
			//this class hasn't been loaded yet, mark it as pending
			//we will come back when ClassLoadFinished hits
			m_allocatedPending = true;
			m_allocatedObject = objectId;
			m_allocatedClass = classId;
			return S_OK;
		}
	}

	//either way at this point, we should be ready to map the class and fire the message
	if(nativeId == 0)
	{
		//just bail out and hope this works out later
		return S_OK;
	}

	m_allocMsg.ClassId = MapClass(nativeId, 0);
	m_allocMsg.Write(*m_server);
	m_allocatedPending = false;

	return S_OK;
}

HRESULT ClrProfiler::ObjectsAllocatedByClass(ULONG classCount, ClassID* classIds, ULONG* cObjects)
{
	return S_OK;
}

HRESULT ClrProfiler::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID* objectRefIds)
{
	return S_OK;
}
