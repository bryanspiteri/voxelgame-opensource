using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;
using VoxelGame.Client;
using VoxelGame.Util.Math;

namespace VoxelGame.Items
{
	public class BlockItem : Item
	{
		public BlockItem(Block source)
		{
			base.Title = source.Title;
			base.Id = source.Id;
			base.itemModel = source.BlockModel;
			//Initialise the yaw pitch roll
			base.PreviewViewMatrix = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.Up);
			base.PreviewWorldMatrix = Matrix.Identity
					* Matrix.CreateScale(0.625f) //scale
					* Matrix.CreateRotationY(-MathHelper.PiOver4) //225 deg in -45 deg out
					* Matrix.CreateRotationX(MathHelper.ToRadians(30)) //30 deg in //Mul translation Mul rotZ
					* Matrix.CreateScale(new Vector3(64, 64, 1))
					* Matrix.CreateTranslation(new Vector3(-32, 32, 0))
					* Matrix.CreateTranslation(new Vector3(32, -32, 0));
		}

		public override void OnUse(Entity holder, Vector3Int useLocation, Vector3Int useLocationPlace, params object[] extra)
		{
			//TODO: IsSneaking?
			if (GameClient.theWorld.GetBlockAt(useLocation).Interactable)
			{
				GameClient.theWorld.GetBlockAt(useLocation).OnInteract(useLocation, holder);
			}
			else
			{
				//place block
				GameClient.theWorld.SetBlockAt(useLocationPlace, Id, (byte)extra[0]);
			}
		}
	}
}
