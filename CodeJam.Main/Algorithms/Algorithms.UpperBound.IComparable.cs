using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] > value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The upper bound for the value</returns>
		[Pure]
		public static int UpperBound<TElement, TValue>(
			[NotNull, ItemNotNull, InstantHandle] this IList<TElement> list, TValue value)
			where TElement : IComparable<TValue> =>
			list.UpperBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] > value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The upper bound for the value</returns>
		[Pure]
		public static int UpperBound<TElement, TValue>(
			[NotNull, ItemNotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				int startIndex)
			where TElement : IComparable<TValue> =>
			list.UpperBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] > value
		/// or endIndex if no such i exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The upper bound for the value</returns>
		[Pure]
		public static int UpperBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				int startIndex,
				int endIndex)
			 where TElement : IComparable<TValue>
		{
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			return UpperBoundCore(list, value, startIndex, endIndex);
		}

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] > value
		/// or endIndex if no such i exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The upper bound for the value</returns>
		private static int UpperBoundCore<TElement, TValue>(
				[NotNull, ItemNotNull, InstantHandle] IList<TElement> list,
				TValue value,
				int startIndex,
				int endIndex)
			where TElement : IComparable<TValue>
		{
			Code.NotNull(list, nameof(list));
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				var compareResult = list[median].CompareTo(value);
				if (compareResult > 0)
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
