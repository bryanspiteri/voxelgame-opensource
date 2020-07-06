using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	public class UiButton : Component
	{
		private string _text = "menu.singleplayer";
		private Vector2 _size = Vector2.Zero;

		//TODO: Controller support
		public bool enabled = true;
		public string text
		{
			get
			{
				if (languageText)
				{
					return LanguageHelper.GetLanguageString(_text);
				}
				else
				{
					return _text;
				}
			}
			set
			{
				_text = value;
			}
		}
		public Color buttonColor = Color.White;
		public Color buttonDisabledColor = Color.LightGray;
		public bool languageText = true;

		public bool isHovered = false;
		public bool isClicked = false;

		/// <summary>
		/// Called whenever the user hovers on the button
		/// </summary>
		public event EventHandler OnHover;
		/// <summary>
		/// Called whenever the user releases the left mouse button
		/// </summary>
		public event EventHandler OnClicked;
		public event EventHandler OnReleased;

		public override void getTextures()
		{

		}

		public UiButton(GuiScreen parent, string text, int x, int y, int sX, int sY) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			this.text = text;
			selectable = true;
		}

		public UiButton(GuiScreen parent, string text, Point location, Point size) : base(parent, location, size)
		{
			this.location = location;
			this.size = size;
			this.text = text;
			selectable = true;
		}

		public override void onUpdate()
		{
			//only handle the update if the button is enabled and the parent screen is visible
			if (screenVisible && render)
			{
				//hovering logic
				if (isMouseHovering() && selectable && enabled)
				{
					Parent.FocusedComponent = this;

					if (isHovered == false)
					{
						isHovered = true;
						if (OnHover != null)
						{
							OnHover.Invoke(this, null);
						}
					}

					if (InputManager.IsPressed("gui.click"))
					{
						if (isClicked == false)
						{
							isClicked = true;
						}
					}

					if (InputManager.Released("gui.click"))
					{
						if (isClicked && Parent.FocusedComponent != null && Parent.FocusedComponent == this)
						{
							isClicked = false;
							if (OnClicked != null)
							{
								OnClicked.Invoke(this, null);
							}
						}
					}
				}
				//unhover logic
				else
				{
					if (isHovered)
					{
						isHovered = false;
					}
					if (!InputManager.IsPressed("gui.click"))
					{
						if (isClicked == true)
						{
							isClicked = false;
							if (OnReleased != null)
							{
								OnReleased.Invoke(this, null);
							}
						}
					}
				}
			}
		}

		public override void Draw()
		{
			if (base.screenVisible)
			{
				_size = UiStateManager.MeasureString(text);

				//Draw the button
				if (enabled)
				{
					UiTexture img = TextureManager.GetUiUv("button_neutral");
					if (isHovered && Parent.FocusedComponent != null && Parent.FocusedComponent == this)
					{
						if (isClicked)
						{
							img = TextureManager.GetUiUv("button_clicked");
						}
						else
						{
							img = TextureManager.GetUiUv("button_hover");
						}
					}
					UiStateManager.Draw9SplicedButton(img, this);
					UiStateManager.DrawText(location.X + size.X / 2 - _size.X / 2, location.Y + size.Y / 2 - _size.Y / 2, buttonColor, text);
				}
				else
				{
					UiTexture img = TextureManager.GetUiUv("button_disabled");
					UiStateManager.Draw9SplicedButton(img, this);
					UiStateManager.DrawText(location.X + size.X / 2 - _size.X / 2, location.Y + size.Y / 2 - _size.Y / 2, buttonDisabledColor, text);
				}
			}
		}
	}
}
