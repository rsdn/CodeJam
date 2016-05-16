using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeJam.Collections
{
	/// <summary>Naive implementation of the suffix tree</summary>
	[DebuggerDisplay("{Print()}")]
	public class SuffixTreeNaive : SuffixTreeBase
	{
		/// <summary>Constructs a new suffix tree</summary>
		/// <param name="data">The string to build the suffix tree for</param>
		/// <param name="terminal">
		/// The terminal character
		/// <remarks>
		/// Used if the last character of the string is present in it more than once.
		/// Should be different from any character in the string
		/// </remarks>
		/// </param>
		protected SuffixTreeNaive(string data, char terminal) : base(data, terminal)
		{
		}

		/// <summary>Constructs a new suffix tree</summary>
		/// <param name="data">The string to build the suffix tree for</param>
		/// <param name="terminal">
		/// The terminal character
		/// <remarks>
		/// Used if the last character of the string is present in it more than once.
		/// Should be different from any character in the string
		/// </remarks>
		/// </param>
		public static SuffixTreeNaive Create(string data, char terminal = char.MaxValue)
		{
			var t = new SuffixTreeNaive(data, terminal);
			t.Construct();
			return t;
		}

		protected override void Build()
		{
			var root = Root;
			var data = InternalData;
			var end = data.Length;
			Func<int, char, int> childComparer = (index, c) =>
			{
				var childFirstChar = data[GetNode(index).Begin];
				return childFirstChar - c;
			};
			for (var i = InternalData.Length - 1; i >= 0; --i)
			{
				var currentNode = root;
				var begin = i;
				for (;;)
				{
					var children = currentNode.Children;
					int nextNodeIndex;
					if (children == null)
					{
						children = new List<int>();
						currentNode.Children = children;
						// force first child addition
						nextNodeIndex = 0;
					}
					else
					{
						nextNodeIndex = children.LowerBound(data[begin], childComparer);
					}

					if (nextNodeIndex == children.Count
						|| data[GetNode(children[nextNodeIndex]).Begin] != data[begin])
					{
						// add new child
						var node = new Node(begin, end);
						var index = AddNode(node);
						currentNode.Children.Insert(nextNodeIndex, index);
						break;
					}
					// there is already a suffix which starts from the same char
					// find the match length
					var nextNode = GetNode(children[nextNodeIndex]);
					var nextBegin = nextNode.Begin;
					var nextEnd = nextNode.End;
					var diffIndex = nextBegin + 1;
					++begin;
					while (diffIndex < nextEnd && data[diffIndex] == data[begin])
					{
						++diffIndex;
						++begin;
					}
					if (diffIndex != nextEnd)
					{
						// split the nextNode
						var splitNode = new Node(diffIndex, nextEnd) { Children = nextNode.Children };
						var splitIndex = AddNode(splitNode);
						nextNode.End = diffIndex;
						nextNode.Children = new List<int> { splitIndex };
					}
					// continue search from the next node
					currentNode = nextNode;
				}
			}
		}
	}
}
