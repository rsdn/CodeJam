using System.Collections.Generic;
using System.Diagnostics;

namespace CodeJam.Collections
{
	/// <summary>Naive implementation of the suffix tree</summary>
	/// see http://felix-halim.net/pg/suffix-tree/ for sample builder
	[DebuggerDisplay("{Print()}")]
	public class SuffixTreeNaive : SuffixTreeBase
	{
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
					var forceNew = false;
					if (children == null)
					{
						children = new List<int>();
						currentNode.Children = children;
						if (currentNode.IsTerminal)
						{
							// add new empty terminal node
							var terminalEnd = currentNode.End;
							var newTerminal = new Node(terminalEnd, terminalEnd, true);
							var index = AddNode(newTerminal);
							children.Add(index);
							currentNode.MakeNonTerminal();
						}
						// force new child addition
						nextNodeIndex = children.Count;
						forceNew = true;
					}
					else
					{
						if (begin == end)
						{
							nextNodeIndex = 0;
							forceNew = true;
						}
						else
						{
							nextNodeIndex = children.LowerBound(data[begin], childComparer);
						}
					}

					if (forceNew
						|| nextNodeIndex == children.Count
						|| data[GetNode(children[nextNodeIndex]).Begin] != data[begin])
					{
						// add new child
						var node = new Node(begin, end, true);
						var index = AddNode(node);
						children.Insert(nextNodeIndex, index);
						break;
					}
					// there is already a suffix which starts from the same char
					// find the match length
					var nextNode = GetNode(children[nextNodeIndex]);
					var nextBegin = nextNode.Begin;
					var nextEnd = nextNode.End;
					var diffIndex = nextBegin + 1;
					++begin;
					while (diffIndex < nextEnd
						&& begin < end
						&& data[diffIndex] == data[begin])
					{
						++diffIndex;
						++begin;
					}
					if (diffIndex != nextEnd)
					{
						// split the nextNode
						var splitNode = new Node(diffIndex, nextEnd, nextNode.IsTerminal) { Children = nextNode.Children };
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
