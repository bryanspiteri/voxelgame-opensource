using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VoxelGame.Client
{
	public class Component
	{
		public Point location;
		public Point size;
		public Point rawSize;
		public bool selectable = false;
		public bool render = true;
		public string name = "";
		/// <summary>
		/// Returns whether the GuiScreen this component is attached to is enabled
		/// </summary>
		public bool screenVisible = true;
		public Texture2D texture;
		public int zIndex = 0;

		public GuiScreen Parent;

		public virtual void getTextures()
		{

		}

		public Component(GuiScreen parent, int x, int y, int sX, int sY)
		{
			this.Parent = parent;
			location = new Point(x, y);
			size = new Point(sX, sY);
			rawSize = size;
		}

		public Component(GuiScreen parent, Point location, Point size)
		{
			this.Parent = parent;
			this.location = location;
			this.size = size;
			rawSize = size;
		}

		/// <summary>
		/// Returns a boolean value stating whether the mouse cursor is currently hovering on this Component.
		/// </summary>
		/// <returns></returns>
		public bool isMouseHovering()
		{
			Point MouseLoc = Mouse.GetState().Position;
			if (MouseLoc.X > location.X && MouseLoc.X < (location.X + size.X) //X
				&& MouseLoc.Y > location.Y && MouseLoc.Y < (location.Y + size.Y)) //Y
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns a boolean value stating whether the mouse cursor is currently hovering on this Component.
		/// </summary>
		/// <returns></returns>
		public bool isMouseHovering(Point location, Point size)
		{
			Point MouseLoc = Mouse.GetState().Position;
			if (MouseLoc.X > location.X && MouseLoc.X < (location.X + size.X) //X
				&& MouseLoc.Y > location.Y && MouseLoc.Y < (location.Y + size.Y)) //Y
			{
				return true;
			}
			return false;
		}

		public Texture2D getRenderTexture()
		{
			return texture;
		}

		public virtual void onUpdate()
		{

		}

		public virtual void Draw()
		{

		}
	}
}
