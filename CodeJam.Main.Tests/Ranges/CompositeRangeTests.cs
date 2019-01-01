#if !LESSTHAN_NET45
// NET 4.0 uses binary array sorting instead of introspection sort, most of tests belongs on order of elements to be preserved
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CodeJam.Strings;

using JetBrains.Annotations;

using NUnit.Framework;

using static NUnit.Framework.Assert;

using static CodeJam.Ranges.RangeTestHelpers;

namespace CodeJam.Ranges
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public static class CompositeRangeTests
	{
		#region Parse helpers
		private static CompositeRange<T> ParseCompositeRange<T>(
			[NotNull] string value,
			Func<string, T> parseValueCallback)
		{
			if (value == RangeInternal.EmptyString)
				return CompositeRange<T>.Empty;

			var boundaryAndRange = value.Split(new[] { CompositeRangeInternal.PrefixString }, 2, StringSplitOptions.None);
			var boundary = ParseRange(boundaryAndRange[0], parseValueCallback);

			var result = boundaryAndRange[1]
				.Substring(0, boundaryAndRange[1].Length - CompositeRangeInternal.SuffixString.Length)
				.Split(new[] { CompositeRangeInternal.SeparatorString }, StringSplitOptions.None)
				.Select(s => ParseRange(s, parseValueCallback))
				.ToCompositeRange();

			AreEqual(boundary, result.ContainingRange);
			return result;
		}

		private static CompositeRange<T, TKey> ParseCompositeRange<T, TKey>(
			[NotNull] string value,
			Func<string, T> parseValueCallback,
			Func<string, TKey> parseKeyCallback)
		{
			if (value == RangeInternal.EmptyString)
				return CompositeRange<T, TKey>.Empty;

			var boundaryAndRange = value.Split(new[] { CompositeRangeInternal.PrefixString }, 2, StringSplitOptions.None);
			var boundary = ParseRange(boundaryAndRange[0], parseValueCallback);

			var result = boundaryAndRange[1]
				.Substring(0, boundaryAndRange[1].Length - CompositeRangeInternal.SuffixString.Length)
				.Split(new[] { CompositeRangeInternal.SeparatorString }, StringSplitOptions.None)
				.Select(s => ParseRange(s, parseValueCallback, parseKeyCallback))
				.ToCompositeRange();

			AreEqual(boundary, result.ContainingRange);
			return result;
		}

		public static Range<double?> ParseRangeDouble(string value) =>
			ParseRange(value, s => (double?)double.Parse(s, CultureInfo.InvariantCulture));

		public static Range<int?> ParseRangeInt32(string value) =>
			ParseRange(value, s => (int?)int.Parse(s, CultureInfo.InvariantCulture));

		public static CompositeRange<double?> ParseCompositeRangeDouble([NotNull] string value) =>
			ParseCompositeRange(value, s => (double?)double.Parse(s, CultureInfo.InvariantCulture));

		public static CompositeRange<int?, string> ParseCompositeKeyedRangeInt32([NotNull] string value) =>
			ParseCompositeRange(value, s => (int?)int.Parse(s, CultureInfo.InvariantCulture), s => s.IsNullOrEmpty() ? null : s);
		#endregion

		[Test]
		public static void TestCompositeRangeUseCase()
		{
			var range = Range.Create(1, 2, "Key1")
				.ToCompositeRange()
				.Union(Range.Create(5, 10, "Key2"))
				.Except(Range.Create(7, 8))
				.Intersect(2, 9);

			AreEqual(range.ToInvariantString(), "[2..9]: { 'Key1':[2..2]; 'Key2':[5..7); 'Key2':(8..9] }");
			AreEqual(
				range.SubRanges.Select(r => r.Key).Distinct().Join(";"),
				"Key1;Key2");

			AreEqual(
				range.WithoutKeys().Union(Range.Create(1, 10)).ToInvariantString(),
				"[1..10]: { [1..10] }");

			range = range.Union(Range.Create(7, 8, "Key1"));
			AreEqual(range.ToInvariantString(), "[2..9]: { 'Key1':[2..2]; 'Key2':[5..7); 'Key1':[7..8]; 'Key2':(8..9] }");

			range = range
				.Except(7, 8)
				.Union(Range.Create(7, 8, "Key2"));
			AreEqual(range.ToInvariantString(), "[2..9]: { 'Key1':[2..2]; 'Key2':[5..9] }");
		}

		[Test]
		[TestCase(
			"[1..2]: { [1..2] }",
			"[1..2]: { [1..2] }")]
		[TestCase(
			"[1..2]: { [1..2]; [1..2]; [1..2] }",
			"[1..2]: { [1..2]; [1..2]; [1..2] }")]
		[TestCase(
			"[1..3]: { [2..3]; [1..2]; [1..3]; ∅ }",
			"[1..3]: { [1..2]; [1..3]; [2..3] }")]
		[TestCase(
			"[1..2]: { (1..2); (1..2]; [1..2); [1..2] }",
			"[1..2]: { [1..2); [1..2]; (1..2); (1..2] }")]
		[TestCase(
			"(0..4): { [2..3]; [3..4); (0..1); [1..2] }",
			"(0..4): { (0..1); [1..2]; [2..3]; [3..4) }")]
		[TestCase(
			"∅: { ∅; ∅; ∅; ∅ }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { (-∞..3]; [3..+∞); [1..5]; [4..5) }",
			"(-∞..+∞): { (-∞..3]; [1..5]; [3..+∞); [4..5) }")]
		[TestCase(
			"[0..8): { [3..4]; [3..5); [0..6]; ∅; ∅; ∅; [1..2]; (4..6); [7..8); [1..2]; [3..5]; (3..5); ∅; ∅; (3..5] }",
			"[0..8): { [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }")]
		[TestCase(
			"(-∞..+∞): { [3..4]; [3..5); [0..6]; ∅; ∅; ∅; [1..2]; (4..6); (-∞..+∞); [7..8); [1..2]; [3..5]; (3..5); ∅; ∅; (3..5] }",
			"(-∞..+∞): { (-∞..+∞); [0..6]; [1..2]; [1..2]; [3..4]; [3..5); [3..5]; (3..5); (3..5]; (4..6); [7..8) }")]
		public static void TestCompositeRangeCreate(
			[NotNull] string ranges, string expected)
		{
			var rnd = TestTools.GetTestRandom();

			var compositeRange = ParseCompositeRangeDouble(ranges);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);

			var compositeRange2 = compositeRange.SubRanges.Shuffle(rnd).ToCompositeRange();
			AreEqual(compositeRange2.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		[TestCase(
			"[1..2]: { '':[1..2] }",
			"[1..2]: { '':[1..2] }")]
		[TestCase(
			"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }",
			"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }")]
		[TestCase(
			"[0..3]: { 'C':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'B':[1..2]; 'A':[1..2]; ' ':[2..3]; ' ':[0..1] }",
			"[0..3]: { ' ':[0..1]; 'C':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'B':[1..2]; 'A':[1..2]; ' ':[2..3] }")]
		[TestCase(
			"[1..3]: { 'A':[2..3]; 'B':[1..2]; 'C':[1..3]; 'D':∅ }",
			"[1..3]: { 'B':[1..2]; 'C':[1..3]; 'A':[2..3] }")]
		[TestCase(
			"[1..2]: { 'A':(1..2); 'B':(1..2]; 'C':[1..2); 'D':[1..2] }",
			"[1..2]: { 'C':[1..2); 'D':[1..2]; 'A':(1..2); 'B':(1..2] }")]
		[TestCase(
			"(0..4): { 'A':[2..3]; 'B':[3..4); 'C':(0..1); 'D':[1..2] }",
			"(0..4): { 'C':(0..1); 'D':[1..2]; 'A':[2..3]; 'B':[3..4) }")]
		[TestCase(
			"∅: { 'A':∅; 'B':∅; 'C':∅; 'D':∅ }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..3]; 'B':[3..+∞); 'C':[1..5]; 'D':[4..5) }",
			"(-∞..+∞): { 'A':(-∞..3]; 'C':[1..5]; 'B':[3..+∞); 'D':[4..5) }")]
		[TestCase(
			"[0..8): { 'A':[3..4]; 'B':[3..5); 'C':[0..6]; 'D':∅; 'E':∅; 'F':∅; 'G':[1..2]; 'H':(4..6); 'I':[7..8); 'J':[1..2]; 'K':[3..5]; 'L':(3..5); 'M':∅; 'N':∅; 'O':(3..5] }",
			"[0..8): { 'C':[0..6]; 'G':[1..2]; 'J':[1..2]; 'A':[3..4]; 'B':[3..5); 'K':[3..5]; 'L':(3..5); 'O':(3..5]; 'H':(4..6); 'I':[7..8) }"
			)]
		[TestCase(
			"(-∞..+∞): { 'A':[3..4]; 'B':[3..5); 'C':[0..6]; 'D':∅; 'E':∅; 'F':∅; 'G':[1..2]; 'H':(4..6); '':(-∞..+∞); 'I':[7..8); 'J':[1..2]; 'K':[3..5]; 'L':(3..5); 'M':∅; 'N':∅; 'O':(3..5] }",
			"(-∞..+∞): { '':(-∞..+∞); 'C':[0..6]; 'G':[1..2]; 'J':[1..2]; 'A':[3..4]; 'B':[3..5); 'K':[3..5]; 'L':(3..5); 'O':(3..5]; 'H':(4..6); 'I':[7..8) }"
			)]
		public static void TestCompositeRangeCreateWithKey(
			string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		public static void TestCompositeRangeRangeProperties()
		{
			var range1 = Range.Create(1, 2);
			var range2 = Range.CreateExclusiveFrom(2, 3);
			var range3 = Range.Create(3, 4);
			var empty = Range<int>.Empty;
			var infinite = Range<int>.Infinite;

			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once AssignNullToNotNullAttribute
			Throws<ArgumentNullException>(() => new CompositeRange<int>(null));

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
		public static void TestCompositeRangeWithKeyProperties()
		{
			var range1A = Range.Create(1, 2, "A");
			var range1B = Range.Create(1, 2, "B");
			var range2C = Range.CreateExclusiveFrom(2, 3, "C");
			var range3A = Range.Create(3, 4).WithKey("A");
			var empty = Range<int>.Empty.WithKey((string)null);
			var infinite = Range<int>.Infinite.WithKey((string)null);

			var a = new CompositeRange<int, string>();
			AreEqual(a, CompositeRange.Create(empty, empty));
			AreEqual(a, new CompositeRange<int, string>());
			AreEqual(a.SubRanges.Count, 0);
			AreEqual(a.ContainingRange, Range<int>.Empty);
			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsTrue(a.IsMerged);

			a = new CompositeRange<int, string>(infinite);
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
		[TestCase(
			"[1..2]: { [1..2] }",
			"[1..2]: { [1..2] }",
			true)]
		[TestCase(
			"[1..2]: { [1..2] }",
			"[1..3]: { [1..3] }",
			false)]
		[TestCase(
			"[1..2]: { [1..2] }",
			"[1..2]: { [1..2]; [1..2] }",
			false)]
		[TestCase(
			"∅: { ∅; ∅; ∅; ∅ }",
			"∅",
			true)]
		[TestCase(
			"[1..5]: { [1..2]; [3..5] }",
			"[1..5]: { [1..2]; [3..5] }",
			true)]
		[TestCase(
			"[1..5]: { [1..2]; [3..5] }",
			"[1..5]: { [1..2]; (3..5] }",
			false)]
		[TestCase(
			"[1..2]: { [1..2] }",
			"(-∞..+∞): { (-∞..+∞) }",
			false)]
		[TestCase(
			"[1..5]: { [1..2]; [3..5] }",
			"∅",
			false)]
		public static void TestCompositeRangeEquality(string range1, string range2, bool expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(range1);
			var compositeRange2 = ParseCompositeRangeDouble(range2);

			AreEqual(compositeRange1.Equals(compositeRange2), expected);
		}

		[Test]
		[TestCase(
			"[1..2]: { 'A':[1..2] }",
			"[1..2]: { 'A':[1..2] }",
			true)]
		[TestCase(
			"[1..2]: { 'A':[1..2] }",
			"[1..2]: { 'B':[1..2] }",
			false)]
		[TestCase(
			"[1..2]: { 'A':[1..2] }",
			"[1..3]: { 'A':[1..3] }",
			false)]
		[TestCase(
			"[1..2]: { 'A':[1..2] }",
			"[1..2]: { 'A':[1..2]; 'A':[1..2] }",
			false)]
		[TestCase(
			"[1..5]: { '':[1..2]; '':[3..5] }",
			"[1..5]: { '':[1..2]; '':[3..5] }",
			true)]
		[TestCase(
			"[1..5]: { '':[1..2]; '':[3..5] }",
			"[1..5]: { '':[1..2]; '':(3..5] }",
			false)]
		[TestCase(
			"[1..2]: { '':[1..2] }",
			"(-∞..+∞): { 'A':(-∞..+∞) }",
			false)]
		[TestCase(
			"∅: { 'A':∅; 'B':∅; 'C':∅; 'D':∅ }",
			"∅",
			true)]
		[TestCase(
			"[1..5]: { '':[1..2]; '':[3..5] }",
			"∅",
			false)]
		[TestCase(
			"[1..2]: { 'A':[1..2]; 'A':[1..2]; 'A':[1..2]; '':[1..2]; 'C':[1..2]; '':[1..2]; 'C':[1..2]; 'B':[1..2] }",
			"[1..2]: { 'C':[1..2]; 'A':[1..2]; '':[1..2]; 'B':[1..2]; '':[1..2]; 'A':[1..2]; 'A':[1..2]; 'C':[1..2] }",
			true)]
		[TestCase(
			"[1..2]: { 'A':[1..2]; 'A':[1..2]; 'A':[1..2]; '':[1..2]; 'C':[1..2]; '':[1..2]; 'C':[1..2]; 'B':[1..2] }",
			"[1..2]: { 'A':[1..2]; 'A':[1..2]; 'C':[1..2]; '':[1..2]; 'C':[1..2]; '':[1..2]; 'C':[1..2]; 'B':[1..2] }",
			false)]
		[TestCase(
			"[0..8): { '':[0..6]; 'A':[1..2]; 'B':[1..2]; 'B':[3..4]; 'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }",
			"[0..8): { '':[0..6]; 'B':[1..2]; 'A':[1..2]; 'B':[3..4]; 'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }",
			true)]
		[TestCase(
			"[0..8): { '':[0..6]; 'A':[1..2]; 'B':[1..2]; 'B':[3..4]; 'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'F':[7..8) }",
			"[0..8): { '':[0..6]; 'B':[1..2]; 'A':[1..2]; 'B':[3..4]; 'G':[3..5); 'C':[3..5]; 'H':(3..5); 'K':(3..5]; 'D':(4..6); 'G':[7..8) }",
			false)]
		public static void TestCompositeRangeWithKeyEquality(string range1, string range2, bool expected)
		{
			var rnd = TestTools.GetTestRandom();

			var compositeRange1 = ParseCompositeKeyedRangeInt32(range1);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(range2);

			// Shuffle keys
			var compositeRange1Rnd = compositeRange1.SubRanges.Shuffle(rnd).ToCompositeRange();
			var compositeRange2Rnd = compositeRange2.SubRanges.Shuffle(rnd).ToCompositeRange();

			AreEqual(compositeRange1.Equals(compositeRange1Rnd), true);
			AreEqual(compositeRange2.Equals(compositeRange2Rnd), true);

			AreEqual(compositeRange1.Equals(compositeRange2), expected);
			AreEqual(compositeRange1Rnd.Equals(compositeRange2), expected);
			AreEqual(compositeRange2Rnd.Equals(compositeRange1), expected);
			AreEqual(compositeRange2Rnd.Equals(compositeRange1Rnd), expected);
		}

		[Test]
		[TestCase(
			"[1..2]: { [1..2] }",
			"[1..2]: { [1..2] }")]
		[TestCase(
			"[1..2]: { [1..2]; [1..2]; [1..2] }",
			"[1..2]: { [1..2] }")]
		[TestCase(
			"[1..3]: { [2..3]; [1..2]; [1..3]; ∅ }",
			"[1..3]: { [1..3] }")]
		[TestCase(
			"[1..2]: { (1..2); (1..2]; [1..2); [1..2] }",
			"[1..2]: { [1..2] }")]
		[TestCase(
			"(0..4): { [2..3); [3..4); (0..1); [1..2] }",
			"(0..4): { (0..4) }")]
		[TestCase(
			"(0..4): { [2..3); (3..4); (0..1); [1..2] }",
			"(0..4): { (0..3); (3..4) }")]
		[TestCase(
			"∅: { ∅; ∅; ∅; ∅ }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { (-∞..3]; [3..+∞); [1..5]; [4..5) }",
			"(-∞..+∞): { (-∞..+∞) }")]
		[TestCase(
			"[0..8): { [3..4]; [3..5); [0..6]; ∅; ∅; ∅; [1..2]; (4..6); [7..8); [1..2]; [3..5]; (3..5); ∅; ∅; (3..5] }",
			"[0..8): { [0..6]; [7..8) }")]
		[TestCase(
			"(-∞..+∞): { [3..4]; [3..5); [0..6]; ∅; ∅; ∅; [1..2]; (4..6); (-∞..+∞); [7..8); [1..2]; [3..5]; (3..5); ∅; ∅; (3..5] }",
			"(-∞..+∞): { (-∞..+∞) }")]
		public static void TestCompositeRangeMerge(string ranges, string expected)
		{
			var compositeRange = ParseCompositeRangeDouble(ranges);
			AreEqual(compositeRange.Merge().ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		[TestCase(
			"[1..2]: { '':[1..2] }",
			"[1..2]: { '':[1..2] }")]
		[TestCase(
			"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }",
			"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'C':[1..2] }")]
		[TestCase(
			"[1..2]: { 'A':[1..2]; 'B':[1..2]; 'A':[1..2] }",
			"[1..2]: { 'A':[1..2]; 'B':[1..2] }")]
		[TestCase(
			"[0..3]: { 'C':[1..2]; 'B':[1..2]; 'C':[1..2]; 'B':[1..2]; 'B':[1..2]; 'A':[1..2]; ' ':[2..3]; ' ':[0..1] }",
			"[0..3]: { ' ':[0..1]; 'C':[1..2]; 'B':[1..2]; 'A':[1..2]; ' ':[2..3] }")]
		[TestCase(
			"[1..3]: { 'A':[2..3]; 'A':[1..2); 'C':[1..3]; 'D':∅ }",
			"[1..3]: { 'A':[1..3]; 'C':[1..3] }")]
		[TestCase(
			"[1..3]: { 'A':(2..3); 'A':[1..2); 'C':[1..3]; 'D':∅ }",
			"[1..3]: { 'A':[1..2); 'C':[1..3]; 'A':(2..3) }")]
		[TestCase(
			"[1..2]: { 'A':(1..2); 'B':(1..2]; 'C':[1..2); 'D':[1..2] }",
			"[1..2]: { 'C':[1..2); 'D':[1..2]; 'A':(1..2); 'B':(1..2] }")]
		[TestCase(
			"[1..2]: { 'A':(1..2); 'B':(1..2]; 'A':[1..2); 'C':[1..2] }",
			"[1..2]: { 'A':[1..2); 'C':[1..2]; 'B':(1..2] }")]
		[TestCase(
			"(0..4): { 'A':[2..3]; 'B':[3..4); 'C':(0..1); 'D':[1..2] }",
			"(0..4): { 'C':(0..1); 'D':[1..2]; 'A':[2..3]; 'B':[3..4) }")]
		[TestCase(
			"∅: { 'A':∅; 'B':∅; 'C':∅; 'D':∅ }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..3]; 'B':[3..+∞); 'C':[1..5]; 'D':[4..5) }",
			"(-∞..+∞): { 'A':(-∞..3]; 'C':[1..5]; 'B':[3..+∞); 'D':[4..5) }")]
		[TestCase(
			"[0..8): { 'A':[3..4]; 'B':[3..5); 'A':[0..6]; 'B':∅; 'A':∅; 'B':∅; 'A':[1..2]; 'B':(4..6); 'A':[7..8); 'B':[1..2]; 'A':[3..5]; 'B':(3..5); 'A':∅; 'B':∅; 'A':(3..5] }",
			"[0..8): { 'A':[0..6]; 'B':[1..2]; 'B':[3..6); 'A':[7..8) }")]
		public static void TestCompositeRangeMergeWithKey(
			string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			AreEqual(compositeRange.Merge().ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		public static void TestCompositeRangeWithOrWithoutKeys()
		{
			var compositeRange = Enumerable
				.Range(1, 2)
				.ToCompositeRange(i => i - 1, i => i + 1);
			AreEqual(compositeRange.ToString(), "[0..3]: { '1':[0..2]; '2':[1..3] }");

			var compositeRange2 = compositeRange.WithKeys(i => "A" + i);
			AreEqual(compositeRange2.ToString(), "[0..3]: { 'A1':[0..2]; 'A2':[1..3] }");

			var compositeRange3 = compositeRange2.WithoutKeys();
			AreEqual(compositeRange3.ToString(), "[0..3]: { [0..2]; [1..3] }");

			var compositeRange4 = compositeRange3.WithKeys("A");
			AreEqual(compositeRange4.ToString(), "[0..3]: { 'A':[0..2]; 'A':[1..3] }");

			var compositeRange5 = compositeRange4.WithKeys("B");
			AreEqual(compositeRange5.ToString(), "[0..3]: { 'B':[0..2]; 'B':[1..3] }");
		}

		[Test]
		public static void TestCompositeRangeWithValues()
		{
			var compositeRange = Enumerable
				.Range(1, 2)
				.ToCompositeRange(i => i - 1, i => i + 1);
			AreEqual(compositeRange.ToString(), "[0..3]: { '1':[0..2]; '2':[1..3] }");

			var compositeRange2 = compositeRange.WithValues(i => "A" + i);
			AreEqual(compositeRange2.ToString(), "[A0..A3]: { '1':[A0..A2]; '2':[A1..A3] }");

			var compositeRange3 = compositeRange2.WithValues(i => i, i => "B" + i.Substring(1));
			AreEqual(compositeRange3.ToString(), "[A0..B3]: { '1':[A0..B2]; '2':[A1..B3] }");

			var compositeRange4 = compositeRange3.WithoutKeys().WithValues(i => int.Parse(i.Substring(1)));
			AreEqual(compositeRange4.ToString(), "[0..3]: { [0..2]; [1..3] }");

			AreEqual(compositeRange4, compositeRange.WithoutKeys());
		}

		[Test]
		[TestCase(
			"∅",
			"(-∞..+∞): { (-∞..+∞) }")]
		[TestCase(
			"[1..2]: { '':[1..2] }",
			"(-∞..+∞): { (-∞..1); (2..+∞) }")]
		[TestCase(
			"(0..2]: { 'A':(0..1); 'B':(1..2] }",
			"(-∞..+∞): { (-∞..0]; [1..1]; (2..+∞) }")]
		[TestCase(
			"(0..2]: { 'A':(0..1); 'B':[1..2] }",
			"(-∞..+∞): { (-∞..0]; (2..+∞) }")]
		[TestCase(
			"(0..2]: { 'A':(0..1); 'B':(1..2]; 'C':[1..2) }",
			"(-∞..+∞): { (-∞..0]; (2..+∞) }")]
		public static void TestCompositeRangeComplementation(string ranges, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges);
			var compositeRange2 = ParseCompositeRangeDouble(expected).WithValues(i => (int?)i);
			var compositeRange1A = compositeRange1.WithoutKeys().Merge();

			AreEqual(compositeRange1.GetComplementation().ToInvariantString(), expected);
			AreEqual(compositeRange2.GetComplementation(), compositeRange1A);

			if (compositeRange1.SubRanges.Count == 1)
			{
				AreEqual(compositeRange1.ContainingRange.GetComplementation().ToInvariantString(), expected);
				AreEqual(compositeRange1.SubRanges[0].GetComplementation().ToInvariantString(), expected);
			}
		}

		[Test]
		[TestCase(
			"[1..2]: { '':[1..2] }",
			"[1..2]: { '':[1..2] }")]
		[TestCase(
			"(0..2]: { 'A':(0..1); 'B':(1..2]; 'C':[1..2) }",
			"[1..2]: { 'C':[1..1]; 'B':[2..2] }")]
		[TestCase(
			"∅: { 'A':∅; 'B':∅; 'C':∅; 'D':∅ }",
			"∅")]
		[TestCase(
			"(1..2): { '':(1..2) }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..3); 'B':(3..+∞) }",
			"(-∞..+∞): { 'A':(-∞..2]; 'B':[4..+∞) }")]
		public static void TestCompositeRangeMakeInclusive(string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			compositeRange = compositeRange.MakeInclusive(i => i + 1, i => i - 1);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		[TestCase(
			"(1..2): { '':(1..2) }",
			"(1..2): { '':(1..2) }")]
		[TestCase(
			"[0..2): { 'A':[0..0]; 'B':[1..1]; 'C':[1..2) }",
			"(-1..2): { 'A':(-1..1); 'B':(0..2); 'C':(0..2) }")]
		[TestCase(
			"∅: { 'A':∅; 'B':∅; 'C':∅; 'D':∅ }",
			"∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..3]; 'B':[3..+∞) }",
			"(-∞..+∞): { 'A':(-∞..4); 'B':(2..+∞) }")]
		public static void TestCompositeRangeMakeExclusive(string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			compositeRange = compositeRange.MakeExclusive(i => i - 1, i => i + 1);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		[TestCase(
			"(1..3): { (1..3) }",
			"(-∞..1]: { ∅ } | (1..3): { (1..3) } | [3..+∞): { ∅ }")]
		[TestCase(
			"[1..3): { [1..2]; (1..3); [2..2] }",
			"(-∞..1): { ∅ } | " +
				"[1..1]: { [1..2] } | " +
				"(1..2): { [1..2]; (1..3) } | " +
				"[2..2]: { [1..2]; (1..3); [2..2] } | " +
				"(2..3): { (1..3) } | " +
				"[3..+∞): { ∅ }")]
		[TestCase(
			"(-∞..+∞): { (-∞..1]; (-∞..1]; [2..+∞); [2..+∞) }",
			"(-∞..1]: { (-∞..1]; (-∞..1] } | (1..2): { ∅ } | [2..+∞): { [2..+∞); [2..+∞) }")]
		public static void TestCompositeRangeIntersections(string ranges, string expected)
		{
			var compositeRange = ParseCompositeRangeDouble(ranges);
			var intersections = compositeRange.GetIntersections()
				.Select(i => i.ToInvariantString()).Join(" | ");
			AreEqual(intersections, expected);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..3) }",
			"(-∞..1]: { ∅ } | (1..3): { 'A':(1..3) } | [3..+∞): { ∅ }")]
		[TestCase(
			"[1..3): { 'A':[1..2]; 'B':(1..3); 'C':[2..2] }",
			"(-∞..1): { ∅ } | " +
				"[1..1]: { 'A':[1..2] } | " +
				"(1..2): { 'A':[1..2]; 'B':(1..3) } | " +
				"[2..2]: { 'A':[1..2]; 'B':(1..3); 'C':[2..2] } | " +
				"(2..3): { 'B':(1..3) } | " +
				"[3..+∞): { ∅ }")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..1]; 'B':(-∞..1]; 'A':[2..+∞); 'B':[2..+∞) }",
			"(-∞..1]: { 'A':(-∞..1]; 'B':(-∞..1] } | (1..2): { ∅ } | [2..+∞): { 'A':[2..+∞); 'B':[2..+∞) }")]
		public static void TestCompositeRangeIntersectionsWithKeys(string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			var intersections = compositeRange.GetIntersections()
				.Select(i => i.ToInvariantString()).Join(" | ");
			AreEqual(intersections, expected);
		}

		[Test]
		[TestCase(
			"(1..3): { (1..3) }",
			"(1..3)",
			"(1..3): { (1..3) }")]
		[TestCase(
			"(1..3): { (1..3); (1..3); (1..3) }",
			"(1..3)",
			"(1..3): { (1..3); (1..3); (1..3) }")]
		[TestCase("∅", "∅", "∅")]
		[TestCase("∅", "(1..3)", "(1..3): { ∅ }")]
		[TestCase("(1..3): { (1..3) }", "∅", "∅")]
		[TestCase(
			"(-∞..+∞): { (-∞..1]; [0..+∞) }",
			"[0..2]",
			"[0..2]: { (-∞..1]; [0..+∞) }")]
		public static void TestCompositeRangeIntersection(string ranges, string intersection, string expected)
		{
			var compositeRange = ParseCompositeRangeDouble(ranges);
			var intersectionRange = ParseRangeDouble(intersection);

			var intersectionText = compositeRange.GetIntersection(intersectionRange).ToString();
			AreEqual(intersectionText, expected);

			intersectionText = compositeRange.GetIntersection(intersectionRange.WithKey(0)).ToString();
			AreEqual(intersectionText, expected);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..3) }",
			"(1..3)",
			"(1..3): { 'A':(1..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..3); 'A':(1..3); 'B':(1..3) }",
			"(1..3)",
			"(1..3): { 'A':(1..3); 'A':(1..3); 'B':(1..3) }")]
		[TestCase("∅", "∅", "∅")]
		[TestCase("∅", "(1..3)", "(1..3): { ∅ }")]
		[TestCase("(1..3): { 'B':(1..3) }", "∅", "∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..1]; 'B':[0..+∞) }",
			"[0..2]",
			"[0..2]: { 'A':(-∞..1]; 'B':[0..+∞) }")]
		public static void TestCompositeRangeIntersectionWithKey(string ranges, string intersection, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			var intersectionRange = ParseRangeInt32(intersection);

			var intersectionText = compositeRange.GetIntersection(intersectionRange).ToString();
			AreEqual(intersectionText, expected);

			intersectionText = compositeRange.GetIntersection(intersectionRange.WithKey(0)).ToString();
			AreEqual(intersectionText, expected);
		}

		[Test]
		[TestCase("(1..3): { 'A':(1..3) }", "(1..3): { 'B':(1..3) }", true)]
		[TestCase("(1..3): { 'A':(1..3) }", "∅", false)]
		[TestCase("∅", "(1..3): { 'A':(1..3) }", false)]
		[TestCase("(1..3): { 'A':(1..3) }", "[1..3]: { '':[1..3] }", false)]
		[TestCase("[0..5]: { 'A':[0..2]; 'B':(2..5] }", "[0..5]: { 'A':[0..2); 'B':[2..5] }", true)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':[2..5] }", "[0..5]: { 'A':[0..2]; 'B':(2..5] }", true)]
		[TestCase("[0..5]: { 'A':[0..2]; 'B':(2..5] }", "[1..4]: { '':[1..3); '':[3..4] }", true)]
		[TestCase("[0..5]: { 'A':[0..2]; 'B':(2..5] }", "[1..4]: { '':[1..3); '':(3..4] }", true)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':(2..5] }", "[1..4]: { '':[1..3); '':[3..4] }", false)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':(2..5] }", "[1..4]: { '':[1..1]; '':[4..4] }", true)]
		[TestCase("∅", "∅", true)]
		public static void TestCompositeRangeContains(string ranges, string other, bool expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(other);
			var compositeRange1A = compositeRange1.WithoutKeys();
			var compositeRange2A = compositeRange2.WithoutKeys();

			IsTrue(compositeRange1.Contains(compositeRange1));
			IsTrue(compositeRange1.Contains(compositeRange1A));
			IsTrue(compositeRange2A.Contains(compositeRange2));
			IsTrue(compositeRange2A.Contains(compositeRange2A));

			AreEqual(compositeRange1.Contains(compositeRange2A), expected);

			if (expected)
			{
				IsTrue(compositeRange2A.SubRanges.All(r => compositeRange1.Contains(r)));
				IsTrue(compositeRange2A.SubRanges.All(r => compositeRange1.Contains(r.From)));
				IsTrue(compositeRange2A.SubRanges.All(r => compositeRange1A.Contains(r.To)));
			}
			else
			{
				IsTrue(compositeRange2.IsEmpty || compositeRange2.SubRanges.Any(r => !compositeRange1A.Contains(r)));
			}
		}

		[Test]
		[TestCase("(1..3): { 'A':(1..3) }", "(1..3): { 'B':(1..3) }", true)]
		[TestCase("(1..3): { 'A':(1..3) }", "∅", false)]
		[TestCase("∅", "(1..3): { 'A':(1..3) }", false)]
		[TestCase("(1..3): { 'A':(1..3) }", "[1..1]: { '':[1..1] }", false)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':(2..5] }", "[2..2]: { '':[2..2] }", false)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':(2..3); 'C':(3..5] }", "[2..3]: { '':[2..2]; '':[3..3] }", false)]
		[TestCase("[0..5]: { 'A':[0..2); 'B':(2..3); 'C':[3..5] }", "[2..3]: { '':[2..2]; '':[3..3] }", true)]
		[TestCase("∅", "∅", true)]
		public static void TestCompositeRangeHasIntersection(string ranges, string other, bool expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(other);
			var compositeRange1A = compositeRange1.WithoutKeys();
			var compositeRange2A = compositeRange2.WithoutKeys();

			IsTrue(compositeRange1.HasIntersection(compositeRange1));
			IsTrue(compositeRange1.HasIntersection(compositeRange1A));
			IsTrue(compositeRange2A.HasIntersection(compositeRange2));
			IsTrue(compositeRange2A.HasIntersection(compositeRange2A));

			AreEqual(compositeRange1.HasIntersection(compositeRange2A), expected);
			AreEqual(compositeRange2A.HasIntersection(compositeRange1), expected);

			if (expected)
			{
				IsTrue(compositeRange2.IsEmpty || compositeRange2A.SubRanges.Any(r => compositeRange1.HasIntersection(r)));
			}
			else
			{
				IsTrue(compositeRange2.SubRanges.All(r => !compositeRange1A.HasIntersection(r)));
			}
		}

		[Test]
		[TestCase(
			"(1..3): { (1..3) }",
			"(1..3): { (1..3); (1..3) }",
			"(1..3): { (1..3) }")]
		[TestCase("∅", "∅", "∅")]
		[TestCase(
			"∅",
			"(1..3): { (1..3) }",
			"(1..3): { (1..3) }")]
		[TestCase(
			"(1..3): { (1..3) }",
			"∅",
			"(1..3): { (1..3) }")]
		[TestCase(
			"(-∞..+∞): { (-∞..1]; [2..+∞) }",
			"[1..2]: { [1..2] }",
			"(-∞..+∞): { (-∞..+∞) }")]
		public static void TestCompositeRangeUnion(string ranges, string other, string expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(ranges);
			var compositeRange2 = ParseCompositeRangeDouble(other);

			AreEqual(compositeRange1.Union(compositeRange2).ToInvariantString(), expected);
			AreEqual(compositeRange2.Union(compositeRange1).ToInvariantString(), expected);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..3) }",
			"(1..3): { 'B':(1..3) }",
			"(1..3): { 'A':(1..3); 'B':(1..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..3); 'A':(1..3); 'B':(1..3) }",
			"(1..3): { 'A':(1..3); 'B':(1..3); 'B':(1..3) }",
			"(1..3): { 'A':(1..3); 'B':(1..3) }")]
		[TestCase("∅", "∅", "∅")]
		[TestCase(
			"∅",
			"(1..3): { 'B':(1..3) }",
			"(1..3): { 'B':(1..3) }")]
		[TestCase(
			"(1..3): { 'B':(1..3) }",
			"∅",
			"(1..3): { 'B':(1..3) }")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..1]; 'B':[0..+∞) }",
			"(-∞..+∞): { 'B':(-∞..1]; 'A':[0..+∞) }",
			"(-∞..+∞): { 'A':(-∞..+∞); 'B':(-∞..+∞) }")]
		public static void TestCompositeRangeUnionWithKey(string ranges, string other, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(other);

			AreEqual(compositeRange1.Union(compositeRange2).ToInvariantString(), expected);
		}

		[Test]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			null,
			"(-∞..3): { (-∞..2]; (-∞..3); (2..3) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			1,
			"[1..3): { [1..2]; [1..3); (2..3) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			2,
			"(1..3): { (1..2]; (1..3); (2..3) }")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeExtendFrom(string ranges, double? extendFrom, string expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(ranges).ExtendFrom(extendFrom);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.ExtendFrom(extendFrom), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			null,
			"(-∞..3): { 'A':(-∞..2]; 'B':(-∞..3); 'A':(2..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			1,
			"[1..3): { 'A':[1..2]; 'B':[1..3); 'A':(2..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			2,
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeExtendFromWithKey(string ranges, int? extendFrom, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges).ExtendFrom(extendFrom);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.ExtendFrom(extendFrom), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			null,
			"(1..+∞): { (1..2]; (1..+∞); (2..+∞) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			3,
			"(1..3]: { (1..2]; (1..3]; (2..3] }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			2,
			"(1..3): { (1..2]; (1..3); (2..3) }")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeExtendTo(string ranges, double? extendTo, string expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(ranges).ExtendTo(extendTo);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.ExtendTo(extendTo), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			null,
			"(1..+∞): { 'A':(1..2]; 'B':(1..+∞); 'A':(2..+∞) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			3,
			"(1..3]: { 'A':(1..2]; 'B':(1..3]; 'A':(2..3] }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			2,
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeExtendToWithKey(string ranges, int? extendTo, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges).ExtendTo(extendTo);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.ExtendTo(extendTo), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..3) }",
			"(1..3): { 'B':(1..3) }",
			"(1..3): { 'A':(1..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..3); 'A':(1..3); 'B':(1..3) }",
			"(1..3): { 'A':(1..3); 'B':(1..3); 'B':(1..3) }",
			"(1..3): { 'A':(1..3); 'A':(1..3); 'B':(1..3) }")]
		[TestCase("∅", "∅", "∅")]
		[TestCase(
			"∅",
			"(1..3): { 'B':(1..3) }",
			"∅")]
		[TestCase(
			"(1..3): { 'B':(1..3) }",
			"∅",
			"∅")]
		[TestCase(
			"(-∞..+∞): { 'A':(-∞..1]; 'B':[0..+∞) }",
			"[0..2]: { 'C':[0..2] }",
			"[0..2]: { 'A':[0..1]; 'B':[0..2] }")]
		public static void TestCompositeRangeIntersect(string ranges, string other, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(other);
			var compositeRange1A = compositeRange1.WithoutKeys();
			var compositeRange2A = compositeRange2.WithoutKeys();

			AreEqual(compositeRange1.Intersect(compositeRange2).ToInvariantString(), expected);
			AreEqual(compositeRange1.Intersect(compositeRange2A).ToInvariantString(), expected);
			AreEqual(
				compositeRange1.Intersect(compositeRange2A).WithoutKeys(),
				compositeRange1A.Intersect(compositeRange2A));
		}


		[Test]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			null,
			"(1..3): { (1..2]; (1..3); (2..3) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			2,
			"[2..3): { [2..2]; [2..3); (2..3) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			4,
			"∅")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeTrimFrom(string ranges, double? trimFrom, string expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(ranges).TrimFrom(trimFrom);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.TrimFrom(trimFrom), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			null,
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			2,
			"[2..3): { 'A':[2..2]; 'B':[2..3); 'A':(2..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			4,
			"∅")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeTrimFromWithKey(string ranges, int? trimFrom, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges).TrimFrom(trimFrom);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.TrimFrom(trimFrom), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			null,
			"(1..3): { (1..2]; (1..3); (2..3) }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			2,
			"(1..2]: { (1..2]; (1..2] }")]
		[TestCase(
			"(1..3): { (1..2]; (1..3); (2..3) }",
			1,
			"∅")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeTrimTo(string ranges, double? trimTo, string expected)
		{
			var compositeRange1 = ParseCompositeRangeDouble(ranges).TrimTo(trimTo);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.TrimTo(trimTo), compositeRange1);
		}

		[Test]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			null,
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			2,
			"(1..2]: { 'A':(1..2]; 'B':(1..2] }")]
		[TestCase(
			"(1..3): { 'A':(1..2]; 'B':(1..3); 'A':(2..3) }",
			1,
			"∅")]
		[TestCase("∅", 1, "∅")]
		[TestCase("∅", null, "∅")]
		public static void TestCompositeRangeTrimToWithKey(string ranges, int? trimTo, string expected)
		{
			var compositeRange1 = ParseCompositeKeyedRangeInt32(ranges).TrimTo(trimTo);

			AreEqual(compositeRange1.ToInvariantString(), expected);
			AreEqual(compositeRange1.TrimTo(trimTo), compositeRange1);
		}

		[Test]
		[TestCase(
			"ADAFEF",
			"[0..+∞): { 'A':[0..3); 'A':[0..3); 'D':[3..4); 'E':[4..5); 'F':[5..+∞); 'F':[5..+∞) }")]
		[TestCase(
			"ADD21",
			"(-∞..+∞): { '2':(-∞..0); '1':(-∞..0); 'A':[0..3); 'D':[3..+∞); 'D':[3..+∞) }")]
		public static void TestCompositeRangeFrom(string chars, string expected)
		{
			var range = chars.ToCompositeRangeFrom(c => c < 'A' ? null : (int?)(c - 'A'));
			AreEqual(range.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		[TestCase(
			"ADAFEF",
			"(-∞..5]: { 'A':(-∞..0]; 'A':(-∞..0]; 'D':(0..3]; 'E':(3..4]; 'F':(4..5]; 'F':(4..5] }")]
		[TestCase(
			"ADD21",
			"(-∞..+∞): { 'A':(-∞..0]; 'D':(0..3]; 'D':(0..3]; '2':(3..+∞); '1':(3..+∞) }")]
		public static void TestCompositeRangeTo(string chars, string expected)
		{
			var range = chars.ToCompositeRangeTo(c => c < 'A' ? null : (int?)(c - 'A'));
			AreEqual(range.ToString(CultureInfo.InvariantCulture), expected);
		}

	}
}
#endif