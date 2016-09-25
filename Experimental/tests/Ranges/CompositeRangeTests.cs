using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CodeJam.Strings;

using NUnit.Framework;

using static NUnit.Framework.Assert;
using static CodeJam.Ranges.RangeTestHelpers;

namespace CodeJam.Ranges
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
	public static class CompositeRangeTests
	{
		#region Parse helpers
		private static CompositeRange<T> ParseCompositeRange<T>(
			string value,
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
			string value,
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


		public static CompositeRange<double?> ParseCompositeRangeDouble(string value) =>
			ParseCompositeRange(value, s => (double?)double.Parse(s, CultureInfo.InvariantCulture));

		public static CompositeRange<int?, string> ParseCompositeKeyedRangeInt32(string value) =>
			ParseCompositeRange(value, s => (int?)int.Parse(s, CultureInfo.InvariantCulture), s => s.IsNullOrEmpty() ? null : s);
		#endregion

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
		public static void TestCreate(
			string ranges, string expected)
		{
			var seed = new Random().Next();
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seed: {seed} (use the seed to reproduce test results).");
			var rnd = new Random(seed);

			var compositeRange = ParseCompositeRangeDouble(ranges);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);

			var compositeRange2 = compositeRange.SubRanges.OrderBy(r => rnd.Next()).ToCompositeRange();
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
			"[0..8): { 'C':[0..6]; 'G':[1..2]; 'J':[1..2]; 'A':[3..4]; 'B':[3..5); 'K':[3..5]; 'L':(3..5); 'O':(3..5]; 'H':(4..6); 'I':[7..8) }")]
		[TestCase(
			"(-∞..+∞): { 'A':[3..4]; 'B':[3..5); 'C':[0..6]; 'D':∅; 'E':∅; 'F':∅; 'G':[1..2]; 'H':(4..6); '':(-∞..+∞); 'I':[7..8); 'J':[1..2]; 'K':[3..5]; 'L':(3..5); 'M':∅; 'N':∅; 'O':(3..5] }",
			"(-∞..+∞): { '':(-∞..+∞); 'C':[0..6]; 'G':[1..2]; 'J':[1..2]; 'A':[3..4]; 'B':[3..5); 'K':[3..5]; 'L':(3..5); 'O':(3..5]; 'H':(4..6); 'I':[7..8) }")]
		public static void TestCreateWithKey(
			string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			AreEqual(compositeRange.ToString(CultureInfo.InvariantCulture), expected);
		}

		[Test]
		public static void TestRangeProperties()
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
		public static void TestKeyedRangeProperties()
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
			var seed = new Random().Next();
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seed: {seed} (use the seed to reproduce test results).");
			var rnd = new Random(seed);

			var compositeRange1 = ParseCompositeKeyedRangeInt32(range1);
			var compositeRange2 = ParseCompositeKeyedRangeInt32(range2);

			// Shuffle keys
			var compositeRange1Rnd = compositeRange1.SubRanges.OrderBy(r => rnd.Next()).ToCompositeRange();
			var compositeRange2Rnd = compositeRange2.SubRanges.OrderBy(r => rnd.Next()).ToCompositeRange();

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
		public static void TestMerge(string ranges, string expected)
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
		public static void TestMergeWithKey(
			string ranges, string expected)
		{
			var compositeRange = ParseCompositeKeyedRangeInt32(ranges);
			AreEqual(compositeRange.Merge().ToString(CultureInfo.InvariantCulture), expected);
		}
	}
}