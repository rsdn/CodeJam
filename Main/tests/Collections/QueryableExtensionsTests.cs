using System;
using System.Linq;

using CodeJam.Ranges;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class QueryableExtensionsTests
	{
		[Test]
		public static void TestIntersectRanges()
		{
			var rangeItems = Enumerable.Range(1, 10)
				.Cast<int?>()
				.Select(i => new
				{
					From = i % 3 == 0 ? null : 10 * i - 5,
					To = i % 5 == 0 ? null : 10 * i + 5
				})
				.OrderBy(x => x.From ?? int.MinValue).ThenBy(x => x.To ?? int.MaxValue)
				.ToArray();

			var ranges = rangeItems.ToCompositeRange(x => x.From, x => x.To);

			var queryableRanges = rangeItems.AsQueryable();

			Assert.AreEqual(queryableRanges.ToArray(), ranges.SubRanges.Select(x => x.Key).ToArray());

			Assert.AreEqual(
				queryableRanges.Intersect(x => x.From, x => x.To, Range.Create<int?>(5, null)).ToArray(),
				ranges.Intersect(5, null).SubRanges.Select(x => x.Key).ToArray());

			Assert.AreEqual(
				queryableRanges.Intersect(x => x.From, x => x.To, Range.Create<int?>(-2, -1)).ToArray(),
				ranges.Intersect(-2, -1).SubRanges.Select(x => x.Key).ToArray());

			Assert.AreEqual(
				queryableRanges.Intersect(x => x.From, x => x.To, Range.Create<int?>(null, -1)).ToArray(),
				ranges.Intersect(null, -1).SubRanges.Select(x => x.Key).ToArray());

			Assert.AreEqual(
				queryableRanges.Intersect(x => x.From, x => x.To, Range<int?>.Empty).ToArray(),
				ranges.Intersect(Range<int?>.Empty).SubRanges.Select(x => x.Key).ToArray());

			Assert.AreEqual(
				queryableRanges.Intersect(x => x.From, x => x.To, Range<int?>.Infinite).ToArray(),
				ranges.Intersect(Range<int?>.Infinite).SubRanges.Select(x => x.Key).ToArray());
		}
	}
}
