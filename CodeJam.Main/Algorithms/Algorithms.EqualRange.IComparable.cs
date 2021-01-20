using System;
using System.Collections.Generic;

using CodeJam.Ranges;

using JetBrains.Annotations;

using Range = CodeJam.Ranges.Range;

namespace CodeJam
{
	public partial class Algorithms
	{
		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [0, list.Count - 1] such that list[i] >= value or list.Count if no such i exists
		///		j is the smallest index in the range [0, list.Count - 1] such that list[i] > value or list.Count if no such j exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Range<int> EqualRange<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value)
			where TElement : IComparable<TValue> =>
				list.EqualRange(value, 0);

		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [startIndex, list.Count - 1] such that list[i] >= value or list.Count if no such i exists
		///		j is the smallest index in the range [startIndex, list.Count - 1] such that list[i] > value or list.Count if no such j exists
		/// </summary>
		/// <typeparam name="TElement">
		/// The list element type
		/// <remarks>Should implement IComparable&lt;TValue&gt;</remarks>
		/// </typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
#pragma warning disable CA1062 // Validate arguments of public methods
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Range<int> EqualRange<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex)
			where TElement : IComparable<TValue> =>
				list.EqualRange(value, startIndex, list.Count);
#pragma warning restore CA1062 // Validate arguments of public methods

		/// <summary>
		/// Returns the tuple of [i, j] where
		///		i is the smallest index in the range [startIndex, endIndex - 1] such that list[i] >= value or endIndex if no such i exists
		///		j is the smallest index in the range [startIndex, endIndex - 1] such that list[i] > value or endIndex if no such j exists
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
		/// <returns>The tuple of lower bound and upper bound for the value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Range<int> EqualRange<TElement, TValue>(
			[InstantHandle] this IList<TElement> list,
			TValue value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
			where TElement : IComparable<TValue>
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			var upperBoundStartIndex = startIndex;
			var upperBoundEndIndex = endIndex;

			// the loop locates the lower bound at the same time restricting the range for upper bound search
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				var compareResult = list[median].CompareTo(value);
				switch (compareResult)
				{
					case < 0:
						startIndex = median + 1;
						upperBoundStartIndex = startIndex;
						break;
					case 0:
						endIndex = median;
						upperBoundStartIndex = endIndex + 1;
						break;
					default:
						endIndex = median;
						upperBoundEndIndex = endIndex;
						break;
				}
			}
			return Range.Create(startIndex, UpperBoundCore(list, value, upperBoundStartIndex, upperBoundEndIndex));
		}
	}
}