using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;
using VoxelGame.Renderers;

namespace VoxelGame.Client
{
	public class BlockModel
	{
		public Voxel[] voxels;

		public BlockModel(Voxel[] voxels)
		{
			this.voxels = voxels;
		}

		public void Render(Vector3 position)
		{
			//Loop through the voxels
			for (int i = 0; i < voxels.Length; i++)
			{
				//Pass the voxels to the voxel renderer
				VoxelRenderer.RenderVoxel(voxels[i]);
			}
		}
	}
}
