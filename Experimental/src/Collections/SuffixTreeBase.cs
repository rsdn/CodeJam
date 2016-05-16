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

		/// <summary>Tree nodes</summary>
		private readonly List<Node> nodes_;
		/// <summary>The root node</summary>
		protected Node Root => nodes_[0];
		/// <summary>Adds a new node</summary>
		/// <param name="node">A node to add</param>
		/// <returns>Index of the node</returns>
	    protected int AddNode(Node node)
	    {
		    var index = nodes_.Count;
			nodes_.Add(node);
		    return index;
	    }
		/// <summary>Gets a node at the index</summary>
		/// <param name="index">The index of the node</param>
		/// <returns>The node</returns>
	    protected Node GetNode(int index) => nodes_[index];
	    /// <summary>Source string with terminal added (if needed)</summary>
		protected string InternalData { get; }
		/// <summary>Source string</summary>
		protected string Data { get; }
		/// <summary>Constructs a base for a suffix tree</summary>
		/// <param name="data">The string to build the suffix tree for</param>
		/// <param name="terminal">
		/// The terminal character
		/// <remarks>
		/// Used if the last character of the string is present in it more than once.
		/// Should be different from any character in the string
		/// </remarks>
		/// </param>
		protected SuffixTreeBase(string data, char terminal = char.MaxValue)
	    {
			Data = data;
			if (data.Length != 0)
		    {
				var lastChar = data[data.Length - 1];
				if (data.IndexOf(lastChar) != data.Length - 1)
				{
					// last char is not unique, need to add a terminal character
					if (data.IndexOf(terminal) != -1)
					{
						throw new ArgumentException("Terminal character is already present in the string. Please, provide a new termanal character that is not present in the string");
					}
					data += terminal;
				}
			}
		    InternalData = data;
		    var root = new Node(0, 0);
			nodes_ = new List<Node>(InternalData.Length + 1) {root};
	    }

		/// <summary>Constructs the tree</summary>
	    protected void Construct()
	    {
		    Build();
			RemoveTerminal();
	    }

		/// <summary>Implementation of the tree building algorithm</summary>
	    protected abstract void Build();

		/// <summary>A suffix tree node</summary>
	    protected class Node
		{
			public Node(int begin, int end)
			{
				Begin = begin;
				End = end;
			}

			/// <summary>
			/// List of child nodes
			/// <remarks>null for leaf nodes</remarks>
			/// </summary>
			public List<int> Children { get; set; }
			/// <summary>
			/// Shows whether it is a leaf or an internal node
			/// </summary>
			public bool IsLeaf => Children == null;
			/// <summary>Index of the first character of a substring corresponding to the node</summary>
			public int Begin { get; }
			/// <summary>Index after the last character of a substring corresponding to the node</summary>
			public int End { get; set; }
		}

		/// <summary>Removes a terminal character if present</summary>
		private void RemoveTerminal()
		{
			if (Data.Length == InternalData.Length)
			{
				// no terminal was added
				return;
			}
			var end = InternalData.Length;
			foreach (var n in nodes_)
			{
				if (n.End == end)
				{
					n.End = end - 1;
				}
			}
		}

		[Pure]
		public string Print()
	    {
		    var sb = new StringBuilder();
		    var currentIndex = 0;
		    var stack = new List<ValueTuple<int, int>>();
		    for (;;)
		    {
			    PrintNode(sb, currentIndex, stack);
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

	    private void PrintNode([NotNull] StringBuilder sb, int nodeIndex
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
		    var n = GetNode(nodeIndex);
		    sb.AppendLine($"({nodeIndex}, [{n.Begin}-{n.End}), {InternalData.Substring(n.Begin, n.End - n.Begin)})");
	    }
	}
}
