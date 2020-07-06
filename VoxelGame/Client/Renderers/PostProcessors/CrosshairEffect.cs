using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;

namespace VoxelGame.Client.Renderers
{
	public class CrosshairEffect : PostProcessor
	{
		public int CrossHairThickness = 2;
		public int CrossHairWidth = 24;

		public BlendState InvertBlendState = new BlendState()
		{
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.SourceColor,
			ColorBlendFunction = BlendFunction.Subtract,
		};

		public override void Draw(Texture2D screen)
		{
			int MinX = screen.Width / 2 - CrossHairWidth / 2;
			int MaxX = MinX + CrossHairWidth;

			int MinY = screen.Height / 2 - CrossHairWidth / 2;
			int MaxY = MinY + CrossHairWidth;

			int HeightMinY = screen.Height / 2 - CrossHairThickness / 2;
			int HeightMaxY = HeightMinY + CrossHairThickness;

			int WidthMinY = screen.Width / 2 - CrossHairThickness / 2;
			int WidthMaxY = WidthMinY + CrossHairThickness;

			base.Draw(screen);

			Texture2D pixel = TextureLoader.getWhitePixel();

			//spriteBatch.Begin(SpriteSortMode.Deferred, InvertBlendState);
			UiStateManager.Begin();
			UiStateManager.DrawImage(new Rectangle(new Point(MaxX, HeightMaxY), new Point(MinX - MaxX, HeightMinY - HeightMaxY)), pixel, Color.White);
			UiStateManager.DrawImage(new Rectangle(new Point(WidthMaxY, MaxY), new Point(WidthMinY - WidthMaxY, MinY - MaxY)), pixel, Color.White);
			UiStateManager.End();
		}
	}
}
