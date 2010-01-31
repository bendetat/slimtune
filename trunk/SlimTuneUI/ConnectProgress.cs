using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using UICore;

namespace SlimTuneUI
{
	public partial class ConnectProgress : Form
	{
		public ProfilerClient Client { get; set; }

		volatile bool m_canceled = false;
		public bool Canceled
		{
			get { return m_canceled; }
			set { m_canceled = value; }
		}

		string m_host;
		int m_port;
		IStorageEngine m_storage;
		IAsyncResult m_async;
		int m_attempts;

		delegate void ConnectDelegate();

		public ConnectProgress(string host, int port, IStorageEngine storage, int attempts)
		{
			InitializeComponent();

			m_host = host;
			m_port = port;
			m_storage = storage;
			m_attempts = attempts;

			m_connectingToLabel.Text = string.Format("Connecting to: {0}:{1}...", m_host, m_port);
			m_progress.Maximum = m_attempts;
		}

		private void UpdateAttempts(int attempt)
		{
			m_attemptsLabel.Text = string.Format("Attempt {0} out of {1}", attempt, m_attempts);
			m_progress.Value = attempt;
		}

		private void UpdateStatus(string status)
		{
			m_statusLabel.Text = "Status: " + status;
		}

		private void Finish(object dummy)
		{
			if(Client == null && !Canceled)
			{
				m_cancelButton.Text = "OK";
				m_progress.Value = 0;
			}

			if(Client != null || Canceled)
				this.Close();
		}

		private void Connect()
		{
			for(int i = 0; i < m_attempts; ++i)
			{
				if(Canceled)
					break;

				this.Invoke(new Action<int>(UpdateAttempts), i + 1);
				try
				{
					Client = new ProfilerClient(m_host, m_port, m_storage);
				}
				catch(System.Net.Sockets.SocketException ex)
				{
					this.Invoke(new Action<string>(UpdateStatus), ex.Message);
					Thread.Sleep(1000);
					continue;
				}

				break;
			}
		}

		private void ConnectFinished(IAsyncResult result)
		{
			this.Invoke(new Action<object>(Finish), new object());
		}

		private void m_cancelButton_Click(object sender, EventArgs e)
		{
			if(m_cancelButton.Text != "OK")
			{
				Canceled = true;
				m_cancelButton.Enabled = false;
			}
			else
			{
				this.Close();
			}
		}

		private void ConnectProgress_Shown(object sender, EventArgs e)
		{
			ConnectDelegate connector = new ConnectDelegate(Connect);
			m_async = connector.BeginInvoke(ConnectFinished, null);
		}
	}
}
