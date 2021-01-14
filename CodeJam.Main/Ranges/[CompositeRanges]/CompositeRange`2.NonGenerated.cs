using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Collections.Backported;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

// The file contains members that should not be copied into CompositeRange<T, TKey>. DO NOT remove it

namespace CodeJam.Ranges
{
	/// <summary>Describes a range of the values.</summary>
	public partial struct CompositeRange<T, TKey>
		where TKey : notnull
	{
		#region Helpers
		private static IEnumerable<Range<T>> MergeRangesNoKeyCore(
			IEnumerable<Range<T, TKey>> sortedRanges)
		{
			var temp = Range<T>.Empty;
			foreach (var range in sortedRanges)
			{
				if (temp.IsEmpty)
				{
					temp = range.WithoutKey();
				}
				else if (IsContinuationFor(temp.To, range))
				{
					temp = temp.ExtendTo(range.To);
				}
				else
				{
					yield return temp;
					temp = range.WithoutKey();
				}

				if (temp.To.IsPositiveInfinity)
					break;
			}

			if (temp.IsNotEmpty)
				yield return temp;
		}
		#endregion

		#region ICompositeRange<T>
		/// <summary>Returns a sequence of merged subranges. Should be used for operations over the ranges.</summary>
		/// <returns>A sequence of merged subranges</returns>
		internal IEnumerable<Range<T>> GetMergedRanges() => _hasRangesToMerge
			? MergeRangesNoKeyCore(SubRanges)
			: SubRanges.Select(r => r.WithoutKey());
		#endregion

		#region Operations
		/// <summary>Returns simplified composite range. Adjacent ranges with same keys will be merged.</summary>
		/// <returns>Simplified composite range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public CompositeRange<T, TKey> Merge()
		{
			if (IsMerged)
				return this;

			var groups = SubRanges.GroupBy(r => r.Key).ToArray();
			if (groups.Length == 1)
			{
				return new CompositeRange<T, TKey>(
					MergeRangesCore(groups[0]),
					UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged);
			}

			// ReSharper disable once ConvertClosureToMethodGroup
			return new CompositeRange<T, TKey>(
				groups.SelectMany(group => MergeRangesCore(group)),
				UnsafeOverload.NoEmptyRanges);
		}

		#region Updating a range
		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public CompositeRange<T, TKey2> WithKeys<TKey2>(TKey2 key)
			where TKey2 : notnull =>
				IsEmpty
					? CompositeRange<T, TKey2>.Empty
					: SubRanges.Select(s => s.WithKey(key)).ToCompositeRange();

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="keySelector">Callback to obtain a value for the range key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public CompositeRange<T, TKey2> WithKeys<TKey2>(
			[InstantHandle] Func<TKey, TKey2> keySelector)
			where TKey2 : notnull =>
				IsEmpty
					? CompositeRange<T, TKey2>.Empty
					: SubRanges.Select(s => s.WithKey(keySelector(s.Key))).ToCompositeRange();

		/// <summary>Removes keys from the composite range.</summary>
		/// <returns>A new composite range without associated keys.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public CompositeRange<T> WithoutKeys() =>
			IsEmpty
				? CompositeRange<T>.Empty
				: SubRanges.Select(s => s.WithoutKey()).ToCompositeRange();
		#endregion

		#endregion

		#region IEquatable<CompositeRange<T, TKey>>
		/// <summary>Indicates whether the current range is equal to another.</summary>
		/// <param name="other">A range to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current range is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		public bool Equals(CompositeRange<T, TKey> other)
		{
			// TODO: BADCODE, rewrite

			if (IsEmpty)
				return other.IsEmpty;
			if (other.IsEmpty)
				return false;

			DebugCode.BugIf(_ranges == null, "_ranges == null");
			DebugCode.BugIf(other._ranges == null, "other._ranges == null");

			var otherRanges = other._ranges;
			if (_containingRange != other._containingRange || _ranges.Count != otherRanges.Count)
				return false;

			var previousRange = Range<T>.Empty;
			var keys = new Dictionary<TKey, int>();
			var nullKeysCount = 0;

			for (var i = 0; i < _ranges.Count; i++)
			{
				var currentWithoutKey = _ranges[i].WithoutKey();

				// TODO: helper method to compare without key.
				if (!currentWithoutKey.Equals(otherRanges[i].WithoutKey()))
					return false;

				if (currentWithoutKey != previousRange)
				{
					var sameKeys = nullKeysCount == 0 && keys.Values.All(a => a == 0);
					if (!sameKeys)
						return false;

					keys.Clear();
					nullKeysCount = 0;
				}

				var key = _ranges[i].Key;
				if (key == null)
					nullKeysCount++;
				else
					keys[key] = keys.GetValueOrDefault(key) + 1;

				var otherKey = otherRanges[i].Key;
				if (otherKey == null)
					nullKeysCount--;
				else
					keys[otherKey] = keys.GetValueOrDefault(otherKey) - 1;

				previousRange = currentWithoutKey;
			}

			return nullKeysCount == 0 && keys.Values.All(a => a == 0);
		}

		/// <summary>Indicates whether the current range and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current range are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object? obj) => obj is CompositeRange<T, TKey> other && Equals(other);

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