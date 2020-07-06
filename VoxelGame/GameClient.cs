using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VoxelGame.Client;
using VoxelGame.Client.Renderers;
using VoxelGame.World;
using VoxelGame.Engine;
using VoxelGame.Util;
using VoxelGame.Client.UI;
using VoxelGame.Util.Managers;
#if !CONSOLE
using System.Management;
#endif

namespace VoxelGame
{
	public class GameClient : VoxelClient
	{
		public static new GameClient Instance;
		private bool firstFrame = false;
		private bool isLoaded = false;
		/// <summary>
		/// The game directory
		/// </summary>
		public static string ClientDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6);

		//public Logger logger;
		//public static Logger LOGGER { get { return Instance.logger; } }
		/// <summary>
		/// The game directory. Files such as options, worlds, logs and screenshots should be saved here
		/// </summary>
		public static string gameDirectory;

		public static int width = 853;
		public static int height = 480;

		public WorldRenderer worldRenderer;
		private bool quitOnEscape = false;
		public BlockManager blockRegistry;
		public ItemManager itemRegistry;
		public static bool canPause = false;
		public static bool isPaused = false;

		public static double TimeSinceBoot = 0D;

		public static World.World theWorld
		{
			get
			{
				return (World.World)World;
			}
			set
			{
				World = value;
			}
		}

		public static bool IsGameWorldloaded
		{
			get
			{
				return World.GetType() == typeof(World.World);
			}
		}

		public SpriteFont font;

		//public static World World;

		public List<GuiScreen> GUIs = new List<GuiScreen>();
		public static List<string> ResourcePacks = new List<string>();

		public static Keys CurrentKey;
		public static char CurrentCharacter;

#if !CONSOLE
		public static DiscordHandler DiscordManager = new DiscordHandler();
#endif

		//Renderers


		/* ============================================================
		 *                         VERSIONING:
		 * ============================================================
		 * 
		 * Versions are written as follows:
		 * <Title> <Base Version> <Actual Version>
		 * Title          = The name of the game (Voxel Game)
		 * Base Version   = Alpha, Beta, Release (and some other stages)
		 * Actual Version = SNAPSHOTS:
		 *                    s<Major>.<Minor (2 d.p.)><letter, being the release this week (a=1st,b=2nd,etc.)>
		 *                    Example: s0.01a
		 *                  RELEASE:
		 *                    <Game Version>
		 *                    Example: 1.0
		 */
		public GameClient(string gameDirectory) : base(1920, 1080, width, height, 853, 480, "VoxelGame proto_1.05b", false)
		{
			System.Threading.Thread.CurrentThread.Name = "main";

			GameClient.Instance = this;
			GameClient.gameDirectory = gameDirectory;
			Version = "proto_1.05b";
			Logger.Initialize(Path.Combine(gameDirectory, "logs"));

			//Attach the hooks to the logger
			Logger.senders.Add("main");
			Logger.info(0, "Loading VoxelGame " + GameClient.Instance.Version);

			Logger.senders.Add("discord_game_sdk");

			ExitOnEscapeKeypress = false;

			Exiting += onExiting;
			Deactivated += onDeactivated;
		}

		/// <summary>
		/// Called when we lose focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void onDeactivated(object sender, EventArgs e)
		{
			//This is so that we don't close the pause menu if the player is already in it
			//if (!isPaused)
			{
				isPaused = true;
			}
		}

		public void onExiting(object sender, EventArgs args)
		{
			//Close world
			if (IsGameWorldloaded)
			{
				World.End();
			}

			ThreadManager.KillAllThreads();

			//Save the snooping data
			Snooper.Save();

			//Save the settings
			GameSettings.Save(Path.Combine(gameDirectory, "options.txt"));
#if DEBUG
			GameSettings.SaveDebug(Path.Combine(gameDirectory, "options.debug"));
#endif

			//Dispose of the discord hooks properly
			DiscordManager.ShutDown();

			//Save the logger
			Logger.Save();
		}

		public void init(int width, int height)
		{
			/*
			GameClient.width = width;
			GameClient.height = height;

			Graphics.PreferredBackBufferWidth = width;
			Graphics.PreferredBackBufferHeight = height;
			Graphics.IsFullScreen = false;
			Graphics.ApplyChanges();
			*/

#if !CONSOLE
			//Set the maximum number of threads we can assign
			ManagementObjectSearcher processorInfo = new ManagementObjectSearcher("select * from Win32_Processor");
			ThreadManager.MaxThreads = 0;
			ThreadManager.MaxProcessorThreads = 0;
			foreach (ManagementObject obj in processorInfo.Get())
			{
				ThreadManager.MaxThreads += int.Parse(obj["NumberOfLogicalProcessors"].ToString());
			}
			ThreadManager.MaxThreads = Math.Max(ThreadManager.MaxThreads, 4);
			ThreadManager.MaxProcessorThreads = Math.Max(ThreadManager.MaxThreads, 4);
#endif

			SetWindowed(width, height);
			RegisterPostProcessingEffect(new CrosshairEffect(), "crosshair");
		}

		protected override void Initialize()
		{
			/*
			//TODO: Loading screen, run this on a separate thread
#if !CONSOLE && CLIENT
			DiscordManager.InitialiseDiscord();
#endif

#if CLIENT
			OnTargetFPSChanged();
			Snooper.Initialise();

			InputManager.Initialise();

			//TODO: Rework for UI system
			font = Content.Load<SpriteFont>("MainFont");
#endif
			//TODO: Connect textures by string, and models are reloaded after textures are reloaded.
			TextureManager.Reload();

			//Initialise the Block & Item Managers
			blockRegistry = new BlockManager();
			itemRegistry = new ItemManager();

			//Register the blocks and items
			BlockManager.RegisterBlocks();
			ItemManager.RegisterItems();

			GameSettings.Load(Path.Combine(gameDirectory, "options.txt"));
#if DEBUG
			GameSettings.LoadDebug(Path.Combine(gameDirectory, "options.debug"));
#endif

#if CLIENT
			//Register the UI Menus
			UiRegisterer.RegisterUiMenus();

			Window.TextInput += OnTextInput;
#endif
			*/
			World = new MenuWorld();

			base.Initialize();
		}

		protected override void Update(GameTime gameTime)
		{
			//First tick initialization
			if (firstFrame == false)
			{
				firstFrame = true;
				FirstUpdate();
			}

			Time.GameTime = gameTime;
			InputManager.Update();

			base.Update(gameTime);

			if (quitOnEscape)
			{
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
					Exit();
			}
			else
			{
				//Handle pausing
				if (InputManager.Released("misc.pause"))
				{
					TogglePause();
				}
			}

#if !CONSOLE && CLIENT
			//Screenshots
			if (InputManager.Released("misc.screenshot"))
			{
				CaptureScreenshot();
			}

#if DEBUG
			//Disable block rendering
			if (InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.renderchunks"))
			{
				GameSettings.RenderChunkBorders = !GameSettings.RenderChunkBorders;
			}

			//Show physics colliders
			if (InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.physicscolliders"))
			{
				GameSettings.Debug.RenderPhysicsTestLocations = !GameSettings.Debug.RenderPhysicsTestLocations;
			}

			//Show entity colliders
			if (InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.entitycolliders"))
			{
				GameSettings.DrawHitboxes = !GameSettings.DrawHitboxes;
			}

			//Respawn
			if (InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.respawn"))
			{
				if (IsGameWorldloaded)
				{
					theWorld.player.position = new Vector3(theWorld.spawnX, theWorld.spawnY, theWorld.spawnZ);
					theWorld.player.Velocity = Vector3.Zero;
				}
			}

			//Noclip
			if (InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.noclip"))
			{
				if (IsGameWorldloaded)
				{
					theWorld.player.noClip = !theWorld.player.noClip;
				}
			}
#endif

			DiscordManager.OnUpdate();
#endif
			//Time logging
			Snooper.Snoop("time.totalplayed", gameTime.ElapsedGameTime.TotalSeconds);
			TimeSinceBoot += gameTime.ElapsedGameTime.TotalSeconds;

			Profiler.Start("UI Update", Color.LightCyan);
			foreach (GuiScreen ui in GUIs)
			{
				ui.Update();
			}
			Profiler.Stop("UI Update");

			IsMouseVisible = isPaused || IsGameWorldloaded == false;

			FinalUpdate();
		}

		public void TogglePause()
		{
			if (IsGameWorldloaded && canPause)
			{
				//TODO: Check if in a world

				//Pause the game
				isPaused = !isPaused;
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			Profiler.Start("Rendering", Color.LimeGreen);
			Time.GameTime = gameTime;
			//GraphicsDevice.Clear(Color.CornflowerBlue);

			base.Draw(gameTime);

			if (IsGameWorldloaded)
			{
				worldRenderer.RenderWorld(theWorld);
			}
			else
			{
				World.Draw(gameTime);
			}

			RenderUtils.Draw(Graphics.GraphicsDevice,
				World.player.camera.ViewMatrix,
				World.player.camera.ProjectionMatrix);

			if (IsGameWorldloaded)
			{
				theWorld.player.Draw();
			}
			else
			{
				if (World.player != null)
				{
					World.player.Draw();
				}
			}

			//Render UI
			UiStateManager.Begin();
			foreach (GuiScreen ui in GUIs)
			{
				if (ui.visible)
				{
					ui.Render();
				}
			}
			UiStateManager.End();

			LateDraw();
			Profiler.Stop("Rendering");
		}

		/// <summary>
		/// This method is called ONCE per session, and is called from the Update() Loop the first time it is ever ran.
		/// Used for multithreaded initialization
		/// </summary>
		private void FirstUpdate()
		{
			//TODO: Loading screen, run this on a separate thread
#if !CONSOLE && CLIENT
			DiscordManager.InitialiseDiscord();
#endif

#if CLIENT
			OnTargetFPSChanged();
			Snooper.Initialise();

			InputManager.Initialise();

			//TODO: Rework for UI system
			font = Content.Load<SpriteFont>("MainFont");
#endif
			//TODO: Connect textures by string, and models are reloaded after textures are reloaded.
			TextureManager.Reload();

			//Initialise the Block & Item Managers
			blockRegistry = new BlockManager();
			itemRegistry = new ItemManager();

			//Register the blocks and items
			BlockManager.RegisterBlocks();
			ItemManager.RegisterItems();

			GameSettings.Load(Path.Combine(gameDirectory, "options.txt"));
#if DEBUG
			GameSettings.LoadDebug(Path.Combine(gameDirectory, "options.debug"));
#endif

#if CLIENT
			//Register the UI Menus
			UiRegisterer.RegisterUiMenus();

			Window.TextInput += OnTextInput;
#endif
		}

#if !CONSOLE
		/// <summary>
		/// Takes a screenshot, saving it to the game directory under screenshots
		/// </summary>
		/// <param name="screenshotLocation"></param>
		public static void CaptureScreenshot()
		{
			Instance.SaveScreenshotP(Path.Combine(gameDirectory, "screenshots", DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".png"));
		}
#endif

		public void OnTargetFPSChanged()
		{
			Title = "VoxelGame " + Version + " @ " + (GameSettings.VSyncEnabled ? "VSync" : GameSettings.UnlimitedFPS ? "Unlimited" : GameSettings.FPS + " FPS");

			Logger.info(-1, "Setting FPS to " + (GameSettings.VSyncEnabled ? "VSync" : GameSettings.UnlimitedFPS ? "Unlimited" : (GameSettings.FPS * 10).ToString()));
			if (GameSettings.UnlimitedFPS)
			{
				Graphics.SynchronizeWithVerticalRetrace = false; //Disable VSync
				IsFixedTimeStep = false; //Disable FPS cap
			}
			else if (GameSettings.VSyncEnabled)
			{
				Graphics.SynchronizeWithVerticalRetrace = true; //Enable VSync
				IsFixedTimeStep = false; //Disable FPS cap
			}
			else
			{
				Graphics.SynchronizeWithVerticalRetrace = false; //Disable VSync
				IsFixedTimeStep = true; //Enable FPS cap
				TargetElapsedTime = TimeSpan.FromSeconds(1d / (GameSettings.FPS)); //Set target FPS
			}
		}

		public void RegisterGui(GuiScreen screen)
		{
			GUIs.Add(screen);
		}

		private void OnTextInput(object sender, TextInputEventArgs e)
		{
			CurrentKey = e.Key;
			CurrentCharacter = e.Character;
		}
	}
}
