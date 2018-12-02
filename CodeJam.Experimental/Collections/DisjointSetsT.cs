using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Generic implementation of the Disjoint sets
	/// </summary>
	///  <remarks>
	/// See http://en.wikipedia.org/wiki/Disjoint-set_data_structure
	/// </remarks>
	[PublicAPI]
	public sealed class DisjointSets<T> : DisjointSetsBase<DisjointSets<T>.Node>
	{
		/// <summary>Creates an empty Disjoint sets</summary>
		public DisjointSets() { }

		/// <summary>Creates a Disjoint sets with the passed values</summary>
		/// <param name="values">The values to store</param>
		public DisjointSets([NotNull, ItemNotNull] IEnumerable<T> values)
		{
			Code.NotNull(values, nameof(values));

			Add(values);
		}

		/// <summary>Gets an element by its index</summary>
		/// <param name="index">Element's index</param>
		public T this[int index] => Nodes[index].Value;

		/// <summary>Appends a list of values</summary>
		/// <param name="values">The values to append</param>
		public void Add([NotNull, ItemNotNull] IEnumerable<T> values)
		{
			Code.NotNull(values, nameof(values));

			var initialNodesCount = Nodes.Count;
			Nodes.AddRange(values.Select(_ => new Node { Value = _, ParentIndex = -1, Rank = 0 }));
			SetsCount += Nodes.Count - initialNodesCount;
		}

		/// <summary>Appends a single element</summary>
		/// <param name="value">The value to append</param>
		public void Add(T value)
		{
			Nodes.Add(new Node { Value = value, ParentIndex = -1, Rank = 0 });
			++SetsCount;
		}

		/// <summary>A sets node</summary>
		public class Node : BasicNode
		{
			/// <summary>The node data</summary>
			public T Value;
		}
	}
}
