using System.Collections.Generic;

namespace CodeJam.Collections
{
	/// <summary>Disjoint sets implementation base</summary>
	///  <remarks>
	/// See http://en.wikipedia.org/wiki/Disjoint-set_data_structure
	/// </remarks>
	public class DisjointSetsBase<T> where T : BasicNode
	{
		/// <summary>All nodes</summary>
		protected readonly List<T> Nodes = new List<T>();

		/// <summary>Creates an empty base</summary>
		protected DisjointSetsBase() { }

		/// <summary>The number of nodes</summary>
		public int Count => Nodes.Count;

		/// <summary>The number of disjoint sets</summary>
		public int SetsCount { get; protected set; }

		/// <summary>Finds a set identifier for the element</summary>
		/// <param name="index">The element index</param>
		/// <returns>The identifier of the containing set</returns>
		/// <remarks>
		/// The set identifier is the index of a single element representing the set.
		/// The Union operation may lead to a choice of a different representative for a set.
		/// In this case IndexToSetId(oldSetId) may be called to get the new set id.
		/// </remarks>
		public int IndexToSetId(int index)
		{
			// First, find a root element of a tree containing the passed element
			var rootIndex = index;
			for (;;)
			{
				var parentIndex = Nodes[rootIndex].ParentIndex;
				if (parentIndex == -1)
				{
					break;
				}
				rootIndex = parentIndex;
			}

			// Then, do the path compression:
			// walk from the passed element upto the root replacing the the ParentIndex with the root index
			while (index != rootIndex)
			{
				var node = Nodes[index];
				index = node.ParentIndex;
				node.ParentIndex = rootIndex;
			}
			return rootIndex;
		}

		/// <summary>Combines to distjoint sets into a single set</summary>
		/// <param name="elementOfSet1Index">Index of an element of the first set</param>
		/// <param name="elementOfSet2Index">Index of an element of the second set</param>
		public void Union(int elementOfSet1Index, int elementOfSet2Index)
		{
			elementOfSet1Index = IndexToSetId(elementOfSet1Index);
			elementOfSet2Index = IndexToSetId(elementOfSet2Index);

			if (elementOfSet1Index == elementOfSet2Index)
			{
				return; // Already the single set
			}

			var set1Root = Nodes[elementOfSet1Index];
			var set2Root = Nodes[elementOfSet2Index];
			var rankDifference = set1Root.Rank - set2Root.Rank;

			// Attach the tree with a smaller rank to the tree with a higher rank.
			// The resulting tree rank is equal to the higher rank
			// except the case when initial ranks are equal.
			// In the latter case the new rank will be increased by 1
			if (rankDifference > 0) // 1st has higher rank
			{
				set2Root.ParentIndex = elementOfSet1Index;
			}
			else if (rankDifference < 0) // 2nd has the higher rank
			{
				set1Root.ParentIndex = elementOfSet2Index;
			}
			else // ranks are equal and the new root choice is arbitrary
			{
				set2Root.ParentIndex = elementOfSet1Index;
				++set1Root.Rank;
			}

			// we have joined 2 sets, so we have to decrease the count
			--SetsCount;
		}
	}

	/// <summary>Node base class</summary>
	public class BasicNode
	{
		/// <summary>Parent node index</summary>
		/// <remarks>Points to the root after a path compression</remarks>
		public int ParentIndex;

		/// <summary>Estimated height of the tree (i.e. maximum length of the path from the root to a node. Path compression is not taken into account)</summary>
		public int Rank;
	}
}
