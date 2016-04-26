using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Extension methods for ImmutableRangeList.
	/// </summary>
	[PublicAPI]
	public static class ImmutableRangeListExtensions
	{
		/// <summary>
		/// Creates new range list with single range.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="range"></param>
		/// <returns>A new range list.</returns>
		[NotNull]
		public static ImmutableRangeList<TValue> ToRangeList<TValue>(this Range<TValue> range)
			where TValue : IComparable<TValue> => new ImmutableRangeList<TValue>(range);

		/// <summary>
		/// Creates new range list with combined overlapped ranges.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="ranges">Ranges to include in new renge list.</param>
		/// <returns>A new range list.</returns>
		[NotNull]
		public static ImmutableRangeList<TValue> ToRangeList<TValue>([CanBeNull] this IEnumerable<Range<TValue>> ranges)
			where TValue : IComparable<TValue>
		{
			if (ranges == null)
				return ImmutableRangeList<TValue>.Empty;

			var newRanges = ranges.Where(r => !r.IsEmpty).ToList();

			switch (newRanges.Count)
			{
				case 0:
					return ImmutableRangeList<TValue>.Empty;
				case 1:
					var first = newRanges[0];
					return ToRangeList(first);
			}

			newRanges.Sort();

			var i = 0;
			do
			{
				var current = newRanges[i];
				if (current.IsFull)
					return ImmutableRangeList<TValue>.Full;

				var next = newRanges[i + 1];
				if (current.IsAdjastent(next) || current.IsIntersectsWith(next))
				{
					current = current.Union(next);

					if (current.IsFull)
						return ImmutableRangeList<TValue>.Full;

					newRanges[i] = current;
					newRanges.RemoveAt(i + 1);
				}
				else
				{
					++i;
				}
			} while (i < newRanges.Count - 1);

			return new ImmutableRangeList<TValue>(newRanges);
		}

		/// <summary>
		/// Creates new range list with inverted ranges. Overlapped ranges are combined.
		/// </summary>
		/// <typeparam name="TValue">Type of range value</typeparam>
		/// <param name="ranges">Ranges to invert and include in new renge list.</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> Invert<TValue>([CanBeNull] this IEnumerable<Range<TValue>> ranges)
			where TValue : IComparable<TValue> =>
				ranges == null
					? ToRangeList(Range.Full<TValue>())
					: ToRangeList(ranges.SelectMany(r => r.Invert()));
	}
}