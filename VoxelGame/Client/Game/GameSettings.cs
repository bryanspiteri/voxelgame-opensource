using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	public class GameSettings
	{
		#region Instancing
		public static GameSettings _gs;
		public static GameSettings Instance
		{
			get
			{
				if (_gs == null)
				{
					_gs = new GameSettings();
				}
				return _gs;
			}
			set
			{
				if (_gs == null)
				{
					_gs = value;
				}
			}
		}

		public GameSettings()
		{
			debugSettings = new DebugSettings();
		}
		#endregion

		#region I/O Handling

		public static void Load(string optionsFile)
		{
			InitializeDefaultSettings();

			#region Templates
			/*
			 
			//SECTION =|= BOOL
			
			if (value == "0")
			{
				HeadBobbing = false;
			}
			else if (value == "1")
			{
				HeadBobbing = true;
			}
			 
			//SECTION =|= INT

			int uiscale = -1;
			if (int.TryParse(value, out uiscale))
			{
				RenderDistance = (byte)renderdistance;
			}
			 
			//SECTION =|= STRING
			
			value
			 
			 */
			#endregion

			if (File.Exists(optionsFile))
			{
				string[] optionsLines = File.ReadAllLines(optionsFile);
				for (int i = 0; i < optionsLines.Length; i++)
				{
					//Ignore whitespace and comments
					if (!(optionsLines[i].Trim() == "" || optionsLines[i].Trim().StartsWith("#")))
					{
						string argument, value;
						argument = optionsLines[i].Substring(0, optionsLines[i].IndexOf('=')).Trim().ToLower();
						value = optionsLines[i].Substring(optionsLines[i].IndexOf('=') + 1).Trim();

						//Handle the options
						switch (argument)
						{
							case "language":
								LanguageHelper.Reset();
								LanguageHelper.SetLanguage(value);
								break;
							case "uiscale":
								int uiscale = -1;
								if (int.TryParse(value, out uiscale))
								{
									if (uiscale == 0)
									{
										AutoUIScale = true;
									}
									else
									{
										AutoUIScale = false;
										UIScale = (byte)uiscale;
									}
								}
								break;
							case "framerate":
								int fps = -1;
								if (int.TryParse(value, out fps))
								{
									FPS = (byte)fps;
								}
								break;
							case "renderdistance":
								int renderdistance = -1;
								if (int.TryParse(value, out renderdistance))
								{
									RenderDistance = (byte)renderdistance;
								}
								break;
							case "headbob":
								if (value == "0")
								{
									HeadBobbing = false;
								}
								else if (value == "1")
								{
									HeadBobbing = true;
								}
								break;
							case "fullscreen":
								if (value == "0")
								{
									Fullscreen = false;
								}
								else if (value == "1")
								{
									Fullscreen = true;
								}
								break;
							case "entityshadows":
								if (value == "0")
								{
									ShadowsEnabled = false;
								}
								else if (value == "1")
								{
									ShadowsEnabled = true;
								}
								break;
							case "fov":
								float fov = -1;
								if (float.TryParse(value, out fov))
								{
									FOV = MathHelper.ToRadians(fov);
								}
								break;
						}
					}
				}
			}
		}

		public static void Save(string optionsFile)
		{
			//Key = value system
			List<string> lines = new List<string>();

			string tmp = "language=" + LanguageHelper.LoadedLanguage;
			lines.Add(tmp);

			tmp = "fullscreen=" + BoolToInt(Fullscreen);
			lines.Add(tmp);

			tmp = "entityshadows=" + BoolToInt(ShadowsEnabled);
			lines.Add(tmp);

			tmp = "headbob=" + BoolToInt(HeadBobbing);
			lines.Add(tmp);

			tmp = "renderdistance=" + RenderDistance;
			lines.Add(tmp);

			tmp = "framerate=" + FPS;
			lines.Add(tmp);

			string uiscale = "";
			if (AutoUIScale)
			{
				uiscale += "0";
			}
			else
			{
				uiscale += UIScale;
			}

			tmp = "uiscale=" + uiscale;
			lines.Add(tmp);

			tmp = "fov=" + MathHelper.ToDegrees(FOV);
			lines.Add(tmp);

			File.WriteAllLines(optionsFile, lines.ToArray());
		}

		public static void InitializeDefaultSettings()
		{
			CultureInfo ci = CultureInfo.CurrentUICulture;
			string cultureName = ci.Name.Replace('-', '_');

			if (File.Exists(FileUtil.GetPath("lang/" + cultureName + ".lang")))
			{
				//Set the language to language selected by the user at OS level.
				LanguageHelper.SetLanguage(cultureName);
			}
			else
			{
				//Default to american english
				LanguageHelper.SetLanguage("en_US");
			}

			AutoUIScale = true;
			FPS = 0;
			Fullscreen = false;
			HeadBobbing = true;
			ShadowsEnabled = true;
			RenderDistance = 8;
		}

		private static int BoolToInt(bool variable)
		{
			return variable ? 1 : 0;
		}

		#region Debug Settings

		public static void LoadDebug(string debugFile)
		{
#if DEBUG
			if (File.Exists(debugFile))
			{
				string[] optionsLines = File.ReadAllLines(debugFile);
				for (int i = 0; i < optionsLines.Length; i++)
				{
					//Ignore whitespace and comments
					if (!(optionsLines[i].Trim() == "" || optionsLines[i].Trim().StartsWith("#")))
					{
						string argument, value;
						argument = optionsLines[i].Substring(0, optionsLines[i].IndexOf('=')).Trim().ToLower();
						value = optionsLines[i].Substring(optionsLines[i].IndexOf('=') + 1).Trim();

						//Handle the options
						switch (argument)
						{
							case "drawchunks":
								if (value == "0")
								{
									Debug.RenderChunks = false;
								}
								else if (value == "1")
								{
									Debug.RenderChunks = true;
								}
								break;
							case "drawblockcolliders":
								if (value == "0")
								{
									Debug.RenderPhysicsTestLocations = false;
								}
								else if (value == "1")
								{
									Debug.RenderPhysicsTestLocations = true;
								}
								break;
							case "drawchunkborders":
								if (value == "0")
								{
									RenderChunkBorders = false;
								}
								else if (value == "1")
								{
									RenderChunkBorders = true;
								}
								break;
							case "drawhitboxes":
								if (value == "0")
								{
									DrawHitboxes = false;
								}
								else if (value == "1")
								{
									DrawHitboxes = true;
								}
								break;
							case "drawrays":
								if (value == "0")
								{
									Debug.RenderRaycastLocations = false;
								}
								else if (value == "1")
								{
									Debug.RenderRaycastLocations = true;
								}
								break;
						}
					}
				}
			}
#endif
		}

		public static void SaveDebug(string debugFile)
		{
#if DEBUG
			List<string> lines = new List<string>();

			string tmp = "drawchunks=" + BoolToInt(Debug.RenderChunks);
			lines.Add(tmp);

			tmp = "drawblockcolliders=" + BoolToInt(Debug.RenderPhysicsTestLocations);
			lines.Add(tmp);

			tmp = "drawchunkborders=" + BoolToInt(RenderChunkBorders);
			lines.Add(tmp);

			tmp = "drawhitboxes=" + BoolToInt(DrawHitboxes);
			lines.Add(tmp);

			tmp = "drawrays=" + BoolToInt(Debug.RenderRaycastLocations);
			lines.Add(tmp);

			File.WriteAllLines(debugFile, lines.ToArray());
#endif
		}

		#endregion

		#endregion

		#region Private Modifiers

		// Render Options
		/// <summary>
		/// Debug settings
		/// </summary>
		public DebugSettings debugSettings;

		/// <summary>
		/// Number of chunks to be drawn around the player
		/// </summary>
		public byte renderDistance = 2;
		public bool renderChunkBorders = false;
		public int renderClouds = 0;

		/// <summary>
		/// 0 = VSync, 10 * fps until 200 FPS, 255 = Unlimited
		/// </summary>
		public byte fps = 255;

		/// <summary>
		/// How far the player can reach
		/// </summary>
		public byte reach = 15;
		public bool shadowsEnabled = false;

		/// <summary>
		/// Specifies the color of clouds
		/// </summary>
		public Color cloudColor = Color.White;

		/// <summary>
		/// Specifies the color of the block selection outline
		/// </summary>
		public Color blockOutlineColor = new Color(76, 76, 76);
		public float blockOutlineWidth = 0.01f;
		public Color entityOutlineColor = new Color(255, 0, 255);

		public bool doHeadBob = true;
		public bool drawColliders = false;

		public byte uiScale = 2;
		public bool autoUIscale = true;
		public bool fullscreen = false;

		public float mouseSentivity = 1.0f;
		public float fieldofview = MathHelper.PiOver2;
		public float particleRenderDistance = 24f;

		public string accessToken = "";

		#endregion

		#region Static Accessors

		public static DebugSettings Debug
		{
			get
			{
				return Instance.debugSettings;
			}
		}

		/// <summary>
		/// Number of chunks to be drawn around the player
		/// </summary>
		public static byte RenderDistance
		{
			get
			{
				return Instance.renderDistance;
			}
			set
			{
				Instance.renderDistance = (byte)MathHelper.Clamp(value, MinRenderDistance, MaxRenderDistance);
			}
		}

		public static bool RenderChunkBorders
		{
			get
			{
				return Instance.renderChunkBorders;
			}
			set
			{
				Instance.renderChunkBorders = value;
			}
		}

		public static bool RenderClouds
		{
			get
			{
				return Instance.renderClouds != 0;
			}
			set
			{
				if (value == true)
				{
					Instance.renderClouds = 1;
				}
				else
				{
					Instance.renderClouds = 0;
				}

			}
		}

		public static bool RenderClouds3D
		{
			get
			{
				return Instance.renderClouds == 2;
			}
			set
			{
				if (value == true)
				{
					Instance.renderClouds = 2;
				}
			}
		}

		public static bool ShadowsEnabled
		{
			get
			{
				return Instance.shadowsEnabled;
			}
			set
			{
				Instance.shadowsEnabled = value;
			}
		}

		/// <summary>
		/// Whether Head-Bobbing is enabled
		/// </summary>
		public static bool HeadBobbing
		{
			get
			{
				return Instance.doHeadBob;
			}
			set
			{
				Instance.doHeadBob = value;
			}
		}

		/// <summary>
		/// 0 = VSync, 10 * fps until 200 FPS, 255 = Unlimited
		/// </summary>
		public static byte FPS
		{
			get
			{
				return Instance.fps;
			}
			set
			{
				Instance.fps = value;
				GameClient.Instance.OnTargetFPSChanged();
			}
		}

		/// <summary>
		/// How far the player can reach
		/// </summary>
		public static byte Reach
		{
			get
			{
				return Instance.reach;
			}
		}

		/// <summary>
		/// Returns whether VSync is enabled
		/// </summary>
		public static bool VSyncEnabled
		{
			get
			{
				return Instance.fps == 0;
			}
			set
			{
				Instance.fps = value ? (byte)0 : Instance.fps;
				GameClient.Instance.OnTargetFPSChanged();
			}
		}

		/// <summary>
		/// Returns whether the FPS is unlocked (no cap)
		/// </summary>
		public static bool UnlimitedFPS
		{
			get
			{
				return Instance.fps == 255;
			}
			set
			{
				Instance.fps = value ? (byte)255 : Instance.fps;
				GameClient.Instance.OnTargetFPSChanged();
			}
		}

		public static byte UIScale
		{
			get
			{
				return Instance.uiScale;
			}
			set
			{
				Instance.uiScale = value;
			}
		}

		public static float UITextScale
		{
			get
			{
				return Instance.uiScale * 0.5f;
			}
		}

		public static bool AutoUIScale
		{
			get
			{
				return Instance.autoUIscale;
			}
			set
			{
				Instance.autoUIscale = value;
			}
		}

		public static bool DrawHitboxes
		{
			get
			{
				return Instance.drawColliders;
			}
			set
			{
				Instance.drawColliders = value;
			}
		}

		public static float MouseSensitivity
		{
			get
			{
				return Instance.mouseSentivity;
			}
			set
			{
				Instance.mouseSentivity = value;
			}
		}

		public static bool Fullscreen
		{
			get
			{
				return Instance.fullscreen;
			}
			set
			{
				Instance.fullscreen = value;

				if (value)
				{
					GameClient.SetFullscreen();
				}
				else
				{
					GameClient.SetWindowed(GameClient.Width, GameClient.Height);
				}
			}
		}

		public static float FOV
		{
			get
			{
				return Instance.fieldofview;
			}
			set
			{
				Instance.fieldofview = value;
			}
		}

		public static float ParticleRenderDistance
		{
			get
			{
				return Instance.particleRenderDistance;
			}
			set
			{
				Instance.particleRenderDistance = value;
			}
		}

		public static float ParticleRenderDistanceSquared
		{
			get
			{
				return Instance.particleRenderDistance * Instance.particleRenderDistance;
			}
		}

		public static string AccessToken
		{
			get
			{
				return Instance.accessToken;
			}
			set
			{
				if (Instance.accessToken == "" || Instance.accessToken == null)
				{
					Instance.accessToken = value;
				}
				else
				{
					Logger.warn(-1, "Tried setting access token, but the access token was already set!");
				}
			}
		}

		#region Constants

		/// <summary>
		/// Specifies the color of clouds
		/// </summary>
		public static Color CloudColor
		{
			get
			{
				return Instance.cloudColor;
			}
		}

		/// <summary>
		/// Specifies the color of the block selection outline
		/// </summary>
		public static Color BlockOutlineColor
		{
			get
			{
				return Instance.blockOutlineColor;
			}
		}

		/// <summary>
		/// Specifies the width of the block selection outline
		/// </summary>
		public static float BlockOutlineWidth
		{
			get
			{
				return Instance.blockOutlineWidth;
			}
		}

		/// <summary>
		/// Specifies the color of the entity's hitbox (used in debug mode only)
		/// </summary>
		public static Color EntityHitboxColor
		{
			get
			{
				return Instance.entityOutlineColor;
			}
		}

		public static byte MaxRenderDistance
		{
			get
			{
				return 32;
			}
		}

		public static byte MinRenderDistance
		{
			get
			{
				return 2;
			}
		}

		#endregion

		#endregion
	}

	public class DebugSettings
	{
		public bool RenderPhysicsTestLocations = false;
		public bool RenderRaycastLocations = false;
		public bool RenderChunks = true;
	}
}
