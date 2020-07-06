using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace VoxelGame.Engine
{
    public struct BlockTextures
    {
        public Vector4 north;
        public Vector4 south;
        public Vector4 east;
        public Vector4 west;
        public Vector4 up;
        public Vector4 down;

        public BlockTextures(Vector4 sides)
        {
            north = sides;
            south = sides;
            east = sides;
            west = sides;
            up = sides;
            down = sides;
        }

        public BlockTextures(Vector4 top, Vector4 sides)
        {
            north = sides;
            south = sides;
            east = sides;
            west = sides;
            up = top;
            down = sides;
        }

        public BlockTextures(Vector4 top, Vector4 sides, Vector4 front)
        {
            north = front;
            south = sides;
            east = sides;
            west = sides;
            up = top;
            down = sides;
        }

        public BlockTextures(Vector4 north, Vector4 south, Vector4 east, Vector4 west, Vector4 up, Vector4 down)
        {
            this.north = north;
            this.south = south;
            this.east = east;
            this.west = west;
            this.up = up;
            this.down = down;
        }

        public static bool operator ==(BlockTextures lhs, BlockTextures rhs)
            => lhs.north == rhs.north && lhs.south == rhs.south && lhs.east == rhs.east && lhs.west == rhs.west && lhs.up == rhs.up && lhs.down == rhs.down;


        public static bool operator !=(BlockTextures lhs, BlockTextures rhs)
            => !(lhs == rhs);

        public override bool Equals(object obj)
        {
            if (obj is BlockTextures)
            {
                return this == (BlockTextures)obj;
            }

            return false;
        }

        public override int GetHashCode() => new { north, south, east, west, up, down }.GetHashCode();
    }
}
