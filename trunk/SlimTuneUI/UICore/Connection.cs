/*
* Copyright (c) 2007-2010 SlimDX Group
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
using System;
using System.Threading;
using System.ComponentModel;

namespace UICore
{
	/// <summary>
	/// A Connection combines two objects: a ProfilerClient and an IStorageEngine.
	/// The storage engine is valid for the lifetime of the Connection.
	/// The ProfilerClient may not be, or may not exist to begin with.
	/// </summary>
	/// <remarks>
	/// Connection is responsible for the receive thread driving a ProfilerClient.
	/// </remarks>
	public class Connection : IDisposable, INotifyPropertyChanged
	{
		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler Closing;

		Thread m_recvThread;
		volatile bool m_receive = false;
		System.Timers.Timer m_snapshotTimer;
		bool m_clearAfterSnapshot = false;

		public string Name
		{
			get { return Utilities.GetStandardCaption(this); }
		}

		public string Executable { get; set; }
		public string HostName
		{
			get { return Client != null ? Client.HostName : string.Empty; }
		}

		public int Port
		{
			get { return Client != null ? Client.Port : 0; }
		}

		public IStorageEngine StorageEngine { get; private set; }
		public ProfilerClient Client { get; private set; }
		public bool IsConnected { get; private set; }

		public Connection(IStorageEngine storageEngine)
		{
			if(storageEngine == null)
				throw new ArgumentNullException("storageEngine");
			this.StorageEngine = storageEngine;
		}

		/// <summary>
		/// Creates a thread to run the specified client, receiving data.
		/// </summary>
		/// <remarks>This method takes ownership of the client.</remarks>
		/// <param name="client">The client to send and receive data with.</param>
		public void RunClient(ProfilerClient client)
		{
			if(client == null)
				throw new ArgumentNullException("client");
			if(this.Client != null)
				throw new InvalidOperationException();
			this.Client = client;

			m_recvThread = new Thread(new ParameterizedThreadStart(ReceiveThread));
			m_receive = true;
			m_recvThread.Start(Client);
		}

		public void SetAutoSnapshots(double interval, bool clearAfterSnapshot)
		{
			if(!m_receive)
				return;

			m_clearAfterSnapshot = clearAfterSnapshot;
			m_snapshotTimer = new System.Timers.Timer(interval);
			m_snapshotTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_snapshotTimer_Elapsed);
			//m_snapshotTimer.Start();
		}

		/// <summary>
		/// Terminates receiving from the target and closes the profiler client.
		/// </summary>
		public void DisconnectClient()
		{
			m_receive = false;
		}

		void m_snapshotTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(!IsConnected)
				return;

			StorageEngine.Snapshot("Auto");
			if(m_clearAfterSnapshot)
				StorageEngine.ClearData();
		}

		void ReceiveThread(object data)
		{
			var client = (ProfilerClient) data;
			Thread.CurrentThread.Name = string.Format("{0}:{1}", client.HostName, client.Port);

			IsConnected = true;
			if(Connected != null)
				Connected(this, EventArgs.Empty);

			try
			{
				while(m_receive)
				{
					try
					{
						string text = client.Receive();
						if(text == null)
							break;
					}
					catch(System.Net.Sockets.SocketException)
					{
						m_receive = false;
					}
					catch(InvalidOperationException)
					{
						m_receive = false;
					}
#if DEBUG
					catch(Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(ex.Message);
						System.Diagnostics.Debugger.Break();
					}
#endif
				}
			}
			finally
			{
				IsConnected = false;
				Client.Dispose();
				Client = null;
				Executable = string.Empty;

				if(PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("IsConnected"));
					PropertyChanged(this, new PropertyChangedEventArgs("Client"));
					PropertyChanged(this, new PropertyChangedEventArgs("Name"));
				}

				if(Disconnected != null)
				{
					Disconnected(this, EventArgs.Empty);
				}

				m_recvThread = null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#region IDisposable Members

		public void Dispose()
		{
			if(Closing != null)
			{
				Closing(this, EventArgs.Empty);
			}

			DisconnectClient();
			StorageEngine.Dispose();
		}

		#endregion
	}
}
