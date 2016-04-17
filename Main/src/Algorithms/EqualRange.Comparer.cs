using System;
using System.Collections.Generic;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [0, list.Count - 1] such that list[i] >= value or list.Count if no such i exists
		///		j is the smallest index in the range [0, list.Count - 1] such that list[i] > value or list.Count if no such j exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
		public static TupleStruct<int, int> EqualRange<TElement, TValue>(this IList<TElement> list, TValue value, Func<TElement, TValue, int> comparer)
			=> EqualRange(list, value, 0, list.Count, comparer);

		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [from, list.Count - 1] such that list[i] >= value or list.Count if no such i exists
		///		j is the smallest index in the range [from, list.Count - 1] such that list[i] > value or list.Count if no such j exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
		public static TupleStruct<int, int> EqualRange<TElement, TValue>(this IList<TElement> list, TValue value, int from, Func<TElement, TValue, int> comparer)
			=> EqualRange(list, value, from, list.Count, comparer);

		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [from, to - 1] such that list[i] >= value or "to" if no such i exists
		///		j is the smallest index in the range [from, to - 1] such that list[i] > value or "to" if no such j exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
		public static TupleStruct<int, int> EqualRange<TElement, TValue>(this IList<TElement> list, TValue value, int from, int to, Func<TElement, TValue, int> comparer)
		{
			ValidateIndicesRange(from, to, list.Count);
			var upperBoundFrom = from;
			var upperBoundTo = to;

			// the loop locates the lower bound at the same time restricting the range for upper bound search
			while (from < to)
			{
				var median = from + (to - from) / 2;
				var compareResult = comparer(list[median], value);
				if (compareResult < 0)
				{
					from = median + 1;
					upperBoundFrom = from;
				}
				else if (compareResult == 0)
				{
					to = median;
					upperBoundFrom = to + 1;
				}
				else
				{
					to = median;
					upperBoundTo = to;
				}
			}
			return TupleStruct.Create(from, UpperBoundCore(list, value, upperBoundFrom, upperBoundTo, comparer));
		}
	}
}
