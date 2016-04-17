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
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value)
			where TElement : IComparable<TValue> =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [from, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				int from)
			where TElement : IComparable<TValue> =>
			list.LowerBound(value, from, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [from, to - 1] such that list[i] >= value
		/// or "to" if no such i exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound<TElement, TValue>(
				[NotNull, InstantHandle] this IList<TElement> list,
				TValue value,
				int from,
				int to)
			where TElement : IComparable<TValue>
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(from, to, list.Count);
			while (from < to)
			{
				var median = from + (to - from) / 2;
				var compareResult = list[median].CompareTo(value);
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
	}
}
