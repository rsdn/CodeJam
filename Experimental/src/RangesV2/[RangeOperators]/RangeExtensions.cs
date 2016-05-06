using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>Extension methods for <seealso cref="Range{T}"/>.</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public static partial class RangeExtensions
	{
		private static TRange MakeInclusiveCore<T, TRange>(
			TRange range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
			where TRange : IRangeFactory<T, TRange>
		{
			if (range.From.IsEmpty || (!range.From.IsExclusiveBoundary && !range.To.IsExclusiveBoundary))
			{
				return range;
			}

			var from = range.From;
			if (from.IsExclusiveBoundary)
			{
				from = Range.BoundaryFrom(fromValueSelector(from.GetValueOrDefault()));
			}
			var to = range.To;
			if (to.IsExclusiveBoundary)
			{
				to = Range.BoundaryTo(toValueSelector(to.GetValueOrDefault()));
			}

			return range.TryCreateRange(from, to);
		}

		private static TRange MakeExclusiveCore<T, TRange>(
			TRange range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
			where TRange : IRangeFactory<T, TRange>
		{
			if (range.From.IsEmpty || (!range.From.IsInclusiveBoundary && !range.To.IsInclusiveBoundary))
			{
				return range;
			}

			var from = range.From;
			if (from.IsInclusiveBoundary)
			{
				from = Range.BoundaryFromExclusive(fromValueSelector(from.GetValueOrDefault()));
			}
			var to = range.To;
			if (to.IsInclusiveBoundary)
			{
				to = Range.BoundaryToExclusive(toValueSelector(to.GetValueOrDefault()));
			}

			return range.TryCreateRange(from, to);
		}

		private static TRange WithValuesCore<T, TRange>(
			TRange range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
			where TRange : IRangeFactory<T, TRange>
		{
			var from = range.From.WithValue(fromValueSelector);
			var to = range.To.WithValue(toValueSelector);
			return range.TryCreateRange(from, to);
		}

		#region Self-operations
		/// <summary>Creates a range without a range key.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="range">The range to remove key from.</param>
		/// <returns>A new range without a key.</returns>
		public static Range<T> WithoutKey<T, TKey>(this Range<T, TKey> range) =>
			Range.Create(range.From, range.To);
		#endregion
	}
}