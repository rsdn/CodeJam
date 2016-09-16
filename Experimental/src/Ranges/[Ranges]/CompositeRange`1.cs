using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

// The file contains members to be shared between CompositeRange<T> and CompositeRange<T, TKey>.

namespace CodeJam.Ranges
{
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[PublicAPI]
	public partial struct CompositeRange<T> : IEquatable<CompositeRange<T>>, IFormattable
	{
		#region Nested types
		/// <summary>
		/// Helper comparer. If <see cref="IRange{T}.From"/> are equal,
		/// shortest range (the one with smaller <see cref="IRange{T}.To"/>) goes first.
		/// </summary>
		[Serializable]
		private sealed class SubRangesComparer : IComparer<Range<T>>
		{
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />
			/// </returns>
			public int Compare(Range<T> x, Range<T> y)
			{
				// DONTTOUCH: DO NOT change this behavior as it may break some CompositeRange operations.
				var result = x.From.CompareTo(y.From);
				if (result == 0)
					return x.To.CompareTo(y.To);

				return result;
			}
		}
		#endregion

		#region Static members

		#region Operators
		/// <summary>Implements the operator ==.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are equal.</returns>
		public static bool operator ==(CompositeRange<T> range1, CompositeRange<T> range2) =>
			range1.Equals(range2);

		/// <summary>Implements the operator !=.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are not equal.</returns>
		public static bool operator !=(CompositeRange<T> range1, CompositeRange<T> range2) =>
			!range1.Equals(range2);
		#endregion

		#region Comparers
		private static readonly SubRangesComparer _rangeComparer = new SubRangesComparer();

		/// <summary>Helper comparer for operations over <see cref="IRange{T}.To"/>.</summary>
		internal static readonly RangeBoundaryToDescendingComparer<T> BoundaryToDescendingComparer =
			new RangeBoundaryToDescendingComparer<T>();
		#endregion

		#region Helpers
		private static bool IsContinuationFor(RangeBoundaryTo<T> lastBoundary, Range<T> nextRange)
		{
			DebugCode.BugIf(lastBoundary.IsEmpty || nextRange.IsEmpty, "No empty ranges expected.");

			return lastBoundary.IsPositiveInfinity ||
				lastBoundary.GetComplementation() >= nextRange.From;
		}
		#endregion

		#region Predefined values
		private static readonly ReadOnlyCollection<Range<T>> _emptyRanges = Array<Range<T>>.Empty.AsReadOnly();

		#region T4-dont-replace
		private static readonly Range<T> _emptyRangeNoKey = Range<T>.Empty;
		#endregion

		#endregion

		#endregion

		#region Fields & .ctor()
		// TODO: REMOVE readonly modifier. Same reason as for Range<T>. Proof: NestedStructAccessPerfTests.
		private readonly ReadOnlyCollection<Range<T>> _ranges; // TODO: own collection?
		private readonly bool _hasRangesToMerge;

		#region T4-dont-replace
		private readonly Range<T> _containingRange;
		#endregion

		/// <summary>Creates instance of <seealso cref="CompositeRange{T}"/>.</summary>
		/// <param name="ranges">Contained ranges.</param>
		public CompositeRange([NotNull] IEnumerable<Range<T>> ranges)
			: this(ranges, UnsafeOverload.FullValidation) { }

		/// <summary>Creates instance of <seealso cref="CompositeRange{T}"/>.</summary>
		/// <param name="range">Contained range.</param>
		public CompositeRange(Range<T> range)
		{
			_ranges = range.IsEmpty ? _emptyRanges : new[] { range }.AsReadOnly();
			_hasRangesToMerge = false;
			_containingRange = Range.Create(range.From, range.To);
		}

		/// <summary>Creates instance of <seealso cref="CompositeRange{T}"/>.</summary>
		/// <param name="ranges">Contained ranges.</param>
		/// <param name="skipsArgHandling">Stub argument to mark unsafe (no validation) constructor overload.</param>
		internal CompositeRange([NotNull] IEnumerable<Range<T>> ranges, UnsafeOverload skipsArgHandling)
		{
			Code.NotNull(ranges, nameof(ranges));

#pragma warning disable 618 // ok by design.
			bool rangesReady = skipsArgHandling == UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged;
			var tempRanges = rangesReady || skipsArgHandling == UnsafeOverload.NoEmptyRanges
				? ranges.ToArray()
				: ranges.Where(r => r.IsNotEmpty).ToArray();
#pragma warning restore 618

			if (tempRanges.Length == 0)
			{
				_ranges = _emptyRanges;
				_hasRangesToMerge = false;
				_containingRange = _emptyRangeNoKey;
				return;
			}

			if (tempRanges.Length == 1 || rangesReady)
			{
				_ranges = tempRanges.AsReadOnly();
				_hasRangesToMerge = false;
				_containingRange = Range.Create(tempRanges[0].From, tempRanges[tempRanges.Length - 1].To);
				return;
			}

#pragma warning disable 618 // ok by design.
			if (skipsArgHandling != UnsafeOverload.RangesAlreadySorted)
#pragma warning restore 618
			{
				Array.Sort(tempRanges, _rangeComparer);
			}

			bool hasRangesToMerge = false;
			var maxToBoundary = tempRanges[0].To;
			for (int i = 1; i < tempRanges.Length; i++)
			{
				var range = tempRanges[i];

				// TODO: check for keyed range.
				hasRangesToMerge = hasRangesToMerge || IsContinuationFor(maxToBoundary, range);

				maxToBoundary = Range.Max(maxToBoundary, range.To);
			}

			_ranges = tempRanges.AsReadOnly();
			_hasRangesToMerge = hasRangesToMerge;
			_containingRange = Range.Create(tempRanges[0].From, maxToBoundary);
		}
		#endregion

		#region Properties
		/// <summary>The composite range cannot be simplified anymore. Subranges do not intersect and start one exactly after another.</summary>
		/// <value><c>true</c> if all subranges are merged already; otherwise, <c>false</c>.</value>
		public bool IsMerged => !_hasRangesToMerge;

		/// <summary>The composite range is empty, ∅.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => _containingRange.IsEmpty;

		/// <summary>The composite range is NOT empty, ≠ ∅</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => _containingRange.IsNotEmpty;

		/// <summary>Collection of subranges.</summary>
		/// <value>The collection of subranges.</value>
		[NotNull]
		public IReadOnlyList<Range<T>> SubRanges => _ranges ?? _emptyRanges;

		#region T4-dont-replace
		/// <summary>Range that contains all subranges.</summary>
		/// <value>The containing range.</value>
		public Range<T> ContainingRange => _containingRange;
		#endregion

		#endregion

		#region ToString
		/// <summary>Returns string representation of the range.</summary>
		/// <returns>The string representation of the range.</returns>
		[Pure]
		public override string ToString()
		{
			if (IsEmpty)
				return ContainingRange.ToString();

			var containingRangePart = ContainingRange.ToString();
			var subRangesPart = string.Join(
				SeparatorString,
				SubRanges.Select(item => item.ToString()));

			return containingRangePart +
				PrefixString +
				subRangesPart +
				SuffixString;
		}

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <returns>The string representation of the range.</returns>
		[Pure, NotNull]
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		[Pure]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (IsEmpty)
				return ContainingRange.ToString(format, formatProvider);

			var containingRangePart = ContainingRange.ToString(format, formatProvider);
			var subRangesPart = string.Join(
				SeparatorString,
				SubRanges.Select(item => item.ToString(format, formatProvider)));

			return containingRangePart +
				PrefixString +
				subRangesPart +
				SuffixString;
		}
		#endregion
	}
}