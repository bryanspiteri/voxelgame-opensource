using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Items;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	public class ItemImage : Component
	{
		private ItemStack _stack;
		private Texture2D image;

		public ItemStack item
		{
			get
			{
				return _stack;
			}
			set
			{
				//Caching image
				if (_stack.ItemID != value.ItemID)
				{
					_stack.ItemCount = value.ItemCount;
					_stack.ItemID = value.ItemID;

					//Regen image
					image = ItemManager.GetItem(value.ItemID).InventoryGraphic;
					if (image == null)
					{
						image = TextureLoader.getErrorTexture();
					}
				}
				else
				{
					_stack.ItemCount = value.ItemCount;
				}
			}
		}

		public bool enabled = true;

		public override void getTextures()
		{
			//get the asset from ui atlas
		}

		#region Constructor
		public ItemImage(GuiScreen parent, int x, int y, int sX, int sY) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			item = new ItemStack(0, 1);
		}

		public ItemImage(GuiScreen parent, Point location, Point size) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			item = new ItemStack(0, 1);
		}
		#endregion

		public override void onUpdate()
		{

		}

		public override void Draw()
		{
			if (_stack.ItemID != 0)
			{
				//Draw the graphic
				if (size != Point.Zero)
				{
					UiStateManager.DrawImage(new Rectangle(location.X, location.Y, size.X, size.Y), image, Color.White);
				}
				else
				{
					UiStateManager.DrawImage(location, image, Color.White);
				}

				//Overlay the count
				if (_stack.ItemCount > 1)
				{
					Vector2 textSize = UiStateManager.gameFont.MeasureString(_stack.ItemCount.ToString());
					UiStateManager.DrawText(new Vector2(location.X + size.X - textSize.X, location.Y + size.Y - textSize.Y), _stack.ItemCount.ToString());
				}
			}
		}
	}
}
