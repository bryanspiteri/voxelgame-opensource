using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util.Storage
{
	[ProtoContract]
	public class BinaryTagNodeList : BinaryTagNode
	{
		[ProtoMember(3)]
		public List<BinaryTagNode> Nodes;

		public BinaryTagNodeList() : base()
		{
			Nodes = new List<BinaryTagNode>();
			_type = (byte)TagType.TagList;
		}

		public BinaryTagNodeList(string name) : base(name)
		{
			Nodes = new List<BinaryTagNode>();
			_type = (byte)TagType.TagList;
		}

		public BinaryTagNodeList(string name, List<BinaryTagNode> nodes) : base(name)
		{
			Nodes = nodes;
			_type = (byte)TagType.TagList;
		}

		#region Helper Methods

		public int Count
		{
			get
			{
				return Nodes.Count;
			}
		}

		public bool Contains(BinaryTagNode node)
		{
			return Nodes.Contains(node);
		}

		public void Remove(BinaryTagNode node)
		{
			Nodes.Remove(node);
		}

		public void Add(BinaryTagNode node)
		{
			if (node.Type == (TagType.TagList | TagType.TagByteArray | TagType.TagIntArray | TagType.TagLongArray | TagType.TagEnd))
			{
				throw new Exception("InvalidBinaryTagNodeException\nTag is not a primitive BinaryTagNode / BinaryTagNodeMultiple");
			}

			Nodes.Add(node);
		}

		public BinaryTagNode this[int index]
		{
			get
			{
				return Nodes[index];
			}
			set
			{
				Nodes[index] = value;
			}
		}

		public BinaryTagNode Get(int index)
		{
			return Nodes[index];
		}

		#endregion
	}
}
