using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX;
using Dxgi = SlimDX.DXGI;
using SlimDX.Direct3D11;

namespace Universe
{
	struct QuadVertex
	{
		public Vector4 Position;
		public Vector4 Color;
		public Vector2 TexCoord;

		public QuadVertex(Vector4 position, Vector4 color, Vector2 texCoord)
		{
			Position = position;
			Color = color;
			TexCoord = texCoord;
		}

		public static InputElement[] Elements
		{
			get
			{
				return new[]
				{
					new InputElement("POSITION", 0, Dxgi.Format.R32G32B32_Float, 0, 0),
					new InputElement("COLOR", 0, Dxgi.Format.R32G32B32_Float, 16, 0),
					new InputElement("TEXCOORD", 0, Dxgi.Format.R32G32_Float, 32, 0)
				};
			}
		}

		public static int SizeBytes
		{
			get { return 40; }
		}
	}

	struct DynamicTexture
	{
		public Texture2D Texture;
		public RenderTargetView RTView;
		public ShaderResourceView SRView;

		public static DynamicTexture Create(Device device, Texture2DDescription desc)
		{
			DynamicTexture dt = new DynamicTexture();
			dt.Texture = new Texture2D(device, desc);
			dt.RTView = new RenderTargetView(device, dt.Texture);
			dt.SRView = new ShaderResourceView(device, dt.Texture);
			return dt;
		}
	}

	static class RenderSupport
	{
		public const int MaxQuads = 512;

		public static SlimDX.Direct3D11.Buffer InitQuadIndices(Device device)
		{
			ushort[] indices = new ushort[6 * MaxQuads];
			for(int i = 0; i < MaxQuads; ++i)
			{
				int baseVal = i * 4;
				int idx = i * 6;
				indices[idx + 0] = (ushort) (baseVal + 0);
				indices[idx + 1] = (ushort) (baseVal + 1);
				indices[idx + 2] = (ushort) (baseVal + 2);

				indices[idx + 3] = (ushort) (baseVal + 2);
				indices[idx + 4] = (ushort) (baseVal + 1);
				indices[idx + 5] = (ushort) (baseVal + 3);
			}

			using(var ds = new DataStream(indices, true, true))
			{
				ds.Position = 0;
				var buffer = new SlimDX.Direct3D11.Buffer(device, ds, new BufferDescription()
				{
					BindFlags = BindFlags.IndexBuffer,
					SizeInBytes = 12 * MaxQuads,
				});
				return buffer;
			}
		}

		public static string ContentPath
		{
			get
			{
				var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
				return Path.GetDirectoryName(assemblyLocation);
			}
		}
	}
}
