using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Storage
{
	[ProtoContract]

	#region Inheritance
	[ProtoInclude(7,  typeof(BinaryTagNodeByte))]
	[ProtoInclude(8,  typeof(BinaryTagNodeByteArray))]
	[ProtoInclude(9,  typeof(BinaryTagNodeDouble))]
	[ProtoInclude(10, typeof(BinaryTagNodeFloat))]
	[ProtoInclude(11, typeof(BinaryTagNodeInt))]
	[ProtoInclude(12, typeof(BinaryTagNodeIntArray))]
	[ProtoInclude(13, typeof(BinaryTagNodeList))]
	[ProtoInclude(14, typeof(BinaryTagNodeLong))]
	[ProtoInclude(15, typeof(BinaryTagNodeLongArray))]
	[ProtoInclude(16, typeof(BinaryTagNodeMultiple))]
	[ProtoInclude(17, typeof(BinaryTagNodeShort))]
	[ProtoInclude(18, typeof(BinaryTagNodeString))]
	[ProtoInclude(19, typeof(BinaryTagNodeUInt))]
	[ProtoInclude(20, typeof(BinaryTagNodeUShort))]
	#endregion

	public class BinaryTagNode
	{
		[ProtoMember(2)]
		public string Name = "";
		[ProtoMember(1)]
		private protected byte _type;

		/// <summary>
		/// The type of tag type file is. Used for serialization
		/// </summary>
		public TagType Type
		{
			get { return (TagType)_type; }
			private set
			{
				_type = (byte)value;
			}
		}

		public BinaryTagNode() { Type = TagType.TagEnd; }

		public BinaryTagNode(string name)
		{
			Name = name;
			Type = TagType.TagEnd;
		}
	}

	public enum TagType
	{
		//Misc
		TagEnd = 0,
		TagMultiple,

		//Primitives
		TagByte,
		TagShort,
		TagUShort,
		TagInt,
		TagUInt,
		TagLong,
		TagFloat,
		TagDouble,
		TagString,

		//Lists
		TagList,
		TagByteArray,
		TagIntArray,
		TagLongArray,
	}
}
