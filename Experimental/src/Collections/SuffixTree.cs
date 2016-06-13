using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeJam.Collections
{
	/// <summary>
	/// Implementation of the suffix tree with Ukkonen's algorithm
	/// <remarks>
	/// See http://stackoverflow.com/questions/9452701/ukkonens-suffix-tree-algorithm-in-plain-english/9513423
	/// and http://www.cise.ufl.edu/~sahni/dsaaj/enrich/c16/suffix.htm
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
		/// <summary>Offset of the first suffix to insert</summary>
		private int nextSuffixOffset_;
		/// <summary>Current working offset in the string</summary>
		private int currentOffset_;
		/// <summary>Index of the previous insertion node that should be linked with a subsequent insertion node</summary>
		private int pendingLinkIndexFrom_;
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
			new Lazy<List<int>>(() => Enumerable.Repeat(InvalidNodeIndex, NodesCount).ToList(), false);

		/// <summary>Shows whether we have a pending link insertion</summary>
		private bool LinkPending => pendingLinkIndexFrom_ != InvalidNodeIndex;

		protected override void BuildFor(int begin, int end)
		{
			Code.AssertState(begin < end, "Invalid parameters passed");
			childComparer_ = GetComparer();
			branchNodeIndex_ = RootNodeIndex;
			activeEdgeIndex_ = InvalidNodeIndex;
			activeLength_ = 0;
			nextSuffixOffset_ = begin;
			pendingLinkIndexFrom_ = InvalidNodeIndex;
			currentOffset_ = begin;
			end_ = end;
			for (;;)
			{
				// save current branching position
				// to be able to make a link to an existing node if needed
				var savedOffset = currentOffset_;
				var saveBranchNodeIndex = branchNodeIndex_;
				FindBranchingPoint();
				if (currentOffset_ != savedOffset && LinkPending)
				{
					// we have omitted an implicit insertion
					// however, we still need to make a link
					CreatePendingLink(saveBranchNodeIndex);
				}
				// here we have a branching point
				// and we need either to add a new branch to an existing node
				// or to create a new internal node
				InsertSuffix();
				++nextSuffixOffset_;
				if (nextSuffixOffset_ == end)
				{
					break;
				}
				UpdateActiveEdgeAndCurentPosition();
			}
			childComparer_ = null;
		}

		/// <summary>Finds the next branching point</summary>
		private void FindBranchingPoint()
		{
			var branchNode = GetNode(branchNodeIndex_);
			var children = branchNode.Children;
			int childNodeIndex;
			Node activeEdge;
			if (activeEdgeIndex_ != InvalidNodeIndex)
			{
				childNodeIndex = children[activeEdgeIndex_];
				activeEdge = GetNode(childNodeIndex);
			}
			else
			{
				childNodeIndex = InvalidNodeIndex;
				activeEdge = null;
			}
			for(;;)
			{
				if (activeEdge == null)
				{					
					DebugCode.AssertState(activeLength_ == 0, "Invalid active state");
					if (currentOffset_ == end_)
					{
						return;
					}
					if (branchNode.IsLeaf)
					{
						// a new branch
						return;
					}
					var c = InternalData[currentOffset_];
					var childIndex = children.LowerBound(c, childComparer_);
					if (childIndex == children.Count)
					{
						// a new branch
						return;
					}
					childNodeIndex = children[childIndex];
					var edgeNode = GetNode(childNodeIndex);
					if (InternalData[edgeNode.Begin] != c)
					{
						// a new branch
						return;
					}
					activeLength_ = 1;
					activeEdgeIndex_ = childIndex;
					activeEdge = edgeNode;
					++currentOffset_;
				}
				var edgeOffset = activeEdge.Begin + activeLength_;
				var edgeEnd = activeEdge.End;
				for (;;)
				{
					if (edgeOffset == edgeEnd)
					{
						// end of the current edge reached
						branchNodeIndex_ = childNodeIndex;
						branchNode = GetNode(branchNodeIndex_);
						children = branchNode.Children;
						activeEdgeIndex_ = InvalidNodeIndex;
						activeEdge = null;
						activeLength_ = 0;
						break;
					}
					if (currentOffset_ == end_)
					{
						return;
					}
					if (InternalData[edgeOffset] != InternalData[currentOffset_])
					{
						return;
					}
					++activeLength_;
					++currentOffset_;
					++edgeOffset;
				}
			}
		}

		/// <summary>Creates a pending link</summary>
		/// <param name="toNodeIndex">The node to link to</param>
		private void CreatePendingLink(int toNodeIndex)
		{
			DebugCode.AssertState(LinkPending, "Pending link should be present");
			nodeLinks_.Value[pendingLinkIndexFrom_] = toNodeIndex;
			pendingLinkIndexFrom_ = InvalidNodeIndex;
		}

		/// <summary>Updates active edge and current position</summary>
		private void UpdateActiveEdgeAndCurentPosition()
		{
			if (nextSuffixOffset_ > currentOffset_)
			{
				// all pending proper subsuffixes have been processed
				// start from the root
				currentOffset_ = nextSuffixOffset_;
				branchNodeIndex_ = RootNodeIndex;
				activeEdgeIndex_ = InvalidNodeIndex;
				activeLength_ = 0;
				pendingLinkIndexFrom_ = InvalidNodeIndex;
				return;
			}
			// try to follow the link if it is present
			branchNodeIndex_ = nodeLinks_.Value[branchNodeIndex_];
			if (branchNodeIndex_ == InvalidNodeIndex)
			{
				activeLength_ = currentOffset_ - nextSuffixOffset_;
				branchNodeIndex_ = RootNodeIndex;
			}
			activeEdgeIndex_ = InvalidNodeIndex;
			if (activeLength_ == 0)
			{
				// we are already at a correct node
				return;
			}
			// go down over the tree
			var branchNode = GetNode(branchNodeIndex_);
			for (;;)
			{
				DebugCode.AssertState(!branchNode.IsLeaf, "Invalid active state");
				var index = currentOffset_ - activeLength_;
				var children = branchNode.Children;
				var childIndex = children.LowerBound(InternalData[index], childComparer_);
				DebugCode.AssertState(childIndex != children.Count, "Invalid active state");
				var edgeIndex = children[childIndex];
				var edgeNode = GetNode(edgeIndex);
				DebugCode.AssertState(InternalData[edgeNode.Begin] == InternalData[index], "Invalid active state");
				var edgeLength = edgeNode.Length;
				if (edgeLength <= activeLength_)
				{
					activeLength_ -= edgeLength;
					branchNodeIndex_ = edgeIndex;
					if (activeLength_ == 0)
					{
						return;
					}
					branchNode = edgeNode;
				}
				else
				{
					activeEdgeIndex_ = childIndex;
					return;
				}
			}
		}

		/// <summary>Inserts a new suffix at the current position</summary>
		private void InsertSuffix()
		{
			Node insertionNode;
			int insertionNodeIndex;
			var branchNode = GetNode(branchNodeIndex_);
			if (activeEdgeIndex_ != InvalidNodeIndex)
			{
				var branchChildren = branchNode.Children;
				var edgeNodeIndex = branchChildren[activeEdgeIndex_];
				// need to create a new internal node
				var edgeNode = GetNode(edgeNodeIndex);
				DebugCode.AssertState(activeLength_ < edgeNode.Length, "Invalid active state");
				var newEdgeNode = new Node(edgeNode.Begin, edgeNode.Begin + activeLength_, false)
				{
					Children = new List<int> { edgeNodeIndex }
				};
				var newEdgeNodeIndex = AddNode(newEdgeNode);
				edgeNode.Begin = newEdgeNode.End;
				branchChildren[activeEdgeIndex_] = newEdgeNodeIndex;
				insertionNode = newEdgeNode;
				insertionNodeIndex = newEdgeNodeIndex;
			}
			else
			{
				DebugCode.AssertState(activeLength_ == 0, "Invalid active state");
				insertionNode = branchNode;
				insertionNodeIndex = branchNodeIndex_;
			}
			// insert a new child edge
			var children = insertionNode.Children;
			int childNodeIndex;
			if (children == null)
			{
				children = new List<int>();
				insertionNode.Children = children;
				if (insertionNode.IsTerminal)
				{
					// Do a split. New children: an empty terminal node and a new suffix node (will be added later)
					var terminalEnd = insertionNode.End;
					var newTerminal = new Node(terminalEnd, terminalEnd, true);
					var newTerminalIndex = AddNode(newTerminal);
					children.Add(newTerminalIndex);
					insertionNode.MakeNonTerminal();
				}
				childNodeIndex = children.Count;
			}
			else
			{
				childNodeIndex = currentOffset_ == end_
					? 0 // empty nodes always at the beginning
					: children.LowerBound(InternalData[currentOffset_], childComparer_);
			}
			// now we have a non-empty children and an insertion index
			// just do an insert
			var newNode = new Node(currentOffset_, end_, true);
			var newIndex = AddNode(newNode);
			insertionNode.Children.Insert(childNodeIndex, newIndex);
			// create a link if needed
			if (LinkPending)
			{
				CreatePendingLink(insertionNodeIndex);
			}
			// and mask a branching node as link pending if it is not the root
			if (insertionNodeIndex != RootNodeIndex)
			{
				pendingLinkIndexFrom_ = insertionNodeIndex;
			}
		}

		/// <summary>Adds a new node with an empty link</summary>
		/// <param name="node">A node to add</param>
		/// <returns>Index of the node</returns>
		private new int AddNode(Node node)
		{
			var result = base.AddNode(node);
			nodeLinks_.Value.Add(InvalidNodeIndex);
			return result;
		}

		protected override void PrintNodeText(StringBuilder sb, int nodeIndex)
		{
			var n = GetNode(nodeIndex);
			var nodeLink = nodeLinks_.IsValueCreated ? nodeLinks_.Value[nodeIndex] : InvalidNodeIndex;
			var linkText = nodeLink != InvalidNodeIndex ? $" -> {nodeLink}" : string.Empty; 
			sb.AppendLine($"({nodeIndex}{linkText}, [{n.Begin}-{n.End}), {InternalData.Substring(n.Begin, n.End - n.Begin)})");
		}
	}
}
