using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client
{
	public class UiText : Component
	{
		private string _text = "text";
		private string[] lines = { };

		public bool enabled
		{
			get { return render; }
			set { render = value; }
		}
		public Color color;

		//The location now represents the top right pixel of the text
		public bool AlignRight = false;

		public string text
		{
			get { return _text; }
			set
			{
				_text = value;
				lines = text.Split('\n');
				RecalculateSize();
			}
		}

		public override void getTextures()
		{
			//get the asset from ui atlas
		}

		public UiText(GuiScreen parent, int x, int y, string text) : base(parent, x, y, 0, 0)
		{
			location = new Point(x, y);
			this.text = text;
			this.color = Color.White;
		}

		public UiText(GuiScreen parent, Point location,  string text) : base(parent, location, Point.Zero)
		{
			this.location = location;
			this.text = text;
			this.color = Color.White;
		}

		public UiText(GuiScreen parent, int x, int y, string text, Color color) : base(parent, x, y, 0, 0)
		{
			location = new Point(x, y);
			this.text = text;
			this.color = color;
		}

		public UiText(GuiScreen parent, Point location, string text, Color color) : base(parent, location, Point.Zero)
		{
			this.location = location;
			this.text = text;
			this.color = color;
		}

		public override void onUpdate()
		{

		}

		public override void Draw()
		{
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
			Vector2 _size = UiStateManager.MeasureString(_text);
			size = new Point((int)_size.X, (int)_size.Y);
		}
	}
}
