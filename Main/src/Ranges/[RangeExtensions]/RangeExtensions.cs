using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <see cref="Range{T}"/>.</summary>
	[PublicAPI]
	public static partial class RangeExtensions
	{
		#region Range factory methods
		[MethodImpl(AggressiveInlining)]
		private static TRange TryCreateRange<T, TRange>(
			this TRange range,
			RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
			where TRange : IRangeFactory<T, TRange> =>
				range.TryCreateRange(from, to);
		#endregion

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="newValueSelector">Callback to obtain a new value for the boundaries. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T2> WithValues<T, T2>(
			this Range<T> range,
			[NotNull, InstantHandle] Func<T, T2> newValueSelector)
		{
			var from = range.From.WithValue(newValueSelector);
			var to = range.To.WithValue(newValueSelector);
			return Range.TryCreate(from, to);
		}

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T2> WithValues<T, T2>(
			this Range<T> range,
			[NotNull, InstantHandle] Func<T, T2> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T2> toValueSelector)
		{
			var from = range.From.WithValue(fromValueSelector);
			var to = range.To.WithValue(toValueSelector);
			return Range.TryCreate(from, to);
		}

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="newValueSelector">Callback to obtain a new value for the boundaries. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T2, TKey> WithValues<T, T2, TKey>(
			this Range<T, TKey> range,
			[NotNull, InstantHandle] Func<T, T2> newValueSelector)
		{
			var from = range.From.WithValue(newValueSelector);
			var to = range.To.WithValue(newValueSelector);
			return Range.TryCreate(from, to, range.Key);
		}
		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T2, TKey> WithValues<T, T2, TKey>(
			this Range<T, TKey> range,
			[NotNull, InstantHandle] Func<T, T2> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T2> toValueSelector)
		{
			var from = range.From.WithValue(fromValueSelector);
			var to = range.To.WithValue(toValueSelector);
			return Range.TryCreate(from, to, range.Key);
		}

		/// <summary>Creates a range without a range key.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="range">The range to remove key from.</param>
		/// <returns>A new range without a key.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> WithoutKey<T, TKey>(this Range<T, TKey> range) =>
			Range.Create(range.From, range.To);
	}
}