using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client.Renderers;
using VoxelGame.Engine;
using VoxelGame.Util;
using VoxelGame.Util.Math;
using VoxelGame.World;

namespace VoxelGame.Client
{
	public class WorldRenderer
	{
		/// <summary>
		/// The render distance squared, measured in chunks
		/// </summary>
		public static int RenderDistanceSquared
		{
			get
			{
				return GameSettings.RenderDistance * GameSettings.RenderDistance;
			}
		}

		public virtual void RenderWorld(World.World toRender)
		{
			toRender.Draw(Time.GameTime);

			#region Effect Setup
			//Prepare the effect
			//Fix the fog

			//TODO: Replace with custom shader for opaque, second shader for transparents

			//Camera
			GraphicsManager.effect.View = GameClient.World.player.camera.ViewMatrix;
			if (GameClient.World.player.Ortho)
			{
				GraphicsManager.effect.Projection = GameClient.World.player.camera.OrtoProjectionMatrix;
			}
			else
			{
				GraphicsManager.effect.Projection = GameClient.World.player.camera.ProjectionMatrix;
			}
			//Texturing
			GraphicsManager.effect.VertexColorEnabled = true;
			GraphicsManager.effect.TextureEnabled = true;
			GraphicsManager.effect.Texture = TextureManager.terrain;
			//Double sided rendering
			RasterizerState stat = new RasterizerState();
			//stat.CullMode = CullMode.None;
			VoxelClient.Graphics.GraphicsDevice.RasterizerState = stat;

			//Point scaling
			VoxelClient.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			GraphicsManager.effect.LightingEnabled = false;
			#endregion

			//Logger.info(-1, "Camera.View.pos = " + GameClient.theWorld.player.camera.ViewMatrix.Translation
			//	+ "\nTrue Pos = " + GameClient.theWorld.player.camera.position
			//+ "\nOffset = " + GameClient.theWorld.player.camera.offset);

			//Now get the chunks
			for (int chunkID = 0; chunkID < toRender.chunks.Count; chunkID++)
			{
				Chunk chunk = toRender.chunks[toRender.chunkHashes[chunkID]];
				if (IsChunkInCameraFrustum(chunk))
				{
					//Outline
					if (GameSettings.RenderChunkBorders)
					{
						RenderUtils.RenderBox(chunk.GetBoundingBoxBlock().Offset(new Vector3(-0.5f, -0.5f, -0.5f)), Color.Yellow, 0.25f);
					}

					if (chunk.ChunkMesh == null)
					{
						chunk.InitialiseMesh();
					}

					//Check if their block mesh was generated
					if (!chunk.ChunkMesh.isGenerated)
					{
						//Create the block meshes

						//TODO: Thread the mesh builder into a seperate mesh thread. this thread shall sort the chunk to be built by distance from
						//the player and the based on whether they are in view (eg, id rather build a mesh which is in the player's view, then
						//around him on the sides, and finally behind him.
						//Util.Managers.ThreadManager.RegisterShortLivedThread("chunk_mesher_" + chunk.GetHashCode(), chunk.ChunkMesh.ConstructMesh, chunk);
						chunk.ChunkMesh.ConstructMesh(chunk);
					}

					if (IsChunkInCameraFrustum(chunk))
					{
						//Render each block mesh
						//Logger.info(-1, "world.render");
						if (chunk.ChunkMesh.vertices.Count > 2 && GameSettings.Debug.RenderChunks)
						{
							//Render each block mesh
							//Logger.info(-1, "world.render");
							chunk.ChunkMesh.RenderMeshes();
						}
					}
				}
			}

			//Temporary. For now only outlines chunks from the infinite terrain chunk manager
			/*lock (toRender.theChunkManager.LoadedChunkPositions)
			{
				for (int i = 0; i < toRender.theChunkManager.chunkCache.Count; i++)
				{
					Chunk chunk = toRender.theChunkManager.chunkCache[i];

					//Handle the mesh
					//RenderUtils.RenderBox(chunk.GetBoundingBoxBlock().Offset(new Vector3(-0.5f, -0.5f, -0.5f)), Color.Yellow, 0.25f);
					if (chunk.ChunkMesh == null)
					{
						chunk.InitialiseMesh();
					}
					//Check if their block mesh was generated
					if (!chunk.ChunkMesh.isGenerated && Util.Managers.ThreadManager.ThreadStackNotFull)
					{
						//Create the block meshes

						//TODO: Thread the mesh builder into a seperate mesh thread. this thread shall sort the chunk to be built by distance from
						//the player and the based on whether they are in view (eg, id rather build a mesh which is in the player's view, then
						//around him on the sides, and finally behind him.
						Util.Managers.ThreadManager.RegisterShortLivedThread("chunk_mesher_" + chunk.GetHashCode(), chunk.ChunkMesh.ConstructMesh, chunk);
					}

					if (chunk.ChunkMesh.isGenerated && chunk.isLoaded && chunk.IsVisible && GameClient.theWorld.player.camera.ViewFrustum.Intersects(chunk.GetBoundingBoxBlock()))
					{
						//If the chunk is beyond render distance
						if (chunk.posX > GameClient.theWorld.player.ChunkPosition.X + GameSettings.RenderDistance
							|| chunk.posY > GameClient.theWorld.player.ChunkPosition.Y + GameSettings.RenderDistance
							|| chunk.posZ > GameClient.theWorld.player.ChunkPosition.Z + GameSettings.RenderDistance

							|| chunk.posX < GameClient.theWorld.player.ChunkPosition.X - GameSettings.RenderDistance
							|| chunk.posY < GameClient.theWorld.player.ChunkPosition.Y - GameSettings.RenderDistance
							|| chunk.posZ < GameClient.theWorld.player.ChunkPosition.Z - GameSettings.RenderDistance)
						{
							//RenderUtils.RenderBox(chunk.GetBoundingBoxBlock().Offset(new Vector3(-0.5f, -0.5f, -0.5f)), Color.Red, 0.25f);
						}
						else
						{
							if (chunk.ChunkMesh.vertices.Count > 2 && GameSettings.Debug.RenderChunks)
							{
								//Render each block mesh
								//Logger.info(-1, "world.render");
								chunk.ChunkMesh.RenderMeshes();
							}
						}
					}
				}
			}*/

			//Single pass for particles
			toRender.particleEngine.Draw();
		}

		public void AddChunk(Chunk chunkToRegister)
		{

		}

		private bool IsChunkInCameraFrustum(Chunk chunk)
		{
			return (chunk.isLoaded && chunk.IsVisible &&
				GameClient.theWorld.player.camera.ViewFrustum.Contains(chunk.GetBoundingBoxBlock()) != ContainmentType.Disjoint);
			//GameClient.theWorld.player.camera.ViewFrustum.Intersects(chunk.GetBoundingBoxBlock()));
		}
	}
}
