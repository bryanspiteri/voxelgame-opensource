using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Assets
{
	public class TextureLoader
	{
		public static Texture2D getErrorTexture()
		{
			Color black = new Color(0, 0, 0, 255);
			Color yellow = new Color(255, 216, 0, 255);
			Texture2D tex = new Texture2D(GameClient.Instance.GraphicsDevice, 32, 32);
			Color[] dt = new Color[1024];
			for (int x = 0; x < tex.Width; x++)
			{
				for (int y = 0; y < tex.Height; y++)
				{
					if (x < 16 && y < 16)
					{
						dt[x + (y * 32)] = black;
					}
					else if (!(x < 16) && y < 16)
					{
						dt[x + (y * 32)] = yellow;
					}
					else if (x < 16 && !(y < 16))
					{
						dt[x + (y * 32)] = yellow;
					}
					else if (!(x < 16 && y < 16))
					{
						dt[x + (y * 32)] = black;
					}
				}
			}
			tex.SetData(dt);
			return tex;
		}

		public static Texture2D getWhitePixel()
		{
			Texture2D tex = new Texture2D(GameClient.Instance.GraphicsDevice, 1, 1);
			Color[] dt = { new Color(new Vector4(1, 1, 1, 1)) };
			tex.SetData(dt);
			return tex;
		}

		public static Texture2D loadTexture(string texturePath)
		{
			try
			{
				FileStream fileStream = new FileStream(Path.GetFullPath(FileUtil.GetPath(texturePath + ".png")), FileMode.Open);
				Texture2D tex = Texture2D.FromStream(GameClient.Instance.GraphicsDevice, fileStream);
				fileStream.Dispose();
				return tex;
			}
			catch (Exception ex)
			{
				Logger.error(0, ex.Message);
			}
			return getErrorTexture();
		}

#if !CONSOLE
		public static void Save(Texture2D texture, string saveLocation)
		{
			if (texture != null)
			{
				if (!File.Exists(saveLocation))
				{
					if (!Directory.Exists(Path.GetDirectoryName(saveLocation)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(saveLocation));
					}
				}

				Texture2D opaqueTexture = new Texture2D(GameClient.Instance.GraphicsDevice, texture.Width, texture.Height);

				Color[] pixelData = new Color[texture.Width * texture.Height];
				texture.GetData(pixelData);

				//Make the texture opaque
				for (int i = 0; i < pixelData.Length; i++)
				{
					Color col = pixelData[i];
					col.A = 255;
					pixelData[i] = col;
				}

				opaqueTexture.SetData(pixelData);

				using (Stream fileStream = File.OpenWrite(saveLocation))
				{
					opaqueTexture.SaveAsPng(fileStream, opaqueTexture.Width, opaqueTexture.Height);
				}
			}
			else
			{
				Logger.warn(0, "tried saving null texture!");
			}
		}
#endif
	}
}
