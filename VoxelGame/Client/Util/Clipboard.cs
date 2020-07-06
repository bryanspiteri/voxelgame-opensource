using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util;

namespace VoxelGame.Client
{
	public static class Clipboard
	{
		public static bool UseLocalClipboard = false;
		private static string _localTextClipboard;

		static Action<string> setAction = CreateSet();
		static Func<string> getFunc = CreateGet();

		public static void SetText(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			if (UseLocalClipboard)
			{
				_localTextClipboard = text;
				return;
			}

			setAction(text);
		}

		public static string GetText()
		{
			if (UseLocalClipboard)
			{
				return _localTextClipboard;
			}

			return getFunc();
		}

		static Action<string> CreateSet()
		{
			if (CurrentPlatform.OS == OS.Windows)
			{
				return WindowsClipboard.SetText;
			}

			if (CurrentPlatform.OS == OS.MacOSX)
			{
				return OsxClipboard.SetText;
			}

			if (CurrentPlatform.OS == OS.Linux)
			{
				return LinuxClipboard.SetText;
			}

			return DefaultSetText;
		}

		static Func<string> CreateGet()
		{
			if (CurrentPlatform.OS == OS.Windows)
			{
				return WindowsClipboard.GetText;
			}

			if (CurrentPlatform.OS == OS.MacOSX)
			{
				return OsxClipboard.GetText;
			}

			if (CurrentPlatform.OS == OS.Linux)
			{
				return LinuxClipboard.GetText;
			}

			return DefaultGetText;
		}

		private static void DefaultSetText(string text) { }
		private static string DefaultGetText() { return ""; }
	}
}
