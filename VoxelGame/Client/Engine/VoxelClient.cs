using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.Client.Renderers;
using VoxelGame.Crash;
using VoxelGame.Util;
using VoxelGame.Util.Managers;

namespace VoxelGame.Engine
{
	public class VoxelClient : Game
	{
		public string Title;
		public string Version;

		// references
		public static VoxelClient Instance { get; private set; }
		public static GraphicsDeviceManager Graphics { get; private set; }

		// time
		public static float DeltaTime { get; private set; }
		public static float RawDeltaTime { get; private set; }
		public static float TimeRate = 1f;
		public static float TimeRateB = 1f;
		public static float FreezeTimer;
		public static int FPS;
		private TimeSpan counterElapsed = TimeSpan.Zero;
		private int fpsCounter = 0;

		// screen size
		public static int Width { get; private set; }
		public static int Height { get; private set; }
		public static int MinimumWidth { get; private set; }
		public static int MinimumHeight { get; private set; }
		public static int ViewWidth { get; private set; }
		public static int ViewHeight { get; private set; }
		public static int ViewPadding
		{
			get { return viewPadding; }
			set
			{
				viewPadding = value;
				Instance.UpdateView();
			}
		}
		private static int viewPadding = 0;
		public static Viewport Viewport { get; private set; }
		private static bool resizing;
		public static Matrix ScreenMatrix;

		public static RenderTarget2D renderer, renderBuffer;
		private static SortedDictionary<string, PostProcessor> postProcessors = new SortedDictionary<string, PostProcessor>();

		// content directory
#if !CONSOLE
		private static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#endif

		public static string ContentDirectory
		{
#if PS4
			get { return Path.Combine("/app0/", Instance.Content.RootDirectory); }
#elif NSWITCH
			get { return Path.Combine("rom:/", Instance.Content.RootDirectory); }
#elif XBOXONE
			get { return Instance.Content.RootDirectory; }
#else
			get { return Path.Combine(AssemblyDirectory, Instance.Content.RootDirectory); }
#endif
		}

		// util
		public static bool ExitOnEscapeKeypress = true;
		private string SaveScreenshot = "";
		private Texture2D screenbuffer;

		// scene
		private BlockWorld level;
		private BlockWorld nextlevel;

		public static BlockWorld World
		{
			get
			{
				return VoxelClient.Instance.level;
			}
			set
			{
				VoxelClient.Instance.nextlevel = value;
			}
		}

		public VoxelClient(int width, int height, int windowWidth, int windowHeight, int minWindowWidth, int minWindowHeight, string windowTitle, bool fullscreen)
		{
			this.Title = windowTitle;
			VoxelClient.Instance = this;
			base.InactiveSleepTime = new TimeSpan(0L);
			VoxelClient.Graphics = new GraphicsDeviceManager(this);
			VoxelClient.Graphics.DeviceReset += this.OnGraphicsReset;
			VoxelClient.Graphics.DeviceCreated += this.OnGraphicsCreate;
			VoxelClient.Graphics.SynchronizeWithVerticalRetrace = true;
			VoxelClient.Graphics.PreferMultiSampling = false;
			VoxelClient.Graphics.GraphicsProfile = GraphicsProfile.HiDef;
			VoxelClient.Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
			VoxelClient.Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
			VoxelClient.Graphics.ApplyChanges();
#if PS4 || XBOXONE
			Graphics.PreferredBackBufferWidth = 1920;
			Graphics.PreferredBackBufferHeight = 1080;
#elif NSWITCH
			Graphics.PreferredBackBufferWidth = 1280;
			Graphics.PreferredBackBufferHeight = 720;
#else
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += OnClientSizeChanged;
			MinimumHeight = minWindowHeight;
			MinimumWidth = minWindowWidth;

			if (fullscreen)
			{
				Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				Graphics.IsFullScreen = true;
				Graphics.ApplyChanges();
			}
			else
			{
				Graphics.PreferredBackBufferWidth = windowWidth;
				Graphics.PreferredBackBufferHeight = windowHeight;
				Graphics.IsFullScreen = false;
				Graphics.ApplyChanges();
			}
#endif
			UpdateView();

			base.Content.RootDirectory = "Content";
			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

			//Register default post processor so that the screen isnt black
			RegisterPostProcessingEffect(new PostProcessor(), "screen");
		}

#if !CONSOLE
		protected virtual void OnClientSizeChanged(object sender, EventArgs e)
		{
			//MINIMUM SIZE
			if (Window.ClientBounds.Width < MinimumWidth || Window.ClientBounds.Height < MinimumHeight)
			{
				Graphics.PreferredBackBufferWidth = MinimumWidth;
				Graphics.PreferredBackBufferHeight = MinimumHeight;
			} else {
				Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
				Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			}

			if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0 && !resizing)
			{
				resizing = true;
				UpdateView();

				resizing = false;
			}
		}
#endif

		protected virtual void OnGraphicsReset(object sender, EventArgs e)
		{
			this.UpdateView();
		}

		protected virtual void OnGraphicsCreate(object sender, EventArgs e)
		{
			this.UpdateView();
		}

		protected override void Update(GameTime gameTime)
		{
			VoxelClient.RawDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			VoxelClient.DeltaTime = VoxelClient.RawDeltaTime * VoxelClient.TimeRate * VoxelClient.TimeRateB;
#if !CONSOLE
			if (ExitOnEscapeKeypress)
			{
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
					|| Keyboard.GetState().IsKeyDown(Keys.Escape))
				{
					Exit();
					return;
				}
			}

			if (InputManager.Released("misc.fullscreen"))
			{
				GameSettings.Fullscreen = !GameSettings.Fullscreen;
			}
#endif
			//Update current scene
			if (FreezeTimer > 0)
			{
				FreezeTimer = Math.Max(FreezeTimer - RawDeltaTime, 0);
			}
			else if (level != null)
			{
				level.BeforeUpdate();
				level.Update(gameTime);
				//level.AfterUpdate();
			}

			//Changing scenes
			if (nextlevel != null)
			{
				if (level != null && level.Focused == true)
				{
					level.End();
				}
				level = nextlevel;
				if (level != null)
				{
					level.Begin();
				}
				nextlevel = null;
			}

			base.Update(gameTime);
		}

		protected void FinalUpdate()
		{
			InputManager.FinalUpdate();
		}

		protected override void Draw(GameTime gameTime)
		{
			VoxelClient.RawDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			VoxelClient.DeltaTime = VoxelClient.RawDeltaTime * VoxelClient.TimeRate * VoxelClient.TimeRateB;

			VoxelClient.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			VoxelClient.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			//check if the render target is null
			if (renderer == null || renderer.IsContentLost || renderer.IsDisposed)
			{
				renderer = new RenderTarget2D(Graphics.GraphicsDevice, Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth, Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
			}
			//check if the render target is null
			if (renderBuffer == null || renderBuffer.IsContentLost || renderBuffer.IsDisposed)
			{
				renderBuffer = new RenderTarget2D(Graphics.GraphicsDevice, Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth, Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
			}

			VoxelClient.Graphics.GraphicsDevice.SetRenderTarget(renderer);
			Graphics.GraphicsDevice.Clear(Color.Black);

			//base.Draw(gameTime);
			//Frame counter
			fpsCounter++;
			counterElapsed += gameTime.ElapsedGameTime;
			if (this.counterElapsed >= TimeSpan.FromSeconds(1.0))
			{
#if DEBUG && !CONSOLE
				Window.Title = Title + " - " + fpsCounter.ToString() + " FPS - " + (GC.GetTotalMemory(false) / 1048576f).ToString("F") + " MB";
#endif
				VoxelClient.FPS = fpsCounter;
				fpsCounter = 0;
				counterElapsed -= TimeSpan.FromSeconds(1.0);
			}
			VoxelClient.Graphics.GraphicsDevice.BlendState = BlendState.Opaque;
			VoxelClient.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		public void LateDraw()
		{
			VoxelClient.Graphics.GraphicsDevice.SetRenderTarget(null);
			screenbuffer = (Texture2D)renderer;

			//Render post processing
			VoxelClient.Graphics.GraphicsDevice.SetRenderTarget(renderBuffer);
			for (int i = 0; i < postProcessors.Count; i++)
			{
				if (postProcessors.ElementAt(i).Key == "screen")
				{
					PostProcessRenderer.Draw(screenbuffer, postProcessors.ElementAt(i).Value);
				}
				else
				{
					PostProcessRenderer.Draw(renderBuffer, postProcessors.ElementAt(i).Value);
				}
			}

			//Stop rendering to the render buffer
			VoxelClient.Graphics.GraphicsDevice.SetRenderTarget(null);
			screenbuffer = (Texture2D)renderBuffer;

			//Render the output on the screen
			UiStateManager.Begin();
			UiStateManager.SpriteBatch.Draw(screenbuffer, Vector2.Zero, Color.White);
			UiStateManager.End();
#if !CONSOLE
			if (SaveScreenshot != "")
			{
				Assets.TextureLoader.Save(screenbuffer, SaveScreenshot);
				Logger.info(0, "Saving screenshot at " + Path.GetDirectoryName(SaveScreenshot));
				SaveScreenshot = "";
			}
#endif
		}

		public static void SetLevel(BlockWorld level)
		{
			Instance.nextlevel = level;
		}

		public static void SetWindowed(int width, int height)
		{
#if !CONSOLE
			if (width > 0 && height > 0)
			{
				resizing = true;
				Graphics.PreferredBackBufferWidth = width;
				Graphics.PreferredBackBufferHeight = height;
				//Graphics.HardwareModeSwitch = true;
				Graphics.HardwareModeSwitch = false;
				Graphics.IsFullScreen = false;
				Graphics.ApplyChanges();
				Instance.UpdateView();
				//GameClient.LOGGER.info("WINDOW-" + width + "x" + height);
				resizing = false;
			}
#endif
		}

		public static void SetFullscreen()
		{
#if !CONSOLE
			resizing = true;
			Width = Graphics.PreferredBackBufferWidth;
			Height = Graphics.PreferredBackBufferHeight;
			Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			Graphics.HardwareModeSwitch = false;
			Graphics.IsFullScreen = true;
			Graphics.ApplyChanges();
			Instance.UpdateView();
			//GameClient.LOGGER.info("FULLSCREEN");
			resizing = false;
#endif
		}

		private void UpdateView()
		{
			float screenWidth = Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
			float screenHeight = Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

			ViewWidth = Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
			ViewHeight = Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

			CalculateUISize();

			// update screen matrix
			ScreenMatrix = Matrix.CreateScale(ViewWidth / (float)Width);

			// update viewport
			Viewport = new Viewport
			{
				X = (int)((screenWidth / 2f) - ViewWidth / 2),
				Y = (int)((screenHeight / 2f) - ViewHeight / 2),
				Width = ViewWidth,
				Height = ViewHeight,
				MinDepth = 0f,
				MaxDepth = 1f
			};

			//Reset the render target
			if (renderer != null)
			{
				renderer.Dispose();
			}
			renderer = null;

			if (renderBuffer != null)
			{
				renderBuffer.Dispose();
			}
			renderBuffer = null;
		}

		public static void CalculateUISize()
		{
			//Determine UI Scale if its set to AUTO
			if (GameSettings.AutoUIScale)
			{
				if (ViewWidth >= 1720 && ViewHeight >= 990)
				{
					GameSettings.UIScale = 4;
				}
				else if (ViewWidth >= 1440 && ViewHeight >= 840)
				{
					GameSettings.UIScale = 3;
				}
				else if (ViewWidth >= 1220 && ViewHeight >= 660)
				{
					GameSettings.UIScale = 2;
				}
				else
				{
					GameSettings.UIScale = 1;
				}
			}
		}

		protected override void OnExiting(Object sender, EventArgs args)
		{
			base.OnExiting(sender, args);

			// Stop the threads
			ThreadManager.KillAllThreads();
		}

		/// <summary>
		/// Crashes the game, under the specified crash reason
		/// </summary>
		/// <param name="crash_reason"></param>
		public static void RaiseCrash(CrashError crash_reason = CrashError.UNKNOWN)
		{
			throw new NotImplementedException();
		}

		internal void SaveScreenshotP(string loc)
		{
			SaveScreenshot = loc;
		}

		/// <summary>
		/// Registers a post processing effect
		/// </summary>
		/// <param name="effect">The post processing effect to register</param>
		/// <param name="name"></param>
		public void RegisterPostProcessingEffect(PostProcessor effect, string name)
		{
			if (postProcessors.ContainsKey(name))
			{
				Logger.error(0, "PostProcessor (" + effect.GetType().Name + ") is already registered under the name \"" + name + "\"!");
			}

			postProcessors.Add(name, effect);
		}

		public void RemovePostProcessingEffect(string name)
		{
			if (postProcessors.ContainsKey(name))
			{
				postProcessors.Remove(name);
			}
		}

		public static T LoadResource <T> (string location)
		{
			return Instance.Content.Load <T> (location);
		}
	}
}
