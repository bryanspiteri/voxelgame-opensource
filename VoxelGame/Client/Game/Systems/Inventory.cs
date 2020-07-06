using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Items;

namespace VoxelGame
{
	/// <summary>
	/// A simple inventory system. Inventory is basically an ItemStack array
	/// </summary>
	public class Inventory
	{
		public ItemStack[] items;

		public Inventory(int slotCount)
		{
			items = new ItemStack[slotCount];
		}

		public ItemStack GetItem(int slot)
		{
			if (items != null && slot >= 0 && slot < items.Length)
			{
				return items[slot];
			}
			return items[0];
		}

		public void SetItem(ItemStack item, int slot)
		{
			if (items != null && slot >= 0 && slot < items.Length)
			{
				items[slot] = item;
			}
		}
	}
}
