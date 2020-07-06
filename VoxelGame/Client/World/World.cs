using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.World.Generator;
using VoxelGame.Engine;
using VoxelGame.Util;
using VoxelGame.Util.Managers;
using VoxelGame.Blocks;
using VoxelGame.Util.Storage;
using System.IO;
using VoxelGame.Client.Renderers;
using VoxelGame.Util.Math;
using VoxelGame.Networking;

namespace VoxelGame.World
{
	public class World : BlockWorld
	{
		//TODO: make spawn calculated from seed
		public float spawnX = 8, spawnY = 32, spawnZ = 8;
		public Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();
		public List<int> chunkHashes = new List<int>();
		public long seed = 0L;
		public ChunkManager theChunkManager;
		public WorldOptions GameRules;
		public string WorldDirectory = GameClient.ClientDirectory;

		//Multiplayer version => send particle packet to players
		public NetworkingManager networkingManager;
		public ParticleEngine particleEngine;

		public int ChunkUpdates = 0, BlockUpdates = 0;

		public long timeTick = 0;

		/// <summary>
		/// Returns the current day tick
		/// </summary>
		public ushort dayTick {
			get
			{
				return (ushort)(timeTick % (20 * 60 * 24)); //ticks * min * day in mins
			}
		}

		public float CelestialAngle
		{
			get
			{
				return dayTick / 2880f;
			}
		}

		public byte dayBrightness
		{
			get
			{
				byte outBright = (byte)MathHelper.Clamp(dayTick, 5, 15);
				return 15;
			}
		}

		public PacketTransferer packetTransferer
		{
			get 
			{
				return networkingManager.packetTransfer; 
			}
			set
			{
				if (networkingManager != null)
				{
					networkingManager.packetTransfer = value;
				}
			}
		}

		/// <summary>
		/// How much time elapsed since the last Update
		/// </summary>
		double timer = 0;

		/// <summary>
		/// Controls how frequently Tick is called. It is called how many times this value is set to.
		/// </summary>
		public int TickSpeed
		{
			get
			{
				return (int)_tickspeed;
			}
			set
			{
				//Tick Speed must be greater than 1, (min value)
				//Let the user use a number as big as they'd like, its their machine that'll crash, so let them BSOD if they wish to do so.
				_tickspeed = (uint)MathHelper.Clamp(value, 1, int.MaxValue);
			}
		}
		private uint _tickspeed = 20;

		public World (long seed, string directory)
		{
			WorldDirectory = directory;
			Logger.info(-1, "Initializing world (" + WorldDirectory + ")");

			theChunkManager = new ChunkManager(WorldDirectory);
			this.seed = seed;
			//generate terrain
			NormalWorldGenerator theGenerator = new NormalWorldGenerator(seed);

			for (int z = -2; z < 2; z++)
			{
				for (int y = 0; y < 4; y++)
				{
					for (int x = -2; x < 2; x++)
					{
						RegisterChunk(theGenerator.GenerateChunkAt(x, y, z));
					}
				}
			}

			player.position = new Vector3(spawnX, spawnY, spawnZ);

			GameRules = new WorldOptions();
		}

		public World (NetworkingManager networking)
		{

		}

		public override void Begin()
		{
			//player.Initialise();
			base.Begin();

			//Load the chunks before initializing the server.
			LoadChunks();

#if CLIENT
			Client.UI.UiRegisterer.loadingUI.visible = false;
#endif

			particleEngine = new ParticleEngine(player.camera);
		}

		public override void BeforeUpdate()
		{
			//player.BeforeUpdate();
			base.BeforeUpdate();
		}

		/// <summary>
		/// Called every frame
		/// </summary>
		/// <param name="gameTime">Snapshot of timing values</param>
		public override void Update(GameTime gameTime)
		{
			networkingManager.Update();

			if (!Paused)
			{
				base.Update(gameTime);
				//TODO: Deprecate
				for (int chunkID = 0; chunkID < chunks.Count; chunkID++)
				{
					chunks[chunkHashes[chunkID]].Update();
				}
				timer += gameTime.ElapsedGameTime.TotalSeconds;
				float updateTime = 1f / 20;

				while (timer >= updateTime)
				{
					Tick();
					timer -= updateTime;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
		}

		public void RegisterChunk(Chunk ChunkToRegister)
		{
			if (chunks.ContainsKey(ChunkToRegister.GetHashCode()))
			{
				Logger.error(-1, "Chunk is already loaded! (Pos: " + ChunkToRegister.posX + ",  Y: " + ChunkToRegister.posY + ",  Z: " + ChunkToRegister.posZ + ")");
				return;
			}
			chunks.Add(ChunkToRegister.GetHashCode(), ChunkToRegister);
			chunkHashes.Add(ChunkToRegister.GetHashCode());

			//GameClient.Instance.worldRenderer.AddChunk(ChunkToRegister);
			//Logger.info(-1, "ADDED CHUNK AT CHUNK COORDS X: " + ChunkToRegister.posX + ",  Y: " + ChunkToRegister.posY + ",  Z: " + ChunkToRegister.posZ + " Hashcode: " + ChunkToRegister.GetHashCode());
		}

		#region Block Setters and Getters

		public uint GetBlockIdAt(int X, int Y, int Z)
		{
			int chunkX, chunkY, chunkZ, x, y, z;

			ConvertToBlockPos(X, Y, Z, out chunkX, out chunkY, out chunkZ, out x, out y, out z);

			/*chunkX = X / Chunk.CHUNK_SIZE;
			chunkY = Y / Chunk.CHUNK_SIZE;
			chunkZ = Z / Chunk.CHUNK_SIZE;

			//Fix negative values
			if (X < 1)
			{
				chunkX--;
			}
			if (Y < 1)
			{
				chunkY--;
			}
			if (Z < 1)
			{
				chunkZ--;
			}

			//local chunk pos
			int x = X;
			int y = Y;
			int z = Z;

			if (chunkX != 0)
			{
				x = X % Chunk.CHUNK_SIZE;
				if (x < 0) x += Chunk.CHUNK_SIZE;
			}
			if (chunkY != 0)
			{
				y = Y % Chunk.CHUNK_SIZE;
				if (y < 0) y += Chunk.CHUNK_SIZE;
			}
			if (chunkZ != 0)
			{
				z = Z % Chunk.CHUNK_SIZE;
				if (z < 0) z += Chunk.CHUNK_SIZE;
			}*/

			Vector3Int chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);

			if (X >= 0 && X < 16 && Y >= 0 && Y < 16 && Z >= 0 && Z < 16)
			{
				//GameClient.LOGGER.info(0, "Getting block (" + X + ", " + Y + ", " + Z + ") in chunk " + chunkPos);
			}

			/*lock (theChunkManager.LoadedChunkPositions)
			{
				//GameClient.LOGGER.info(-1, "theChunkManager.LoadedChunkPositions.Contains(" + chunkPos + "): " + theChunkManager.LoadedChunkPositions.Contains(chunkPos));
				if (theChunkManager.LoadedChunkPositions.Contains(chunkPos) && theChunkManager.chunkCache.Count >= theChunkManager.ChunkIndexMapping[chunkPos])
				{
					//GameClient.LOGGER.info(-1, "theChunkManager.ChunkIndexMapping[" + chunkPos + "]: " + theChunkManager.ChunkIndexMapping[chunkPos]);
					//GameClient.LOGGER.info(-1, "theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[" + chunkPos+"]]: " + theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[chunkPos]]);

					if (theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[chunkPos]].isLoaded)
					{
						return theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[chunkPos]].GetBlockAt(x, y, z);
					}
				}
			}*/


			/*for (int i = 0; i < chunks.Count; i++)
			{
				if (chunks[i].isLoaded && chunks[i].posX == chunkX && chunks[i].posY == chunkY && chunks[i].posZ == chunkZ)
				{
					return chunks[i].GetBlockAt(x, y, z);
				}
			}*/

			var hashCode = Chunk.GetHash(chunkX, chunkY, chunkZ);

			if (chunks.ContainsKey(hashCode))
			{
				return chunks[hashCode].GetBlockAt(x, y, z);
			}

			return 0;
		}

		public void SetBlockAt(int X, int Y, int Z, uint id, uint reason)
		{
			int chunkX, chunkY, chunkZ, x, y, z;

			ConvertToBlockPos(X, Y, Z, out chunkX, out chunkY, out chunkZ, out x, out y, out z);

			//theChunkManager.SetBlock(new Vector3Int(chunkX, chunkY, chunkZ), new Vector3Int(x, y, z), id, reason);

			/*chunkX = X / Chunk.CHUNK_SIZE;
			chunkY = Y / Chunk.CHUNK_SIZE;
			chunkZ = Z / Chunk.CHUNK_SIZE;

			//Fix negative values
			if (X < 1)
			{
				chunkX--;
			}
			if (Y < 1)
			{
				chunkY--;
			}
			if (Z < 1)
			{
				chunkZ--;
			}

			//local chunk pos
			int x = X;
			int y = Y;
			int z = Z;

			if (chunkX != 0)
			{
				x = X % Chunk.CHUNK_SIZE;
				if (x < 0) x += Chunk.CHUNK_SIZE;
			}
			if (chunkY != 0)
			{
				y = Y % Chunk.CHUNK_SIZE;
				if (y < 0) y += Chunk.CHUNK_SIZE;
			}
			if (chunkZ != 0)
			{
				z = Z % Chunk.CHUNK_SIZE;
				if (z < 0) z += Chunk.CHUNK_SIZE;
			}*/
			
			int targetX = int.MinValue, targetY = int.MinValue, targetZ = int.MinValue;

			#region Chunk Hedging

			if (x == 0)
			{
				targetX = chunkX - 1;
				targetY = targetY == int.MinValue ? chunkY : targetY;
				targetZ = targetZ == int.MinValue ? chunkZ : targetZ;
			}
			if (x == Chunk.CHUNK_SIZE - 1)
			{
				targetX = chunkX + 1;
				targetY = targetY == int.MinValue ? chunkY : targetY;
				targetZ = targetZ == int.MinValue ? chunkZ : targetZ;
			}

			if (y == 0)
			{
				targetX = targetX == int.MinValue ? chunkX : targetX;
				targetY = chunkY - 1;
				targetZ = targetZ == int.MinValue ? chunkZ : targetZ;
			}
			if (y == Chunk.CHUNK_SIZE - 1)
			{
				targetX = targetX == int.MinValue ? chunkX : targetX;
				targetY = chunkY + 1;
				targetZ = targetZ == int.MinValue ? chunkZ : targetZ;
			}

			if (z == 0)
			{
				targetX = targetX == int.MinValue ? chunkX : targetX;
				targetY = targetY == int.MinValue ? chunkY : targetY;
				targetZ = chunkZ - 1;
			}
			if (z == Chunk.CHUNK_SIZE - 1)
			{
				targetX = targetX == int.MinValue ? chunkX : targetX;
				targetY = targetY == int.MinValue ? chunkY : targetY;
				targetZ = chunkZ + 1;
			}
			#endregion

			Vector3Int chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);

			/*
			//Chunk hedging
			Vector3Int targetPosX = new Vector3Int(targetX, chunkY, chunkZ);
			Vector3Int targetPosY = new Vector3Int(chunkX, targetY, chunkZ);
			Vector3Int targetPosZ = new Vector3Int(chunkX, chunkY, targetZ);

			lock (theChunkManager.LoadedChunkPositions)
			{
				lock (theChunkManager.chunkCache)
				{
					if (theChunkManager.LoadedChunkPositions.Contains(chunkPos))
					{
						if (theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[chunkPos]].isLoaded && theChunkManager.chunkCache.Count <= theChunkManager.ChunkIndexMapping[chunkPos])
						{
							theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[chunkPos]].SetBlock(id, x, y, z, reason);
						}
					}

					if (theChunkManager.LoadedChunkPositions.Contains(targetPosX))
					{
						if (theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosX]].isLoaded && theChunkManager.chunkCache.Count <= theChunkManager.ChunkIndexMapping[targetPosX])
						{
							theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosX]].UpdateNeeded = true;
						}
					}

					if (theChunkManager.LoadedChunkPositions.Contains(targetPosY))
					{
						if (theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosY]].isLoaded && theChunkManager.chunkCache.Count <= theChunkManager.ChunkIndexMapping[targetPosY])
						{
							theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosY]].UpdateNeeded = true;
						}
					}

					if (theChunkManager.LoadedChunkPositions.Contains(targetPosZ))
					{
						if (theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosZ]].isLoaded && theChunkManager.chunkCache.Count <= theChunkManager.ChunkIndexMapping[targetPosZ])
						{
							theChunkManager.chunkCache[theChunkManager.ChunkIndexMapping[targetPosZ]].UpdateNeeded = true;
						}
					}
				}
			}*/
			/*
			for (int i = 0; i < chunks.Count; i++)
			{
				if (chunks[i].isLoaded && chunks[i].posX == chunkX && chunks[i].posY == chunkY && chunks[i].posZ == chunkZ)
				{
					chunks[i].SetBlock(id, (byte)x, (byte)y, (byte)z, reason);
				}

				//Chunk hedging
				if (chunks[i].isLoaded &&
					((chunks[i].posX == targetX && chunks[i].posY == chunkY && chunks[i].posZ == chunkZ)
					|| (chunks[i].posX == chunkX && chunks[i].posY == targetY && chunks[i].posZ == chunkZ)
					|| (chunks[i].posX == chunkX && chunks[i].posY == chunkY && chunks[i].posZ == targetZ)))
				{
					chunks[i].UpdateNeeded = true;
				}
			}*/
			
			var hashCode = Chunk.GetHash(chunkX, chunkY, chunkZ);

			if (chunks.ContainsKey(hashCode))
			{
				chunks[hashCode].SetBlock(id, (byte)x, (byte)y, (byte)z, reason);
			}

			//targetX
			hashCode = Chunk.GetHash(targetX, chunkY, chunkZ);

			if (chunks.ContainsKey(hashCode))
			{
				chunks[hashCode].UpdateNeeded = true;
			}

			//targetY
			hashCode = Chunk.GetHash(chunkX, targetY, chunkZ);

			if (chunks.ContainsKey(hashCode))
			{
				chunks[hashCode].UpdateNeeded = true;
			}

			//targetZ
			hashCode = Chunk.GetHash(chunkX, chunkY, targetZ);

			if (chunks.ContainsKey(hashCode))
			{
				chunks[hashCode].UpdateNeeded = true;
			}

		}
		#endregion

		#region Block Overloads

		public uint GetBlockIdAt(Vector3Int coord)
		{
			return GetBlockIdAt(coord.X, coord.Y, coord.Z);
		}

		public Block GetBlockAt(Vector3Int coord)
		{
			return BlockManager.GetBlock(GetBlockIdAt(coord));
		}

		public Block GetBlockAt(int X, int Y, int Z)
		{
			return BlockManager.GetBlock(GetBlockIdAt(X, Y, Z));
		}

		public void SetBlockAt(int X, int Y, int Z, uint id, BlockPlaceReason reason)
		{
			SetBlockAt(X, Y, Z, id, (uint)reason);
		}

		public void SetBlockAt(int X, int Y, int Z, uint id, BlockBreakReason reason)
		{
			SetBlockAt(X, Y, Z, id, (uint)reason);
		}

		public void SetBlockAt(Vector3Int pos, uint id, uint reason)
		{
			SetBlockAt(pos.X, pos.Y, pos.Z, id, reason);
		}

		public void SetBlockAt(Vector3Int pos, uint id, BlockPlaceReason reason)
		{
			SetBlockAt(pos.X, pos.Y, pos.Z, id, (uint)reason);
		}

		public void SetBlockAt(Vector3Int pos, uint id, BlockBreakReason reason)
		{
			SetBlockAt(pos.X, pos.Y, pos.Z, id, (uint)reason);
		}
		#endregion

		#region Ticking
		/// <summary>
		/// This method is called depending on the tickSpeed variable
		/// </summary>
		public void Tick()
		{
			//advanced the day tick
			timeTick++;

			ChunkUpdates = 0;
			BlockUpdates = 0;

			if (timeTick % TickSpeed == 0)
			{
				//Call the chunk's tick methods\
				for (int i = 0; i < chunks.Count; i++)
				{
					chunks[chunkHashes[i]].OnTick();
				}
			}
		}

		public void QueueBlockUpdate(int X, int Y, int Z)
		{
			QueueBlockUpdate(new Vector3Int(X, Y, Z));
		}

		public void QueueBlockUpdate(Vector3Int coord)
		{
			int chunkX, chunkY, chunkZ, x, y, z;

			ConvertToBlockPos(coord.X, coord.Y, coord.Z, out chunkX, out chunkY, out chunkZ, out x, out y, out z);

			var hashCode = Chunk.GetHash(chunkX, chunkY, chunkZ);

			if (chunks.ContainsKey(hashCode))
			{
				var hash = Chunk.GetHash(x, y, z);
				if (!chunks[hashCode].chunk_updates.ContainsKey(hash))
				{
					chunks[hashCode].chunk_updates.Add(hash, new Vector3Int(x, y, z));
				}
			}
		}

		#endregion

		public static void ConvertToBlockPos(int X, int Y, int Z, out int chunkX, out int chunkY, out int chunkZ, out int x, out int y, out int z)
		{
			chunkX = X / Chunk.CHUNK_SIZE;
			chunkY = Y / Chunk.CHUNK_SIZE;
			chunkZ = Z / Chunk.CHUNK_SIZE;

			//Fix negative values
			/*if (X < 0)
			{
				chunkX--;
			}
			if (Y < 0)
			{
				chunkY--;
			}
			if (Z < 0)
			{
				chunkZ--;
			}*/

			//local chunk pos
			x = X - chunkX * Chunk.CHUNK_SIZE;
			y = Y - chunkY * Chunk.CHUNK_SIZE;
			z = Z - chunkZ * Chunk.CHUNK_SIZE;

			//0 Chunks
			if (chunkX == 0)
			{
				x = X;
			}
			if (chunkY == 0)
			{
				y = Y;
			}
			if (chunkZ == 0)
			{
				z = Z;
			}

			//Negative Chunks
			if (chunkX <= 0 && x < 0)
			{
				x += Chunk.CHUNK_SIZE;
				chunkX--;
			}
			if (chunkY <= 0 && y < 0)
			{
				y += Chunk.CHUNK_SIZE;
				chunkY--;
			}
			if (chunkZ <= 0 && z < 0)
			{
				z += Chunk.CHUNK_SIZE;
				chunkZ--;
			}
			/*if (chunkX != 0)
			{
				x = X % Chunk.CHUNK_SIZE;
				if (x < 0) x += Chunk.CHUNK_SIZE;
			}
			if (chunkY != 0)
			{
				y = Y % Chunk.CHUNK_SIZE;
				if (y < 0) y += Chunk.CHUNK_SIZE;
			}
			if (chunkZ != 0)
			{
				z = Z % Chunk.CHUNK_SIZE;
				if (z < 0) z += Chunk.CHUNK_SIZE;
			}*/
		}

		//Save the world
		public override void End()
		{
			SaveChunks();
			chunks.Clear();

			base.End();

			GC.Collect();

#if CLIENT
			//Done saving, hide
			Client.UI.UiRegisterer.loadingUI.visible = false;
#endif
		}

		#region Serialization
		public void SaveChunks()
		{
			//TODO: Serialize

			//ROADMAP:
			//
			// Get world folder
			// Construct Level NBT Data
			// Write File to folder
			// create chunks dir if non-existent
			// for (chunk in chunks)
			// {
			//		NBT = chunk.Serialize();
			//		writeToFile(NBT);
			// }

			#region World Data
			var root = new BinaryTagNodeMultiple("root");

			//Serialize player position
			#region Player State
			var plr = new BinaryTagNodeMultiple("player");
			var plrPos = new BinaryTagNodeMultiple("position");
			var plrRot = new BinaryTagNodeMultiple("rotation");

			plrPos.Add(new BinaryTagNodeFloat("X", player.position.X));
			plrPos.Add(new BinaryTagNodeFloat("Y", player.position.Y));
			plrPos.Add(new BinaryTagNodeFloat("Z", player.position.Z));

			plrRot.Add(new BinaryTagNodeFloat("X", player.camera.Pitch));
			plrRot.Add(new BinaryTagNodeFloat("Y", player.camera.Yaw));
			plrRot.Add(new BinaryTagNodeFloat("Z", player.camera.Roll));

			plr.Add(plrPos);
			plr.Add(plrRot);

			plr.Add(new BinaryTagNodeByte("inventorySlot", (byte)player.HeldSlot));

			//TODO: Serialize inventory

			root.Add(plr);

			#endregion

			root.Add(new BinaryTagNodeLong("seed", seed));

			root.Add(new BinaryTagNode(""));
			#endregion

			FileUtil.SerializeToFile(Path.Combine(WorldDirectory, "level.dat"), root);

			string chunkFolder = Path.Combine(WorldDirectory, "chunks");

			#region Chunks
			for (int i = 0; i < chunks.Count; i++)
			{
				var data = chunks[chunkHashes[i]].SerializeToBinaryTag();

				FileUtil.SerializeToFile(Path.Combine(chunkFolder, chunks[chunkHashes[i]].posX + "." + chunks[chunkHashes[i]].posY + "." + chunks[chunkHashes[i]].posZ + "p.cnk"), data);
			}
			#endregion
		}

		public void LoadChunks()
		{
			//TODO: Deserialize

			//ROADMAP:
			//
			// Get chunks folder
			// Get level data
			// Get player position
			// Apply level data
			// Get chunk at player pos
			// Read
			// Update => if(chunkManager.ChunkCount < ChunkThreshold) { return; }

			#region World Data
			BinaryTagNode levelData = FileUtil.DeseralizeFromFile(Path.Combine(WorldDirectory, "level.dat"));

			if (levelData.Type == TagType.TagEnd)
			{
				Logger.error(-1, "The file \"" + Path.Combine(WorldDirectory, "level.dat") + "\" doesn't exist, or is malformed!");
			}
			if (levelData.Type == TagType.TagMultiple)
			{
				try
				{
					BinaryTagNodeMultiple levelRoot = (BinaryTagNodeMultiple)levelData;

					#region Player state
					BinaryTagNodeMultiple playerTag = (BinaryTagNodeMultiple)levelRoot.Get("player");

					BinaryTagNodeMultiple playerPosTag = (BinaryTagNodeMultiple)playerTag.Get("position");
					BinaryTagNodeMultiple playerRotTag = (BinaryTagNodeMultiple)playerTag.Get("rotation");

					player.position.X = ((BinaryTagNodeFloat)playerPosTag.Get("X")).Value;
					player.position.Y = ((BinaryTagNodeFloat)playerPosTag.Get("Y")).Value;
					player.position.Z = ((BinaryTagNodeFloat)playerPosTag.Get("Z")).Value;

					player.camera.Pitch = ((BinaryTagNodeFloat)playerRotTag.Get("X")).Value;
					player.camera.Yaw = ((BinaryTagNodeFloat)playerRotTag.Get("Y")).Value;
					player.camera.Roll = ((BinaryTagNodeFloat)playerRotTag.Get("Z")).Value;

					player.HeldSlot = ((BinaryTagNodeByte)playerTag.Get("inventorySlot")).Value;

					//TODO: Deserialize inventory
					#endregion

					seed = ((BinaryTagNodeLong)levelRoot.Get("seed")).Value;
				}
				catch (Exception ex)
				{
					Logger.error(-1, "MalformedBinaryTagNodeException:\n" + ex.StackTrace);
				}
			}
			#endregion

			#region Chunks

			string chunkFolder = Path.Combine(WorldDirectory, "chunks");

			if (Directory.Exists(chunkFolder))
			{
				string[] fileEntries = Directory.GetFiles(chunkFolder, "*.cnk");

				for (int i = 0; i < fileEntries.Length; i++)
				{
					//TODO: loop through chunks folder
					BinaryTagNode chunkData = FileUtil.DeseralizeFromFile(fileEntries[i]);

					if (chunkData.Type == TagType.TagEnd)
					{
						Logger.error(-1, "The file \"" + Path.Combine(chunkFolder, "level.dat") + "\" doesn't exist, or is malformed!");
					}
					if (chunkData.Type == TagType.TagMultiple)
					{
						try
						{
							var chunkRoot = (BinaryTagNodeMultiple)chunkData;

							var chunkX = ((BinaryTagNodeInt)chunkRoot.Get("X"));
							var chunkY = ((BinaryTagNodeInt)chunkRoot.Get("Y"));
							var chunkZ = ((BinaryTagNodeInt)chunkRoot.Get("Z"));

							if (chunkX != null && chunkY != null && chunkZ != null)
							{
								var hash = Chunk.GetHash(chunkX.Value, chunkY.Value, chunkZ.Value);

								if (chunks.ContainsKey(hash))
								{
									chunks[hash].PopulateFromBinaryTag(chunkRoot);
								}
							}
						}
						catch (Exception ex)
						{
							Logger.error(-1, "MalformedBinaryTagNodeException:\n" + ex.StackTrace);
						}
					}
				}
			}
			#endregion
		}
		#endregion
	}
}
