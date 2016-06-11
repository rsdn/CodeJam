using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeJam.Collections
{
	/// <summary>
	/// Implementation of the suffix tree with Ukkonen's algorithm
	/// <remarks>
	/// See http://stackoverflow.com/questions/9452701/ukkonens-suffix-tree-algorithm-in-plain-english/9513423
	/// </remarks>
	/// </summary>
	[DebuggerDisplay("{Print()}")]
	public class SuffixTree : SuffixTreeBase
	{
		/// <summary>Unassigned node index</summary>
		protected const int InvalidNodeIndex = -1;

		/// <summary>Links between nodes</summary>
		private Lazy<List<int>> nodeLinks_;
		/// <summary>Comparer for nodes against a char</summary>
		private Func<int, char, int> childComparer_;
		// state: (activeNode_, activeChild_, activeLength_), pending_
		/// <summary>Index of the branch node</summary>
		private int branchNodeIndex_;
		/// <summary>Index of the active edge (child node) of the branch node</summary>
		private int activeEdgeIndex_ ;
		/// <summary>The length of the current part of the active child</summary>
		private int activeLength_;
		/// <summary>
		/// Offset of the first suffix to insert.
		/// </summary>
		private int pendingPosition_;
		/// <summary>Index of the previous internal node that got a new child in the current iteration</summary>
		private int previousInsertNodeIndex_;
		/// <summary>The end of the string</summary>
		private int end_;

		public SuffixTree()
		{
			ResetLinks();
		}

		/// <summary>Releases internal structures used only for tree building to free some memory</summary>
		/// <remarks>
		/// Calling this method may result in a worse building algorithm complexity for subsequent <see cref="SuffixTreeBase.Add"/> calls.
		/// So, it is not recommended to call it unless no more string are going to be added to the tree.
		/// </remarks>
		public void Compact() => ResetLinks();

		/// <summary>Resets node links lazy list to a default value</summary>
		private void ResetLinks() => nodeLinks_ =
			new Lazy<List<int>>(() => Enumerable.Repeat(RootNodeIndex, NodesCount).ToList(), false);

		protected override void BuildFor(int begin, int end)
		{
			childComparer_ = GetComparer();
			branchNodeIndex_ = RootNodeIndex;
			activeEdgeIndex_ = InvalidNodeIndex;
			activeLength_ = 0;
			pendingPosition_ = begin;
			previousInsertNodeIndex_ = InvalidNodeIndex;
			var currentIndex = begin;
			end_ = end;
			while (pendingPosition_ < end)
			{
				if (pendingPosition_ == currentIndex)
				{
					// there is no pending subsuffixes to insert
					// so, try to insert a new suffix directly at the root
					DebugCode.AssertState(branchNodeIndex_ == RootNodeIndex
						&& activeEdgeIndex_ == InvalidNodeIndex && activeLength_ == 0
						, "Invalid active state");
					TryAddSuffix(currentIndex);
					++currentIndex;
					continue;
				}
				if (activeEdgeIndex_ == InvalidNodeIndex)
				{
					DebugCode.AssertState(activeLength_ == 0, "Invalid active state");
					if (TryAddSuffix(currentIndex))
					{
						// added a new suffix to the current branch
						// now we need to add all pending proper subsuffixes
						branchNodeIndex_ = nodeLinks_.Value[branchNodeIndex_];
						if (branchNodeIndex_ == RootNodeIndex)
						{
							activeLength_ = currentIndex - pendingPosition_;
						}
						UpdateActiveEdge();
					}
					else
					{
						++currentIndex;
					}
					continue;
				}
				//TODO: we have an active edge. Try to move one step farther on it
				//todo:
				/*				var currentNode = root;
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
				*/
			}
			childComparer_ = null;
		}

		/// <summary>Updates active edge index</summary>
		private void UpdateActiveEdge()
		{
			activeEdgeIndex_ = InvalidNodeIndex;
			if (activeLength_ == 0)
			{
				return;
			}
			var index = pendingPosition_;
			var branchNode = GetNode(branchNodeIndex_);
			for (;;)
			{
				DebugCode.AssertState(!branchNode.IsLeaf, "Invalid active state");
				var edgeIndex = branchNode.Children.LowerBound(InternalData[index], childComparer_);
				var edgeNode = GetNode(edgeIndex);
				DebugCode.AssertState(InternalData[edgeNode.Begin] == InternalData[index], "Invalid tree state");
				var edgeLength = edgeNode.Length;
				if (edgeLength <= activeLength_)
				{
					activeLength_ -= edgeLength;
					branchNodeIndex_ = edgeIndex;
					branchNode = edgeNode;
				}
				else
				{
					activeEdgeIndex_ = edgeIndex;
					return;
				}
			}
		}

		/// <summary>Tries to add a new suffix to the tree</summary>
		/// <param name="index">Index of a first suffix char</param>
		/// <returns>true if a new node has been added, false otherwise</returns>
		private bool TryAddSuffix(int index)
		{
			var node = GetNode(branchNodeIndex_);
			var children = node.Children;
			int childNodeIndex;
			var forceNew = false;
			if (children == null)
			{
				children = new List<int>();
				node.Children = children;
				if (node.IsTerminal)
				{
					// Do a split. New children: an empty terminal node and a new suffix node (will be added later)
					var terminalEnd = node.End;
					var newTerminal = new Node(terminalEnd, terminalEnd, true);
					AddNode(newTerminal);
					children.Add(index);
					node.MakeNonTerminal();
				}
				// force new child addition
				childNodeIndex = children.Count;
				forceNew = true;
			}
			else
			{
				if (index == end_)
				{
					forceNew = true; // force empty terminal node insertion
					childNodeIndex = 0;
				}
				else
				{
					childNodeIndex = children.LowerBound(InternalData[index], childComparer_);
				}
			}
			if (!forceNew && childNodeIndex != children.Count)
			{
				var childNode = GetNode(children[childNodeIndex]);
				var childBegin = childNode.Begin;
				if (InternalData[childBegin] == InternalData[index])
				{
					if (branchNodeIndex_ != RootNodeIndex && previousInsertNodeIndex_ > RootNodeIndex)
					{
						nodeLinks_.Value[previousInsertNodeIndex_] = branchNodeIndex_;
						previousInsertNodeIndex_ = InvalidNodeIndex;
					}
					if (childNode.Length == 1)
					{
						branchNodeIndex_ = childNodeIndex;
						activeEdgeIndex_ = InvalidNodeIndex;
						activeLength_ = 0;
					}
					else
					{
						//branchNodeIndex remains the same
						activeEdgeIndex_ = childNodeIndex;
						activeLength_ = 1;
					}
					return false;
				}
			}
			// add new child
			var newNode = new Node(index, end_, true);
			var newIndex = AddNode(newNode);
			node.Children.Insert(childNodeIndex, newIndex);
			// create a link if it is not the root and it is not the 1st insert in the current iteration
			if (branchNodeIndex_ == RootNodeIndex)
			{
				previousInsertNodeIndex_ = InvalidNodeIndex;
			}
			else if (previousInsertNodeIndex_ > RootNodeIndex) 
			{
				nodeLinks_.Value[previousInsertNodeIndex_] = branchNodeIndex_;
				previousInsertNodeIndex_ = branchNodeIndex_;
			}
			++pendingPosition_;
			return true;
		}

		/// <summary>Adds a new node with an empty link</summary>
		/// <param name="node">A node to add</param>
		/// <returns>Index of the node</returns>
		private new int AddNode(Node node)
		{
			var result = base.AddNode(node);
			nodeLinks_.Value.Add(RootNodeIndex);
			return result;
		}
	}
}
