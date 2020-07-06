using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace VoxelGame.Util
{
	public class Logger
	{
		private static string logFile = "";
		private static string folder = "";
		private static List<string> logContents = new List<string>();
		private static bool isSaving = false;
		public static bool LogToConsole = true;
		public static List<string> senders = new List<string>();

#if DEBUG
		private const ConsoleColor DebugColor = ConsoleColor.Gray;
#endif
		private const ConsoleColor InfoColor = ConsoleColor.White;
		private const ConsoleColor WarnColor = ConsoleColor.Yellow;
		private const ConsoleColor ErrorColor = ConsoleColor.Red;
		private const ConsoleColor FatalColor = ConsoleColor.DarkRed;

		public static void Initialize(string logFolder)
		{
			folder = logFolder;
			logFile = logFolder + "\\log-20" + DateTime.Now.ToString("yy-HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".log";


			try
			{
				Console.OutputEncoding = Encoding.UTF8;
#if WINDOWS
				Console.OutputEncoding = Encoding.Unicode;
#endif
			}
			catch
			{
			}
		}

		public static bool isSetup()
		{
			return logFile != null && logFile != "";
		}

		public static void Save()
		{
			if (!isSetup())
			{
				throw new NullReferenceException();
			}
			else
			{
				if (isSaving == false)
				{
					isSaving = true;
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					try
					{
						SaveLogToFile(logFile);

						//Latest.LOG
						SaveLogToFile(Path.Combine(GameClient.gameDirectory, "latest.log"));

						logContents.Clear();
					}
					catch (Exception err)
					{
						Console.Error.WriteLine(err.ToString());
					}
					isSaving = false;
				}
			}
		}

		private static void SaveLogToFile(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}

			// Create a file to write to.
			using (StreamWriter sw = File.CreateText(path))
			{
				for (int line = 0; line < logContents.Count; line++)
				{
					sw.WriteLine(logContents[line]);
				}
			}
		}

#region DEBUG
		public static void debug(int senderIndex, string message)
		{
#if DEBUG
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [DEBUG]: " + message;
			if (LogToConsole)
			{
				Console.ForegroundColor = DebugColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
#endif
		}

		public static void debug(int senderIndex, string message, params object[] args)
		{
#if DEBUG
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [DEBUG]: " + string.Format(message, args);
			if (LogToConsole)
			{
				Console.ForegroundColor = DebugColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
#endif
		}

		public static void debug(int senderIndex, object message)
		{
#if DEBUG
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [DEBUG]: " + message.ToString();
			if (LogToConsole)
			{
				Console.ForegroundColor = DebugColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
#endif
		}
#endregion

#region INFO
		public static void info(int senderIndex, string message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [INFO]: " + message;
			if (LogToConsole)
			{
				Console.ForegroundColor = InfoColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void info(int senderIndex, string message, params object[] args)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [INFO]: " + string.Format(message, args);
			if (LogToConsole)
			{
				Console.ForegroundColor = InfoColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void info(int senderIndex, object message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [INFO]: " + message.ToString();
			if (LogToConsole)
			{
				Console.ForegroundColor = InfoColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}
#endregion

#region WARN
		public static void warn(int senderIndex, string message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [WARNING]: " + message;
			if (LogToConsole)
			{
				Console.ForegroundColor = WarnColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void warn(int senderIndex, string message, params object[] args)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [WARNING]: " + string.Format(message, args);
			if (LogToConsole)
			{
				Console.ForegroundColor = WarnColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void warn(int senderIndex, object message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [WARNING]: " + message.ToString();
			if (LogToConsole)
			{
				Console.ForegroundColor = WarnColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}
#endregion

#region ERROR
		public static void error(int senderIndex, string message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [ERROR]: " + message;
			if (LogToConsole)
			{
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void error(int senderIndex, string message, params object[] args)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [ERROR]: " + string.Format(message, args);
			if (LogToConsole)
			{
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void error(int senderIndex, object message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [ERROR]: " + message.ToString();
			if (LogToConsole)
			{
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}
#endregion

#region FATAL
		public static void fatal(int senderIndex, string message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [FATAL]: " + message;
			if (LogToConsole)
			{
				Console.ForegroundColor = FatalColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void fatal(int senderIndex, string message, params object[] args)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [FATAL]: " + string.Format(message, args);
			if (LogToConsole)
			{
				Console.ForegroundColor = FatalColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}

		public static void fatal(int senderIndex, object message)
		{
			string prefix = "";

			if (0 <= senderIndex && senderIndex < senders.Count)
			{
				prefix = " [" + senders[senderIndex] + "]";
			}
			else
			{
				prefix = " [" + Thread.CurrentThread.Name + "]";
			}

			string msg = getSystemTime() + prefix + " [FATAL]: " + message.ToString();
			if (LogToConsole)
			{
				Console.ForegroundColor = FatalColor;
				Console.WriteLine(msg);
			}
			logContents.Add(msg);
		}
#endregion

		public static string getSystemTime()
		{
			return DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
		}
	}
}
