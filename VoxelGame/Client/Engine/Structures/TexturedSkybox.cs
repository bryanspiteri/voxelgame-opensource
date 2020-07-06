using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VoxelGame.Engine
{
    public class TexturedSkybox : Skybox
    {
        public static new Color skycolor = Color.Black;
        public static BlockTextures textures;

        public TexturedSkybox(BlockTextures textures)
        {

        }
    }
}
