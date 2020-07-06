#if CLIENT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VoxelGame.Util;

namespace VoxelGame
{
	public static class Snooper
	{
		private static Dictionary<string, double> snoopingTracker = new Dictionary<string, double>();
		private static Dictionary<string, double> snoopingTrackerFile = new Dictionary<string, double>();

		private static string SnoopFile = "snooper.txt";

		public static void Snoop(string snoop_key, double value)
		{
			if (snoopingTracker.ContainsKey(snoop_key))
			{
				snoopingTracker[snoop_key] += value;
			}
			else
			{
				snoopingTracker.Add(snoop_key, value);
			}
		}

		public static void SetSnoop(string snoop_key, double value)
		{
			if (snoopingTracker.ContainsKey(snoop_key))
			{
				snoopingTracker[snoop_key] = value;
			}
			else
			{
				snoopingTracker.Add(snoop_key, value);
			}
		}

		public static double GetSnoop(string snoop_key)
		{
			if (snoopingTracker.ContainsKey(snoop_key))
			{
				return snoopingTracker[snoop_key];
			}
			return 0D;
		}

		public static void Initialise()
		{
			//Load snooping data from file
			snoopingTrackerFile.Clear();
			if (File.Exists(Path.Combine(GameClient.gameDirectory, SnoopFile)))
			{
				string[] snoop_data_tmp = File.ReadAllLines(Path.Combine(GameClient.gameDirectory, SnoopFile));

				for (int i = 0; i < snoop_data_tmp.Length; i++)
				{
					string value, param;
					value = snoop_data_tmp[i].Substring(0, snoop_data_tmp[i].IndexOf('=')).Trim();
					param = snoop_data_tmp[i].Substring(snoop_data_tmp[i].IndexOf('=') + 1).Trim();

					//Default to 0f
					double theData = 0f;
					//try parsing the data
					double.TryParse(param, out theData);

					Snoop(value, theData);
				}
			}
		}

		public static void Save()
		{
			//Combine the dictionaries
			string[] snoopingTrackerKeys = snoopingTracker.Keys.ToArray();
			for (int i = 0; i < snoopingTracker.Count; i++)
			{
				//If the key is already there, combine the values by means of addition
				if (snoopingTrackerFile.ContainsKey(snoopingTrackerKeys[i]))
				{
					snoopingTrackerFile[snoopingTrackerKeys[i]] += snoopingTracker[snoopingTrackerKeys[i]];
				}
				//otherwise add the value
				else
				{
					snoopingTrackerFile.Add(snoopingTrackerKeys[i], snoopingTracker[snoopingTrackerKeys[i]]);
				}
			}
			//Convert snoopingTrackerFile to a string[] that we can save
			string[] snoopingFile = new string[snoopingTrackerFile.Count];

			string[] snoopingTrackerFileKeys = snoopingTrackerFile.Keys.ToArray();
			for (int i = 0; i < snoopingTrackerFile.Count; i++)
			{
				snoopingFile[i] = snoopingTrackerFileKeys[i] + "=" + snoopingTrackerFile[snoopingTrackerFileKeys[i]];
			}

			//Save the snooping data
			File.WriteAllLines(Path.Combine(GameClient.gameDirectory, SnoopFile), snoopingFile);

			Logger.info(0, "Saved snooping data to \"" + Path.Combine(GameClient.gameDirectory, SnoopFile) + "\"!");

			//Clear the snooping cache, excluding special values
			snoopingTracker.Clear();
		}
	}
}
#endif