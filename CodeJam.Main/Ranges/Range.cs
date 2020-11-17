using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Ranges.RangeInternal;
using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Ranges
{
	// TODO: Conflict with System.Range
	/// <summary>Helper methods for the <seealso cref="Range{T}"/>.</summary>
	[PublicAPI]
	public static partial class Range
	{
		#region CompareTo boundary
		/// <summary>Helper method for obtaining a comparison boundary from a value.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="value">The value to compare with.</param>
		/// <returns>A new boundary to be used in comparison</returns>
		[MethodImpl(AggressiveInlining)]
		internal static RangeBoundaryFrom<T> GetCompareToBoundary<T>(T? value) =>
			RangeBoundaryFrom<T>.AdjustAndCreate(value, RangeBoundaryFromKind.Inclusive);
		#endregion

		#region Boundary factory methods
		/// <summary>Inclusive boundary From factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New inclusive boundary From,
		/// or negative infinity boundary if the <paramref name="fromValue"/> is <c>null</c> or equals to NegativeInfinity static field of the <typeparamref name="T"/>.
		/// or empty boundary if the <paramref name="fromValue"/> equals to NaN static field of the <typeparamref name="T"/>.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> BoundaryFrom<T>(T? fromValue) =>
			RangeBoundaryFrom<T>.AdjustAndCreate(fromValue, RangeBoundaryFromKind.Inclusive);

		/// <summary>Exclusive boundary From factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New exclusive boundary From,
		/// or negative infinity boundary if the <paramref name="fromValue"/> is <c>null</c> or equals to NegativeInfinity static field of the <typeparamref name="T"/>.
		/// or empty boundary if the <paramref name="fromValue"/> equals to NaN static field of the <typeparamref name="T"/>.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> BoundaryFromExclusive<T>(T? fromValue) =>
			RangeBoundaryFrom<T>.AdjustAndCreate(fromValue, RangeBoundaryFromKind.Exclusive);

		/// <summary>Negative infinity boundary (-∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The negative infinity boundary (-∞).</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> BoundaryFromInfinity<T>() =>
			RangeBoundaryFrom<T>.NegativeInfinity;

		/// <summary>Inclusive boundary To factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New inclusive boundary To,
		/// or positive infinity boundary if the <paramref name="toValue"/> is <c>null</c> or equals to PositiveInfinity static field of the <typeparamref name="T"/>.
		/// or empty boundary if the <paramref name="toValue"/> equals to NaN static field of the <typeparamref name="T"/>.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> BoundaryTo<T>(T? toValue) =>
			RangeBoundaryTo<T>.AdjustAndCreate(toValue, RangeBoundaryToKind.Inclusive);

		/// <summary>Exclusive boundary To factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New exclusive boundary To,
		/// or positive infinity boundary if the <paramref name="toValue"/> is <c>null</c> or equals to PositiveInfinity static field of the <typeparamref name="T"/>.
		/// or empty boundary if the <paramref name="toValue"/> equals to NaN static field of the <typeparamref name="T"/>.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> BoundaryToExclusive<T>(T? toValue) =>
			RangeBoundaryTo<T>.AdjustAndCreate(toValue, RangeBoundaryToKind.Exclusive);

		/// <summary>Positive infinity boundary (+∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The positive infinity boundary (-∞).</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> BoundaryToInfinity<T>() =>
			RangeBoundaryTo<T>.PositiveInfinity;
		#endregion

		#region Boundary operations
		/// <summary>Returns the less one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The less one of the two boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> Min<T>(RangeBoundaryFrom<T> boundary1, RangeBoundaryFrom<T> boundary2) =>
			boundary1 <= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the less one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The less one of the two boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> Min<T>(RangeBoundaryTo<T> boundary1, RangeBoundaryTo<T> boundary2) =>
			boundary1 <= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the greater one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The greater one of the two boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> Max<T>(RangeBoundaryFrom<T> boundary1, RangeBoundaryFrom<T> boundary2) =>
			boundary1 >= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the greater one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The greater one of the two boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> Max<T>(RangeBoundaryTo<T> boundary1, RangeBoundaryTo<T> boundary2) =>
			boundary1 >= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the less one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The less one of the two From boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> MinFrom<T>(T? value1, T value2) =>
			Min(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the greater one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two From boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> MaxFrom<T>(T? value1, T value2) =>
			Max(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the less one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The less one of the two To boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> MinTo<T>(T? value1, T value2) =>
			Min(BoundaryTo(value1), BoundaryTo(value2));

		/// <summary>Returns the greater one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two To boundaries.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static RangeBoundaryTo<T> MaxTo<T>(T? value1, T value2) =>
			Max(BoundaryTo(value1), BoundaryTo(value2));
		#endregion

		#region Range validation
		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">The value of the boundary From.</param>
		/// <param name="to">The value of the boundary To.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static bool IsValid<T>(T? from, T? to) =>
			RangeBoundaryFrom<T>.IsValid(from) &&
				RangeBoundaryTo<T>.IsValid(to) &&
				IsValid(BoundaryFrom(from), BoundaryTo(to));

		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static bool IsValid<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			from.IsEmpty ? to.IsEmpty : from <= to;
		#endregion

		#region Failsafe Range factory methods
		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> TryCreate<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			IsValid(from, to)
#pragma warning disable 618 // Validation not required: IsValid() called.
				? new Range<T>(from, to, SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		[MethodImpl(AggressiveInlining)]
		private static Range<T> TryCreateCore<T>(
			T from, RangeBoundaryFromKind fromKind,
			T to, RangeBoundaryToKind toKind) =>
				RangeBoundaryFrom<T>.IsValid(from) && RangeBoundaryTo<T>.IsValid(to)
					? TryCreate(
						RangeBoundaryFrom<T>.AdjustAndCreate(from, fromKind),
						RangeBoundaryTo<T>.AdjustAndCreate(to, toKind))
					: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <param name="key">The value of the range key</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> TryCreate<T, TKey>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to, TKey key) =>
			IsValid(from, to)
#pragma warning disable 618 // Validation not required: IsValid() called.
				? new Range<T, TKey>(from, to, key, SkipsArgValidation)
				: new Range<T, TKey>(RangeBoundaryFrom<T>.Empty, RangeBoundaryTo<T>.Empty, key, SkipsArgValidation);
#pragma warning restore 618

		[MethodImpl(AggressiveInlining)]
		private static Range<T, TKey> TryCreateCore<T, TKey>(
			T from, RangeBoundaryFromKind fromKind,
			T to, RangeBoundaryToKind toKind,
			TKey key) =>
				RangeBoundaryFrom<T>.IsValid(from) && RangeBoundaryTo<T>.IsValid(to)
					? TryCreate(
						RangeBoundaryFrom<T>.AdjustAndCreate(from, fromKind),
						RangeBoundaryTo<T>.AdjustAndCreate(to, toKind),
						key)
					: Range<T, TKey>.Empty;
		#endregion
	}
}