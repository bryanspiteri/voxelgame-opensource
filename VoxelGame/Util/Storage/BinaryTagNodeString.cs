using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Storage
{
	[ProtoContract]
	public class BinaryTagNodeString : BinaryTagNode
	{
		[ProtoMember(4)]
		private string _value = "";
		[ProtoMember(3)]
		public int Length = 0;

		public string Value
		{
			get { return _value; }
			set
			{
				_value = value;
				Length = value.Length;
			}
		}

		public BinaryTagNodeString() : base()
		{
			Value = "";
			_type = (byte)TagType.TagString;
		}

		public BinaryTagNodeString(string name) : base(name)
		{
			Value = "";
			_type = (byte)TagType.TagString;
		}

		public BinaryTagNodeString(string name, string value) : base(name)
		{
			Value = value;
			Length = Value.Length;
			_type = (byte)TagType.TagString;
		}
	}
}
