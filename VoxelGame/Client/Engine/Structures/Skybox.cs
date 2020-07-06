using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client.Renderers;

namespace VoxelGame.Engine
{
	public class Skybox
	{
		public virtual Color getSkyHorizonColor() { return Color.White; }
		public virtual Color getSkyTopColor() { return new Color(Color.White.ToVector3() + new Vector3(0.1f, 0.1f, 0.1f)); }
		public virtual Color getSkyBottomColor() { return new Color(Color.White.ToVector3() - new Vector3(0.1f, 0.1f, 0.1f)); }

		public Skybox()
		{
		}

		public virtual void Initialise()
		{

		}

		public VertexPositionColorTexture[] SkyPlane, VoidPlane, AtmoshpericScatteringPlane;

		public virtual void BeforeUpdate()
		{

		}

		public virtual void Update()
		{

		}

		private Color prevTop, prevBott;

		public virtual void Draw()
		{
			GameClient.Instance.GraphicsDevice.Clear(getSkyHorizonColor());

			//Enable fog for blending
			GraphicsManager.effect.FogEnabled = true;
			GraphicsManager.effect.FogColor = getSkyHorizonColor().ToVector3(); // For best results, make this color whatever your background is.
			GraphicsManager.effect.FogStart = 0f;
			GraphicsManager.effect.FogEnd = 64f;

			GraphicsManager.effect.View = GameClient.World.player.camera.SkyHeightViewMatrix;
			GraphicsManager.effect.World = Matrix.Identity;
			GraphicsManager.effect.Projection = GameClient.World.player.camera.ProjectionMatrix;

			GraphicsManager.effect.VertexColorEnabled = true;

			//Enable alpha blending
			BlendState blend = GameClient.Instance.GraphicsDevice.BlendState;
			GameClient.Instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

			DepthStencilState renderstateOld = GameClient.Instance.GraphicsDevice.DepthStencilState;
			DepthStencilState renderstate = new DepthStencilState();
			renderstate.DepthBufferEnable = false;
			GameClient.Instance.GraphicsDevice.DepthStencilState = renderstate;

			//Null check
			if (SkyPlane == null || VoidPlane == null || AtmoshpericScatteringPlane == null 
				|| prevTop != getSkyTopColor() || prevBott != getSkyBottomColor()) { BuildMesh(); }

			//Sky
			if (SkyPlane.Length > 2)
			{
				//Color
				//basicEffect.AmbientLightColor = skybox.WorldSkyColor.ToVector3();
				GraphicsManager.effect.CurrentTechnique.Passes[0].Apply();
				// Draw the primitives from the vertex buffer to the device as triangles
				try { GameClient.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, SkyPlane.ToArray(), 0, SkyPlane.Length / 3); } catch { }
			}
			//Void
			if (VoidPlane.Length > 2)
			{
				GraphicsManager.effect.CurrentTechnique.Passes[0].Apply();
				//Color
				// Draw the primitives from the vertex buffer to the device as triangles
				try { GameClient.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VoidPlane.ToArray(), 0, VoidPlane.Length / 3); } catch { }
			}

			//TODO: Atmospheric scattering plane
			//Atmospheric Plane
			/*
			if (AtmoshpericScatteringPlane.Length > 2)
			{
				//rotate the plane
				GraphicsManager.effect.View = GameClient.theWorld.player.camera.ViewMatrix;
				GraphicsManager.effect.World = Matrix.CreateFromYawPitchRoll(0f, GameClient.theWorld.CelestialAngle * MathHelper.TwoPi, 0f);
				//add transparency
				float atmosphericAlpha = MathHelper.Clamp((float)Math.Sin(GameClient.theWorld.CelestialAngle), -0.25f, 0.25f) * 4;
				atmosphericAlpha = Math.Abs(atmosphericAlpha);
				GraphicsManager.effect.Alpha = 1 - atmosphericAlpha;
				//Color
				GraphicsManager.effect.CurrentTechnique.Passes[0].Apply();
				// Draw the primitives from the vertex buffer to the device as triangles
				try { GameClient.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, AtmoshpericScatteringPlane.ToArray(), 0, AtmoshpericScatteringPlane.Length / 3); } catch { }
			}
			*/

			//TODO: Stars

			//TODO: Celestial bodies


			GraphicsManager.effect.Alpha = 1;
			GameClient.Instance.GraphicsDevice.BlendState = blend;
			GameClient.Instance.GraphicsDevice.DepthStencilState = renderstateOld;

			prevTop = getSkyTopColor();
			prevBott = getSkyBottomColor();
		}

		public void BuildMesh()
		{
			//Get the Coords
			float XP = 256;
			float XN = -256;
			float ZP = 256;
			float ZN = -256;
			float YP = 6;
			float YN = -6;

			// Top face
			Vector3 topLeftFront = new Vector3(XP, YP, ZN);
			Vector3 topLeftBack = new Vector3(XN, YP, ZN);
			Vector3 topRightFront = new Vector3(XP, YP, ZP);
			Vector3 topRightBack = new Vector3(XN, YP, ZP);

			// Calculate the cubePos of the vertices on the bottom face.
			Vector3 btmLeftFront = new Vector3(XP, YN, ZN);
			Vector3 btmLeftBack = new Vector3(XN, YN, ZN);
			Vector3 btmRightFront = new Vector3(XP, YN, ZP);
			Vector3 btmRightBack = new Vector3(XN, YN, ZP);

			// Atmospheric plane face
			Vector3 atmLeftFront = new Vector3(XP, XP, YP);
			Vector3 atmLeftBack = new Vector3(XP, XN, YP);
			Vector3 atmRightFront = new Vector3(XN, XP, YP);
			Vector3 atmRightBack = new Vector3(XN, XN, YP);

			//Build the Mesh
			List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

			//Void Plane
			vertices.Add(new VertexPositionColorTexture(btmLeftFront, getSkyBottomColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(btmRightBack, getSkyBottomColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(btmLeftBack, getSkyBottomColor(), Vector2.Zero));

			vertices.Add(new VertexPositionColorTexture(btmLeftFront, getSkyBottomColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(btmRightFront, getSkyBottomColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(btmRightBack, getSkyBottomColor(), Vector2.Zero));

			VoidPlane = vertices.ToArray();
			vertices.Clear();

			//Sky Plane
			vertices.Add(new VertexPositionColorTexture(topLeftFront, getSkyTopColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(topLeftBack, getSkyTopColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(topRightBack, getSkyTopColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(topLeftFront, getSkyTopColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(topRightBack, getSkyTopColor(), Vector2.Zero));
			vertices.Add(new VertexPositionColorTexture(topRightFront, getSkyTopColor(), Vector2.Zero));

			SkyPlane = vertices.ToArray();
			vertices.Clear();

			AtmoshpericScatteringPlane = vertices.ToArray();
			vertices.Clear();
		}
	}
}