using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client.Renderers
{
	public class PostProcessRenderer
	{
		public static void Draw(Texture2D screen, PostProcessor postProcessingEffect)
		{
			postProcessingEffect.Draw(screen);
		}
	}
}
