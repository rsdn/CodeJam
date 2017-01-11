using System.Collections.Generic;
using System.Linq;

using CodeJam.PerfTests;
using CodeJam.Collections;
using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Ranges
{
	[CompetitionReannotateSources]
	[CompetitionBurstMode]
	public class IntervalTreePerfTest
	{
		private const int Count = 10000;

		private readonly CompositeRange<int, string> ranges;

		private readonly IntervalTree<int, string> tree;
		private readonly Range<int> intersection;

		public IntervalTreePerfTest()
		{
			ranges = Enumerable.Range(0, Count)
				.ToCompositeRange(i => 100 * i - 30 * (i % 11), i => 100 * i, i => i.ToString());

			intersection = ranges.SubRanges.TakeLast(100).First().WithoutKey();

			tree = new IntervalTree<int, string>(ranges);
		}


		[Test]
		public void TestEqual()
		{
			Assert.AreEqual(
				ranges.SubRanges.Where(r => r.HasIntersection(intersection)).ToArray(),
				IntersectNaive(intersection).ToArray());

			Assert.AreEqual(
				tree.Intersect(intersection).Join(";"),
				IntersectNaive(intersection).ToArray().Join(";"));
		}

		//[Test]
		//public void Test() => Competition.Run(this);

		[CompetitionBaseline]
		public void Intersect()
		{
			IntersectNaive(intersection);
		}

		private List<Range<int, string>> IntersectNaive(Range<int> check)
		{
			var result = new List<Range<int, string>>();
			if (!ranges.ContainingRange.HasIntersection(check))
				return result;
			foreach (var range in ranges.SubRanges)
			{
				if (range.From > check.To)
					break;
				if (range.To >= check.From)
					result.Add(range);
			}
			return result;
		}

		[CompetitionBenchmark(0.0154, 0.0182)]
		public void IntersectTree() => tree.Intersect(intersection);
	}
}