using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	/// <summary>
	/// A text label whose text is dependant on the current language
	/// </summary>
	public class UiLangText : Component
	{
		private string _text = "text";
		private string[] lines = { };

		public Color color;
		public bool enabled
		{
			get { return render; }
			set { render = value; }
		}

		//The location now represents the top right pixel of the text
		public bool AlignRight = false;

		/// <summary>
		/// Returns the display string based on language.
		/// Sets the language key (eg. options.audio)
		/// </summary>
		public string text
		{
			get { return LanguageHelper.GetLanguageString(_text); }
			set
			{
				_text = value;
				RecalculateSize();
			}
		}

		public override void getTextures()
		{
			//get the asset from ui atlas
		}

		public UiLangText(GuiScreen parent, int x, int y, string text) : base(parent, x, y, 0, 0)
		{
			location = new Point(x, y);
			this.text = text;
			this.color = Color.White;
		}

		public UiLangText(GuiScreen parent, Point location, string text) : base(parent, location, Point.Zero)
		{
			this.location = location;
			this.text = text;
			this.color = Color.White;
		}

		public UiLangText(GuiScreen parent, int x, int y, string text, Color color) : base(parent, x, y, 0, 0)
		{
			location = new Point(x, y);
			this.text = text;
			this.color = color;
		}

		public UiLangText(GuiScreen parent, Point location, string text, Color color) : base(parent, location, Point.Zero)
		{
			this.location = location;
			this.text = text;
			this.color = color;
		}

		public override void onUpdate()
		{
			//size = LanguageHelper.GetLanguageString(text);
		}

		public override void Draw()
		{
			lines = text.Split('\n');
			RecalculateSize();

			if (AlignRight)
			{
				int _heightOffset = 0;
				for (int i = 0; i < lines.Length; i++)
				{
					//Empty check
					if (lines[i] != null && lines[i].Length > 0)
					{
						//Get the size
						Vector2 _size = UiStateManager.MeasureString(lines[i]);

						UiStateManager.DrawText(location.X - _size.X, location.Y + _heightOffset, color, lines[i]);

						_heightOffset += UiStateManager.gameFont.LineSpacing;
					}
				}
			}
			else
			{
				UiStateManager.DrawText(location.X, location.Y, color, text);
			}
		}

		private void RecalculateSize()
		{
			Vector2 _size = UiStateManager.MeasureString(text);
			size = new Point((int)_size.X, (int)_size.Y);
		}
	}
}