using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using static CodeJam.RangesV2.RangeInternal;

namespace CodeJam.RangesV2
{
	/// <summary>Helper methods for the <seealso cref="Range{T}" />.</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public static class Range
	{
		#region Boundary factory methods
		/// <summary>Inclusive From boundary factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New inclusive from boundary, or the positive infinity boundary if the <paramref name="fromValue" /> is <c>null</c>
		/// </returns>
		public static RangeBoundary<T> BoundaryFrom<T>(T fromValue) =>
#pragma warning disable 618 // Args are validated
			new RangeBoundary<T>(
				fromValue,
				fromValue == null
					? RangeBoundaryKind.NegativeInfinity
					: RangeBoundaryKind.FromInclusive,
				SkipsArgValidation);
#pragma warning restore 618

		/// <summary>Exclusive From boundary factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New exclusive from boundary, or the positive infinity boundary if the <paramref name="fromValue" /> is <c>null</c>
		/// </returns>
		public static RangeBoundary<T> ExclusiveBoundaryFrom<T>(T fromValue) =>
#pragma warning disable 618 // Args are validated
			new RangeBoundary<T>(
				fromValue,
				fromValue == null
					? RangeBoundaryKind.NegativeInfinity
					: RangeBoundaryKind.FromExclusive,
				SkipsArgValidation);
#pragma warning restore 618

		/// <summary>Negative infinity boundary (-∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The negative infinity boundary (-∞).</returns>
		public static RangeBoundary<T> InfiniteBoundaryFrom<T>() =>
			RangeBoundary<T>.NegativeInfinity;

		/// <summary>Inclusive To boundary factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New inclusive to boundary, or the positive infinity boundary if the <paramref name="toValue" /> is <c>null</c>
		/// </returns>
		public static RangeBoundary<T> BoundaryTo<T>(T toValue) =>
#pragma warning disable 618 // Args are validated
			new RangeBoundary<T>(
				toValue,
				toValue == null
					? RangeBoundaryKind.PositiveInfinity
					: RangeBoundaryKind.ToInclusive,
				SkipsArgValidation);
#pragma warning restore 618

		/// <summary>Exclusive To boundary factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New exclusive to boundary, or the positive infinity boundary if the <paramref name="toValue" /> is <c>null</c>
		/// </returns>
		public static RangeBoundary<T> ExclusiveBoundaryTo<T>(T toValue) =>
#pragma warning disable 618 // Args are validated
			new RangeBoundary<T>(
				toValue,
				toValue == null
					? RangeBoundaryKind.PositiveInfinity
					: RangeBoundaryKind.ToExclusive,
				SkipsArgValidation);
#pragma warning restore 618

		/// <summary>Positive infinity boundary (+∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The positive infinity boundary (-∞).</returns>
		public static RangeBoundary<T> InfiniteBoundaryTo<T>() =>
			RangeBoundary<T>.PositiveInfinity;
		#endregion

		#region Boundary operations
		/// <summary>Returns the less one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The less one of the two boundaries.</returns>
		public static RangeBoundary<T> Min<T>(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2)
		{
			ValidateSame(boundary1, boundary2);
			return boundary1 <= boundary2 ? boundary1 : boundary2;
		}

		/// <summary>Returns the greater one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The greater one of the two boundaries.</returns>
		public static RangeBoundary<T> Max<T>(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2)
		{
			ValidateSame(boundary1, boundary2);
			return boundary1 >= boundary2 ? boundary1 : boundary2;
		}

		/// <summary>Returns the less one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two From boundaries.</returns>
		public static RangeBoundary<T> MinFrom<T>(T value1, T value2) => Min(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the less one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two From boundaries.</returns>
		public static RangeBoundary<T> MaxFrom<T>(T value1, T value2) => Max(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the less one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two To boundaries.</returns>
		public static RangeBoundary<T> MinTo<T>(T value1, T value2) => Min(BoundaryTo(value1), BoundaryTo(value2));

		/// <summary>Returns the less one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two To boundaries.</returns>
		public static RangeBoundary<T> MaxTo<T>(T value1, T value2) => Max(BoundaryTo(value1), BoundaryTo(value2));
		#endregion

		#region Range validation
		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">The value of the from boundary.</param>
		/// <param name="to">The value of the to boundary.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		public static bool IsValid<T>(T from, T to) =>
			BoundaryFrom(from) <= BoundaryTo(to);

		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">The from boundary.</param>
		/// <param name="to">The to boundary.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		public static bool IsValid<T>(RangeBoundary<T> from, RangeBoundary<T> to) =>
			(from.IsEmpty && to.IsEmpty) ||
				(from.IsFromBoundary && to.IsToBoundary && from <= to);
		#endregion

		#region Range factory methods
		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">From boundary.</param>
		/// <param name="to">To boundary.</param>
		/// <returns></returns>
		public static Range<T> Create<T>(RangeBoundary<T> from, RangeBoundary<T> to) =>
			new Range<T>(from, to);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from inclusive boundary.</param>
		/// <param name="toValue">The value of to inclusive boundary.</param>
		/// <returns>A new range.</returns>
		public static Range<T> Create<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFrom(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from exclusive boundary.</param>
		/// <param name="toValue">The value of to exclusive boundary.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusive<T>(T fromValue, T toValue) =>
			new Range<T>(ExclusiveBoundaryFrom(fromValue), ExclusiveBoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from exclusive boundary.</param>
		/// <param name="toValue">The value of to inclusive boundary.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusiveFrom<T>(T fromValue, T toValue) =>
			new Range<T>(ExclusiveBoundaryFrom(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from inclusive boundary.</param>
		/// <param name="toValue">The value of to exclusive boundary.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusiveTo<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFrom(fromValue), ExclusiveBoundaryTo(toValue));
		#endregion

		#region Failsafe Range factory methods
		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">From boundary.</param>
		/// <param name="to">To boundary.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreate<T>(RangeBoundary<T> from, RangeBoundary<T> to) =>
			IsValid(from, to)
#pragma warning disable 618 // Args are validated
				? new Range<T>(from, to, SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from inclusive boundary.</param>
		/// <param name="toValue">The value of to inclusive boundary.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreate<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(BoundaryFrom(fromValue), BoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from exclusive boundary.</param>
		/// <param name="toValue">The value of to exclusive boundary.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusive<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(ExclusiveBoundaryFrom(fromValue), ExclusiveBoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from exclusive boundary.</param>
		/// <param name="toValue">The value of to inclusive boundary.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusiveFrom<T>(T fromValue, T toValue) => IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
			? new Range<T>(ExclusiveBoundaryFrom(fromValue), BoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
			: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of from inclusive boundary.</param>
		/// <param name="toValue">The value of to exclusive boundary.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusiveTo<T>(T fromValue, T toValue) => IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
			? new Range<T>(BoundaryFrom(fromValue), ExclusiveBoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
			: Range<T>.Empty;
		#endregion
	}
}