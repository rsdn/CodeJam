using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Ranges
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
	public static class CompositeRangeTests
	{
		[Test]
		public static void TestCreate()
		{
			var range1 = Range.Create(1, 2);
			var range2 = Range.Create(2, 3);

			var keyedRange1 = Range.Create(1, 2, "A");
			var keyedRange2 = Range.Create(2, 3, "B");

			DoesNotThrow(() => CompositeRange.Create<int>());
			DoesNotThrow(() => CompositeRange.Create(range1));
			DoesNotThrow(() => CompositeRange.Create(range1, range2));
			DoesNotThrow(() => CompositeRange.Create(range1, range1));
			DoesNotThrow(() => CompositeRange.Create(range2, range1));
			Throws<ArgumentNullException>(() => CompositeRange.Create<int>(null));

			DoesNotThrow(() => CompositeRange.Create<int, string>());
			DoesNotThrow(() => CompositeRange.Create(keyedRange1));
			DoesNotThrow(() => CompositeRange.Create(keyedRange1, keyedRange2));
			DoesNotThrow(() => CompositeRange.Create(keyedRange1, keyedRange1));
			DoesNotThrow(() => CompositeRange.Create(keyedRange2, keyedRange1));
			Throws<ArgumentNullException>(() => CompositeRange.Create<int, string>(null));

			AreEqual(new CompositeRange<int>(), CompositeRange<int>.Empty);
			AreEqual(CompositeRange.Create<int>(), CompositeRange<int>.Empty);
			AreEqual(CompositeRange.Create(Range<int>.Empty), CompositeRange<int>.Empty);
			AreEqual(CompositeRange.Create(Range<int>.Empty, Range<int>.Empty), CompositeRange<int>.Empty);
			AreEqual(CompositeRange.Create(Range<int>.Infinite), CompositeRange<int>.Infinite);
		}
		[Test]
		public static void TestRangeProperties()
		{
			var range1 = Range.Create(1, 2);
			var range2 = Range.CreateExclusiveFrom(2, 3);
			var range3 = Range.Create(3, 4);
			var empty = Range<int>.Empty;
			var infinite = Range<int>.Infinite;

			var a = new CompositeRange<int>();
			AreEqual(a, CompositeRange<int>.Empty);
			AreEqual(a, CompositeRange.Create(empty, empty));
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, new CompositeRange<int>());
			AreEqual(a.SubRanges.Count, 0);
			AreEqual(a.ContainingRange, empty);
			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = new CompositeRange<int>(infinite);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreEqual(a, CompositeRange<int>.Infinite);
			AreNotEqual(a, CompositeRange.Create(infinite, infinite));
			AreEqual(a.SubRanges.Count, 1);
			AreEqual(a.ContainingRange, infinite);
			AreEqual(a.SubRanges[0], infinite);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = CompositeRange.Create(range1);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreNotEqual(a, CompositeRange.Create(range1, range1));
			AreEqual(a.SubRanges.Count, 1);
			AreEqual(a.ContainingRange, range1);
			AreEqual(a.SubRanges[0], range1);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = CompositeRange.Create(range1, range1);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range1, range1));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, range1);
			AreEqual(a.SubRanges[0], range1);
			AreEqual(a.SubRanges[1], range1);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsMerged);

			a = CompositeRange.Create(range1, range2);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range2, range1));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, Range.Create(1, 3));
			AreEqual(a.SubRanges[0], range1);
			AreEqual(a.SubRanges[1], range2);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsMerged);
			IsFalse(a.SubRanges[0].HasIntersection(a.SubRanges[1]));
			IsTrue(a.SubRanges[0].To.GetComplementation() == a.SubRanges[1].From);

			a = CompositeRange.Create(range1, range3);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range3, range1));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, Range.Create(1, 4));
			AreEqual(a.SubRanges[0], range1);
			AreEqual(a.SubRanges[1], range3);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);
		}

		[Test]
		public static void TestKeyedRangeProperties()
		{
			var range1A = Range.Create(1, 2, "A");
			var range1B = Range.Create(1, 2, "B");
			var range2C = Range.CreateExclusiveFrom(2, 3, "C");
			var range3A = Range.Create(3, 4).WithKey("A");
			var empty = Range<int>.Empty.WithKey((string)null);
			var infinite = Range<int>.Infinite.WithKey((string)null);

			var a = new CompositeRange<int,string>();
			AreEqual(a, CompositeRange.Create(empty, empty));
			AreEqual(a, new CompositeRange<int, string>());
			AreEqual(a.SubRanges.Count, 0);
			AreEqual(a.ContainingRange, Range<int>.Empty);
			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = new CompositeRange<int,string>(infinite);
			AreNotEqual(a, new CompositeRange<int, string>());
			AreEqual(a, CompositeRange.Create(infinite));
			AreNotEqual(a, CompositeRange.Create(infinite, infinite));
			AreEqual(a.SubRanges.Count, 1);
			AreEqual(a.ContainingRange, infinite.WithoutKey());
			AreEqual(a.SubRanges[0], infinite);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = CompositeRange.Create(range1A);
			AreNotEqual(a, new CompositeRange<int, string>());
			AreNotEqual(a, new CompositeRange<int, string>(infinite));
			AreNotEqual(a, CompositeRange.Create(range1A, range1A));
			AreNotEqual(a, CompositeRange.Create(range1B));
			AreEqual(a.SubRanges.Count, 1);
			AreEqual(a.ContainingRange, range1A.WithoutKey());
			AreEqual(a.SubRanges[0], range1A);
			AreNotEqual(a.SubRanges[0], range1B);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = CompositeRange.Create(range1A, range1B);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range1B, range1A));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, range1A.WithoutKey());
			AreEqual(a.SubRanges[0], range1A);
			AreEqual(a.SubRanges[1], range1B);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsMerged);

			a = CompositeRange.Create(range1A, range2C);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range2C, range1A));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, Range.Create(1, 3));
			AreEqual(a.SubRanges[0], range1A);
			AreEqual(a.SubRanges[1], range2C);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsMerged);
			IsFalse(a.SubRanges[0].HasIntersection(a.SubRanges[1]));
			IsTrue(a.SubRanges[0].To.GetComplementation() == a.SubRanges[1].From);

			a = CompositeRange.Create(range1A, range3A);
			AreNotEqual(a, CompositeRange<int>.Empty);
			AreNotEqual(a, CompositeRange<int>.Infinite);
			AreEqual(a, CompositeRange.Create(range3A, range1A));
			AreEqual(a.SubRanges.Count, 2);
			AreEqual(a.ContainingRange, Range.Create(1, 4));
			AreEqual(a.SubRanges[0], range1A);
			AreEqual(a.SubRanges[1], range3A);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsMerged);
		}

		[Test]
		public static void TestCompositeRangeArgsOrder()
		{
			var seek = new Random().Next();
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seek: {seek} (use the seek to reproduce test results).");
			var rnd = new Random(seek);

			var ranges = new[]
			{
				Range<int>.Empty,
				Range.Create(1, 2),
				Range.Create(1, 2),
				Range.Create(3, 4),
				Range.Create(3, 5),
				Range<int>.Empty,
				Range<int>.Empty,
				Range<int>.Empty,
				Range.CreateExclusive(4, 6),
				Range.CreateExclusiveTo(7, 8),
				Range.CreateExclusiveTo(3, 5),
				Range.CreateExclusive(3, 5),
				Range.CreateExclusiveFrom(3, 5),
				Range.Create(0, 6),
				Range<int>.Empty,
				Range<int>.Empty,
				Range<int>.Infinite
			};

			var compositeRange = new CompositeRange<int>(ranges);
			AreEqual(
				compositeRange.ToString(),
				"(-∞..+∞): { (-∞..+∞); [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }");

			compositeRange = ranges.OrderBy(r => rnd.Next()).ToCompositeRange();
			AreEqual(
				compositeRange.ToString(),
				"(-∞..+∞): { (-∞..+∞); [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }");

			compositeRange = ranges.Take(ranges.Length - 1).ToCompositeRange();
			AreEqual(
				compositeRange.ToString(),
				"[0..8): { [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }");

			compositeRange = ranges.Take(ranges.Length - 1).OrderBy(r => rnd.Next()).ToCompositeRange();
			AreEqual(
				compositeRange.ToString(),
				"[0..8): { [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }");

			compositeRange = new CompositeRange<int>(ranges.Last());
			AreEqual(compositeRange.ToString(), "(-∞..+∞): { (-∞..+∞) }");

			compositeRange = new CompositeRange<int>(ranges.First());
			AreEqual(compositeRange.ToString(), "∅");

			compositeRange = new CompositeRange<int>(ranges.Take(0));
			AreEqual(compositeRange.ToString(), "∅");

			compositeRange = new CompositeRange<int>();
			AreEqual(compositeRange.ToString(), "∅");
		}

		[Test]
		public static void TestCompositeRangeArgsOrderWithKeys()
		{
			var seek = new Random().Next();
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seek: {seek} (use the seek to reproduce test results).");
			var rnd = new Random(seek);

			var emtpy = Range.Create(RangeBoundaryFrom<int>.Empty, RangeBoundaryTo<int>.Empty, "E");
			var infinite = Range.Create(
				RangeBoundaryFrom<int>.NegativeInfinity,
				RangeBoundaryTo<int>.PositiveInfinity, "I");
			// Equal ranges should use same keys to provide repeatable results after
			// ranges array will be shuffled.
			var ranges = new[]
			{
				emtpy,
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "A"),
				Range.Create(3, 4, "B"),
				Range.Create(3, 5, "C"),
				emtpy,
				emtpy,
				emtpy,
				Range.CreateExclusive(4, 6, "D"),
				Range.CreateExclusiveTo(7, 8, "F"),
				Range.CreateExclusiveTo(3, 5, "G"),
				Range.CreateExclusive(3, 5, "H"),
				Range.CreateExclusiveFrom(3, 5, "K"),
				Range.Create(0, 6, (string)null),
				emtpy,
				emtpy,
				infinite
			};

			var compositeRange = new CompositeRange<int, string>(ranges);
			AreEqual(
				compositeRange.ToString(),
				"(-∞..+∞): { 'I':(-∞..+∞); '':[0..6]; 'A':[1..2]; 'A':[1..2]; 'B':[3..4]; " +
					"'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }");

			compositeRange = new CompositeRange<int, string>(ranges.OrderBy(r => rnd.Next()));
			AreEqual(
				compositeRange.ToString(),
				"(-∞..+∞): { 'I':(-∞..+∞); '':[0..6]; 'A':[1..2]; 'A':[1..2]; 'B':[3..4]; " +
					"'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }");

			compositeRange = new CompositeRange<int, string>(ranges.Take(ranges.Length - 1));
			AreEqual(
				compositeRange.ToString(),
				"[0..8): { '':[0..6]; 'A':[1..2]; 'A':[1..2]; 'B':[3..4]; " +
					"'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }");

			compositeRange = new CompositeRange<int, string>(ranges.Take(ranges.Length - 1).OrderBy(r => rnd.Next()));
			AreEqual(
				compositeRange.ToString(),
				"[0..8): { '':[0..6]; 'A':[1..2]; 'A':[1..2]; 'B':[3..4]; " +
					"'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }");

			compositeRange = new CompositeRange<int, string>(ranges.Last());
			AreEqual(compositeRange.ToString(), "(-∞..+∞): { 'I':(-∞..+∞) }");

			compositeRange = new CompositeRange<int, string>(ranges.First());
			AreEqual(compositeRange.ToString(), "∅");

			compositeRange = new CompositeRange<int, string>(ranges.Take(0));
			AreEqual(compositeRange.ToString(), "∅");

			compositeRange = new CompositeRange<int, string>();
			AreEqual(compositeRange.ToString(), "∅");
		}

		[Test]
		public static void TestCompositeRangEqualsWithKeysSimple()
		{
			var seek = new Random().Next();
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seek: {seek} (use the seek to reproduce test results).");
			var rnd = new Random(seek);

			var range = new[]
			{
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"),
				Range.Create(1, 2, "B")
			}.ToCompositeRange();

			var rangeRandomReordered = new[]
			{
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"),
				Range.Create(1, 2, "B")
			}.OrderBy(r=>rnd.Next()).ToCompositeRange();

			var rangeReordered = new[]
			{
				Range.Create(1, 2, "C"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "A")
			}.ToCompositeRange();

			var rangeOtherKeys = new[]
			{
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C")
			}.ToCompositeRange();

			var rangeLessKeys = CompositeRange.Create(
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"));

			AreEqual(
				range.ToString(),
				"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2] }");
			AreEqual(
				rangeReordered.ToString(),
				"[1..2]: { 'C':[1..2]; 'B':[1..2]; 'B':[1..2]; 'A':[1..2] }");
			AreEqual(
				rangeOtherKeys.ToString(),
				"[1..2]: { 'A':[1..2]; 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }");
			AreEqual(
				rangeLessKeys.ToString(),
				"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }");

			AreEqual(range, range);
			AreEqual(rangeRandomReordered, rangeRandomReordered);
			AreEqual(rangeReordered, rangeReordered);
			AreEqual(rangeOtherKeys, rangeOtherKeys);
			AreEqual(rangeLessKeys, rangeLessKeys);

			AreEqual(range, rangeReordered);
			AreEqual(range, rangeRandomReordered);
			AreNotEqual(range, rangeOtherKeys);
			AreNotEqual(range, rangeLessKeys);
			AreEqual(rangeReordered, rangeRandomReordered);
		}

		[Test]
		public static void TestCompositeRangEqualsWithKeysComplex()
		{
			var ranges = new[]
			{
				Range.Create(0, 1, "X"),
				Range.Create(0, 1, "Y"),
				Range.Create(1, 1, "Z"),
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"),
				Range.Create(1, 2, "B"),
				Range.Create(2, 3, "X"),
				Range.Create(2, 3, "Y"),
				Range.Create(2, 2, "Z")
			};
			var ranges3 = new[]
			{
				Range.Create(0, 1, "X"),
				Range.Create(0, 1, "Y"),
				Range.Create(1, 1, "Z"),
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"),
				Range.Create(1, 2, "B"),
				Range.Create(2, 3, "Y2"),
				Range.Create(2, 3, "Y2"),
				Range.Create(2, 2, "Z")
			};
			var ranges4 = new[]
			{
				Range.Create(0, 1, "X"),
				Range.Create(0, 1, "Y"),
				Range.Create(1, 1, "Z"),
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "A"),
				Range.Create(1, 2, "B"),
				Range.Create(1, 2, "C"),
				Range.Create(2, 3, "X"),
				Range.Create(2, 3, "Y"),
				Range.Create(2, 2, "Z")
			};

			var ranges2 = ranges.Reverse().ToArray();

			var compositeRange = new CompositeRange<int, string>(ranges);
			var compositeRange2 = new CompositeRange<int, string>(ranges2);
			var compositeRange3 = new CompositeRange<int, string>(ranges3);
			var compositeRange4 = new CompositeRange<int, string>(ranges4);

			AreEqual(
				compositeRange.ToString(),
				"[0..3]: { 'X':[0..1]; 'Y':[0..1]; 'Z':[1..1]; 'A':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'Z':[2..2]; 'X':[2..3]; 'Y':[2..3] }");
			AreEqual(
				compositeRange2.ToString(),
				"[0..3]: { 'Y':[0..1]; 'X':[0..1]; 'Z':[1..1]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'A':[1..2]; 'Z':[2..2]; 'Y':[2..3]; 'X':[2..3] }");
			AreEqual(
				compositeRange3.ToString(),
				"[0..3]: { 'X':[0..1]; 'Y':[0..1]; 'Z':[1..1]; 'A':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'Z':[2..2]; 'Y2':[2..3]; 'Y2':[2..3] }");
			AreEqual(
				compositeRange4.ToString(),
				"[0..3]: { 'X':[0..1]; 'Y':[0..1]; 'Z':[1..1]; 'A':[1..2]; 'A':[1..2]; 'B':[1..2]; 'C':[1..2]; 'Z':[2..2]; 'X':[2..3]; 'Y':[2..3] }");

			AreEqual(compositeRange, compositeRange);
			AreEqual(compositeRange2, compositeRange2);
			AreEqual(compositeRange3, compositeRange3);
			AreEqual(compositeRange4, compositeRange4);

			AreEqual(compositeRange, compositeRange2);
			AreNotEqual(compositeRange, compositeRange3);
			AreNotEqual(compositeRange, compositeRange4);
			AreNotEqual(compositeRange2, compositeRange3);
			AreNotEqual(compositeRange2, compositeRange4);
			AreNotEqual(compositeRange3, compositeRange4);
		}
	}
}