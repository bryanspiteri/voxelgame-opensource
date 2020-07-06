using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Client.UI
{
	public class HotbarScreen : GuiScreen
	{
		public int CrossHairThickness = 1;
		public int CrossHairWidth = 12;

		UiImage CrosshairY, CrosshairX;

		public UiImage Hotbar;
		public UiImage Selected;
		public ItemImage[] items = new ItemImage[9];
		public UiText HoverText;

		//TODO: Move crosshair to here
		public HotbarScreen()
		{
			//Init the hotbar
			Hotbar = new UiImage(this, new Point(0, 0), new Point(5, 5), "textures/ui/hotbar_gui");
			Hotbar.size = new Point(Hotbar.image.Width, Hotbar.image.Height);
			Hotbar.rawSize = Hotbar.size;
			Hotbar.UV = new Vector4(0, 0, 0.3515625f, 0.0390625f);
			AddComponent(Hotbar);

			//init the items
			for (int i = 0; i < 9; i++)
			{
				items[i] = new ItemImage(this, 0, 0, 64, 64);
				items[i].rawSize = new Point(16, 16);
				AddComponent(items[i]);
			}

			//init the selection dot
			Selected = new UiImage(this, new Point(0, 0), new Point(5, 5), "textures/ui/hotbar_gui");
			Selected.size = new Point(Selected.image.Width, Selected.image.Height);
			Selected.rawSize = Selected.size;
			Selected.UV = new Vector4(0, 0.0625f, 0.04296875f, 0.10546875f);
			AddComponent(Selected);

			//init the crosshair
			CrosshairX = new UiImage(this, Point.Zero, new Point(CrossHairWidth, CrossHairThickness));
			CrosshairY = new UiImage(this, Point.Zero, new Point(CrossHairThickness, CrossHairWidth));
			AddComponent(CrosshairX);
			AddComponent(CrosshairY);

			HoverText = new UiText(this, Point.Zero, "");
			AddComponent(HoverText);
		}

		public override void Update()
		{
			if (GameClient.IsGameWorldloaded)
			{
				//resize the crosshair
				CrosshairX.size = new Point(CrossHairWidth * GameSettings.UIScale, CrossHairThickness * GameSettings.UIScale);
				CrosshairY.size = new Point(CrossHairThickness * GameSettings.UIScale, CrossHairWidth * GameSettings.UIScale);
				//center the crosshair
				CrosshairX.location = new Point(GameClient.ViewWidth / 2 - CrosshairX.size.X / 2, GameClient.ViewHeight / 2 - CrosshairX.size.Y / 2);
				CrosshairY.location = new Point(GameClient.ViewWidth / 2 - CrosshairY.size.X / 2, GameClient.ViewHeight / 2 - CrosshairY.size.Y / 2);

				//Scale the hotbar
				Hotbar.size = new Point(Hotbar.rawSize.X * GameSettings.UIScale, Hotbar.rawSize.Y * GameSettings.UIScale);
				//Center the hotbar at the bottom
				Hotbar.location = new Point(GameClient.ViewWidth / 2 - Hotbar.size.X / 2, GameClient.ViewHeight - Hotbar.size.Y);

				//Scale the selection
				Selected.size = new Point(Selected.rawSize.X * GameSettings.UIScale, Selected.rawSize.Y * GameSettings.UIScale);
				//Align the selection
				Selected.location = Hotbar.location + new Point(-GameSettings.UIScale + GameClient.theWorld.player.HeldSlot * 20 * GameSettings.UIScale, -GameSettings.UIScale);

				for (int i = 0; i < 9; i++)
				{
					//Align the items
					items[i].size = new Point(items[i].rawSize.X * GameSettings.UIScale, items[i].rawSize.Y * GameSettings.UIScale);
					items[i].location = new Point(Hotbar.location.X + 2 * GameSettings.UIScale + i * 20 * GameSettings.UIScale, GameClient.ViewHeight - Hotbar.size.Y + 2 * GameSettings.UIScale);

					//Sync the items with the player inventory
					items[i].item = GameClient.theWorld.player.inventory.items[i];
				}

				//Selected item UI
				HoverText.text = LanguageHelper.GetLanguageString("block." + ItemManager.GetItem(GameClient.theWorld.player.inventory.items[GameClient.theWorld.player.HeldSlot].ItemID).Title);
				HoverText.location = new Point(GameClient.ViewWidth / 2 - HoverText.size.X / 2, Selected.location.Y - 10 * GameSettings.UIScale - HoverText.size.Y);
				//TODO: Fade

				//Update text
			}
			visible = GameClient.IsGameWorldloaded;
			base.Update();
		}
	}
}
