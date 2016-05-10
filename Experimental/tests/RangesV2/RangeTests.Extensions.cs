using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.RangesV2
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
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

		[Test]
		public static void TestRangeStartsAfter()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			IsTrue(range.StartsAfter(null));
			IsTrue(range.StartsAfter(double.NegativeInfinity));
			IsFalse(range.StartsAfter(double.PositiveInfinity));
			IsFalse(range.StartsAfter(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.StartsAfter(RangeBoundaryTo<double?>.Empty));
			IsTrue(range.StartsAfter(0));
			IsFalse(range.StartsAfter(1));
			IsFalse(range.StartsAfter(1.5));
			IsFalse(range.StartsAfter(2));
			IsFalse(range.StartsAfter(3));

			IsTrue(range.StartsAfter(Range.Create(empty, 0, RangeKey2)));
			IsTrue(range.StartsAfter(Range.CreateExclusiveTo(empty, 1, RangeKey2)));
			IsFalse(range.StartsAfter(Range.Create(empty, 1, RangeKey2)));

			range = Range.Create(emptyFrom, emptyTo);
			IsFalse(range.StartsAfter(null));
			IsFalse(range.StartsAfter(double.NegativeInfinity));
			IsFalse(range.StartsAfter(double.PositiveInfinity));
			IsFalse(range.StartsAfter(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.StartsAfter(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.StartsAfter(0));

			range = Range.CreateExclusive(empty, empty);
			IsFalse(range.StartsAfter(null));
			IsFalse(range.StartsAfter(double.NegativeInfinity));
			IsFalse(range.StartsAfter(double.PositiveInfinity));
			IsFalse(range.StartsAfter(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.StartsAfter(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.StartsAfter(0));

			range = Range.CreateExclusive(value1, value2);
			IsTrue(range.StartsAfter(1));
			IsFalse(range.StartsAfter(1.5));
			IsFalse(range.StartsAfter(2));

			range = Range.CreateExclusive(value1, value2);
			IsTrue(range.StartsAfter(Range.BoundaryFrom<double?>(1)));
			IsFalse(range.StartsAfter(Range.BoundaryTo<double?>(2)));
			IsFalse(range.StartsAfter(Range.BoundaryFromExclusive<double?>(1)));
			IsFalse(range.StartsAfter(Range.BoundaryFromExclusive<double?>(1.5)));
			IsFalse(range.StartsAfter(Range.BoundaryFromExclusive<double?>(2)));
			IsTrue(range.StartsAfter(Range.BoundaryToExclusive<double?>(1)));
			IsFalse(range.StartsAfter(Range.BoundaryToExclusive<double?>(1.5)));
			IsFalse(range.StartsAfter(Range.BoundaryToExclusive<double?>(2)));

			Throws<ArgumentException>(
				() => range.StartsAfter(Range.BoundaryFrom<double?>(double.PositiveInfinity)));
			Throws<ArgumentException>(
				() => range.StartsAfter(Range.BoundaryTo<double?>(double.NegativeInfinity)));
		}

		[Test]
		public static void TestRangeEndsBefore()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			IsTrue(range.EndsBefore(null));
			IsFalse(range.EndsBefore(double.NegativeInfinity));
			IsTrue(range.EndsBefore(double.PositiveInfinity));
			IsFalse(range.EndsBefore(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.EndsBefore(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.EndsBefore(0));
			IsFalse(range.EndsBefore(1));
			IsFalse(range.EndsBefore(1.5));
			IsFalse(range.EndsBefore(2));
			IsTrue(range.EndsBefore(3));

			IsFalse(range.EndsBefore(Range.Create(2, empty, RangeKey2)));
			IsTrue(range.EndsBefore(Range.CreateExclusiveFrom(2, empty, RangeKey2)));
			IsTrue(range.EndsBefore(Range.Create(3, empty, RangeKey2)));

			range = Range.Create(emptyFrom, emptyTo);
			IsFalse(range.EndsBefore(null));
			IsFalse(range.EndsBefore(double.NegativeInfinity));
			IsFalse(range.EndsBefore(double.PositiveInfinity));
			IsFalse(range.EndsBefore(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.EndsBefore(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.EndsBefore(0));

			range = Range.CreateExclusive(empty, empty);
			IsFalse(range.EndsBefore(null));
			IsFalse(range.EndsBefore(double.NegativeInfinity));
			IsFalse(range.EndsBefore(double.PositiveInfinity));
			IsFalse(range.EndsBefore(RangeBoundaryFrom<double?>.Empty));
			IsFalse(range.EndsBefore(RangeBoundaryTo<double?>.Empty));
			IsFalse(range.EndsBefore(0));

			range = Range.CreateExclusive(value1, value2);
			IsFalse(range.EndsBefore(1));
			IsFalse(range.EndsBefore(1.5));
			IsTrue(range.EndsBefore(2));

			range = Range.CreateExclusive(value1, value2);
			IsFalse(range.EndsBefore(Range.BoundaryFrom<double?>(1)));
			IsTrue(range.EndsBefore(Range.BoundaryTo<double?>(2)));
			IsFalse(range.EndsBefore(Range.BoundaryFromExclusive<double?>(1)));
			IsFalse(range.EndsBefore(Range.BoundaryFromExclusive<double?>(1.5)));
			IsTrue(range.EndsBefore(Range.BoundaryFromExclusive<double?>(2)));
			IsFalse(range.EndsBefore(Range.BoundaryToExclusive<double?>(1)));
			IsFalse(range.EndsBefore(Range.BoundaryToExclusive<double?>(1.5)));
			IsFalse(range.EndsBefore(Range.BoundaryToExclusive<double?>(2)));

			Throws<ArgumentException>(
				() => range.EndsBefore(Range.BoundaryFrom<double?>(double.PositiveInfinity)));
			Throws<ArgumentException>(
				() => range.EndsBefore(Range.BoundaryTo<double?>(double.NegativeInfinity)));
		}

		[Test]
		public static void TestRangeUnion()
		{
			double value1 = 1;
			double value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double>.Empty;
			var emptyTo = RangeBoundaryTo<double>.Empty;
			var emptyRange = Range.Create(emptyFrom, emptyTo);
			var infiniteRange = Range.Create(double.NegativeInfinity, double.PositiveInfinity);

			var range = Range.Create(value1, value2);
			AreEqual(range.Union(range), range);
			AreEqual(range.Union(1, 2), range);
			AreEqual(range.Union(1.5, 1.5), range);
			AreEqual(range.Union(0, 3), Range.Create(0.0, 3.0));
			AreEqual(range.Union(1.5, 3), Range.Create(1.0, 3.0));
			AreEqual(range.Union(0, 1.5), Range.Create(0.0, 2.0));
			AreEqual(range.Union(3, 4), Range.Create(1.0, 4.0));
			AreEqual(range.Union(-2, -1), Range.Create(-2.0, 2.0));
			AreEqual(range.Union(emptyRange), range);
			AreEqual(emptyRange.Union(range), range);
			AreEqual(range.Union(infiniteRange), infiniteRange);
			AreEqual(infiniteRange.Union(range), infiniteRange);
			AreEqual(emptyRange.Union(infiniteRange), infiniteRange);
			AreEqual(infiniteRange.Union(emptyRange), infiniteRange);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.Union(range), range);
			AreEqual(range.Union(1, 2), Range.Create(1.0, 2.0));
			AreEqual(range.Union(1.5, 1.5), range);
			AreEqual(range.Union(0, 3), Range.Create(0.0, 3.0));
			AreEqual(range.Union(1.5, 3), Range.CreateExclusiveFrom(1.0, 3.0));
			AreEqual(range.Union(0, 1.5), Range.CreateExclusiveTo(0.0, 2.0));
			AreEqual(range.Union(3, 4), Range.CreateExclusiveFrom(1.0, 4.0));
			AreEqual(range.Union(-2, -1), Range.CreateExclusiveTo(-2.0, 2.0));
			AreEqual(range.Union(emptyRange), range);
			AreEqual(emptyRange.Union(range), range);
			AreEqual(range.Union(infiniteRange), infiniteRange);
			AreEqual(infiniteRange.Union(range), infiniteRange);
			AreEqual(emptyRange.Union(infiniteRange), infiniteRange);
			AreEqual(infiniteRange.Union(emptyRange), infiniteRange);

			Throws<ArgumentException>(
				() => range.Union(2, 1));
			Throws<ArgumentException>(
				() => range.Union(double.PositiveInfinity, double.NegativeInfinity));
			Throws<ArgumentException>(
				() => range.Union(2, double.NegativeInfinity));
			Throws<ArgumentException>(
				() => range.Union(double.PositiveInfinity, 1));
		}

		[Test]
		public static void TestRangeIntersection()
		{
			double value1 = 1;
			double value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double>.Empty;
			var emptyTo = RangeBoundaryTo<double>.Empty;
			var emptyRange = Range.Create(emptyFrom, emptyTo);
			var infiniteRange = Range.Create(double.NegativeInfinity, double.PositiveInfinity);

			var range = Range.Create(value1, value2);
			AreEqual(range.Intersect(range), range);
			AreEqual(range.Intersect(1, 2), range);
			AreEqual(range.Intersect(1.5, 1.5), Range.Create(1.5, 1.5));
			AreEqual(range.Intersect(0, 3), range);
			AreEqual(range.Intersect(1.5, 3), Range.Create(1.5, 2.0));
			AreEqual(range.Intersect(0, 1.5), Range.Create(1.0, 1.5));
			AreEqual(range.Intersect(3, 4), emptyRange);
			AreEqual(range.Intersect(-2, -1), emptyRange);
			AreEqual(range.Intersect(emptyRange), emptyRange);
			AreEqual(emptyRange.Intersect(range), emptyRange);
			AreEqual(range.Intersect(infiniteRange), range);
			AreEqual(infiniteRange.Intersect(range), range);
			AreEqual(emptyRange.Intersect(infiniteRange), emptyRange);
			AreEqual(infiniteRange.Intersect(emptyRange), emptyRange);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.Intersect(range), range);
			AreEqual(range.Intersect(1, 2), range);
			AreEqual(range.Intersect(1.5, 1.5), Range.Create(1.5, 1.5));
			AreEqual(range.Intersect(0, 3), range);
			AreEqual(range.Intersect(1.5, 3), Range.CreateExclusiveTo(1.5, 2.0));
			AreEqual(range.Intersect(0, 1.5), Range.CreateExclusiveFrom(1.0, 1.5));
			AreEqual(range.Intersect(3, 4), emptyRange);
			AreEqual(range.Intersect(-2, -1), emptyRange);
			AreEqual(range.Intersect(emptyRange), emptyRange);
			AreEqual(emptyRange.Intersect(range), emptyRange);
			AreEqual(range.Intersect(infiniteRange), range);
			AreEqual(infiniteRange.Intersect(range), range);
			AreEqual(emptyRange.Intersect(infiniteRange), emptyRange);
			AreEqual(infiniteRange.Intersect(emptyRange), emptyRange);

			Throws<ArgumentException>(
				() => range.Intersect(2, 1));
			Throws<ArgumentException>(
				() => range.Intersect(double.PositiveInfinity, double.NegativeInfinity));
			Throws<ArgumentException>(
				() => range.Intersect(2, double.NegativeInfinity));
			Throws<ArgumentException>(
				() => range.Intersect(double.PositiveInfinity, 1));
		}

		[Test]
		public static void TestRangeExtendFrom()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			AreEqual(range.ExtendFrom(null), Range.Create(empty, value2));
			AreEqual(range.ExtendFrom(double.NegativeInfinity), Range.Create(empty, value2));
			Throws<ArgumentException>(() => range.ExtendFrom(double.PositiveInfinity));
			AreEqual(range.ExtendFrom(RangeBoundaryFrom<double?>.Empty), range);
			AreEqual(range.ExtendFrom(0), Range.Create(0, value2));
			AreEqual(range.ExtendFrom(1), range);
			AreEqual(range.ExtendFrom(1.5), range);
			AreEqual(range.ExtendFrom(2), range);
			AreEqual(range.ExtendFrom(3), range);

			range = Range.Create(emptyFrom, emptyTo);
			AreEqual(range.ExtendFrom(null), range);
			AreEqual(range.ExtendFrom(double.NegativeInfinity), range);
			Throws<ArgumentException>(() => range.ExtendFrom(double.PositiveInfinity));
			AreEqual(range.ExtendFrom(RangeBoundaryFrom<double?>.Empty), range);
			AreEqual(range.ExtendFrom(0), range);

			range = Range.CreateExclusive(empty, empty);
			AreEqual(range.ExtendFrom(null), range);
			AreEqual(range.ExtendFrom(double.NegativeInfinity), range);
			Throws<ArgumentException>(() => range.ExtendFrom(double.PositiveInfinity));
			AreEqual(range.ExtendFrom(RangeBoundaryFrom<double?>.Empty), range);
			AreEqual(range.ExtendFrom(0), range);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.ExtendFrom(1), Range.CreateExclusiveTo(1, value2));
			AreEqual(range.ExtendFrom(1.5), range);
			AreEqual(range.ExtendFrom(2), range);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.ExtendFrom(Range.BoundaryFrom<double?>(1)), Range.CreateExclusiveTo(1, value2));
			AreEqual(range.ExtendFrom(Range.BoundaryFromExclusive<double?>(1)), range);
			AreEqual(range.ExtendFrom(Range.BoundaryFromExclusive<double?>(0)), Range.CreateExclusive(0, value2));
		}

		[Test]
		public static void TestRangeExtendTo()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;

			var range = Range.Create(value1, value2);
			AreEqual(range.ExtendTo(null), Range.Create(value1, empty));
			AreEqual(range.ExtendTo(double.PositiveInfinity), Range.Create(value1, empty));
			Throws<ArgumentException>(() => range.ExtendTo(double.NegativeInfinity));
			AreEqual(range.ExtendTo(RangeBoundaryTo<double?>.Empty), range);
			AreEqual(range.ExtendTo(0), range);
			AreEqual(range.ExtendTo(1), range);
			AreEqual(range.ExtendTo(1.5), range);
			AreEqual(range.ExtendTo(2), range);
			AreEqual(range.ExtendTo(3), Range.Create(value1, 3));

			range = Range.Create(emptyFrom, emptyTo);
			AreEqual(range.ExtendTo(null), range);
			AreEqual(range.ExtendTo(double.PositiveInfinity), range);
			Throws<ArgumentException>(() => range.ExtendTo(double.NegativeInfinity));
			AreEqual(range.ExtendTo(RangeBoundaryTo<double?>.Empty), range);
			AreEqual(range.ExtendTo(0), range);

			range = Range.CreateExclusive(empty, empty);
			AreEqual(range.ExtendTo(null), range);
			AreEqual(range.ExtendTo(double.PositiveInfinity), range);
			Throws<ArgumentException>(() => range.ExtendTo(double.NegativeInfinity));
			AreEqual(range.ExtendTo(RangeBoundaryTo<double?>.Empty), range);
			AreEqual(range.ExtendTo(0), range);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.ExtendTo(1), range);
			AreEqual(range.ExtendTo(1.5), range);
			AreEqual(range.ExtendTo(2), Range.CreateExclusiveFrom(value1, 2));

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.ExtendTo(Range.BoundaryTo<double?>(2)), Range.CreateExclusiveFrom(value1, 2));
			AreEqual(range.ExtendTo(Range.BoundaryToExclusive<double?>(2)), range);
			AreEqual(range.ExtendTo(Range.BoundaryToExclusive<double?>(3)), Range.CreateExclusive(value1, 3));
		}

		[Test]
		public static void TestRangeTrimFrom()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;
			var emptyRange = Range.Create(emptyFrom, emptyTo);

			var range = Range.Create(value1, value2);
			AreEqual(range.TrimFrom(null), range);
			AreEqual(range.TrimFrom(double.NegativeInfinity), range);
			Throws<ArgumentException>(() => range.TrimFrom(double.PositiveInfinity));
			AreEqual(range.TrimFrom(RangeBoundaryFrom<double?>.Empty), emptyRange);
			AreEqual(range.TrimFrom(0), range);
			AreEqual(range.TrimFrom(1), range);
			AreEqual(range.TrimFrom(1.5), Range.Create(1.5, value2));
			AreEqual(range.TrimFrom(2), Range.Create(2, value2));
			AreEqual(range.TrimFrom(3), emptyRange);

			range = Range.Create(emptyFrom, emptyTo);
			AreEqual(range.TrimFrom(null), range);
			AreEqual(range.TrimFrom(double.NegativeInfinity), range);
			Throws<ArgumentException>(() => range.TrimFrom(double.PositiveInfinity));
			AreEqual(range.TrimFrom(RangeBoundaryFrom<double?>.Empty), range);
			AreEqual(range.TrimFrom(0), range);

			range = Range.CreateExclusive(empty, empty);
			AreEqual(range.TrimFrom(null), range);
			AreEqual(range.TrimFrom(double.NegativeInfinity), range);
			Throws<ArgumentException>(() => range.TrimFrom(double.PositiveInfinity));
			AreEqual(range.TrimFrom(RangeBoundaryFrom<double?>.Empty), emptyRange);
			AreEqual(range.TrimFrom(0), Range.Create(0, empty));

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.TrimFrom(1), range);
			AreEqual(range.TrimFrom(1.5), Range.CreateExclusiveTo(1.5, value2));
			AreEqual(range.TrimFrom(2), emptyRange);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.TrimFrom(Range.BoundaryFrom<double?>(1)), range);
			AreEqual(range.TrimFrom(Range.BoundaryFrom<double?>(1.5)), Range.CreateExclusiveTo(1.5, value2));
			AreEqual(range.TrimFrom(Range.BoundaryFrom<double?>(2)), emptyRange);
		}

		[Test]
		public static void TestRangeTrimTo()
		{
			double? empty = null;
			double? value1 = 1;
			double? value2 = 2;
			var emptyFrom = RangeBoundaryFrom<double?>.Empty;
			var emptyTo = RangeBoundaryTo<double?>.Empty;
			var emptyRange = Range.Create(emptyFrom, emptyTo);

			var range = Range.Create(value1, value2);
			AreEqual(range.TrimTo(null), range);
			AreEqual(range.TrimTo(double.PositiveInfinity), range);
			Throws<ArgumentException>(() => range.TrimTo(double.NegativeInfinity));
			AreEqual(range.TrimTo(RangeBoundaryTo<double?>.Empty), emptyRange);
			AreEqual(range.TrimTo(0), emptyRange);
			AreEqual(range.TrimTo(1), Range.Create(value1, 1));
			AreEqual(range.TrimTo(1.5), Range.Create(value1, 1.5));
			AreEqual(range.TrimTo(2), range);
			AreEqual(range.TrimTo(3), range);

			range = Range.Create(emptyFrom, emptyTo);
			AreEqual(range.TrimTo(null), range);
			AreEqual(range.TrimTo(double.PositiveInfinity), range);
			Throws<ArgumentException>(() => range.TrimTo(double.NegativeInfinity));
			AreEqual(range.TrimTo(RangeBoundaryTo<double?>.Empty), range);
			AreEqual(range.TrimTo(0), range);

			range = Range.CreateExclusive(empty, empty);
			AreEqual(range.TrimTo(null), range);
			AreEqual(range.TrimTo(double.PositiveInfinity), range);
			Throws<ArgumentException>(() => range.TrimTo(double.NegativeInfinity));
			AreEqual(range.TrimTo(RangeBoundaryTo<double?>.Empty), emptyRange);
			AreEqual(range.TrimTo(0), Range.Create(empty, 0));

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.TrimTo(2), range);
			AreEqual(range.TrimTo(1.5), Range.CreateExclusiveFrom(value1, 1.5));
			AreEqual(range.TrimTo(1), emptyRange);

			range = Range.CreateExclusive(value1, value2);
			AreEqual(range.TrimTo(Range.BoundaryTo<double?>(2)), range);
			AreEqual(range.TrimTo(Range.BoundaryTo<double?>(1.5)), Range.CreateExclusiveFrom(value1, 1.5));
			AreEqual(range.TrimTo(Range.BoundaryTo<double?>(1)), emptyRange);
		}
	}
}