using System.Collections.Generic;
using System.Diagnostics;

namespace CodeJam.Collections
{
	/// <summary>Naive implementation of the suffix tree</summary>
	/// see http://felix-halim.net/pg/suffix-tree/ for sample builder
	[DebuggerDisplay("{Print()}")]
	public class SuffixTreeNaive : SuffixTreeBase
	{
		/// <summary>Constructs a new suffix tree</summary>
		/// <param name="data">The string to build the suffix tree for</param>
		/// <returns>The suffix tree</returns>
		public static SuffixTreeNaive Build(string data) => Builder<SuffixTreeNaive>.Build(data);

		public static Builder<SuffixTreeNaive> CreateBuilder() => new Builder<SuffixTreeNaive>();

		protected override void BuildFor(int start, int end)
		{
			var root = Root;
			var data = InternalData;
			var childComparer = GetComparer();
			for (var i = end - 1; i >= start; --i)
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
