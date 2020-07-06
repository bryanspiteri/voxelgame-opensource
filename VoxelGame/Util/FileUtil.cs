using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Util.Storage;

namespace VoxelGame.Util
{
	public class FileUtil
	{
		/// <summary>
		/// Returns the absolute file path of a resource. This method is useful due to resource packs
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static string GetPath(string relativePath)
		{
			//Check the resource packs

			//Get the game file
			string finalPath = Path.Combine(GameClient.ContentDirectory, relativePath);

			for (int i = GameClient.ResourcePacks.Count - 1; i > 0; i--) 
			{
				//Attempt to find the file under a ResourcePack
				//If the file exists, change final path
				string thePath = Path.Combine(GameClient.ResourcePacks[i], relativePath);
				if (File.Exists(Path.GetFullPath(thePath)))
				{
					finalPath = thePath;
				}
			}

			//TODO: Hashes maybe? For files shared between versions other than textures and english language

			return Path.GetFullPath(finalPath);
		}

		/// <summary>
		/// Deletes a directory, including all subdirectories and files. Returns whether the result was successful
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns>Whether anything was deleted</returns>
		public static bool EraseDirectory(string folderPath)
		{
			//Safety check for directory existence.
			if (!Directory.Exists(folderPath))
				return false;

			foreach (string file in Directory.GetFiles(folderPath))
			{
				File.Delete(file);
			}

			//Iterate to sub directory only if required.
			foreach (string dir in Directory.GetDirectories(folderPath))
			{
				EraseDirectory(dir);
			}

			//Delete the parent directory before leaving
			Directory.Delete(folderPath);
			return true;
		}

		//TODO: Packet to Stream ; Packet from Stream

		#region Serialization / Deserialization
		#region BinaryTagNode <=> File Conversion
		public static BinaryTagNode DeseralizeFromFile(string binaryFile)
		{
			BinaryTagNode toReturn;

			if (File.Exists(binaryFile))
			{
				using (Stream stream = File.Open(binaryFile, FileMode.Open))
				{
					using (var decompressor = new GZipStream(stream, CompressionMode.Decompress))
					{
						//var binaryFormatter = new BinaryFormatter();
						//binaryFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
						//toReturn = (BinaryTagNode)binaryFormatter.Deserialize(decompressor);
						toReturn = Serializer.Deserialize<BinaryTagNode>(decompressor);
					}
				}
			}
			else
			{
				toReturn = new BinaryTagNode();
			}

			return toReturn;
		}

		public static void SerializeToFile(string outputFile, BinaryTagNode nodes)
		{
			//Ensure the directory exists
			if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
			}

			using (Stream stream = File.Open(outputFile, FileMode.Create))
			{
				using (var compressor = new GZipStream(stream, CompressionMode.Compress))
				{
					using (var memStream = new MemoryStream())
					{
						//BinaryFormatter formatter = new BinaryFormatter();
						//formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
						//formatter.Serialize(compressor, nodes);
						Serializer.Serialize(compressor, nodes);
					}
				}

				stream.Close();
			}
		}
		#endregion

		#region T <=> MemoryStream
		public static MemoryStream SerializeToStream<T>(T value)
		{
			MemoryStream stream = new MemoryStream();
			//BinaryFormatter formatter = new BinaryFormatter();
			//formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			//formatter.Serialize(stream, value);
			Serializer.Serialize(stream, value);
			return stream;
		}

		public static T DeserializeFromStream<T>(Stream stream)
		{
			//BinaryFormatter formatter = new BinaryFormatter();
			//formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			stream.Seek(0, SeekOrigin.Begin);
			//T value = (T)formatter.Deserialize(stream);
			T value = Serializer.Deserialize<T>(stream);
			return value;
		}
		#endregion

		#region T <=> Byte[]
		public static byte[] BinarySerializeToBytes<T>(T value) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}

			using (MemoryStream memoryStream = SerializeToStream(value))
			{
				return memoryStream.ToArray();
			}
		}

		public static T BinaryDeserializeFromBytes<T>(byte[] bytes)
										 where T : class
		{
			if (bytes == null || bytes.Length == 0)
			{
				throw new InvalidOperationException();
			}

			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				return DeserializeFromStream<T>(memoryStream);
			}
		}
		#endregion

		#region Byte[] GZip De/Compression
		public static byte[] Compress(byte[] data)
		{
			using (var compressedStream = new MemoryStream())
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
			{
				zipStream.Write(data, 0, data.Length);
				zipStream.Close();
				return compressedStream.ToArray();
			}
		}

		public static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data))
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			using (var resultStream = new MemoryStream())
			{
				zipStream.CopyTo(resultStream);
				return resultStream.ToArray();
			}
		}
		#endregion
		#endregion
	}
}
