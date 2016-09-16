using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

namespace CodeJam.Ranges
{
	public struct RangeIntersection<T> : IFormattable // : IGrouping<Range<T>, Range<T>>
	{
		private static readonly ReadOnlyCollection<Range<T>> _emptyRanges = Array<Range<T>>.Empty.AsReadOnly();

		private readonly ReadOnlyCollection<Range<T>> _ranges;

		#region Fields & .ctor()
		internal RangeIntersection(Range<T> intersectionRange, [NotNull] Range<T>[] ranges)
		{
			// TODO: bugif.
			DebugCode.AssertState(
				ranges.All(r => r.Contains(intersectionRange)),
				"Ranges should contain groupingRange.");

			IntersectionRange = intersectionRange;
			_ranges = ranges.AsReadOnly();
		}
		#endregion

		public Range<T> IntersectionRange { get; }

		[NotNull]
		public IReadOnlyList<Range<T>> Ranges => _ranges ?? _emptyRanges;

		public bool IsEmpty => Ranges.Count == 0;

		public bool IsNotEmpty => Ranges.Count > 0;

		#region ToString
		public override string ToString()
		{
			var intersectionRangePart = IntersectionRange.ToString();
			var rangesPart = string.Join(
				SeparatorString,
				Ranges.Select(element => element.ToString()));

			return intersectionRangePart +
				PrefixString +
				rangesPart +
				SuffixString;
		}

		[NotNull]
		public string ToString(string format) => ToString(format, null);

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var intersectionRangePart = IntersectionRange.ToString(format, formatProvider);
			var rangesPart = string.Join(
				SeparatorString,
				Ranges.Select(element => element.ToString(format, formatProvider)));

			return intersectionRangePart +
				PrefixString +
				rangesPart +
				SuffixString;
		}
		#endregion
	}
}