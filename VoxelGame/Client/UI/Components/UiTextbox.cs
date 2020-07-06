using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using VoxelGame.Util.Math;
using VoxelGame.Util;
using VoxelGame.Assets;

using MonoBMFont;
using System;

namespace VoxelGame.Client
{
	public class UiTextbox : Component
	{
		#region Variables
		private string _placeholderText = "";
		private string _text = "";
		private char[] characters;

		private int _caret = 0;
		private int _select = -1;
		private float caretTimer = 0f;
		private float timingMachine = 0f;
		private float previousClickTime = 0f;
		private Point prevMouseLocation = Point.Zero;
		private int selectedCaretPositionTMP = -1;
		private float timingChars = 0f;

		private Component previousComponent;

		#endregion

		#region Properties 

		#region Constants
		/// <summary>
		/// The amount in seconds between each blink of the caret
		/// </summary>
		public const float caretAnimationTime = 1f;
		#endregion

		#region Appearance
		public Color placeholderColor = Color.Gray;
		public Color textColor = Color.White;
		public Color HighlightColor = Color.Blue;

		public int paddingX = 4;
		public int paddingY = 4;

		public string placeholderText
		{
			get
			{
				if (isLanguageText)
				{
					return LanguageHelper.GetLanguageString(_placeholderText);
				}
				else
				{
					return _placeholderText;
				}
			}
			set
			{
				_placeholderText = value;
			}
		}
		public string text
		{
			get
			{
				return _text;
			}
			set
			{
				//Max characters
				if (value.Length > maxCharacters)
				{
					value = value.Substring(0, maxCharacters);
				}

				//Set the text
				_text = value;
				characters = value.ToCharArray();

				if (OnValueChanged != null)
				{
					OnValueChanged.Invoke(null, null);
				}
			}
		}
		#endregion

		#region Behavior
		/// <summary>
		/// The index of the position of the selected text
		/// </summary>
		public int scrollPosition = 0;
		public bool isLanguageText = false;

		public bool enabled = true;
		public bool isHovered = false;
		public bool isClicked = false;
		public bool isEditing = false;

		public int maxCharacters = 1024;

		public int caretPosition
		{
			get
			{
				return _caret;
			}
			set
			{
				//_caret = value;
				_caret = FastMath.FastClamp(value, 0, text.Length);

				//Calculate the position of the caret on screen, then we will handle scrolling by clipping
				if (_caret - scrollPosition > 0)
				{
					int pos = paddingX * GameSettings.UIScale + (int)UiStateManager.gameFont.MeasureString(text.Substring(scrollPosition, _caret - scrollPosition)).X;

					//For pasting large blocks of text
					while (pos + 2 * GameSettings.UIScale > size.X - paddingX) //out of bounds +X
					{
						scrollPosition++;
						pos = paddingX * GameSettings.UIScale + (int)UiStateManager.gameFont.MeasureString(text.Substring(scrollPosition, _caret - scrollPosition)).X;
					}
				}

				//For deleting large blocks of text?
				while (_caret < scrollPosition) //out of bounds -X
				{
					scrollPosition--;
				}

				//Clamp
				scrollPosition = FastMath.FastClamp(scrollPosition, 0, text.Length);

				if (OnCaretMoved != null)
				{
					OnCaretMoved.Invoke(null, null);
				}
			}
		}
		public int selectedPosition
		{
			get
			{
				return _select;
			}
			set
			{
				_select = value;

				if (OnCaretMoved != null)
				{
					OnCaretMoved.Invoke(null, null);
				}
			}
		}
		#endregion

		#region Events

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
		public event EventHandler OnCaretMoved;

		#endregion
		#endregion

		#region Constructors
		public UiTextbox(GuiScreen parent, int x, int y, int sizeX, int sizeY, string placeholder) : base(parent, x, y, 0, 0)
		{
			location = new Point(x, y);
			size = new Point(sizeX, sizeY);
			rawSize = new Point(sizeX, sizeY);
			this.placeholderText = placeholder;
			this.text = "";

			this.textColor = Color.White;
			this.placeholderColor = Color.Gray;

			selectable = true;
		}

		//Overload
		public UiTextbox(GuiScreen parent, Point location, Point size, string placeholder) : this(parent, location.X, location.Y, size.X, size.Y, placeholder)
		{
		}

		#endregion

		#region Update
		public override void onUpdate()
		{
			//only handle the update if the button is enabled and the parent screen is visible
			if (screenVisible && render)
			{
				#region Mouse Logic
				//hovering logic (checks if the slider is hovered)
				if (isMouseHovering() && selectable && enabled)
				{
					var prevPosition = caretPosition;
					Mouse.SetCursor(MouseCursor.IBeam);

					Parent.FocusedComponent = this;

					if (isHovered == false)
					{
						isHovered = true;
						if (OnHover != null)
						{
							OnHover.Invoke(null, null);
						}
					}

					#region Focusing Logic
					if (InputManager.IsPressed("gui.click"))
					{
						if (isClicked == false)
						{
							isClicked = true;
							if (OnClicked != null)
							{
								OnClicked.Invoke(null, null);
							}
							isEditing = true;
						}

						#region Mouse Highlighting Logic
						if (isEditing)
						{
							GetCaretUnderMouse();

							if (selectedCaretPositionTMP == -1)
							{
								selectedCaretPositionTMP = caretPosition;
							}
							if (selectedCaretPositionTMP != caretPosition)
							{
								//is holding
								selectedPosition = selectedCaretPositionTMP;
							}
						}
						#endregion
					}
					#endregion

					#region Double Click Logic
					if (InputManager.Released("gui.click"))
					{
						if (isClicked && Parent.FocusedComponent != null && Parent.FocusedComponent == this)
						{
							isClicked = false;
							if (OnReleased != null)
							{
								OnReleased.Invoke(null, null);
							}

							selectedCaretPositionTMP = -1;
							caretTimer = 0f;

							//empty text boxes shouldnt crash
							if (text.Length > 0)
							{
								//Double click logic
								if (previousClickTime - timingMachine > 0f && previousClickTime - timingMachine <= 0.5f) //Check the time frame
								{
									if (selectedPosition == -1)
									{
										//If we moved more than 5 pixels from the last click in any direction then it doesnt count as a double click
										if (prevMouseLocation.X - 5 <= InputManager.MouseX && prevMouseLocation.X + 5 >= InputManager.MouseX
											&& prevMouseLocation.Y - 5 <= InputManager.MouseY && prevMouseLocation.Y + 5 >= InputManager.MouseY)
										{
											GetCaretUnderMouse();

											//Get the bounds of the current word
											int end = IndexOfNextCharAfterWhitespace();
											caretPosition = end;

											int start = IndexOfLastCharBeforeWhitespace();
											selectedPosition = start;

											timingMachine = 0f;
											previousClickTime = 0f;

										}
									}
									else
									{
										selectedPosition = -1;
									}
								}

								timingMachine = previousClickTime;
							}

							prevMouseLocation = new Point(InputManager.MouseX, InputManager.MouseY);
						}
					}
					#endregion

					if (InputManager.PressedStart("gui.click"))
					{
						selectedPosition = -1;
					}
				}

				#region Unhover Logic
				else
				{
					if (!isMouseHovering() && Parent.FocusedComponent == this)
					{
						Mouse.SetCursor(MouseCursor.Arrow);
					}

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
							caretTimer = 0f;
							isEditing = false;
						}
					}
				}
				#endregion
				#endregion

				caretTimer += Time.DeltaTime;

				#region Losing Focus to Another Component
				if (isEditing)
				{
					previousClickTime += Time.DeltaTime;
				}

				if (Parent.FocusedComponent != this)
				{
					if (Parent.FocusedComponent != null)
					{
						previousComponent = Parent.FocusedComponent;
					}
					else
					{
						isEditing = false;
					}
				}

				//Focusing logic (on click on unfocused = unfocused)
				if (InputManager.Released("gui.click"))
				{
					if (!isMouseHovering())
					{
						isEditing = false;
						timingChars = 0f;
					}
				}
				#endregion

				#region Keyboard Handling
				//Get Textbox input
				if (isEditing)
				{
					bool isShiftPressed = InputManager.IsPressed("misc.shift") || InputManager.IsPressed("misc.shift.alt");
					bool isControlHeld = InputManager.IsPressed("misc.control") || InputManager.IsPressed("misc.control.alt");
					bool isAltHeld = InputManager.IsPressed("misc.alternate") || InputManager.IsPressed("misc.alternate.alt");

					#region Misc Input Handling

					#region Caret Movement
					timingChars += Time.DeltaTime;
					if (InputManager.Released("misc.left") || InputManager.Released("misc.right"))
					{
						timingChars = 0f;
					}

					//Arrow hold timings
					if ((timingChars < 1f && timingChars % 0.5f < 0.1f) //First second delay
						 || (timingChars > 1f && timingChars < 2f && timingChars % 0.25f < 0.1f) //Second second delay
						 || (timingChars > 2f && timingChars < 3f && timingChars % 0.1f < 0.05f)) //Hold delay
					{
						//Movement
						if (InputManager.IsPressed("misc.left") && !isAltHeld) //Left Arrow
						{
							if (isShiftPressed)
							{
								if (selectedPosition == -1)
								{
									selectedPosition = FastMath.FastClamp(caretPosition, 0, text.Length);
								}
							}
							else
							{
								if (selectedPosition != -1)
								{
									caretPosition = Math.Min(caretPosition, selectedPosition);
									selectedPosition = -1;
									caretPosition++;
								}
							}
							if (isControlHeld)
							{
								caretPosition = IndexOfLastCharBeforeWhitespace() + 1;
							}

							caretPosition = FastMath.FastClamp(caretPosition - 1, 0, text.Length); //Minimum position = 0
						}

						if (InputManager.IsPressed("misc.right") && !isAltHeld) //Right Arrow
						{
							if (isShiftPressed)
							{
								if (selectedPosition == -1)
								{
									selectedPosition = FastMath.FastClamp(caretPosition, 0, text.Length);
								}
							}
							else
							{
								if (selectedPosition != -1)
								{
									caretPosition = Math.Max(caretPosition, selectedPosition);
									selectedPosition = -1;
									caretPosition--;
								}
							}
							if (isControlHeld)
							{
								caretPosition = IndexOfNextCharAfterWhitespace() - 1;
							}

							caretPosition = FastMath.FastClamp(caretPosition + 1, 0, text.Length);  //Maximum position = text.Length
						}
					}

					//Home key
					if (InputManager.Released("misc.home"))
					{
						caretPosition = 0;
					}

					//End key
					if (InputManager.Released("misc.end"))
					{
						caretPosition = text.Length;
					}

					#endregion

					#region Clipboard (Copy / Cut / Paste)
					//Control things
					if (isControlHeld)
					{
						//CTRL + A
						if (InputManager.Released("misc.A"))
						{
							selectedPosition = 0;
							caretPosition = text.Length;
						}

						//Clipboard
						if (InputManager.Released("misc.X"))
						{
							if (text.Length > 0)
							{
								if (selectedPosition != -1)
								{
									//Get the selected text
									string toCopy = text.Substring(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition));

									Clipboard.SetText(toCopy);

									//Same logic as backspace / delete
									Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), string.Empty);
									if (selectedPosition < caretPosition)
									{
										caretPosition -= caretPosition - selectedPosition;
									}
									selectedPosition = -1;
								}
							}
						}
						if (InputManager.Released("misc.C"))
						{
							if (text.Length > 0)
							{
								if (selectedPosition != -1)
								{
									int min = Math.Min(selectedPosition, caretPosition);
									int max = Math.Max(selectedPosition, caretPosition);

									//Get the selected text
									string toCopy = text.Substring(min, max - min);
									Clipboard.SetText(toCopy);
								}
							}
						}
						if (InputManager.Released("misc.V"))
						{
							string toAdd = Clipboard.GetText();

							//remove newlines
							toAdd = toAdd.Replace("\n", string.Empty);
							toAdd = toAdd.Replace("\r", string.Empty);
							toAdd = toAdd.Replace("\t", string.Empty);

							//Append the clipboard text
							if (text.Length > 0)
							{
								if (selectedPosition == -1)
								{
									text = text.Substring(0, caretPosition) + toAdd + text.Substring(caretPosition);
									//caretPosition += toAdd.Length;
								}
								else
								{
									Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), toAdd.ToString());
									if (selectedPosition < caretPosition)
									{
										caretPosition -= caretPosition - selectedPosition;
									}
									selectedPosition = -1;
								}
							}
							else
							{
								text += toAdd;
							}

							caretPosition += toAdd.Length;
						}
					}
					#endregion

					//Escape means lose focus
					if (InputManager.Released("misc.pause"))
					{
						//lose focus
						isEditing = false;

						//reset the cursor
						Mouse.SetCursor(MouseCursor.Arrow);

						//reset
						GameClient.CurrentCharacter = (char)0;
						GameClient.CurrentKey = Keys.None;
						return;
					}

					#region Text Removal

					if (GameClient.CurrentKey == Keys.Back) //Backspace
					{
						//If no highlighted text
						if (selectedPosition == -1 || text.Length == 0)
						{
							text = text.Substring(0, Math.Max(0, caretPosition - 1)) + text.Substring(Math.Min(caretPosition, text.Length));
							caretPosition = FastMath.FastClamp(caretPosition, 0, text.Length);
						}
						else
						{
							Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), string.Empty);
							if (selectedPosition < caretPosition)
							{
								caretPosition -= caretPosition - selectedPosition;
							}
							selectedPosition = -1;
						}
					}

					if (GameClient.CurrentKey == Keys.Delete) //Delete?
					{
						//If no highlighted text
						if (selectedPosition == -1)
						{
							text = text.Substring(0, caretPosition) + text.Substring(Math.Min(caretPosition + 1, text.Length));
							caretPosition = FastMath.FastClamp(caretPosition, 0, text.Length);
						}
						else
						{
							Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), string.Empty);
							if (selectedPosition < caretPosition)
							{
								caretPosition -= caretPosition - selectedPosition;
							}
							selectedPosition = -1;
						}
					}
					#endregion
					#endregion

					#region Input Handling
					//Appending text
					if (!(GameClient.CurrentKey == Keys.Back
						|| GameClient.CurrentKey == Keys.Delete
						|| GameClient.CurrentKey == Keys.Escape
						|| GameClient.CurrentKey == Keys.Tab)) //If the current key isn't a printable character
					{
						#region Regular Text Input
						if (GameClient.CurrentKey != Keys.None)
						{
							//Append the character
							if (text.Length > 0)
							{
								if (selectedPosition == -1)
								{
									text = text.Substring(0, caretPosition) + GameClient.CurrentCharacter + text.Substring(caretPosition);
								}
								else
								{
									Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), GameClient.CurrentCharacter.ToString());
									if (selectedPosition < caretPosition)
									{
										caretPosition -= caretPosition - selectedPosition;
									}
									selectedPosition = -1;
								}
							}
							else 
							{
								text += GameClient.CurrentCharacter;
							}

							caretPosition++;
						}
						#endregion
						#region Modifier Characters
						else if (GameClient.CurrentCharacter != '\0')
						{
							char toAdd = GameClient.CurrentCharacter;

							//if uppercase
							if (isShiftPressed)
							{
								toAdd = char.ToUpper(toAdd);
							}

							//Caps Lock
#if DESKTOP
							if (InputManager.IsPressed("misc.capslock"))
							{
								toAdd = char.ToUpper(toAdd);
							}
#endif

							//Append the character
							if (text.Length > 0)
							{
								if (selectedPosition == -1)
								{
									text = text.Substring(0, caretPosition) + toAdd + text.Substring(caretPosition);
								}
								else
								{
									Replace(Math.Min(selectedPosition, caretPosition), Math.Max(selectedPosition, caretPosition), toAdd.ToString());
									if (selectedPosition < caretPosition)
									{
										caretPosition -= caretPosition - selectedPosition;
									}
									selectedPosition = -1;
								}
							}
							else
							{
								text += toAdd;
							}

							caretPosition++;
						}
						#endregion
					}
					#endregion

					//Reset
					GameClient.CurrentCharacter = (char)0;
					GameClient.CurrentKey = Keys.None;
				}
				#endregion
				else
				{
					//Reset highlighting
					selectedPosition = -1;
				}
			}
		}
#endregion

		#region Util Functions
		public void Replace(int start, int end, string replacement)
		{
			Logger.info(-1, "bug hunting (text.replace): s=" + start + ";e=" + end);

			string startStr = text.Substring(0, start);
			string endStr = text.Substring(end);
			string newStr = startStr + replacement + endStr;

			text = newStr;
		}

		public int IndexOfNextCharAfterWhitespace()
		{
			if (caretPosition == text.Length)
			{
				return text.Length;
			}
			else
			{
				char[] chars = text.ToCharArray();
				int pos = caretPosition;

				char c = chars[pos];
				bool whiteSpaceFound = false;
				while (true)
				{
					if (c.Equals(' ') && pos != caretPosition)
					{
						whiteSpaceFound = true;
					}
					else if (whiteSpaceFound)
					{
						return pos;
					}

					++pos;
					if (pos >= chars.Length)
					{
						return chars.Length;
					}
					c = chars[pos];
				}
			}
		}

		public int IndexOfLastCharBeforeWhitespace()
		{
			char[] chars = text.ToCharArray();
			int pos = caretPosition;

			bool charFound = false;
			while (true)
			{
				--pos;
				if (pos <= 0)
				{
					return 0;
				}
				var c = chars[pos];

				if (c.Equals(' ') && pos != caretPosition)
				{
					if (charFound)
					{
						return ++pos;
					}
				}
				else
				{
					charFound = true;
				}
			}
		}

		public void GetCaretUnderMouse()
		{
			//Calculate the character index of where we clicked
			int relativeX = InputManager.MouseX - location.X; //mouse relative to the textbox

			//now that we have the position of the cursor, calculate the index of the caret
			//get the string on screen
			string renderStr = text.Substring(scrollPosition);

			int selectedIndex = scrollPosition;

			for (int i = 0; i < renderStr.Length; i++)
			{
				var txVisible = renderStr.Substring(0, i + 1);
				var currentChar = renderStr.Substring(i, FastMath.FastClamp(i + 1, 0, 1));

				var txtSize = UiStateManager.MeasureString(txVisible);
				var charWidth = UiStateManager.MeasureString(currentChar);

				if (txtSize.X + charWidth.X < relativeX)
				{
					selectedIndex++;
				}
				if (txtSize.X + charWidth.X >= relativeX)
				{
					this.caretPosition = selectedIndex;
					break;
				}

				if (i == renderStr.Length - 1)
				{
					this.caretPosition = renderStr.Length;
					break;
				}
			}
		}
		#endregion

		#region Rendering
		public override void Draw()
		{
			if (screenVisible)
			{
				//If the uiscale updated, redraw the text

				//Background image
				UiTexture img = TextureManager.GetUiUv("button_disabled");
				UiStateManager.Draw9SplicedButton(img, this);

				//Check if we have text or we're focused
				if (text != "" || isEditing)
				{
					bool caretVisible = caretTimer * 2 % caretAnimationTime > 0.5f;

					DrawText(text, textColor, scrollPosition, caretPosition, caretVisible && isEditing); //Don't display the caret if we aren't editing
				}
				else
				{
					//Placeholder
					DrawText(placeholderText, placeholderColor, 0, -1, false);
				}
			}
		}

		public int GetCharacterPixel(int position, string text)
		{
			Vector2 _tmp = UiStateManager.MeasureString(text.Substring(0, position));
			return (int)_tmp.X;
		}

		public void DrawText(string text, Color color, int leftMostCharacter, int caretPosition, bool CaretVisible)
		{
			string finalString = text.Substring(leftMostCharacter);

			DrawStringClip(size.X - 2 * paddingX + location.X, UiStateManager.gameFont, finalString, 
				new Vector2(location.X + paddingX * GameSettings.UIScale, location.Y + paddingY * GameSettings.UIScale), color,
				0.0f, Vector2.Zero, new Vector2(GameSettings.UITextScale), SpriteEffects.None, 0);

			if (CaretVisible)
			{
				Vector2 textWidth = UiStateManager.gameFont.MeasureString("L");

				UiStateManager.DrawImage(new Rectangle(
					location.X + paddingX * GameSettings.UIScale + 
					(int) UiStateManager.gameFont.MeasureString(text.Substring(leftMostCharacter, caretPosition - leftMostCharacter)).X, //Width

					location.Y + paddingY,
					2 * GameSettings.UIScale, 
					FastMath.FastClamp((int)(textWidth.Y * GameSettings.UIScale), 0, size.Y - 2 * paddingY)),

					TextureLoader.getWhitePixel(),
					placeholderColor);
			}
		}

		public void DrawStringClip(int width, BMFont bmFont, string text,
			Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects,
			float layerDepth)
		{
			if (bmFont == null) { throw new ArgumentNullException(nameof(bmFont)); }
			if (text == null) { throw new ArgumentNullException(nameof(text)); }

			Matrix temp;
			Matrix transform;
			Matrix.CreateScale(scale.X, scale.Y, 1, out temp);
			Matrix.CreateRotationZ(rotation, out transform);
			Matrix.Multiply(ref temp, ref transform, out transform);

			//character index
			int prevMaxX = -1;
			int index = 0;
			int rightMostCharacter = 0;
			bool isHighlighted = false;
			int selectionEdge = caretPosition + selectedPosition;

			int minSelection = Math.Min(caretPosition, selectedPosition);
			int maxSelection = Math.Max(caretPosition, selectedPosition);

			bmFont.ProcessChars(text, (actual, drawPos, data, previous) => {
				var sourceRectangle = new Rectangle(data.X, data.Y, data.Width, data.Height);
				Vector2.Transform(ref drawPos, ref transform, out drawPos);
				var destRectangle = new Rectangle((int)(position.X + drawPos.X), (int)(position.Y + drawPos.Y),
					(int)(data.Width * scale.X), (int)(data.Height * scale.Y));

				//Check if the character is highlighted
				isHighlighted =

				text.Length > 0											//text isnt empty
				&& selectedPosition != -1 &&							//we have highlighted something
				(maxSelection > index && minSelection <= index);		//selection edge is greater than caret position

				//Check if it clips
				rightMostCharacter = destRectangle.Right;
				if(rightMostCharacter > width && destRectangle.X < width) //if the rectangle passes through width
				{
					//recalculate the rectangles
					//width * percentage of covered area = new width
					sourceRectangle.Width = (int)(sourceRectangle.Width * (float)(width - destRectangle.X) / destRectangle.Width);
					destRectangle.Width = width - destRectangle.X;
				}

				//Drawing
				if (isHighlighted)
				{
					if (prevMaxX == -1)
					{
						prevMaxX = destRectangle.X;
					}

					UiStateManager.DrawImage(new Rectangle(prevMaxX, location.Y + paddingY, destRectangle.Width + GameSettings.UIScale + (destRectangle.X - prevMaxX), size.Y - 2 * paddingY),
						new Rectangle(0, 0, 1, 1), TextureLoader.getWhitePixel(), HighlightColor);
					prevMaxX = prevMaxX + destRectangle.Width + GameSettings.UIScale + (destRectangle.X - prevMaxX);
				}

				UiStateManager.SpriteBatch.Draw(bmFont.Texture, destRectangle, sourceRectangle, color, rotation, origin,
				   effects, layerDepth);

				index++;
			}, null);
		}
		#endregion
	}
}
