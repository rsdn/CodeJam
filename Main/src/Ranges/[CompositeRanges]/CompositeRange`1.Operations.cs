using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

namespace CodeJam.Ranges
{
	/// <summary>Describes a composite range that contains some subranges.</summary>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_while")]
	public partial struct CompositeRange<T>
	{
		#region Updating a range
		/// <summary>
		/// Replaces exclusive boundaries with inclusive ones with the values from the selector callbacks
		/// </summary>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is exclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is exclusive.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		[Pure]
		public CompositeRange<T> MakeInclusive(
			[NotNull, InstantHandle] Func<T, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T> toValueSelector)
		{
			if (IsEmpty)
				return this;

			return SubRanges
				.Select(r => r.MakeInclusive(fromValueSelector, toValueSelector))
				.ToCompositeRange();
		}

		/// <summary>
		/// Replaces inclusive boundaries with exclusive ones with the values from the selector callbacks
		/// </summary>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is inclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is inclusive.</param>
		/// <returns>A range with exclusive boundaries.</returns>
		[Pure]
		public CompositeRange<T> MakeExclusive(
			[NotNull, InstantHandle] Func<T, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T> toValueSelector)
		{
			if (IsEmpty)
				return this;

			return SubRanges
				.Select(r => r.MakeExclusive(fromValueSelector, toValueSelector))
				.Where(r => r.IsNotEmpty)
				.ToCompositeRange();
		}

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="newValueSelector">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		[Pure]
		public CompositeRange<T2> WithValues<T2>(
			[NotNull, InstantHandle] Func<T, T2> newValueSelector) =>
				IsEmpty
					? CompositeRange<T2>.Empty
					: SubRanges.Select(s => s.WithValues(newValueSelector)).ToCompositeRange();

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A new composite range with the key specified.</returns>
		[Pure]
		public CompositeRange<T2> WithValues<T2>(
			[NotNull, InstantHandle] Func<T, T2> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T2> toValueSelector) =>
				IsEmpty
					? CompositeRange<T2>.Empty
					: SubRanges.Select(s => s.WithValues(fromValueSelector, toValueSelector)).ToCompositeRange();
		#endregion

		#region Get intersections
		private RangeIntersection<T> GetRangeIntersection(
			RangeBoundaryFrom<T> intersectionFrom, RangeBoundaryTo<T> intersectionTo,
			[NotNull] IEnumerable<Range<T>> intersectionRanges) =>
				new RangeIntersection<T>(
					Range.Create(intersectionFrom, intersectionTo),
					intersectionRanges.ToArray());

		/// <summary>Returns all range intersections from the composite range.</summary>
		/// <returns>All range intersections from the composite range.</returns>
		[Pure, NotNull]
		public IEnumerable<RangeIntersection<T>> GetIntersections()
		{
			if (IsEmpty)
			{
				yield break;
			}

			var toBoundaries = new List<RangeBoundaryTo<T>>(); // Sorted by descending.
			var rangesToYield = new List<Range<T>>();

			var fromBoundary = RangeBoundaryFrom<T>.NegativeInfinity;
			foreach (var range in SubRanges)
			{
				// return all ranges that has no intersection with current range.
				while (toBoundaries.Count > 0 && toBoundaries.Last() < range.From)
				{
					var toBoundary = toBoundaries.Last();
					yield return GetRangeIntersection(fromBoundary, toBoundary, rangesToYield);

					rangesToYield.RemoveAll(r => r.To == toBoundary);
					toBoundaries.RemoveAt(toBoundaries.Count - 1);
					fromBoundary = toBoundary.GetComplementation();
				}

				// return rangesToYield as they starts before current range.
				if (fromBoundary < range.From)
				{
					var to = range.From.GetComplementation();
					yield return GetRangeIntersection(fromBoundary, to, rangesToYield);
				}

				// updating the state
				rangesToYield.Add(range);
				InsertInSortedList(
					toBoundaries, range.To,
					RangeBoundaryToDescendingComparer<T>.Instance,
					// ReSharper disable once ArgumentsStyleLiteral
					skipDuplicates: true);

				fromBoundary = range.From;
			}

			// flush all ranges.
			while (toBoundaries.Count > 0 && toBoundaries.Last() < RangeBoundaryTo<T>.PositiveInfinity)
			{
				var toBoundary = toBoundaries.Last();
				yield return GetRangeIntersection(fromBoundary, toBoundary, rangesToYield);

				rangesToYield.RemoveAll(r => r.To == toBoundary);
				toBoundaries.RemoveAt(toBoundaries.Count - 1);
				fromBoundary = toBoundary.GetComplementation();
			}

			yield return GetRangeIntersection(fromBoundary, RangeBoundaryTo<T>.PositiveInfinity, rangesToYield);
		}
		#endregion

		#region Get intersection
		/// <summary>Returns ranges that has intersections with passed range.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns>Ranges that has intersections with passed range.</returns>
		[Pure, NotNull]
		public Range<T>[] GetIntersection(T value)
		{
			var ranges = new List<Range<T>>();
			if (!ContainingRange.Contains(value))
				return Array<Range<T>>.Empty;

			foreach (var range in SubRanges)
			{
				if (range.StartsAfter(value))
					break;
				if (range.Contains(value))
					ranges.Add(range);
			}
			return ranges.ToArray();
		}

		/// <summary>Returns ranges that has intersections with passed range.</summary>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns>Ranges that has intersections with passed range.</returns>
		[Pure]
		public RangeIntersection<T> GetIntersection(T from, T to) =>
			GetIntersection(Range.Create(from, to));

		/// <summary>Returns ranges that has intersections with passed range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns>Ranges that has intersections with passed range.</returns>
		[Pure]
		public RangeIntersection<T> GetIntersection(

		#region T4-dont-replace
			Range<T> other
		#endregion

			)
		{
			var ranges = new List<Range<T>>();
			if (!ContainingRange.HasIntersection(other))
				return
					GetRangeIntersection(
						other.From,
						other.To,
						Array<Range<T>>.Empty);
			foreach (var range in SubRanges)
			{
				if (range.From > other.To)
					break;
				if (range.To >= other.From)
					ranges.Add(range);
			}
			return GetRangeIntersection(other.From, other.To, ranges.ToArray());
		}

		/// <summary>Returns ranges that has intersections with passed range.</summary>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns>Ranges that has intersections with passed range.</returns>
		[Pure]
		public RangeIntersection<T> GetIntersection<TKey2>(Range<T, TKey2> other)
		{
			var ranges = new List<Range<T>>();
			if (!ContainingRange.HasIntersection(other))
				return
					GetRangeIntersection(
						other.From,
						other.To,
						Array<Range<T>>.Empty);
			foreach (var range in SubRanges)
			{
				if (range.From > other.To)
					break;
				if (range.To >= other.From)
					ranges.Add(range);
			}
			return GetRangeIntersection(other.From, other.To, ranges.ToArray());
		}
		#endregion

		#region Contains
		/// <summary>Determines whether the composite range contains the specified value.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the composite range contains the value.</returns>
		[Pure]
		public bool Contains(T value) =>
			ContainingRange.Contains(value) &&
				SubRanges.Any(r => r.Contains(value));

		/// <summary>Determines whether the composite range contains the specified range boundary.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the composite range contains the boundary.</returns>
		[Pure]
		public bool Contains(RangeBoundaryFrom<T> other) =>
			ContainingRange.Contains(other) &&
				SubRanges.Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains the specified range boundary.</summary>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the composite range contains the boundary.</returns>
		[Pure]
		public bool Contains(RangeBoundaryTo<T> other) =>
			ContainingRange.Contains(other) &&
				SubRanges.Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		[Pure]
		public bool Contains(T from, T to) =>
			Contains(Range.Create(from, to));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		[Pure]
		public bool Contains(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				ContainingRange.Contains(other) &&
					GetMergedRanges().Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		[Pure]
		public bool Contains<TKey2>(Range<T, TKey2> other) =>
			ContainingRange.Contains(other) &&
				GetMergedRanges().Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		[Pure]
		public bool Contains(

		#region T4-dont-replace
			CompositeRange<T> other
		#endregion

			)
		{
			if (IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!ContainingRange.Contains(other.ContainingRange))
			{
				return false;
			}

			bool result = true;
			using (var containingRanges = GetMergedRanges().GetEnumerator())
			{
				bool hasContainingRange = containingRanges.MoveNext();
				foreach (var otherRange in other.GetMergedRanges())
				{
					while (hasContainingRange && containingRanges.Current.EndsBefore(otherRange))
					{
						hasContainingRange = containingRanges.MoveNext();
					}

					if (!hasContainingRange || !containingRanges.Current.Contains(otherRange))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		public bool Contains<TKey2>(CompositeRange<T, TKey2> other)
		{
			if (IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!ContainingRange.Contains(other.ContainingRange))
			{
				return false;
			}

			bool result = true;
			using (var containingRanges = GetMergedRanges().GetEnumerator())
			{
				bool hasContainingRange = containingRanges.MoveNext();
				foreach (var otherRange in other.GetMergedRanges())
				{
					while (hasContainingRange && containingRanges.Current.EndsBefore(otherRange))
					{
						hasContainingRange = containingRanges.MoveNext();
					}

					if (!hasContainingRange || !containingRanges.Current.Contains(otherRange))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}
		#endregion

		#region HasIntersection
		/// <summary>Determines whether the composite  has intersection with another range.</summary>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		[Pure]
		public bool HasIntersection(T from, T to) =>
			HasIntersection(Range.Create(from, to));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		[Pure]
		public bool HasIntersection(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				ContainingRange.HasIntersection(other) &&
					SubRanges.Any(r => r.HasIntersection(other));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		[Pure]
		public bool HasIntersection<TKey2>(Range<T, TKey2> other) =>
			ContainingRange.HasIntersection(other) &&
				SubRanges.Any(r => r.HasIntersection(other));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		[Pure]
		public bool HasIntersection(

		#region T4-dont-replace
			CompositeRange<T> other
		#endregion

			)
		{
			if (IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!ContainingRange.HasIntersection(other.ContainingRange))
			{
				return false;
			}

			bool result = false;
			using (var containingRanges = GetMergedRanges().GetEnumerator())
			{
				bool hasContainingRange = containingRanges.MoveNext();
				foreach (var otherRange in other.GetMergedRanges())
				{
					while (hasContainingRange && containingRanges.Current.EndsBefore(otherRange))
					{
						hasContainingRange = containingRanges.MoveNext();
					}

					if (!hasContainingRange || containingRanges.Current.HasIntersection(otherRange))
					{
						result = hasContainingRange;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		[Pure]
		public bool HasIntersection<TKey2>(CompositeRange<T, TKey2> other)
		{
			if (IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!ContainingRange.HasIntersection(other.ContainingRange))
			{
				return false;
			}

			bool result = false;
			using (var containingRanges = GetMergedRanges().GetEnumerator())
			{
				bool hasContainingRange = containingRanges.MoveNext();
				foreach (var otherRange in other.GetMergedRanges())
				{
					while (hasContainingRange && containingRanges.Current.EndsBefore(otherRange))
					{
						hasContainingRange = containingRanges.MoveNext();
					}

					if (!hasContainingRange || containingRanges.Current.HasIntersection(otherRange))
					{
						result = hasContainingRange;
						break;
					}
				}
			}

			return result;
		}
		#endregion

		#region Union / Extend
		/// <summary>Returns a union range containing all subranges.</summary>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing all subranges.</returns>
		[Pure]
		public CompositeRange<T> Union(Range<T> other) =>
			Union(other.ToCompositeRange());

		/// <summary>Returns a union range containing all subranges.</summary>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing all subranges.</returns>
		[Pure]
		public CompositeRange<T> Union(CompositeRange<T> other)
		{
			if (other.IsEmpty)
			{
				return this;
			}
			if (IsEmpty)
			{
				return other;
			}

			var ranges1 = SubRanges;
			var ranges2 = other.SubRanges;
			var resultRanges = new Range<T>[ranges1.Count + ranges2.Count];

			var overload = IsMerged && other.IsMerged
				? UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged
				: UnsafeOverload.RangesAlreadySorted;

			if (other.ContainingRange.EndsBefore(ContainingRange))
			{
				ranges2.CopyTo(resultRanges, 0);
				ranges1.CopyTo(resultRanges, ranges2.Count);
			}
			else
			{
				ranges1.CopyTo(resultRanges, 0);
				ranges2.CopyTo(resultRanges, ranges1.Count);

				if (!ContainingRange.EndsBefore(other.ContainingRange))
				{
					overload = UnsafeOverload.NoEmptyRanges;
				}
			}
			var result = new CompositeRange<T>(resultRanges, overload);
			if (overload != UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged)
				result = result.Merge();

			return result;
		}

		/// <summary>Extends the range from the left.</summary>
		/// <param name="from">A new value From.</param>
		/// <returns>
		/// A range with a new From boundary or the source fange if the new boundary is greater than original.
		/// </returns>
		[Pure]
		public CompositeRange<T> ExtendFrom(T from) =>
			ExtendFrom(Range.BoundaryFrom(from));

		/// <summary>Extends the range from the left.</summary>
		/// <param name="from">A new boundary From.</param>
		/// <returns>
		/// A range with a new From boundary or the source fange if the new boundary is greater than original.
		/// </returns>
		[Pure]
		public CompositeRange<T> ExtendFrom(RangeBoundaryFrom<T> from)
		{
			if (IsEmpty || from.IsEmpty || from >= ContainingRange.From)
				return this;

			var ranges = SubRanges.ToArray();
			for (int i = 0; i < ranges.Length; i++)
			{
				if (ranges[i].From != ContainingRange.From)
					break;

				ranges[i] = ranges[i].ExtendFrom(from);
			}
			return new CompositeRange<T>(ranges, UnsafeOverload.RangesAlreadySorted);
		}

		/// <summary>Extends the range from the right.</summary>
		/// <param name="to">A new value To.</param>
		/// <returns>
		/// A range with a new To boundary or the source fange if the new boundary is less than original.
		/// </returns>
		[Pure]
		public CompositeRange<T> ExtendTo(T to) =>
			ExtendTo(Range.BoundaryTo(to));

		/// <summary>Extends the range from the right.</summary>
		/// <param name="to">A new boundary To.</param>
		/// <returns>
		/// A range with a new To boundary or the source fange if the new boundary is less than original.
		/// </returns>
		[Pure]
		public CompositeRange<T> ExtendTo(RangeBoundaryTo<T> to)
		{
			if (IsEmpty || to.IsEmpty || to <= ContainingRange.To)
				return this;

			var ranges = SubRanges.ToArray();
			for (int i = ranges.Length - 1; i >= 0; i--)
			{
				if (ranges[i].To != ContainingRange.To)
					break;

				ranges[i] = ranges[i].ExtendTo(to);
			}
			return new CompositeRange<T>(ranges, UnsafeOverload.RangesAlreadySorted);
		}
		#endregion

		#region Intersect / Trim / Except
		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure]
		public CompositeRange<T> Intersect(T from, T to) =>
			Intersect(Range.Create(from, to).ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure]
		public CompositeRange<T> Intersect(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				Intersect(other.ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure]
		public CompositeRange<T> Intersect<TKey2>(Range<T, TKey2> other) =>
			Intersect(other.ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure]
		public CompositeRange<T> Intersect(

		#region T4-dont-replace
			CompositeRange<T> other
		#endregion
			)
		{
			if (IsEmpty)
			{
				return this;
			}
			if (other.IsEmpty || !ContainingRange.HasIntersection(other.ContainingRange))
			{
				return Empty;
			}

			var intersectionResult = new List<Range<T>>(SubRanges.Count);

			var rangesToIntersect = new List<Range<T>>(SubRanges);

			foreach (var otherRange in other.GetMergedRanges())
			{
				for (int i = 0; i < rangesToIntersect.Count; i++)
				{
					var intersectionRange = rangesToIntersect[i];
					if (intersectionRange.StartsAfter(otherRange))
					{
						break;
					}

					intersectionRange = intersectionRange.Intersect(otherRange);
					if (intersectionRange.IsEmpty)
					{
						rangesToIntersect.RemoveAt(i);
						i--;
					}
					else
					{
						intersectionResult.Add(intersectionRange);
					}
				}

				if (rangesToIntersect.Count == 0)
				{
					break;
				}
			}

			CompositeRange<T> result;
			if (intersectionResult.Count == 0)
			{
				result = Empty;
			}
			else
			{
				var overload = IsMerged
					? UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged
					: UnsafeOverload.RangesAlreadySorted;

				result = new CompositeRange<T>(intersectionResult, overload);
			}

			return result;
		}
		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		[Pure]
		public CompositeRange<T> Intersect<TKey2>(CompositeRange<T, TKey2> other)
		{
			if (IsEmpty)
			{
				return this;
			}
			if (other.IsEmpty || !ContainingRange.HasIntersection(other.ContainingRange))
			{
				return Empty;
			}

			var intersectionResult = new List<Range<T>>(SubRanges.Count);

			var rangesToIntersect = new List<Range<T>>(SubRanges);

			foreach (var otherRange in other.GetMergedRanges())
			{
				for (int i = 0; i < rangesToIntersect.Count; i++)
				{
					var intersectionRange = rangesToIntersect[i];
					if (intersectionRange.StartsAfter(otherRange))
					{
						break;
					}

					intersectionRange = intersectionRange.Intersect(otherRange);
					if (intersectionRange.IsEmpty)
					{
						rangesToIntersect.RemoveAt(i);
						i--;
					}
					else
					{
						intersectionResult.Add(intersectionRange);
					}
				}

				if (rangesToIntersect.Count == 0)
				{
					break;
				}
			}

			CompositeRange<T> result;
			if (intersectionResult.Count == 0)
			{
				result = Empty;
			}
			else
			{
				var overload = IsMerged
					? UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged
					: UnsafeOverload.RangesAlreadySorted;

				result = new CompositeRange<T>(intersectionResult, overload);
			}

			return result;
		}

		/// <summary>Trims the range from the left.</summary>
		/// <param name="from">A new value From.</param>
		/// <returns>A range trimmed with a new From boundary.</returns>
		[Pure]
		public CompositeRange<T> TrimFrom(T from) =>
			TrimFrom(Range.BoundaryFrom(from));

		/// <summary>Trims the range from the left.</summary>
		/// <param name="from">A new boundary From.</param>
		/// <returns>A range trimmed with a new From boundary.</returns>
		[Pure]
		public CompositeRange<T> TrimFrom(RangeBoundaryFrom<T> from) =>
			Intersect(Range.TryCreate(from, RangeBoundaryTo<T>.PositiveInfinity));

		/// <summary>Trims the range from the right.</summary>
		/// <param name="to">A new value To.</param>
		/// <returns>A range trimmed with a new To boundary.</returns>
		[Pure]
		public CompositeRange<T> TrimTo(T to) => TrimTo(Range.BoundaryTo(to));

		/// <summary>Trims the range from the right.</summary>
		/// <param name="to">A new boundary To.</param>
		/// <returns>A range trimmed with a new To boundary.</returns>
		[Pure]
		public CompositeRange<T> TrimTo(RangeBoundaryTo<T> to) =>
			Intersect(Range.TryCreate(RangeBoundaryFrom<T>.NegativeInfinity, to));

		/// <summary>Returns source range with other range excluded.</summary>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>Source range with other range excluded.</returns>
		[Pure]
		public CompositeRange<T> Except(T from, T to) =>
			Except(Range.Create(from, to).ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		[Pure]
		public CompositeRange<T> Except(

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				Except(other.ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		[Pure]
		public CompositeRange<T> Except<TKey2>(Range<T, TKey2> other) =>
			Except(other.ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		[Pure]
		public CompositeRange<T> Except(

		#region T4-dont-replace
			CompositeRange<T> other
		#endregion

			)
		{
			if (IsEmpty || other.IsEmpty || !ContainingRange.HasIntersection(other.ContainingRange))
			{
				return this;
			}

			return Intersect(other.GetComplementation());
		}

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="TKey2">The type of the key of another range.</typeparam>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		[Pure]
		public CompositeRange<T> Except<TKey2>(CompositeRange<T, TKey2> other)
		{
			if (IsEmpty || other.IsEmpty || !ContainingRange.HasIntersection(other.ContainingRange))
			{
				return this;
			}

			return Intersect(other.GetComplementation());
		}

		/// <summary>
		/// Returns complementation composite range.
		/// Result range contains result of (infinityRange.Exclude(this).
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <returns>Complementation composite range.</returns>
		#region T4-dont-replace
		[Pure]
		public CompositeRange<T> GetComplementation()
		{
			if (IsEmpty)
			{
				return CompositeRange<T>.Infinite;
			}

			var result = new List<Range<T>>();

			if (ContainingRange.From.HasValue)
			{
				result.Add(
					Range.Create(
						RangeBoundaryFrom<T>.NegativeInfinity,
						ContainingRange.From.GetComplementation()));
			}

			var previousRange = Range<T>.Empty;
			foreach (var range in GetMergedRanges())
			{
				if (previousRange.IsNotEmpty)
				{
					result.Add(
						Range.Create(
							previousRange.To.GetComplementation(),
							range.From.GetComplementation()));
				}
				previousRange = range;
			}

			if (ContainingRange.To.HasValue)
			{
				result.Add(
					Range.Create(
						ContainingRange.To.GetComplementation(),
						RangeBoundaryTo<T>.PositiveInfinity));
			}

			return new CompositeRange<T>(result, UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged);
		}
		#endregion
		#endregion
	}
}