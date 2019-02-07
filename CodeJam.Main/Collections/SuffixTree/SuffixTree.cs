using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Implementation of the suffix tree with Ukkonen's algorithm
	/// <remarks>
	/// See http://stackoverflow.com/questions/9452701/ukkonens-suffix-tree-algorithm-in-plain-english/9513423
	/// and http://www.cise.ufl.edu/~sahni/dsaaj/enrich/c16/suffix.htm
	/// </remarks>
	/// </summary>
	[DebuggerDisplay("{" + nameof(Print) + "()}")]
	[PublicAPI]
	public class SuffixTree : SuffixTreeBase
	{
		/// <summary>Unassigned node index</summary>
		protected const int InvalidNodeIndex = -1;

		/// <summary>Links between nodes</summary>
		private Lazy<List<int>> _nodeLinks;
		// state: (activeNode_, activeChild_, activeLength_), pending_
		/// <summary>Index of the branch node</summary>
		private int _branchNodeIndex;
		/// <summary>Index of the active edge (child node) of the branch node</summary>
		private int _activeEdgeIndex;
		/// <summary>The length of the current part of the active child</summary>
		private int _activeLength;
		/// <summary>Offset of the first suffix to insert</summary>
		private int _nextSuffixOffset;
		/// <summary>Current working offset in the string</summary>
		private int _currentOffset;
		/// <summary>Index of the previous insertion node that should be linked with a subsequent insertion node</summary>
		private int _pendingLinkIndexFrom;
		/// <summary>The end of the string</summary>
		private int _end;

		/// <summary>
		/// Creates instance of <see cref="SuffixTree"/>.
		/// </summary>
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
		private void ResetLinks() =>
			_nodeLinks = new Lazy<List<int>>(() => Enumerable.Repeat(InvalidNodeIndex, NodesCount).ToList(), false);

		/// <summary>Shows whether we have a pending link insertion</summary>
		private bool LinkPending => _pendingLinkIndexFrom != InvalidNodeIndex;

		/// <summary>Appends suffixes for the last added string</summary>
		/// <inheritdoc cref="SuffixTreeBase.BuildFor"/>
		protected override void BuildFor(int begin, int end)
		{
			Code.BugIf(begin >= end, "Invalid parameters passed");
			_branchNodeIndex = RootNodeIndex;
			_activeEdgeIndex = InvalidNodeIndex;
			_activeLength = 0;
			_nextSuffixOffset = begin;
			_pendingLinkIndexFrom = InvalidNodeIndex;
			_currentOffset = begin;
			_end = end;
			for (;;)
			{
				// save current branching position
				// to be able to make a link to an existing node if needed
				var savedOffset = _currentOffset;
				var saveBranchNodeIndex = _branchNodeIndex;
				FindBranchingPoint();
				if (_currentOffset != savedOffset && LinkPending)
				{
					// we have omitted an implicit insertion
					// however, we still need to make a link
					CreatePendingLink(saveBranchNodeIndex);
				}
				// here we have a branching point
				// and we need either to add a new branch to an existing node
				// or to create a new internal node
				InsertSuffix();
				++_nextSuffixOffset;
				if (_nextSuffixOffset == end)
				{
					break;
				}
				UpdateActiveEdgeAndCurrentPosition();
			}
		}

		/// <summary>Finds the next branching point</summary>
		private void FindBranchingPoint()
		{
			var branchNode = GetNode(_branchNodeIndex);
			var children = branchNode.Children;
			int childNodeIndex;
			Node activeEdge;
			if (_activeEdgeIndex != InvalidNodeIndex)
			{
				childNodeIndex = children[_activeEdgeIndex];
				activeEdge = GetNode(childNodeIndex);
			}
			else
			{
				childNodeIndex = InvalidNodeIndex;
				activeEdge = default;
			}
			for (;;)
			{
				if (_activeEdgeIndex == InvalidNodeIndex)
				{
					DebugCode.AssertState(_activeLength == 0, "Invalid active state");
					if (_currentOffset == _end)
					{
						return;
					}
					if (branchNode.IsLeaf)
					{
						// a new branch
						return;
					}
					var c = InternalData[_currentOffset];
					var childIndex = children.LowerBound(c, EdgeComparer);
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
					_activeLength = 1;
					_activeEdgeIndex = childIndex;
					activeEdge = edgeNode;
					++_currentOffset;
				}
				var edgeOffset = activeEdge.Begin + _activeLength;
				var edgeEnd = activeEdge.End;
				for (;;)
				{
					if (edgeOffset == edgeEnd)
					{
						// end of the current edge reached
						_branchNodeIndex = childNodeIndex;
						branchNode = GetNode(_branchNodeIndex);
						children = branchNode.Children;
						_activeEdgeIndex = InvalidNodeIndex;
						activeEdge = default;
						_activeLength = 0;
						break;
					}
					if (_currentOffset == _end)
					{
						return;
					}
					if (InternalData[edgeOffset] != InternalData[_currentOffset])
					{
						return;
					}
					++_activeLength;
					++_currentOffset;
					++edgeOffset;
				}
			}
		}

		/// <summary>Creates a pending link</summary>
		/// <param name="toNodeIndex">The node to link to</param>
		private void CreatePendingLink(int toNodeIndex)
		{
			DebugCode.AssertState(LinkPending, "Pending link should be present");
			_nodeLinks.Value[_pendingLinkIndexFrom] = toNodeIndex;
			_pendingLinkIndexFrom = InvalidNodeIndex;
		}

		/// <summary>Updates active edge and current position</summary>
		private void UpdateActiveEdgeAndCurrentPosition()
		{
			if (_nextSuffixOffset > _currentOffset)
			{
				// all pending proper sub-suffixes have been processed
				// start from the root
				_currentOffset = _nextSuffixOffset;
				_branchNodeIndex = RootNodeIndex;
				_activeEdgeIndex = InvalidNodeIndex;
				_activeLength = 0;
				_pendingLinkIndexFrom = InvalidNodeIndex;
				return;
			}
			// try to follow the link if it is present
			_branchNodeIndex = _nodeLinks.Value[_branchNodeIndex];
			if (_branchNodeIndex == InvalidNodeIndex)
			{
				_activeLength = _currentOffset - _nextSuffixOffset;
				_branchNodeIndex = RootNodeIndex;
			}
			_activeEdgeIndex = InvalidNodeIndex;
			if (_activeLength == 0)
			{
				// we are already at a correct node
				return;
			}
			// go down over the tree
			var branchNode = GetNode(_branchNodeIndex);
			for (;;)
			{
				DebugCode.AssertState(!branchNode.IsLeaf, "Invalid active state");
				var index = _currentOffset - _activeLength;
				var children = branchNode.Children;
				var childIndex = children.LowerBound(InternalData[index], EdgeComparer);
				DebugCode.AssertState(childIndex != children.Count, "Invalid active state");
				var edgeIndex = children[childIndex];
				var edgeNode = GetNode(edgeIndex);
				DebugCode.AssertState(InternalData[edgeNode.Begin] == InternalData[index], "Invalid active state");
				var edgeLength = edgeNode.Length;
				if (edgeLength <= _activeLength)
				{
					_activeLength -= edgeLength;
					_branchNodeIndex = edgeIndex;
					if (_activeLength == 0)
					{
						return;
					}
					branchNode = edgeNode;
				}
				else
				{
					_activeEdgeIndex = childIndex;
					return;
				}
			}
		}

		/// <summary>Inserts a new suffix at the current position</summary>
		private void InsertSuffix()
		{
			Node insertionNode;
			int insertionNodeIndex;
			var branchNode = GetNode(_branchNodeIndex);
			if (_activeEdgeIndex != InvalidNodeIndex)
			{
				var branchChildren = branchNode.Children;
				var edgeNodeIndex = branchChildren[_activeEdgeIndex];
				// need to create a new internal node
				var edgeNode = GetNode(edgeNodeIndex);
				DebugCode.AssertState(_activeLength < edgeNode.Length, "Invalid active state");
				var newEdgeNode = new Node(edgeNode.Begin, edgeNode.Begin + _activeLength, false
					, new List<int> { edgeNodeIndex });
				var newEdgeNodeIndex = AddNode(newEdgeNode);
				var updatedEdgeNode = new Node(newEdgeNode.End, edgeNode.End, edgeNode.IsTerminal
					, edgeNode.Children);
				UpdateNode(edgeNodeIndex, updatedEdgeNode);
				branchChildren[_activeEdgeIndex] = newEdgeNodeIndex;
				insertionNode = newEdgeNode;
				insertionNodeIndex = newEdgeNodeIndex;
			}
			else
			{
				DebugCode.AssertState(_activeLength == 0, "Invalid active state");
				insertionNode = branchNode;
				insertionNodeIndex = _branchNodeIndex;
			}
			// insert a new child edge
			var children = insertionNode.Children;
			int childNodeIndex;
			if (children == null)
			{
				children = new List<int>();
				var insertionNodeEnd = insertionNode.End;
				var updatedNode = new Node(insertionNode.Begin, insertionNodeEnd, false, children);
				if (insertionNode.IsTerminal)
				{
					// Do a split. New children: an empty terminal node and a new suffix node (will be added later)
					var newTerminal = new Node(insertionNodeEnd, insertionNodeEnd, true);
					var newTerminalIndex = AddNode(newTerminal);
					children.Add(newTerminalIndex);
				}
				UpdateNode(insertionNodeIndex, updatedNode);
				// insertionNode = updatedNode not needed since insertionNode value is not used later
				childNodeIndex = children.Count;
			}
			else
			{
				childNodeIndex = _currentOffset == _end
					? 0 // empty nodes always at the beginning
					: children.LowerBound(InternalData[_currentOffset], EdgeComparer);
			}
			// now we have a non-empty children and an insertion index
			// just do an insert
			var newNode = new Node(_currentOffset, _end, true);
			var newIndex = AddNode(newNode);
			children.Insert(childNodeIndex, newIndex);
			// create a link if needed
			if (LinkPending)
			{
				CreatePendingLink(insertionNodeIndex);
			}
			// and mask a branching node as link pending if it is not the root
			if (insertionNodeIndex != RootNodeIndex)
			{
				_pendingLinkIndexFrom = insertionNodeIndex;
			}
		}

		/// <summary>Adds a new node with an empty link</summary>
		/// <param name="node">A node to add</param>
		/// <returns>Index of the node</returns>
		private new int AddNode(Node node)
		{
			var result = base.AddNode(node);
			_nodeLinks.Value.Add(InvalidNodeIndex);
			return result;
		}

		/// <summary>
		/// Appends specified node text.
		/// </summary>
		/// <param name="sb"><see cref="StringBuilder"/> to append node text to</param>
		/// <param name="nodeIndex">Node index.</param>
		protected override void AppendNodeText(StringBuilder sb, int nodeIndex)
		{
			var n = GetNode(nodeIndex);
			var nodeLink = _nodeLinks.IsValueCreated ? _nodeLinks.Value[nodeIndex] : InvalidNodeIndex;
			var linkText = nodeLink != InvalidNodeIndex ? $" -> {nodeLink}" : string.Empty;
			sb.AppendLine($"({nodeIndex}{linkText}, [{n.Begin}-{n.End}), {InternalData.Substring(n.Begin, n.End - n.Begin)})");
		}
	}
}
