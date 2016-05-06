using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.RangesV2
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public partial class RangeExtensionsTests
	{
		[Test]
		public static void TestRangeMakeInclusiveExclusive()
		{
			var range = Range.Create(1, 2);
			AreEqual(range, range.MakeInclusive(i => i - 1, i => i + 1));
			range = Range.CreateExclusive(1, 2);
			AreEqual(Range.Create(0, 3), range.MakeInclusive(i => i - 1, i => i + 1));
			range = Range.CreateExclusiveTo(1, 2);
			AreEqual(Range.Create(1, 3), range.MakeInclusive(i => i - 1, i => i + 1));

			range = Range.CreateExclusive(1, 2);
			AreEqual(range, range.MakeExclusive(i => i - 1, i => i + 1));
			range = Range.Create(1, 2);
			AreEqual(Range.CreateExclusive(0, 3), range.MakeExclusive(i => i - 1, i => i + 1));
			range = Range.CreateExclusiveFrom(1, 2);
			AreEqual(Range.CreateExclusive(1, 3), range.MakeExclusive(i => i - 1, i => i + 1));

			range = Range.CreateExclusive(2, 3);
			IsTrue(range.MakeInclusive(i => i + 1, i => i - 1).IsEmpty);
			range = Range.Create(2, 3);
			IsTrue(range.MakeExclusive(i => i + 1, i => i - 1).IsEmpty);

			range = Range.CreateExclusive(2, 3);
			IsTrue(range.MakeInclusive(i => i + 1, i => i - 1).IsEmpty);
			range = Range.Create(2, 3);
			IsTrue(range.MakeExclusive(i => i + 1, i => i - 1).IsEmpty);

			var range2 = Range.CreateExclusive(1, double.PositiveInfinity);
			IsTrue(range2.MakeInclusive(i => double.NegativeInfinity, i => i + 1).IsInfinite);
			range2 = Range.Create(double.NegativeInfinity, 2);
			IsTrue(range2.MakeExclusive(i => i + 1, i => double.PositiveInfinity).IsInfinite);
			range2 = Range.Create(double.NegativeInfinity, double.PositiveInfinity);
			AreEqual(range2, range2.MakeInclusive(i => i - 1, i => i + 1));
		}

		[Test]
		public static void TestRangeWithValue()
		{
			var range = Range.Create(1, 2);
			AreEqual(Range.Create(0, 3), range.WithValues(i => i - 1, i => i + 1));

			range = Range.CreateExclusiveFrom(1, 2);
			AreEqual(Range.CreateExclusiveFrom(0, 3), range.WithValues(i => i - 1, i => i + 1));

			range = Range.CreateExclusive(1, 2);
			AreEqual(Range.CreateExclusive(2, 3), range.WithValues(i => i + 1));

			var toInf = (double?)double.PositiveInfinity;
			var range2 = Range.CreateExclusive(1, toInf);
			IsTrue(range2.WithValues(i => null).IsInfinite);
			range2 = Range.Create(double.NegativeInfinity, toInf);
			AreEqual(range2, range2.WithValues(i => i - 1, i => i + 1));
		}

		[Test]
		public static void TestRangeWithKey()
		{
			var range = Range.Create(1, 2);
			AreEqual(range.WithKey('^'), Range.Create(1, 2, '^'));
			AreEqual(range.WithKey('^').WithoutKey(), new Range<int>(1, 2));

			var toInf = (double?)double.PositiveInfinity;
			var range2 = Range.CreateExclusive(1, toInf);
			AreEqual(range2.WithKey('^').Key, '^');
		}
	}
}