using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

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
	}
}