using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <see cref="CompositeRange{T}"/>.</summary>
	[PublicAPI]
	public static partial class CompositeRangeExtensions
	{
		#region ToCompositeRange
		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRange<T, TKey>(
			this IEnumerable<TKey> source,
			[InstantHandle] Func<TKey, T> fromValueSelector,
			[InstantHandle] Func<TKey, T> toValueSelector)
			where TKey : notnull =>
				source
					.Select(s => Range.Create(fromValueSelector(s), toValueSelector(s), s))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRange<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector,
			[InstantHandle] Func<TSource, T> toValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				source
					.Select(s => Range.Create(fromValueSelector(s), toValueSelector(s), keySelector(s)))
					.ToCompositeRange();

		/// <summary>
		/// Converts sequence of elements to the composite range using only From boundary.
		/// The To boundary value is taken from the next item in sequence (+∞ for the last item in sequence)
		/// </summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TSource> ToCompositeRangeFrom<TSource, T>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector)
			where TSource : notnull =>
				ToCompositeRangeFrom(source, fromValueSelector, t => t);

		/// <summary>
		/// Converts sequence of elements to the composite range using only From boundary.
		/// The To boundary value is taken from the next item in sequence (+∞ for the last item in sequence)
		/// </summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeFrom<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull
		{
			var keyAndFromBoundary = source
				.Select(s => (From: Range.BoundaryFrom(fromValueSelector(s)), Key: keySelector(s)))
				.OrderBy(s => s.From)
				.ToArray();

			if (keyAndFromBoundary.Length == 0)
				return CompositeRange<T, TKey>.Empty;

			// logic is following:
			// foreach item in sequence
			//   if same boundary as before - add to pending
			//   else add all (pending to current) ranges. Store current as pending.

			var prevBoundary = RangeBoundaryFrom<T>.Empty;
			var prevKeys = new List<TKey>();
			var ranges = new List<Range<T, TKey>>();
			foreach (var (rangeFrom, key) in keyAndFromBoundary)
			{
				if (prevBoundary != rangeFrom)
				{
					ranges.AddRange(
						prevKeys.Select(prevKey => Range.Create(prevBoundary, rangeFrom.GetComplementation(), prevKey)));
					prevKeys.Clear();
					prevBoundary = rangeFrom;
				}

				prevKeys.Add(key);
			}

			ranges.AddRange(
				prevKeys.Select(prevKey => Range.Create(prevBoundary, RangeBoundaryTo<T>.PositiveInfinity, prevKey)));

			return ranges.ToCompositeRange();
		}

		/// <summary>
		/// Converts sequence of elements to the composite range using only To boundary.
		/// The From boundary value is taken from the previous item in sequence (-∞ for the last item in sequence).
		/// </summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TSource> ToCompositeRangeTo<TSource, T>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> toValueSelector)
			where TSource : notnull =>
				ToCompositeRangeTo(source, toValueSelector, t => t);

		/// <summary>
		/// Converts sequence of elements to the composite range using only To boundary.
		/// The From boundary value is taken from the previous item in sequence (-∞ for the last item in sequence).
		/// </summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeTo<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> toValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull
		{
			var keyAndToBoundary = source
				.Select(s => (Key: keySelector(s), To: Range.BoundaryTo(toValueSelector(s))))
				.OrderBy(s => s.To)
				.ToArray();

			if (keyAndToBoundary.Length == 0)
				return CompositeRange<T, TKey>.Empty;

			// logic is following:
			// foreach item in sequence
			//   if To is same as before - add same range with new key
			//   else add range (old.To..To, key) and.

			var prevRange = Range<T, TKey>.Empty;
			var ranges = new List<Range<T, TKey>>();
			foreach (var (key, to) in keyAndToBoundary)
			{
				if (prevRange.IsEmpty)
					prevRange = Range.Create(RangeBoundaryFrom<T>.NegativeInfinity, to, key);
				else if (prevRange.To == to)
					prevRange = prevRange.WithKey(key);
				else
					prevRange = Range.Create(prevRange.To.GetComplementation(), to, key);
				ranges.Add(prevRange);
			}

			return ranges.ToCompositeRange();
		}
		#endregion

		#region ToCompositeRangeExclusive
		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusive<T, TKey>(
			this IEnumerable<TKey> source,
			[InstantHandle] Func<TKey, T> fromValueSelector,
			[InstantHandle] Func<TKey, T> toValueSelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusive(fromValueSelector(s), toValueSelector(s), s))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusive<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector,
			[InstantHandle] Func<TSource, T> toValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusive(fromValueSelector(s), toValueSelector(s), keySelector(s)))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusiveFrom<T, TKey>(
			this IEnumerable<TKey> source,
			[InstantHandle] Func<TKey, T> fromValueSelector,
			[InstantHandle] Func<TKey, T> toValueSelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusiveFrom(fromValueSelector(s), toValueSelector(s), s))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusiveFrom<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector,
			[InstantHandle] Func<TSource, T> toValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusiveFrom(fromValueSelector(s), toValueSelector(s), keySelector(s)))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusiveTo<T, TKey>(
			this IEnumerable<TKey> source,
			[InstantHandle] Func<TKey, T> fromValueSelector,
			[InstantHandle] Func<TKey, T> toValueSelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusiveTo(fromValueSelector(s), toValueSelector(s), s))
					.ToCompositeRange();

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="TSource">The type of the values in original collection.</typeparam>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="source">Original collection.</param>
		/// <param name="fromValueSelector">Callback to obtain a value for the From boundary.</param>
		/// <param name="toValueSelector">Callback to obtain a value for the To boundary.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with keys filled from the original collection.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T, TKey> ToCompositeRangeExclusiveTo<TSource, T, TKey>(
			this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> fromValueSelector,
			[InstantHandle] Func<TSource, T> toValueSelector,
			[InstantHandle] Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				source
					.Select(s => Range.CreateExclusiveTo(fromValueSelector(s), toValueSelector(s), keySelector(s)))
					.ToCompositeRange();
		#endregion

		#region GetComplementation
		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="range"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> GetComplementation<T>(this Range<T> range) =>
			range.ToCompositeRange().GetComplementation();

		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="range"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="range">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> GetComplementation<T, TKey>(this Range<T, TKey> range) =>
			range.ToCompositeRange().GetComplementation();
		#endregion
	}
}