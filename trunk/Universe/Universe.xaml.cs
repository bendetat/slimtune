using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;

using UICore;

namespace Universe
{
	/// <summary>
	/// Interaction logic for Universe.xaml
	/// </summary>
	public partial class Universe : UserControl
	{
		Connection m_connection;
		ElementHost m_host;
		D3DImageSlimDX m_container;
		Scene m_scene;

		public Universe(Connection connection, ElementHost host)
		{
			InitializeComponent();

			m_connection = connection;
			m_host = host;

			m_container = new D3DImageSlimDX();
			m_container.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);
			RenderImage.Source = m_container;

			m_scene = new Scene(m_host, m_connection);
			var texture = m_scene.SharedTexture;
			m_container.SetBackBufferSlimDX(texture);
			BeginRenderingScene();
		}

		private void BeginRenderingScene()
		{
			if(m_container.IsFrontBufferAvailable)
			{
				var texture = m_scene.SharedTexture;
				m_container.SetBackBufferSlimDX(texture);
				CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
			}
		}

		void StopRenderingScene()
		{
			CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
		}

		void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(m_container.IsFrontBufferAvailable)
				BeginRenderingScene();
			else
				StopRenderingScene();
		}

		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			m_scene.Render();
			m_container.InvalidateD3DImage();
		}
	}
}
