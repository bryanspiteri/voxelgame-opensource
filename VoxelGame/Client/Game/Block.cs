using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util.Math;
using VoxelGame.Util;
using VoxelGame.Client.Renderers;
#if CLIENT
using VoxelGame.Client;
#endif

namespace VoxelGame.Blocks
{
	public class Block
	{
#if CLIENT
		public BlockModel BlockModel;
#endif
		public string Title;
		public uint Id = 0;
		public bool Tickable = false;
		public bool HasItem = true;

		/// <summary>
		/// How prone the block is to explosive damage
		/// </summary>
		public float BlastResistance = 5f;
		/// <summary>
		/// How long it takes to mine this block with a tool / hand
		/// </summary>
		public float BlockHardness = 5f;
		/// <summary>
		/// Whether the block can catch fire
		/// </summary>
		public bool Flammable = false;
		/// <summary>
		/// Whether interacting with this block calls the use method or not
		/// </summary>
		public bool Interactable = false;

		public virtual BoundingBox GetBoundingBox(Vector3Int position)
		{
			return IsSolid ? Collider.Offset(position) : new BoundingBox();
		}

		//TODO: Model definition

		/// <summary>
		/// The default bounding box
		/// </summary>
		public static readonly BoundingBox FullCube = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

		public virtual BoundingBox Collider
		{
			get
			{
				return FullCube;
			}
		}

		public Block(string title, string modelLocation)
		{
			this.Title = title;
			//id = title;
			BlockModel = ModelLoader.loadBlockModel(modelLocation);
		}

		public bool IsSolid = true;
		public bool IsTransparent = false;

		/// <summary>
		/// Whether the block can be selected by the block picker
		/// </summary>
		public bool Selectable = true;

		public virtual void OnPlace(Vector3Int blockPos, BlockPlaceReason reason)
		{
			if (GameClient.IsGameWorldloaded && GameClient.theWorld != null)
			{

			}
		}

		public virtual void OnBreak(Vector3Int blockPos, BlockBreakReason reason)
		{
			if (GameClient.IsGameWorldloaded && GameClient.theWorld != null)
			{
				//Spawn particles
				//TODO: Particles
				ParticleEngine.SpawnParticles(blockPos.ToVector3(), Title, 5f, Particle.Behavior.BlockParticle, 15);
			}
		}

		public virtual void OnTick(Vector3Int blockPos)
		{

		}

		public virtual void OnBlockUpdate(Vector3Int blockPos)
		{
			if (Tickable)
			{
				GameClient.theWorld.QueueBlockUpdate(blockPos);
			}
		}

		public virtual void OnInteract(Vector3Int blockPos, Entity interactor)
		{
			if (GameClient.IsGameWorldloaded && GameClient.theWorld != null)
			{

			}
		}

		public virtual bool CanTickAt(int x, int y, int z)
		{
			return false;
		}

		public bool CanTickAt(Vector3Int coord)
		{
			return CanTickAt(coord.X, coord.Y, coord.Z);
		}

		public void BlockUpdateNeighbours(Vector3Int position)
		{
			//Send a block update to the neighbouring blocks
			TryUpdate(position + Side.NORTH.GetOffset());
			TryUpdate(position + Side.SOUTH.GetOffset());
			TryUpdate(position + Side.EAST.GetOffset());
			TryUpdate(position + Side.WEST.GetOffset());
			TryUpdate(position + Side.UP.GetOffset());
			TryUpdate(position + Side.DOWN.GetOffset());

			/*GameClient.theWorld.GetBlockAt(position + Side.NORTH.GetOffset()).OnBlockUpdate(position + Side.NORTH.GetOffset());
			GameClient.theWorld.GetBlockAt(position + Side.SOUTH.GetOffset()).OnBlockUpdate(position + Side.SOUTH.GetOffset());
			GameClient.theWorld.GetBlockAt(position + Side.EAST.GetOffset()).OnBlockUpdate(position + Side.EAST.GetOffset());
			GameClient.theWorld.GetBlockAt(position + Side.WEST.GetOffset()).OnBlockUpdate(position + Side.WEST.GetOffset());
			GameClient.theWorld.GetBlockAt(position + Side.UP.GetOffset()).OnBlockUpdate(position + Side.UP.GetOffset());
			GameClient.theWorld.GetBlockAt(position + Side.DOWN.GetOffset()).OnBlockUpdate(position + Side.DOWN.GetOffset());*/
		}

		private void TryUpdate(Vector3Int coord)
		{
			if (GameClient.IsGameWorldloaded)
			{
				Block theBlock = GameClient.theWorld.GetBlockAt(coord);
				
				if (theBlock.Tickable && theBlock.CanTickAt(coord))
				{
					theBlock.OnBlockUpdate(coord);
				}
			}
		}
	}

	//Enums
	public enum BlockBreakReason : byte
	{
		GAME_INTERNAL = 0,
		COMMAND = 1,
		PLAYER_SURVIVAL = 2,
		PLAYER_CREATIVE = 3,
		ENTITY = 4,
		EXPLOSION = 5,
		BLOCK_UPDATE = 6,
	}

	public enum BlockPlaceReason : byte
	{
		GAME_INTERNAL = 0,
		COMMAND = 1,
		PLAYER = 2 | 3,
		ENTITY = 4,
		EXPLOSION = 5,
		BLOCK_UPDATE = 6,
	}
}
