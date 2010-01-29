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
#ifndef IPROFILERDATA_H
#define IPROFILERDATA_H
#pragma once

#include "Messages.h"

struct ModuleInfo
{
	const unsigned int Id;
	std::wstring Name;

	ModuleID NativeId;

	ModuleInfo(unsigned int id)
		: Id(id)
	{
	}
};

struct ClassInfo
{
	const unsigned int Id;
	std::wstring Name;

	ClassID NativeId;

	ClassInfo(unsigned int id, ClassID nativeId)
		: Id(id),
		NativeId(nativeId)
	{
	}
};

struct ThreadContext
{
	unsigned int Id;
	LONG InstCount;
	LONG DisableCount;
	std::vector<unsigned int> ShadowStack;

	ThreadContext()
		: Id(0), InstCount(0), DisableCount(0)
	{
		ShadowStack.reserve(32);
	}
};

struct FunctionInfo
{
	const unsigned int Id;
	unsigned int ClassId;
	std::wstring Name;
	std::wstring Signature;
	int IsNative;

	//runtime props that aren't broadcast
	const FunctionID NativeId;
	volatile bool TriggerInstrumentation;
	volatile bool DisableInstrumentation;

	FunctionInfo(unsigned int id, FunctionID nativeId)
		: Id(id),
		NativeId(nativeId),
		TriggerInstrumentation(false),
		DisableInstrumentation(false)
	{
	}
};

struct ThreadInfo
{
	unsigned int Id;
	std::wstring Name;
	bool Destroyed;

	ThreadID NativeId;
	DWORD SystemId;
	ThreadContext* Context;
	unsigned int EntryPoint;

	ThreadInfo()
		: Id(0), NativeId(0), SystemId(0), Destroyed(false), Context(NULL), EntryPoint(0)
	{
		Name[0] = 0;
	}

	ThreadInfo(unsigned int id, ThreadID nativeId, ThreadContext* context)
		: Id(id), NativeId(nativeId), SystemId(0), Destroyed(false), Context(context)
	{
		Name[0] = 0;
	}
};

struct IProfilerData
{
	virtual const FunctionInfo* GetFunction(unsigned int id) = 0;
	virtual const ClassInfo* GetClass(unsigned int id) = 0;
	virtual const ThreadInfo* GetThread(unsigned int id) = 0;

	virtual void SetInstrument(unsigned int id, bool enable) = 0;

	virtual bool SuspendTarget() = 0;
	virtual bool ResumeTarget() = 0;
};

#endif
