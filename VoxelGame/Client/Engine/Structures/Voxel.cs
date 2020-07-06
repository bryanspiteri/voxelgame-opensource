using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Engine
{
    public enum BlockFace
    {
        North,
        South,
        East,
        West,
        Up,
        Down,
    }

    public class Voxel
    {
        public Vector3Int size;
        public BlockTextures textures;
        public Vector3Int position;
        public Vector3 rotation;
        public bool[] renderSides = new bool[6];

        public bool RenderSide(BlockFace check)
        {
            return false;
        }

        public Voxel(Vector3Int position, Vector3Int size, BlockTextures textures)
        {
            this.position = position;
            this.textures = textures;
            this.size = size;
            rotation = Vector3.Zero;
        }

        public Voxel(Vector3Int position, Vector3Int size, BlockTextures textures, Vector3 rotation)
        {
            this.position = position;
            this.textures = textures;
            this.size = size;
            this.rotation = rotation;
        }

        public void Initialise()
        {

        }

        public void BeforeUpdate()
        {

        }

        public void Update()
        {

        }
    }
}
