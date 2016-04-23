using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Immutable struct for keeping range parameters
	/// </summary>
	/// <typeparam name="TValue">Type of range value</typeparam>
	[PublicAPI]
	[DebuggerDisplay("{GetDisplayValue()}")]
	public struct Range<TValue> : IComparable<Range<TValue>>
		where TValue : IComparable<TValue>
	{
		/// <summary>
		/// Returns empty range.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <returns>Predefined Empty range.</returns>
		public static readonly Range<TValue> Empty = new Range<TValue>(
			default(TValue), default(TValue),
			RangeOptions.IsEmpty);

		/// <summary>
		/// Returns full range.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <returns>Predefined Full range.</returns>
		public static readonly Range<TValue> Full = new Range<TValue>(
			default(TValue), default(TValue),
			RangeOptions.None);

		private readonly RangeOptions _options;

		/// <summary>
		/// End value. Valid only if <see cref="HasEnd"/> is true.>
		/// </summary>
		public readonly TValue End;

		/// <summary>
		/// Start value. Valid only if <see cref="HasStart"/> is true.>
		/// </summary>
		public readonly TValue Start;

		/// <summary>
		/// Range constructor
		/// </summary>
		[DebuggerStepThrough]
		public Range(TValue start, TValue end, RangeOptions options)
		{
			if (options.HasFlag(RangeOptions.IsEmpty))
			{
				// clear other flags
				_options = RangeOptions.IsEmpty;
				Start = default(TValue);
				End = default(TValue);
				return;
			}

			if (ReferenceEquals(start, null))
				options &= ~RangeOptions.HasStart;

			if (ReferenceEquals(end, null))
				options &= ~RangeOptions.HasEnd;

			if (!options.HasFlag(RangeOptions.HasStart))
			{
				start = default(TValue);
				options &= ~RangeOptions.IncludingStart;
			}

			if (!options.HasFlag(RangeOptions.HasEnd))
			{
				end = default(TValue);
				options &= ~RangeOptions.IncludingEnd;
			}

			_options = options;

			if (!options.HasFlag(RangeOptions.HasStart) || !options.HasFlag(RangeOptions.HasEnd))
			{
				Start = start;
				End = end;
				return;
			}

			Debug.Assert(start != null, "start != null");
			var compare = start.CompareTo(end);
			if (compare > 0)
			{
				throw new ArgumentException("'Start' must be less or equal to the 'End' parameter");
			}

			Start = start;
			End = end;

			if (compare == 0)
			{
				if (!(options.HasFlag(RangeOptions.IncludingStart)
					|| options.HasFlag(RangeOptions.IncludingEnd)))
				{
					// empty
					Start = default(TValue);
					End = default(TValue);
					_options = RangeOptions.IsEmpty;
				}
				else
				{
					// ensure that everything are included
					_options |= RangeOptions.IncludingStart | RangeOptions.IncludingEnd;
				}
			}
		}

		private Range(RangeValue start, RangeValue end)
			: this(start, end, start.Included, end.Included)
		{
		}

		private Range(RangeValue start, RangeValue end, bool includeStart, bool includeEnd)
			: this(start.Value, end.Value,
				(start.HasValue ? RangeOptions.HasStart : RangeOptions.None)
					| (end.HasValue ? RangeOptions.HasEnd : RangeOptions.None)
					| (includeStart ? RangeOptions.IncludingStart : RangeOptions.None)
					| (includeEnd ? RangeOptions.IncludingEnd : RangeOptions.None)
				)
		{}

		/// <summary>
		/// Indicates that range has Start.
		/// </summary>
		public bool HasStart => _options.HasFlag(RangeOptions.HasStart);

		/// <summary>
		/// Indicates that range has End.
		/// </summary>
		public bool HasEnd => _options.HasFlag(RangeOptions.HasEnd);

		/// <summary>
		/// Indicates that <see cref="Start"/> value is included in range.
		/// </summary>
		public bool IncludeStart => _options.HasFlag(RangeOptions.IncludingStart);

		/// <summary>
		/// Indicates that <see cref="End"/> value is included in range.
		/// </summary>
		public bool IncludeEnd => _options.HasFlag(RangeOptions.IncludingEnd);

		/// <summary>
		/// Indicates that range is empty.
		/// </summary>
		public bool IsEmpty => _options.HasFlag(RangeOptions.IsEmpty);

		/// <summary>
		/// Indicates that range covers whole dispason.
		/// </summary>
		public bool IsFull => _options == RangeOptions.None;

		private RangeValue StartValue =>
			new RangeValue(Start, !HasStart ? ValueInfo.MinValue : IncludeStart ? ValueInfo.Included : ValueInfo.Excluded);

		private RangeValue EndValue =>
			new RangeValue(End, !HasEnd ? ValueInfo.MaxValue : IncludeEnd ? ValueInfo.Included : ValueInfo.Excluded);

		/// <summary>
		/// Compares this instance with a specified <see cref="Range{TValue}"/> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="Range{TValue}"/>.
		/// </summary>
		/// 
		/// <returns>
		/// A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="other"/> parameter.Value Condition Less than zero This instance precedes <paramref name="other"/>. Zero This instance has the same position in the sort order as <paramref name="other"/>. Greater than zero This instance follows <paramref name="other"/>.
		/// </returns>
		/// <param name="other">An object that evaluates to a <see cref="Range{TValue}"/>.</param>
		public int CompareTo(Range<TValue> other)
		{
			var current = this;
			if (!current.HasStart)
			{
				if (!other.HasStart)
					return 0;
				else
					return -1;
			}

			if (!other.HasStart)
				return 1;

			var compare = current.Start.CompareTo(other.Start);
			if (compare == 0)
			{
				if (current.IncludeStart != other.IncludeStart)
				{
					if (current.IncludeStart)
						compare = -1;
					else
						compare = 1;
				}
				else
				{
					if (current.HasEnd && other.HasEnd)
					{
						compare = current.End.CompareTo(other.End);
						if (compare == 0)
						{
							if (current.IncludeEnd != other.IncludeEnd)
							{
								if (current.IncludeEnd)
									compare = 1;
								else
									compare = -1;
							}
						}
					}
				}
			}

			return compare;
		}

		/// <summary>
		/// Returns a value indicating whether this instance and a specified <see cref="Range{TValue}"/> object represent the same value.
		/// </summary>
		/// 
		/// <returns>
		/// true if <paramref name="other"/> is equal to this instance; otherwise, false.
		/// </returns>
		/// <param name="other">A <see cref="Range{TValue}"/> object to compare to this instance.</param><filterpriority>2</filterpriority>
		public bool Equals(Range<TValue> other) =>
			_options == other._options
				&& EqualityComparer<TValue>.Default.Equals(End, other.End)
				&& EqualityComparer<TValue>.Default.Equals(Start, other.Start);

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// 
		/// <returns>
		/// true if <paramref name="obj"/> is an instance of <see cref="Range{TValue}"/> and equals the value of this instance; otherwise, false.
		/// </returns>
		/// <param name="obj">An object to compare with this instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Range<TValue> && Equals((Range<TValue>)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// 
		/// <returns>
		/// A 32-bit signed integer hash code.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)_options;
				hashCode = (hashCode * 397) ^ EqualityComparer<TValue>.Default.GetHashCode(End);
				hashCode = (hashCode * 397) ^ EqualityComparer<TValue>.Default.GetHashCode(Start);
				return hashCode;
			}
		}

		/// <summary>
		/// Creates new instance of <see cref="Range{TValue}"/> with modified <see cref="Start"/> value.
		/// </summary>
		/// <param name="newStart">New value for <see cref="Start"/>.</param>
		/// <param name="include">Indicates that new <see cref="Start"/> value should be included or excluded in result range. Leave it null for deriving that parameter from original range.</param>
		/// <returns>A new range.</returns>
		[Pure]
		public Range<TValue> ChangeStart(TValue newStart, bool? include = null) =>
			new Range<TValue>(
				newStart,
				End,
				((include ?? IncludeStart) ? RangeOptions.IncludingStart : RangeOptions.None)
					| (IncludeEnd ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Creates new instance of <see cref="Range{TValue}"/> with modified <see cref="End"/> value.
		/// </summary>
		/// <param name="newEnd">New value for <see cref="End"/>.</param>
		/// <param name="include">Indicates that new <see cref="End"/> value should be included or excluded in result range. Leave it null for deriving that parameter from original range.</param>
		/// <returns>A new range.</returns>
		[Pure]
		public Range<TValue> ChangeEnd(TValue newEnd, bool? include = null) =>
			new Range<TValue>(
				Start,
				newEnd,
				(IncludeStart ? RangeOptions.IncludingStart : RangeOptions.None)
					| ((include ?? IncludeEnd) ? RangeOptions.IncludingEnd : RangeOptions.None));

		/// <summary>
		/// Gets a <see cref="Range{TValue}"/> structure that contains the union of two <see cref="Range{TValue}"/> structures.
		/// </summary>
		/// <param name="other">A rectangle to union.</param><filterpriority>1</filterpriority>
		/// <returns>
		/// A <see cref="Range{TValue}"/> structure that bounds the union of the two <see cref="Range{TValue}"/> structures.
		/// </returns>
		[Pure]
		public Range<TValue> Union(Range<TValue> other)
		{
			var current = this;

			if (current.IsEmpty)
				return other;

			if (other.IsEmpty)
				return this;

			var currentStart = current.StartValue;
			var currentEnd = current.EndValue;

			var otherStart = other.StartValue;
			var otherEnd = other.EndValue;

			var overlap = currentStart <= otherEnd && otherStart <= currentEnd;

			if (!overlap)
				return Empty;

			RangeValue newStart;
			RangeValue newEnd;

			var compareStart = currentStart.CompareTo(otherStart);
			if (compareStart < 0)
				newStart = currentStart;
			else if (compareStart > 0)
				newStart = otherStart;
			else
				newStart = new RangeValue(
					currentStart.Value,
					!currentStart.HasValue
						? currentStart.ValueInfo
						: (currentStart.Included || otherStart.Included ? ValueInfo.Included : ValueInfo.Excluded));

			var compareEnd = currentEnd.CompareTo(otherEnd);
			if (compareEnd > 0)
				newEnd = currentEnd;
			else if (compareEnd < 0)
				newEnd = otherEnd;
			else
				newEnd = new RangeValue(
					currentEnd.Value,
					!currentEnd.HasValue
						? currentEnd.ValueInfo
						: (currentEnd.Included || otherEnd.Included ? ValueInfo.Included : ValueInfo.Excluded));

			return new Range<TValue>(newStart, newEnd);
		}

		/// <summary>
		/// Determines whether value is in the range.
		/// </summary>
		/// <param name="value">Value to match in the range. The value can be null for reference types.</param>
		/// <returns>
		/// true if <paramref name="value"/> is in range; otherwise, false.
		/// </returns>
		[Pure]
		public bool Contains(TValue value) => Contains(value, true);

		private bool Contains(TValue value, bool included)
		{
			if (IsEmpty)
				return false;

			var result = true;

			if (HasStart)
			{
				var compare = Start.CompareTo(value);
				result = compare < 0 || compare == 0 && IncludeStart && included;
			}

			if (result && HasEnd)
			{
				var compare = End.CompareTo(value);
				result = compare > 0 || compare == 0 && IncludeEnd && included;
			}

			return result;
		}

		/// <summary>
		/// Determines whether <paramref name="other"/> is adjastent to current instance.
		/// </summary>
		/// <param name="other">range.</param>
		/// <returns>
		/// true if <paramref name="other"/> is adjastent to current instance; otherwise, false.
		/// </returns>
		[Pure]
		public bool IsAdjastent(Range<TValue> other)
		{
			var current = this;
			if (current.HasEnd && other.HasStart)
				return (current.IncludeEnd || other.IncludeStart) && current.End.CompareTo(other.Start) == 0;

			if (other.HasEnd && current.HasStart)
				return (other.IncludeEnd || current.IncludeStart) && other.End.CompareTo(current.Start) == 0;
			return false;
		}

		/// <summary>
		/// Determines whether current range intersects with <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The range to test. </param><filterpriority>1</filterpriority>
		/// <returns>
		/// This method returns true if there is any intersection, otherwise false.
		/// </returns>
		[Pure]
		public bool IsIntersectsWith(Range<TValue> other)
		{
			var current = this;

			if (current.IsEmpty || other.IsEmpty)
				return false;

			if (current.IsFull || other.IsFull)
				return true;

			if (!current.HasStart && !other.HasStart)
				return true;

			if (!current.HasEnd && !other.HasEnd)
				return true;

			var result = other.HasStart && current.Contains(other.Start, other.IncludeStart)
				|| other.HasEnd && current.Contains(other.End, other.IncludeEnd)
				|| current.HasStart && other.Contains(current.Start, current.IncludeStart)
				|| current.HasEnd && other.Contains(current.End, current.IncludeEnd);

			return result;
		}

		/// <summary>
		/// Returns enumeration of ranges after excluding <paramref name="other"/> from current range.
		/// </summary>
		/// <param name="other">range for exclude.</param>
		/// <returns>Enumeration of new ranges.</returns>
		[NotNull]
		[Pure]
		public IEnumerable<Range<TValue>> Exclude(Range<TValue> other)
		{
			var current = this;

			if (current.IsEmpty || other.IsFull)
			{
				yield return Empty;
				yield break;
			}

			if (other.IsEmpty)
			{
				yield return this;
				yield break;
			}

			if (current.IsFull)
			{
				foreach (var r in other.Invert())
				{
					yield return r;
				}
				yield break;
			}

			var currentStart = current.StartValue;
			var currentEnd = current.EndValue;

			var otherStart = other.StartValue;
			var otherEnd = other.EndValue;

			var overlap = currentStart <= otherEnd && otherStart <= currentEnd;

			if (!overlap)
				yield return current; // Nothing to exclude

			var compareStart = currentStart.CompareTo(otherStart);
			if (compareStart <= 0) // currentStart <= otherStart
			{
				// =====---C---------      //
				//      --------O--        //
				//      --------O----      //
				//      --------O--------- //

				if (compareStart == 0)
				{
					if (!otherStart.Included && currentStart.Included)
					{
						var singleValueRange = new Range<TValue>(currentStart, currentStart, true, true);
						yield return singleValueRange;
					}
				}
				else
				{
					var newRange = new Range<TValue>(currentStart, otherStart, currentStart.Included, !otherStart.Included);
					yield return newRange;
				}
			}

			var compareEnd = currentEnd.CompareTo(otherEnd);
			if (compareEnd >= 0) // currentEnd >= otherEnd
			{
				//    --------C----=====      //
				// --------O-------           //
				// --------O------------      //

				if (compareEnd == 0)
				{
					if (!otherEnd.Included && currentEnd.Included)
					{
						var singleValueRange = new Range<TValue>(currentEnd, currentEnd, true, true);
						yield return singleValueRange;
					}
				}
				else
				{
					var newRange = new Range<TValue>(otherEnd, currentEnd, !otherEnd.Included, currentEnd.Included);
					yield return newRange;
				}
			}
		}

		/// <summary>
		/// Returns <see cref="Range{TValue}"/> structure that represents the intersection of current intstance and other <see cref="Range{TValue}"/> structures. 
		/// If there is no intersection, an empty <see cref="Range{TValue}"/> is returned.
		/// </summary>
		/// <param name="other">A range to intersect. </param><filterpriority>1</filterpriority>
		/// <returns>
		/// A new <see cref="Range{TValue}"/> that represents the intersection of current instance and <paramref name="other"/>.
		/// </returns>
		[Pure]
		public Range<TValue> Intersect(Range<TValue> other)
		{
			var current = this;

			if (current.IsEmpty || other.IsEmpty)
			{
				return Empty;
			}

			if (other.IsFull)
			{
				return this;
			}

			if (current.IsFull)
			{
				return other;
			}

			var currentStart = current.StartValue;
			var currentEnd = current.EndValue;

			var otherStart = other.StartValue;
			var otherEnd = other.EndValue;

			var overlap = currentStart <= otherEnd && otherStart <= currentEnd;

			if (!overlap)
				return Empty;

			RangeValue newStart;
			RangeValue newEnd;

			var compareStart = currentStart.CompareTo(otherStart);
			if (compareStart > 0)
				newStart = currentStart;
			else if (compareStart < 0)
				newStart = otherStart;
			else
				newStart = new RangeValue(
					currentStart.Value,
					!currentStart.HasValue
						? currentStart.ValueInfo
						: (currentStart.Included && otherStart.Included ? ValueInfo.Included : ValueInfo.Excluded));

			var compareEnd = currentEnd.CompareTo(otherEnd);
			if (compareEnd < 0)
				newEnd = currentEnd;
			else if (compareEnd > 0)
				newEnd = otherEnd;
			else
				newEnd = new RangeValue(
					currentEnd.Value,
					!currentEnd.HasValue
						? currentEnd.ValueInfo
						: (currentEnd.Included && otherEnd.Included ? ValueInfo.Included : ValueInfo.Excluded));

			if (newStart.CompareTo(newEnd) == 0 && (!newStart.Included || !newEnd.Included))
			{
				return Empty;
			}

			return new Range<TValue>(newStart, newEnd);
		}

		/// <summary>
		/// Inverts current range
		/// </summary>
		/// <returns>An Enumerator of ranges after inversion</returns>
		[NotNull]
		[Pure]
		public IEnumerable<Range<TValue>> Invert()
		{
			if (IsEmpty)
				yield return Full;
			else
			{
				if (IsFull)
					yield return Empty;
				else
				{
					if (HasStart)
					{
						yield return
							new Range<TValue>(
								default(TValue), Start,
								RangeOptions.HasEnd | (IncludeStart ? RangeOptions.None : RangeOptions.IncludingEnd));
					}

					if (HasEnd)
					{
						yield return
							new Range<TValue>(
								End, default(TValue),
								RangeOptions.HasStart | (IncludeEnd ? RangeOptions.None : RangeOptions.IncludingStart));
					}
				}
			}
		}

		/// <summary>
		/// Generates human readable string for debugging.
		/// </summary>
		/// <returns>A new string.</returns>
		[Pure]
		public string GetDisplayValue()
		{
			if (IsEmpty)
				return string.Empty;
			if (IsFull)
				return "...";

			if (HasStart && HasEnd && Equals(Start, End))
			{
				var value = StartValue.ToString();
				if (!IncludeStart)
					value = "(" + value;
				if (!IncludeEnd)
					value = value + ")";

				return value;
			}

			var startValue = string.Empty;
			var endValue = string.Empty;

			if (HasStart)
			{
				startValue = Start.ToString();
				if (!IncludeStart)
					startValue = "(" + startValue + ")";
			}

			if (HasEnd)
			{
				endValue = End.ToString();
				if (!IncludeEnd)
					endValue = "(" + endValue + ")";
			}

			return startValue + ".." + endValue;
		}

		private enum ValueInfo
		{
			Excluded,
			Included,
			MinValue,
			MaxValue
		}

		[DebuggerDisplay("{ToString()}")]
		private struct RangeValue : IComparable<RangeValue>
		{
			public readonly TValue Value;

			public ValueInfo ValueInfo { get; }

			public RangeValue(TValue value, ValueInfo valueInfo)
			{
				Value = value;
				ValueInfo = valueInfo;
			}

			private bool IsMin => ValueInfo == ValueInfo.MinValue;
			private bool IsMax => ValueInfo == ValueInfo.MaxValue;
			public bool Included => ValueInfo == ValueInfo.Included;
			public bool HasValue => ValueInfo == ValueInfo.Included || ValueInfo == ValueInfo.Excluded;

			public int CompareTo(RangeValue other)
			{
				var current = this;
				if (current.IsMin)
				{
					if (other.IsMin)
						return 0;
					return -1;
				}

				if (current.IsMax)
				{
					if (other.IsMax)
						return 0;
					return 1;
				}

				if (other.IsMin)
					return 1;

				if (other.IsMax)
					return -1;

				var compare = current.Value.CompareTo(other.Value);
				return compare;
			}

			public override string ToString()
			{
				switch (ValueInfo)
				{
					case ValueInfo.Excluded:
						return $"({Value})";
					case ValueInfo.Included:
						return $"{Value}";
					case ValueInfo.MinValue:
						return "min";
					case ValueInfo.MaxValue:
						return "max";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public static bool operator <(RangeValue value1, RangeValue value2)
			{
				var compare = value1.CompareTo(value2);
				return compare < 0;
			}

			public static bool operator >(RangeValue value1, RangeValue value2)
			{
				var compare = value1.CompareTo(value2);
				return compare > 0;
			}

			public static bool operator <=(RangeValue value1, RangeValue value2)
			{
				var compare = value1.CompareTo(value2);
				return compare <= 0;
			}

			public static bool operator >=(RangeValue value1, RangeValue value2)
			{
				var compare = value1.CompareTo(value2);
				return compare >= 0;
			}
		}
	}
}