using System.Collections.Generic;
using System.Linq;

using CodeJam.PerfTests;
using CodeJam.Strings;

using IntervalTree;

using NUnit.Framework;

namespace CodeJam.Ranges
{
	[CompetitionBurstMode]
	public class IntervalTreePerfTest
	{
		private const int _count = 10000;

		private readonly CompositeRange<int, string> _ranges;

		private readonly IntervalTree<int, string> _tree;
		private readonly IntervalTreeCostin<int, string> _treeCostin;
		private readonly Range<int> _intersection;
		private readonly Interval<int> _intersectionCostin;

		public IntervalTreePerfTest()
		{
			_ranges = Enumerable.Range(0, _count)
				.ToCompositeRange(i => 4 * i - 2 * (i % 4), i => 1 + 4 * i + 2 * (i % 4), i => i.ToString());
			;

			_intersection = _ranges.SubRanges[1000].WithoutKey();
			_intersectionCostin = new Interval<int>(_intersection.FromValue, _intersection.ToValue);

			_tree = new IntervalTree<int, string>(_ranges);
			_treeCostin = new IntervalTreeCostin<int, string>(
				_ranges.SubRanges.Select(r => new KeyValuePair<IInterval<int>, string>(
					new Interval<int>(r.FromValue, r.ToValue), r.Key))
				);
		}

		private List<Range<int, string>> IntersectNaive(Range<int> check)
		{
			var result = new List<Range<int, string>>();
			if (!_ranges.ContainingRange.HasIntersection(check))
				return result;
			foreach (var range in _ranges.SubRanges)
			{
				if (range.From > check.To)
					break;
				if (range.To >= check.From)
					result.Add(range);
			}
			return result;
		}

		[Test]
		public void TestIntervalTreeEqual()
		{
			Assert.AreEqual(
				_ranges.SubRanges.Where(r => r.HasIntersection(_intersection)).ToArray(),
				IntersectNaive(_intersection).ToArray());

			Assert.AreEqual(
				_tree.Intersect(_intersection).Join(";"),
				IntersectNaive(_intersection).ToArray().Join(";"));

			Assert.AreEqual(
				_treeCostin
					.GetIntervalsOverlappingWith(_intersectionCostin)
					.Select(i => new Range<int, string>(i.Key.Start, i.Key.End, i.Value))
					.Join(";"),
				IntersectNaive(_intersection).ToArray().Join(";"));
		}

		[Test]
		public void RunIntervalTreePerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		public void Intersect() => IntersectNaive(_intersection);

		[CompetitionBenchmark(0.083, 0.092)]
		public void IntersectTree() => _tree.Intersect(_intersection);

		[CompetitionBenchmark(0.21, 0.27)]
		public void IntersectCostin() => _treeCostin.GetIntervalsOverlappingWith(_intersectionCostin).ToArray();
	}
}