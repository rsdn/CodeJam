using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>Naive implementation of the suffix tree</summary>
	/// see http://felix-halim.net/pg/suffix-tree/ for sample builder
	[DebuggerDisplay("{Print()}")]
	public class SuffixTreeNaive : SuffixTreeBase
	{
		protected override void BuildFor([NonNegativeValue] int begin, [NonNegativeValue] int end)
		{
			for (var i = end - 1; i >= begin; --i)
			{
				var currentNodeIndex = RootNodeIndex;
				var currentNode = Root;
				var intBegin = i;
				for (;;)
				{
					var children = currentNode.Children;
					int childIndex;
					var forceNew = false;
					if (children == null)
					{
						var currentNodeEnd = currentNode.End;
						children = new List<int>();
						var updatedNode = new Node(currentNode.Begin, currentNodeEnd, false, children);
						if (currentNode.IsTerminal)
						{
							// add new empty terminal node
							var newTerminal = new Node(currentNodeEnd, currentNodeEnd, true);
							var index = AddNode(newTerminal);
							children.Add(index);
						}
						UpdateNode(currentNodeIndex, updatedNode);
						// currentNode = updatedNode is not needed since we will not use currentNode value anymore
						childIndex = children.Count;
						forceNew = true;
					}
					else
					{
						if (intBegin == end)
						{
							childIndex = 0;
							forceNew = true;
						}
						else
						{
							childIndex = children.LowerBound(InternalData[intBegin], EdgeComparer);
						}
					}

					if (forceNew
						|| childIndex == children.Count
						|| InternalData[GetNode(children[childIndex]).Begin] != InternalData[intBegin])
					{
						// add new child
						var node = new Node(intBegin, end, true);
						var index = AddNode(node);
						children.Insert(childIndex, index);
						break;
					}
					// there is already a suffix which starts from the same char
					// find the match length
					var childNodeIndex = children[childIndex];
					var childNode = GetNode(childNodeIndex);
					var childBegin = childNode.Begin;
					var childEnd = childNode.End;
					var diffIndex = childBegin + 1;
					++intBegin;
					while (diffIndex < childEnd
						&& intBegin < end
						&& InternalData[diffIndex] == InternalData[intBegin])
					{
						++diffIndex;
						++intBegin;
					}
					if (diffIndex != childEnd)
					{
						// split the nextNode
						var splitNode = new Node(diffIndex, childEnd, childNode.IsTerminal, childNode.Children);
						var splitIndex = AddNode(splitNode);
						var updatedChildNode = new Node(childBegin, diffIndex, false, new List<int> { splitIndex });
						UpdateNode(childNodeIndex, updatedChildNode);
						currentNode = updatedChildNode;
					}
					else
					{
						currentNode = childNode;
					}
					currentNodeIndex = childNodeIndex;
					// continue search from the child node
				}
			}
		}
	}
}
