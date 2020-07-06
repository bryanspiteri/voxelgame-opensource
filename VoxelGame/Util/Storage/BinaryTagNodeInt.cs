using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Storage
{
	[ProtoContract]
	public class BinaryTagNodeInt : BinaryTagNode
	{
		[ProtoMember(3)]
		public int Value = 0;

		public BinaryTagNodeInt() : base()
		{
			Value = 0;
			_type = (byte)TagType.TagInt;
		}

		public BinaryTagNodeInt(string name) : base(name)
		{
			Value = 0;
			_type = (byte)TagType.TagInt;
		}

		public BinaryTagNodeInt(string name, int value) : base(name)
		{
			Value = value;
			_type = (byte)TagType.TagInt;
		}
	}
}
