using System;
using System.Collections.Generic;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] > value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		public static int UpperBound<TElement, TValue>(this IList<TElement> list, TValue value, Func<TElement, TValue, int> comparer)
			=> list.UpperBound(value, 0, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [from, list.Count - 1] such that list[i] > value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		public static int UpperBound<TElement, TValue>(this IList<TElement> list, TValue value, int from, Func<TElement, TValue, int> comparer)
			=> list.UpperBound(value, from, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [from, to - 1] such that list[i] > value
		/// or "to" if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		public static int UpperBound<TElement, TValue>(this IList<TElement> list, TValue value, int from, int to, Func<TElement, TValue, int> comparer)
		{
			ValidateIndicesRange(from, to, list.Count);
			return UpperBoundCore(list, value, from, to, comparer);
		}

		/// <summary>
		/// Returns the minimum index i in the range [from, to - 1] such that list[i] > value
		/// or "to" if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="from">The minimum index</param>
		/// <param name="to">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		private static int UpperBoundCore<TElement, TValue>(this IList<TElement> list, TValue value, int from, int to, Func<TElement, TValue, int> comparer)
		{
			while (from < to)
			{
				var median = from + (to - from) / 2;
				var compareResult = comparer(list[median], value);
				if (compareResult > 0)
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
