#if !CONSOLE
using System.Diagnostics;
using Discord;
using VoxelGame.Util;
#endif

namespace VoxelGame.Client
{
	public class DiscordHandler
	{
#if !CONSOLE
		public static Discord.Discord DiscordHook;
		public static bool IsDiscordAvailable = false;

		private static long startTime = 0x00;
#endif

		public void InitialiseDiscord()
		{
#if !CONSOLE
			//Check if discord is installed and running
			Process[] discordProcessesRunning = Process.GetProcessesByName("discord");
			if (discordProcessesRunning.Length != 0)
			{
				IsDiscordAvailable = true;
				Logger.info(1, "Discord is installed! Setting up Discord hooks...");
			}
			else
			{
				Logger.info(1, "Discord is not installed, or is not runnning! (DISCORD_NOT_FOUND)");
			}
			//TODO: Check application signatures

			IsDiscordAvailable = true;
			if (IsDiscordAvailable)
			{
				try
				{
					startTime = Time.UnixEpoch;
					DiscordHook = new Discord.Discord(629419523879206954, (ulong)CreateFlags.NoRequireDiscord);

					//Attach a logger into discord
					DiscordHook.SetLogHook(LogLevel.Info, (level, message) =>
					{
						switch (level)
						{
							case LogLevel.Info:
								Logger.info(1, message);
								break;
							case LogLevel.Warn:
								Logger.warn(1, message);
								break;
							case LogLevel.Error:
								Logger.error(1, message);
								break;
						}
					});
				}
				catch
				{
					IsDiscordAvailable = false;
					Logger.info(1, "Discord is not installed, or is not runnning! (DISCORD_NOT_FOUND)");
				}
			}
#endif
		}

		public void ShutDown()
		{
			if (IsDiscordAvailable)
			{
				DiscordHook.Dispose();
				Logger.info(1, "Successfully deregistered Discord hooks! Rich presence deactivated");
			}
		}

		public void OnUpdate()
		{
#if !CONSOLE
			try
			{
				if (IsDiscordAvailable)
				{
					DiscordHook.RunCallbacks();
				}
			}
			catch
			{
				IsDiscordAvailable = false;
				Logger.debug(1, "Discord is not installed, or is not runnning! (DISCORD_NOT_FOUND)");
			}
#endif
		}

		/// <summary>
		/// Sets the state of the discord client
		/// </summary>
		/// <param name="State">Basically description</param>
		/// <param name="Details">Basically title</param>
		public void SetState(string State, string Details)
		{
#if !CONSOLE
			if (IsDiscordAvailable)
			{
				var activityManager = DiscordHook.GetActivityManager();
				var activity = new Activity
				{
					State = State,
					Details = Details,
					Timestamps =
					{
						Start = startTime,
					},
					/*Assets =
					{
						LargeImage = "foo largeImageKey",
						LargeText = "foo largeImageText",
						SmallImage = "foo smallImageKey",
						SmallText = "foo smallImageText",
					},*/
					Instance = false,
				};
				activityManager.UpdateActivity(activity, (res) =>
				{
					if (res == Result.Ok)
					{
						Logger.debug(1, "Successfully changed rich presence!");
					}
					else
					{
						Logger.fatal(1, "An error occured when setting the rich presence! (ID: " + res.ToFriendlyString() + ")");
					}
				});
			}
#endif
		}
	}
}
