using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.Items;

namespace VoxelGame.Entities
{
	public class EntityItemStack : Entity
	{
		public ItemStack theStack;

		public EntityItemStack(ItemStack stack, Vector3 position, ulong id) : base(id, position)
		{
			base.Collidable = false;
			base.EntityCollider = BoundingBoxExtensions.FromSize(0.25f, 0.25f, 0.25f);
		}

		public override void Draw()
		{
			//TODO: Draw the item. Currently draws colliders

			base.Draw();
		}
	}
}
