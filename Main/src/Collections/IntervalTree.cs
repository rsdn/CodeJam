#if !LESSTHAN_NET35
using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Ranges;

namespace CodeJam.Collections
{
	/// <summary>
	/// Interval tree implementation.
	/// </summary>
	public class IntervalTree<T, TKey>
	{
		private readonly Range<T, TKey>[] _sortedRanges;
		private readonly int[] _treeIndexes;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		public IntervalTree(CompositeRange<T, TKey> source)
		{
			_sortedRanges = source.SubRanges.ToArray();
			_treeIndexes = new int[_sortedRanges.Length];
			ResetIndexes();
		}

		private void ResetIndexes()
		{
#if DEBUG
			for (var i = 0; i < _treeIndexes.Length; i++)
			{
				_treeIndexes[i] = -1;
			}
#endif
			SetIndexes(0, (_treeIndexes.Length - 1) / 2, _treeIndexes.Length - 1);
			DebugCode.BugIf(_treeIndexes.Min() < 0, "treeIndexes.Min() < 0");
		}

		private void SetIndexes(int startIndex, int middleIndex, int endIndex)
		{
			DebugCode.BugIf(startIndex > middleIndex, "startIndex > middleIndex");
			DebugCode.BugIf(middleIndex > endIndex, "middleIndex > endIndex");
			if (startIndex == endIndex)
				_treeIndexes[startIndex] = startIndex;
			else if (startIndex == endIndex - 1)
			{
				DebugCode.BugIf(middleIndex != startIndex, "middleIndex != startIndex");

				_treeIndexes[endIndex] = endIndex;
				_treeIndexes[startIndex] = MaxToIndex(startIndex, endIndex);
			}
			else if (startIndex == endIndex - 2)
			{
				DebugCode.BugIf(middleIndex != startIndex + 1, "middleIndex != startIndex + 1");

				_treeIndexes[endIndex] = endIndex;
				_treeIndexes[startIndex] = MaxToIndex(startIndex, endIndex);
				_treeIndexes[middleIndex] = MaxToIndex(startIndex, MaxToIndex(middleIndex, endIndex));
			}
			else
			{
				DebugCode.BugIf(
					middleIndex != startIndex + (endIndex-startIndex) / 2,
					"middleIndex != startIndex + (endIndex-startIndex) / 2");

				var newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
				SetIndexes(startIndex, newMiddleA, middleIndex - 1);

				var newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
				SetIndexes(middleIndex + 1, newMiddleB, endIndex);
				_treeIndexes[middleIndex] = MaxToIndex(_treeIndexes[newMiddleA], MaxToIndex(middleIndex, _treeIndexes[newMiddleB]));
			}
		}

		private int MaxToIndex(int indexA, int indexB) => _sortedRanges[indexA].To >= _sortedRanges[indexB].To
			? indexA
			: indexB;

		/// <summary>
		/// Find intersection between specified range and source.
		/// </summary>
		public List<Range<T, TKey>> Intersect(Range<T> intersection)
		{
			var result = new List<Range<T, TKey>>();
			Intersect(0, (_treeIndexes.Length - 1) / 2, _treeIndexes.Length - 1, intersection, result);
			return result;
		}

		private void Intersect(int startIndex, int middleIndex, int endIndex, Range<T> intersection, List<Range<T, TKey>> result)
		{
			while (true)
			{
				if (endIndex - startIndex <= 5)
				{
					for (var i = startIndex; i <= endIndex; i++)
					{
						if (_sortedRanges[i].HasIntersection(intersection))
							result.Add(_sortedRanges[i]);
					}
					return;
				}
				var middleFromBoundary = _sortedRanges[middleIndex].From;
				var middleMaxToBoundary = _sortedRanges[_treeIndexes[middleIndex]].To;

				if (intersection.To < middleFromBoundary)
				{
					//toFind ends before subtree.Data begins, prune the right subtree
					if (startIndex < middleIndex)
					{
						var newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
						var middleIndex1 = middleIndex;
						middleIndex = newMiddleA;
						endIndex = middleIndex1 - 1;
						continue;
					}
				}
				else if (intersection.From > middleMaxToBoundary)
				{
					////toFind begins after the subtree.Max ends, prune the left subtree
					if (middleIndex < endIndex)
					{
						var newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
						startIndex = middleIndex + 1;
						middleIndex = newMiddleB;
						continue;
					}
				}
				else
				{
					if (startIndex < middleIndex)
					{
						var newMiddleA = startIndex + (middleIndex - 1 - startIndex) / 2;
						Intersect(startIndex, newMiddleA, middleIndex - 1, intersection, result);
					}

					if (_sortedRanges[middleIndex].HasIntersection(intersection))
						result.Add(_sortedRanges[middleIndex]);

					////toFind begins after the subtree.Max ends, prune the left subtree
					if (middleIndex < endIndex)
					{
						var newMiddleB = middleIndex + 1 + (endIndex - (middleIndex + 1)) / 2;
						startIndex = middleIndex + 1;
						middleIndex = newMiddleB;
						continue;
					}
				}
				break;
			}
		}
	}
}
#endif