using System;
using VoxelGame.Client;
using VoxelGame.Client.UI;

namespace VoxelGame
{
	public static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			//TODO: Console overides (resolution and game dir)

			//Sample params:
			//voxelgame.exe --gameDirectory=%appdata%\.voxelgame --width=1280 --height=720

			//Parse the options
			string gameDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , ".voxelgame");
			int width = 853;
			int height = 480;
			for (int y = 0; y < args.Length; y++)
			{
				string argument, value;
				argument = args[y].Substring(0, args[y].IndexOf('='));
				value = args[y].Substring(args[y].IndexOf('=') + 1);
				//GameClient.LOGGER.info("ARG: " + argument + ", VAL: " + value);
				if (argument == "--gameDirectory")
				{
					if (value.Substring(value.Length - 1) == "\\" || value.Substring(value.Length - 1) == "/") //trim the folder seperator from the end
					{
						value = value.Substring(0, value.Length - 1);
					}
					gameDir = value;
				}
				if (argument == "--width")
				{
					width = Int32.Parse(value);
				}
				if (argument == "--height")
				{
					height = Int32.Parse(value);
				}
			}

			using (var game = new GameClient(gameDir))
			{
				try
				{
					game.init(width, height);
					game.Run();
				}
				catch (Exception error)
				{
					Util.Logger.fatal(-1, error.ToString());
#if !CONSOLE
					if (UiRegisterer.debugUI != null)
					{
						//Get RAM Usage
						var memory = 0.0;
						using (System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess())
						{
							// The proc.PrivateMemorySize64 will returns the private memory usage in byte.
							// Would like to Convert it to Megabyte? divide it by 2^20
							memory = proc.PrivateMemorySize64 / (1024 * 1024);
						}

						Util.Logger.info(-1, "System Information\n" + UiRegisterer.debugUI.CPUName + UiRegisterer.debugUI.RAMInfoGB + "GB RAM Installed ("
							+ memory + "/" + UiRegisterer.debugUI.RAMInfo + ")\n" + UiRegisterer.debugUI.GPUName);
					}
#endif
					Util.Logger.Save();

					//Kill all threads
					Util.Managers.ThreadManager.KillAllThreads();
				}
			}
		}
	}
}
