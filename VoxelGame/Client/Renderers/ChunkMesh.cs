using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using VoxelGame.Util;
using VoxelGame.World;
using VoxelGame.Blocks;
using VoxelGame.Engine;
using VoxelGame.Util.Math;

namespace VoxelGame.Client.Renderers
{
	public class ChunkMesh
	{
		//TODO: Opaque Buffer
		//TODO: Transparent Buffer
		//TODO: Fluid Buffer

		#region Opqaue Buffers
		//buffers and their respective caches
		public VertexBuffer vertexBuffer;
		public IndexBuffer indexBuffer;

		public uint vertexBufferCount = 0;
		public uint indexBufferCount = 0;

		public List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
		public VertexPositionColorTexture[] verts = { };
		public List<int> indices = new List<int>();
		#endregion

		public int posX, posY, posZ = 0;
		public bool isGenerated = false;

		public ChunkMesh(int X, int Y, int Z)
		{
			posX = X;
			posY = Y;
			posZ = Z;
		}

		public void ConstructMesh(object param) //For metadata
		{
			try
			{
				//check if the param is a chunk
				if (param == null || !(param is Chunk))
				{
					return;
				}

				//convert the param to usable data
				Chunk baseChunk = (Chunk)param;

				Profiler.Start("Mesh Building", Color.Blue);

				//Force other worker threads to wait
				lock (vertices)
				{
					if (vertices.Count == 0)
					{
						//isGenerated = false; //just incase
					}

					Color topShadowBase = new Color(0xE5E5E5); //229
					Color bottomShadowBase = new Color(0x999999); //153
					Color nsShadowBase = new Color(0xAFAFAF); //175
					Color ewShadowBase = new Color(0xC4C4C4); //196

					//tmp
					//Vector3Int position = new Vector3Int(posX, posY, posZ);
					//Vector3Int size = new Vector3Int(16);

					//Construct Quad Vertices
					List<VertexPositionColorTexture> finalVerts = new List<VertexPositionColorTexture>();
					List<VertexPositionColorTexture> vertTmp = new List<VertexPositionColorTexture>();

					int i = 0;

					for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
					{
						for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
						{
							for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
							{
								Block currentBlock = BlockManager.GetBlock(baseChunk.chunk_blockdata[x, y, z]);
								if (currentBlock.IsTransparent == false)
								{
									//GameClient.LOGGER.info("X: " + x + ", Y: " + y + ", Z: " + z);
									i++;
									#region Mesh Building
									//TODO: Build according to model
									/*
									Color topShadowBase = new Color(0xE5E5E5); //229
									Color bottomShadowBase = new Color(0x999999); //153
									Color nsShadowBase = new Color(0xAFAFAF); //175
									Color ewShadowBase = new Color(0xC4C4C4); //196*/

									Vector3Int position = new Vector3Int(x + 1, y + 1, z + 1);
									Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
									//GameClient.LOGGER.info("pos: " + position + " size: " + size);

									//List<VertexPositionColorTexture> vertTmp = new List<VertexPositionColorTexture>();

									if (CheckBlockVisible((sbyte)(x + 1), (sbyte)y, (sbyte)z))
									{
										//North
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z + size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.north.X, currentBlock.BlockModel.voxels[0].textures.north.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z - size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.north.Z, currentBlock.BlockModel.voxels[0].textures.north.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z - size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.north.Z, currentBlock.BlockModel.voxels[0].textures.north.W)));

										vertTmp.Add(vertTmp[0]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z + size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.north.X, currentBlock.BlockModel.voxels[0].textures.north.Y)));
										vertTmp.Add(vertTmp[1]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									if (CheckBlockVisible((sbyte)(x - 1), (sbyte)y, (sbyte)z))
									{
										//South
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z - size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.south.X, currentBlock.BlockModel.voxels[0].textures.south.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z - size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.south.X, currentBlock.BlockModel.voxels[0].textures.south.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z + size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.south.Z, currentBlock.BlockModel.voxels[0].textures.south.W)));

										vertTmp.Add(vertTmp[1]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z + size.Z), nsShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.south.Z, currentBlock.BlockModel.voxels[0].textures.south.Y)));
										vertTmp.Add(vertTmp[2]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									if (CheckBlockVisible((sbyte)x, (sbyte)(y + 1), (sbyte)z))
									{
										//Top
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z - size.Z), topShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.up.Z, currentBlock.BlockModel.voxels[0].textures.up.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z + size.Z), topShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.up.X, currentBlock.BlockModel.voxels[0].textures.up.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z - size.Z), topShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.up.X, currentBlock.BlockModel.voxels[0].textures.up.Y)));

										vertTmp.Add(vertTmp[0]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z + size.Z), topShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.up.Z, currentBlock.BlockModel.voxels[0].textures.up.W)));
										vertTmp.Add(vertTmp[1]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									if (CheckBlockVisible((sbyte)x, (sbyte)(y - 1), (sbyte)z))
									{
										//Bottom
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z - size.Z), bottomShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.down.X, currentBlock.BlockModel.voxels[0].textures.down.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z + size.Z), bottomShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.down.X, currentBlock.BlockModel.voxels[0].textures.down.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z - size.Z), bottomShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.down.Z, currentBlock.BlockModel.voxels[0].textures.down.Y)));

										vertTmp.Add(vertTmp[1]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z + size.Z), bottomShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.down.Z, currentBlock.BlockModel.voxels[0].textures.down.W)));
										vertTmp.Add(vertTmp[2]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									if (CheckBlockVisible((sbyte)x, (sbyte)y, (sbyte)(z + 1)))
									{
										//West
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z + size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.west.X, currentBlock.BlockModel.voxels[0].textures.west.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z + size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.west.X, currentBlock.BlockModel.voxels[0].textures.west.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z + size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.west.Z, currentBlock.BlockModel.voxels[0].textures.west.W)));

										vertTmp.Add(vertTmp[1]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z + size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.west.Z, currentBlock.BlockModel.voxels[0].textures.west.Y)));
										vertTmp.Add(vertTmp[2]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									if (CheckBlockVisible((sbyte)x, (sbyte)y, (sbyte)(z - 1)))
									{
										//East
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y - size.Y, position.Z - size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.east.X, currentBlock.BlockModel.voxels[0].textures.east.W)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y + size.Y, position.Z - size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.east.Z, currentBlock.BlockModel.voxels[0].textures.east.Y)));
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X - size.X, position.Y - size.Y, position.Z - size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.east.Z, currentBlock.BlockModel.voxels[0].textures.east.W)));

										vertTmp.Add(vertTmp[0]);
										vertTmp.Add(new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, position.Z - size.Z), ewShadowBase, new Vector2(currentBlock.BlockModel.voxels[0].textures.east.X, currentBlock.BlockModel.voxels[0].textures.east.Y)));
										vertTmp.Add(vertTmp[1]);
										finalVerts.AddRange(vertTmp);
										vertTmp.Clear();
									}

									#endregion
								}
								else
								{
									//TODO: Handle transparent meshes
								}
							}
						}
					}

					//GameClient.LOGGER.info(i + " blocks found!");

					//vertices.Clear();
					vertices = finalVerts;

					if (vertices.Count != 0 && VoxelClient.Graphics.GraphicsDevice != null)
					{
						vertexBuffer = new VertexBuffer(VoxelClient.Graphics.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
						vertexBuffer.SetData(vertices.ToArray());

						indexBuffer = new IndexBuffer(VoxelClient.Graphics.GraphicsDevice, typeof(int), indices.Count, BufferUsage.WriteOnly);
						indexBuffer.SetData(indices.ToArray());
					}
					lock (verts)
					{
						verts = vertices.ToArray();
					}
					isGenerated = true;
				}

				Profiler.Stop("Mesh Building");

				//Kill this thread
				Util.Managers.ThreadManager.RemoveThreadClean(Thread.CurrentThread.Name);
			}
			catch (ThreadAbortException ex)
			{
				Logger.fatal(-1, "Thread \"" + Thread.CurrentThread.Name + "\" aborted! This is usually normal!");
			}
		}

		private bool CheckBlockVisible(sbyte X, sbyte Y, sbyte Z)
		{
			return GameClient.theWorld.GetBlockAt(X + posX * Chunk.CHUNK_SIZE, Y + posY * Chunk.CHUNK_SIZE, Z + posZ * Chunk.CHUNK_SIZE).IsTransparent;
		}

		public void RenderMeshes()
		{
			//Prepare the effect
			//rotation
			//GraphicsManager.effect.World = Matrix.CreateTranslation((float)position.X, (float)position.Y, (float)position.Z);
			GraphicsManager.effect.World = Matrix.CreateTranslation(posX * Chunk.CHUNK_SIZE - 1f, posY * Chunk.CHUNK_SIZE - 1f, posZ * Chunk.CHUNK_SIZE - 1f);

			//VoxelClient.Graphics.GraphicsDevice.

			//Draw the tris
			foreach (var pass in GraphicsManager.effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				VoxelClient.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
			}
		}

#region MESH HANDLING
		public void Update()
		{
			/*
			if (vertexBuffer != null && !vertexBuffer.IsDisposed)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
				vertexBufferCount = 0;
			}
			if (indexBuffer != null && !indexBuffer.IsDisposed)
			{
				indexBuffer.Dispose();
				indexBuffer = null;
				indexBufferCount = 0;
			}*/

			//if the buffers expired, prepare to rebuild the mesh in the next frame
			if (indexBuffer == null || vertexBuffer == null) {
				//isGenerated = false;
			}
		}

		public void Dispose()
		{
			vertexBuffer.Dispose();
			indexBuffer.Dispose();
			vertexBufferCount = 0;
			indexBufferCount = 0;
			isGenerated = false;
		}
#endregion
	}
}
