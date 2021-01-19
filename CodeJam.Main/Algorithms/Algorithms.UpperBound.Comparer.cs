﻿using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	public partial class Algorithms
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int UpperBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[InstantHandle] Func<TElement, TValue, int> comparer) =>
				list.UpperBound(value, 0, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] > value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int UpperBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[InstantHandle] Func<TElement, TValue, int> comparer) =>
				list.UpperBound(value, startIndex, list.Count, comparer);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] > value
		/// or endIndex if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int UpperBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex,
			[InstantHandle] Func<TElement, TValue, int> comparer)
		{
			Code.NotNull(list, nameof(list));
			Code.NotNull(comparer, nameof(comparer));
			ValidateIndicesRange(startIndex, endIndex, list.Count);

			return UpperBoundCore(list, value, startIndex, endIndex, comparer);
		}

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] > value
		/// or endIndex if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The upper bound for the value</returns>
		private static int UpperBoundCore<TElement, TValue>(
			this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex,
			Func<TElement, TValue, int> comparer)
		{
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				var compareResult = comparer(list[median], value);
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