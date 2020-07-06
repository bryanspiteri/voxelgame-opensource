using Launcher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoBMFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	public class UiStateManager
	{
		private static GraphicsDevice _graphicsDevice
		{
			get
			{
				return GameClient.Graphics.GraphicsDevice;
			}
		}

		private static ContentManager _content;
		private static SpriteBatch _spriteBatch;
		public static BMFont gameFont;
		private static Texture2D _pixel;

		public static SpriteBatch SpriteBatch
		{
			get
			{
				return _spriteBatch;
			}
		}

		public static Vector2 MeasureString(string text)
		{
			return gameFont.MeasureString(text);
		}

		static UiStateManager()
		{
			_content = GameClient.Instance.Content;

			_spriteBatch = new SpriteBatch(_graphicsDevice);
			//LoadFont("fonts/time"); // default font
			LoadFont("time");

			_pixel = TextureLoader.getWhitePixel();
		}

		public static void LoadFont(string assetName)
		{
			string fontTextureName = FontData.GetTextureNameForFont(assetName);
			Texture2D fontTexture = TextureLoader.loadTexture(fontTextureName);
			//texturefile
			string fontFilePath = Path.Combine(GameClient.ContentDirectory, assetName) + ".fnt";

			if (File.Exists(fontFilePath))
			{
				using (Stream stream = File.OpenRead(fontFilePath))
				{
					FontData fontDesc = FontData.Load(stream);
					gameFont = new BMFont(fontTexture, fontDesc);
				}
			}

			//_spriteFont = Game1.Instance.Content.Load<SpriteFont>(assetName);
		}

		public static void Begin()
		{
			// save graphics state
			GraphicsState.Push();

			// set blend state
			_graphicsDevice.BlendState = BlendState.AlphaBlend;

			// tell the sprite batch to begin
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
		}

		public static void End()
		{
			_spriteBatch.End();
			GraphicsState.Pop();
		}

		public static void DrawText(float x, float y, string text)
		{
			_spriteBatch.DrawString(gameFont, text, new Vector2(x, y), Color.White);
		}

		public static void DrawText(float x, float y, string text, params object[] args)
		{
			_spriteBatch.DrawString(gameFont, string.Format(text, args), new Vector2(x, y), Color.White);
		}

		public static void DrawText(float x, float y, Color color, string text)
		{
			_spriteBatch.DrawString(gameFont, text, new Vector2(x, y), color);
		}

		public static void DrawText(float x, float y, Color color, string text, params object[] args)
		{
			_spriteBatch.DrawString(gameFont, string.Format(text, args), new Vector2(x, y), color);
		}

		public static void DrawText(Vector2 pos, string text)
		{
			_spriteBatch.DrawString(gameFont, text, pos, Color.White);
		}

		public static void DrawText(Vector2 pos, string text, params object[] args)
		{
			_spriteBatch.DrawString(gameFont, string.Format(text, args), pos, Color.White);
		}

		public static void DrawText(Vector2 pos, Color color, string text)
		{
			_spriteBatch.DrawString(gameFont, text, pos, color);
		}

		public static void DrawText(Vector2 pos, Color color, string text, params object[] args)
		{
			_spriteBatch.DrawString(gameFont, string.Format(text, args), pos, color);
		}

		public static void DrawImage(float x, float y, Texture2D image)
		{
			_spriteBatch.Draw(image, new Rectangle((int)x, (int)y, image.Width, image.Height), Color.White);
		}

		public static void DrawImage(float x, float y, Texture2D image, Color color)
		{
			_spriteBatch.Draw(image, new Rectangle((int)x, (int)y, image.Width, image.Height), color);
		}

		public static void DrawImage(Vector2 pos, Texture2D image)
		{
			_spriteBatch.Draw(image, new Rectangle((int)pos.X, (int)pos.Y, image.Width, image.Height), Color.White);
		}

		public static void DrawImage(Vector2 pos, Texture2D image, Color color)
		{
			_spriteBatch.Draw(image, new Rectangle((int)pos.X, (int)pos.Y, image.Width, image.Height), color);
		}

		public static void DrawImage(Point pos, Texture2D image)
		{
			_spriteBatch.Draw(image, new Rectangle(pos.X, pos.Y, image.Width, image.Height), Color.White);
		}

		public static void DrawImage(Point pos, Texture2D image, Color color)
		{
			_spriteBatch.Draw(image, new Rectangle(pos.X, pos.Y, image.Width, image.Height), color);
		}

		public static void DrawImage(Rectangle rect, Texture2D image, Color color)
		{
			_spriteBatch.Draw(image, rect, color);
		}

		public static void DrawImage(Rectangle rect, Texture2D image, Color color, int depth)
		{
			_spriteBatch.Draw(image, rect, null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
		}

		public static void DrawImage(Rectangle rect, Rectangle uv, Texture2D image)
		{
			_spriteBatch.Draw(image, rect, uv, Color.White);
		}

		public static void DrawImage(Rectangle rect, Rectangle uv, Texture2D image, Color color)
		{
			_spriteBatch.Draw(image, rect, uv, color);
		}

		public static void DrawImage(Rectangle rect, Rectangle uv, Texture2D image, Color color, int depth)
		{
			_spriteBatch.Draw(image, rect, uv, color, 0, Vector2.Zero, SpriteEffects.None, depth);
		}

		public static void Draw9SplicedButton(UiTexture buttonTex, Component component)
		{
			Draw9SplicedButton(buttonTex, component.size, component.location);
		}

		public static void Draw9SplicedButton(UiTexture buttonTex, Point size, Point location)
		{
			int sizePaddingX = 3 * GameSettings.UIScale;
			int sizePaddingY = 3 * GameSettings.UIScale;

			int sizeCenterX = size.X - 2 * sizePaddingX;
			int sizeCenterY = size.Y - 2 * sizePaddingY;

			int paddingX = (int)(buttonTex.UV.Width * 0.046875f);
			int paddingY = (int)(buttonTex.UV.Height * 0.1875f);
			int centerX = buttonTex.UV.Width - 2 * paddingX;
			int centerY = buttonTex.UV.Height - 2 * paddingY;

			//TopLeft
			DrawImage(new Rectangle(location.X, location.Y, sizePaddingX, sizePaddingY),
				new Rectangle(buttonTex.UV.X, buttonTex.UV.Y, paddingX, paddingY), buttonTex.texture);

			//TopCenter
			DrawImage(new Rectangle(location.X + sizePaddingX, location.Y, sizeCenterX, sizePaddingY),
				new Rectangle(buttonTex.UV.X + paddingX, buttonTex.UV.Y, centerX, paddingY), buttonTex.texture);

			//TopRight
			DrawImage(new Rectangle(location.X + sizeCenterX + sizePaddingX, location.Y, sizePaddingX, sizePaddingY),
				new Rectangle(buttonTex.UV.X + buttonTex.UV.Width - paddingX, buttonTex.UV.Y, paddingX, paddingY), buttonTex.texture);

			//CenterLeft
			DrawImage(new Rectangle(location.X, location.Y + sizePaddingY, sizePaddingX, sizeCenterY),
				new Rectangle(buttonTex.UV.X, buttonTex.UV.Y + paddingY, paddingX, centerY), buttonTex.texture);

			//CenterCenter
			DrawImage(new Rectangle(location.X + sizePaddingX, location.Y + sizePaddingY, sizeCenterX, sizeCenterY),
				new Rectangle(buttonTex.UV.X + paddingX, buttonTex.UV.Y + paddingY, centerX, centerY), buttonTex.texture);

			//CenterRight
			DrawImage(new Rectangle(location.X + sizeCenterX + sizePaddingX, location.Y + sizePaddingY, sizePaddingX, sizeCenterY),
				new Rectangle(buttonTex.UV.X + buttonTex.UV.Width - paddingX, buttonTex.UV.Y + paddingY, paddingX, centerY), buttonTex.texture);
			//BottomLeft
			DrawImage(new Rectangle(location.X, location.Y + sizePaddingY + sizeCenterY, sizePaddingX, sizePaddingY),
				new Rectangle(buttonTex.UV.X, buttonTex.UV.Y + centerY + paddingY, paddingX, paddingY), buttonTex.texture);

			//BottomCenter
			DrawImage(new Rectangle(location.X + sizePaddingX, location.Y + sizePaddingY + sizeCenterY, sizeCenterX, sizePaddingY),
				new Rectangle(buttonTex.UV.X + paddingX, buttonTex.UV.Y + centerY + paddingY, centerX, paddingY), buttonTex.texture);

			//BottomRight
			DrawImage(new Rectangle(location.X + sizeCenterX + sizePaddingX, location.Y + sizePaddingY + sizeCenterY, sizePaddingX, sizePaddingY),
				new Rectangle(buttonTex.UV.X + buttonTex.UV.Width - paddingX, buttonTex.UV.Y + centerY + paddingY, paddingX, paddingY), buttonTex.texture);
		}

	}
}
