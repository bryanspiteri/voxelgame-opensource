using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;

namespace VoxelGame.Client.Renderers
{
	public class GraphicsManager
	{
		public static BasicEffect effect = new BasicEffect(VoxelClient.Graphics.GraphicsDevice);

		public static SpriteBatch batcher
		{
			get
			{
				return UiStateManager.SpriteBatch;
			}
		}
	}
}
