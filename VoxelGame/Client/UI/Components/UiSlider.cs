using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Util;
using VoxelGame.Util.Math;

namespace VoxelGame.Client
{
	public class UiSlider : Component
	{
		private string _text = "0";
		private Vector2 _size = Vector2.Zero;

		#region Slider Properties
		private int _value = 0;
		private int _offset = 0;
		private int _minValue = 0;
		private int _maxValue = 0;

		public int Value
		{
			get
			{
				return _value + _offset;
			}
			set
			{
				_value = value - _offset;
			}
		}
		public int MinValue
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _maxValue + _offset;
			}
			set
			{
				_maxValue = value - _offset;
			}
		}
		#endregion

		/// <summary>
		/// Handle Width is the width of the slider handle in texture pixels (Screen pixels / UIScale)
		/// </summary>
		public int HandleWidth = 4;

		//TODO: Controller support
		public bool enabled = true;
		public bool languageText = true;
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

		public bool isHovered = false;
		public bool isClicked = false;
		public bool isSliding = false;

		private int handleX = 0;

		/// <summary>
		/// Called whenever the user hovers on the button
		/// </summary>
		public event EventHandler OnHover;
		/// <summary>
		/// Called whenever the user releases the left mouse button
		/// </summary>
		public event EventHandler OnClicked;
		public event EventHandler OnReleased;
		/// <summary>
		/// Called when the value changes
		/// </summary>
		public event EventHandler OnValueChanged;

		/// <summary>
		/// The mouse position relative to the slider handle's top left most pixel
		/// </summary>
		private Point MouseOffset = Point.Zero;

		public override void getTextures()
		{

		}

		public UiSlider(GuiScreen parent, string text, int x, int y, int sX, int sY) : base(parent, x, y, sX, sY)
		{
			location = new Point(x, y);
			size = new Point(sX, sY);
			this.text = text;
			selectable = true;
		}

		public UiSlider(GuiScreen parent, string text, Point location, Point size) : base(parent, location, size)
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
				//hovering logic (checks if the slider is hovered)
				if (isMouseHovering() && selectable && enabled)
				{
					Parent.FocusedComponent = this;

					if (isMouseHovering(new Point(FastMath.FastClamp(location.X + (_value * size.X / (_maxValue - _minValue)), location.X, location.X + size.X - HandleWidth * GameSettings.UIScale), location.Y), new Point(HandleWidth * GameSettings.UIScale, size.Y)))
					{
						if (isHovered == false)
						{
							isHovered = true;
							if (OnHover != null)
							{
								OnHover.Invoke(null, null);
							}
						}
					}

					if (InputManager.IsPressed("gui.click"))
					{
						if (isClicked == false)
						{
							isClicked = true;
							if (OnClicked != null)
							{
								OnClicked.Invoke(null, null);
							}
						}
					}

					if (InputManager.Released("gui.click"))
					{
						if (isClicked && Parent.FocusedComponent != null && Parent.FocusedComponent == this)
						{
							isClicked = false;
							if (OnReleased != null)
							{
								OnReleased.Invoke(null, null);
							}
						}
					}

					if (isClicked)
					{
						if (isSliding == false)
						{
							//If we are hovering on the handle then:
							if (isMouseHovering(new Point(FastMath.FastClamp(location.X + (_value * size.X / (_maxValue - _minValue)), location.X, location.X + size.X - HandleWidth * GameSettings.UIScale), location.Y), new Point(HandleWidth * GameSettings.UIScale, size.Y)))
							{
								MouseOffset = new Point(handleX - InputManager.MouseX, location.Y - InputManager.MouseY);
							}
							//We are not hovering on the handle, so assume its half the width
							else
							{
								MouseOffset = new Point(-HandleWidth * GameSettings.UIScale / 2, location.Y - InputManager.MouseY);
							}
							isSliding = true;
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
							if (OnClicked != null)
							{
								OnClicked.Invoke(null, null);
							}
							if (OnReleased != null)
							{
								OnReleased.Invoke(null, null);
							}
						}
					}
				}

				if (isClicked == false || Parent.FocusedComponent != this)
				{
					isSliding = false;
				}

				//Handle the slider position
				if (isSliding)
				{
					//Get the mouse relative to the component
					int localPosition = InputManager.MouseX - location.X;

					float coef = (float)(_maxValue) / (float)(size.X - 3);
					int value = (int)(localPosition * coef);
					value = FastMath.FastClamp(value, 0, _maxValue);

					if (value != _value)
					{
						_value = value;

						if (OnValueChanged != null)
						{
							OnValueChanged.Invoke(null, null);
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

				UiTexture img = TextureManager.GetUiUv("button_disabled");
				UiStateManager.Draw9SplicedButton(img, this);

				//Draw the button
				if (enabled)
				{
					img = TextureManager.GetUiUv("button_neutral");
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
					if (isSliding)
					{
						UiStateManager.Draw9SplicedButton(img, new Point(HandleWidth * GameSettings.UIScale, size.Y), new Point(FastMath.FastClamp(InputManager.MouseX + MouseOffset.X, location.X, location.X + size.X - HandleWidth * GameSettings.UIScale), location.Y));
					}
					else
					{
						//location.X = component.location.X + % of slider value * component.size.X
						handleX = FastMath.FastClamp(location.X + (_value * size.X / (_maxValue - _minValue)), location.X, location.X + size.X - HandleWidth * GameSettings.UIScale);
						UiStateManager.Draw9SplicedButton(img, new Point(HandleWidth * GameSettings.UIScale, size.Y), new Point(handleX, location.Y));
					}
					UiStateManager.DrawText(location.X + size.X / 2 - _size.X / 2, location.Y + size.Y / 2 - _size.Y / 2, buttonColor, text);
				}
				else
				{
					UiStateManager.Draw9SplicedButton(img, new Point(HandleWidth * GameSettings.UIScale, size.Y), new Point(FastMath.FastClamp(location.X + (_value * size.X / (_maxValue - _minValue)), location.X, location.X + size.X - HandleWidth * GameSettings.UIScale), location.Y));
					UiStateManager.DrawText(location.X + size.X / 2 - _size.X / 2, location.Y + size.Y / 2 - _size.Y / 2, buttonDisabledColor, text);
				}
			}
		}
	}
}
