using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Crash;
using VoxelGame.Items;

namespace VoxelGame.Util
{
	public class ItemManager
	{
		public static ItemManager ItemRegistry { get { return GameClient.Instance.itemRegistry; } }

		public Dictionary<uint, Item> registeredItems = new Dictionary<uint, Item>();
		public Dictionary<string, uint> idRegistry = new Dictionary<string, uint>();

		public static void RegisterItems()
		{
			//Blocks are already registered at this point
		}

		/// <summary>
		/// Registers the given block into the block pool
		/// </summary>
		/// <param name="itemToRegister"></param>
		public static void RegisterItem(Item itemToRegister)
		{
			if (ItemRegistry.idRegistry.ContainsValue(itemToRegister.Id)
				|| ItemRegistry.registeredItems.ContainsKey(itemToRegister.Id)
				|| ItemRegistry.idRegistry.ContainsKey(itemToRegister.Title))
			{
				Logger.fatal(0, "The item \"" + itemToRegister.Title + "\" (" + itemToRegister.Id + ") has already been registed! (ERR_ID_CONFLICT)");
				Engine.VoxelClient.RaiseCrash(CrashError.ID_CONFLICT);
			}
			else
			{
				//TODO: Remove / comment with model api
				itemToRegister.CacheTexture();
				//Add to the id registry so that we can use string lookups
				ItemRegistry.idRegistry.Add(itemToRegister.Title, itemToRegister.Id);
				//Actually register the block with it's id
				ItemRegistry.registeredItems.Add(itemToRegister.Id, itemToRegister);
			}
		}

		public static uint GetItemId(string id)
		{
			if (ItemRegistry.idRegistry.ContainsKey(id))
			{
				return ItemRegistry.idRegistry[id];
			}
			return 0;
		}

		public static Item GetItem(uint id)
		{
			//if the block is registered
			if (ItemRegistry.registeredItems.ContainsKey(id))
			{
				return ItemRegistry.registeredItems[id];
			}
			//return an error defining the undefined block
			Logger.error(0, "Unknown block (ID: " + id + ")! Defaulting to Air (0).");
			//default to air
			return ItemRegistry.registeredItems[0];
		}

		public static Item GetItemFromName(string name)
		{
			return GetItem(GetItemId(name));
		}

		/// <summary>
		/// Rebuilds the texture cache
		/// </summary>
		public static void RecacheTextures()
		{
			for (uint i = 0; i < ItemRegistry.registeredItems.Count; i++)
			{
				ItemRegistry.registeredItems[i].CacheTexture();
			}
		}
	}
}
