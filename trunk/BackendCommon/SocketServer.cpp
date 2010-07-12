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
#include "SocketServer.h"

using boost::asio::ip::tcp;
typedef boost::shared_ptr<class TcpConnection> TcpConnectionPtr;

class TcpConnection : public boost::enable_shared_from_this<TcpConnection>
{
private:
	SocketServer& m_server;
	boost::asio::ip::tcp::socket m_socket;
	boost::recursive_mutex& m_lock;

	unsigned int m_parseOffset;
	unsigned int m_recvSize;

	static const int kBufferSize = 896;
	boost::array<char, kBufferSize> m_recvBuffer;

	void HandleWrite(const boost::system::error_code&, size_t);
	bool ContinueRead(const boost::system::error_code& err, size_t);
	void HandleRead( const boost::system::error_code& err, size_t);

	TcpConnection(SocketServer& server, boost::asio::io_service& io, boost::recursive_mutex& lock);

	void SendFunction(const Requests::GetFunctionMapping& request);
	void SendClass(const Requests::GetClassMapping& request);
	void SendThread(const Requests::GetThreadMapping& request);
	void SendCounterName(const Requests::GetCounterName& request);
	void SendEventName(const Requests::GetEventName& request);

public:
	~TcpConnection();

	static TcpConnectionPtr Create(SocketServer& server, boost::asio::io_service& io, boost::recursive_mutex& lock)
	{
		return TcpConnectionPtr(new TcpConnection(server, io, lock));
	}

	boost::asio::ip::tcp::socket& GetSocket()
	{
		return m_socket;
	}

	bool IsOpen() const { return m_socket.is_open(); }
	void Close() { m_socket.close(); }

	void BeginRead(unsigned int offset = 0);
	void Write(const void* data, size_t size);
};

TcpConnection::TcpConnection(SocketServer& server, boost::asio::io_service& io, boost::recursive_mutex& lock)
: m_server(server),
m_socket(io),
m_lock(lock),
m_parseOffset(0)
{
}

TcpConnection::~TcpConnection()
{
}

void TcpConnection::BeginRead(unsigned int offset)
{
	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	m_recvSize = kBufferSize - offset;
	boost::asio::async_read(m_socket, boost::asio::buffer(&m_recvBuffer[0] + offset, m_recvSize),
		//ContinueRead
		boost::bind(&TcpConnection::ContinueRead, shared_from_this(),
		boost::asio::placeholders::error,
		boost::asio::placeholders::bytes_transferred),
		//HandleRead
		boost::bind(&TcpConnection::HandleRead, shared_from_this(),
		boost::asio::placeholders::error,
		boost::asio::placeholders::bytes_transferred)
		);
}

void TcpConnection::Write(const void* data, size_t size)
{
	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	boost::asio::async_write(m_socket, boost::asio::buffer(data, size),
		boost::bind(&SocketServer::HandleWrite, &m_server,
		shared_from_this(),
		boost::asio::placeholders::error,
		boost::asio::placeholders::bytes_transferred));
}

void TcpConnection::SendFunction(const Requests::GetFunctionMapping& request)
{
	const FunctionInfo* func = m_server.ProfilerData().GetFunction(request.FunctionId);
	if(func != NULL)
	{
		Messages::MapFunction mapping;
		mapping.FunctionId = func->Id;
		mapping.IsNative = func->IsNative;
		mapping.ClassId = func->ClassId;
		wcscpy_s(mapping.Name, Messages::MapFunction::MaxNameSize, func->Name.c_str());
		wcscpy_s(mapping.Signature, Messages::MapFunction::MaxSignatureSize, func->Signature.c_str());
		mapping.Write(m_server, func->Name.size(), func->Signature.size());
	}
}

void TcpConnection::SendClass(const Requests::GetClassMapping& request)
{
	const ClassInfo* classInfo = m_server.ProfilerData().GetClass(request.ClassId);
	if(classInfo != NULL)
	{
		Messages::MapClass mapping;
		mapping.ClassId = request.ClassId;
		wcscpy_s(mapping.Name, Messages::MapClass::MaxNameSize, classInfo->Name.c_str());
		mapping.Write(m_server, classInfo->Name.size());
	}
}

void TcpConnection::SendThread(const Requests::GetThreadMapping& request)
{
	const ThreadInfo* info = m_server.ProfilerData().GetThread(request.ThreadId);
	if(info != NULL)
	{
		Messages::MapThread mapping;
		mapping.ThreadId = request.ThreadId;
		wcscpy_s(mapping.Name, Messages::MapThread::MaxNameSize, info->Name.c_str());
		mapping.Write(m_server, info->Name.size());
	}
}

void TcpConnection::SendCounterName(const Requests::GetCounterName& request)
{
	const std::wstring& name = m_server.ProfilerData().GetCounterName(request.CounterId);
	Messages::CounterName counterName;
	counterName.CounterId = request.CounterId;
	std::copy(name.begin(), name.end(), counterName.Name);
	counterName.Write(m_server, name.size());
}

void TcpConnection::SendEventName(const Requests::GetEventName& request)
{
	const std::wstring& name = m_server.ProfilerData().GetEventName(request.EventId);
	Messages::EventName eventName;
	eventName.EventId = request.EventId;
	std::copy(name.begin(), name.end(), eventName.Name);
	eventName.Write(m_server, name.size());
}

bool TcpConnection::ContinueRead(const boost::system::error_code&, size_t bytesRead)
{
	if(bytesRead < 1)
		return false;

	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	size_t prevBytes = kBufferSize - m_recvSize;
	size_t bytesToParse = bytesRead + prevBytes - m_parseOffset;

	char* bufPtr = &m_recvBuffer[m_parseOffset];
	while(bytesToParse > 0)
	{
		size_t bytesParsed = 0;
		switch((unsigned char) *bufPtr)
		{
		case CR_GetFunctionMapping:
			{
				if(bytesToParse < 5)
					goto FinishRead;
				Requests::GetFunctionMapping gfmReq = Requests::GetFunctionMapping::Read(++bufPtr, bytesParsed);
				SendFunction(gfmReq);
				break;
			}

		case CR_GetClassMapping:
			{
				if(bytesToParse < 5)
					goto FinishRead;
				Requests::GetClassMapping gcmReq = Requests::GetClassMapping::Read(++bufPtr, bytesParsed);
				SendClass(gcmReq);
				break;
			}

		case CR_GetThreadMapping:
			{
				if(bytesToParse < 5)
					goto FinishRead;
				Requests::GetThreadMapping gtmReq = Requests::GetThreadMapping::Read(++bufPtr, bytesParsed);
				SendThread(gtmReq);
				break;
			}

		case CR_SetFunctionFlags:
			{
				if(bytesToParse < 6)
					goto FinishRead;
				Requests::SetFunctionFlags sffReq = Requests::SetFunctionFlags::Read(++bufPtr, bytesParsed);
				m_server.ProfilerData().SetInstrument(sffReq.FunctionId, sffReq.Flags > 0);
				break;
			}

		case CR_GetCounterName:
			{
				if(bytesToParse < 5)
					goto FinishRead;
				Requests::GetCounterName gcnReq = Requests::GetCounterName::Read(++bufPtr, bytesParsed);
				SendCounterName(gcnReq);
				break;
			}

		case CR_GetEventName:
			{
				if(bytesToParse < 5)
					goto FinishRead;
				Requests::GetEventName genReq = Requests::GetEventName::Read(++bufPtr, bytesParsed);
				SendEventName(genReq);
				break;
			}

		case CR_Suspend:
			{
				m_server.ProfilerData().SuspendTarget();
				break;
			}

		case CR_Resume:
			{
				m_server.ProfilerData().ResumeTarget();
			}

		default:
#ifdef DEBUG
			__debugbreak();
#endif
			//this is considered catastrophic corruption, so terminate the connection
			m_socket.close();
			return true;
		}

		bufPtr += bytesParsed;
		bytesToParse -= bytesParsed + 1;
	}

FinishRead:
	if(bytesRead >= m_recvSize)
	{
		//ran out of space, so don't keep reading
		//copy the remaining buffer to the beginning and store the offset to start the next recv there
		memmove_s(&m_recvBuffer[0], kBufferSize, bufPtr, bytesToParse);
		m_parseOffset = static_cast<unsigned int>(bytesToParse);
		return true;
	}

	m_parseOffset = static_cast<unsigned int>(bufPtr - &m_recvBuffer[0]);
	return false;
}

void TcpConnection::HandleRead(const boost::system::error_code& err, size_t bytesRead)
{
	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	//this means the read ended, so we'll launch a new one
	BeginRead(m_parseOffset);
	m_parseOffset = 0;
}

void TcpConnection::HandleWrite(const boost::system::error_code& err, size_t)
{

}

SocketServer::SocketServer(IProfilerData& profilerData, unsigned short port, boost::recursive_mutex& lock)
: m_profilerData(profilerData),
m_io(),
m_acceptor(m_io, tcp::endpoint(tcp::v4(), port)),
m_lock(lock),
m_onConnect(NULL),
m_onDisconnect(NULL),
m_sendBuffer(SendBufferSize),
m_writeStart(m_sendBuffer.GetBufferRoot()),
m_writeLength(0)
{
}

SocketServer::~SocketServer()
{
}

void SocketServer::OnTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired)
{
	SocketServer* server = static_cast<SocketServer*>(lpParameter);
	server->KeepAlive();
}

void SocketServer::Start()
{
	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	TcpConnectionPtr conn = TcpConnection::Create(*this, m_acceptor.io_service(), m_lock);
	m_acceptor.async_accept(conn->GetSocket(),
		boost::bind(&SocketServer::Accept, this, conn, boost::asio::placeholders::error));

	CreateTimerQueueTimer(&m_keepAliveTimer, NULL, OnTimerGlobal, this, 10000, 10000, WT_EXECUTEDEFAULT);
}

void SocketServer::Run()
{
	m_io.run();
}

void SocketServer::Stop()
{
	DeleteTimerQueueTimer(NULL, m_keepAliveTimer, INVALID_HANDLE_VALUE);

	boost::recursive_mutex::scoped_lock EnterLock(m_lock);

	int oldLength = InterlockedExchange(&m_writeLength, 0);
	char* bufferPos = m_sendBuffer.Alloc(0);
	Flush(oldLength, bufferPos);

	m_io.stop();

	for(size_t i = 0; i < m_connections.size(); ++i)
	{
		if(m_connections[i]->IsOpen())
			m_connections[i]->Close();
	}
}

void SocketServer::Accept(TcpConnectionPtr conn, const boost::system::error_code& error)
{
	if(!error)
	{
		boost::recursive_mutex::scoped_lock EnterLock(m_lock);

		//activate the new connection
		m_connections.push_back(conn);
		conn->Write(m_profilerData.GetSessionId(), sizeof(GUID));
		conn->BeginRead();

		if(m_connections.size() == 1 && m_onConnect)
			m_onConnect();

		Start();
	}
}

void SocketServer::HandleWrite(TcpConnectionPtr source, const boost::system::error_code& error, size_t size)
{
	if(error)
	{
		std::string errorMessage = error.message();

		//we can remove this connection from our list
		boost::recursive_mutex::scoped_lock EnterLock(m_lock);

		std::vector<TcpConnectionPtr>::iterator deadConn = std::find(m_connections.begin(), m_connections.end(), source);
		//this callback will trigger multiple times when multiple writes are queued up,
		//so we can't be sure the connection exists to erase
		if(deadConn != m_connections.end())
		{
			m_connections.erase(deadConn);

			if(m_connections.size() == 0 && m_onDisconnect)
			{
				m_onDisconnect();
			}
		}
	}
}

void SocketServer::WaitForConnection()
{
	if(m_connections.size() == 0)
		m_io.run_one();
}

void SocketServer::SetCallbacks(ServerCallback onConnect, ServerCallback onDisconnect)
{
	m_onConnect = onConnect;
	m_onDisconnect = onDisconnect;
}

void SocketServer::Write(const void* data, size_t sizeBytes)
{
	Write(data, sizeBytes, false);
}

void SocketServer::Write(const void* data, size_t sizeBytes, bool forceFlush)
{
	assert(sizeBytes < FlushSize); //this function will break otherwise

#ifndef LOCKLESS
	boost::recursive_mutex::scoped_lock EnterLock(m_lock);
#endif

	char* bufferPos = m_sendBuffer.Alloc((LONG) sizeBytes);
	memcpy(bufferPos, data, sizeBytes);

	//Decide whether or not to flush -- forceFlush may cause this to break in lockless mode
	bool flush = forceFlush;

#ifdef LOCKLESS
	LONG oldLength = InterlockedExchangeAdd(&m_writeLength, (LONG) sizeBytes);
	LeaveCriticalSection(&m_writeLock);

	//We will flush either if we pushed over the limit, or the ring buffer rolls around
	if(oldLength + sizeBytes >= FlushSize && oldLength < FlushSize)
	{
		//we're the ones who pushed the write length over the limit
		flush = true;
	}
	else if(bufferPos == m_sendBuffer.GetBufferRoot())
	{
		//this means the ring buffer has rolled over and it's our fault
		flush = true;
	}
#else
	//Used when locks are being used. Locks are being used because my lockless code is broken.
	LONG oldLength = m_writeLength;
	m_writeLength += static_cast<LONG>(sizeBytes);

	if(m_writeLength > FlushSize || bufferPos == m_sendBuffer.GetBufferRoot())
		flush = true;
#endif

	if(flush)
	{
		Flush(oldLength, bufferPos);
	}
}

void SocketServer::Flush(int oldLength, const char* bufferPos)
{
	if(oldLength == 0)
		return;

	//Lock is now taken by Write
	//EnterLock localLock(&m_writeLock);

#ifdef LOCKLESS
	//we do NOT flush the current write
	InterlockedExchangeAdd(&m_writeLength, -oldLength);
#else
	m_writeLength -= oldLength;
#endif

	for(size_t i = 0; i < m_connections.size(); ++i)
	{
		m_connections[i]->Write(m_writeStart, oldLength);
	}
	//Since the introduction of the lock here, we don't need to do this interlocked
	m_writeStart = bufferPos;
}

void SocketServer::KeepAlive()
{
	unsigned char data = MID_KeepAlive;
	Write(&data, 1, true);
}