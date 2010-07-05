using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.Direct3D11;
using Dxgi = SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Universe
{
	class Scene : IDisposable
	{
		System.Windows.Forms.Control Container;
		Dxgi.SwapChain SwapChain;
		DeviceContext m_context;
		Texture2D DepthTexture;

		IView m_currentView;

		public int Width { get; set; }
		public int Height { get; set; }
		public Device Device { get; private set; }
		public RenderTargetView RenderView { get; private set; }
		public DepthStencilView DepthView { get; private set; }
		public Buffer QuadIndices { get; private set; }

		public Texture2D SharedTexture
		{
			get;
			set;
		}

		public Scene(System.Windows.Forms.Control container, UICore.Connection connection)
		{
			Container = container;
			Width = Container.ClientSize.Width;
			Height = Container.ClientSize.Height;
			InitD3D();

			m_currentView = new ThreadView(connection, this);
			m_currentView.UpdateData();
		}

		public void Dispose()
		{
			DestroyD3D();
		}

		public void Render()
		{
			m_context.OutputMerger.SetTargets(DepthView, RenderView);
			m_context.Rasterizer.SetViewports(new Viewport(0, 0, Width, Height, 0.0f, 1.0f));

			m_context.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
			float c = 0.25f;
			m_context.ClearRenderTargetView(RenderView, new SlimDX.Color4(1.0f, c, c, c));

			if(m_currentView != null)
				m_currentView.Draw(this);

			m_context.Flush();
		}

		void InitD3D()
		{
			var swapDesc = new Dxgi.SwapChainDescription
			{
				BufferCount = 1,
				ModeDescription = new Dxgi.ModeDescription
					{
						Width = Container.ClientSize.Width,
						Height = Container.ClientSize.Height,
						RefreshRate = new Rational(60, 1),
						Format = Dxgi.Format.R8G8B8A8_UNorm
					},
				IsWindowed = true,
				OutputHandle = Container.Handle,
				SampleDescription = new Dxgi.SampleDescription(1, 0),
				SwapEffect = Dxgi.SwapEffect.Discard,
				Usage = Dxgi.Usage.RenderTargetOutput
			};

			Device device;
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport,
				swapDesc, out device, out SwapChain);
			Device = device;
			m_context = Device.ImmediateContext;

			Texture2DDescription colordesc = new Texture2DDescription
			{
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				Format = Dxgi.Format.B8G8R8A8_UNorm,
				Width = Width,
				Height = Height,
				MipLevels = 1,
				SampleDescription = new Dxgi.SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				OptionFlags = ResourceOptionFlags.Shared,
				CpuAccessFlags = CpuAccessFlags.None,
				ArraySize = 1
			};

			Texture2DDescription depthdesc = new Texture2DDescription
			{
				BindFlags = BindFlags.DepthStencil,
				Format = Dxgi.Format.D24_UNorm_S8_UInt,
				Width = Width,
				Height = Height,
				MipLevels = 1,
				SampleDescription = new Dxgi.SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				OptionFlags = ResourceOptionFlags.None,
				CpuAccessFlags = CpuAccessFlags.None,
				ArraySize = 1
			};

			SharedTexture = new Texture2D(Device, colordesc);
			DepthTexture = new Texture2D(Device, depthdesc);
			RenderView = new RenderTargetView(Device, SharedTexture);
			DepthView = new DepthStencilView(Device, DepthTexture);

			var rastDesc = new RasterizerStateDescription()
			{
				CullMode = CullMode.None,
				FillMode = FillMode.Solid,
			};
			m_context.Rasterizer.State = RasterizerState.FromDescription(Device, rastDesc);

			QuadIndices = RenderSupport.InitQuadIndices(Device);

			m_context.Flush();
		}

		void DestroyD3D()
		{
			if(SharedTexture != null)
			{
				SharedTexture.Dispose();
				SharedTexture = null;
			}

			if(DepthTexture != null)
			{
				DepthTexture.Dispose();
				DepthTexture = null;
			}

			if(Device != null)
			{
				Device.Dispose();
				Device = null;
			}
		}
	}
}
