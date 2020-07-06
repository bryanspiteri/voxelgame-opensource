using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client.Renderers
{
	public static class RenderUtils
	{
		static BasicEffect effect;
		static VertexBuffer vertex_Buffer;

		static bool rendered = false, debugRendered = false;

		static List<VertexPositionColor> lineQueue = new List<VertexPositionColor>();
		static List<VertexPositionColor> debugLineQueue = new List<VertexPositionColor>();
		private static List<VertexPositionColor> vertTmp = new List<VertexPositionColor>();


		static int[] boxIndices = new int[]
		{
			0, 1,
			1, 2,
			2, 3,
			3, 0,
			0, 4,
			1, 5,
			2, 6,
			3, 7,
			4, 5,
			5, 6,
			6, 7,
			7, 4,
		};

		#region Drawing Methods
		/// <summary>
		/// Draws a line from start to position with the specified color and thickness
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="col"></param>
		/// <param name="thickness"></param>
		public static void RenderLine(Vector3 start, Vector3 end, Color col, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (rendered)
			{
				lineQueue.Clear();
				rendered = false;
			}

			Vector3 thicknessOffset = new Vector3(thickness / 2f, thickness / 2f, thickness / 2f);

			Vector3 min = start + thicknessOffset;
			Vector3 max = end - thicknessOffset;

			#region Line formation
			//North
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[1]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//South
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//Top
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[1]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//Bottom
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//West
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//East
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(vertTmp[1]);
			lineQueue.AddRange(vertTmp);
			vertTmp.Clear();
			#endregion
		}

		/// <summary>
		/// Renders the given ray with distance 5
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="col"></param>
		/// <param name="thickness"></param>
		public static void RenderRay(Ray ray, Color col, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (rendered)
			{
				lineQueue.Clear();
				rendered = false;
			}

			RenderLine(ray.Position, ray.Position + ray.Direction * 5f, col, thickness);
		}

		/// <summary>
		/// Renders the bounding box for debugging purposes.
		/// </summary>
		/// <param name="box">The box to render.</param>
		/// <param name="graphicsDevice">The graphics device to use when rendering.</param>
		/// <param name="view">The current view matrix.</param>
		/// <param name="projection">The current projection matrix.</param>
		/// <param name="color">The color to use for drawing the lines of the box.</param>
		public static void RenderBox(BoundingBox box, Color color, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (rendered)
			{
				lineQueue.Clear();
				rendered = false;
			}

			Vector3[] corners = box.GetCorners();
			
			VertexPositionColor vert = new VertexPositionColor();
			vert.Color = color;
			for (int i = 0; i < boxIndices.Length / 2; i++)
			{
				RenderLine(corners[boxIndices[i * 2]], corners[boxIndices[i * 2 + 1]], color, thickness);
			}
		}
		#endregion

		#region DebugRenders

		/// <summary>
		/// Draws a line from start to position with the specified color and thickness
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="col"></param>
		/// <param name="thickness"></param>
		public static void DebugRenderLine(Vector3 start, Vector3 end, Color col, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (debugRendered)
			{
				debugLineQueue.Clear();
				debugRendered = false;
			}

			Vector3 thicknessOffset = new Vector3(thickness / 2f, thickness / 2f, thickness / 2f);

			Vector3 min = start + thicknessOffset;
			Vector3 max = end - thicknessOffset;

			#region Line formation
			//North
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[1]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//South
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//Top
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[1]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//Bottom
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//West
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), col));

			vertTmp.Add(vertTmp[1]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), col));
			vertTmp.Add(vertTmp[2]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();

			//East
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), col));
			vertTmp.Add(new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), col));

			vertTmp.Add(vertTmp[0]);
			vertTmp.Add(new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), col));
			vertTmp.Add(vertTmp[1]);
			debugLineQueue.AddRange(vertTmp);
			vertTmp.Clear();
			#endregion
		}

		/// <summary>
		/// Renders the given ray with distance 5
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="col"></param>
		/// <param name="thickness"></param>
		public static void DebugRenderRay(Ray ray, Color col, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (debugRendered)
			{
				debugLineQueue.Clear();
				debugRendered = false;
			}

			DebugRenderLine(ray.Position, ray.Position + ray.Direction * 5f, col, thickness);
		}

		/// <summary>
		/// Renders the bounding box for debugging purposes.
		/// </summary>
		/// <param name="box">The box to render.</param>
		/// <param name="graphicsDevice">The graphics device to use when rendering.</param>
		/// <param name="view">The current view matrix.</param>
		/// <param name="projection">The current projection matrix.</param>
		/// <param name="color">The color to use for drawing the lines of the box.</param>
		public static void DebugRenderBox(BoundingBox box, Color color, float thickness)
		{
			//Stutter fix at higher fps than 60
			if (debugRendered)
			{
				debugLineQueue.Clear();
				debugRendered = false;
			}

			Vector3[] corners = box.GetCorners();

			VertexPositionColor vert = new VertexPositionColor();
			vert.Color = color;
			for (int i = 0; i < boxIndices.Length / 2; i++)
			{
				DebugRenderLine(corners[boxIndices[i * 2]], corners[boxIndices[i * 2 + 1]], color, thickness);
			}
		}
		#endregion

		public static void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
		{
			RasterizerState prevCullRast = graphicsDevice.RasterizerState;
			RasterizerState cullState = new RasterizerState()
			{
				CullMode = CullMode.None,
				DepthBias = prevCullRast.DepthBias,
				FillMode = prevCullRast.FillMode,
				MultiSampleAntiAlias = prevCullRast.MultiSampleAntiAlias,
				Name = prevCullRast.Name,
				ScissorTestEnable = prevCullRast.ScissorTestEnable,
				SlopeScaleDepthBias = prevCullRast.SlopeScaleDepthBias,
				Tag = prevCullRast.Tag,
			};
			graphicsDevice.RasterizerState = cullState;

			//Draws
			if (lineQueue != null && lineQueue.Count > 2)
			{
				graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.WithDepthBias(-0.0001f);
				if (effect == null)
				{
					effect = new BasicEffect(graphicsDevice)
					{
						VertexColorEnabled = true,
						LightingEnabled = false
					};
				}

				effect.View = view;
				effect.Projection = projection;

				effect.CurrentTechnique.Passes[0].Apply();

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineQueue.ToArray(), 0, lineQueue.Count / 3);
				}
				
				graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.WithDepthBias(0);

				rendered = true;
				lineQueue.Clear();
			}

			//Debug draws
			if (debugLineQueue != null && debugLineQueue.Count > 2)
			{
				//graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.WithDepthBias(-0.0001f);
				if (effect == null)
				{
					effect = new BasicEffect(graphicsDevice)
					{
						VertexColorEnabled = true,
						LightingEnabled = false
					};
				}

				effect.View = view;
				effect.Projection = projection;

				effect.CurrentTechnique.Passes[0].Apply();

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, debugLineQueue.ToArray(), 0, debugLineQueue.Count / 3);
				}

				//graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.WithDepthBias(0);

				debugRendered = true;
				debugLineQueue.Clear();
			}
			graphicsDevice.RasterizerState = prevCullRast;
		}
	}
}
