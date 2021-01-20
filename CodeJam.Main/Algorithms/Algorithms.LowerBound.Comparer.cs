using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	public partial class Algorithms
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
#pragma warning disable CA1062 // Validate arguments of public methods
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int LowerBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[InstantHandle] Func<TElement, TValue, int> comparer) =>
				list.LowerBound(value, 0, list.Count, comparer);
#pragma warning restore CA1062 // Validate arguments of public methods

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The lower bound for the value</returns>
#pragma warning disable CA1062 // Validate arguments of public methods
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int LowerBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[InstantHandle] Func<TElement, TValue, int> comparer) =>
				list.LowerBound(value, startIndex, list.Count, comparer);
#pragma warning restore CA1062 // Validate arguments of public methods

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <typeparam name="TElement">The list element type</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <param name="comparer">The function with the Comparer&lt;T&gt;.Compare semantics</param>
		/// <returns>The lower bound for the value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int LowerBound<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex,
			[InstantHandle] Func<TElement, TValue, int> comparer)
		{
			Code.NotNull(list, nameof(list));
			Code.NotNull(comparer, nameof(comparer));
			ValidateIndicesRange(startIndex, endIndex, list.Count);

			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				var compareResult = comparer(list[median], value);
				if (compareResult >= 0)
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

		/// <summary>Validates a range of indices of a list</summary>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound of the index (not included)</param>
		/// <param name="count">The number of elements in the list</param>
		private static void ValidateIndicesRange(
			[NonNegativeValue] int startIndex, [NonNegativeValue] int endIndex, [NonNegativeValue] int count)
		{
			Code.InRange(startIndex, nameof(startIndex), 0, endIndex);
			Code.InRange(endIndex, nameof(endIndex), startIndex, count);
		}
	}
}