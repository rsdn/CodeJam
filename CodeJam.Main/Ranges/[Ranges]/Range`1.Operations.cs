using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Ranges
{
	/// <summary>Describes a range of the values.</summary>
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public partial struct Range<T>
	{
		#region Updating a range
		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="newValueSelector">Callback to obtain a new value for the boundaries. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T2> WithValues<T2>(
			[NotNull, InstantHandle] Func<T, T2> newValueSelector)
		{
			var from = From.WithValue(newValueSelector);
			var to = To.WithValue(newValueSelector);
			return TryCreateRange(from, to);
		}

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T2> WithValues<T2>(
			[NotNull, InstantHandle] Func<T, T2> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T2> toValueSelector)
		{
			var from = From.WithValue(fromValueSelector);
			var to = To.WithValue(toValueSelector);
			return TryCreateRange(from, to);
		}

		/// <summary>Creates a new range with the key specified.</summary>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new range with the key specified.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T, TKey2> WithKey<TKey2>(TKey2 key) =>
			Range.Create(From, To, key);

		/// <summary>
		/// Replaces exclusive boundaries with inclusive ones with the values from the selector callbacks
		/// </summary>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is exclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is exclusive.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		[Pure]
		public Range<T> MakeInclusive(
			[NotNull, InstantHandle] Func<T?, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T?, T> toValueSelector)
		{
			if (IsEmpty || (!From.IsExclusiveBoundary && !To.IsExclusiveBoundary))
			{
				return this;
			}

			var from = From;
			if (from.IsExclusiveBoundary)
				from = Range.BoundaryFrom(fromValueSelector(from.GetValueOrDefault()));
			var to = To;
			if (to.IsExclusiveBoundary)
				to = Range.BoundaryTo(toValueSelector(to.GetValueOrDefault()));

			return TryCreateRange(from, to);
		}

		/// <summary>
		/// Replaces inclusive boundaries with exclusive ones with the values from the selector callbacks
		/// </summary>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is inclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is inclusive.</param>
		/// <returns>A range with exclusive boundaries.</returns>
		[Pure]
		public Range<T> MakeExclusive(
			[NotNull, InstantHandle] Func<T?, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T?, T> toValueSelector)
		{
			if (IsEmpty || (!From.IsInclusiveBoundary && !To.IsInclusiveBoundary))
			{
				return this;
			}

			var from = From;
			if (from.IsInclusiveBoundary)
			{
				from = Range.BoundaryFromExclusive(fromValueSelector(from.GetValueOrDefault()));
			}
			var to = To;
			if (to.IsInclusiveBoundary)
			{
				to = Range.BoundaryToExclusive(toValueSelector(to.GetValueOrDefault()));
			}

			return TryCreateRange(from, to);
		}
		#endregion

		#region Contains & HasIntersection
		/// <summary>Determines whether the range contains the specified value.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range contains the value.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains(T value) =>
			RangeBoundaryFrom<T>.IsValid(value)
				? Contains(Range.BoundaryFrom(value))
				: Contains(Range.BoundaryTo(value));

		/// <summary>Determines whether the range contains the specified range boundary.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range contains the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains(RangeBoundaryTo<T> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other && To >= other;
		}

		/// <summary>Determines whether the range contains the specified range boundary.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range contains the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains(RangeBoundaryFrom<T> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other && To >= other;
		}

		/// <summary>Determines whether the range contains another range.</summary>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the range contains another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains(T from, T to) =>
			Contains(Range.Create(from, to));

		/// <summary>Determines whether the range contains another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range contains another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains(

		#region T4-dont-replace
			Range<T> other
		#endregion

			)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.From && To >= other.To;
		}

		/// <summary>Determines whether the range contains another range.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range contains another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool Contains<TKey2>(Range<T, TKey2> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.From && To >= other.To;
		}

		/// <summary>Determines whether the range has intersection with another range.</summary>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the range has intersection with another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool HasIntersection(T from, T to) =>
			HasIntersection(Range.Create(from, to));

		/// <summary>Determines whether the range has intersection with another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range has intersection with another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool HasIntersection(

		#region T4-dont-replace
			Range<T> other
		#endregion

			)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.To && To >= other.From;
		}

		/// <summary>Determines whether the range has intersection with another range.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range has intersection with another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool HasIntersection<TKey2>(Range<T, TKey2> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.To && To >= other.From;
		}
		#endregion

		#region Adjust
		/// <summary>Ensures that the value fits into a range.</summary>
		/// <param name="value">The value to be adjusted.</param>
		/// <exception cref="ArgumentException">The range is empty or any of its boundaries is exclusive.</exception>
		/// <returns>A new value that fits into a range specified</returns>
		[Pure]
		public T Clamp(T value)
		{
			Code.AssertState(IsNotEmpty, "Cannot fit the value into empty range.");
			Code.AssertState(
				!From.IsExclusiveBoundary, "The clamp range boundary From is exclusive and has no value.");
			Code.AssertState(
				!To.IsExclusiveBoundary, "The clamp range boundary To is exclusive and has no value.");

			// case for the positive infinity
			if (!RangeBoundaryFrom<T>.IsValid(value))
			{
				if (To < RangeBoundaryTo<T>.PositiveInfinity)
					return To.Value;
				return value;
			}

			if (From > value)
				return From.Value;

			if (To < value)
				return To.Value;

			return value;
		}
		#endregion

		#region StartsAfter & EndsBefore
		/// <summary>Determines whether the range starts after the value specified.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range starts after the value.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool StartsAfter(T value) =>
			RangeBoundaryFrom<T>.IsValid(value) && From > Range.BoundaryFrom(value);

		/// <summary>Determines whether the range starts after the boundary specified.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range starts after the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool StartsAfter(RangeBoundaryFrom<T> other) =>
			other.IsNotEmpty && From > other;

		/// <summary>Determines whether the range starts after the boundary specified.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range starts after the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool StartsAfter(RangeBoundaryTo<T> other) =>
			other.IsNotEmpty && From > other;

		/// <summary>Determines whether the range starts after the range specified.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range starts after another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool StartsAfter(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				other.IsNotEmpty && From > other.To;

		/// <summary>Determines whether the range starts after the range specified.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range starts after another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool StartsAfter<TKey2>(Range<T, TKey2> other) =>
			other.IsNotEmpty && From > other.To;

		/// <summary>Determines whether the range ends before the value specified.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the range ends before the value.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool EndsBefore(T value) =>
			IsNotEmpty && RangeBoundaryTo<T>.IsValid(value) && To < Range.BoundaryTo(value);

		/// <summary>Determines whether the range ends before the boundary specified.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range ends before the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool EndsBefore(RangeBoundaryFrom<T> other) =>
			IsNotEmpty && other.IsNotEmpty && To < other;

		/// <summary>Determines whether the range ends before the boundary specified.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the range ends before the boundary.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool EndsBefore(RangeBoundaryTo<T> other) =>
			IsNotEmpty && other.IsNotEmpty && To < other;

		/// <summary>Determines whether the range ends before the range specified.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range ends before another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool EndsBefore(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				IsNotEmpty && other.IsNotEmpty && To < other.From;

		/// <summary>Determines whether the range ends before the range specified.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the range ends before another range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public bool EndsBefore<TKey2>(Range<T, TKey2> other) =>
			IsNotEmpty && other.IsNotEmpty && To < other.From;
		#endregion

		#region Union/Extend
		/// <summary>Returns a union range containing original range and the <paramref name="value"/>.</summary>
		/// <param name="value">The value to be included in range.</param>
		/// <returns>A union range containing both of the ranges.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Union(T value) =>
			Union(Range.Create(value, value));

		/// <summary>Returns a union range containing both of the ranges.</summary>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>A union range containing both of the ranges.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Union(T from, T to) =>
			Union(Range.Create(from, to));

		/// <summary>Returns a union range containing both of the ranges.</summary>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing both of the ranges.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Union(

		#region T4-dont-replace
			Range<T> other
		#endregion

			)
		{
			if (other.IsEmpty)
				return this;

#pragma warning disable 618 // Validation not required: From & To extended.
			if (IsEmpty)
				return CreateUnsafe(other.From, other.To);

			return CreateUnsafe(
				other.From >= From ? From : other.From,
				To >= other.To ? To : other.To);
#pragma warning restore
		}

		/// <summary>Returns a union range containing both of the ranges.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing both of the ranges.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Union<TKey2>(Range<T, TKey2> other)
		{
			if (other.IsEmpty)
				return this;

#pragma warning disable 618 // Validation not required: From & To extended.
			if (IsEmpty)
				return CreateUnsafe(other.From, other.To);

			return CreateUnsafe(
				other.From >= From ? From : other.From,
				To >= other.To ? To : other.To);
#pragma warning restore
		}

		/// <summary>Extends the range from the left.</summary>
		/// <param name="from">A new value From.</param>
		/// <returns>
		/// A range with a new From boundary or the source range if the new boundary is greater than original.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> ExtendFrom(T from) =>
			ExtendFrom(Range.BoundaryFrom(from));

		/// <summary>Extends the range from the left.</summary>
		/// <param name="from">A new boundary From.</param>
		/// <returns>
		/// A range with a new From boundary or the source range if the new boundary is greater than original.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> ExtendFrom(RangeBoundaryFrom<T> from)
		{
			if (IsEmpty || from.IsEmpty)
				return this;

			return From <= from
				? this
				: TryCreateRange(from, To);
		}

		/// <summary>Extends the range from the right.</summary>
		/// <param name="to">A new value To.</param>
		/// <returns>
		/// A range with a new To boundary or the source range if the new boundary is less than original.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> ExtendTo(T to) =>
			ExtendTo(Range.BoundaryTo(to));

		/// <summary>Extends the range from the right.</summary>
		/// <param name="to">A new boundary To.</param>
		/// <returns>
		/// A range with a new To boundary or the source range if the new boundary is less than original.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> ExtendTo(RangeBoundaryTo<T> to)
		{
			if (IsEmpty || to.IsEmpty)
				return this;

			return To >= to
				? this
				: TryCreateRange(From, to);
		}
		#endregion

		#region Intersect/Trim
		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Intersect(T from, T to) =>
			Intersect(Range.Create(from, to));

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Intersect(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				TryCreateRange(
					(IsEmpty || From >= other.From) ? From : other.From,
					To <= other.To ? To : other.To);

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> Intersect<TKey2>(Range<T, TKey2> other) =>
			TryCreateRange(
				(IsEmpty || From >= other.From) ? From : other.From,
				To <= other.To ? To : other.To);

		/// <summary>Trims the range from the left.</summary>
		/// <param name="from">A new value From.</param>
		/// <returns>
		/// A range with a new From boundary
		/// or the source range if the new boundary is less than original
		/// or an empty range if the new From boundary is greater than To boundary of the range.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> TrimFrom(T from) =>
			TrimFrom(Range.BoundaryFrom(from));

		/// <summary>Trims the range from the left.</summary>
		/// <param name="from">A new boundary From.</param>
		/// <returns>
		/// A range with a new From boundary
		/// or the source range if the new boundary is less than original
		/// or an empty range if the new From boundary is greater than To boundary of the range.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> TrimFrom(RangeBoundaryFrom<T> from) =>
			from.IsNotEmpty && From >= from
				? this
				: TryCreateRange(from, To);

		/// <summary>Trims the range from the right.</summary>
		/// <param name="to">A new value To.</param>
		/// <returns>
		/// A range with a new To boundary
		/// or the source range if the new boundary is greater than original
		/// or an empty range if the new To boundary is less than From boundary of the range.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> TrimTo(T to) =>
			TrimTo(Range.BoundaryTo(to));

		/// <summary>Trims the range from the right.</summary>
		/// <param name="to">A new boundary To.</param>
		/// <returns>
		/// A range with a new To boundary
		/// or the source range if the new boundary is greater than original
		/// or an empty range if the new To boundary is less than From boundary of the range.
		/// </returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public Range<T> TrimTo(RangeBoundaryTo<T> to) =>
			to.IsNotEmpty && To <= to
				? this
				: TryCreateRange(From, to);
		#endregion
	}
}