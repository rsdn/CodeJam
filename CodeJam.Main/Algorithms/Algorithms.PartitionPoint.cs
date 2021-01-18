using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Returns the index i in the range [0, list.Count - 1] such that
		///		predicate(list[j]) = true for j &lt; i
		///		and predicate(list[k]) = false for k >= i
		/// or list.Count if no such i exists
		/// <remarks>The list should be partitioned according to the predicate</remarks>
		/// </summary>
		/// <typeparam name="T">The list element type</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>The partition point</returns>
		public static int PartitionPoint<T>(this IList<T> list, Predicate<T> predicate) =>
			PartitionPoint(list, 0, list.Count, predicate);

		/// <summary>
		/// Returns the index i in the range [startIndex, list.Count - 1] such that
		///		predicate(list[j]) = true for j &lt; i
		///		and predicate(list[k]) = false for k >= i
		/// or list.Count if no such i exists
		/// <remarks>The list should be partitioned according to the predicate</remarks>
		/// </summary>
		/// <typeparam name="T">The list element type</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>The partition point</returns>
		public static int PartitionPoint<T>(this IList<T> list, [NonNegativeValue] int startIndex, Predicate<T> predicate) =>
			PartitionPoint(list, startIndex, list.Count, predicate);

		/// <summary>
		/// Returns the index i in the range [startIndex, endIndex - 1] such that
		///		predicate(list[j]) = true for j &lt; i
		///		and predicate(list[k]) = false for k >= i
		/// or endIndex if no such i exists
		/// <remarks>The list should be partitioned according to the predicate</remarks>
		/// </summary>
		/// <typeparam name="T">The list element type</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>The partition point</returns>
		public static int PartitionPoint<T>(
			this IList<T> list, [NonNegativeValue] int startIndex, [NonNegativeValue] int endIndex, Predicate<T> predicate)
		{
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				var testResult = predicate(list[median]);
				if (!testResult)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
	}
}