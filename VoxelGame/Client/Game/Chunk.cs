using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;
using VoxelGame.Client;
using VoxelGame.Client.Renderers;
using VoxelGame.Util;
using VoxelGame.Util.Storage;

namespace VoxelGame.World
{
	public class Chunk
	{
		//Constants
		public const int CHUNK_SIZE = 16;
		public static int Size { get { return CHUNK_SIZE; } }

		//Sturctures
		public uint[,,] chunk_blockdata = new uint[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE]; //block ids
		public byte[,,] chunk_blockstates = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE]; //block states. used for orientations
		public byte[,,] chunk_blockLighting = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE]; //block light from light sources
		public byte[,,] chunk_skyLighting = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE]; //light influence from sun (used as a percentage)
		public SortedList<Vector3Int, BinaryTagNode> chunk_metadata = new SortedList<Vector3Int, BinaryTagNode>(); //block metadata. dictionary because less data = -RAM = -DISK
		public SortedList<int, Vector3Int> chunk_updates = new SortedList<int, Vector3Int>(); //List of blocks to update

		//global chunk data (per chunk)
		public int posX, posY, posZ = 0; //HACK: Change to long?
		bool _updateNeeded = false;
		public bool UpdateNeeded
		{
			get
			{
				return _updateNeeded;
			}
			set
			{
				_updateNeeded = value;
#if CLIENT
				if (ChunkMesh != null)
				{
					ChunkMesh.isGenerated = false;
				}
#endif
			}
		}

		public bool isLoaded
		{
			get
			{
				return (chunk_blockdata != null || chunk_blockdata.Length != 0)
					&& (chunk_blockLighting != null || chunk_blockLighting.Length != 0)
					&& (chunk_skyLighting != null || chunk_skyLighting.Length != 0);
			}
		}

		public Chunk(int posX, int posY, int posZ)
		{
			this.posX = posX;
			this.posY = posY;
			this.posZ = posZ;

			//initialise it to air
			for (int x = 0; x < CHUNK_SIZE; x++)
			{
				for (int y = 0; y < CHUNK_SIZE; y++)
				{
					for (int z = 0; z < CHUNK_SIZE; z++)
					{
						chunk_blockdata[x, y, z] = 0; //Set to air (id: 0)
					}
				}
			}
		}

		public void Dispose()
		{
			//SAVE CHUNK
			for (int z = 0; z < CHUNK_SIZE; z++)
			{
				for (int y = 0; y < CHUNK_SIZE; y++)
				{
					for (int x = 0; x < CHUNK_SIZE; x++)
					{
						chunk_blockdata[x, y, z] = 0x0000000000000000;
					}
				}
			}
		}

		public void SetBlock(uint id, int x, int y, int z, uint reason)
		{
			if (x >= 0 && x < 16)
			{
				if (y >= 0 && y < 16)
				{
					if (z >= 0 && z < 16)
					{
						//Revert block position to world position
						Vector3Int blockPosition = new Vector3Int(posX * CHUNK_SIZE + x, posY * CHUNK_SIZE + y, posZ * CHUNK_SIZE + z);

						if (chunk_updates.ContainsKey(GetHash(x, y, z)))
						{
							chunk_updates.Remove(GetHash(x, y, z));
						}

						BlockManager.GetBlock(chunk_blockdata[x, y, z]).OnBreak(blockPosition, (BlockBreakReason)reason);
						chunk_blockdata[x, y, z] = id;
						BlockManager.GetBlock(id).OnPlace(blockPosition, (BlockPlaceReason)reason);
						//Queue the update
						if (reason != (uint)BlockBreakReason.GAME_INTERNAL)
						{
							Block theBlock = BlockManager.GetBlock(chunk_blockdata[x, y, z]);
							if (theBlock.Tickable && theBlock.CanTickAt(blockPosition))
							{
								chunk_updates.Add(GetHash(x, y, z), new Vector3Int(x, y, z));
							}
							BlockManager.GetBlock(chunk_blockdata[x, y, z]).BlockUpdateNeighbours(blockPosition);
						}
						UpdateNeeded = true;
					}
				}
			}
		}

		public void SetBlock(ushort id, byte x, byte y, byte z, BlockPlaceReason reason)
		{
			SetBlock(id, x, y, z, (byte)reason);
		}

		public void SetBlock(ushort id, byte x, byte y, byte z, BlockBreakReason reason)
		{
			SetBlock(id, x, y, z, (byte)reason);
		}

		public uint GetBlockAt(int x, int y, int z)
		{
			if (x >= 0 && x < 16)
			{
				if (y >= 0 && y < 16)
				{
					if (z >= 0 && z < 16)
					{
						return chunk_blockdata[x, y, z];
					}
				}
			}
			return 0;
		}

		public void OnTick()
		{
			//Profiling	
			GameClient.theWorld.BlockUpdates += chunk_updates.Count;

			if (chunk_updates.Count != 0)
			{
				GameClient.theWorld.ChunkUpdates++;
			}

			for (int i = 0; i < chunk_updates.Count; i++)
			{
				if (chunk_updates.Values[i].X >= 0 && chunk_updates.Values[i].X < 16)
				{
					if (chunk_updates.Values[i].Y >= 0 && chunk_updates.Values[i].Y < 16)
					{
						if (chunk_updates.Values[i].Z >= 0 && chunk_updates.Values[i].Z < 16)
						{
							Vector3Int coord = chunk_updates.Values[i];
							Block theBlock = BlockManager.GetBlock(chunk_blockdata[coord.X, coord.Y, coord.Z]);

							//Remove the block update before telling the block to update
							chunk_updates.Remove(GetHash(coord.X, coord.Y, coord.Z));
							theBlock.OnTick(new Vector3Int(coord.X + CHUNK_SIZE * posX, coord.Y + CHUNK_SIZE * posY, coord.Z + CHUNK_SIZE * posZ));
						}
					}
				}
			}

		}

		/// <summary>
		/// Recalculates the lighting table
		/// </summary>
		public void RecalculateLightLevels()
		{
			//TODO: Thread it
		}

		public byte GetBrightness(int x, int y, int z)
		{
			if (x >= 0 && x < 16)
			{
				if (y >= 0 && y < 16)
				{
					if (z >= 0 && z < 16)
					{
						return (byte)MathHelper.Clamp(
							chunk_blockLighting[x, y, z]
							+ MathHelper.Clamp(chunk_skyLighting[x, y, z] / 15 * GameClient.theWorld.dayBrightness,
							0, 15) //sky lighting
							,
							0, 15);
					}
				}
			}
			return 0;
		}

		public override string ToString()
		{
			return "Chunk at X: " + posX + " Y: " + posY + " Z: " + posZ;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				//Same as Vector3Int.GetHashCode()
				return GetHash(posX, posY, posZ);
			}
		}

		public static int GetHash(int X, int Y, int Z)
		{
			int result = X;
			result = 31 * result + Y;
			result = 31 * result + Z;
			return result;
		}

#if CLIENT
		//Client Rendering Code

		public bool IsVisible = true;
		public ChunkMesh ChunkMesh;

		//Initialise the mesh if it is null
		public void InitialiseMesh()
		{
			ChunkMesh = new ChunkMesh(posX, posY, posZ);
		}
#endif

		public void Update()
		{
			//TODO: Neighbouring (Infinite Terrain)

#if CLIENT
			if (ChunkMesh != null)
			{
				//Handle the mesh updating
				ChunkMesh.Update();
				if (UpdateNeeded)
				{
					UpdateNeeded = false;
				}
			}
#endif
		}

		/// <summary>
		/// Returns the bounding box in chunk coordinates
		/// </summary>
		/// <returns></returns>
		public BoundingBox GetBoundingBoxChunk()
		{
			return new BoundingBox(new Vector3(posX, posY, posZ), new Vector3(posX + 1, posY + 1, posZ + 1));
		}

		/// <summary>
		/// Returns the bounding box in block coordinates
		/// </summary>
		/// <returns></returns>
		public BoundingBox GetBoundingBoxBlock()
		{
			return new BoundingBox(new Vector3(posX * CHUNK_SIZE, posY * CHUNK_SIZE, posZ * CHUNK_SIZE), new Vector3(posX * CHUNK_SIZE + CHUNK_SIZE, posY * CHUNK_SIZE + CHUNK_SIZE, posZ * CHUNK_SIZE + CHUNK_SIZE));
		}

		#region Serialization

		public BinaryTagNodeMultiple SerializeToBinaryTag()
		{
			BinaryTagNodeMultiple rootTag = new BinaryTagNodeMultiple("root");

			//Position. Most important parts
			rootTag.Add(new BinaryTagNodeInt("X", posX));
			rootTag.Add(new BinaryTagNodeInt("Y", posY));
			rootTag.Add(new BinaryTagNodeInt("Z", posZ));

			//Chunk versioning. Unused for now, to be used by the file upgrader API when written.
			rootTag.Add(new BinaryTagNodeUShort("version", 0));

			#region Block Compression
			byte blockCompressionID = 1;

			//TODO: Compress with all algorithms, determine smallest and write smallest to file

			#region No Compression
			//Convert the block array to a flat linear array
			var blockFlatArr = new List<BinaryTagNode>();

			for (int z = 0; z < CHUNK_SIZE; z++)
			{
				for (int y = 0; y < CHUNK_SIZE; y++)
				{
					for (int x = 0; x < CHUNK_SIZE; x++)
					{
						int pos = (z * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + x;
						blockFlatArr.Add(new BinaryTagNodeUShort(pos.ToString(), (ushort)chunk_blockdata[x, y, z]));
					}
				}
			}
			#endregion

			#region Compression Variant 1
			var blockCompressedArr = new List<BinaryTagNode>();

			uint size = 56;
			uint currentBlockID = 0;
			int blockLength = 0;

			for (int z = 0; z < CHUNK_SIZE; z++)
			{
				for (int y = 0; y < CHUNK_SIZE; y++)
				{
					for (int x = 0; x < CHUNK_SIZE; x++)
					{
						//get the voxel and check if its the current block
						if (chunk_blockdata[x, y, z] == currentBlockID)
						{
							blockLength++;
						}
						else
						{
							//Add data to the binarytag
							var currentNode = GetCompressedBinaryTagBlock(currentBlockID, Math.Max(0, blockLength));
							blockCompressedArr.Add(currentNode);

							size += 56; //Assuming each part is 16 + 8 bits (BlockID) + 16 + 8 bits (Length) + 8 bits (TagMultiple)

							//Reset
							currentBlockID = chunk_blockdata[x, y, z];
							blockLength = 1;
						}
					}
				}
			}

			blockCompressedArr.Add(GetCompressedBinaryTagBlock(currentBlockID, Math.Max(0, blockLength)));

			#endregion

			if (size >= 98304) //(16 + 8) bits * 4096 entries (no compression)
			{
				blockCompressionID = 0;
			}

			rootTag.Add(new BinaryTagNodeByte("blockCompression", blockCompressionID));

			if (blockCompressionID == 0)
			{
				rootTag.Add(new BinaryTagNodeList("blocks", blockFlatArr));
			}
			else if (blockCompressionID == 1)
			{
				rootTag.Add(new BinaryTagNodeList("blocks", blockCompressedArr));
			}
			#endregion

			#region Block states

			//TODO: Compress blockstates

			#endregion

			#region Lighting

			//TODO: Compress sky light

			//TODO: Compress block light

			#endregion

			#region Metadata

			//TODO: Compress blockmeta

			#endregion

			#region Tickables

			//No updates means we don't bother adding it, since its the default state
			if (chunk_updates.Count > 0)
			{
				#region No Compression
				//Convert the block array to a flat linear array
				var tickFlatArr = new List<BinaryTagNode>();

				for (int i = 0; i < chunk_updates.Count; i++)
				{
					int pos = (chunk_updates.Values[i].Z * CHUNK_SIZE * CHUNK_SIZE) + (chunk_updates.Values[i].Y * CHUNK_SIZE) + chunk_updates.Values[i].X;
					tickFlatArr.Add(new BinaryTagNodeInt("block_update", pos));
				}
				#endregion

				rootTag.Add(new BinaryTagNodeList("updates", tickFlatArr));
			}

			#endregion

			return rootTag;
		}

		public void PopulateFromBinaryTag(BinaryTagNodeMultiple tag)
		{
			#region Block Loading
			var blocksNode = (BinaryTagNodeList)tag.Get("blocks");
			var blockCompressionType = (BinaryTagNodeByte)tag.Get("blockCompression");

			if (blocksNode != null && blockCompressionType != null)
			{
				//No compression
				if (blockCompressionType.Value == 0)
				{
					for (int i = 0; i < blocksNode.Count; i++)
					{
						var node = (BinaryTagNodeUShort)blocksNode[i];

						if (node != null)
						{
							var nodeName = node.Name;
							int pos = int.Parse(nodeName);

							int x, y, z;
							z = pos / (CHUNK_SIZE * CHUNK_SIZE);
							pos -= (z * CHUNK_SIZE * CHUNK_SIZE);
							y = pos / CHUNK_SIZE;
							x = pos % CHUNK_SIZE;

							chunk_blockdata[x, y, z] = node.Value;
						}
					}
				}
				//Compression Section 
				else if(blockCompressionType.Value == 1)
				{
					//used for traversing the linear array
					int linearPosition = -1;

					for (int i = 0; i < blocksNode.Count; i++)
					{
						var nodeRoot = (BinaryTagNodeMultiple)blocksNode[i];

						if (nodeRoot != null)
						{
							var blockID = (BinaryTagNodeUShort)nodeRoot.Get("block");
							var length = (BinaryTagNodeUShort)nodeRoot.Get("length");

							//null check
							if (blockID != null && length != null)
							{
								for (int j = 0; j < length.Value; j++)
								{
									linearPosition++;

									//Convert from linear to 3d
									var posTmp = linearPosition;

									int x, y, z;
									z = posTmp / (CHUNK_SIZE * CHUNK_SIZE);
									posTmp -= (z * CHUNK_SIZE * CHUNK_SIZE);
									y = posTmp / CHUNK_SIZE;
									x = posTmp % CHUNK_SIZE;

									chunk_blockdata[x, y, z] = blockID.Value;
								}
							}
						}
					}
				}
			}
			#endregion

			#region Block State Loading

			#endregion

			#region Lighting Loading

			#endregion

			#region Metadata Loading

			#endregion

			#region Block Updates
			var blockUpdatesNodes = (BinaryTagNodeList)tag.Get("updates");

			if (blockUpdatesNodes != null)
			{
				for (int i = 0; i < blockUpdatesNodes.Count; i++)
				{
					var theThing = (BinaryTagNodeInt)blockUpdatesNodes[i];
					if (theThing != null && theThing.Name == "block_update")
					{
						//Extract the X, Y, Z values from the int
						int x, y, z;
						z = theThing.Value / (CHUNK_SIZE * CHUNK_SIZE);
						theThing.Value -= (z * CHUNK_SIZE * CHUNK_SIZE);
						y = theThing.Value / CHUNK_SIZE;
						x = theThing.Value % CHUNK_SIZE;

						//Add the block update
						Vector3Int theVector = new Vector3Int(x, y, z);
						chunk_updates.Add(GetHash(x, y, z), theVector);
					}
				}
			}

			#endregion
		}

		#region Helpers

		private BinaryTagNodeMultiple GetCompressedBinaryTagBlock(uint blockID, int len)
		{
			var currentNode = new BinaryTagNodeMultiple("_");

			var blockNode = new BinaryTagNodeUShort("block");
			var lengthNode = new BinaryTagNodeUShort("length"); //Biggest possible value is 4096 so ushort

			lengthNode.Value = (ushort)Math.Max(0, len);
			blockNode.Value = (ushort)blockID;

			currentNode.Add(lengthNode);
			currentNode.Add(blockNode);

			return currentNode;
		}

		#endregion

		#endregion

	}
}
