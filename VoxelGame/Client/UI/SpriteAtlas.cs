using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame;

namespace Launcher.Client
{
    public class SpriteAtlas
    {
        public Texture2D atlas;
        public List<AtlasTexture> atlasTexs = new List<AtlasTexture>();

        public SpriteAtlas(string directory)
        {

        }

        public Texture2D Get(string textureName)
        {
            AtlasTexture tx= new AtlasTexture("",Point.Zero,Point.Zero);
            foreach (AtlasTexture atlt in atlasTexs) {
                if (atlt.name==textureName) {
                    tx = atlt;
                    break;
                }
            }
            Texture2D outImg = new Texture2D(GameClient.Graphics.GraphicsDevice,tx.size.X,tx.size.Y);
            return null;
        }
    }

    public class AtlasTexture
    {
        public Point location;
        public Point size;
        public string name;

        public AtlasTexture(string name, Point location, Point size)
        {
            this.location = location;
            this.size = size;
            this.name = name;
        }
    }
}
