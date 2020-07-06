using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Math
{
	public static class FastMath
	{
		static Random _rng = new Random();
		public static float Epsilon = 0.0f;

		public static int Floor(float num)
		{
			return num == 0 ? 0 : num > 0 ? (int)num : (int)num - 1;
		}

		public static int Floor(double num)
		{
			return num == 0 ? 0 : num > 0 ? (int)num : (int)num - 1;
		}

		public static int Ceil(double num)
		{
			return num == 0 ? 0 : num > 0 ? (int)num + 1 : (int)num;
		}

		public static int Ceil(float num)
		{
			return num == 0 ? 0 : num > 0 ? (int)num + 1 : (int)num;
		}

		public static float Min(float a, float b)
		{
			return a < b ? a : b;
		}

		public static float Max(float a, float b)
		{
			return a > b ? a : b;
		}

		public static float ClosestToZero(float a, float b)
		{
			float aTmp = a;
			float bTmp = b;

			//Absolute value
			if (a < 0)
			{
				aTmp = -1 * a;
			}
			if (b < 0)
			{
				bTmp = -1 * a;
			}

			//Math.Min
			return aTmp < bTmp ? a : b;
		}

		public static float FurthestToZero(float a, float b)
		{
			float aTmp = a;
			float bTmp = b;

			//Absolute value
			if (a < 0)
			{
				aTmp = -1 * a;
			}
			if (b < 0)
			{
				bTmp = -1 * a;
			}

			//Math.Max
			return aTmp > bTmp ? a : b;
		}

		/// <summary>
		/// Rounds the specififed number
		/// </summary>
		/// <param name="num">The number to round</param>
		/// <returns></returns>
		public static float Round(float num)
		{
			return (float)System.Math.Round(num, MidpointRounding.AwayFromZero);
		}

		/// <summary>
		/// Rounds the specififed number
		/// </summary>
		/// <param name="num">The number to round</param>
		/// <returns></returns>
		public static int RoundToInt(float num)
		{
			return (int)System.Math.Round(num, 0, MidpointRounding.AwayFromZero);
		}

		public static float FastClamp(float value, float min, float max)
		{
			return MathHelper.Min(max, MathHelper.Max(min, value));
		}

		public static int FastClamp(int value, int min, int max)
		{
			return MathHelper.Min(max, MathHelper.Max(min, value));
		}

		public static Color MultiplyColor(Color lhs, Color rhs)
		{
			Vector4 lhsVec = lhs.ToVector4();
			Vector4 rhsVec = rhs.ToVector4();

			return new Color(lhsVec.X * rhsVec.X, lhsVec.Y * rhsVec.Y, lhsVec.Z * rhsVec.Z, lhsVec.W * rhsVec.W);
		}

		#region Normalisation
		public static byte Normalise(byte number, byte min, byte max)
		{
			number = (byte)(number % max);
			if (number < min)
			{
				number += max;
			}
			return number;
		}

		public static short Normalise(short number, short min, short max)
		{
			number = (short)(number % max);
			if (number < min)
			{
				number += max;
			}
			return number;
		}

		public static ushort Normalise(ushort number, ushort min, ushort max)
		{
			number = (ushort)(number % max);
			if (number < min)
			{
				number += max;
			}
			return number;
		}

		public static int Normalise(int number, int min, int max)
		{
			number = number % max;
			if (number < min)
			{
				number += max;
			}
			return number;
		}

		public static float Normalise(float number, float min, float max)
		{
			number = number % max;
			if (number < min)
			{
				number += max;
			}
			return number;
		}
		#endregion

		public static int GetRandom(int min, int max)
		{
			return _rng.Next(min, max);
		}
	}
}
