using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client.Renderers;

namespace VoxelGame.Client
{
	public class PostProcessor
	{
		internal GraphicsDeviceManager deviceManager
		{
			get
			{
				return GameClient.Graphics;
			}
		}
		internal SpriteBatch spriteBatch
		{
			get
			{
				return GraphicsManager.batcher;
			}
		}

		public PostProcessor()
		{

		}

		public virtual void Draw(Texture2D screen)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(screen, Vector2.Zero, Color.White);
			spriteBatch.End();
		}
	}
}
