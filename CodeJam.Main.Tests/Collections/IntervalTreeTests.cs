﻿using System.Collections.Generic;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Ranges
{
	public class IntervalTreeTests
	{
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

		private const int Count = 1000;

		private readonly CompositeRange<int, string> _sameStartRanges;
		private readonly CompositeRange<int, string> _sameEndRanges;
		private readonly CompositeRange<int, string> _nonOverlappingRanges;
		private readonly CompositeRange<int, string> _overlappingRanges;

		public IntervalTreeTests()
		{
			_sameStartRanges = Enumerable.Range(0, Count)
				.ToCompositeRange(i => 0, i => 2 * i, i => i.ToString());
			_sameEndRanges = Enumerable.Range(0, Count)
				.ToCompositeRange(i => 0, i => 2 * i, i => i.ToString());
			_nonOverlappingRanges = Enumerable.Range(0, Count)
				.ToCompositeRange(i => 4 * i - 2, i => 4 * i + 2, i => i.ToString());
			_overlappingRanges = Enumerable.Range(0, Count)
				.ToCompositeRange(i => 4 * i - 2 * (i % 4), i => 4 * i + 2 * (i % 4), i => i.ToString());
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

		private static void AssertSameOverlap(CompositeRange<int, string> ranges, [NotNull] IntervalTree<int, string> tree, Range<int> overlapRange) =>
			Assert.AreEqual(
				ranges.SubRanges.Where(r => r.HasIntersection(overlapRange)).Join(";"),
				tree.Intersect(overlapRange).Join(";"));
	}
}