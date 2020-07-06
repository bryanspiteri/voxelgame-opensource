using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Storage
{
	[ProtoContract]
	public class BinaryTagNodeIntArray : BinaryTagNode
	{
		[ProtoMember(3)]
		public int[] Values;

		public BinaryTagNodeIntArray() : base()
		{
			Values = new int[1024];
			_type = (byte)TagType.TagIntArray;
		}

		public BinaryTagNodeIntArray(string name) : base(name)
		{
			Values = new int[1024];
			_type = (byte)TagType.TagIntArray;
		}

		public BinaryTagNodeIntArray(string name, int size) : base(name)
		{
			Values = new int[size];
			_type = (byte)TagType.TagIntArray;
		}

		public BinaryTagNodeIntArray(string name, int[] data) : base(name)
		{
			Values = data;
			_type = (byte)TagType.TagIntArray;
		}

		#region Helper Methods

		public int this[int index]
		{
			get
			{
				return Values[index];
			}
			set
			{
				Values[index] = value;
			}
		}

		public int Length
		{
			get
			{
				return Values.Length;
			}
		}

		#endregion
	}
}
