using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;

namespace VoxelGame.Client.UI
{
	public class ProfilerGraphRenderer : Component
	{
		public bool enabled
		{
			get { return render; }
			set { render = value; }
		}

		private int X_START = 340;
		private const float ADJUSTMENT = .0005f;
		public const int Padding = 4;

		private readonly Color BorderColor = Color.White;
		private readonly Color BackgroundColor = new Color(0, 0, 0, 123);

		public ProfilerGraphRenderer(GuiScreen parent, int x, int y) : base(parent, x, y, 290, 152)
		{
			location = new Point(x, y);
		}

		/// <summary>
		/// Renders the profiler graph
		/// TODO: Make the graph renderer prettier. ie border, background, and then scaling it
		/// </summary>
		public override void Draw()
		{
			//Draw the background
			UiStateManager.DrawImage(new Rectangle(location.X + GameSettings.UIScale, location.Y + GameSettings.UIScale, size.X - 2 * GameSettings.UIScale, size.Y - 2 * GameSettings.UIScale), TextureLoader.getWhitePixel(), BackgroundColor);

			//Draw the border
			UiStateManager.DrawImage(new Rectangle(location.X, location.Y, size.X, GameSettings.UIScale), TextureLoader.getWhitePixel(), BorderColor);
			UiStateManager.DrawImage(new Rectangle(location.X, location.Y + size.Y - GameSettings.UIScale, size.X, GameSettings.UIScale), TextureLoader.getWhitePixel(), BorderColor);
			UiStateManager.DrawImage(new Rectangle(location.X, location.Y, GameSettings.UIScale, size.Y), TextureLoader.getWhitePixel(), BorderColor);
			UiStateManager.DrawImage(new Rectangle(location.X + size.X - GameSettings.UIScale, location.Y, GameSettings.UIScale, size.Y), TextureLoader.getWhitePixel(), BorderColor);

			X_START = location.X + GameSettings.UIScale + Padding;
			int currentY = location.Y + GameSettings.UIScale + Padding / 2;
			foreach (string timerId in Profiler.Timers.Keys)
			{
				//Draw each line, and overlay the text
				UiStateManager.DrawImage(new Rectangle(X_START, currentY, (int)(Profiler.Timers[timerId].ElapsedTicks * ADJUSTMENT), 18), TextureLoader.getWhitePixel(), Profiler.ProfilerColors[timerId]);
				UiStateManager.DrawText(new Vector2(X_START, currentY - 3), Profiler.ProfilerColors[timerId].Invert(), timerId + "(" + Profiler.Timers[timerId].ElapsedMilliseconds + " ms)");
				Profiler.Timers[timerId].Reset();
				currentY += 20;
			}
		}
	}
}
