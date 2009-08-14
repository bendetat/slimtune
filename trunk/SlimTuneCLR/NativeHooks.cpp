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
#include "NativeHooks.h"
#include "Profiler.h"

//These three functions are called by the appropriate native assembly hook for the platform
void __stdcall FunctionEnterGlobal(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO *argInfo)
{
    if (g_ProfilerCallback != NULL && g_ProfilerCallback->IsActive() && (g_ProfilerCallback->GetMode() & PM_Tracing))
        g_ProfilerCallback->Enter(functionID, clientData, frameInfo, argInfo);
}

void __stdcall FunctionLeaveGlobal(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
    if (g_ProfilerCallback != NULL && g_ProfilerCallback->IsActive() && (g_ProfilerCallback->GetMode() & PM_Tracing))
        g_ProfilerCallback->Leave(functionID,clientData,frameInfo,retvalRange);
}

void __stdcall FunctionTailcallGlobal(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo)
{
    if (g_ProfilerCallback != NULL && g_ProfilerCallback->IsActive() && (g_ProfilerCallback->GetMode() & PM_Tracing))
        g_ProfilerCallback->Tailcall(functionID,clientData,frameInfo);
}

//x86 implementations can be done from inline assembly
//x64 has to be handled by MASM or shortcutted via C (see http://blogs.msdn.com/jkeljo/archive/2005/08/11/450506.aspx)
#ifdef X86
void _declspec(naked) FunctionEnterNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
{
    __asm
    {
		//Set up a stack frame and preserve registers
        push    ebp                 
        mov     ebp,esp
        pushad

        mov     eax,[ebp+0x14]      //argumentInfo
        push    eax
        mov     ecx,[ebp+0x10]      //frameInfo
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //functionID
        push    eax
        call    FunctionEnterGlobal

		//Restore registers and pop the stack frame
        popad
        pop     ebp
        ret     16
    }
}

void _declspec(naked) FunctionLeaveNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
    __asm
    {
		//Set up a stack frame and preserve registers
        push    ebp
        mov     ebp,esp
        pushad

        mov     eax,[ebp+0x14]      //argumentInfo
        push    eax
        mov     ecx,[ebp+0x10]      //frameInfo
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //functionID
        push    eax
        call    FunctionLeaveGlobal

		//Restore registers and pop the stack frame
        popad
        pop     ebp
        ret     16
    }
}

void _declspec(naked) FunctionTailcallNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo)
{
    __asm
    {
		//Set up a stack frame and preserve registers
        push    ebp
        mov     ebp,esp
        pushad

        mov     ecx,[ebp+0x10]      //frameInfo
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //functionID
        push    eax
        call    FunctionTailcallGlobal

		//Restore registers and pop the stack frame
        popad
        pop     ebp
        ret     12
    }
}
#endif

#ifdef X64
void FunctionEnterNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
{
	FunctionEnterGlobal(functionID, clientData, frameInfo, argumentInfo);
}

void FunctionLeaveNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
	FunctionLeaveGlobal(functionID, clientData, frameInfo, retvalRange);
}

void FunctionTailcallNaked(FunctionID functionID, UINT_PTR clientData, COR_PRF_FRAME_INFO frameInfo)
{
	FunctionTailcallGlobal(functionID, clientData, frameInfo);
}

#endif