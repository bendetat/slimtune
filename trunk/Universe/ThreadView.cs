using System;
using System.Collections.Generic;
using System.Drawing;

using SlimDX;
using Dxgi = SlimDX.DXGI;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;
using SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;

using UICore;

namespace Universe
{
	class ThreadView : IView
	{
		Connection m_connection;
		IList<ClassInfo> m_threads;

		Device m_device;
		DeviceContext m_context;
		InputLayout m_layout;
		SlimDX.Direct3D11.Buffer m_vertices;
		Effect m_effect;
		Texture2D m_texture;
		ShaderResourceView m_textureView;

		//List<DynamicTexture> m_dynamicTextures;

		const int MaxQuads = 512;

		public ThreadView(Connection conn, Scene scene)
		{
			m_connection = conn;
			m_device = scene.Device;
			m_context = m_device.ImmediateContext;

			var bytecode = ShaderBytecode.CompileFromFile(RenderSupport.ContentPath + "\\Universe.fx", "fx_5_0", ShaderFlags.None, EffectFlags.None);
			m_effect = new Effect(m_device, bytecode);
			var pass = m_effect.GetTechniqueByIndex(0).GetPassByIndex(0);
			m_layout = new InputLayout(m_device, pass.Description.Signature, QuadVertex.Elements);

			m_vertices = new SlimDX.Direct3D11.Buffer(m_device, new BufferDescription()
			{
				CpuAccessFlags = CpuAccessFlags.Write,
				BindFlags = BindFlags.VertexBuffer,
				SizeInBytes = 6 * MaxQuads * QuadVertex.SizeBytes,
				Usage = ResourceUsage.Dynamic
			});

			m_texture = Texture2D.FromFile(m_device, RenderSupport.ContentPath + "\\stars.png");
			m_textureView = new ShaderResourceView(m_device, m_texture);
		}

		public void UpdateData()
		{
			using(var session = m_connection.DataEngine.OpenSession())
			{
				m_threads = session.CreateCriteria<ClassInfo>().List<ClassInfo>();
			}

			int columns = (int) Math.Ceiling(Math.Sqrt(m_threads.Count));
			int rows = (int) Math.Ceiling((double) m_threads.Count / columns);
			const float ViewRangeX = 4.0f;
			const float ViewRangeY = 2.0f;
			float columnWidth = 2 * ViewRangeX / columns;
			float rowHeight = 2 * ViewRangeY / rows;
			float quadWidth = columnWidth * 0.8f;
			float quadHeight = rowHeight * 0.8f;
			float xmargin = (columnWidth - quadWidth) / 2.0f;
			float ymargin = (rowHeight - quadHeight) / 2.0f;

			var data = m_context.MapSubresource(m_vertices, 0, 6 * MaxQuads * QuadVertex.SizeBytes, MapMode.WriteDiscard, MapFlags.None).Data;
			float xcoord = -ViewRangeX + xmargin;
			float ycoord = ViewRangeY - ymargin;
			int threadIndex = 0;
			for(int r = 0; r < rows; ++r)
			{
				for(int c = 0; c < columns && threadIndex < m_threads.Count; ++c)
				{
					Color4 color = new Color4(m_threads[threadIndex].Name.GetHashCode());
					data.Write(new Vector4(xcoord, ycoord, 5.0f, 1.0f));
					data.Write(color);
					data.Write(new Vector2(0.0f, 0.0f));

					data.Write(new Vector4(xcoord + quadWidth, ycoord, 5.0f, 1.0f));
					data.Write(color);
					data.Write(new Vector2(1.0f, 0.0f));

					data.Write(new Vector4(xcoord, ycoord - quadHeight, 5.0f, 1.0f));
					data.Write(color);
					data.Write(new Vector2(0.0f, 1.0f));

					data.Write(new Vector4(xcoord + quadWidth, ycoord - quadHeight, 5.0f, 1.0f));
					data.Write(color);
					data.Write(new Vector2(1.0f, 1.0f));

					xcoord += columnWidth;
					++threadIndex;
				}
				xcoord = -ViewRangeX + xmargin;
				ycoord -= rowHeight;
			}
			m_context.UnmapSubresource(m_vertices, 0);

			/*m_dynamicTextures.Capacity = m_threads.Count;
			for(int i = 0; i < m_threads.Count; ++i)
			{
				if(m_dynamicTextures.Count < i)
				{
					m_dynamicTextures.Add(DynamicTexture.Create(m_device, new Texture2DDescription()
					{
						ArraySize = 1,
						BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
						Format = Dxgi.Format.R8G8B8A8_UNorm,
						Height = 128,
						Width = 128,
						MipLevels = 1,
						SampleDescription = new Dxgi.SampleDescription(1, 0),
					}));
				}

				Color4 color = new Color4(m_threads[i].Name.GetHashCode());
				m_context.ClearRenderTargetView(m_dynamicTextures[i].RTView, color);
			}*/
		}

		public void Draw(Scene parentScene)
		{
			m_context.InputAssembler.InputLayout = m_layout;
			m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			m_context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertices, QuadVertex.SizeBytes, 0));
			m_context.InputAssembler.SetIndexBuffer(parentScene.QuadIndices, SlimDX.DXGI.Format.R16_UInt, 0);

			var mvp = m_effect.GetVariableByName("ModelViewProj");
			//Matrix modelViewProj = Matrix.Scaling(0.5f, 1.0f, 1.0f);
			Matrix modelViewProj = Matrix.PerspectiveFovLH((float) Math.PI / 3.0f, (float) parentScene.Width / parentScene.Height, 0.1f, 10.0f);
			mvp.AsMatrix().SetMatrix(modelViewProj);
			m_effect.GetVariableByName("Texture").AsResource().SetResource(m_textureView);

			var technique = m_effect.GetTechniqueByIndex(0);
			var pass = technique.GetPassByIndex(0);

			for(int i = 0; i < technique.Description.PassCount; ++i)
			{
				pass.Apply(m_context);
				m_context.DrawIndexed(6 * m_threads.Count, 0, 0);
			}
		}

		public void DrawIconic(Scene parentScene)
		{
			throw new NotImplementedException();
		}
	}
}
