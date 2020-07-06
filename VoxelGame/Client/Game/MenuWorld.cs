using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;

namespace VoxelGame.Client
{
	public class MenuWorld : BlockWorld
	{
		public MenuWorld()
		{
			GameClient.DiscordManager.SetState("Making lots of menus", "Title Screen");
		}

		public override void Draw(GameTime gameTime)
		{
			VoxelClient.Instance.GraphicsDevice.Clear(Color.DeepPink);
		}
	}
}
