using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.RangesV2
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	public static partial class RangeTests
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
			AreEqual(range.WithKey(RangeKey2), Range.Create(1, 2, RangeKey2));
			AreEqual(range.WithKey(RangeKey2).WithoutKey(), new Range<int>(1, 2));

			var toInf = (double?)double.PositiveInfinity;
			var range2 = Range.CreateExclusive(1, toInf);
			AreEqual(range2.WithKey(RangeKey2).Key, RangeKey2);
		}

		[Test]
		public static void TestRangeContains()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			IsFalse(range.Contains(null));
			IsFalse(range.Contains(double.NegativeInfinity));
			IsFalse(range.Contains(double.PositiveInfinity));
			IsFalse(range.Contains(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.Contains(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.Contains(0));
			IsTrue(range.Contains(1));
			IsTrue(range.Contains(1.5));
			IsTrue(range.Contains(2));
			IsFalse(range.Contains(3));

			range = Range.Create(emptyFrom, emptyTo);
			IsFalse(range.Contains(null));
			IsFalse(range.Contains(double.NegativeInfinity));
			IsFalse(range.Contains(double.PositiveInfinity));
			IsTrue(range.Contains(RangeBoundaryFrom<double?>.Empty));
			IsTrue(range.Contains(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.Contains(0));

			range = Range.CreateExclusive(empty, empty);
			IsTrue(range.Contains(null));
			IsTrue(range.Contains(double.NegativeInfinity));
			IsTrue(range.Contains(double.PositiveInfinity));
			IsFalse(range.Contains(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.Contains(RangeBoundaryTo<double?>.Empty));
			IsTrue(range.Contains(0));

			range = Range.CreateExclusive(value1, value2);
			IsFalse(range.Contains(1));
			IsTrue(range.Contains(1.5));
			IsFalse(range.Contains(2));

			range = Range.CreateExclusive(value1, value2);
			IsFalse(range.Contains(Range.BoundaryFrom<double?>(1)));
			IsFalse(range.Contains(Range.BoundaryTo<double?>(2)));
			IsTrue(range.Contains(Range.BoundaryFromExclusive<double?>(1)));
			IsTrue(range.Contains(Range.BoundaryFromExclusive<double?>(1.5)));
			IsFalse(range.Contains(Range.BoundaryFromExclusive<double?>(2)));
			IsFalse(range.Contains(Range.BoundaryToExclusive<double?>(1)));
			IsTrue(range.Contains(Range.BoundaryToExclusive<double?>(1.5)));
			IsTrue(range.Contains(Range.BoundaryToExclusive<double?>(2)));

			Throws<ArgumentException>(
				() => range.Contains(Range.BoundaryFrom<double?>(double.PositiveInfinity)));
			Throws<ArgumentException>(
				() => range.Contains(Range.BoundaryTo<double?>(double.NegativeInfinity)));
		}

		[Test]
		public static void TestRangeContainsRange()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			IsTrue(range.Contains(range));
			IsTrue(range.Contains(1, 2));
			IsTrue(range.Contains(Range.CreateExclusive(value1, value2, RangeKey2)));
			IsFalse(range.Contains(1, null));
			IsFalse(range.Contains(Range<double?>.Empty));
			IsFalse(range.Contains(Range<double?>.Infinite));
			IsFalse(range.Contains(null, null));
			IsFalse(range.Contains(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.Contains(2, 1));
			Throws<ArgumentException>(
				() => range.Contains(double.PositiveInfinity, double.NegativeInfinity));
			IsTrue(range.Contains(1.5, 1.5));
			IsTrue(range.Contains(1.5, 2));
			IsFalse(range.Contains(0, 3));
			IsFalse(range.Contains(0, 1.5));
			IsFalse(range.Contains(1.5, 3));
			IsFalse(range.Contains(3, 4));

			range = Range.Create(emptyFrom, emptyTo);
			IsTrue(range.Contains(range));
			IsFalse(range.Contains(1, 2));
			IsTrue(range.Contains(Range.Create(emptyFrom, emptyTo, RangeKey2)));
			IsFalse(range.Contains(1, null));
			IsTrue(range.Contains(Range<double?>.Empty));
			IsFalse(range.Contains(Range<double?>.Infinite));
			IsFalse(range.Contains(null, null));
			IsFalse(range.Contains(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.Contains(2, 1));
			Throws<ArgumentException>(
				() => range.Contains(double.PositiveInfinity, double.NegativeInfinity));

			range = Range.CreateExclusive(empty, empty);
			IsTrue(range.Contains(range));
			IsTrue(range.Contains(Range.CreateExclusive(empty, empty, RangeKey2)));
			IsTrue(range.Contains(1, 2));
			IsTrue(range.Contains(1, null));
			IsFalse(range.Contains(Range<double?>.Empty));
			IsTrue(range.Contains(Range<double?>.Infinite));
			IsTrue(range.Contains(null, null));
			IsTrue(range.Contains(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.Contains(2, 1));
			Throws<ArgumentException>(
				() => range.Contains(double.PositiveInfinity, double.NegativeInfinity));

			range = Range.CreateExclusive(value1, value2);
			IsTrue(range.Contains(Range.CreateExclusive(value1, value2, RangeKey2)));
			IsFalse(range.Contains(1, 2));
			IsTrue(range.Contains(1.5, 1.5));
			IsFalse(range.Contains(1.5, 2));
			IsFalse(range.Contains(3, 4));
		}

		[Test]
		public static void TestRangeHasIntersection()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			IsTrue(range.HasIntersection(range));
			IsTrue(range.HasIntersection(1, 2));
			IsTrue(range.HasIntersection(Range.CreateExclusive(value1, value2, RangeKey2)));
			IsTrue(range.HasIntersection(1, null));
			IsFalse(range.HasIntersection(Range<double?>.Empty));
			IsTrue(range.HasIntersection(Range<double?>.Infinite));
			IsTrue(range.HasIntersection(null, null));
			IsTrue(range.HasIntersection(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.HasIntersection(2, 1));
			Throws<ArgumentException>(
				() => range.HasIntersection(double.PositiveInfinity, double.NegativeInfinity));
			IsTrue(range.HasIntersection(1.5, 1.5));
			IsTrue(range.HasIntersection(1.5, 2));
			IsTrue(range.HasIntersection(0, 3));
			IsTrue(range.HasIntersection(0, 1.5));
			IsTrue(range.HasIntersection(1.5, 3));
			IsFalse(range.HasIntersection(3, 4));

			range = Range.Create(emptyFrom, emptyTo);
			IsTrue(range.HasIntersection(range));
			IsFalse(range.HasIntersection(1, 2));
			IsTrue(range.HasIntersection(Range.Create(emptyFrom, emptyTo, RangeKey2)));
			IsFalse(range.HasIntersection(1, null));
			IsTrue(range.HasIntersection(Range<double?>.Empty));
			IsFalse(range.HasIntersection(Range<double?>.Infinite));
			IsFalse(range.HasIntersection(null, null));
			IsFalse(range.HasIntersection(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.HasIntersection(2, 1));
			Throws<ArgumentException>(
				() => range.HasIntersection(double.PositiveInfinity, double.NegativeInfinity));

			range = Range.CreateExclusive(empty, empty);
			IsTrue(range.HasIntersection(range));
			IsTrue(range.HasIntersection(Range.CreateExclusive(empty, empty, RangeKey2)));
			IsTrue(range.HasIntersection(1, 2));
			IsTrue(range.HasIntersection(1, null));
			IsFalse(range.HasIntersection(Range<double?>.Empty));
			IsTrue(range.HasIntersection(Range<double?>.Infinite));
			IsTrue(range.HasIntersection(null, null));
			IsTrue(range.HasIntersection(double.NegativeInfinity, double.PositiveInfinity));
			Throws<ArgumentException>(
				() => range.HasIntersection(2, 1));
			Throws<ArgumentException>(
				() => range.HasIntersection(double.PositiveInfinity, double.NegativeInfinity));

			range = Range.CreateExclusive(value1, value2);
			IsTrue(range.HasIntersection(Range.CreateExclusive(value1, value2, RangeKey2)));
			IsTrue(range.HasIntersection(1, 2));
			IsTrue(range.HasIntersection(1.5, 1.5));
			IsTrue(range.HasIntersection(1.5, 2));
			IsFalse(range.HasIntersection(3, 4));
		}


		[Test]
		public static void TestRangeAdjust()
		{
			var emptyFrom = RangeBoundaryFrom<double>.Empty;
			var emptyTo = RangeBoundaryTo<double>.Empty;

			var range = Range.Create(1.0, 2.0);
			AreEqual(range.Adjust(double.NegativeInfinity), 1);
			AreEqual(range.Adjust(0), 1);
			AreEqual(range.Adjust(1), 1);
			AreEqual(range.Adjust(1.5), 1.5);
			AreEqual(range.Adjust(2), 2);
			AreEqual(range.Adjust(3), 2);
			AreEqual(range.Adjust(double.PositiveInfinity), 2);

			range = Range.Create(double.NegativeInfinity, double.PositiveInfinity);
			AreEqual(range.Adjust(double.NegativeInfinity), double.NegativeInfinity);
			AreEqual(range.Adjust(0), 0);
			AreEqual(range.Adjust(double.PositiveInfinity), double.PositiveInfinity);

			range = Range.Create(emptyFrom, emptyTo);
			Throws<ArgumentException>(() => range.Adjust(double.NegativeInfinity));
			Throws<ArgumentException>(() => range.Adjust(1));
			Throws<ArgumentException>(() => range.Adjust(double.PositiveInfinity));

			range = Range.CreateExclusive(1.0, 2.0);
			Throws<ArgumentException>(() => range.Adjust(double.NegativeInfinity));
			Throws<ArgumentException>(() => range.Adjust(1.5));
			Throws<ArgumentException>(() => range.Adjust(double.PositiveInfinity));
		}
	}
}