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
#ifndef SOCKETSERVER_H
#define SOCKETSERVER_H
#pragma once

#include "IProfilerServer.h"
#include "IProfilerData.h"
#include "Messages.h"
#include "RingBuffer.h"

typedef boost::shared_ptr<class TcpConnection> TcpConnectionPtr;

class SocketServer : public IProfilerServer
{
public:
	SocketServer(IProfilerData& profilerData, unsigned short port, boost::recursive_mutex& lock);
	~SocketServer();

	IProfilerData& ProfilerData() { return m_profilerData; }

	void Start();
	void Run();
	void Stop();

	void WaitForConnection();
	bool Connected() { return m_connections.size() > 0; }
	void SetCallbacks(ServerCallback onConnect, ServerCallback onDisconnect);

	void Write(const void* data, size_t sizeBytes);

private:
	friend class TcpConnection;

	IProfilerData& m_profilerData;

	boost::asio::io_service m_io;
	boost::asio::ip::tcp::acceptor m_acceptor;
	HANDLE m_keepAliveTimer;

	std::vector<TcpConnectionPtr> m_connections;
	boost::recursive_mutex& m_lock;
	ServerCallback m_onConnect;
	ServerCallback m_onDisconnect;

	//CONFIG: buffer sizes?
	static const size_t SendBufferSize = 16 * 1024 * 1024;
	static const size_t FlushSize = 16 * 1024;
	RingBuffer m_sendBuffer;
	//writeStart is protected by a lock, so volatile is no longer necessary
	const char* m_writeStart;
	volatile LONG m_writeLength;

	static void CALLBACK OnTimerGlobal(LPVOID lpParameter, BOOLEAN TimerOrWaitFired);

	void KeepAlive();
	void Accept(TcpConnectionPtr conn, const boost::system::error_code& error);
	void HandleWrite(TcpConnectionPtr source, const boost::system::error_code&, size_t);
	void Write(const void* data, size_t sizeBytes, bool forceFlush);
	void Flush(int oldLength, const char* bufferPos);
};

#endif
