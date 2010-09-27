#include "stdafx.h"
#include <iostream>
#include <Objbase.h>
#include "Profiler.h"
#include "PerformanceCounters.h"

int wmain(int argc, wchar_t * argv[])
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	//Arguments are passed unaltered to the profiling target.
	/*std::wstringstream argumentStream;
	for(int i = 1; i < argc; ++i)
	{
		if (i != 1)
		{
			argumentStream << L' '; 
		}
		argumentStream << argv[i];
	}
	LPTSTR temp = GetCommandLine();

	//Setup the argument buffer.
	//The buffer must be non-const due to weird issues with CreateProcessW, which can apparently modify the argument buffer.
	std::wstring command = argumentStream.str();
	std::vector<wchar_t> buffer(command.begin(), command.end());*/
	LPTSTR cmdLineRaw = GetCommandLine();
	const wchar_t* cmdLine = wcsstr(cmdLineRaw + 1, L"\"") + 2;
	std::wstring cmdLineStr(cmdLine);

	STARTUPINFO startupInfo;
	ZeroMemory(&startupInfo, sizeof(startupInfo));
	startupInfo.cb = sizeof(startupInfo);

	const wchar_t* curdir = L"D:\\Promit\\Documents\\Projects\\BioReplicant\\trunk\\Demo\\";

	PROCESS_INFORMATION processInformation = {0};
	BOOL processLaunch = CreateProcess(
		NULL, 
		&cmdLineStr[0], 
		NULL, 
		NULL,
		FALSE,
		0, 
		NULL, 
		curdir, 
		&startupInfo,
		&processInformation);

	if(!processLaunch)
	{
		DWORD err = GetLastError();
		std::cout << "Failed to launch target process (code " << err << ").";
		return 1;
	}

	//Register for debug events.
	Profiler profiler(processInformation.hProcess);
	BOOL debugAttach = DebugActiveProcess(GetProcessId(processInformation.hProcess));

	if (!debugAttach)
	{
		//Something bad.
		std::cout << "Attaching as a debugger has failed.";
	}

	//Wait for debug events
	DEBUG_EVENT debugEvent;
	bool running = true;
	while(running)
	{
		WaitForDebugEvent(&debugEvent, INFINITE);
		switch(debugEvent.dwDebugEventCode)
		{
		case EXCEPTION_DEBUG_EVENT:
			{
				break;
			}
		case CREATE_THREAD_DEBUG_EVENT:
			{
				HANDLE hThread = debugEvent.u.CreateThread.hThread;
				profiler.ThreadCreated(GetThreadId(hThread));
				break;
			}
		case CREATE_PROCESS_DEBUG_EVENT:
			{
				//We don't want to mess with the image file.
				CloseHandle(debugEvent.u.CreateProcessInfo.hFile);

				//This is also creation of the main thread.
				HANDLE hMain = debugEvent.u.CreateProcessInfo.hThread;
				profiler.ThreadCreated(GetThreadId(hMain));
				break;
			}
		case EXIT_THREAD_DEBUG_EVENT:
			{
				profiler.ThreadDestroyed(debugEvent.dwThreadId);
				break;
			}
		case EXIT_PROCESS_DEBUG_EVENT:
		case RIP_EVENT:
			{
				running = false;
				break;
			}
		case LOAD_DLL_DEBUG_EVENT:
			{
				break;
			}
		case UNLOAD_DLL_DEBUG_EVENT:
			{
				break;
			}
		case OUTPUT_DEBUG_STRING_EVENT:
			{
				//don't really care
				break;
			}
		default:
			{
				std::cout << "[EVENT] with id: " << debugEvent.dwDebugEventCode << "\n";
				break;
			}
		}

		ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_EXCEPTION_NOT_HANDLED);
	}

	

	CloseHandle(processInformation.hProcess);
	CloseHandle(processInformation.hThread);
}