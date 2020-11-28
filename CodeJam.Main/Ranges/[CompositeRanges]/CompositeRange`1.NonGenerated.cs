using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

// The file contains members that should not be copied into CompositeRange<T, TKey>. DO NOT remove it

namespace CodeJam.Ranges
{
	/// <summary>Describes a composite range that contains some subranges.</summary>
	public partial struct CompositeRange<T>
	{
		#region ICompositeRange<T>
		/// <summary>Returns a sequence of merged subranges. Should be used for operations over the ranges.</summary>
		/// <returns>A sequence of merged subranges</returns>
		[NotNull]
		internal IEnumerable<Range<T>> GetMergedRanges() => _hasRangesToMerge
			? MergeRangesCore()
			: SubRanges;

		[NotNull]
		private IEnumerable<Range<T>> MergeRangesCore()
		{
			var mergedRange = _emptyRangeNoKey;

			foreach (var range in SubRanges)
			{
				if (range.IsEmpty)
				{
					continue;
				}
				if (mergedRange.IsEmpty)
				{
					mergedRange = range;
				}
				else if (IsContinuationFor(mergedRange.To, range))
				{
					mergedRange = mergedRange.ExtendTo(range.To);
				}
				else
				{
					yield return mergedRange;
					mergedRange = range;
				}
			}

			if (mergedRange.IsNotEmpty)
			{
				yield return mergedRange;
			}
		}
		#endregion

		#region Operations
		/// <summary>Returns simplified composite range. Adjacent ranges with same keys will be merged.</summary>
		/// <returns>Simplified composite range.</returns>
		[Pure]
		public CompositeRange<T> Merge()
		{
			if (IsMerged)
				return this;

			return new CompositeRange<T>(
				MergeRangesCore(SubRanges),
				CompositeRangeInternal.UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged);
		}

		#region Updating a range
		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		[Pure]
		public CompositeRange<T, TKey2> WithKeys<TKey2>(TKey2 key)
			where TKey2 : notnull =>
			IsEmpty
				? CompositeRange<T, TKey2>.Empty
				: SubRanges.Select(s => s.WithKey(key)).ToCompositeRange();
		#endregion

		#endregion

		#region IEquatable<CompositeRange<T>>
		/// <summary>Indicates whether the current range is equal to another.</summary>
		/// <param name="other">A range to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current range is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		public bool Equals(CompositeRange<T> other)
		{
			if (IsEmpty)
				return other.IsEmpty;
			if (other.IsEmpty)
				return false;

			DebugCode.BugIf(_ranges == null, "_ranges == null");
			DebugCode.BugIf(other._ranges == null, "other._ranges == null");

			var otherRanges = other._ranges;
			if (_containingRange != other._containingRange || _ranges.Count != otherRanges.Count)
				return false;

			for (var i = 0; i < _ranges.Count; i++)
			{
				if (!_ranges[i].Equals(otherRanges[i]))
					return false;
			}

			return true;
		}

		/// <summary>Indicates whether the current range and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current range are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object? obj) => obj is CompositeRange<T> other && Equals(other);

		/// <summary>Returns a hash code for the current range.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			var result = 0;
			foreach (var range in SubRanges)
			{
				result = HashCode.Combine(result, range.GetHashCode());
			}
			return result;
		}
		#endregion
	}
}