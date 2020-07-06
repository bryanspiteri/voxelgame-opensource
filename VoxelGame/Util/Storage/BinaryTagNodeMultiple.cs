using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace VoxelGame.Util.Storage
{
	/// <summary>
	/// A BinaryTagNode that allows multiple unique values to be assigned to it. Used for structuring data.
	/// </summary>
	[ProtoContract]
	public class BinaryTagNodeMultiple : BinaryTagNode
	{
		[ProtoMember(3)]
		public SortedDictionary<string, BinaryTagNode> Nodes;

		public BinaryTagNodeMultiple() : base()
		{
			Nodes = new SortedDictionary<string, BinaryTagNode>();
			_type = (byte)TagType.TagMultiple;
		}

		public BinaryTagNodeMultiple(string name) : base(name)
		{
			Nodes = new SortedDictionary<string, BinaryTagNode>();
			_type = (byte)TagType.TagMultiple;
		}

		#region Helper Methods
		public BinaryTagNode this[string tagName]
		{
			get
			{
				return Nodes[tagName];
			}
			set
			{
				Nodes[tagName] = value;
			}
		}

		public void Add(BinaryTagNode node)
		{
			if (Nodes.ContainsKey(node.Name))
			{
				throw new ArgumentException("DuplicateBinaryTagNodeException: The BinaryTagNode \"" + node.Name + "\" already exists!");
			}
			Nodes.Add(node.Name, node);
		}

		public BinaryTagNode Get(string name)
		{
			if (Nodes.ContainsKey(name))
			{
				return Nodes[name];
			}
			return null;
		}

		public bool Contains(BinaryTagNode node)
		{
			return Nodes.ContainsValue(node);
		}

		public void Remove(BinaryTagNode node)
		{
			Nodes.Remove(node.Name);
		}
		#endregion
	}
}
