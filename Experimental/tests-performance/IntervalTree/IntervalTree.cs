using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.Ranges
{
	public class IntervalTree<T, TKey>
	{
		private Range<T, TKey>[] sortedRanges;
		private int[] treeIndexes;

		public IntervalTree(CompositeRange<T, TKey> source)
		{
			sortedRanges = source.SubRanges.ToArray();
			treeIndexes = new int[sortedRanges.Length];
			ResetIndexes();
		}

		private void ResetIndexes()
		{
#if DEBUG
			for (int i = 0; i < treeIndexes.Length; i++)
			{
				treeIndexes[i] = -1;
			}
#endif
			SetIndexes(0, (treeIndexes.Length - 1) / 2, treeIndexes.Length - 1);
			DebugCode.BugIf(treeIndexes.Min() < 0, "treeIndexes.Min() < 0");
		}

		private void SetIndexes(int startIndex, int middleIndex, int endIndex)
		{
			DebugCode.BugIf(startIndex > middleIndex, "startIndex > middleIndex");
			DebugCode.BugIf(middleIndex > endIndex, "middleIndex > endIndex");
			if (startIndex == endIndex)
				treeIndexes[startIndex] = startIndex;
			else if (startIndex == endIndex - 1)
			{
				DebugCode.BugIf(middleIndex != startIndex, "middleIndex != startIndex");

				treeIndexes[endIndex] = endIndex;
				treeIndexes[startIndex] = MaxToIndex(startIndex, endIndex);
			}
			else if (startIndex == endIndex - 2)
			{
				DebugCode.BugIf(middleIndex != startIndex + 1, "middleIndex != startIndex + 1");

				treeIndexes[endIndex] = endIndex;
				treeIndexes[startIndex] = MaxToIndex(startIndex, endIndex);
				treeIndexes[middleIndex] = MaxToIndex(startIndex, MaxToIndex(middleIndex, endIndex));
			}
			else
			{
				int newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
				SetIndexes(startIndex, newMiddleA, middleIndex - 1);

				int newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
				SetIndexes(middleIndex + 1, newMiddleB, endIndex);
				treeIndexes[middleIndex] = MaxToIndex(treeIndexes[newMiddleA], MaxToIndex(middleIndex, treeIndexes[newMiddleB]));
			}
		}

		private int MaxToIndex(int indexA, int indexB) => sortedRanges[indexA].To >= sortedRanges[indexB].To
			? indexA
			: indexB;

		public List<Range<T, TKey>> Intersect(Range<T> intersection)
		{
			var result = new List<Range<T, TKey>>();
			Intersect(0, (treeIndexes.Length - 1) / 2, treeIndexes.Length - 1, intersection, result);
			return result;
		}

		private void Intersect(
			int startIndex, int middleIndex, int endIndex, Range<T> intersection, List<Range<T, TKey>> result)
		{
			if (endIndex - startIndex <= 5)
			{
				for (int i = startIndex; i <= endIndex; i++)
				{
					if (sortedRanges[i].HasIntersection(intersection))
						result.Add(sortedRanges[i]);
				}
				return;
			}
			var middleFromBoundary = sortedRanges[middleIndex].From;
			var middleMaxToBoundary = sortedRanges[treeIndexes[middleIndex]].To;

			if (intersection.To <= middleFromBoundary)
			{
				//toFind ends before subtree.Data begins, prune the right subtree
				if (startIndex < middleIndex)
				{
					int newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
					Intersect(startIndex, newMiddleA, middleIndex - 1, intersection, result);
				}
			}
			else if (intersection.From >= middleMaxToBoundary)
			{
				////toFind begins after the subtree.Max ends, prune the left subtree
				if (middleIndex < endIndex)
				{
					int newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
					Intersect(middleIndex + 1, newMiddleB, endIndex, intersection, result);
				}
			}
			else
			{
				if (startIndex < middleIndex)
				{
					int newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
					Intersect(startIndex, newMiddleA, middleIndex - 1, intersection, result);
				}

				if (sortedRanges[middleIndex].HasIntersection(intersection))
					result.Add(sortedRanges[middleIndex]);

				////toFind begins after the subtree.Max ends, prune the left subtree
				if (middleIndex < endIndex)
				{
					int newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
					Intersect(middleIndex + 1, newMiddleB, endIndex, intersection, result);
				}
			}
		}
	}
}