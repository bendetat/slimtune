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

volatile ULONG ComServerLocks = 0;

BOOL SlimTuneProfilerModule::DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	hModuleInstance = hInstance;	
	lpReserved;
	dwReason;

	return TRUE;
}

HRESULT SlimTuneProfilerModule::DllCanUnloadNow()
{
	if(ComServerLocks == 0)
		return S_OK;
	else
		return S_FALSE;
}

HRESULT SlimTuneProfilerModule::DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
	if(rclsid != CLSID_Profiler)
		return CLASS_E_CLASSNOTAVAILABLE;

	if((riid != IID_IUnknown) && (riid != IID_IClassFactory))
		return CLASS_E_CLASSNOTAVAILABLE;
	
	ProfilerClassFactory* newClassFactory = new ProfilerClassFactory();

	if(!SUCCEEDED(newClassFactory->QueryInterface(riid, ppv)))
	{
		newClassFactory->Release();
		newClassFactory = NULL;

		delete newClassFactory;
		*ppv = NULL;

		return CLASS_E_CLASSNOTAVAILABLE;
	}

	//We had a ref count of 1 before QueryInterface();
	newClassFactory->Release();
	newClassFactory = NULL;

	return S_OK;
}

HRESULT SlimTuneProfilerModule::DllRegisterServer()
{
	WCHAR szModuleName[MAX_PATH];

	DWORD rv = GetModuleFileName(hModuleInstance, szModuleName, MAX_PATH);
	if((rv == 0) || (rv == MAX_PATH))
		return SELFREG_E_CLASS;
	
	return RegisterComServer(CLSID_Profiler, szModuleName, TEXT("Both"));
}

HRESULT SlimTuneProfilerModule::DllUnregisterServer()
{

	return UnRegisterComServer(CLSID_Profiler);
}

HRESULT SlimTuneProfilerModule::RegisterComServer(const CLSID& serverCLSID, const LPWSTR szModuleName, const LPWSTR szThreadingModel)
{
	//Find the string for the CLSID
	//Use old skool C error handling...
	//Actually seems a bit cleaner in this case.

	WCHAR tmpStr[512];
	HKEY hKey = NULL;
	LPOLESTR clsidStr = NULL;
		
	HRESULT hr = StringFromCLSID(serverCLSID, &clsidStr);
	if(FAILED(hr))
		goto ErrorExit; 

	//Add the minimal registry keys in order to allow the CLR to start the profiler.
	//There really isnt much use adding the other COM stuff...
	//For example, when is anyone going to want to use the ProgID? Or a type lib?
	//Possibly for diagnostic tools, but we only have one component anyway.

	wsprintf(tmpStr, TEXT("CLSID\\%s"), clsidStr);
	if(RegCreateKeyEx(HKEY_CLASSES_ROOT,tmpStr, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL) != ERROR_SUCCESS)
		goto ErrorExit;
	RegCloseKey(hKey);
	hKey = NULL;

	wsprintf(tmpStr, TEXT("CLSID\\%s\\InProcServer32"), clsidStr);
	if(RegCreateKeyEx(HKEY_CLASSES_ROOT,tmpStr, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL) != ERROR_SUCCESS)
		goto ErrorExit;

	//Set the path
	if(RegSetValueEx(hKey, NULL, 0, REG_SZ, (const BYTE *)szModuleName, (wcslen(szModuleName) + 1) * sizeof(WCHAR)) != ERROR_SUCCESS)
		goto ErrorExit;

	//Set the threading model, although it is probably not useful...
	if(RegSetValueEx(hKey, TEXT("ThreadingModel"), 0, REG_SZ, (const BYTE *)szThreadingModel, (wcslen(szThreadingModel) + 1) * sizeof(WCHAR)) != ERROR_SUCCESS)
		goto ErrorExit;

	RegCloseKey(hKey);
	hKey = NULL;

	CoTaskMemFree(clsidStr);
	clsidStr = NULL;

	return S_OK;

ErrorExit:
	
	if(hKey != NULL)
	{
		RegCloseKey(hKey);
		hKey = NULL;
	}

	if(clsidStr != NULL)
	{
		CoTaskMemFree(clsidStr);
		clsidStr = NULL;
	}

	return SELFREG_E_CLASS;

}

HRESULT SlimTuneProfilerModule::UnRegisterComServer(const CLSID& serverCLSID)
{
	WCHAR tmpStr[512];
	LPOLESTR clsidStr = NULL;
		
	HRESULT hr = StringFromCLSID(serverCLSID, &clsidStr);
	if(FAILED(hr))
		goto ErrorExit; 

	//Need to delete sub keys as RegDeleteKey() is not recursive and RegDeleteTree() is only on Vista.
	wsprintf(tmpStr, TEXT("CLSID\\%s\\InProcServer32"), clsidStr);
	if(RegDeleteKey(HKEY_CLASSES_ROOT, tmpStr) != ERROR_SUCCESS)
		goto ErrorExit;

	wsprintf(tmpStr, TEXT("CLSID\\%s"), clsidStr);
	if(RegDeleteKey(HKEY_CLASSES_ROOT, tmpStr) != ERROR_SUCCESS)
		goto ErrorExit;

	CoTaskMemFree(clsidStr);
	clsidStr = NULL;

	return S_OK;

ErrorExit:
	
	if(clsidStr != NULL)
	{
		CoTaskMemFree(clsidStr);
		clsidStr = NULL;
	}

	return SELFREG_E_CLASS;
}