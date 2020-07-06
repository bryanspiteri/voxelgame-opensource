using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;
using VoxelGame.Util;
using VoxelGame.Util.Math;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
#if !CONSOLE
using System.Management;
#endif

namespace VoxelGame.Client.UI
{
	public class DebugScreen : GuiScreen
	{
		public UiText debugTxtLeft;
		public UiText debugTxtRight;
		public ProfilerGraphRenderer profiler;

		public string GPUName = "Unidentified GPU";
		public string CPUName = "Unidentified CPU";
		public string RAMInfo = "0";
		public string RAMInfoGB = "0";

		public bool RenderProfilerGraph = true;
		public bool RenderFramerateGraph = true;

		public DebugScreen()
		{
			debugTxtLeft = new UiText(this, 5, 5, "", Color.White);
			AddComponent(debugTxtLeft);

			debugTxtRight = new UiText(this, 5, 5, "", Color.White);
			debugTxtRight.AlignRight = true;
			AddComponent(debugTxtRight);

			profiler = new ProfilerGraphRenderer(this, 0, 0);
			AddComponent(profiler);

			visible = false;

			#region System Data
#if !CONSOLE
			//Get the system data, and cache it into the strings (you cant power off a machine, change the cpu and still be running the same apps)
			ManagementObjectSearcher videoCardInfo = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
			ManagementObjectSearcher processorInfo = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
			ManagementObjectSearcher memoryInfo = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");

			//Reset RAM, CPU, GPU info
			GPUName = "";
			CPUName = "";
			RAMInfo = "";
			RAMInfoGB = "";

			//Get the GPU using MonoGame APIs. If it fails, return all connected GPUs
			GPUName = GraphicsAdapter.DefaultAdapter.Description;
			if (GPUName == "" || GPUName == null)
			{
				foreach (ManagementObject obj in videoCardInfo.Get())
				{
					//"Intel(R) HD Graphics 5500 (AdapterRAM (int32) B => GB) GB"
					//"Name (AdapterRAM GB)"
					GPUName += obj["Name"] + " (" + (uint.Parse(obj["AdapterRAM"].ToString()) / 1024 / 1024 / 1024) + " GB)\n";
				}
			}

			foreach (ManagementObject obj in processorInfo.Get())
			{
				//"Intel(R) Core(TM) i7-5600U CPU @ 2.60GHz (2/2; 4) (64)"
				//"Name (CoreCount/EnabledCoreCount; LogicalProcessors) (AddressWidth)"
				CPUName += obj["Name"] + " (" + obj["NumberOfCores"] + "/" + obj["NumberOfCores"]
					+ "; " + obj["NumberOfLogicalProcessors"] + ") (" + obj["AddressWidth"] + ")\n";
			}

			ulong ramCnt = 0;
			foreach (ManagementObject obj in memoryInfo.Get())
			{
				//"Samsung 4GB RAM @ 1600 MHz"
				//"Manufacturer MemoryType => GB RAM @ Speed MHz"
				ramCnt += ulong.Parse(obj["Capacity"].ToString()) / 1024 / 1024;
			}
			RAMInfo = ramCnt.ToString();
			RAMInfoGB = (ramCnt / 1024).ToString();

			if (GPUName == "" || GPUName == null)
			{
				GPUName = "Unidentified GPU";
			}
			if (CPUName == "" || CPUName == null)
			{
				CPUName = "Unidentified CPU";
			}
			if (RAMInfo == "" || RAMInfo == null)
			{
				RAMInfo = "0";
			}
			if (RAMInfoGB == "" || RAMInfoGB == null)
			{
				RAMInfoGB = "0";
			}
#endif
			#endregion
		}

		public override void Update()
		{
			if (InputManager.Released("debug.hotkey"))
			{
				visible = !visible;
			}

			if (visible)
			{
				//Update text
				debugTxtLeft.text = "VoxelGame (" + GameClient.Instance.Version + ") \n"
				+ "FPS: " + GameClient.FPS //TODO: World info and stuff;
				+ "\nThreads: " + Util.Managers.ThreadManager.ActiveThreadCount + " / " + (Util.Managers.ThreadManager.MaxThreads + Util.Managers.ThreadManager.threads.Count);

				if (GameClient.IsGameWorldloaded)
				{
					debugTxtLeft.text += "\n\n"
					+ "Block Position X: " + GameClient.World.player.blockPosition.X + " Y: " + GameClient.World.player.blockPosition.Y + " Z: " + GameClient.World.player.blockPosition.Z
					+ " Chunk X: " + GameClient.World.player.ChunkPosition.X + " Y: " + GameClient.World.player.ChunkPosition.Y + " Z: " + GameClient.World.player.ChunkPosition.Z
					+ "\nChunks Loaded: " + GameClient.theWorld.theChunkManager.chunks.Count
					+ "\nChunk Updates: " + GameClient.theWorld.ChunkUpdates + " Block Updates: " + GameClient.theWorld.BlockUpdates
					+ "\nX: " + GameClient.World.player.position.X.ToString("n2") + " Y: " + GameClient.World.player.position.Y.ToString("n2") + " Z: " + GameClient.World.player.position.Z.ToString("n2")
					+ "\nGrounded: " + GameClient.World.player.IsGrounded
					+ "\nVelocity: " + GameClient.World.player.Velocity
					+ "\nRotation: " + MathHelper.ToDegrees(GameClient.World.player.camera.Yaw).ToString("n2") + " " + MathHelper.ToDegrees(GameClient.World.player.camera.Pitch).ToString("n2");
					if (GameClient.World.player.SelectedBlock != null && GameClient.World.player.rayHit != null)
					{
						Block CurrentBlock = GameClient.theWorld.GetBlockAt(GameClient.World.player.SelectedBlock);
						debugTxtLeft.text += "\nLooking at " + CurrentBlock.Title + " (" + CurrentBlock.Id //Block id
							+ ") X: " + GameClient.World.player.SelectedBlock.X + " Y: " + GameClient.World.player.SelectedBlock.Y + " Z: " + GameClient.World.player.SelectedBlock.Z + " Side: " + GameClient.World.player.rayHit.sideHit.ToFriendlyString();
					}
				}

				//Get RAM Usage
				var memory = 0.0;
				using (Process proc = Process.GetCurrentProcess())
				{
					// The proc.PrivateMemorySize64 will returns the private memory usage in byte.
					// Would like to Convert it to Megabyte? divide it by 2^20
					memory = proc.PrivateMemorySize64 / (1024 * 1024);
				}

				debugTxtRight.text = "System Information\n" + CPUName + RAMInfoGB + "GB RAM Installed (" + memory + "/" + RAMInfo + ")\n" + GPUName;
				debugTxtRight.location = new Point(GameClient.ViewWidth - 5, 5);

				if (RenderProfilerGraph)
				{
					profiler.size = new Point(profiler.rawSize.X * GameSettings.UIScale, profiler.rawSize.Y * GameSettings.UIScale);
					profiler.location = new Point(GameClient.ViewWidth - profiler.size.X, GameClient.ViewHeight - profiler.size.Y);
				}
			}

			base.Update();
		}


#if !CONSOLE
		/// <summary>
		/// Returns the type of RAM as a string given a ushort RamType ID
		/// </summary>
		/// <param name="RAMType"></param>
		/// <returns></returns>
		public static string GetRamTypeFriendlyString(ushort RAMType)
		{
			switch (RAMType)
			{
				case 1:
					return "Other";
				case 2:
					return "DRAM";
				case 3:
					return "Synchronous DRAM";
				case 4:
					return "Cache DRAM";
				case 5:
					return "EDO";
				case 6:
					return "EDRAM";
				case 7:
					return "VRAM";
				case 8:
					return "SRAM";
				case 9:
					return "RAM";
				case 10:
					return "ROM";
				case 11:
					return "Flash";
				case 12:
					return "EEPROM";
				case 13:
					return "FEPROM";
				case 14:
					return "EPROM";
				case 15:
					return "CDRAM";
				case 16:
					return "3DRAM";
				case 17:
					return "SDRAM";
				case 18:
					return "SGRAM";
				case 19:
					return "RDRAM";
				case 20:
					return "DDR";
				case 21:
					return "DDR2";
				case 22:
					return "DDR2 FB-DIMM";
				case 24:
					return "DDR3";
				case 25:
					return "FBD2";
				default:
					return "Unknown";
			}
		}
#endif
	}
}
