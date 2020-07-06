using System;
using VoxelGame;
using VoxelGame.Util.Math;

namespace Microsoft.Xna.Framework
{
	public static class BoundingBoxExtensions
	{
		public static BoundingBox Offset(this BoundingBox box, Vector3 Offset)
		{
			return new BoundingBox(
				box.Min + Offset,
				box.Max + Offset);
		}

		public static BoundingBox Offset(this BoundingBox box, Vector3Int Offset)
		{
			return new BoundingBox(
				new Vector3(box.Min.X + Offset.X, box.Min.Y + Offset.Y, box.Min.Z + Offset.Z),
				new Vector3(box.Max.X + Offset.X, box.Max.Y + Offset.Y, box.Max.Z + Offset.Z));
		}

		/// <summary>
		/// Returns a BoundingBox of size Size, such that it's center is the origin
		/// </summary>
		/// <param name="Size">The size of the BoundingBox</param>
		/// <returns>The BoundingBox of the specified size</returns>
		public static BoundingBox FromSize(Vector3 Size)
		{
			return new BoundingBox(
				new Vector3(-Size.X / 2, -Size.Y / 2, -Size.Z / 2),
				new Vector3(Size.X / 2, Size.Y / 2, Size.Z / 2)
				);
		}

		/// <summary>
		/// Returns a BoundingBox of size X, Y, Z, such that is center is the origin along X and Z, but its Y above the origin
		/// </summary>
		/// <param name="X">The X coordinate</param>
		/// <param name="Y">The Y coordinate</param>
		/// <param name="Z">The Z coordinate</param>
		/// <returns>The BoundingBox of the specified size</returns>
		public static BoundingBox FromSize(float X, float Y, float Z)
		{
			return new BoundingBox(
				new Vector3(-X / 2, 0, -Z / 2),
				new Vector3(X / 2, Y, Z / 2)
				);
		}

		public static bool IntersectsX(this BoundingBox thisBox, BoundingBox otherBox, out float IntersectionDistance)
		{
			return IntersectsAxis(thisBox, otherBox, thisBox.Min.X, thisBox.Max.X, otherBox.Min.X, otherBox.Max.X, out IntersectionDistance);
		}

		public static bool IntersectsY(this BoundingBox thisBox, BoundingBox otherBox, out float IntersectionDistance)
		{
			return IntersectsAxis(thisBox, otherBox, thisBox.Min.Y, thisBox.Max.Y, otherBox.Min.Y, otherBox.Max.Y, out IntersectionDistance);
		}

		public static bool IntersectsZ(this BoundingBox thisBox, BoundingBox otherBox, out float IntersectionDistance)
		{
			return IntersectsAxis(thisBox, otherBox, thisBox.Min.Z, thisBox.Max.Z, otherBox.Min.Z, otherBox.Max.Z, out IntersectionDistance);
		}

		public static bool IntersectsAxis(BoundingBox thisBox, BoundingBox otherBox, float thisMin, float thisMax, float otherMin, float otherMax, out float IntersectionDistance)
		{
			IntersectionDistance = 0;

			//Intersection
			if (thisMax > otherMin)
			{
				//IntersectionDistance = (thisMax + thisMin) / 2f + otherMax - thisMin;
				IntersectionDistance = -(thisMax - otherMin);
			}

			if (thisMin < otherMax)
			{
				//IntersectionDistance = (thisMax + thisMin) / 2f - thisMax + otherMin;
				IntersectionDistance = otherMax - thisMin;
			}

			return thisBox.Intersects(otherBox);
		}
	}
}
