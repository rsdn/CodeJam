using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <seealso cref="CompositeRange{T}"/>.</summary>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_while")]
	[PublicAPI]
	public static partial class CompositeRangeExtensions
	{
		#region ToCompositeRange
		/// <summary>Converts range to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <returns>A new composite range.</returns>
		public static CompositeRange<T> ToCompositeRange<T>(this Range<T> range)
			=> new CompositeRange<T>(range);

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="ranges">The ranges.</param>
		/// <returns>A new composite range.</returns>
		public static CompositeRange<T> ToCompositeRange<T>([NotNull] this IEnumerable<Range<T>> ranges)
			=> new CompositeRange<T>(ranges);
		#endregion

		#region Updating a range
		/// <summary>
		/// Replaces exclusive boundaries with inclusive ones with the values from the selector callbacks
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is exclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is exclusive.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		public static CompositeRange<T> MakeInclusive<T>(
			this CompositeRange<T> compositeRange,
			[NotNull, InstantHandle] Func<T, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T> toValueSelector)
		{
			if (compositeRange.IsEmpty)
				return compositeRange;

			return compositeRange.SubRanges
				.Select(r => r.MakeInclusive(fromValueSelector, toValueSelector))
				.ToCompositeRange();
		}

		/// <summary>
		/// Replaces inclusive boundaries with exclusive ones with the values from the selector callbacks
		/// </summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is inclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is inclusive.</param>
		/// <returns>A range with exclusive boundaries.</returns>
		public static CompositeRange<T> MakeExclusive<T>(
			this CompositeRange<T> compositeRange,
			[NotNull, InstantHandle] Func<T, T> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T> toValueSelector)
		{
			if (compositeRange.IsEmpty)
				return compositeRange;

			return compositeRange.SubRanges
				.Select(r => r.MakeExclusive(fromValueSelector, toValueSelector))
				.Where(r => r.IsNotEmpty)
				.ToCompositeRange();
		}

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="newValueSelector">The value of the new key.</param>
		/// <returns>A new composite range with the key specified.</returns>
		public static CompositeRange<T2> WithValues<T, T2>(
			this CompositeRange<T> compositeRange,
			[NotNull, InstantHandle] Func<T, T2> newValueSelector) =>
				compositeRange.IsEmpty
					? CompositeRange<T2>.Empty
					: compositeRange.SubRanges.Select(s => s.WithValues(newValueSelector)).ToCompositeRange();

		/// <summary>Creates a new composite range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="T2">The type of new range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A new composite range with the key specified.</returns>
		public static CompositeRange<T2> WithValues<T, T2>(
			this CompositeRange<T> compositeRange,
			[NotNull, InstantHandle] Func<T, T2> fromValueSelector,
			[NotNull, InstantHandle] Func<T, T2> toValueSelector) =>
				compositeRange.IsEmpty
					? CompositeRange<T2>.Empty
					: compositeRange.SubRanges.Select(s => s.WithValues(fromValueSelector, toValueSelector)).ToCompositeRange();
		#endregion

		#region Get intersections
		private static RangeIntersection<T> GetRangeIntersection<T>(
			RangeBoundaryFrom<T> intersectionFrom, RangeBoundaryTo<T> intersectionTo,
			[NotNull] IEnumerable<Range<T>> intersectionRanges) =>
				new RangeIntersection<T>(
					Range.Create(intersectionFrom, intersectionTo),
					intersectionRanges.ToArray());

		/// <summary>Returns all range intersections from the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <returns>All range intersections from the composite range.</returns>
		[NotNull]
		public static IEnumerable<RangeIntersection<T>> GetIntersections<T>(this CompositeRange<T> compositeRange)
		{
			if (compositeRange.IsEmpty)
			{
				yield break;
			}

			var toBoundaries = new List<RangeBoundaryTo<T>>(); // Sorted by descending.
			var rangesToYield = new List<Range<T>>();

			var fromBoundary = RangeBoundaryFrom<T>.NegativeInfinity;
			foreach (var range in compositeRange.SubRanges)
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

		#region Contains
		/// <summary>Determines whether the composite range contains the specified value.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c>, if the composite range contains the value.</returns>
		public static bool Contains<T>(this CompositeRange<T> compositeRange, T value) =>
			compositeRange.ContainingRange.Contains(value) &&
				compositeRange.SubRanges.Any(r => r.Contains(value));

		/// <summary>Determines whether the composite range contains the specified range boundary.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the composite range contains the boundary.</returns>
		public static bool Contains<T>(this CompositeRange<T> compositeRange, RangeBoundaryFrom<T> other) =>
			compositeRange.ContainingRange.Contains(other) &&
				compositeRange.SubRanges.Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains the specified range boundary.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The boundary to check.</param>
		/// <returns><c>true</c>, if the composite range contains the boundary.</returns>
		public static bool Contains<T>(this CompositeRange<T> compositeRange, RangeBoundaryTo<T> other) =>
			compositeRange.ContainingRange.Contains(other) &&
				compositeRange.SubRanges.Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		public static bool Contains<T>(this CompositeRange<T> compositeRange, T from, T to) =>
			Contains(compositeRange, Range.Create(from, to));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		public static bool Contains<T>(
			this CompositeRange<T> compositeRange,

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				compositeRange.ContainingRange.Contains(other) &&
					compositeRange.GetMergedRanges().Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		public static bool Contains<T, TKey2>(
			this CompositeRange<T> compositeRange, Range<T, TKey2> other) =>
				compositeRange.ContainingRange.Contains(other) &&
					compositeRange.GetMergedRanges().Any(r => r.Contains(other));

		/// <summary>Determines whether the composite range contains another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TCompositeRange">The type of another range.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range contains another range.</returns>
		public static bool Contains<T, TCompositeRange>(
			this CompositeRange<T> compositeRange, TCompositeRange other)
			where TCompositeRange : ICompositeRange<T>
		{
			if (compositeRange.IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!compositeRange.ContainingRange.Contains(other.ContainingRange))
			{
				return false;
			}

			bool result = true;
			using (var containingRanges = compositeRange.GetMergedRanges().GetEnumerator())
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
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="from">The boundary From value of the range to check.</param>
		/// <param name="to">The boundary To value of the range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		public static bool HasIntersection<T>(
			this CompositeRange<T> compositeRange,
			T from, T to) =>
				HasIntersection(compositeRange, Range.Create(from, to));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		public static bool HasIntersection<T>(
			this CompositeRange<T> compositeRange,

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				compositeRange.ContainingRange.HasIntersection(other) &&
					compositeRange.SubRanges.Any(r => r.HasIntersection(other));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		public static bool HasIntersection<T, TKey2>(
			this CompositeRange<T> compositeRange, Range<T, TKey2> other) =>
				compositeRange.ContainingRange.HasIntersection(other) &&
					compositeRange.SubRanges.Any(r => r.HasIntersection(other));

		/// <summary>Determines whether the composite range has intersection with another range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TCompositeRange">The type of another range.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to check.</param>
		/// <returns><c>true</c>, if the composite range has intersection with another range.</returns>
		public static bool HasIntersection<T, TCompositeRange>(
			this CompositeRange<T> compositeRange, TCompositeRange other)
			where TCompositeRange : ICompositeRange<T>
		{
			if (compositeRange.IsEmpty && other.IsEmpty)
			{
				return true;
			}
			if (!compositeRange.ContainingRange.HasIntersection(other.ContainingRange))
			{
				return false;
			}

			bool result = false;
			using (var containingRanges = compositeRange.GetMergedRanges().GetEnumerator())
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

		#region Union & Intersect
		/// <summary>Returns a union range containing all subranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing all subranges.</returns>
		public static CompositeRange<T> Union<T>(this CompositeRange<T> compositeRange, Range<T> other) =>
				Union(compositeRange, other.ToCompositeRange());

		/// <summary>Returns a union range containing all subranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to union with.</param>
		/// <returns>A union range containing all subranges.</returns>
		public static CompositeRange<T> Union<T>(
			this CompositeRange<T> compositeRange, CompositeRange<T> other)
		{
			if (other.IsEmpty)
			{
				return compositeRange;
			}
			if (compositeRange.IsEmpty)
			{
				return other;
			}

			var ranges1 = compositeRange.SubRanges;
			var ranges2 = other.SubRanges;
			var resultRanges = new Range<T>[ranges1.Count + ranges2.Count];

			var overload = compositeRange.IsMerged && other.IsMerged
				? UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged
				: UnsafeOverload.RangesAlreadySorted;

			if (other.ContainingRange.EndsBefore(compositeRange.ContainingRange))
			{
				ranges2.CopyTo(resultRanges, 0);
				ranges1.CopyTo(resultRanges, ranges2.Count);
			}
			else
			{
				ranges1.CopyTo(resultRanges, 0);
				ranges2.CopyTo(resultRanges, ranges1.Count);

				if (!compositeRange.ContainingRange.EndsBefore(other.ContainingRange))
				{
					overload = UnsafeOverload.NoEmptyRanges;
				}
			}
			var result = new CompositeRange<T>(resultRanges, overload);
			if (overload != UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged)
				result = result.Merge();

			return result;
		}

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		public static CompositeRange<T> Intersect<T>(this CompositeRange<T> compositeRange, T from, T to) =>
			Intersect(compositeRange, Range.Create(from, to).ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		public static CompositeRange<T> Intersect<T>(
			this CompositeRange<T> compositeRange,

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				Intersect(compositeRange, other.ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		public static CompositeRange<T> Intersect<T, TKey2>(this CompositeRange<T> compositeRange, Range<T, TKey2> other) =>
			Intersect(compositeRange, other.ToCompositeRange());

		/// <summary>Returns an intersection of the the ranges.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TCompositeRange">The type of another range.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>An intersection range or empty range if the ranges do not intersect.</returns>
		public static CompositeRange<T> Intersect<T, TCompositeRange>(
			this CompositeRange<T> compositeRange, TCompositeRange other)
			where TCompositeRange : ICompositeRange<T>
		{
			if (compositeRange.IsEmpty)
			{
				return compositeRange;
			}
			if (other.IsEmpty || !compositeRange.ContainingRange.HasIntersection(other.ContainingRange))
			{
				return CompositeRange<T>.Empty;
			}

			var intersectionResult = new List<Range<T>>(compositeRange.SubRanges.Count);

			var rangesToIntersect = new List<Range<T>>(compositeRange.SubRanges);

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
				result = CompositeRange<T>.Empty;
			}
			else
			{
				var overload = compositeRange.IsMerged
					? UnsafeOverload.NoEmptyRangesAlreadySortedAndMerged
					: UnsafeOverload.RangesAlreadySorted;

				result = new CompositeRange<T>(intersectionResult, overload);
			}

			return result;
		}

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="from">The boundary From value.</param>
		/// <param name="to">The boundary To value.</param>
		/// <returns>Source range with other range excluded.</returns>
		public static CompositeRange<T> Except<T>(this CompositeRange<T> compositeRange, T from, T to) =>
			Except(compositeRange, Range.Create(from, to).ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		public static CompositeRange<T> Except<T>(
			this CompositeRange<T> compositeRange,

		#region T4-dont-replace
			Range<T> other
		#endregion

			) =>
				Except(compositeRange, other.ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the other range key</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		public static CompositeRange<T> Except<T, TKey2>(this CompositeRange<T> compositeRange, Range<T, TKey2> other) =>
			Except(compositeRange, other.ToCompositeRange());

		/// <summary>Returns source range with other range excluded.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TCompositeRange">The type of another range.</typeparam>
		/// <param name="compositeRange">The source range.</param>
		/// <param name="other">The range to intersect with.</param>
		/// <returns>Source range with other range excluded.</returns>
		public static CompositeRange<T> Except<T, TCompositeRange>(
			this CompositeRange<T> compositeRange, TCompositeRange other)
			where TCompositeRange : ICompositeRange<T>
		{
			if (compositeRange.IsEmpty || other.IsEmpty || !compositeRange.ContainingRange.HasIntersection(other.ContainingRange))
			{
				return compositeRange;
			}

			return Intersect(compositeRange, GetComplementationCore<T, TCompositeRange>(other));
		}
		#endregion
	}
}