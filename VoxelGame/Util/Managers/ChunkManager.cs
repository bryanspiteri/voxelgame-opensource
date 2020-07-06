using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.World;
using VoxelGame.Util.Storage;
using VoxelGame.World.Generator;
using VoxelGame.Util.Math;
using Microsoft.Xna.Framework;
using System.Threading;

namespace VoxelGame.Util.Managers
{
	//TODO: OPTIMISE THE FUCK OUT OF THE CHUNK MANAGER ITS DIPPED THE FPS >:(
	public class ChunkManager
	{
		public string WorldDirectory = "";
		public List<Chunk> chunks;
		/// <summary>
		/// List sent to clients
		/// </summary>
		public volatile List<Chunk> chunkCache;
		public volatile List<BlockUpdate> updates;
		public List<int> chunkCacheChanges = new List<int>();
		public List<Vector3Int> LoadedChunkPositions = new List<Vector3Int>();
		public Dictionary<Vector3Int, int> ChunkIndexMapping = new Dictionary<Vector3Int, int>();
		public WorldGenerator theGenerator;

		/// <summary>
		/// How many chunks can be loaded in the X and Z axises (both positive and negative)
		/// </summary>
		public const int WorldWidth = 67108864; // 30 bits divided by chunk width 2^30 / 16

		/// <summary>
		/// How many chunks can be loaded in the Y axis (Height) direction. This is unsigned.
		/// </summary>
		public readonly static ushort MaxChunksY = 4096; // 16 bits divided by chunk width 2^16 / 16

		/// <summary>
		/// The amount of chunks that are loaded into RAM (No processing is done on these)
		/// </summary>
		public const byte SimulationPadding = 3;

		public Vector3Int previousPlayerChunk;
		byte LoadDistance = GameSettings.MinRenderDistance;
		ushort numberOfChunksLoaded = 0;
		bool updatedBlocks = false;

		public static byte MaximumChunksToLoadPerFrame
		{
			get
			{
				return (byte)FastMath.Floor(GameSettings.RenderDistance / 4);
			}
		}

		public static byte SimulationDistance
		{
			get
			{
				return (byte)(GameSettings.RenderDistance + SimulationPadding);
			}
		}

		public static ushort SimulationArea
		{
			get
			{
				return (ushort)(SimulationDistance * SimulationDistance * SimulationDistance * 3.5f - SimulationDistance * SimulationDistance * 2); // (SimulationDistance * 2) ^ 3
			}
		}

		/// <summary>
		/// Returns the player's current chunk position. Used when determining load / unload distances
		/// </summary>
		public Vector3Int playerChunk
		{
			get
			{
				return GameClient.World.player.ChunkPosition;
			}
		}

		public ChunkManager(string worldDir)
		{
			WorldDirectory = worldDir;
			//TODO: init chunk manager (Load chunks from disk)
			//initialise
			chunks = new List<Chunk>();
			chunkCache = new List<Chunk>();
			updates = new List<BlockUpdate>();
		}

		public void SetGenerator(WorldGenerator worldGenerator)
		{
			if (theGenerator != null)
			{
				Logger.fatal(-1, "Tried to assign a world generator, but there already is a world generator! Assigned new generator.");
			}
			theGenerator = worldGenerator;
			previousPlayerChunk = Vector3Int.Zero;
		}

		public void Update()
		{
			try
			{
				long ProcessingTime = 0L;

				while (ThreadManager.RunChunkManagerThread)
				{
					ProcessingTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
					Profiler.Start("Chunk Manager Update", Color.MediumPurple);

					#region Legacy Infinite Terrain Code

					//Update the tick counters

					//Handle loading and unloading

					//First unload all bad chunks
					//Bad being they are too far away
					//Also keep them in a temporary Vector3Int array representing chunk positions of loaded chunks

					#region Block Updates
					//Sync block updates
					lock (updates)
					{
						lock (LoadedChunkPositions)
					{
							Logger.info(-1, "Update Count: " + updates.Count);

							updatedBlocks = !(updates.Count == 0);
							for (int i = 0; i < updates.Count; i++)
							{
								int targetX = int.MinValue, targetY = int.MinValue, targetZ = int.MinValue;

								#region Chunk Hedging

								if (updates[i].X == 0)
								{
									targetX = updates[i].ChunkPos.X - 1;
									targetY = targetY == int.MinValue ? updates[i].ChunkPos.Y : targetY;
									targetZ = targetZ == int.MinValue ? updates[i].ChunkPos.Z : targetZ;
								}
								if (updates[i].X == Chunk.CHUNK_SIZE - 1)
								{
									targetX = updates[i].ChunkPos.X + 1;
									targetY = targetY == int.MinValue ? updates[i].ChunkPos.Y : targetY;
									targetZ = targetZ == int.MinValue ? updates[i].ChunkPos.Z : targetZ;
								}

								if (updates[i].Y == 0)
								{
									targetX = targetX == int.MinValue ? updates[i].ChunkPos.X : targetX;
									targetY = updates[i].ChunkPos.Y - 1;
									targetZ = targetZ == int.MinValue ? updates[i].ChunkPos.Z : targetZ;
								}
								if (updates[i].Y == Chunk.CHUNK_SIZE - 1)
								{
									targetX = targetX == int.MinValue ? updates[i].ChunkPos.X : targetX;
									targetY = updates[i].ChunkPos.Y + 1;
									targetZ = targetZ == int.MinValue ? updates[i].ChunkPos.Z : targetZ;
								}

								if (updates[i].Z == 0)
								{
									targetX = targetX == int.MinValue ? updates[i].ChunkPos.X : targetX;
									targetY = targetY == int.MinValue ? updates[i].ChunkPos.Y : targetY;
									targetZ = updates[i].ChunkPos.Z - 1;
								}
								if (updates[i].Z == Chunk.CHUNK_SIZE - 1)
								{
									targetX = targetX == int.MinValue ? updates[i].ChunkPos.X : targetX;
									targetY = targetY == int.MinValue ? updates[i].ChunkPos.Y : targetY;
									targetZ = updates[i].ChunkPos.Z + 1;
								}
								#endregion

								Vector3Int targetPosX = new Vector3Int(targetX, updates[i].ChunkPos.Y, updates[i].ChunkPos.Z);
								Vector3Int targetPosY = new Vector3Int(updates[i].ChunkPos.X, targetY, updates[i].ChunkPos.Z);
								Vector3Int targetPosZ = new Vector3Int(updates[i].ChunkPos.X, updates[i].ChunkPos.Y, targetZ);

								if (LoadedChunkPositions.Contains(updates[i].ChunkPos))
								{
									if (chunks[ChunkIndexMapping[updates[i].ChunkPos]].isLoaded && chunks.Count <= ChunkIndexMapping[updates[i].ChunkPos])
									{
										chunks[ChunkIndexMapping[updates[i].ChunkPos]].SetBlock(updates[i].BlockID, updates[i].X, updates[i].Y, updates[i].Z, updates[i].Reason);
									}
								}

								if (LoadedChunkPositions.Contains(targetPosX))
								{
									if (chunks[ChunkIndexMapping[targetPosX]].isLoaded && chunks.Count <= ChunkIndexMapping[targetPosX])
									{
										chunks[ChunkIndexMapping[targetPosX]].UpdateNeeded = true;
									}
								}

								if (LoadedChunkPositions.Contains(targetPosY))
								{
									if (chunks[ChunkIndexMapping[targetPosY]].isLoaded && chunks.Count <= ChunkIndexMapping[targetPosY])
									{
										chunks[ChunkIndexMapping[targetPosY]].UpdateNeeded = true;
									}
								}

								if (LoadedChunkPositions.Contains(targetPosZ))
								{
									if (chunks[ChunkIndexMapping[targetPosZ]].isLoaded && chunks.Count <= ChunkIndexMapping[targetPosZ])
									{
										chunks[ChunkIndexMapping[targetPosZ]].UpdateNeeded = true;
									}
								}
							}
							updates.Clear();
						}
					}
					#endregion

					#region Unloading
					lock (LoadedChunkPositions)
					{
						for (int i = 0; i < chunks.Count; i++)
						{
							if (chunks[i].isLoaded)
							{
								if (previousPlayerChunk != playerChunk)
								{
									Profiler.Start("Chunk Unload", Color.Red);
									//Calculate distance in the form of a cube
									Vector3Int chunkPos = new Vector3Int(chunks[i].posX, chunks[i].posY, chunks[i].posZ);

									//GameClient.LOGGER.info(0, "Currently dealing with chunk (" + chunkPos.X + " " + chunkPos.Y + " " + chunkPos.Z + ")!");

									if (LoadedChunkPositions.Contains(chunkPos) &&
										chunkPos.X > playerChunk.X + SimulationDistance || chunkPos.X < playerChunk.X - SimulationDistance - 1
										|| chunkPos.Y > playerChunk.Y + SimulationDistance || chunkPos.Y < playerChunk.Y - SimulationDistance - 1
										|| chunkPos.Z > playerChunk.Z + SimulationDistance || chunkPos.Z < playerChunk.Z - SimulationDistance - 1

										//Max world bounds
										|| chunkPos.X > WorldWidth || chunkPos.X < -WorldWidth - 1
										|| chunkPos.Y > MaxChunksY || chunkPos.Y < 0
										|| chunkPos.Z > WorldWidth || chunkPos.Z < -WorldWidth - 1)
									{
										UnloadChunk(chunks[i].posX, chunks[i].posY, chunks[i].posZ);

									}
									else
									{
										//Add to LoadedChunkPositions
										if (!LoadedChunkPositions.Contains(chunkPos))
										{
											LoadedChunkPositions.Add(chunkPos);
										}
									}
									Profiler.Stop("Chunk Unload");

									Profiler.Start("Chunk Manager Update", Color.MediumPurple);

									//Update the chunk
									chunks[i].Update();
								}
							}
						}
						#endregion

					#region Loading
						//Now that we've unloaded all chunks, we can see which chunks we need to load
						byte numberOfChunksLoadedThisFrame = 0;

						//This algorithm loads all chunks on the z axis, then the x axis and lastly the y axis
						//It only load 1 z layer every frame (a layer is anything from 1 chunk to RENDER_DISTANCE chunks
						if (numberOfChunksLoaded < SimulationArea + 1)
						{
							for (int i = 0; i < LoadDistance; i++)
							{
								Profiler.Start("Chunk Load Math", Color.DarkOliveGreen);

								//Calculate the bounds of chunks that can be loaded
								int minX = MathHelper.Clamp(playerChunk.X - i, playerChunk.X - SimulationDistance, playerChunk.X + SimulationDistance);
								int minY = MathHelper.Clamp(playerChunk.Y - i, playerChunk.Y - SimulationDistance, playerChunk.Y + SimulationDistance);
								int minZ = MathHelper.Clamp(playerChunk.Z - i, playerChunk.Z - SimulationDistance, playerChunk.Z + SimulationDistance);

								int maxX = MathHelper.Clamp(playerChunk.X + i, playerChunk.X - SimulationDistance, playerChunk.X + SimulationDistance);
								int maxY = MathHelper.Clamp(playerChunk.Y + i, playerChunk.Y - SimulationDistance, playerChunk.Y + SimulationDistance);
								int maxZ = MathHelper.Clamp(playerChunk.Z + i, playerChunk.Z - SimulationDistance, playerChunk.Z + SimulationDistance);

								//World boundaries. Chunks cant be loaded outside these boundaries
								minX = MathHelper.Clamp(minX, -WorldWidth, WorldWidth);
								minY = MathHelper.Clamp(minY, 0, MaxChunksY);
								minZ = MathHelper.Clamp(minZ, -WorldWidth, WorldWidth);

								maxX = MathHelper.Clamp(maxX, -WorldWidth, WorldWidth);
								maxY = MathHelper.Clamp(maxY, 0, MaxChunksY);
								maxZ = MathHelper.Clamp(maxZ, -WorldWidth, WorldWidth);

								Profiler.Stop("Chunk Load Math");

								for (int y = minY; y < maxY; y++)
								{
									for (int x = minX; x < maxX; x++)
									{
										for (int z = minZ; z < maxZ; z++)
										{
											Profiler.Start("Chunk Load", Color.Ivory);

											Vector3Int chunkPos = new Vector3Int(x, y, z);

											//Check if we should load this chunk (if its within simulation distance & isn't inside LoadedChunkPositions>)
											if (!LoadedChunkPositions.Contains(chunkPos) &&
												chunkPos.X < playerChunk.X + SimulationDistance && chunkPos.X > playerChunk.X - SimulationDistance
												&& chunkPos.Y < playerChunk.Y + SimulationDistance && chunkPos.Y > playerChunk.Y - SimulationDistance
												&& chunkPos.Z < playerChunk.Z + SimulationDistance && chunkPos.Z > playerChunk.Z - SimulationDistance

												//Max world bounds
												&& chunkPos.X < WorldWidth && chunkPos.X > -WorldWidth
												&& chunkPos.Y < MaxChunksY && chunkPos.Y > -1
												&& chunkPos.Z < WorldWidth && chunkPos.Z > -WorldWidth)
											{
												Profiler.Stop("Chunk Load");

												//Chunk is unloaded. So load it
												Profiler.Start("Chunk Generation", Color.DeepPink);

												//Chunk NextChunkToLoad = SolidChunk.Generate(chunkPos.X, chunkPos.Y, chunkPos.Z, 1);
												Chunk NextChunkToLoad = FetchChunk(chunkPos.X, chunkPos.Y, chunkPos.Z);
												RegisterChunk(NextChunkToLoad);

												Profiler.Stop("Chunk Generation");

												Profiler.Start("Chunk Load", Color.Ivory);

												//RegisterChunk(FetchChunk(chunkPos.X, chunkPos.Y, chunkPos.Z));

												numberOfChunksLoaded++;
												numberOfChunksLoadedThisFrame++;

												Profiler.Stop("Chunk Load");

												//If we loaded more than the maximum number of chunks, stop loading chunks this frame to keep a playable framerate
												if (numberOfChunksLoadedThisFrame > MaximumChunksToLoadPerFrame)
												{
													break;
												}
											}
											else
											{
												Profiler.Stop("Chunk Load");
											}
										}

										//If we loaded more than the maximum number of chunks, stop loading chunks this frame to keep a playable framerate
										if (numberOfChunksLoadedThisFrame > MaximumChunksToLoadPerFrame)
										{
											break;
										}
									}

									//Break the loop if we loaded a single chunk
									if (numberOfChunksLoadedThisFrame > 0)
									{
										break;
									}
								}

								//Break the loop if we loaded a single chunk
								if (numberOfChunksLoadedThisFrame > 0)
								{
									break;
								}
							}

							if (numberOfChunksLoadedThisFrame < 1)
							{
								LoadDistance++;
							}
							if (LoadDistance > SimulationDistance)
							{
								LoadDistance = 2;
							}
						}
					}

					previousPlayerChunk = playerChunk;
					#endregion

					#endregion

					#region Fixed Terrain Code
					/*byte numberOfChunksLoadedThisFrame = 0;

					//This algorithm loads all chunks on the z axis, then the x axis and lastly the y axis
					//It only load 1 z layer every frame (a layer is anything from 1 chunk to RENDER_DISTANCE chunks
					if (numberOfChunksLoaded <= SimulationArea)
					{
						Profiler.Start("Chunk Load Loop");

						for (int i = 0; i < LoadDistance; i++)
						{
							Profiler.Start("Chunk Load Math", Color.DarkOliveGreen);

							//Calculate the bounds of chunks that can be loaded
							int minX = playerChunk.X - i;
							int minY = playerChunk.Y - i;
							int minZ = playerChunk.Z - i;

							int maxX = playerChunk.X + i;
							int maxY = playerChunk.Y + i;
							int maxZ = playerChunk.Z + i;

							//World boundaries. Chunks cant be loaded outside these boundaries
							minX = MathHelper.Clamp(minX, -8, 8);
							minY = MathHelper.Clamp(minY, 0, 16);
							minZ = MathHelper.Clamp(minZ, -8, 8);

							maxX = MathHelper.Clamp(maxX, -8, 8);
							maxY = MathHelper.Clamp(maxY, 0, 16);
							maxZ = MathHelper.Clamp(maxZ, -8, 8);

							Profiler.Stop("Chunk Load Math");

							for (int y = minY; y < maxY; y++)
							{
								for (int x = minX; x < maxX; x++)
								{
									for (int z = minZ; z < maxZ; z++)
									{
										Profiler.Start("Chunk Load", Color.Ivory);

										Vector3Int chunkPos = new Vector3Int(x, y, z);

										//Check if we should load this chunk (if its within simulation distance & isn't inside LoadedChunkPositions>)
										if (!LoadedChunkPositions.Contains(chunkPos) &&
											chunkPos.X < playerChunk.X + SimulationDistance && chunkPos.X > playerChunk.X - SimulationDistance
											&& chunkPos.Y < playerChunk.Y + SimulationDistance && chunkPos.Y > playerChunk.Y - SimulationDistance
											&& chunkPos.Z < playerChunk.Z + SimulationDistance && chunkPos.Z > playerChunk.Z - SimulationDistance

											//Max world bounds
											&& chunkPos.X < WorldWidth && chunkPos.X > -WorldWidth
											&& chunkPos.Y < MaxChunksY && chunkPos.Y > -1
											&& chunkPos.Z < WorldWidth && chunkPos.Z > -WorldWidth)
										{
											//Chunk is unloaded. So load it
											Profiler.Start("Chunk Generation", Color.DeepPink);
											Chunk NextChunkToLoad = SolidChunk.Generate(chunkPos.X, chunkPos.Y, chunkPos.Z, 1);
											RegisterChunk(NextChunkToLoad);
											Profiler.Stop("Chunk Generation");

											//RegisterChunk(FetchChunk(chunkPos.X, chunkPos.Y, chunkPos.Z));

											numberOfChunksLoaded++;
											numberOfChunksLoadedThisFrame++;

											Profiler.Stop("Chunk Load");

											//If we loaded more than the maximum number of chunks, stop loading chunks this frame to keep a playable framerate
											if (numberOfChunksLoadedThisFrame > MaximumChunksToLoadPerFrame)
											{
												break;
											}
										}
										else
										{
											Profiler.Stop("Chunk Load");
										}
									}

									//If we loaded more than the maximum number of chunks, stop loading chunks this frame to keep a playable framerate
									if (numberOfChunksLoadedThisFrame > MaximumChunksToLoadPerFrame)
									{
										break;
									}
								}

								//Break the loop if we loaded a single chunk
								if (numberOfChunksLoadedThisFrame > 0)
								{
									break;
								}
							}

							//Break the loop if we loaded a single chunk
							if (numberOfChunksLoadedThisFrame > 0)
							{
								break;
							}
						}

						if (numberOfChunksLoadedThisFrame < 1)
						{
							LoadDistance++;
						}
						if (LoadDistance > SimulationDistance)
						{
							LoadDistance = 2;
						}
					}*/
					#endregion

					SyncCache();

					Profiler.Stop("Chunk Manager Update");
					ProcessingTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds - ProcessingTime;

					//Wait 1/60 - the time elapsed, clamped between 0 and int.max
					Thread.Sleep(FastMath.FastClamp((int)(50 - ProcessingTime), 50, int.MaxValue));
				}
			}
			catch (ThreadAbortException ex)
			{
				//Thread aborting!
				Logger.fatal(-1, "Thread \"" + Thread.CurrentThread.Name + "\" aborted! This is usually normal!");
			}
		}

		//TODO: Load / unload / generate / save

		/// <summary>
		/// Registers a Chunk into memory
		/// </summary>
		/// <param name="ChunkToRegister"></param>
		public void RegisterChunk(Chunk ChunkToRegister)
		{
			//TODO: Loop through chunks, if posX,Y,Z are identical, do stuff and then return

			//Use the index mapping to avoid the loop
			Vector3Int chunkPos = new Vector3Int(ChunkToRegister.posX, ChunkToRegister.posY, ChunkToRegister.posZ);

			lock (LoadedChunkPositions)
			{
				if (LoadedChunkPositions.Contains(chunkPos) && ChunkIndexMapping.ContainsKey(chunkPos))
				{
					if (chunks[ChunkIndexMapping[chunkPos]].isLoaded)
					{
						Logger.error(-1, "Chunk is already loaded!");
						return;
					}
				}

				ChunkIndexMapping.Add(chunkPos, ChunkIndexMapping.Count);
				LoadedChunkPositions.Add(chunkPos);
				chunks.Add(ChunkToRegister);

				//Tell all neighbouring chunks to rebuild their meshes
				for (int i = 0; i < 6; i++)
				{
					Vector3Int thisChunkPos = chunkPos + ((Side)i).GetOffset();
					if (LoadedChunkPositions.Contains(thisChunkPos) && ChunkIndexMapping.ContainsKey(thisChunkPos))
					{
						if (chunks[ChunkIndexMapping[thisChunkPos]].isLoaded)
						{
							chunks[ChunkIndexMapping[thisChunkPos]].UpdateNeeded = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the chunk at the specified chunk coordinates.
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Z"></param>
		/// <returns></returns>
		public Chunk FetchChunk(int X, int Y, int Z)
		{
			Chunk theChunk = LoadChunk(X, Y, Z);
			if (theChunk == null)
			{
				theChunk = GenerateChunk(X, Y, Z);
			}
			return theChunk;
		}

		/// <summary>
		/// Loads a chunk from disk. Returns null if the chunk doesn't exist
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Z"></param>
		/// <returns></returns>
		public Chunk LoadChunk(int X, int Y, int Z)
		{
			//TODO: Read from disk
			return null;
		}
		
		/// <summary>
		/// Generates a chunk at the specified coordinates
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Z"></param>
		/// <returns></returns>
		public Chunk GenerateChunk(int X, int Y, int Z)
		{
			Logger.info(-1, "Generating chunk at " + X + ", " + Y + " , " + Z);

			//Use the index mapping to avoid the loop
			Vector3Int chunkPos = new Vector3Int(X, Y, Z);

			lock (LoadedChunkPositions)
			{
				if (LoadedChunkPositions.Contains(chunkPos) && ChunkIndexMapping.ContainsKey(chunkPos))
				{
					if (chunks[ChunkIndexMapping[chunkPos]].isLoaded)
					{
						Logger.error(-1, "Chunk is already loaded!");
						return chunks[ChunkIndexMapping[chunkPos]];
					}
				}
			}

			return theGenerator.GenerateChunkAt(X, Y, Z);
		}

		public void SaveChunk(Chunk tosave)
		{

		}

		/// <summary>
		/// Unloads the chunks from RAM
		/// </summary>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Z"></param>
		public void UnloadChunk(int X, int Y, int Z)
		{
			Vector3Int chunkPos = new Vector3Int(X, Y, Z);

			lock (LoadedChunkPositions)
			{
				if (LoadedChunkPositions.Contains(chunkPos) && ChunkIndexMapping.ContainsKey(chunkPos))
				{
					if (chunks[ChunkIndexMapping[chunkPos]].isLoaded)
					{
						int chunkIndex = ChunkIndexMapping[chunkPos];
						//Chunks[i] is the chunk we wish to unload
						//Save the chunk
						SaveChunk(chunks[chunkIndex]);
						//Unload the chunk
						chunks[chunkIndex].Dispose();
						//Remove it from the chunk list
						chunks.RemoveAt(chunkIndex);

						//We are done using the index mapping, and since the chunk is now unloaded, remove it
						ChunkIndexMapping.Remove(chunkPos);
						LoadedChunkPositions.Remove(chunkPos);
						//IMPORTANT (UNLOADING): MOVE EVERYTHING BACK ONE SPACE
					}
				}
				else
				{
					Logger.error(-1, "Chunk (" + X + " " + Y + " " + Z + ") doesn't exist in the current context!");
					return;
				}

				//Tell all neighbouring chunks to rebuild their meshes
				for (int i = 0; i < 6; i++)
				{
					Vector3Int thisChunkPos = chunkPos + ((Side)i).GetOffset();
					if (LoadedChunkPositions.Contains(thisChunkPos) && ChunkIndexMapping.ContainsKey(thisChunkPos) && chunks.Count < ChunkIndexMapping[thisChunkPos])
					{
						if (chunks[ChunkIndexMapping[thisChunkPos]].isLoaded)
						{
							chunks[ChunkIndexMapping[thisChunkPos]].UpdateNeeded = true;
						}
					}
				}
			}
		}

		public void SyncCache()
		{
			lock (LoadedChunkPositions)
			{
				if (chunkCache.Count != chunks.Count || updatedBlocks)
				{
					//If we changed ANYTHING in the cache, update the changed chunks
					if (chunkCacheChanges.Count > 0) {
						for (int i = 0; i < chunkCacheChanges.Count; i++)
						{
							//If the chunk wasn't unloaded
							if (chunkCache.Count < chunkCacheChanges[i])
							{
								chunkCache.Add(chunks[chunkCacheChanges[i]]);
							}
							//The chunk was unloaded, so remove it
							else
							{
								chunkCache.Remove(chunks[chunkCacheChanges[i]]);
							}
						}
						//clear the change cache
						chunkCacheChanges.Clear();
					}
				}
			}
		}

		public void SetBlock(Vector3Int chunk, Vector3Int block, uint blockID, uint Reason)
		{
			BlockUpdate newUpdate = new BlockUpdate(chunk, block, blockID, Reason);
			lock (updates)
			{
				updates.Add(newUpdate);
			}
		}

		/// <summary>
		/// Sorts the Chunks by distance
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private int ChunkDistanceSorter(Chunk a, Chunk b)
		{
			float da = Vector3Int.DistanceSquared(new Vector3Int(a.posX, a.posY, a.posZ), playerChunk);
			float db = Vector3Int.DistanceSquared(new Vector3Int(b.posX, b.posY, b.posZ), playerChunk);

			if (da < db)
				return -1;
			else if (da > db)
				return 1;
			return 0;
		}

		/// <summary>
		/// Sorts the Vector3Ints by distance
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private int DistanceSorter(Vector3Int a, Vector3Int b)
		{
			float da = Vector3Int.DistanceSquared(a, playerChunk);
			float db = Vector3Int.DistanceSquared(b, playerChunk);

			if (da < db)
				return -1;
			else if (da > db)
				return 1;
			return 0;
		}
	}

	public struct BlockUpdate
	{
		public Vector3Int ChunkPos;

		public int X, Y, Z;

		public uint BlockID;
		public uint Reason;

		public BinaryTagNode Metadata;

		public BlockUpdate(Vector3Int chunk, int X, int Y, int Z, uint Block, uint Reason)
		{
			ChunkPos = chunk;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			BlockID = Block;
			this.Reason = Reason;
			Metadata = null;
		}

		public BlockUpdate(Vector3Int chunk, Vector3Int local, uint Block, uint Reason)
		{
			ChunkPos = chunk;
			X = local.X;
			Y = local.Y;
			Z = local.Z;
			BlockID = Block;
			this.Reason = Reason;
			Metadata = null;
		}

		public BlockUpdate(Vector3Int chunk, int X, int Y, int Z, uint Block, uint Reason, BinaryTagNode meta)
		{
			ChunkPos = chunk;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			BlockID = Block;
			this.Reason = Reason;
			Metadata = meta;
		}

		public BlockUpdate(Vector3Int chunk, Vector3Int local, uint Block, uint Reason, BinaryTagNode meta)
		{
			ChunkPos = chunk;
			X = local.X;
			Y = local.Y;
			Z = local.Z;
			BlockID = Block;
			this.Reason = Reason;
			Metadata = meta;
		}
	}
}
