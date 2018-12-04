using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Base class for suffix tree algorithm implementation.
	/// </summary>
	[PublicAPI]
	public abstract class SuffixTreeBase
	{
		/// <summary>Node alignment in Print output</summary>
		private const int _align = 6;

		/// <summary>Root node index</summary>
		protected const int RootNodeIndex = 0;

		/// <summary>Tree nodes</summary>
		[NotNull]
		private readonly List<Node> _nodes;

		/// <summary>The root node</summary>
		protected Node Root => _nodes[RootNodeIndex];
		/// <summary>The comparer to compare edges of a node against a char</summary>
		protected Func<int, char, int> EdgeComparer { get; }

		/// <summary>The comparer to compare string locations against a string end</summary>
		private readonly Func<(int Start, int End), int, int> _stringLocationByEndComparer =
			(position, end) => position.Item2 - end;

		/// <summary>Adds a new node</summary>
		/// <param name="node">A node to add</param>
		/// <returns>Index of the node</returns>
		protected int AddNode(Node node)
		{
			var index = _nodes.Count;
			_nodes.Add(node);
			return index;
		}

		/// <summary>Updates the node at the index</summary>
		/// <param name="index">The index to update</param>
		/// <param name="node">The new node value</param>
		protected void UpdateNode(int index, Node node) => _nodes[index] = node;

		/// <summary>Gets a node at the index</summary>
		/// <param name="index">The index of the node</param>
		/// <returns>The node</returns>
		protected Node GetNode(int index) => _nodes[index];

		/// <summary>Number of nodes</summary>
		protected int NodesCount => _nodes.Count;

		/// <summary>Concatenated input strings</summary>
		[NotNull]
		protected string InternalData { get; private set; }

		/// <summary>List of locations of added strings inside the InternalData</summary>
		[NotNull]
		protected List<(int Start, int Length)> StringLocations { get; }

		/// <summary>Constructs a base for a suffix tree</summary>
		protected SuffixTreeBase()
		{
			InternalData = string.Empty;
			var root = new Node(0, 0, false);
			_nodes = new List<Node> { root };
			StringLocations = new List<(int, int)>();
			EdgeComparer = (index, c) =>
			{
				var node = GetNode(index);
				if (node.Begin == node.End) // no char always less than any char
				{
					return -1;
				}
				var firstChar = InternalData[node.Begin];
				return firstChar - c;
			};
		}

		/// <summary>Adds a new string to the tree</summary>
		/// <param name="data">
		/// The string to add
		/// <remarks>The last string character should be unique among all added strings</remarks>
		/// </param>
		public void Add([NotNull] string data)
		{
			Code.NotNull(data, nameof(data));
			if (data.Length == 0)
			{
				return;
			}
			var begin = InternalData.Length;
			InternalData = InternalData + data;
			StringLocations.Add((begin, InternalData.Length));
			BuildFor(begin, InternalData.Length);
		}

		/// <summary>Enumerates all suffixes in the suffix tree</summary>
		/// <remarks>
		/// May return suffixes with the same value of the they are present in different source strings
		/// </remarks>
		/// <returns>The enumeration of all suffixes in lexicographical order</returns>
		[Pure]
		public IEnumerable<Suffix> All() => AllFromNode(Root, 0);

		/// <summary>Checks whether the suffix tree contains the given substring or not</summary>
		/// <param name="substring">The substring to locate</param>
		/// <returns>true if found, false otherwise</returns>
		[Pure]
		public bool Contains([NotNull] string substring)
		{
			Code.NotNull(substring, nameof(substring));
			if (substring == string.Empty)
			{
				return true;
			}
			var r = FindBranch(substring);
			return r != null;
		}

		/// <summary>Checks whether the suffix tree contains the given suffix or not</summary>
		/// <param name="suffix">The suffix to locate</param>
		/// <returns>true if found, false otherwise</returns>
		[Pure]
		public bool ContainsSuffix([NotNull] string suffix)
		{
			Code.NotNull(suffix, nameof(suffix));
			if (suffix == string.Empty)
			{
				return true;
			}
			var r = FindBranch(suffix);
			if (r == null)
			{
				return false;
			}
			var edge = r.Item1;
			var length = r.Item2;
			if (length < edge.Length) // proper substring of a suffix?
			{
				return false;
			}
			if (edge.IsLeaf) // a terminal edge?
			{
				return true;
			}
			return GetNode(edge.Children[0]).Length == 0; // has a child terminal edge of zero length
		}

		/// <summary>Enumerates all suffixes starting with the given prefix</summary>
		/// <param name="prefix">The prefix to find</param>
		/// <returns>The enumeration of all suffixes with the given prefix in lexicographical order</returns>
		[Pure]
		public IEnumerable<Suffix> StartingWith([NotNull] string prefix)
		{
			Code.NotNull(prefix, nameof(prefix));
			if (prefix == string.Empty)
			{
				return All();
			}
			var first = FindBranch(prefix);
			if (first == null)
			{
				return Enumerable.Empty<Suffix>();
			}
			var length = prefix.Length;
			var edge = first.Item1;
			var matchLength = first.Item2;
			if (matchLength < edge.Length)
			{
				length += edge.Length - matchLength;
			}
			return AllFromNode(edge, length);
		}

		/// <summary>Enumerates all suffixes in the subtree of the given node</summary>
		/// <remarks>
		/// May return suffixes with the same value of the they are present in different source strings
		/// </remarks>
		/// <returns>The enumeration of all suffixes in the subtree in lexicographical order</returns>
		[Pure]
		private IEnumerable<Suffix> AllFromNode(Node node, int length)
		{
			DebugCode.AssertArgument(length >= 0, nameof(length), "The length should be non-negative");
			if (node.IsLeaf) // Empty subtree
			{
				if (length != 0)
				{
					yield return CreateSuffix(node.End, length);
				}
				yield break;
			}

			var branchStack = new Stack<BranchPoint>();
			var branchPoint = new BranchPoint { Node = node, EdgeIndex = 0 };
			for (;;)
			{
				var edge = GetNode(branchPoint.Node.Children[branchPoint.EdgeIndex]);
				var edgeLength = edge.Length;
				length += edgeLength;
				if (!edge.IsTerminal)
				{
					branchPoint.Length = edgeLength;
					branchStack.Push(branchPoint);
					branchPoint = new BranchPoint { Node = edge, EdgeIndex = 0 };
					continue;
				}

				// We have descended to a terminal edge. Let's produce a suffix
				yield return CreateSuffix(edge.End, length);

				// Move to the next suffix branch
				for (;;)
				{
					length -= edgeLength;
					var nextEdgeIndex = branchPoint.EdgeIndex + 1;
					if (nextEdgeIndex < branchPoint.Node.Children.Count)
					{
						branchPoint.EdgeIndex = nextEdgeIndex;
						break;
					}
					// There is no more branches on the current level
					// Return to the previous level
					if (branchStack.Count == 0)
					{
						// no more branches to visit
						yield break;
					}
					branchPoint = branchStack.Pop();
					edgeLength = branchPoint.Length;
				}
			}
		}

		/// <summary>Creates a new suffix description</summary>
		/// <param name="end">The suffix end</param>
		/// <param name="length">The suffix length</param>
		/// <returns>The suffix</returns>
		[Pure]
		private Suffix CreateSuffix(int end, int length)
		{
			var sourceIndex = GetSourceIndexByEnd(end);
			var sourceOffset = StringLocations[sourceIndex].Start;
			var offset = end - length - sourceOffset;
			return new Suffix(InternalData, sourceIndex, sourceOffset, offset, length);
		}

		/// <summary>Locates the branch corresponding to the given string</summary>
		/// <param name="s">The string to find</param>
		/// <returns>The last matched edge and the matched length over this edge or null if no match found</returns>
		[Pure]
		private Tuple<Node, int> FindBranch([NotNull] string s)
		{
			DebugCode.AssertState(s.Length > 0, "The string length should be positive");
			var currentNode = Root;
			var offset = 0;
			for (;;)
			{
				var edgeIndex = FindEdge(currentNode, s[offset], out var edge);
				if (edgeIndex == -1)
				{
					return null;
				}
				var edgeLength = edge.Length;
				var compareLength = Math.Min(s.Length - offset, edgeLength);
				if (compareLength > 1
					&& string.Compare(s, offset + 1, InternalData, edge.Begin + 1, compareLength - 1) != 0)
				{
					return null;
				}
				offset += compareLength;
				if (offset == s.Length)
				{
					return Tuple.Create(edge, compareLength);
				}
				DebugCode.AssertState(compareLength == edgeLength, "Invalid compare length. Check logic");
				currentNode = edge;
				// continue search from the next level
			}
		}

		/// <summary>Finds an edge from the given node corresponding to the given char</summary>
		/// <param name="node">The node to search in</param>
		/// <param name="c">The char to find</param>
		/// <param name="edge">Te edge found</param>
		/// <returns>The index of the edge or -1 if there is no edge starting with the given char</returns>
		[Pure]
		private int FindEdge(Node node, char c, out Node edge)
		{
			edge = default;
			if (node.IsLeaf)
			{
				return -1;
			}
			var edgeIndex = node.Children.LowerBound(c, EdgeComparer);
			if (edgeIndex == node.Children.Count)
			{
				return -1;
			}
			edge = GetNode(node.Children[edgeIndex]);
			return edge.Length > 0 && InternalData[edge.Begin] == c ? edgeIndex : -1;
		}

		/// <summary>Locates the source string index by the suffix end</summary>
		/// <param name="end">The suffix end</param>
		/// <returns>The source string index</returns>
		private int GetSourceIndexByEnd(int end)
		{
			// CSC bug?
			// ReSharper disable once RedundantTypeArgumentsOfMethod
			var index = StringLocations.LowerBound<(int, int), int>(end, _stringLocationByEndComparer);
			DebugCode.AssertState(
				index < StringLocations.Count && StringLocations[index].Length == end,
				"Invalid source index computed. Check logic");
			return index;
		}

		/// <summary>Appends suffixes for the last added string</summary>
		protected abstract void BuildFor(int begin, int end);

		/// <summary>Prints the tree structure to the string for the debugging purposes</summary>
		/// <returns>The tree structure as a string</returns>
		[Pure]
		public string Print()
		{
			var sb = new StringBuilder();
			var currentIndex = RootNodeIndex;
#if LESSTHAN_NET45
			var stack = new ListWithReadOnly<(int Start, int Length)>();
#else
			var stack = new List<(int Start, int Length)>();
#endif
			for (;;)
			{
				PrintNodeWithPath(sb, currentIndex, stack);
				var node = GetNode(currentIndex);
				if (node.Children != null)
				{
					stack.Add((currentIndex, node.Children.Count - 2));
					currentIndex = node.Children[node.Children.Count - 1];
					continue;
				}
				currentIndex = -1;
				while (stack.Count > 0)
				{
					var t = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
					node = GetNode(t.Start);
					var nextChild = t.Length;
					if (nextChild >= 0)
					{
						currentIndex = node.Children[nextChild];
						stack.Add((t.Item1, nextChild - 1));
						break;
					}
				}
				if (currentIndex == -1)
				{
					break;
				}
			}
			return sb.ToString();
		}

		/// <summary>Prints a single node representation along with the path prefix</summary>
		/// <param name="sb">The builder to print to</param>
		/// <param name="nodeIndex">THe index of the node</param>
		/// <param name="stack">The stack of nodes to process</param>
		private void PrintNodeWithPath(
			[NotNull] StringBuilder sb,
			int nodeIndex,
			[NotNull] IReadOnlyList<(int Start, int Length)> stack)
		{
			if (stack.Count > 0)
			{
				for (var i = 0; i < stack.Count - 1; ++i)
				{
					sb.Append(stack[i].Length >= 0 ? '|' : ' ');
					sb.Append(' ', _align - 1);
				}
				sb.AppendLine("|");
				for (var i = 0; i < stack.Count - 1; ++i)
				{
					sb.Append(stack[i].Length >= 0 ? '|' : ' ');
					sb.Append(' ', _align - 1);
				}
				sb.Append('|');
				sb.Append('_', _align - 1);
			}
			AppendNodeText(sb, nodeIndex);
		}

		/// <summary>Prints a single node information</summary>
		/// <param name="sb">The builder to print to</param>
		/// <param name="nodeIndex">The node index</param>
		protected virtual void AppendNodeText([NotNull] StringBuilder sb, int nodeIndex)
		{
			var n = GetNode(nodeIndex);
			sb.AppendLine($"({nodeIndex}, [{n.Begin}-{n.End}), {InternalData.Substring(n.Begin, n.Length)})");
		}

		/// <summary>A suffix tree edge combined with the end node</summary>
		protected struct Node
		{
			private readonly int _end;

			/// <summary>Constructs a new node</summary>
			/// <param name="begin">An edge start offset</param>
			/// <param name="end">An edge end offset</param>
			/// <param name="terminal">Is the edge terminates the string or not</param>
			/// <param name="children">A list of child nodes (edges)</param>
			public Node(int begin, int end, bool terminal, List<int> children = null)
			{
				DebugCode.AssertArgument(end >= 0, nameof(end), "end should be nonnegative");
				Begin = begin;
				_end = terminal ? -end : end;
				Children = children;
			}

			/// <summary>
			/// A list of child nodes
			/// <remarks>null for leaf nodes</remarks>
			/// </summary>
			[CanBeNull]
			public List<int> Children { get; }
			/// <summary>Shows whether it is a leaf or an internal node</summary>
			public bool IsLeaf => Children == null;
			/// <summary>Shows whether it is a terminal (ending at a string end) node or not</summary>
			public bool IsTerminal => _end < 0;
			/// <summary>Index of the first character of a substring corresponding to the node</summary>
			public int Begin { get; }
			/// <summary>Index after the last character of a substring corresponding to the node</summary>
			public int End => Math.Abs(_end);
			/// <summary>Length of the corresponding substring</summary>
			public int Length => End - Begin;
		}

		/// <summary>Branching point</summary>
		private class BranchPoint
		{
			/// <summary>The tree node</summary>
			public Node Node;

			/// <summary>The chosen edge</summary>
			public int EdgeIndex;

			/// <summary>The length over the edge</summary>
			public int Length;
		}
	}
}