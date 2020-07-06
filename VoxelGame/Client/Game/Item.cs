using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.Client.Renderers;

namespace VoxelGame.Items
{
	public class Item
	{
		public string Title;
		public uint Id = 0;
		public Texture2D InventoryGraphic;

		public BlockModel itemModel;
		/// <summary>
		/// The view matrix used when rendering item previews
		/// </summary>
		public Matrix PreviewViewMatrix = Matrix.Identity, PreviewWorldMatrix = Matrix.Identity;
		//TODO: Animations

		public void CacheTexture()
		{
			//Caches the texture of the item into InventoryGraphic
			InventoryGraphic = ItemRenderer.Render(itemModel, PreviewViewMatrix, PreviewWorldMatrix);
		}

		public virtual void OnPickup(Entity holder)
		{

		}

		public virtual void OnDrop(Entity holder)
		{
			//Spawn the item stack

		}

		public virtual void OnUse(Entity holder, Vector3Int useLocation, Vector3Int useLocationPlace, params object[] extra)
		{

		}
	}
}
