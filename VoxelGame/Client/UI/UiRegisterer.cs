using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client.UI
{
	public class UiRegisterer
	{
		public static DebugScreen debugUI;
		public static HotbarScreen hotbarUI;
		public static PauseScreen pauseUI;
		public static OptionsScreen optionsUI;
		public static LanguageScreen languageUI;
		public static StatisticsScreen statsUI;

		public static MainMenuScreen mainMenuUI;
		public static MultiplayerScreen multiplayerUI;
		public static LoadingScreen loadingUI;

		public static void RegisterUiMenus()
		{
			//Caching images requires us to start the spritebatch
			UiStateManager.Begin();
			//ORDER MATTERS!!

			//UI
			hotbarUI = new HotbarScreen();
			GameClient.Instance.RegisterGui(hotbarUI);

#if !DEBUG
			//If this is a release build, we add the debug UI on top of the hotbar but beneath all other UI elements
			debugUI = new DebugScreen();
			GameClient.Instance.RegisterGui(debugUI);
#endif

			pauseUI = new PauseScreen();
			GameClient.Instance.RegisterGui(pauseUI);

			mainMenuUI = new MainMenuScreen();
			GameClient.Instance.RegisterGui(mainMenuUI);

			multiplayerUI = new MultiplayerScreen();
			GameClient.Instance.RegisterGui(multiplayerUI);

			optionsUI = new OptionsScreen();
			GameClient.Instance.RegisterGui(optionsUI);

			languageUI = new LanguageScreen();
			GameClient.Instance.RegisterGui(languageUI);

			statsUI = new StatisticsScreen();
			GameClient.Instance.RegisterGui(statsUI);

			loadingUI = new LoadingScreen();
			GameClient.Instance.RegisterGui(loadingUI);

#if DEBUG
			//If we're in debug mode, the debug UI is added last for debugging reasons
			debugUI = new DebugScreen();
			GameClient.Instance.RegisterGui(debugUI);
#endif

			UiStateManager.End();
		}
	}
}
