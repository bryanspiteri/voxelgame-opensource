using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;

namespace VoxelGame.Client
{
	public class UiImage : Component
	{
		public Texture2D image;
		public bool enabled
		{
			get { return render; }
			set { render = value; }
		}

		/// <summary>
		/// The UV mapping of this image
		/// </summary>
		public Vector4 UV = new Vector4(0, 0, 1, 1);

		public Color imageColor = Color.White;

		public override void getTextures()
		{
			//get the asset from ui atlas
		}

		#region Constructor
		public UiImage(GuiScreen parent, int x, int y, int sX, int sY, string ImageResourceLocation) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			image = TextureLoader.loadTexture(ImageResourceLocation);
		}

		public UiImage(GuiScreen parent, Point location, Point size, string ImageResourceLocation) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			image = TextureLoader.loadTexture(ImageResourceLocation);
		}

		public UiImage(GuiScreen parent, int x, int y, int sX, int sY) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			image = TextureLoader.getWhitePixel();
		}

		public UiImage(GuiScreen parent, Point location, Point size) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			image = TextureLoader.getWhitePixel();
		}

		public UiImage(GuiScreen parent, int x, int y, int sX, int sY, string ImageResourceLocation, Color col) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			image = TextureLoader.loadTexture(ImageResourceLocation);
			imageColor = col;
		}

		public UiImage(GuiScreen parent, Point location, Point size, string ImageResourceLocation, Color col) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			image = TextureLoader.loadTexture(ImageResourceLocation);
			imageColor = col;
		}

		public UiImage(GuiScreen parent, int x, int y, int sX, int sY, Color col) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			imageColor = col;
			image = TextureLoader.getWhitePixel();
		}

		public UiImage(GuiScreen parent, Point location, Point size, Color col) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			imageColor = col;
			image = TextureLoader.getWhitePixel();
		}
		#endregion

		public override void onUpdate()
		{
			rawSize = new Point((int)((UV.Z - UV.X) * image.Width), (int)((UV.W - UV.Y) * image.Height));

			if (screenVisible && render)
			{
				//hovering logic
				if (isMouseHovering() && selectable && enabled)
				{
					Parent.FocusedComponent = this;
				}
			}
		}

		public override void Draw()
		{
			Rectangle source = new Rectangle((int)(UV.X * image.Width), (int)(UV.Y * image.Height), (int)((UV.Z - UV.X) * image.Width), (int)((UV.W - UV.Y) * image.Height));
			
			if (size != Point.Zero)
			{
				UiStateManager.DrawImage(new Rectangle(location.X, location.Y, size.X, size.Y), source, image, imageColor);
			}
			else
			{
				UiStateManager.DrawImage(new Rectangle(location.X, location.Y, image.Width, image.Height), source, image, imageColor);
			}
		}

	}
}
