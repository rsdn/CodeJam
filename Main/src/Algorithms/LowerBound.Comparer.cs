using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				[NotNull, InstantHandle] Func<TElement, TValue, int> comparer) =>
			list.LowerBound(value, 0, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [from, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				int from,
				[NotNull, InstantHandle] Func<TElement, TValue, int> comparer) =>
			list.LowerBound(value, from, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [from, to - 1] such that list[i] >= value
		/// or "to" if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
			[NotNull, InstantHandle] this IList<TElement> list,
			TValue value,
			int from,
			int to,
			[NotNull, InstantHandle] Func<TElement, TValue, int> comparer)
		{
			Code.NotNull(list, nameof(list));
			Code.NotNull(comparer, nameof(comparer));
			ValidateIndicesRange(from, to, list.Count);

			while (from < to)
			{
				var median = from + (to - from) / 2;
				var compareResult = comparer(list[median], value);
				if (compareResult >= 0)
				{
					to = median;
				}
				else
				{
					from = median + 1;
				}
			}
			return from;
		}

		/// <summary>Validates a range of indices of a list</summary>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound of the index (not included)</param>
		/// <param name="count">The number of elements in the list</param>
		private static void ValidateIndicesRange(int from, int to, int count)
		{
			Code.InRange(from, nameof(from), 0);
			Code.InRange(to, nameof(to), 0);
			Code.AssertArgument(
				to <= count,
				nameof(to),
				() => $"The {nameof(to)} index should not exceed the {nameof(count)}");
			Code.AssertArgument(
				to >= from,
				nameof(from),
				() => $"The {nameof(to)} index should be not less than the {nameof(from)} index");
		}
	}
}
