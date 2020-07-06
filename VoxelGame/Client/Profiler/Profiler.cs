using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Client
{
	public class Profiler
	{
		public static Dictionary<string, Stopwatch> Timers = new Dictionary<string, Stopwatch>();
		public static Dictionary<string, Color> ProfilerColors = new Dictionary<string, Color>();

		public static void Start(string id)
		{
			lock (Timers)
			{
				if (!Timers.ContainsKey(id))
				{
					Timers.Add(id, new Stopwatch());
				}
				Timers[id].Start();
			}

			lock (ProfilerColors)
			{
				if (!ProfilerColors.ContainsKey(id))
				{
					ProfilerColors.Add(id, Color.Yellow);
				}
			}
		}

		public static void Start(string id, Color color)
		{
			lock (Timers)
			{
				if (!Timers.ContainsKey(id))
				{
					Timers.Add(id, new Stopwatch());
				}
				Timers[id].Start();
			}

			lock (ProfilerColors)
			{
				//Profiling colors
				if (!ProfilerColors.ContainsKey(id))
				{
					ProfilerColors.Add(id, color);
				}
			}
		}

		public static void Reset(string id)
		{
			lock (ProfilerColors)
			{
				if (Timers.ContainsKey(id))
				{
					Timers[id].Reset();
				}
			}
		}

		public static void Stop(string id)
		{
			lock (ProfilerColors)
			{
				if (Timers.ContainsKey(id))
				{
					Timers[id].Stop();
				}
			}
		}
	}
}
