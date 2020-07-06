using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client.UI
{
	public class PauseScreen : GuiScreen
	{
		public UiImage darken;
		public UiLangText PausedText;

		public UiButton Resume;
		public UiButton Options;
		public UiButton MainMenu;

		public PauseScreen()
		{
			//Darken overlay
			darken = new UiImage(this, 0, 0, int.MaxValue, int.MaxValue, new Color(0, 0, 0, 123));
			darken.location = Point.Zero;
			AddComponent(darken);

			PausedText = new UiLangText(this, 0, 0, "pause.title", Color.White);
			AddComponent(PausedText);

			Resume = new UiButton(this, "pause.resume", new Point(0, 0), new Point(164, 20));
			AddComponent(Resume);
			Resume.OnClicked += OnResumeClick;

			Options = new UiButton(this, "pause.options", new Point(0, 0), new Point(164, 20));
			AddComponent(Options);
			Options.OnClicked += OnOptionsClick;

			MainMenu = new UiButton(this, "pause.mainmenu", new Point(0, 0), new Point(164, 20));
			AddComponent(MainMenu);
			MainMenu.OnClicked += OnMainMenuClick;
		}

		public override void Update()
		{
			if (GameClient.IsGameWorldloaded && GameClient.isPaused && UiRegisterer.optionsUI.visible == false && UiRegisterer.languageUI.visible == false && UiRegisterer.statsUI.visible == false)
			{
				visible = true;
			}
			else
			{
				visible = false;
			}

			//Center the paused text
			PausedText.location = new Point(GameClient.ViewWidth / 2 - PausedText.size.X / 2, GameClient.ViewHeight/2 - GameSettings.UIScale * 20);

			//Resize darken to fit the screen
			darken.size = new Point(GameClient.ViewWidth, GameClient.ViewHeight);

			//Center the buttons
			Resume.size = new Point(Resume.rawSize.X * GameSettings.UIScale, Resume.rawSize.Y * GameSettings.UIScale);
			Resume.location = new Point(GameClient.ViewWidth / 2 - Resume.size.X / 2, PausedText.location.Y + UiStateManager.gameFont.LineSpacing + 10 * GameSettings.UIScale);

			Options.size = Resume.size;
			Options.location = new Point(GameClient.ViewWidth / 2 - Options.size.X / 2, Resume.location.Y + Options.size.Y + 5 * GameSettings.UIScale);

			MainMenu.size = Resume.size;
			MainMenu.location = new Point(GameClient.ViewWidth / 2 - MainMenu.size.X / 2, Options.location.Y + MainMenu.size.Y + 5 * GameSettings.UIScale);

			//Update text
			base.Update();
		}


		#region Button Handling
		public void OnResumeClick(object sender, EventArgs e)
		{
			//unpause the client
			GameClient.isPaused = false;
		}

		public void OnOptionsClick(object sender, EventArgs e)
		{
			UiRegisterer.optionsUI.previousScreen = this;
			UiRegisterer.optionsUI.visible = true;
			GameClient.canPause = false;
			this.visible = false;
		}

		public void OnMainMenuClick(object sender, EventArgs e)
		{
			//do stuff

			//Set the world renderer.
			//TODO: Allow the player to select a world
			UiRegisterer.loadingUI.visible = true;

			GameClient.World.End();
			GameClient.World = new MenuWorld();

			//unpause the client
			GameClient.isPaused = false;
			visible = false;

			UiRegisterer.mainMenuUI.visible = true;
		}
		#endregion
	}
}
