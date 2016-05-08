using System;

namespace CodeJam.RangesV2
{
	/// <summary>Extension methods for <seealso cref="Range{T}"/>.</summary>
	public static partial class RangeExtensions
	{
		#region Updating a range
		/// <summary>Replaces exclusive boundaries with inclusive ones with the values from the selector callbacks</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is exclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is exclusive.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		public static Range<T> MakeInclusive<T>(
			this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
				MakeInclusiveCore(range, fromValueSelector, toValueSelector);

		/// <summary>Replaces inclusive boundaries with exclusive ones with the values from the selector callbacks</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is inclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is inclusive.</param>
		/// <returns>A range with exclusive boundaries.</returns>
		public static Range<T> MakeExclusive<T>(
			this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
				MakeExclusiveCore(range, fromValueSelector, toValueSelector);

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="valueSelector">Callback to obtain a new value for the boundaries. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		public static Range<T> WithValues<T>(this Range<T> range, Func<T, T> valueSelector) =>
			WithValuesCore(range, valueSelector, valueSelector);

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		public static Range<T> WithValues<T>(this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
			WithValuesCore(range, fromValueSelector, toValueSelector);

		/// <summary>Creates a new range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new range with the key specified.</returns>
		public static Range<T, TKey2> WithKey<T, TKey2>(this Range<T> range, TKey2 key) =>
			Range.Create(range.From, range.To, key);
		#endregion

		#region Contains & HasIntersection
		/// <summary>Determines whether the range contains the specified value.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range contains the value.</returns>
		public static bool Contains<T>(this Range<T> range, T value) =>
			RangeBoundaryFrom<T>.IsValid(value)
				? Contains(range, Range.BoundaryFrom(value))
				: Contains(range, Range.BoundaryTo(value));

		/// <summary>Determines whether the range contains the specified range boundary.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range contains the boundary.</returns>
		public static bool Contains<T>(this Range<T> range, RangeBoundaryFrom<T> other)
		{
			if (range.IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && range.From <= other && range.To >= other;
		}

		/// <summary>Determines whether the range contains the specified range boundary.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range contains the boundary.</returns>
		public static bool Contains<T>(this Range<T> range, RangeBoundaryTo<T> other)
		{
			if (range.IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && range.From <= other && range.To >= other;
		}

		/// <summary>Determines whether the range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the range contains another range.</returns>
		public static bool Contains<T>(this Range<T> range, T from, T to) =>
			Contains(range, Range.Create(from, to));

		/// <summary>Determines whether the range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TRange">The type of the range.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range contains another range.</returns>
		// DONTTOUCH: The last parameter should be nongeneric to avoid overload resolution conflicts
		// WAITINGFOR: https://github.com/dotnet/roslyn/issues/250 (case 2)
		public static bool Contains<T, TRange>(this TRange range, Range<T> other)
			where TRange : IRange<T>
		{
			if (range.IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && range.From <= other.From && range.To >= other.To;
		}

		/// <summary>Determines whether the range has intersection with another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the range has intersection with another range.</returns>
		public static bool HasIntersection<T>(this Range<T> range, T from, T to) =>
			HasIntersection(range, Range.Create(from, to));

		/// <summary>Determines whether the range has intersection with another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TRange">The type of another range.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range has intersection with another range.</returns>
		public static bool HasIntersection<T, TRange>(this Range<T> range, TRange other)
			where TRange : IRange<T>
		{
			if (range.IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && range.From <= other.To && range.To >= other.From;
		}
		#endregion

		#region Adjust
		/// <summary>Adjusts the specified value so that it fits into a range specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range the value will be fitted to.</param>
		/// <param name="value">The value to be adjusted.</param>
		/// <exception cref="ArgumentException">The range is empty or any of its boundaries is exclusive.</exception>
		/// <returns>A new value that fits into a range specified</returns>
		public static T Adjust<T>(this Range<T> range, T value)
		{
			Code.AssertArgument(
				range.IsNotEmpty, nameof(range), "Cannot fit the value into empty range.");
			Code.AssertArgument(
				!range.From.IsExclusiveBoundary, nameof(range), "The boundary From is exclusive and has no value.");
			Code.AssertArgument(
				!range.To.IsExclusiveBoundary, nameof(range), "The boundary To is exclusive and has no value.");

			// case for the positive infinity
			if (!RangeBoundaryFrom<T>.IsValid(value))
			{
				if (range.To < RangeBoundaryTo<T>.PositiveInfinity)
					return range.To.Value;
				return value;
			}

			if (range.From > value)
				return range.From.Value;

			if (range.To < value)
				return range.To.Value;

			return value;
		}
		#endregion

		#region StartsAfter & EndsBefore
		/// <summary>Determines whether the range starts after the value specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range starts after the value.</returns>
		public static bool StartsAfter<T>(this Range<T> range, T value) =>
			RangeBoundaryFrom<T>.IsValid(value) && range.From > Range.BoundaryFrom(value);

		/// <summary>Determines whether the range starts after the boundary specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range starts after the boundary.</returns>
		public static bool StartsAfter<T>(this Range<T> range, RangeBoundaryFrom<T> other) =>
			other.IsNotEmpty && range.From > other;

		/// <summary>Determines whether the range starts after the boundary specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range starts after the boundary.</returns>
		public static bool StartsAfter<T>(this Range<T> range, RangeBoundaryTo<T> other) =>
			other.IsNotEmpty && range.From > other;

		/// <summary>Determines whether the range starts after the range specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TRange">The type of the range.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range starts after another range.</returns>
		// DONTTOUCH: The last parameter should be nongeneric to avoid overload resolution conflicts
		// WAITINGFOR: https://github.com/dotnet/roslyn/issues/250 (case 2)
		public static bool StartsAfter<T, TRange>(this TRange range, Range<T> other)
			where TRange : IRange<T> =>
				other.IsNotEmpty && range.From > other.To;

		/// <summary>Determines whether the range ends before the value specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range ends before the value.</returns>
		public static bool EndsBefore<T>(this Range<T> range, T value) =>
			range.IsNotEmpty && RangeBoundaryTo<T>.IsValid(value) && range.To < Range.BoundaryTo(value);

		/// <summary>Determines whether the range ends before the boundary specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range ends before the boundary.</returns>
		public static bool EndsBefore<T>(this Range<T> range, RangeBoundaryFrom<T> other) =>
			range.IsNotEmpty && other.IsNotEmpty && range.To < other;

		/// <summary>Determines whether the range ends before the boundary specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range ends before the boundary.</returns>
		public static bool EndsBefore<T>(this Range<T> range, RangeBoundaryTo<T> other) =>
			range.IsNotEmpty && other.IsNotEmpty && range.To < other;

		/// <summary>Determines whether the range ends before the range specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TRange">The type of the range.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range ends before another range.</returns>
		// DONTTOUCH: The last parameter should be nongeneric to avoid overload resolution conflicts
		// WAITINGFOR: https://github.com/dotnet/roslyn/issues/250 (case 2)
		public static bool EndsBefore<T, TRange>(this TRange range, Range<T> other)
			where TRange : IRange<T> =>
				range.IsNotEmpty && other.IsNotEmpty && range.To < other.From;
		#endregion
	}
}