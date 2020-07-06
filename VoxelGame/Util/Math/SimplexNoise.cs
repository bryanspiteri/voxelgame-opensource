using System;
using System.Runtime.CompilerServices;

namespace VoxelGame.Util.Math
{
	/// <summary>
	/// Implementation of the Perlin simplex noise, an improved Perlin noise algorithm.
	/// Based loosely on SimplexNoise1234 by Stefan Gustavson: http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/
	/// </summary>
	public class SimplexNoise
	{
		public float[] Calc1D(int width, float scale)
		{
			var values = new float[width];
			for (var i = 0; i < width; i++)
				values[i] = Generate(i * scale);
			return values;
		}

		public float[,] Calc2D(int width, int height, float scale)
		{
			var values = new float[width, height];
			for (var i = 0; i < width; i++)
				for (var j = 0; j < height; j++)
					values[i, j] = Generate(i * scale, j * scale);
			return values;
		}

		public float[,,] Calc3D(int width, int height, int length, float scale)
		{
			var values = new float[width, height, length];
			for (var i = 0; i < width; i++)
				for (var j = 0; j < height; j++)
					for (var k = 0; k < length; k++)
						values[i, j, k] = Generate(i * scale, j * scale, k * scale);
			return values;
		}

		public float CalcPixel1D(int x, float scale)
		{
			return Generate(x * scale);
		}

		public float CalcPixel2D(int x, int y, float scale)
		{
			return Generate(x * scale, y * scale);
		}

		public float CalcPixel3D(int x, int y, int z, float scale)
		{
			return Generate(x * scale, y * scale, z * scale);
		}

		public SimplexNoise(long seed)
		{
			_perm = new byte[PermOriginal.Length];
			PermOriginal.CopyTo(_perm, 0);
			Seed = seed;
		}

		public long Seed
		{
			get => _seed;
			set
			{
				if (value == 0)
				{
					_perm = new byte[PermOriginal.Length];
					PermOriginal.CopyTo(_perm, 0);
				}
				else
				{
					_perm = new byte[512];
					var random = new LongRandom(value);
					random.NextBytes(_perm);
				}

				_seed = value;
			}
		}

		private long _seed;

		/// <summary>
		/// 1D simplex noise
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		private float Generate(float x)
		{
			var i0 = FastFloor(x);
			var i1 = i0 + 1;
			var x0 = x - i0;
			var x1 = x0 - 1.0f;

			var t0 = 1.0f - x0 * x0;
			t0 *= t0;
			var n0 = t0 * t0 * Grad(_perm[i0 & 0xff], x0);

			var t1 = 1.0f - x1 * x1;
			t1 *= t1;
			var n1 = t1 * t1 * Grad(_perm[i1 & 0xff], x1);
			// The maximum value of this noise is 8*(3/4)^4 = 2.53125
			// A factor of 0.395 scales to fit exactly within [-1,1]
			return 0.395f * (n0 + n1);
		}

		/// <summary>
		/// 2D simplex noise
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private float Generate(float x, float y)
		{
			const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
			const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

			float n0, n1, n2; // Noise contributions from the three corners

			// Skew the input space to determine which simplex cell we're in
			var s = (x + y) * F2; // Hairy factor for 2D
			var xs = x + s;
			var ys = y + s;
			var i = FastFloor(xs);
			var j = FastFloor(ys);

			var t = (i + j) * G2;
			var X0 = i - t; // Unskew the cell origin back to (x,y) space
			var Y0 = j - t;
			var x0 = x - X0; // The x,y distances from the cell origin
			var y0 = y - Y0;

			// For the 2D case, the simplex shape is an equilateral triangle.
			// Determine which simplex we are in.
			int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
			if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
			else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

			// A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
			// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
			// c = (3-sqrt(3))/6

			var x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
			var y1 = y0 - j1 + G2;
			var x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
			var y2 = y0 - 1.0f + 2.0f * G2;

			// Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
			var ii = Mod(i, 256);
			var jj = Mod(j, 256);

			// Calculate the contribution from the three corners
			var t0 = 0.5f - x0 * x0 - y0 * y0;
			if (t0 < 0.0f) n0 = 0.0f;
			else
			{
				t0 *= t0;
				n0 = t0 * t0 * Grad(_perm[ii + _perm[jj]], x0, y0);
			}

			var t1 = 0.5f - x1 * x1 - y1 * y1;
			if (t1 < 0.0f) n1 = 0.0f;
			else
			{
				t1 *= t1;
				n1 = t1 * t1 * Grad(_perm[ii + i1 + _perm[jj + j1]], x1, y1);
			}

			var t2 = 0.5f - x2 * x2 - y2 * y2;
			if (t2 < 0.0f) n2 = 0.0f;
			else
			{
				t2 *= t2;
				n2 = t2 * t2 * Grad(_perm[ii + 1 + _perm[jj + 1]], x2, y2);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			return 40.0f * (n0 + n1 + n2); // TODO: The scale factor is preliminary!
		}


		private float Generate(float x, float y, float z)
		{
			// Simple skewing factors for the 3D case
			const float F3 = 0.333333333f;
			const float G3 = 0.166666667f;

			float n0, n1, n2, n3; // Noise contributions from the four corners

			// Skew the input space to determine which simplex cell we're in
			var s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
			var xs = x + s;
			var ys = y + s;
			var zs = z + s;
			var i = FastFloor(xs);
			var j = FastFloor(ys);
			var k = FastFloor(zs);

			var t = (i + j + k) * G3;
			var X0 = i - t; // Unskew the cell origin back to (x,y,z) space
			var Y0 = j - t;
			var Z0 = k - t;
			var x0 = x - X0; // The x,y,z distances from the cell origin
			var y0 = y - Y0;
			var z0 = z - Z0;

			// For the 3D case, the simplex shape is a slightly irregular tetrahedron.
			// Determine which simplex we are in.
			int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
			int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

			/* This code would benefit from a backport from the GLSL version! */
			if (x0 >= y0)
			{
				if (y0 >= z0)
				{ i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
				else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
				else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
			}
			else
			{ // x0<y0
				if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
				else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
				else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
			}

			// A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
			// a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
			// a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
			// c = 1/6.

			var x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
			var y1 = y0 - j1 + G3;
			var z1 = z0 - k1 + G3;
			var x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
			var y2 = y0 - j2 + 2.0f * G3;
			var z2 = z0 - k2 + 2.0f * G3;
			var x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
			var y3 = y0 - 1.0f + 3.0f * G3;
			var z3 = z0 - 1.0f + 3.0f * G3;

			// Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
			var ii = Mod(i, 256);
			var jj = Mod(j, 256);
			var kk = Mod(k, 256);

			// Calculate the contribution from the four corners
			var t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
			if (t0 < 0.0f) n0 = 0.0f;
			else
			{
				t0 *= t0;
				n0 = t0 * t0 * Grad(_perm[ii + _perm[jj + _perm[kk]]], x0, y0, z0);
			}

			var t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
			if (t1 < 0.0f) n1 = 0.0f;
			else
			{
				t1 *= t1;
				n1 = t1 * t1 * Grad(_perm[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]], x1, y1, z1);
			}

			var t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
			if (t2 < 0.0f) n2 = 0.0f;
			else
			{
				t2 *= t2;
				n2 = t2 * t2 * Grad(_perm[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]], x2, y2, z2);
			}

			var t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
			if (t3 < 0.0f) n3 = 0.0f;
			else
			{
				t3 *= t3;
				n3 = t3 * t3 * Grad(_perm[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]], x3, y3, z3);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to stay just inside [-1,1]
			return 32.0f * (n0 + n1 + n2 + n3); // TODO: The scale factor is preliminary!
		}

		private byte[] _perm;

		private static readonly byte[] PermOriginal = {
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
		};

		private int FastFloor(float x)
		{
			return (x > 0) ? ((int)x) : (((int)x) - 1);
		}

		private int Mod(int x, int m)
		{
			var a = x % m;
			return a < 0 ? a + m : a;
		}

		private float Grad(int hash, float x)
		{
			var h = hash & 15;
			var grad = 1.0f + (h & 7);   // Gradient value 1.0, 2.0, ..., 8.0
			if ((h & 8) != 0) grad = -grad;         // Set a random sign for the gradient
			return (grad * x);           // Multiply the gradient with the distance
		}

		private float Grad(int hash, float x, float y)
		{
			var h = hash & 7;      // Convert low 3 bits of hash code
			var u = h < 4 ? x : y;  // into 8 simple gradient directions,
			var v = h < 4 ? y : x;  // and compute the dot product with (x,y).
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
		}

		private float Grad(int hash, float x, float y, float z)
		{
			var h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
			var u = h < 8 ? x : y; // gradient directions, and compute dot product.
			var v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
		}

		private float Grad(int hash, float x, float y, float z, float t)
		{
			var h = hash & 31;      // Convert low 5 bits of hash code into 32 simple
			var u = h < 24 ? x : y; // gradient directions, and compute dot product.
			var v = h < 16 ? y : z;
			var w = h < 8 ? z : t;
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
		}
	}


	public class OpenSimplexNoise
	{
		private const double STRETCH_2D = -0.211324865405187;    //(1/Math.sqrt(2+1)-1)/2;
		private const double STRETCH_3D = -1.0 / 6.0;            //(1/Math.sqrt(3+1)-1)/3;
		private const double STRETCH_4D = -0.138196601125011;    //(1/Math.sqrt(4+1)-1)/4;
		private const double SQUISH_2D = 0.366025403784439;      //(Math.sqrt(2+1)-1)/2;
		private const double SQUISH_3D = 1.0 / 3.0;              //(Math.sqrt(3+1)-1)/3;
		private const double SQUISH_4D = 0.309016994374947;      //(Math.sqrt(4+1)-1)/4;
		private const double NORM_2D = 1.0 / 47.0;
		private const double NORM_3D = 1.0 / 103.0;
		private const double NORM_4D = 1.0 / 30.0;

		private byte[] perm;
		private byte[] perm2D;
		private byte[] perm3D;
		private byte[] perm4D;

		private static double[] gradients2D = new double[]
		{
			 5,  2,    2,  5,
			-5,  2,   -2,  5,
			 5, -2,    2, -5,
			-5, -2,   -2, -5,
		};

		private static double[] gradients3D =
		{
			-11,  4,  4,     -4,  11,  4,    -4,  4,  11,
			 11,  4,  4,      4,  11,  4,     4,  4,  11,
			-11, -4,  4,     -4, -11,  4,    -4, -4,  11,
			 11, -4,  4,      4, -11,  4,     4, -4,  11,
			-11,  4, -4,     -4,  11, -4,    -4,  4, -11,
			 11,  4, -4,      4,  11, -4,     4,  4, -11,
			-11, -4, -4,     -4, -11, -4,    -4, -4, -11,
			 11, -4, -4,      4, -11, -4,     4, -4, -11,
		};

		private static double[] gradients4D =
		{
			 3,  1,  1,  1,      1,  3,  1,  1,      1,  1,  3,  1,      1,  1,  1,  3,
			-3,  1,  1,  1,     -1,  3,  1,  1,     -1,  1,  3,  1,     -1,  1,  1,  3,
			 3, -1,  1,  1,      1, -3,  1,  1,      1, -1,  3,  1,      1, -1,  1,  3,
			-3, -1,  1,  1,     -1, -3,  1,  1,     -1, -1,  3,  1,     -1, -1,  1,  3,
			 3,  1, -1,  1,      1,  3, -1,  1,      1,  1, -3,  1,      1,  1, -1,  3,
			-3,  1, -1,  1,     -1,  3, -1,  1,     -1,  1, -3,  1,     -1,  1, -1,  3,
			 3, -1, -1,  1,      1, -3, -1,  1,      1, -1, -3,  1,      1, -1, -1,  3,
			-3, -1, -1,  1,     -1, -3, -1,  1,     -1, -1, -3,  1,     -1, -1, -1,  3,
			 3,  1,  1, -1,      1,  3,  1, -1,      1,  1,  3, -1,      1,  1,  1, -3,
			-3,  1,  1, -1,     -1,  3,  1, -1,     -1,  1,  3, -1,     -1,  1,  1, -3,
			 3, -1,  1, -1,      1, -3,  1, -1,      1, -1,  3, -1,      1, -1,  1, -3,
			-3, -1,  1, -1,     -1, -3,  1, -1,     -1, -1,  3, -1,     -1, -1,  1, -3,
			 3,  1, -1, -1,      1,  3, -1, -1,      1,  1, -3, -1,      1,  1, -1, -3,
			-3,  1, -1, -1,     -1,  3, -1, -1,     -1,  1, -3, -1,     -1,  1, -1, -3,
			 3, -1, -1, -1,      1, -3, -1, -1,      1, -1, -3, -1,      1, -1, -1, -3,
			-3, -1, -1, -1,     -1, -3, -1, -1,     -1, -1, -3, -1,     -1, -1, -1, -3,
		};

		private static Contribution2[] lookup2D;
		private static Contribution3[] lookup3D;
		private static Contribution4[] lookup4D;

		static OpenSimplexNoise()
		{
			var base2D = new int[][]
			{
				new int[] { 1, 1, 0, 1, 0, 1, 0, 0, 0 },
				new int[] { 1, 1, 0, 1, 0, 1, 2, 1, 1 }
			};
			var p2D = new int[] { 0, 0, 1, -1, 0, 0, -1, 1, 0, 2, 1, 1, 1, 2, 2, 0, 1, 2, 0, 2, 1, 0, 0, 0 };
			var lookupPairs2D = new int[] { 0, 1, 1, 0, 4, 1, 17, 0, 20, 2, 21, 2, 22, 5, 23, 5, 26, 4, 39, 3, 42, 4, 43, 3 };

			var contributions2D = new Contribution2[p2D.Length / 4];
			for (int i = 0; i < p2D.Length; i += 4)
			{
				var baseSet = base2D[p2D[i]];
				Contribution2 previous = null, current = null;
				for (int k = 0; k < baseSet.Length; k += 3)
				{
					current = new Contribution2(baseSet[k], baseSet[k + 1], baseSet[k + 2]);
					if (previous == null)
					{
						contributions2D[i / 4] = current;
					}
					else
					{
						previous.Next = current;
					}
					previous = current;
				}
				current.Next = new Contribution2(p2D[i + 1], p2D[i + 2], p2D[i + 3]);
			}

			lookup2D = new Contribution2[64];
			for (var i = 0; i < lookupPairs2D.Length; i += 2)
			{
				lookup2D[lookupPairs2D[i]] = contributions2D[lookupPairs2D[i + 1]];
			}


			var base3D = new int[][]
			{
				new int[] { 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1 },
				new int[] { 2, 1, 1, 0, 2, 1, 0, 1, 2, 0, 1, 1, 3, 1, 1, 1 },
				new int[] { 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 2, 1, 1, 0, 2, 1, 0, 1, 2, 0, 1, 1 }
			};
			var p3D = new int[] { 0, 0, 1, -1, 0, 0, 1, 0, -1, 0, 0, -1, 1, 0, 0, 0, 1, -1, 0, 0, -1, 0, 1, 0, 0, -1, 1, 0, 2, 1, 1, 0, 1, 1, 1, -1, 0, 2, 1, 0, 1, 1, 1, -1, 1, 0, 2, 0, 1, 1, 1, -1, 1, 1, 1, 3, 2, 1, 0, 3, 1, 2, 0, 1, 3, 2, 0, 1, 3, 1, 0, 2, 1, 3, 0, 2, 1, 3, 0, 1, 2, 1, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 0, 2, 0, 2, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 0, 0, 0, 1, 1, -1, 1, 2, 0, 0, 0, 0, 1, -1, 1, 1, 2, 0, 0, 0, 0, 1, 1, 1, -1, 2, 3, 1, 1, 1, 2, 0, 0, 2, 2, 3, 1, 1, 1, 2, 2, 0, 0, 2, 3, 1, 1, 1, 2, 0, 2, 0, 2, 1, 1, -1, 1, 2, 0, 0, 2, 2, 1, 1, -1, 1, 2, 2, 0, 0, 2, 1, -1, 1, 1, 2, 0, 0, 2, 2, 1, -1, 1, 1, 2, 0, 2, 0, 2, 1, 1, 1, -1, 2, 2, 0, 0, 2, 1, 1, 1, -1, 2, 0, 2, 0 };
			var lookupPairs3D = new int[] { 0, 2, 1, 1, 2, 2, 5, 1, 6, 0, 7, 0, 32, 2, 34, 2, 129, 1, 133, 1, 160, 5, 161, 5, 518, 0, 519, 0, 546, 4, 550, 4, 645, 3, 647, 3, 672, 5, 673, 5, 674, 4, 677, 3, 678, 4, 679, 3, 680, 13, 681, 13, 682, 12, 685, 14, 686, 12, 687, 14, 712, 20, 714, 18, 809, 21, 813, 23, 840, 20, 841, 21, 1198, 19, 1199, 22, 1226, 18, 1230, 19, 1325, 23, 1327, 22, 1352, 15, 1353, 17, 1354, 15, 1357, 17, 1358, 16, 1359, 16, 1360, 11, 1361, 10, 1362, 11, 1365, 10, 1366, 9, 1367, 9, 1392, 11, 1394, 11, 1489, 10, 1493, 10, 1520, 8, 1521, 8, 1878, 9, 1879, 9, 1906, 7, 1910, 7, 2005, 6, 2007, 6, 2032, 8, 2033, 8, 2034, 7, 2037, 6, 2038, 7, 2039, 6 };

			var contributions3D = new Contribution3[p3D.Length / 9];
			for (int i = 0; i < p3D.Length; i += 9)
			{
				var baseSet = base3D[p3D[i]];
				Contribution3 previous = null, current = null;
				for (int k = 0; k < baseSet.Length; k += 4)
				{
					current = new Contribution3(baseSet[k], baseSet[k + 1], baseSet[k + 2], baseSet[k + 3]);
					if (previous == null)
					{
						contributions3D[i / 9] = current;
					}
					else
					{
						previous.Next = current;
					}
					previous = current;
				}
				current.Next = new Contribution3(p3D[i + 1], p3D[i + 2], p3D[i + 3], p3D[i + 4]);
				current.Next.Next = new Contribution3(p3D[i + 5], p3D[i + 6], p3D[i + 7], p3D[i + 8]);
			}

			lookup3D = new Contribution3[2048];
			for (var i = 0; i < lookupPairs3D.Length; i += 2)
			{
				lookup3D[lookupPairs3D[i]] = contributions3D[lookupPairs3D[i + 1]];
			}

			var base4D = new int[][]
			{
				new int[] { 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
				new int[] { 3, 1, 1, 1, 0, 3, 1, 1, 0, 1, 3, 1, 0, 1, 1, 3, 0, 1, 1, 1, 4, 1, 1, 1, 1 },
				new int[] { 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 2, 1, 1, 0, 0, 2, 1, 0, 1, 0, 2, 1, 0, 0, 1, 2, 0, 1, 1, 0, 2, 0, 1, 0, 1, 2, 0, 0, 1, 1 },
				new int[] { 3, 1, 1, 1, 0, 3, 1, 1, 0, 1, 3, 1, 0, 1, 1, 3, 0, 1, 1, 1, 2, 1, 1, 0, 0, 2, 1, 0, 1, 0, 2, 1, 0, 0, 1, 2, 0, 1, 1, 0, 2, 0, 1, 0, 1, 2, 0, 0, 1, 1 }
			};
			var p4D = new int[] { 0, 0, 1, -1, 0, 0, 0, 1, 0, -1, 0, 0, 1, 0, 0, -1, 0, 0, -1, 1, 0, 0, 0, 0, 1, -1, 0, 0, 0, 1, 0, -1, 0, 0, -1, 0, 1, 0, 0, 0, -1, 1, 0, 0, 0, 0, 1, -1, 0, 0, -1, 0, 0, 1, 0, 0, -1, 0, 1, 0, 0, 0, -1, 1, 0, 2, 1, 1, 0, 0, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 0, 2, 1, 0, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 0, 2, 0, 1, 1, 0, 1, -1, 1, 1, 0, 1, 0, 1, 1, -1, 0, 2, 1, 0, 0, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 1, 0, 2, 0, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, 0, 2, 0, 0, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 1, 1, 1, 4, 2, 1, 1, 0, 4, 1, 2, 1, 0, 4, 1, 1, 2, 0, 1, 4, 2, 1, 0, 1, 4, 1, 2, 0, 1, 4, 1, 1, 0, 2, 1, 4, 2, 0, 1, 1, 4, 1, 0, 2, 1, 4, 1, 0, 1, 2, 1, 4, 0, 2, 1, 1, 4, 0, 1, 2, 1, 4, 0, 1, 1, 2, 1, 2, 1, 1, 0, 0, 3, 2, 1, 0, 0, 3, 1, 2, 0, 0, 1, 2, 1, 0, 1, 0, 3, 2, 0, 1, 0, 3, 1, 0, 2, 0, 1, 2, 0, 1, 1, 0, 3, 0, 2, 1, 0, 3, 0, 1, 2, 0, 1, 2, 1, 0, 0, 1, 3, 2, 0, 0, 1, 3, 1, 0, 0, 2, 1, 2, 0, 1, 0, 1, 3, 0, 2, 0, 1, 3, 0, 1, 0, 2, 1, 2, 0, 0, 1, 1, 3, 0, 0, 2, 1, 3, 0, 0, 1, 2, 2, 3, 1, 1, 1, 0, 2, 1, 1, 1, -1, 2, 2, 0, 0, 0, 2, 3, 1, 1, 0, 1, 2, 1, 1, -1, 1, 2, 2, 0, 0, 0, 2, 3, 1, 0, 1, 1, 2, 1, -1, 1, 1, 2, 2, 0, 0, 0, 2, 3, 1, 1, 1, 0, 2, 1, 1, 1, -1, 2, 0, 2, 0, 0, 2, 3, 1, 1, 0, 1, 2, 1, 1, -1, 1, 2, 0, 2, 0, 0, 2, 3, 0, 1, 1, 1, 2, -1, 1, 1, 1, 2, 0, 2, 0, 0, 2, 3, 1, 1, 1, 0, 2, 1, 1, 1, -1, 2, 0, 0, 2, 0, 2, 3, 1, 0, 1, 1, 2, 1, -1, 1, 1, 2, 0, 0, 2, 0, 2, 3, 0, 1, 1, 1, 2, -1, 1, 1, 1, 2, 0, 0, 2, 0, 2, 3, 1, 1, 0, 1, 2, 1, 1, -1, 1, 2, 0, 0, 0, 2, 2, 3, 1, 0, 1, 1, 2, 1, -1, 1, 1, 2, 0, 0, 0, 2, 2, 3, 0, 1, 1, 1, 2, -1, 1, 1, 1, 2, 0, 0, 0, 2, 2, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 0, 0, 0, 0, 0, 2, 1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 0, 0, 0, 0, 0, 2, 1, -1, 1, 1, 0, 1, 0, 1, 1, -1, 0, 0, 0, 0, 0, 2, 1, 1, -1, 0, 1, 1, 1, 0, -1, 1, 0, 0, 0, 0, 0, 2, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, 0, 0, 0, 0, 0, 2, 1, -1, 0, 1, 1, 1, 0, -1, 1, 1, 0, 0, 0, 0, 0, 2, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 2, 2, 0, 0, 0, 2, 1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 2, 2, 0, 0, 0, 2, 1, 1, -1, 0, 1, 1, 1, 0, -1, 1, 2, 2, 0, 0, 0, 2, 1, 1, 1, -1, 0, 1, 1, 1, 0, -1, 2, 0, 2, 0, 0, 2, 1, -1, 1, 1, 0, 1, 0, 1, 1, -1, 2, 0, 2, 0, 0, 2, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, 2, 0, 2, 0, 0, 2, 1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 2, 0, 0, 2, 0, 2, 1, -1, 1, 1, 0, 1, 0, 1, 1, -1, 2, 0, 0, 2, 0, 2, 1, -1, 0, 1, 1, 1, 0, -1, 1, 1, 2, 0, 0, 2, 0, 2, 1, 1, -1, 0, 1, 1, 1, 0, -1, 1, 2, 0, 0, 0, 2, 2, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, 2, 0, 0, 0, 2, 2, 1, -1, 0, 1, 1, 1, 0, -1, 1, 1, 2, 0, 0, 0, 2, 3, 1, 1, 0, 0, 0, 2, 2, 0, 0, 0, 2, 1, 1, 1, -1, 3, 1, 0, 1, 0, 0, 2, 0, 2, 0, 0, 2, 1, 1, 1, -1, 3, 1, 0, 0, 1, 0, 2, 0, 0, 2, 0, 2, 1, 1, 1, -1, 3, 1, 1, 0, 0, 0, 2, 2, 0, 0, 0, 2, 1, 1, -1, 1, 3, 1, 0, 1, 0, 0, 2, 0, 2, 0, 0, 2, 1, 1, -1, 1, 3, 1, 0, 0, 0, 1, 2, 0, 0, 0, 2, 2, 1, 1, -1, 1, 3, 1, 1, 0, 0, 0, 2, 2, 0, 0, 0, 2, 1, -1, 1, 1, 3, 1, 0, 0, 1, 0, 2, 0, 0, 2, 0, 2, 1, -1, 1, 1, 3, 1, 0, 0, 0, 1, 2, 0, 0, 0, 2, 2, 1, -1, 1, 1, 3, 1, 0, 1, 0, 0, 2, 0, 2, 0, 0, 2, -1, 1, 1, 1, 3, 1, 0, 0, 1, 0, 2, 0, 0, 2, 0, 2, -1, 1, 1, 1, 3, 1, 0, 0, 0, 1, 2, 0, 0, 0, 2, 2, -1, 1, 1, 1, 3, 3, 2, 1, 0, 0, 3, 1, 2, 0, 0, 4, 1, 1, 1, 1, 3, 3, 2, 0, 1, 0, 3, 1, 0, 2, 0, 4, 1, 1, 1, 1, 3, 3, 0, 2, 1, 0, 3, 0, 1, 2, 0, 4, 1, 1, 1, 1, 3, 3, 2, 0, 0, 1, 3, 1, 0, 0, 2, 4, 1, 1, 1, 1, 3, 3, 0, 2, 0, 1, 3, 0, 1, 0, 2, 4, 1, 1, 1, 1, 3, 3, 0, 0, 2, 1, 3, 0, 0, 1, 2, 4, 1, 1, 1, 1, 3, 3, 2, 1, 0, 0, 3, 1, 2, 0, 0, 2, 1, 1, 1, -1, 3, 3, 2, 0, 1, 0, 3, 1, 0, 2, 0, 2, 1, 1, 1, -1, 3, 3, 0, 2, 1, 0, 3, 0, 1, 2, 0, 2, 1, 1, 1, -1, 3, 3, 2, 1, 0, 0, 3, 1, 2, 0, 0, 2, 1, 1, -1, 1, 3, 3, 2, 0, 0, 1, 3, 1, 0, 0, 2, 2, 1, 1, -1, 1, 3, 3, 0, 2, 0, 1, 3, 0, 1, 0, 2, 2, 1, 1, -1, 1, 3, 3, 2, 0, 1, 0, 3, 1, 0, 2, 0, 2, 1, -1, 1, 1, 3, 3, 2, 0, 0, 1, 3, 1, 0, 0, 2, 2, 1, -1, 1, 1, 3, 3, 0, 0, 2, 1, 3, 0, 0, 1, 2, 2, 1, -1, 1, 1, 3, 3, 0, 2, 1, 0, 3, 0, 1, 2, 0, 2, -1, 1, 1, 1, 3, 3, 0, 2, 0, 1, 3, 0, 1, 0, 2, 2, -1, 1, 1, 1, 3, 3, 0, 0, 2, 1, 3, 0, 0, 1, 2, 2, -1, 1, 1, 1 };
			var lookupPairs4D = new int[] { 0, 3, 1, 2, 2, 3, 5, 2, 6, 1, 7, 1, 8, 3, 9, 2, 10, 3, 13, 2, 16, 3, 18, 3, 22, 1, 23, 1, 24, 3, 26, 3, 33, 2, 37, 2, 38, 1, 39, 1, 41, 2, 45, 2, 54, 1, 55, 1, 56, 0, 57, 0, 58, 0, 59, 0, 60, 0, 61, 0, 62, 0, 63, 0, 256, 3, 258, 3, 264, 3, 266, 3, 272, 3, 274, 3, 280, 3, 282, 3, 2049, 2, 2053, 2, 2057, 2, 2061, 2, 2081, 2, 2085, 2, 2089, 2, 2093, 2, 2304, 9, 2305, 9, 2312, 9, 2313, 9, 16390, 1, 16391, 1, 16406, 1, 16407, 1, 16422, 1, 16423, 1, 16438, 1, 16439, 1, 16642, 8, 16646, 8, 16658, 8, 16662, 8, 18437, 6, 18439, 6, 18469, 6, 18471, 6, 18688, 9, 18689, 9, 18690, 8, 18693, 6, 18694, 8, 18695, 6, 18696, 9, 18697, 9, 18706, 8, 18710, 8, 18725, 6, 18727, 6, 131128, 0, 131129, 0, 131130, 0, 131131, 0, 131132, 0, 131133, 0, 131134, 0, 131135, 0, 131352, 7, 131354, 7, 131384, 7, 131386, 7, 133161, 5, 133165, 5, 133177, 5, 133181, 5, 133376, 9, 133377, 9, 133384, 9, 133385, 9, 133400, 7, 133402, 7, 133417, 5, 133421, 5, 133432, 7, 133433, 5, 133434, 7, 133437, 5, 147510, 4, 147511, 4, 147518, 4, 147519, 4, 147714, 8, 147718, 8, 147730, 8, 147734, 8, 147736, 7, 147738, 7, 147766, 4, 147767, 4, 147768, 7, 147770, 7, 147774, 4, 147775, 4, 149509, 6, 149511, 6, 149541, 6, 149543, 6, 149545, 5, 149549, 5, 149558, 4, 149559, 4, 149561, 5, 149565, 5, 149566, 4, 149567, 4, 149760, 9, 149761, 9, 149762, 8, 149765, 6, 149766, 8, 149767, 6, 149768, 9, 149769, 9, 149778, 8, 149782, 8, 149784, 7, 149786, 7, 149797, 6, 149799, 6, 149801, 5, 149805, 5, 149814, 4, 149815, 4, 149816, 7, 149817, 5, 149818, 7, 149821, 5, 149822, 4, 149823, 4, 149824, 37, 149825, 37, 149826, 36, 149829, 34, 149830, 36, 149831, 34, 149832, 37, 149833, 37, 149842, 36, 149846, 36, 149848, 35, 149850, 35, 149861, 34, 149863, 34, 149865, 33, 149869, 33, 149878, 32, 149879, 32, 149880, 35, 149881, 33, 149882, 35, 149885, 33, 149886, 32, 149887, 32, 150080, 49, 150082, 48, 150088, 49, 150098, 48, 150104, 47, 150106, 47, 151873, 46, 151877, 45, 151881, 46, 151909, 45, 151913, 44, 151917, 44, 152128, 49, 152129, 46, 152136, 49, 152137, 46, 166214, 43, 166215, 42, 166230, 43, 166247, 42, 166262, 41, 166263, 41, 166466, 48, 166470, 43, 166482, 48, 166486, 43, 168261, 45, 168263, 42, 168293, 45, 168295, 42, 168512, 31, 168513, 28, 168514, 31, 168517, 28, 168518, 25, 168519, 25, 280952, 40, 280953, 39, 280954, 40, 280957, 39, 280958, 38, 280959, 38, 281176, 47, 281178, 47, 281208, 40, 281210, 40, 282985, 44, 282989, 44, 283001, 39, 283005, 39, 283208, 30, 283209, 27, 283224, 30, 283241, 27, 283256, 22, 283257, 22, 297334, 41, 297335, 41, 297342, 38, 297343, 38, 297554, 29, 297558, 24, 297562, 29, 297590, 24, 297594, 21, 297598, 21, 299365, 26, 299367, 23, 299373, 26, 299383, 23, 299389, 20, 299391, 20, 299584, 31, 299585, 28, 299586, 31, 299589, 28, 299590, 25, 299591, 25, 299592, 30, 299593, 27, 299602, 29, 299606, 24, 299608, 30, 299610, 29, 299621, 26, 299623, 23, 299625, 27, 299629, 26, 299638, 24, 299639, 23, 299640, 22, 299641, 22, 299642, 21, 299645, 20, 299646, 21, 299647, 20, 299648, 61, 299649, 60, 299650, 61, 299653, 60, 299654, 59, 299655, 59, 299656, 58, 299657, 57, 299666, 55, 299670, 54, 299672, 58, 299674, 55, 299685, 52, 299687, 51, 299689, 57, 299693, 52, 299702, 54, 299703, 51, 299704, 56, 299705, 56, 299706, 53, 299709, 50, 299710, 53, 299711, 50, 299904, 61, 299906, 61, 299912, 58, 299922, 55, 299928, 58, 299930, 55, 301697, 60, 301701, 60, 301705, 57, 301733, 52, 301737, 57, 301741, 52, 301952, 79, 301953, 79, 301960, 76, 301961, 76, 316038, 59, 316039, 59, 316054, 54, 316071, 51, 316086, 54, 316087, 51, 316290, 78, 316294, 78, 316306, 73, 316310, 73, 318085, 77, 318087, 77, 318117, 70, 318119, 70, 318336, 79, 318337, 79, 318338, 78, 318341, 77, 318342, 78, 318343, 77, 430776, 56, 430777, 56, 430778, 53, 430781, 50, 430782, 53, 430783, 50, 431000, 75, 431002, 72, 431032, 75, 431034, 72, 432809, 74, 432813, 69, 432825, 74, 432829, 69, 433032, 76, 433033, 76, 433048, 75, 433065, 74, 433080, 75, 433081, 74, 447158, 71, 447159, 68, 447166, 71, 447167, 68, 447378, 73, 447382, 73, 447386, 72, 447414, 71, 447418, 72, 447422, 71, 449189, 70, 449191, 70, 449197, 69, 449207, 68, 449213, 69, 449215, 68, 449408, 67, 449409, 67, 449410, 66, 449413, 64, 449414, 66, 449415, 64, 449416, 67, 449417, 67, 449426, 66, 449430, 66, 449432, 65, 449434, 65, 449445, 64, 449447, 64, 449449, 63, 449453, 63, 449462, 62, 449463, 62, 449464, 65, 449465, 63, 449466, 65, 449469, 63, 449470, 62, 449471, 62, 449472, 19, 449473, 19, 449474, 18, 449477, 16, 449478, 18, 449479, 16, 449480, 19, 449481, 19, 449490, 18, 449494, 18, 449496, 17, 449498, 17, 449509, 16, 449511, 16, 449513, 15, 449517, 15, 449526, 14, 449527, 14, 449528, 17, 449529, 15, 449530, 17, 449533, 15, 449534, 14, 449535, 14, 449728, 19, 449729, 19, 449730, 18, 449734, 18, 449736, 19, 449737, 19, 449746, 18, 449750, 18, 449752, 17, 449754, 17, 449784, 17, 449786, 17, 451520, 19, 451521, 19, 451525, 16, 451527, 16, 451528, 19, 451529, 19, 451557, 16, 451559, 16, 451561, 15, 451565, 15, 451577, 15, 451581, 15, 451776, 19, 451777, 19, 451784, 19, 451785, 19, 465858, 18, 465861, 16, 465862, 18, 465863, 16, 465874, 18, 465878, 18, 465893, 16, 465895, 16, 465910, 14, 465911, 14, 465918, 14, 465919, 14, 466114, 18, 466118, 18, 466130, 18, 466134, 18, 467909, 16, 467911, 16, 467941, 16, 467943, 16, 468160, 13, 468161, 13, 468162, 13, 468163, 13, 468164, 13, 468165, 13, 468166, 13, 468167, 13, 580568, 17, 580570, 17, 580585, 15, 580589, 15, 580598, 14, 580599, 14, 580600, 17, 580601, 15, 580602, 17, 580605, 15, 580606, 14, 580607, 14, 580824, 17, 580826, 17, 580856, 17, 580858, 17, 582633, 15, 582637, 15, 582649, 15, 582653, 15, 582856, 12, 582857, 12, 582872, 12, 582873, 12, 582888, 12, 582889, 12, 582904, 12, 582905, 12, 596982, 14, 596983, 14, 596990, 14, 596991, 14, 597202, 11, 597206, 11, 597210, 11, 597214, 11, 597234, 11, 597238, 11, 597242, 11, 597246, 11, 599013, 10, 599015, 10, 599021, 10, 599023, 10, 599029, 10, 599031, 10, 599037, 10, 599039, 10, 599232, 13, 599233, 13, 599234, 13, 599235, 13, 599236, 13, 599237, 13, 599238, 13, 599239, 13, 599240, 12, 599241, 12, 599250, 11, 599254, 11, 599256, 12, 599257, 12, 599258, 11, 599262, 11, 599269, 10, 599271, 10, 599272, 12, 599273, 12, 599277, 10, 599279, 10, 599282, 11, 599285, 10, 599286, 11, 599287, 10, 599288, 12, 599289, 12, 599290, 11, 599293, 10, 599294, 11, 599295, 10 };
			var contributions4D = new Contribution4[p4D.Length / 16];
			for (int i = 0; i < p4D.Length; i += 16)
			{
				var baseSet = base4D[p4D[i]];
				Contribution4 previous = null, current = null;
				for (int k = 0; k < baseSet.Length; k += 5)
				{
					current = new Contribution4(baseSet[k], baseSet[k + 1], baseSet[k + 2], baseSet[k + 3], baseSet[k + 4]);
					if (previous == null)
					{
						contributions4D[i / 16] = current;
					}
					else
					{
						previous.Next = current;
					}
					previous = current;
				}
				current.Next = new Contribution4(p4D[i + 1], p4D[i + 2], p4D[i + 3], p4D[i + 4], p4D[i + 5]);
				current.Next.Next = new Contribution4(p4D[i + 6], p4D[i + 7], p4D[i + 8], p4D[i + 9], p4D[i + 10]);
				current.Next.Next.Next = new Contribution4(p4D[i + 11], p4D[i + 12], p4D[i + 13], p4D[i + 14], p4D[i + 15]);
			}

			lookup4D = new Contribution4[1048576];
			for (var i = 0; i < lookupPairs4D.Length; i += 2)
			{
				lookup4D[lookupPairs4D[i]] = contributions4D[lookupPairs4D[i + 1]];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FastFloor(double x)
		{
			var xi = (int)x;
			return x < xi ? xi - 1 : xi;
		}

		public OpenSimplexNoise()
			: this(DateTime.Now.Ticks)
		{
		}

		public OpenSimplexNoise(long seed)
		{
			perm = new byte[256];
			perm2D = new byte[256];
			perm3D = new byte[256];
			perm4D = new byte[256];
			var source = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				source[i] = (byte)i;
			}
			seed = seed * 6364136223846793005L + 1442695040888963407L;
			seed = seed * 6364136223846793005L + 1442695040888963407L;
			seed = seed * 6364136223846793005L + 1442695040888963407L;
			for (int i = 255; i >= 0; i--)
			{
				seed = seed * 6364136223846793005L + 1442695040888963407L;
				int r = (int)((seed + 31) % (i + 1));
				if (r < 0)
				{
					r += (i + 1);
				}
				perm[i] = source[r];
				perm2D[i] = (byte)(perm[i] & 0x0E);
				perm3D[i] = (byte)((perm[i] % 24) * 3);
				perm4D[i] = (byte)(perm[i] & 0xFC);
				source[r] = source[i];
			}
		}

		public double Evaluate(double x, double y)
		{
			double stretchOffset = (x + y) * STRETCH_2D;
			double xs = x + stretchOffset;
			double ys = y + stretchOffset;

			int xsb = FastFloor(xs);
			int ysb = FastFloor(ys);

			double squishOffset = (xsb + ysb) * SQUISH_2D;
			double dx0 = x - (xsb + squishOffset);
			double dy0 = y - (ysb + squishOffset);

			double xins = xs - xsb;
			double yins = ys - ysb;

			double inSum = xins + yins;

			int hash =
			   (int)(xins - yins + 1) |
			   (int)(inSum) << 1 |
			   (int)(inSum + yins) << 2 |
			   (int)(inSum + xins) << 4;

			Contribution2 c = lookup2D[hash];

			double value = 0.0;
			while (c != null)
			{
				double dx = dx0 + c.dx;
				double dy = dy0 + c.dy;
				double attn = 2 - dx * dx - dy * dy;
				if (attn > 0)
				{
					int px = xsb + c.xsb;
					int py = ysb + c.ysb;

					byte i = perm2D[(perm[px & 0xFF] + py) & 0xFF];
					double valuePart = gradients2D[i] * dx + gradients2D[i + 1] * dy;

					attn *= attn;
					value += attn * attn * valuePart;
				}
				c = c.Next;
			}
			return value * NORM_2D;
		}

		public double Evaluate(double x, double y, double z)
		{
			var stretchOffset = (x + y + z) * STRETCH_3D;
			var xs = x + stretchOffset;
			var ys = y + stretchOffset;
			var zs = z + stretchOffset;

			var xsb = FastFloor(xs);
			var ysb = FastFloor(ys);
			var zsb = FastFloor(zs);

			var squishOffset = (xsb + ysb + zsb) * SQUISH_3D;
			var dx0 = x - (xsb + squishOffset);
			var dy0 = y - (ysb + squishOffset);
			var dz0 = z - (zsb + squishOffset);

			var xins = xs - xsb;
			var yins = ys - ysb;
			var zins = zs - zsb;

			var inSum = xins + yins + zins;

			var hash =
			   (int)(yins - zins + 1) |
			   (int)(xins - yins + 1) << 1 |
			   (int)(xins - zins + 1) << 2 |
			   (int)inSum << 3 |
			   (int)(inSum + zins) << 5 |
			   (int)(inSum + yins) << 7 |
			   (int)(inSum + xins) << 9;

			var c = lookup3D[hash];

			var value = 0.0;
			while (c != null)
			{
				var dx = dx0 + c.dx;
				var dy = dy0 + c.dy;
				var dz = dz0 + c.dz;
				var attn = 2 - dx * dx - dy * dy - dz * dz;
				if (attn > 0)
				{
					var px = xsb + c.xsb;
					var py = ysb + c.ysb;
					var pz = zsb + c.zsb;

					var i = perm3D[(perm[(perm[px & 0xFF] + py) & 0xFF] + pz) & 0xFF];
					var valuePart = gradients3D[i] * dx + gradients3D[i + 1] * dy + gradients3D[i + 2] * dz;

					attn *= attn;
					value += attn * attn * valuePart;
				}

				c = c.Next;
			}
			return value * NORM_3D;
		}

		public double Evaluate(double x, double y, double z, double w)
		{
			var stretchOffset = (x + y + z + w) * STRETCH_4D;
			var xs = x + stretchOffset;
			var ys = y + stretchOffset;
			var zs = z + stretchOffset;
			var ws = w + stretchOffset;

			var xsb = FastFloor(xs);
			var ysb = FastFloor(ys);
			var zsb = FastFloor(zs);
			var wsb = FastFloor(ws);

			var squishOffset = (xsb + ysb + zsb + wsb) * SQUISH_4D;
			var dx0 = x - (xsb + squishOffset);
			var dy0 = y - (ysb + squishOffset);
			var dz0 = z - (zsb + squishOffset);
			var dw0 = w - (wsb + squishOffset);

			var xins = xs - xsb;
			var yins = ys - ysb;
			var zins = zs - zsb;
			var wins = ws - wsb;

			var inSum = xins + yins + zins + wins;

			var hash =
				(int)(zins - wins + 1) |
				(int)(yins - zins + 1) << 1 |
				(int)(yins - wins + 1) << 2 |
				(int)(xins - yins + 1) << 3 |
				(int)(xins - zins + 1) << 4 |
				(int)(xins - wins + 1) << 5 |
				(int)inSum << 6 |
				(int)(inSum + wins) << 8 |
				(int)(inSum + zins) << 11 |
				(int)(inSum + yins) << 14 |
				(int)(inSum + xins) << 17;

			var c = lookup4D[hash];

			var value = 0.0;
			while (c != null)
			{
				var dx = dx0 + c.dx;
				var dy = dy0 + c.dy;
				var dz = dz0 + c.dz;
				var dw = dw0 + c.dw;
				var attn = 2 - dx * dx - dy * dy - dz * dz - dw * dw;
				if (attn > 0)
				{
					var px = xsb + c.xsb;
					var py = ysb + c.ysb;
					var pz = zsb + c.zsb;
					var pw = wsb + c.wsb;

					var i = perm4D[(perm[(perm[(perm[px & 0xFF] + py) & 0xFF] + pz) & 0xFF] + pw) & 0xFF];
					var valuePart = gradients4D[i] * dx + gradients4D[i + 1] * dy + gradients4D[i + 2] * dz + gradients4D[i + 3] * dw;

					attn *= attn;
					value += attn * attn * valuePart;
				}

				c = c.Next;
			}
			return value * NORM_4D;
		}

		public double OctaveEvaluate(double x, double y, int octaves, double persistence)
		{
			double total = 0;
			double frequency = 1;
			double amplitude = 1;
			double maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
			for (int i = 0; i < octaves; i++)
			{
				total += Evaluate((int)(x * frequency), (int)(y * frequency)) * amplitude;

				maxValue += amplitude;

				amplitude *= persistence;
				frequency *= 2;
			}

			return total / maxValue;
		}

		public double OctaveEvaluate(double x, double y, double z, int octaves, double persistence)
		{
			double total = 0;
			double frequency = 1;
			double amplitude = 1;
			double maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
			for (int i = 0; i < octaves; i++)
			{
				total += Evaluate(x * frequency, y * frequency, z * frequency) * amplitude;

				maxValue += amplitude;

				amplitude *= persistence;
				frequency *= 2;
			}

			return total / maxValue;
		}

		public double OctaveEvaluate(double x, double y, double z, double w, int octaves, double persistence)
		{
			double total = 0;
			double frequency = 1;
			double amplitude = 1;
			double maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
			for (int i = 0; i < octaves; i++)
			{
				total += Evaluate(x * frequency, y * frequency, z * frequency, w * frequency) * amplitude;

				maxValue += amplitude;

				amplitude *= persistence;
				frequency *= 2;
			}

			return total / maxValue;
		}

		private class Contribution2
		{
			public double dx, dy;
			public int xsb, ysb;
			public Contribution2 Next;

			public Contribution2(double multiplier, int xsb, int ysb)
			{
				dx = -xsb - multiplier * SQUISH_2D;
				dy = -ysb - multiplier * SQUISH_2D;
				this.xsb = xsb;
				this.ysb = ysb;
			}
		}

		private class Contribution3
		{
			public double dx, dy, dz;
			public int xsb, ysb, zsb;
			public Contribution3 Next;

			public Contribution3(double multiplier, int xsb, int ysb, int zsb)
			{
				dx = -xsb - multiplier * SQUISH_3D;
				dy = -ysb - multiplier * SQUISH_3D;
				dz = -zsb - multiplier * SQUISH_3D;
				this.xsb = xsb;
				this.ysb = ysb;
				this.zsb = zsb;
			}
		}

		private class Contribution4
		{
			public double dx, dy, dz, dw;
			public int xsb, ysb, zsb, wsb;
			public Contribution4 Next;

			public Contribution4(double multiplier, int xsb, int ysb, int zsb, int wsb)
			{
				dx = -xsb - multiplier * SQUISH_4D;
				dy = -ysb - multiplier * SQUISH_4D;
				dz = -zsb - multiplier * SQUISH_4D;
				dw = -wsb - multiplier * SQUISH_4D;
				this.xsb = xsb;
				this.ysb = ysb;
				this.zsb = zsb;
				this.wsb = wsb;
			}
		}
	}
}
