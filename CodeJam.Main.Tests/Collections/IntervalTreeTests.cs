using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using NUnit.Framework;

#pragma warning disable CA1810 // Initialize reference type static fields inline

namespace CodeJam.Ranges
{
	public class IntervalTreeTests
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private static List<Range<int, string>> IntersectNaive(CompositeRange<int, string> rangeA, Range<int> overlap)
		{
			var result = new List<Range<int, string>>();
			if (!rangeA.ContainingRange.HasIntersection(overlap))
				return result;
			foreach (var range in rangeA.SubRanges)
			{
				if (range.From > overlap.To)
					break;
				if (range.To >= overlap.From)
					result.Add(range);
			}
			return result;
		}

		private const int _count = 1000;

		private static readonly CompositeRange<int, string> _sameStartRanges;
		private static readonly CompositeRange<int, string> _sameEndRanges;
		private static readonly CompositeRange<int, string> _nonOverlappingRanges;
		private static readonly CompositeRange<int, string> _overlappingRanges;

		static IntervalTreeTests()
		{
			_sameStartRanges = Enumerable.Range(0, _count)
				.ToCompositeRange(i => 0, i => 2 * i, i => i.ToString(CultureInfo.InvariantCulture));
			_sameEndRanges = Enumerable.Range(0, _count)
				.ToCompositeRange(i => 0, i => 2 * i, i => i.ToString(CultureInfo.InvariantCulture));
			_nonOverlappingRanges = Enumerable.Range(0, _count)
				.ToCompositeRange(i => 4 * i - 2, i => 4 * i + 2, i => i.ToString(CultureInfo.InvariantCulture));
			_overlappingRanges = Enumerable.Range(0, _count)
				.ToCompositeRange(i => 4 * i - 2 * (i % 4), i => 4 * i + 2 * (i % 4), i => i.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void TestSameStartRanges() => TestRangesCore(_sameStartRanges);

		[Test]
		public void TestSameEndRanges() => TestRangesCore(_sameEndRanges);

		[Test]
		public void TestNonOverlappingRanges() => TestRangesCore(_nonOverlappingRanges);

		[Test]
		public void TestOverlappingRanges() => TestRangesCore(_overlappingRanges);

		public static void TestRangesCore(CompositeRange<int, string> ranges)
		{
			var tree = new IntervalTree<int,string>(ranges);
			for (var i = ranges.ContainingRange.FromValue; i <= ranges.ContainingRange.ToValue; i++)
			{
				var overlapRange = Range.Create(i, i);
				AssertSameOverlap(ranges, tree, overlapRange);
				overlapRange = Range.Create(i - 1, i + 1);
				AssertSameOverlap(ranges, tree, overlapRange);
				overlapRange = Range.Create(i - 2, i + 2);
				AssertSameOverlap(ranges, tree, overlapRange);
				overlapRange = Range.Create(i - 10, i);
				AssertSameOverlap(ranges, tree, overlapRange);
			}
		}

		private static void AssertSameOverlap(CompositeRange<int, string> ranges, IntervalTree<int, string> tree, Range<int> overlapRange) =>
			Assert.AreEqual(
				ranges.SubRanges.Where(r => r.HasIntersection(overlapRange)).Join(";"),
				tree.Intersect(overlapRange).Join(";"));
	}
}