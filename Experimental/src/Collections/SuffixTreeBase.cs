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
		protected string InternalData { get; private set; }

		/// <summary>
		/// List of end positions of added strings inside the InternalData
		/// </summary>
		protected List<int> EndPositions { get; }

		/// <summary>Constructs a base for a suffix tree</summary>
		protected SuffixTreeBase()
		{
			InternalData = string.Empty;
			var root = new Node(0, 0, false);
			nodes_ = new List<Node> { root };
			EndPositions = new List<int>();
		}

		/// <summary>Adds a new string to the tree</summary>
		/// <param name="data">
		/// The string to add
		/// <remarks>The last string character should be unique among all added strings</remarks>
		/// </param>
		public void Add(string data)
		{
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

		/// <summary>A suffix tree node</summary>
		protected class Node
		{
			private int _end;

			public Node(int begin, int end, bool terminal)
			{
				DebugCode.AssertArgument(end >= 0, nameof(end), "end should be nonnegative");
				Begin = begin;
				_end = terminal ? -end : end;
			}

			/// <summary>
			/// List of child nodes
			/// <remarks>null for leaf nodes</remarks>
			/// </summary>
			public List<int> Children { get; set; }
			/// <summary>Shows whether it is a leaf or an internal node</summary>
			public bool IsLeaf => Children == null;
			/// <summary>Shows whether it is a terminal (ending at a string end) node or not</summary>
			public bool IsTerminal => _end < 0;
			/// <summary>Index of the first character of a substring corresponding to the node</summary>
			public int Begin { get; }
			/// <summary>Index after the last character of a substring corresponding to the node</summary>
			public int End
			{
				get { return Math.Abs(_end); }
				set
				{
					DebugCode.AssertArgument(value >= 0, nameof(value), "value should be nonnegative");
					_end = value;
				}
			} 
			/// <summary>Clear terminal flag</summary>
			public void MakeNonTerminal() => _end = End;
		}
	}
}
