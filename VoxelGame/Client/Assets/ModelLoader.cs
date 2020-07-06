using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;

namespace VoxelGame.Client
{
    public class ModelLoader
    {
        public static BlockModel loadBlockModel(string modelLocation)
        {
            //TODO: Get the model definition
            Voxel fullCube = new Voxel(Vector3Int.Zero, new Vector3Int(1, 1, 1), new BlockTextures());
            Voxel[] voxArr = { fullCube };
            BlockModel model = new BlockModel(voxArr);
            return model;
        }
    }
}
