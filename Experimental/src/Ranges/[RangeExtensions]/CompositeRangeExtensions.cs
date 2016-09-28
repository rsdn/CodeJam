using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;
namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <seealso cref="CompositeRange{T}"/>.</summary>
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
		public static CompositeRange<T, TKey> ToCompositeRange<T, TKey>(
			[NotNull] this IEnumerable<TKey> source,
			[NotNull] Func<TKey, T> fromValueSelector,
			[NotNull] Func<TKey, T> toValueSelector) =>
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
		public static CompositeRange<T, TKey> ToCompositeRange<TSource, T, TKey>(
			[NotNull] this IEnumerable<TSource> source,
			[NotNull] Func<TSource, T> fromValueSelector,
			[NotNull] Func<TSource, T> toValueSelector,
			[NotNull] Func<TSource, TKey> keySelector) =>
				source
					.Select(s => Range.Create(fromValueSelector(s), toValueSelector(s), keySelector(s)))
					.ToCompositeRange();
		#endregion

		#region Update range
		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		public static CompositeRange<T, TKey2> WithKeys<T, TKey2>(this CompositeRange<T> compositeRange, TKey2 key) =>
			compositeRange.IsEmpty
				? CompositeRange<T, TKey2>.Empty
				: compositeRange.SubRanges.Select(s => s.WithKey(key)).ToCompositeRange();

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		public static CompositeRange<T, TKey2> WithKeys<T, TKey, TKey2>(this CompositeRange<T, TKey> compositeRange, TKey2 key) =>
			compositeRange.IsEmpty
				? CompositeRange<T, TKey2>.Empty
				: compositeRange.SubRanges.Select(s => s.WithKey(key)).ToCompositeRange();

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		public static CompositeRange<T, TKey2> WithKeys<T, TKey, TKey2>(
			this CompositeRange<T, TKey> compositeRange,
			[NotNull] Func<TKey, TKey2> keySelector) =>
				compositeRange.IsEmpty
					? CompositeRange<T, TKey2>.Empty
					: compositeRange.SubRanges.Select(s => s.WithKey(keySelector(s.Key))).ToCompositeRange();

		/// <summary>Removes keys from the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <returns>A new composite range without associated keys.</returns>
		public static CompositeRange<T> WithoutKeys<T, TKey>(this CompositeRange<T, TKey> compositeRange) =>
			compositeRange.IsEmpty
				? CompositeRange<T>.Empty
				: compositeRange.SubRanges.Select(s => s.WithoutKey()).ToCompositeRange();
		#endregion

		#region GetComplementation
		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="range"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		public static CompositeRange<T> GetComplementation<T>(this Range<T> range) =>
			GetComplementationCore<T, CompositeRange<T>>(range.ToCompositeRange());

		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="range"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="range">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		public static CompositeRange<T> GetComplementation<T, TKey>(this Range<T, TKey> range) =>
			GetComplementationCore<T, CompositeRange<T, TKey>>(range.ToCompositeRange());

		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="compositeRange"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		public static CompositeRange<T> GetComplementation<T>(this CompositeRange<T> compositeRange) =>
			GetComplementationCore<T, CompositeRange<T>>(compositeRange);

		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(<paramref name="compositeRange"/>).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <returns>Complementation composite range.</returns>
		public static CompositeRange<T> GetComplementation<T, TKey>(this CompositeRange<T, TKey> compositeRange) =>
			GetComplementationCore<T, CompositeRange<T, TKey>>(compositeRange);

		private static CompositeRange<T> GetComplementationCore<T, TCompositeRange>(
			TCompositeRange compositeRange)
			where TCompositeRange:ICompositeRange<T>
		{
			if (compositeRange.IsEmpty)
			{
				return CompositeRange<T>.Infinite;
			}

			var result = new List<Range<T>>();

			if (compositeRange.ContainingRange.From.HasValue)
			{
				result.Add(
					Range.Create(
						RangeBoundaryFrom<T>.NegativeInfinity,
						compositeRange.ContainingRange.From.GetComplementation()));
			}

			var previousRange = Range<T>.Empty;
			foreach (var range in compositeRange.GetMergedRanges())
			{
				if (previousRange.IsNotEmpty)
				{
					result.Add(
						Range.Create(
							previousRange.To.GetComplementation(),
							range.From.GetComplementation()));
				}
				previousRange = range;
			}

			if (compositeRange.ContainingRange.To.HasValue)
			{
				result.Add(
					Range.Create(
						compositeRange.ContainingRange.To.GetComplementation(),
						RangeBoundaryTo<T>.PositiveInfinity));
			}

			return new CompositeRange<T>(result, UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged);
		}
		#endregion
	}
}