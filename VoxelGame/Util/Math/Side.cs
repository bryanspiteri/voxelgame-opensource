using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Math
{
	public enum Side
	{
		DOWN = 0,
		UP = 1,
		NORTH = 2,
		SOUTH = 3,
		WEST = 4,
		EAST = 5
	}

	public static class SideExtensions
	{
		/// <summary>
		/// Gets the offset of the block in front of the EnumFacing's side
		/// </summary>
		/// <param name="face"></param>
		/// <returns></returns>
		public static Vector3Int GetOffset(this Side face)
		{
			switch (face)
			{
				case Side.WEST:
					return new Vector3Int(0, 0, -1);
				case Side.EAST:
					return new Vector3Int(0, 0, 1);
				case Side.DOWN:
					return new Vector3Int(0, -1, 0);
				case Side.UP:
					return new Vector3Int(0, 1, 0);
				case Side.NORTH:
					return new Vector3Int(1, 0, 0);
				case Side.SOUTH:
					return new Vector3Int(-1, 0, 0);
			}
			return Vector3Int.Zero;
		}


		public static string ToFriendlyString(this Side face)
		{
			switch (face)
			{
				case Side.UP:
					return "Up";
				case Side.DOWN:
					return "Down";
				case Side.NORTH:
					return "North";
				case Side.SOUTH:
					return "South";
				case Side.WEST:
					return "West";
				case Side.EAST:
					return "East";
			}
			return "Unknown";
		}
	}
}
