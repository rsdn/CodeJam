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
			var root = new Node(0, 0);
			nodes_ = new List<Node> { root };
			EndPositions = new List<int>();
		}

		/// <summary>Adds a new string to the tree</summary>
		/// <param name="data">
		/// The string to add
		/// <remarks>The last string character should be unique among all added strings</remarks>
		/// </param>
		private void Add(string data)
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
		    var firstChar = InternalData[GetNode(index).Begin];
		    return firstChar - c;
	    };

	    /// <summary>Finalizes the tree construction by removal of terminal characters</summary>
		protected void RemoveTerminals(ISet<char> terminals)
		{
		    if (terminals.Count == 0)
		    {
			    return;
		    }
		    if (Root.IsLeaf) // no child nodes
		    {
			    return;
		    }
			var positionsToRemove = new HashSet<int>();
		    foreach (var p in EndPositions)
		    {
			    if (terminals.Contains(InternalData[p - 1]))
			    {
				    positionsToRemove.Add(p);
			    }
		    }
		    if (positionsToRemove.Count == 0)
		    {
			    return;
		    }
			foreach (var n in nodes_)
			{
				if (n.IsLeaf && positionsToRemove.Contains(n.End))
				{
					n.End = n.End - 1;
				}
			}
			// Drop empty nodes from Root children
		    Root.Children.RemoveAll(_ =>
			    {
				    var n = nodes_[_];
				    return n.Begin == n.End;
			    });
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

		/// <summary>Builder for the suffix tree</summary>
	    public class Builder<T> where T: SuffixTreeBase, new()
		{
			private T tree_;
			private readonly HashSet<char> usedChars_ = new HashSet<char>();
			private int nextTerminalCandidate_ = char.MaxValue;
			private readonly HashSet<char> terminals_ = new HashSet<char>();

			public Builder()
			{
				tree_ = new T();
			}

			/// <summary>Builds a suffix tree for a single string</summary>
			/// <param name="data">The string</param>
			/// <returns>The suffix tree</returns>
			public static T Build([NotNull] string data)
			{
				Code.NotNull(data, nameof(data));
				var length = data.Length;
				var lastChar = data[length - 1];
				if (data.IndexOf(lastChar) == length - 1)
				{
					// no terminal needed				
					var tree = new T();
					tree.Add(data);
					return tree;
				}
				const char terminal = char.MaxValue;
				if (data.IndexOf(terminal) == -1)
				{
					var tree = new T();
					tree.Add(data + terminal);
					tree.RemoveTerminals(new HashSet<char> { terminal });
					return tree;
				}
				// fallback to the complex way
				var builder = new Builder<T>();
				builder.Add(data);
				return builder.Complete();
			}

			/// <summary>Builds a suffix tree for a collection of strings</summary>
			/// <param name="data">A collection of strings</param>
			public void Add([NotNull] string data)
			{
				EnsureNotFinalized();
				Code.NotNull(data, nameof(data));
				var length = data.Length;
				if (length == 0)
				{
					return;
				}
				for (var i = 0; i < length; ++i)
				{
					usedChars_.Add(data[i]);
				}
				// find new terminal char
				char candidate;
				for (;;)
				{
					if (nextTerminalCandidate_ < 0)
					{
						throw new ArgumentException("Impossible to find a free terminal character for the given string"
							, nameof(data));
					}
					candidate = (char)nextTerminalCandidate_--;
					if (usedChars_.Add(candidate))
					{
						break;
					}
				}
				terminals_.Add(candidate);
				data += candidate;
				tree_.Add(data);
			}

			/// <summary>Finishes the suffix tree construction and returns the resulting tree</summary>
			/// <returns>The suffix tree</returns>
			public T Complete()
			{
				EnsureNotFinalized();
				if (terminals_.Count > 0)
				{
					tree_.RemoveTerminals(terminals_);
				}
				var t = tree_;
				tree_ = null;
				return t;
			}

			private void EnsureNotFinalized()
			{
				if (tree_ == null)
				{
					throw new InvalidOperationException("The suffix tree construction has been completed already");
				}
			}
		}
	}
}
