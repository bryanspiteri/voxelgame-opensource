using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;

namespace VoxelGame.Util
{
	public class TextureManager
	{
		private static Texture2D err;

		//Terrain
		public static Texture2D terrain;
		public static Texture2D whiteTerrain;

		//Ui
		private static Dictionary<string, Texture2D> uiTextureMap = new Dictionary<string, Texture2D>();

		//UV tables
		private static Dictionary<string, Vector4> uvMap = new Dictionary<string, Vector4>();
		private static Dictionary<string, Vector4> uiUvs = new Dictionary<string, Vector4>();

		private static Rectangle errUV = new Rectangle(0, 0, 32, 32);

		public static void Reload()
		{
			//TODO: Construct from multiple pngs
			uvMap.Clear();
			uiUvs.Clear();
			uiTextureMap.Clear();

			terrain = TextureLoader.loadTexture("terrain");
			uiTextureMap.Add("generic_gui", TextureLoader.loadTexture("textures/ui/generic_gui"));
			uiTextureMap.Add("hotbar_gui", TextureLoader.loadTexture("textures/ui/hotbar_gui"));
			err = TextureLoader.getErrorTexture();
			//Vector4 format (min.X, min.Y, max.X, max.Y)

			uvMap.Add("grass_top",   new Vector4(0.0625f * 00, 0.0625f * 00, 0.0625f * 01, 0.0625f * 01));
			uvMap.Add("northTest",   new Vector4(0.0625f * 14, 0.0625f * 15, 0.0625f * 15, 0.0625f * 16));
			uvMap.Add("dirt",        new Vector4(0.0625f * 02, 0.0625f * 00, 0.0625f * 03, 0.0625f * 01));
			uvMap.Add("grass_side",  new Vector4(0.0625f * 03, 0.0625f * 00, 0.0625f * 04, 0.0625f * 01));
			uvMap.Add("stone",       new Vector4(0.0625f * 01, 0.0625f * 00, 0.0625f * 02, 0.0625f * 01));

			uvMap.Add("wood_planks", new Vector4(0.0625f * 04, 0.0625f * 00, 0.0625f * 05, 0.0625f * 01));
			uvMap.Add("tectonite",  new Vector4(0.0625f * 05, 0.0625f * 00, 0.0625f * 06, 0.0625f * 01));
			uvMap.Add("wood_log_side", new Vector4(0.0625f * 06, 0.0625f * 00, 0.0625f * 07, 0.0625f * 01));
			uvMap.Add("wood_log_top", new Vector4(0.0625f * 07, 0.0625f * 00, 0.0625f * 08, 0.0625f * 01));
			uvMap.Add("leaves", new Vector4(0.0625f * 08, 0.0625f * 00, 0.0625f * 09, 0.0625f * 01));
			uvMap.Add("explosive_side", new Vector4(0.0625f * 09, 0.0625f * 00, 0.0625f * 10, 0.0625f * 01));
			uvMap.Add("explosive_top", new Vector4(0.0625f * 10, 0.0625f * 00, 0.0625f * 11, 0.0625f * 01));

			// `!err` is the error texture. it starts with ! so that it is an invalid file name, meaning the user can NEVER overwrite it
			uvMap.Add("!err", new Vector4(0.0625f * 15, 0.0625f * 15, 0.0625f * 16, 0.0625f * 16));
			//compose terrain during runtime
			//idk how but this is my brief idea rn

			//int x, int y = 1 (current size of texture)
			//rendertexture texturebuffer
			//texture2d outputTexture

			//for texture in texturesFolder
			//

			uiUvs.Add("button_disabled", new Vector4(0, 0.03125f * 0, 0.125f, 0.03125f * 1));
			uiUvs.Add("button_neutral",  new Vector4(0, 0.03125f * 1, 0.125f, 0.03125f * 2));
			uiUvs.Add("button_hover",    new Vector4(0, 0.03125f * 2, 0.125f, 0.03125f * 3));
			uiUvs.Add("button_clicked",  new Vector4(0, 0.03125f * 3, 0.125f, 0.03125f * 4));

			uiUvs.Add("hotbar_full", new Vector4(0, 0, 0.3515625f, 0.0390625f));
			uiUvs.Add("hotbar_selection", new Vector4(0, 0.0625f, 0.04296875f, 0.10546875f));

			RebuildWhiteTerrain();

			//ItemManager.RecacheTextures();
		}

		private static void RebuildWhiteTerrain()
		{
			whiteTerrain = new Texture2D(GameClient.Instance.GraphicsDevice, terrain.Width, terrain.Height, false, SurfaceFormat.Color);

			Color[] pixelData = new Color[terrain.Width * terrain.Height];
			terrain.GetData(pixelData);

			//Make the texture white, hiding only 100% transparent textures.
			for (int i = 0; i < pixelData.Length; i++)
			{
				Color col = pixelData[i];

				//Set to white
				col.R = 255;
				col.G = 255;
				col.B = 255;

				if (col.A > 0)
				{
					col.A = 255;
				}
				else
				{
					//col.A = 0;
				}
				pixelData[i] = col;
			}

			whiteTerrain.SetData(pixelData);
		}

		public static Vector4 GetUV(string texture)
		{
			if (uvMap.ContainsKey(texture))
			{
				return uvMap[texture];
			}
			return uvMap["!err"];
		}

		/// <summary>
		/// Returns the Rectangle that covers a certain texture
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public static UiTexture GetUiUv(string texture)
		{
			if (uiUvs.ContainsKey(texture))
			{
				Texture2D uiTex = getUiTextureFromString(texture);
				return new UiTexture(uiTex, new Rectangle(
					 (int)(uiUvs[texture].X * uiTex.Width), (int)(uiUvs[texture].Y * uiTex.Height),
					 (int)(System.Math.Abs(uiUvs[texture].X - uiUvs[texture].Z) * uiTex.Width), (int)(System.Math.Abs(uiUvs[texture].W - uiUvs[texture].Y) * uiTex.Height)));
			}
			return new UiTexture(err, errUV);
		}

		private static Texture2D getUiTextureFromString(string texName)
		{
			switch(texName)
			{
				case "button_disabled":
				case "button_neutral":
				case "button_hover":
				case "button_clicked":
					return uiTextureMap["generic_gui"];
				case "hotbar_full":
				case "hotbar_selection":
					return uiTextureMap["hotbar_gui"];
			}
			return err;
		}
	}

	public struct UiTexture
	{
		public Texture2D texture;
		public Rectangle UV;

		public UiTexture(Texture2D tex, Rectangle uv)
		{
			texture = tex;
			UV = uv;
		}
	}
}
