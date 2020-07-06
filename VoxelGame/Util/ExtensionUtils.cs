using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util.Math;

namespace VoxelGame
{
	public static class ExtensionUtils
	{
		private const float f = 0.5f;
		public static Vector3 Center(this Vector3 v)
		{
			return new Vector3((float)Math.Round(v.X + f), (float)Math.Round(v.Y + f), (float)Math.Round(v.Z + f));
		}

		/// <summary>
		/// Inverts the given color
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		public static Color Invert(this Color col)
		{
			col.R = (byte)FastMath.FastClamp(255 - col.R, 0, 255);
			col.G = (byte)FastMath.FastClamp(255 - col.G, 0, 255);
			col.B = (byte)FastMath.FastClamp(255 - col.B, 0, 255);

			return col;
		}
	}
}
