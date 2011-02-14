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

#pragma once

#ifndef STRICT
#define STRICT
#endif

// Modify the following defines if you have to target a platform prior to the ones specified below.
// Refer to MSDN for the latest info on corresponding values for different platforms.
#ifndef WINVER				// Allow use of features specific to Windows XP or later.
#define WINVER 0x0600		// Change this to the appropriate value to target other versions of Windows.
#endif

#define NOMINMAX
#ifndef _WIN32_WINNT		// Allow use of features specific to Windows Vista or later.                   
#define _WIN32_WINNT 0x0600	// Change this to the appropriate value to target other versions of Windows.
#endif						

#ifndef _WIN32_WINDOWS		// Allow use of features specific to Windows 98 or later.
#define _WIN32_WINDOWS 0x0410 // Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE			// Allow use of features specific to IE 6.0 or later.
#define _WIN32_IE 0x0600	// Change this to the appropriate value to target other versions of IE.
#endif

#define DBGHELP_TRANSLATE_TCHAR

//Probably always want this unless we are using something esoteric. In any case parts of boost will define
//it anyway(although they are included after windows.h ...)
#define WIN32_LEAN_AND_MEAN

//Iterator debugging is great! Except it takes global locks that are a deadlock risk when mixed with SuspendThread.
//#define _HAS_ITERATOR_DEBUGGING 0

#pragma warning(disable:4100)

#include "Memory.h"
#include "resource.h"
#include <windows.h>
#include <mmsystem.h>
#include <dbghelp.h>

//Make sure we include winsock2 for asio.
#include <winsock2.h>
#include <ws2tcpip.h>
#include <mswsock.h>

//For COM error codes.
#include <olectl.h>

#include <cstdlib>
#include <cstddef>
#include <cassert>

#pragma warning(push)
#pragma warning(disable:4100)
#pragma warning(disable:4244)
#pragma warning(disable:4251)
#pragma warning(disable:4267)
#include <boost/scoped_ptr.hpp>
#include <boost/scoped_array.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/function.hpp>
#include <boost/bind.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/thread.hpp>
#include <boost/lexical_cast.hpp>
#include <boost/pool/pool_alloc.hpp>
#include <boost/preprocessor/stringize.hpp>
#include <boost/format.hpp>
#include <boost/asio.hpp>
#pragma warning(pop)

#include <stdexcept>
#include <string>
#include <vector>
#include <unordered_map>

#include "SlimComPtr.h"
#include "ProfilerBase.h"

typedef boost::pool_allocator<unsigned int> UIntPoolAlloc;
typedef std::vector<unsigned int, UIntPoolAlloc> PooledUIntVector;
typedef boost::recursive_mutex Mutex;