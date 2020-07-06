using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;
using VoxelGame.Util;

namespace VoxelGame.Client.Renderers
{
	public class ItemRenderer
	{
		public static readonly Color topShadowBase = new Color(0xE5E5E5); //229
		public static readonly Color bottomShadowBase = new Color(0x999999); //153
		public static readonly Color nsShadowBase = new Color(0xAFAFAF); //175
		public static readonly Color ewShadowBase = new Color(0xC4C4C4); //196

		static BasicEffect effect = null;

		public static Texture2D Render(BlockModel itemModel, Matrix View, Matrix World)
		{
			if (effect == null)
			{
				effect = new BasicEffect(GameClient.Instance.GraphicsDevice);
			}
			//Create the projectionMatrix
			Matrix projection = Matrix.CreateOrthographic(64, 64, 0.1f, 1024f);
			
			//Initialise the render target
			RenderTarget2D targetTexture = new RenderTarget2D(GameClient.Instance.GraphicsDevice, 64, 64, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
			RenderTarget2D targetTextureNoTex = new RenderTarget2D(GameClient.Instance.GraphicsDevice, 64, 64, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

			GameClient.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			GameClient.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			GameClient.Instance.GraphicsDevice.SetRenderTarget(targetTexture);
			GameClient.Instance.GraphicsDevice.Clear(Color.Transparent);
			GameClient.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			GameClient.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			if (itemModel.voxels.Length > 0)
			{

				#region Mesh Construction
				List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
				List<VertexPositionColorTexture> vertTmp = new List<VertexPositionColorTexture>();

				for (int i = 0; i < itemModel.voxels.Length; i++)
				{
					Voxel voxel = itemModel.voxels[i];

					Vector3Int position = voxel.position;
					Vector3Int size = voxel.size;

					//Construct Quad Vertices

					//North
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.north.X, voxel.textures.north.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.north.Z, voxel.textures.north.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.north.Z, voxel.textures.north.W)));

					vertTmp.Add(vertTmp[0]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.north.X, voxel.textures.north.Y)));
					vertTmp.Add(vertTmp[1]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();

					//South
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.south.X, voxel.textures.south.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.south.X, voxel.textures.south.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.south.Z, voxel.textures.south.W)));

					vertTmp.Add(vertTmp[1]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), nsShadowBase, new Vector2(voxel.textures.south.Z, voxel.textures.south.Y)));
					vertTmp.Add(vertTmp[2]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();

					//Top
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), topShadowBase, new Vector2(voxel.textures.up.Z, voxel.textures.up.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), topShadowBase, new Vector2(voxel.textures.up.X, voxel.textures.up.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), topShadowBase, new Vector2(voxel.textures.up.X, voxel.textures.up.Y)));

					vertTmp.Add(vertTmp[0]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), topShadowBase, new Vector2(voxel.textures.up.Z, voxel.textures.up.W)));
					vertTmp.Add(vertTmp[1]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();

					//Bottom
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), bottomShadowBase, new Vector2(voxel.textures.down.X, voxel.textures.down.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), bottomShadowBase, new Vector2(voxel.textures.down.X, voxel.textures.down.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), bottomShadowBase, new Vector2(voxel.textures.down.Z, voxel.textures.down.Y)));

					vertTmp.Add(vertTmp[1]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), bottomShadowBase, new Vector2(voxel.textures.down.Z, voxel.textures.down.W)));
					vertTmp.Add(vertTmp[2]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();

					//West
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.west.X, voxel.textures.west.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.west.X, voxel.textures.west.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z + size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.west.Z, voxel.textures.west.W)));

					vertTmp.Add(vertTmp[1]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z + size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.west.Z, voxel.textures.west.Y)));
					vertTmp.Add(vertTmp[2]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();

					//East
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.east.X, voxel.textures.east.W)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.east.Z, voxel.textures.east.Y)));
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X / 2f, position.Y - size.Y / 2f, position.Z - size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.east.Z, voxel.textures.east.W)));

					vertTmp.Add(vertTmp[0]);
					vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X / 2f, position.Y + size.Y / 2f, position.Z - size.Z / 2f), ewShadowBase, new Vector2(voxel.textures.east.X, voxel.textures.east.Y)));
					vertTmp.Add(vertTmp[1]);
					vertices.AddRange(vertTmp);
					vertTmp.Clear();
				}
				#endregion

				//Prepare the effect
				//rotation
				effect.World = World;
				//Camera
				effect.View = View;
				effect.Projection = projection;
				//Texturing
				effect.VertexColorEnabled = true;
				effect.TextureEnabled = true;
				effect.Texture = TextureManager.terrain;

				//Double sided rendering
				RasterizerState stat = new RasterizerState();
				//stat.CullMode = CullMode.None;
				GameClient.Instance.GraphicsDevice.RasterizerState = stat;

				//Point scaling
				GameClient.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

				//Draw the tris
				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					GameClient.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
				}

				//Draw the opaque texture (black clear color)
				GameClient.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				GameClient.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

				GameClient.Instance.GraphicsDevice.SetRenderTarget(targetTextureNoTex);
				GameClient.Instance.GraphicsDevice.Clear(Color.Black);
				GameClient.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				GameClient.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

				//Double sided rendering
				GameClient.Instance.GraphicsDevice.RasterizerState = stat;

				//Point scaling
				GameClient.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

				//Change the effect
				effect.VertexColorEnabled = false;
				effect.TextureEnabled = true;
				effect.Texture = TextureManager.whiteTerrain;

				//Draw the tris
				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					GameClient.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
				}
			}

			GameClient.Instance.GraphicsDevice.SetRenderTarget(null);

			//Make texture opaque
			Texture2D opaqueTexture = new Texture2D(GameClient.Instance.GraphicsDevice, targetTexture.Width, targetTexture.Height, false, SurfaceFormat.Color);

			Color[] pixelData = new Color[targetTexture.Width * targetTexture.Height];
			targetTexture.GetData(pixelData);

			Color[] opaquePixelData = new Color[targetTextureNoTex.Width * targetTextureNoTex.Height];
			targetTextureNoTex.GetData(opaquePixelData);

			//Make the texture opaque
			for (int i = 0; i < pixelData.Length; i++)
			{
				Color col = pixelData[i];

				//If the RGB says its WHITE
				if (opaquePixelData[i].R == 255)
				{
					col.A = 255;
				}
				else
				{
					col.A = 0;
				}

				pixelData[i] = col;
			}

			opaqueTexture.SetData(pixelData);

			return opaqueTexture;
		}
	}
}
