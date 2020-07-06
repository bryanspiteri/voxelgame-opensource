using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.Blocks;
using VoxelGame.Crash;

namespace VoxelGame.Util
{
	public class BlockManager
	{
		public static BlockManager BlockRegistry { get { return GameClient.Instance.blockRegistry; } }

		public Dictionary<uint, Block> registeredBlocks = new Dictionary<uint, Block>();
		public Dictionary<string, uint> idRegistry = new Dictionary<string, uint>();

		/// <summary>
		/// An air block
		/// </summary>
		public static Block AIR
		{
			get
			{
				return GetBlockFromName("air");
			}
		}

		public static void RegisterBlocks()
		{
			RegisterBlock(new Air());
			RegisterBlock(new StoneBlock());
			RegisterBlock(new DirtBlock());
			RegisterBlock(new GrassBlock());
			RegisterBlock(new TectoniteBlock());
			RegisterBlock(new WoodPlanks());
			RegisterBlock(new WoodLog());
			RegisterBlock(new ExplosiveBlock());
		}

		/// <summary>
		/// Registers the given block into the block pool
		/// </summary>
		/// <param name="blockToRegister"></param>
		public static void RegisterBlock(Block blockToRegister)
		{
			if (BlockRegistry.idRegistry.ContainsValue(blockToRegister.Id)
				|| BlockRegistry.registeredBlocks.ContainsKey(blockToRegister.Id)
				|| BlockRegistry.idRegistry.ContainsKey(blockToRegister.Title))
			{
				Logger.fatal(0, "The block \"" + blockToRegister.Title + "\" (" + blockToRegister.Id + ") has already been registed! (ERR_ID_CONFLICT)");
				Engine.VoxelClient.RaiseCrash(CrashError.ID_CONFLICT);
			}
			else
			{
				//Add to the id registry so that we can use string lookups
				BlockRegistry.idRegistry.Add(blockToRegister.Title, blockToRegister.Id);
				//Actually register the block with it's id
				BlockRegistry.registeredBlocks.Add(blockToRegister.Id, blockToRegister);
				//If the block has an item (3d block item model). Allows for other items to handle placement
				if (blockToRegister.HasItem)
				{
					ItemManager.RegisterItem(new Items.BlockItem(blockToRegister));
				}
			}
		}

		public static uint GetBlockId(string id)
		{
			if (BlockRegistry.idRegistry.ContainsKey(id))
			{
				return BlockRegistry.idRegistry[id];
			}
			return 0;
		}

		public static Block GetBlock(uint id)
		{
			//if the block is registered
			if (BlockRegistry.registeredBlocks.ContainsKey(id))
			{
				return BlockRegistry.registeredBlocks[id];
			}
			//return an error defining the undefined block
			Logger.error(0, "Unknown block (ID: " + id + ")! Defaulting to Air (0).");
			//default to air
			return BlockRegistry.registeredBlocks[0];
		}

		public static Block GetBlockFromName(string name)
		{
			return GetBlock(GetBlockId(name));
		}
	}
}
