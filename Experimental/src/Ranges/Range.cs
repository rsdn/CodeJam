using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>
	/// A set of initialization methods for instances of <see cref="Range{TValue}"/>.
	/// </summary>
	[PublicAPI]
	public static class Range
	{
		/// <summary>
		/// Returns empty range.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <returns>Predefined Empty range.</returns>
		[Pure]
		public static Range<TValue> Empty<TValue>() where TValue : IComparable<TValue> => Range<TValue>.Empty;

		/// <summary>
		/// Returns full range.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <returns>Predefined Full range.</returns>
		[Pure]
		public static Range<TValue> Full<TValue>() where TValue : IComparable<TValue> => Range<TValue>.Full;

		/// <summary>
		/// Creates new instance of finite range<see cref="Range{TValue}"/>.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="start">Lower bound value of the range</param>
		/// <param name="end">Upper bound value of the range</param>
		/// <param name="includeStart">Indicates whenever Lower bound should be inclusive in the range.</param>
		/// <param name="includeEnd">Indicates whenever Upper bound should be inclusive in the range.</param>
		/// <returns>A new range.</returns>
		[DebuggerStepThrough]
		[Pure]
		public static Range<TValue> Create<TValue>(TValue start, TValue end, bool includeStart, bool includeEnd)
			where TValue : IComparable<TValue> =>
				new Range<TValue>(
					start,
					end,
					RangeOptions.HasStart | RangeOptions.HasEnd
						| (includeStart ? RangeOptions.IncludingStart : RangeOptions.None)
						| (includeEnd ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Creates new instance of finite range<see cref="Range{TValue}"/>.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="start">Lower bound value of the range.</param>
		/// <param name="end">Upper bound value of the range.</param>
		/// <param name="include">Indicates whenever Lower and Upper bounds should be inclusive in the range.</param>
		/// <returns>A new range.</returns>
		[DebuggerStepThrough]
		[Pure]
		public static Range<TValue> Create<TValue>(TValue start, TValue end, bool include = true)
			where TValue : IComparable<TValue> =>
				Create(start, end, include, include);

		/// <summary>
		/// Creates new instance of infinite range<see cref="Range{TValue}"/>.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="start">Lower bound value of the range.</param>
		/// <param name="include">Indicates whenever Lower bound should be inclusive in the range.</param>
		/// <returns>A new range.</returns>
		[DebuggerStepThrough]
		[Pure]
		public static Range<TValue> StartsWith<TValue>(TValue start, bool include = true)
			where TValue : IComparable<TValue> =>
				new Range<TValue>(
					start,
					default(TValue),
					RangeOptions.HasStart | (include ? RangeOptions.IncludingStart : RangeOptions.None));

		[DebuggerStepThrough]
		[Pure]
		public static Range<TValue> IsSimple<TValue>(TValue value)
			where TValue : IComparable<TValue> => Create(value, value, true, true);

		[DebuggerStepThrough]
		[Pure]
		public static Range<TValue> EndsWith<TValue>(TValue end, bool include = true)
			where TValue : IComparable<TValue> =>
				new Range<TValue>(
					default(TValue),
					end,
					RangeOptions.HasEnd | (include ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Creates new instance of <see cref="Range{TValue}"/> with modified <see cref="Range{TValue}.Start"/> value.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="newStart">New value for <see cref="Range{TValue}.Start"/>.</param>
		/// <param name="include">
		/// Indicates that new <see cref="Range{TValue}.Start"/> value should be included or excluded in result range.
		/// Leave it null for deriving that parameter from original range.
		/// </param>
		/// <returns>A new range.</returns>
		[Pure]
		public static Range<TValue> ChangeStart<TValue>(this Range<TValue> range, TValue newStart, bool? include = null)
			where TValue : IComparable<TValue> =>
			new Range<TValue>(
				newStart,
				range.End,
				((include ?? range.IncludeStart) ? RangeOptions.IncludingStart : RangeOptions.None)
					| (range.IncludeEnd ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Creates new instance of <see cref="Range{TValue}"/> with modified <see cref="Range{TValue}.End"/> value.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="newEnd">New value for <see cref="Range{TValue}.End"/>.</param>
		/// <param name="include">Indicates that new <see cref="Range{TValue}.End"/> value should be included or excluded in result range. Leave it null for deriving that parameter from original range.</param>
		/// <returns>A new range.</returns>
		[Pure]
		public static Range<TValue> ChangeEnd<TValue>(this Range<TValue> range, TValue newEnd, bool? include = null)
			where TValue : IComparable<TValue> =>
			new Range<TValue>(
				range.Start,
				newEnd,
				(range.IncludeStart ? RangeOptions.IncludingStart : RangeOptions.None)
					| ((include ?? range.IncludeEnd) ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Determines whether value is in the range.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="value">Value to match in the range. The value can be null for reference types.</param>
		/// <returns>
		/// true if <paramref name="value"/> is in range; otherwise, false.
		/// </returns>
		[Pure]
		public static bool Contains<TValue>(this Range<TValue> range, TValue value) where TValue : IComparable<TValue> =>
			Contains(range, value, true);

		/// <summary>
		/// Determines whether value is in the range.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="value">Value to match in the range. The value can be null for reference types.</param>
		/// <param name="included"></param>
		/// <returns>
		/// true if <paramref name="value"/> is in range; otherwise, false.
		/// </returns>
		[Pure]
		private static bool Contains<TValue>(this Range<TValue> range, TValue value, bool included)
			where TValue : IComparable<TValue>
		{
			if (range.IsEmpty)
				return false;

			var result = true;

			if (range.HasStart)
			{
				var compare = range.Start.CompareTo(value);
				result = compare < 0 || compare == 0 && range.IncludeStart && included;
			}

			if (result && range.HasEnd)
			{
				var compare = range.End.CompareTo(value);
				result = compare > 0 || compare == 0 && range.IncludeEnd && included;
			}

			return result;
		}

		/// <summary>
		/// Determines whether <paramref name="other"/> is adjastent to current instance.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="other">range.</param>
		/// <returns>
		/// true if <paramref name="other"/> is adjastent to current instance; otherwise, false.
		/// </returns>
		[Pure]
		public static bool IsAdjastent<TValue>(this Range<TValue> range, Range<TValue> other)
			where TValue : IComparable<TValue>
		{
			if (range.HasEnd && other.HasStart)
				return (range.IncludeEnd || other.IncludeStart) && range.End.CompareTo(other.Start) == 0;

			if (other.HasEnd && range.HasStart)
				return (other.IncludeEnd || range.IncludeStart) && other.End.CompareTo(range.Start) == 0;
			return false;
		}

		/// <summary>
		/// Determines whether current range intersects with <paramref name="other"/>.
		/// </summary>
		/// <param name="range">The range</param>
		/// <param name="other">The range to test. </param><filterpriority>1</filterpriority>
		/// <returns>
		/// This method returns true if there is any intersection, otherwise false.
		/// </returns>
		[Pure]
		public static bool IsIntersectsWith<TValue>(this Range<TValue> range, Range<TValue> other)
			where TValue : IComparable<TValue>
		{
			if (range.IsEmpty || other.IsEmpty)
				return false;

			if (range.IsFull || other.IsFull)
				return true;

			if (!range.HasStart && !other.HasStart)
				return true;

			if (!range.HasEnd && !other.HasEnd)
				return true;

			var result = other.HasStart && range.Contains(other.Start, other.IncludeStart)
				|| other.HasEnd && range.Contains(other.End, other.IncludeEnd)
				|| range.HasStart && other.Contains(range.Start, range.IncludeStart)
				|| range.HasEnd && other.Contains(range.End, range.IncludeEnd);

			return result;
		}

		/// <summary>
		/// Inverts current range
		/// </summary>
		/// <param name="range">The range</param>
		/// <returns>An Enumerator of ranges after inversion</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<Range<TValue>> Invert<TValue>(this Range<TValue> range)
			where TValue : IComparable<TValue>
		{
			if (range.IsEmpty)
				yield return Range<TValue>.Full;
			else
			{
				if (range.IsFull)
					yield return Range<TValue>.Empty;
				else
				{
					if (range.HasStart)
					{
						yield return
							new Range<TValue>(
								default(TValue),
								range.Start,
								RangeOptions.HasEnd | (range.IncludeStart ? RangeOptions.None : RangeOptions.IncludingEnd));
					}

					if (range.HasEnd)
					{
						yield return
							new Range<TValue>(
								range.End,
								default(TValue),
								RangeOptions.HasStart | (range.IncludeEnd ? RangeOptions.None : RangeOptions.IncludingStart));
					}
				}
			}
		}
	}
}