using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;
using VoxelGame.Util;

namespace VoxelGame
{
	public static class InputManager
	{
		#region Variables
		private static KeyboardState CurrentKeys;
		private static KeyboardState PreviousKeys;

		private static MouseState CurrentMouse;
		private static MouseState PreviousMouse;

		private static GamePadState CurrentGamepad;
		private static GamePadState PreviousGamepad;

		#region Misc Mouse Properties
		public static int MouseX
		{
			get
			{
				return CurrentMouse.X;
			}
		}

		public static int MouseY
		{
			get
			{
				return CurrentMouse.Y;
			}
		}

		public static int MouseDeltaX
		{
			get
			{
				return PreviousMouse.X - CurrentMouse.X;
			}
		}

		public static int MouseDeltaY
		{
			get
			{
				return PreviousMouse.Y - CurrentMouse.Y;
			}
		}

		public static int MouseScrollNotchesY
		{
			get
			{
				return CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
			}
		}

		public static int MouseScrollNotchesX
		{
			get
			{
				return CurrentMouse.HorizontalScrollWheelValue - PreviousMouse.HorizontalScrollWheelValue;
			}
		}

		public static int MouseScrollWheel
		{
			get
			{
				return CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
			}
		}

		#endregion

		/// <summary>
		/// The Gamepad port to use
		/// </summary>
		public static PlayerIndex GamepadPort = PlayerIndex.One;

		/// <summary>
		/// Whether a gamepad is connected and recognised by the game
		/// </summary>
		public static bool IsGamepadConnected = false;

		/// <summary>
		/// Whether to use gamepad for input
		/// </summary>
		public static bool UseGamepad = false;

		private static Dictionary<string, Keybind> keybinds = new Dictionary<string, Keybind>();
		private static bool _centerMouse = false;
		#endregion

		#region Initialisation
		/// <summary>
		/// Registers keybinds
		/// </summary>
		public static void Initialise()
		{
			//GUI Interactions
			BindKey("gui.click", new Keybind(MouseButton.Button0, Buttons.A));

			//Movement
			BindKey("move.forward", new Keybind(Keys.W, Buttons.LeftThumbstickUp));
			BindKey("move.back", new Keybind(Keys.S, Buttons.LeftThumbstickDown));
			BindKey("move.left", new Keybind(Keys.A, Buttons.LeftThumbstickLeft));
			BindKey("move.right", new Keybind(Keys.D, Buttons.LeftThumbstickRight));

			//Controller look
			BindKey("look.forward", new Keybind(Buttons.RightThumbstickUp));
			BindKey("look.back", new Keybind(Buttons.RightThumbstickDown));
			BindKey("look.left", new Keybind(Buttons.RightThumbstickLeft));
			BindKey("look.right", new Keybind(Buttons.RightThumbstickRight));

			//Block interaction
			BindKey("item.break", new Keybind(MouseButton.Button0, Buttons.RightTrigger));
			BindKey("item.use", new Keybind(MouseButton.Button1, Buttons.LeftTrigger));

			//Movement modifiers
			BindKey("move.jump", new Keybind(Keys.Space, Buttons.A));
			BindKey("move.sneak", new Keybind(Keys.LeftShift, Buttons.RightStick));
			BindKey("move.sprint", new Keybind(Keys.LeftControl, Buttons.LeftStick));

			//Controller hotbar scrolling
			BindKey("hotbar.left", new Keybind(Buttons.LeftShoulder));
			BindKey("hotbar.right", new Keybind(Buttons.RightShoulder));

			//Keyboard number to hotbar mappings
			BindKey("hotbar.one", new Keybind(Keys.D1));
			BindKey("hotbar.two", new Keybind(Keys.D2));
			BindKey("hotbar.three", new Keybind(Keys.D3));
			BindKey("hotbar.four", new Keybind(Keys.D4));
			BindKey("hotbar.five", new Keybind(Keys.D5));
			BindKey("hotbar.six", new Keybind(Keys.D6));
			BindKey("hotbar.seven", new Keybind(Keys.D7));
			BindKey("hotbar.eight", new Keybind(Keys.D8));
			BindKey("hotbar.nine", new Keybind(Keys.D9));

			//Keyboard keys. Used for textbox handling
			BindKey("misc.left", new Keybind(Keys.Left));
			BindKey("misc.right", new Keybind(Keys.Right));
			BindKey("misc.shift", new Keybind(Keys.LeftShift));
			BindKey("misc.shift.alt", new Keybind(Keys.RightShift));
			BindKey("misc.alt", new Keybind(Keys.LeftAlt));
			BindKey("misc.alt2", new Keybind(Keys.RightAlt));
			BindKey("misc.control", new Keybind(Keys.LeftControl));
			BindKey("misc.control.alt", new Keybind(Keys.RightControl));
			BindKey("misc.alternate", new Keybind(Keys.LeftAlt));
			BindKey("misc.alternate.alt", new Keybind(Keys.RightAlt));
			BindKey("misc.capslock", new Keybind(Keys.CapsLock));
			BindKey("misc.scrolllock", new Keybind(Keys.Scroll));
			BindKey("misc.numlock", new Keybind(Keys.NumLock));
			BindKey("misc.home", new Keybind(Keys.Home));
			BindKey("misc.end", new Keybind(Keys.End));
			BindKey("misc.A", new Keybind(Keys.A));
			BindKey("misc.X", new Keybind(Keys.X));
			BindKey("misc.C", new Keybind(Keys.C));
			BindKey("misc.V", new Keybind(Keys.V));

			//Controller pausing
			BindKey("misc.pause", new Keybind(Keys.Escape, Buttons.Start));
			BindKey("misc.screenshot", new Keybind(Keys.F2, Buttons.Back));
			BindKey("misc.tab", new Keybind(Keys.Tab));
			BindKey("misc.fullscreen", new Keybind(Keys.F11));
			BindKey("misc.hideui", new Keybind(Keys.F1));

			//DEBUG
			BindKey("debug.hotkey", new Keybind(Keys.F3));
			BindKey("debug.renderchunks", new Keybind(Keys.P));
			BindKey("debug.physicscolliders", new Keybind(Keys.T));
			BindKey("debug.entitycolliders", new Keybind(Keys.B));
			BindKey("debug.respawn", new Keybind(Keys.R));
			BindKey("debug.orthographic", new Keybind(Keys.O));
			BindKey("debug.noclip", new Keybind(Keys.N));
		}

		private static void BindKey(string name, Keybind fallback)
		{
			if (!keybinds.ContainsKey(name.ToLower()))
			{
				//try to load the keybind from file
				keybinds.Add(name.ToLower(), fallback);
			}
			else
			{
				Logger.fatal(0, "An error occured when registering keybind \"" + name.ToLower() + "\"! (ERR_KEYBIND_CONFLICT)");
			}
		}

		/// <summary>
		/// Rebinds a key to the given Keybind
		/// </summary>
		/// <param name="name"></param>
		/// <param name="newBinding"></param>
		public static void RebindKey(string name, Keybind newBinding)
		{
			if (keybinds.ContainsKey(name.ToLower()))
			{
				Keybind toBind = keybinds[name.ToLower()];
				toBind.keyboardBinding = newBinding.keyboardBinding;
				toBind.gamepadBinding = newBinding.gamepadBinding;
				toBind.mouseBinding = newBinding.mouseBinding;
				keybinds[name.ToLower()] = toBind;
			}
		}

		/// <summary>
		/// Returns a keybind representing the currently pressed key
		/// </summary>
		/// <returns></returns>
		public static Keybind CaptureBinding()
		{
			Keybind currentBinding = new Keybind();

			//Get the first element in the current keyboard keys
			currentBinding.keyboardBinding = CurrentKeys.GetPressedKeys()[0];
			//Get the first element in the current gamepad state
			currentBinding.gamepadBinding = GetGamepadButtons();
			//Get the first element in the current mouse state
			currentBinding.mouseBinding = GetMouseButtons();

			return currentBinding;
		}

		#region Current getters
		private static Buttons GetGamepadButtons()
		{
			Buttons buttons = new Buttons();

			//DPad
			if (CurrentGamepad.IsButtonDown(Buttons.DPadUp))
			{
				buttons = Buttons.DPadUp;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.DPadDown))
			{
				buttons = Buttons.DPadDown;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.DPadLeft))
			{
				buttons = Buttons.DPadLeft;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.DPadRight))
			{
				buttons = Buttons.DPadRight;
			}

			//Start and Back
			else if (CurrentGamepad.IsButtonDown(Buttons.Start))
			{
				buttons = Buttons.Start;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.Back))
			{
				buttons = Buttons.Back;
			}

			//Shoulders and Triggers
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftShoulder))
			{
				buttons = Buttons.LeftShoulder;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightShoulder))
			{
				buttons = Buttons.RightShoulder;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightTrigger))
			{
				buttons = Buttons.RightTrigger;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftTrigger))
			{
				buttons = Buttons.LeftTrigger;
			}

			//Big button
			else if (CurrentGamepad.IsButtonDown(Buttons.BigButton))
			{
				buttons = Buttons.BigButton;
			}

			//Action buttons
			else if (CurrentGamepad.IsButtonDown(Buttons.A))
			{
				buttons = Buttons.A;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.B))
			{
				buttons = Buttons.B;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.X))
			{
				buttons = Buttons.X;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.Y))
			{
				buttons = Buttons.Y;
			}

			//Stick Clicks
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftStick))
			{
				buttons = Buttons.LeftStick;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightStick))
			{
				buttons = Buttons.RightStick;
			}

			//Right thumbstick
			else if (CurrentGamepad.IsButtonDown(Buttons.RightThumbstickUp))
			{
				buttons = Buttons.RightThumbstickUp;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightThumbstickDown))
			{
				buttons = Buttons.RightThumbstickDown;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightThumbstickRight))
			{
				buttons = Buttons.RightThumbstickRight;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.RightThumbstickLeft))
			{
				buttons = Buttons.RightThumbstickLeft;
			}

			//Left thumbstick
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftThumbstickUp))
			{
				buttons = Buttons.LeftThumbstickUp;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftThumbstickDown))
			{
				buttons = Buttons.LeftThumbstickDown;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftThumbstickRight))
			{
				buttons = Buttons.LeftThumbstickRight;
			}
			else if (CurrentGamepad.IsButtonDown(Buttons.LeftThumbstickLeft))
			{
				buttons = Buttons.LeftThumbstickLeft;
			}

			return buttons;
		}

		private static MouseButton GetMouseButtons()
		{
			MouseButton mouse = new MouseButton();

			//Check the button
			if (CurrentMouse.LeftButton == ButtonState.Pressed)
			{
				mouse = MouseButton.Button0;
			}
			else if (CurrentMouse.RightButton == ButtonState.Pressed)
			{
				mouse = MouseButton.Button1;
			}
			else if (CurrentMouse.MiddleButton == ButtonState.Pressed)
			{
				mouse = MouseButton.Button2;
			}

			else if (CurrentMouse.XButton1 == ButtonState.Pressed)
			{
				mouse = MouseButton.Button3;
			}
			else if (CurrentMouse.XButton2 == ButtonState.Pressed)
			{
				mouse = MouseButton.Button4;
			}

			return mouse;
		}
		#endregion

		#endregion

		#region Input Handling
		
		/// <summary>
		/// Centers the mouse
		/// </summary>
		public static void CenterMouse()
		{
			_centerMouse = true;
		}

		/// <summary>
		/// Returns whether the keybind is currently pressed.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool IsPressed(string name)
		{
			if (keybinds.ContainsKey(name.ToLower()))
			{
				return IsKeybindPressedCurrent(keybinds[name.ToLower()]);
			}

			Logger.error(0, "Keybind \"" + name.ToLower() + "\" doesn't exist! (ERR_KEYBIND_NULL)");

			return false;
		}

		/// <summary>
		/// Returns whether the keybind was released
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool Released(string name)
		{
			if (keybinds.ContainsKey(name.ToLower()))
			{
				return IsKeybindPressedCurrent(keybinds[name.ToLower()]) == false && IsKeybindPressedPrevious(keybinds[name.ToLower()]) == true;
			}

			Logger.error(0, "Keybind \"" + name.ToLower() + "\" doesn't exist! (ERR_KEYBIND_NULL)");

			return false;
		}

		/// <summary>
		/// Returns whether the keybind started being pressed
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool PressedStart(string name)
		{
			if (keybinds.ContainsKey(name.ToLower()))
			{
				return IsKeybindPressedCurrent(keybinds[name.ToLower()]) == true && IsKeybindPressedPrevious(keybinds[name.ToLower()]) == false;
			}

			Logger.error(0, "Keybind \"" + name.ToLower() + "\" doesn't exist! (ERR_KEYBIND_NULL)");

			return false;
		}
		#endregion

		#region Keybind Checks
		private static bool IsKeybindPressedCurrent(Keybind keybind)
		{
			bool isPressed = false;
			//if (IsGamepadConnected && UseGamepad)
			if (UseGamepad)
			{
				isPressed = CurrentGamepad.IsButtonDown(keybind.gamepadBinding);
			}
			if (!keybind.GamepadOnly)
			{
				if (!keybind.PreferKeyboard)
				{
					//Mouse keybinds
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button0 && CurrentMouse.LeftButton == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button1 && CurrentMouse.RightButton == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button2 && CurrentMouse.MiddleButton == ButtonState.Pressed;

					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button3 && CurrentMouse.XButton1 == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button4 && CurrentMouse.XButton2 == ButtonState.Pressed;
				}

				//Keyboard binding
				isPressed = isPressed || CurrentKeys.IsKeyDown(keybind.keyboardBinding);
			}
			return isPressed;
		}

		private static bool IsKeybindPressedPrevious(Keybind keybind)
		{
			bool isPressed = false;
			//if (IsGamepadConnected && UseGamepad)
			if (UseGamepad)
			{
				isPressed = PreviousGamepad.IsButtonDown(keybind.gamepadBinding);
			}
			if (!keybind.GamepadOnly)
			{
				if (!keybind.PreferKeyboard)
				{
					//Mouse keybinds
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button0 && PreviousMouse.LeftButton == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button1 && PreviousMouse.RightButton == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button2 && PreviousMouse.MiddleButton == ButtonState.Pressed;

					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button3 && PreviousMouse.XButton1 == ButtonState.Pressed;
					isPressed = isPressed || keybind.mouseBinding == MouseButton.Button4 && PreviousMouse.XButton2 == ButtonState.Pressed;
				}

				//Keyboard binding
				isPressed = isPressed || PreviousKeys.IsKeyDown(keybind.keyboardBinding);
			}
			return isPressed;
		}
		#endregion

		#region Update
		public static void Update()
		{
			//Set the current state
			CurrentKeys = Keyboard.GetState();
			CurrentMouse = Mouse.GetState();
		}

		public static void FinalUpdate()
		{
			if (_centerMouse)
			{
				Mouse.SetPosition(VoxelClient.Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / 2, VoxelClient.Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight / 2);
				_centerMouse = false;
			}

			//Save the current state
			PreviousKeys = CurrentKeys;
			PreviousMouse = Mouse.GetState();

			//Gamepad handling
			for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
			{
				GamePadCapabilities state = GamePad.GetCapabilities(i);
				if (state.IsConnected)
				{
					GamepadPort = i;
				}
			}
			GamePadCapabilities capabilities = GamePad.GetCapabilities(GamepadPort);
			IsGamepadConnected = capabilities.IsConnected;
			//if (IsGamepadConnected)
			{
				PreviousGamepad = CurrentGamepad;
				CurrentGamepad = GamePad.GetState(GamepadPort);
				UseGamepad = UseGamepad || CurrentGamepad.IsButtonDown(
					//If any button is down currently
					Buttons.DPadUp | Buttons.DPadDown | Buttons.DPadLeft | Buttons.DPadRight |
					Buttons.Start | Buttons.Back | Buttons.LeftStick | Buttons.RightStick |
					Buttons.LeftShoulder | Buttons.RightShoulder | Buttons.BigButton |
					Buttons.A | Buttons.B | Buttons.X | Buttons.Y |
					Buttons.RightTrigger | Buttons.LeftTrigger |
					Buttons.RightThumbstickUp | Buttons.RightThumbstickDown | Buttons.RightThumbstickRight | Buttons.RightThumbstickLeft |
					Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickUp | Buttons.LeftThumbstickDown | Buttons.LeftThumbstickRight);
			}
		}
		#endregion

		#region Helpers
		public static void InspectGamePad(int playerNum)
		{
			GamePadCapabilities gpc = GamePad.GetCapabilities(playerNum);

			System.Diagnostics.Debug.WriteLine("inspecting gamepad #" + playerNum);
			System.Diagnostics.Debug.WriteLine("\t type: " + gpc.GamePadType);

			System.Diagnostics.Debug.WriteLine("\t has left X joystick: " + gpc.HasLeftXThumbStick);
			System.Diagnostics.Debug.WriteLine("\t has left Y joystick: " + gpc.HasLeftYThumbStick);

			System.Diagnostics.Debug.WriteLine("\t has A button: " + gpc.HasAButton);
			System.Diagnostics.Debug.WriteLine("\t has B button: " + gpc.HasBButton);
			System.Diagnostics.Debug.WriteLine("\t has X button: " + gpc.HasXButton);
			System.Diagnostics.Debug.WriteLine("\t has Y button: " + gpc.HasYButton);

			System.Diagnostics.Debug.WriteLine("\t has back button: " + gpc.HasBackButton);
			System.Diagnostics.Debug.WriteLine("\t has big button: " + gpc.HasBigButton);
			System.Diagnostics.Debug.WriteLine("\t has start button: " + gpc.HasStartButton);

			System.Diagnostics.Debug.WriteLine("\t has Dpad Down button: " + gpc.HasDPadDownButton);
			System.Diagnostics.Debug.WriteLine("\t has Dpad Left button: " + gpc.HasDPadLeftButton);
			System.Diagnostics.Debug.WriteLine("\t has Dpad Right button: " + gpc.HasDPadRightButton);
			System.Diagnostics.Debug.WriteLine("\t has Dpad Up button: " + gpc.HasDPadUpButton);

			System.Diagnostics.Debug.WriteLine("\t has Left Shoulder button: " + gpc.HasLeftShoulderButton);
			System.Diagnostics.Debug.WriteLine("\t has Left Trigger button: " + gpc.HasLeftTrigger);
			System.Diagnostics.Debug.WriteLine("\t has Left Stick button: " + gpc.HasLeftStickButton);
			System.Diagnostics.Debug.WriteLine("\t has Left vibration motor: " + gpc.HasLeftVibrationMotor);

			System.Diagnostics.Debug.WriteLine("\t has Right Shoulder button: " + gpc.HasRightShoulderButton);
			System.Diagnostics.Debug.WriteLine("\t has Right Trigger button: " + gpc.HasRightTrigger);
			System.Diagnostics.Debug.WriteLine("\t has Right Stick button: " + gpc.HasRightStickButton);
			System.Diagnostics.Debug.WriteLine("\t has Right vibration motor: " + gpc.HasRightVibrationMotor);
		}
		#endregion
	}

	#region Enums
	public struct Keybind
	{
		public Keys keyboardBinding;
		public MouseButton mouseBinding;
		public Buttons gamepadBinding;

		/// <summary>
		/// Whether to use keyboard or mouse on this keybind
		/// </summary>
		public bool PreferKeyboard;
		/// <summary>
		/// Whether this keybind only applies on gamepads
		/// </summary>
		public bool GamepadOnly;

		#region Constructors
		public Keybind(Keys key)
		{
			keyboardBinding = key;
			mouseBinding = new MouseButton();
			gamepadBinding = new Buttons();

			GamepadOnly = false;
			PreferKeyboard = true;
		}

		public Keybind(Keys key, Buttons gamepad)
		{
			keyboardBinding = key;
			mouseBinding = new MouseButton();
			gamepadBinding = gamepad;

			GamepadOnly = false;
			PreferKeyboard = true;
		}

		public Keybind(Buttons gamepad)
		{
			keyboardBinding = new Keys();
			mouseBinding = new MouseButton();
			gamepadBinding = gamepad;

			GamepadOnly = true;
			PreferKeyboard = false;
		}

		public Keybind(MouseButton mouse, Buttons gamepad)
		{
			keyboardBinding = new Keys();
			mouseBinding = mouse;
			gamepadBinding = gamepad;

			GamepadOnly = false;
			PreferKeyboard = false;
		}

		public Keybind(Keys keyboard, MouseButton mouse, Buttons gamepad)
		{
			keyboardBinding = keyboard;
			mouseBinding = mouse;
			gamepadBinding = gamepad;

			GamepadOnly = false;
			PreferKeyboard = false;
		}
		#endregion
	}

	/// <summary>
	/// Defines a mouse button
	/// </summary>
	public enum MouseButton
	{
		/// <summary>
		/// Left Mouse Button
		/// </summary>
		Button0,
		/// <summary>
		/// Right Mouse Button
		/// </summary>
		Button1,
		/// <summary>
		/// Middle Mouse Button
		/// </summary>
		Button2,

		/// <summary>
		/// Macro 1
		/// </summary>
		Button3,
		/// <summary>
		/// Macro 2
		/// </summary>
		Button4,
	}
	#endregion
}
