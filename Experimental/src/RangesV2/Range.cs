using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using static CodeJam.RangesV2.RangeInternal;

namespace CodeJam.RangesV2
{
	/// <summary>Helper methods for the <seealso cref="Range{T}"/>.</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public static class Range
	{
		#region Assertion methods
		/// <summary>Validates that the boundary is not empty.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary">The boundary.</param>
		/// <exception cref="System.ArgumentException">Thrown if the boundary is empty.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateNotEmpty<T>(RangeBoundaryFrom<T> boundary)
		{
			if (boundary.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(boundary),
					"The boundary should be not empty.");
			}
		}

		/// <summary>Validates that the boundary is not empty.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary">The boundary.</param>
		/// <exception cref="System.ArgumentException">Thrown if the boundary is empty.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateNotEmpty<T>(RangeBoundaryTo<T> boundary)
		{
			if (boundary.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(boundary),
					"The boundary should be not empty.");
			}
		}

		/// <summary>Validates that the range is not empty.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <exception cref="System.ArgumentException">Thrown if the range is empty.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateNotEmpty<T>(Range<T> range)
		{
			if (range.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(range),
					"The range should be not empty.");
			}
		}
		#endregion

		#region Boundary factory methods
		/// <summary>Inclusive boundary From factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New inclusive boundary From, or the negative infinity boundary if the <paramref name="fromValue"/> is <c>null</c>.
		/// </returns>
		public static RangeBoundaryFrom<T> BoundaryFrom<T>(T fromValue)
		{
			var kind = RangeBoundaryFromKind.Inclusive;
			RangeBoundaryFrom<T>.CoerceBoundaryValue(ref fromValue, ref kind);
#pragma warning disable 618 // Args are validated
			return new RangeBoundaryFrom<T>(fromValue, kind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>Exclusive boundary From factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromValue">From value.</param>
		/// <returns>
		/// New exclusive boundary From, or the negative infinity boundary if the <paramref name="fromValue"/> is <c>null</c>.
		/// </returns>
		public static RangeBoundaryFrom<T> BoundaryFromExclusive<T>(T fromValue)
		{
			var kind = RangeBoundaryFromKind.Exclusive;
			RangeBoundaryFrom<T>.CoerceBoundaryValue(ref fromValue, ref kind);
#pragma warning disable 618 // Args are validated
			return new RangeBoundaryFrom<T>(fromValue, kind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>Negative infinity boundary (-∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The negative infinity boundary (-∞).</returns>
		public static RangeBoundaryFrom<T> BoundaryFromInfinity<T>() =>
			RangeBoundaryFrom<T>.NegativeInfinity;

		/// <summary>Inclusive boundary To factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New inclusive boundary To, or the positive infinity boundary if the <paramref name="toValue"/> is <c>null</c>.
		/// </returns>
		public static RangeBoundaryTo<T> BoundaryTo<T>(T toValue)
		{
			var kind = RangeBoundaryToKind.Inclusive;
			RangeBoundaryTo<T>.CoerceBoundaryValue(ref toValue, ref kind);
#pragma warning disable 618 // Args are validated
			return new RangeBoundaryTo<T>(toValue, kind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>Exclusive boundary To factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toValue">To value.</param>
		/// <returns>
		/// New exclusive boundary To, or the positive infinity boundary if the <paramref name="toValue"/> is <c>null</c>.
		/// </returns>
		public static RangeBoundaryTo<T> BoundaryToExclusive<T>(T toValue)
		{
			var kind = RangeBoundaryToKind.Exclusive;
			RangeBoundaryTo<T>.CoerceBoundaryValue(ref toValue, ref kind);
#pragma warning disable 618 // Args are validated
			return new RangeBoundaryTo<T>(toValue, kind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>Positive infinity boundary (+∞) factory method.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <returns>The positive infinity boundary (-∞).</returns>
		public static RangeBoundaryTo<T> BoundaryToInfinity<T>() =>
			RangeBoundaryTo<T>.PositiveInfinity;
		#endregion

		#region Boundary operations
		/// <summary>Returns the less one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The less one of the two boundaries.</returns>
		public static RangeBoundaryFrom<T> Min<T>(RangeBoundaryFrom<T> boundary1, RangeBoundaryFrom<T> boundary2) =>
			boundary1 <= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the less one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The less one of the two boundaries.</returns>
		public static RangeBoundaryTo<T> Min<T>(RangeBoundaryTo<T> boundary1, RangeBoundaryTo<T> boundary2) =>
			boundary1 <= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the greater one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The greater one of the two boundaries.</returns>
		public static RangeBoundaryFrom<T> Max<T>(RangeBoundaryFrom<T> boundary1, RangeBoundaryFrom<T> boundary2) =>
			boundary1 >= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the greater one of the two boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <returns>The greater one of the two boundaries.</returns>
		public static RangeBoundaryTo<T> Max<T>(RangeBoundaryTo<T> boundary1, RangeBoundaryTo<T> boundary2) =>
			boundary1 >= boundary2 ? boundary1 : boundary2;

		/// <summary>Returns the less one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The less one of the two From boundaries.</returns>
		public static RangeBoundaryFrom<T> MinFrom<T>(T value1, T value2) =>
			Min(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the greater one of the two From boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two From boundaries.</returns>
		public static RangeBoundaryFrom<T> MaxFrom<T>(T value1, T value2) =>
			Max(BoundaryFrom(value1), BoundaryFrom(value2));

		/// <summary>Returns the less one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The less one of the two To boundaries.</returns>
		public static RangeBoundaryTo<T> MinTo<T>(T value1, T value2) =>
			Min(BoundaryTo(value1), BoundaryTo(value2));

		/// <summary>Returns the greater one of the two To boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="value1">The value of the boundary1.</param>
		/// <param name="value2">The value of the boundary2.</param>
		/// <returns>The greater one of the two To boundaries.</returns>
		public static RangeBoundaryTo<T> MaxTo<T>(T value1, T value2) =>
			Max(BoundaryTo(value1), BoundaryTo(value2));
		#endregion

		#region Range validation
		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">The value of the boundary From.</param>
		/// <param name="to">The value of the boundary To.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		public static bool IsValid<T>(T from, T to) =>
			RangeBoundaryFrom<T>.IsValid(from) &&
				RangeBoundaryTo<T>.IsValid(to) &&
				BoundaryFrom(from) <= BoundaryTo(to);

		/// <summary>Returns true if the boundaries can be used for valid range creation.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns><c>true</c>, if the boundaries can be used for valid range creation.</returns>
		public static bool IsValid<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			(from.IsEmpty && to.IsEmpty) || from <= to;
		#endregion

		#region Range factory methods
		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns></returns>
		public static Range<T> Create<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new Range<T>(from, to);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range.</returns>
		public static Range<T> Create<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFrom(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusive<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFromExclusive(fromValue), BoundaryToExclusive(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusiveFrom<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFromExclusive(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range.</returns>
		public static Range<T> CreateExclusiveTo<T>(T fromValue, T toValue) =>
			new Range<T>(BoundaryFrom(fromValue), BoundaryToExclusive(toValue));
		#endregion

		#region Failsafe Range factory methods
		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreate<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			IsValid(from, to)
#pragma warning disable 618 // Args are validated
				? new Range<T>(from, to, SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreate<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(BoundaryFrom(fromValue), BoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusive<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(BoundaryFromExclusive(fromValue), BoundaryToExclusive(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusiveFrom<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(BoundaryFromExclusive(fromValue), BoundaryTo(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;

		/// <summary>Tries to create the range. Returns <seealso cref="Range{T}.Empty"/> if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range or the <seealso cref="Range{T}.Empty"/> if the boundaries forms invalid range.</returns>
		public static Range<T> TryCreateExclusiveTo<T>(T fromValue, T toValue) =>
			IsValid(fromValue, toValue)
#pragma warning disable 618 // Args are validated
				? new Range<T>(BoundaryFrom(fromValue), BoundaryToExclusive(toValue), SkipsArgValidation)
#pragma warning restore 618
				: Range<T>.Empty;
		#endregion
	}
}