using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>Disjoint sets without payload</summary>
	/// <remarks>
	/// See http://en.wikipedia.org/wiki/Disjoint-set_data_structure
	/// </remarks>
	[PublicAPI]
	public sealed class DisjointSets : DisjointSetsBase<BasicNode>
	{
		/// <summary>Creates an empty Disjoint sets</summary>
		public DisjointSets() { }

		/// <summary>Creates a Disjoint sets with the given number of elements</summary>
		/// <param name="count">The initial number of elements</param>
		public DisjointSets(int count)
		{
			Add(count);
		}

		/// <summary>Appends the given number of new elements</summary>
		/// <param name="count">The number of elements to add</param>
		public void Add(int count)
		{
			for (var i = 0; i < count; ++i)
			{
				Nodes.Add(new BasicNode { ParentIndex = -1, Rank = 0 });
			}
			SetsCount += count;
		}
	}
}
