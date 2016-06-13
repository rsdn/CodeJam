using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CodeJam.Collections
{
    public abstract class SuffixTreeBase
    {
		/// <summary>Node alignment in Print output</summary>
		private const int Align = 6;
		/// <summary>Root node index</summary>
	    protected const int RootNodeIndex = 0;

		/// <summary>Tree nodes</summary>
		private readonly List<Node> _nodes;

		/// <summary>The root node</summary>
		protected Node Root => _nodes[RootNodeIndex];

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
		protected string InternalData { get; private set; }

		/// <summary>List of end positions of added strings inside the InternalData</summary>
		protected List<int> EndPositions { get; }

		/// <summary>Constructs a base for a suffix tree</summary>
		protected SuffixTreeBase()
		{
			InternalData = string.Empty;
			var root = new Node(0, 0, false);
			_nodes = new List<Node> { root };
			EndPositions = new List<int>();
		}

		/// <summary>Adds a new string to the tree</summary>
		/// <param name="data">
		/// The string to add
		/// <remarks>The last string character should be unique among all added strings</remarks>
		/// </param>
		[PublicAPI]
		public void Add([NotNull]string data)
		{
			Code.NotNull(data, nameof(data));
			if (data.Length == 0)
			{
				return;
			}
			var begin = InternalData.Length;
			InternalData = InternalData + data;
			EndPositions.Add(InternalData.Length);
			BuildFor(begin, InternalData.Length);
		}

		/// <summary>Appends suffixes for the last added string</summary>
	    protected abstract void BuildFor(int begin, int end);

		/// <summary>Creates a comparer for nodes against a char</summary>
		/// <returns>The comparer</returns>
	    protected Func<int, char, int> GetComparer() => (index, c) =>
	    {
		    var node = GetNode(index);
		    if (node.Begin == node.End) // no char always less than any char
		    {
			    return -1;
		    }
			var firstChar = InternalData[node.Begin];
		    return firstChar - c;
	    };

		/// <summary>Prints the tree structure to the string for the debugging purposes</summary>
		/// <returns>The tree structure as a string</returns>
		[Pure]
		public string Print()
	    {
		    var sb = new StringBuilder();
		    var currentIndex = RootNodeIndex;
		    var stack = new List<ValueTuple<int, int>>();
		    for (;;)
		    {
			    PrintNodeWithPath(sb, currentIndex, stack);
			    var node = GetNode(currentIndex);
			    if (node.Children != null)
			    {
				    stack.Add(ValueTuple.Create(currentIndex, node.Children.Count - 2));
				    currentIndex = node.Children[node.Children.Count - 1];
					continue;
			    }
			    currentIndex = -1;
			    while (stack.Count > 0)
			    {
					var t = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
					node = GetNode(t.Item1);
					var nextChild = t.Item2;
				    if (nextChild >= 0)
				    {
					    currentIndex = node.Children[nextChild];
						stack.Add(ValueTuple.Create(t.Item1, nextChild - 1));
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
	    private void PrintNodeWithPath([NotNull] StringBuilder sb, int nodeIndex
			, [NotNull] IReadOnlyList<ValueTuple<int, int>> stack)
	    {
		    if (stack.Count > 0)
		    {
			    for (var i = 0; i < stack.Count - 1; ++i)
			    {
					sb.Append(stack[i].Item2 >= 0 ? '|' : ' ');
					sb.Append(' ', Align - 1);
				}
				sb.AppendLine("|");
				for (var i = 0; i < stack.Count - 1; ++i)
				{
					sb.Append(stack[i].Item2 >= 0 ? '|' : ' ');
					sb.Append(' ', Align - 1);
				}
				sb.Append('|');
				sb.Append('_', Align - 1);
			}
			PrintNodeText(sb, nodeIndex);
	    }

		/// <summary>Prints a single node information</summary>
		/// <param name="sb">The builder to print to</param>
		/// <param name="nodeIndex">The node index</param>
	    protected virtual void PrintNodeText([NotNull] StringBuilder sb, int nodeIndex)
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
			public Node(int begin, int end, bool terminal) : this(begin, end, terminal, null) {}

			/// <summary>Constructs a new node</summary>
			/// <param name="begin">An edge start offset</param>
			/// <param name="end">An edge end offset</param>
			/// <param name="terminal">Is the edge terminates the string or not</param>
			/// <param name="children">A list of child nodes (edges)</param>
			public Node(int begin, int end, bool terminal, List<int> children)
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
	}
}
