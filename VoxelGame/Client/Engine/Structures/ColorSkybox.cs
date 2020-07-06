using Microsoft.Xna.Framework;
using System;

namespace VoxelGame.Engine
{
    public class ColorSkybox : Skybox
    {
        public Color skycolor = Color.Black;

        public ColorSkybox(Color skycolor)
        {
            this.skycolor = skycolor;
        }
    }
}
