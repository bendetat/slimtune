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
#ifndef CONFIG_H
#define CONFIG_H
#pragma once

enum ProfilerMode
{
	PM_Disabled = 0,

	PM_Sampling = 0x01,
	PM_Tracing = 0x02,

	PM_Hybrid = PM_Sampling | PM_Tracing,
};

enum TimingMode
{
	TM_WallTime,
	TM_Cycles,
};

struct ProfilerConfig
{
	OSVERSIONINFO Version;

	//General properties
	ProfilerMode Mode;
	unsigned int ListenPort;
	bool WaitForConnection;
	bool SuspendOnConnection;

	bool TrackMemory;
	bool AllowInlining;

	//Instrumentation properties
	bool InstrumentSmallFunctions;
	bool CycleTiming;

	//Sampling properties
	unsigned int SampleInterval;
	bool SampleUnmanaged;
	bool SampleSuspended;

	//Perf counter properties
	unsigned int CounterInterval;
	std::vector<std::pair<std::wstring, std::wstring> > Counters;

	ProfilerConfig();
	bool LoadEnv();
};

#endif
