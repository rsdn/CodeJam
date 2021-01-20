using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using NUnit.Framework;

using static NUnit.Framework.Assert;

using static CodeJam.Ranges.RangeTestHelper;

namespace CodeJam.Ranges
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	public static partial class RangeTests
	{
		private const char _rangeKey = '_';
		private const string _rangeKey2 = ">";

		[Test]
		public static void TestRangeCreate()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;
			var key = "Hello!";

			DoesNotThrow(() => Range.Create(value1, value2));
			DoesNotThrow(() => Range.Create(value1, value1));
			DoesNotThrow(() => Range.Create(empty, value2));
			DoesNotThrow(() => Range.Create(empty, empty));

			DoesNotThrow(() => Range.CreateExclusiveFrom(value1, value2));
			Throws<ArgumentException>(() => Range.CreateExclusiveFrom(value1, value1));
			DoesNotThrow(() => Range.CreateExclusiveFrom(empty, value2));
			DoesNotThrow(() => Range.CreateExclusiveFrom(empty, empty));

			DoesNotThrow(() => Range.CreateExclusiveTo(value1, value2, key));
			Throws<ArgumentException>(() => Range.CreateExclusiveTo(value2, value2, key));
			DoesNotThrow(() => Range.CreateExclusiveTo(empty, value2, key));
			DoesNotThrow(() => Range.CreateExclusiveTo(empty, empty, key));

			DoesNotThrow(() => Range.CreateExclusive(value1, value2, key));
			Throws<ArgumentException>(() => Range.CreateExclusive(value2, value2, key));
			DoesNotThrow(() => Range.CreateExclusive(empty, value2, key));
			DoesNotThrow(() => Range.CreateExclusive(empty, empty, key));

			Throws<ArgumentException>(() => Range.Create(value2, value1, key));
			Throws<ArgumentException>(() => Range.CreateExclusiveFrom(value2, value1));
			Throws<ArgumentException>(() => Range.CreateExclusiveTo(value2, value1, key));
			Throws<ArgumentException>(() => Range.CreateExclusive(value2, value1, key));
			Throws<ArgumentException>(() => Range.Create(RangeBoundaryFrom<int?>.Empty, Range.BoundaryTo(value2), key));
			Throws<ArgumentException>(() => Range.Create(Range.BoundaryFrom(empty), RangeBoundaryTo<int?>.Empty, key));
			Throws<ArgumentException>(() => Range.Create(double.NaN, 1, key));
			Throws<ArgumentException>(() => Range.Create(1, double.NaN, key));
			Throws<ArgumentException>(() => Range.Create(double.NegativeInfinity, double.NegativeInfinity, key));
			Throws<ArgumentException>(() => Range.Create(double.PositiveInfinity, double.PositiveInfinity, key));
			Throws<ArgumentException>(() => Range.Create(double.PositiveInfinity, double.NegativeInfinity, key));
			Throws<ArgumentException>(() => Range.Create(double.PositiveInfinity, 2, key));
			Throws<ArgumentException>(() => Range.Create(1, double.NegativeInfinity, key));

			AreEqual(
				Range<int?>.Empty,
				new Range<int?>(RangeBoundaryFrom<int?>.Empty, RangeBoundaryTo<int?>.Empty));
			AreEqual(
				Range<int?>.Infinite,
				Range.Create(empty, empty));
			AreEqual(
				Range<double>.Infinite,
				Range.Create(double.NegativeInfinity, double.PositiveInfinity));
			AreEqual(
				Range.TryCreate(value2, value1),
				Range<int?>.Empty);
			AreEqual(
				Range<int?, string>.Empty,
				Range.TryCreate(value2, value1, (string?)null));
			AreEqual(
				Range<int?, string>.Infinite,
				Range.Create(empty, empty, (string?)null));
			AreNotEqual(
				Range<int?, string>.Empty,
				Range.TryCreate(value2, value1, key));
			AreNotEqual(
				Range<int?, string>.Infinite,
				Range.Create(empty, empty, key));

			IsTrue(Range.TryCreate(value2, value1, key).IsEmpty);
			IsFalse(Range.TryCreate(value1, value2, key).IsEmpty);
			IsTrue(Range.TryCreate(double.NegativeInfinity, double.NegativeInfinity, key).IsEmpty);
			IsTrue(Range.TryCreate(double.NaN, 1, key).IsEmpty);
			IsTrue(Range.TryCreate(double.NaN, double.NaN, key).IsEmpty);
		}

		[Test]
		public static void TestRangeProperties()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;
			var key = "Hello!";

			var a = new Range<int?>();
			Throws<InvalidOperationException>(() => a.FromValue.ToString());
			Throws<InvalidOperationException>(() => a.ToValue.ToString());
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Empty);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Empty);
			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsFalse(a.IsInfinite);
			IsFalse(a.IsSinglePoint);

			a = Range<int?>.Infinite;
			Throws<InvalidOperationException>(() => a.FromValue.ToString());
			Throws<InvalidOperationException>(() => a.ToValue.ToString());
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Infinite);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Infinite);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsInfinite);
			IsFalse(a.IsSinglePoint);

			a = Range.CreateExclusiveTo(empty, value2);
			Throws<InvalidOperationException>(() => a.FromValue.ToString());
			AreEqual(a.ToValue, value2);
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Infinite);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Exclusive);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsInfinite);
			IsFalse(a.IsSinglePoint);

			a = Range.Create(value1, value2);
			AreEqual(a.FromValue, value1);
			AreEqual(a.ToValue, value2);
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Inclusive);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Inclusive);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsInfinite);
			IsFalse(a.IsSinglePoint);

			var b = Range.Create(value1, value1, key);
			AreEqual(b.FromValue, value1);
			AreEqual(b.ToValue, value1);
			AreEqual(b.Key, key);
			AreEqual(b.From.Kind, RangeBoundaryFromKind.Inclusive);
			AreEqual(b.To.Kind, RangeBoundaryToKind.Inclusive);
			IsFalse(b.IsEmpty);
			IsTrue(b.IsNotEmpty);
			IsFalse(b.IsInfinite);
			IsTrue(b.IsSinglePoint);
		}

		[Test]
		public static void TestRangeEquality()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? value3 = 3;
			int? empty = null;
			var key = "Hello!";
			var key2 = "Hello2!";

			var eFrom = new RangeBoundaryFrom<int?>();
			var eTo = new RangeBoundaryTo<int?>();

			AreEqual(Range<int?>.Empty, Range.Create(eFrom, eTo));
			AreEqual(Range<int?>.Infinite, Range.Create(empty, empty));
			AreNotEqual(Range<double?>.Infinite, Range.Create(empty, empty));

			AreEqual(
				Range.CreateExclusiveFrom(value1, value2).From,
				Range.BoundaryFromExclusive(value1));

			AreEqual(
				Range.CreateExclusiveFrom(value1, value2).To,
				Range.BoundaryTo(value2));

			AreNotEqual(
				Range.CreateExclusiveFrom(value1, value2),
				Range.Create(value1, value2));

			IsTrue(Range.Create(value1, value2) == Range.Create<int?>(1, 2));
			IsTrue(Range.Create(value1, value2) != Range.Create(value1, 1));
			IsTrue(Range.Create(value1, value2, key) == Range.Create<int?, string>(1, 2, key));
			IsTrue(Range.Create(value1, value2, key) != Range.Create<int?, string>(1, 2, key2));
			IsTrue(Range.Create(value1, value2, key) != Range.Create<int?, string>(1, 1, key));

			IsTrue(Range.TryCreate(value1, value2) == Range.Create<int?>(1, 2));
			IsTrue(Range.TryCreate(value1, value2) != Range.Create(value1, 1));
			IsTrue(Range.TryCreate(value1, value2, key) == Range.Create<int?, string>(1, 2, key));
			IsTrue(Range.TryCreate(value1, value2, key) != Range.Create<int?, string>(1, 2, key2));
			IsTrue(Range.TryCreate(value1, value2, key) != Range.Create<int?, string>(1, 1, key));

			IsTrue(Range.TryCreateExclusiveFrom(value1, value2) == Range.CreateExclusiveFrom<int?>(1, 2));
			IsTrue(Range.TryCreateExclusiveFrom(value1, value3) == Range.CreateExclusiveFrom(value1, 3));
			IsTrue(Range.TryCreateExclusiveFrom(value1, value3) != Range.CreateExclusiveFrom(value1, 4));
			IsTrue(Range.TryCreateExclusiveFrom(value1, value2, key) == Range.CreateExclusiveFrom<int?, string>(1, 2, key));
			IsTrue(Range.TryCreateExclusiveFrom(value1, value2, key) != Range.CreateExclusiveFrom<int?, string>(1, 2, key2));

			IsTrue(Range.TryCreateExclusiveTo(value1, value2) == Range.CreateExclusiveTo<int?>(1, 2));
			IsTrue(Range.TryCreateExclusiveTo(value1, value3) == Range.CreateExclusiveTo(value1, 3));
			IsTrue(Range.TryCreateExclusiveTo(value1, value3) != Range.CreateExclusiveTo(value1, 4));
			IsTrue(Range.TryCreateExclusiveTo(value1, value2, key) == Range.CreateExclusiveTo<int?, string>(1, 2, key));
			IsTrue(Range.TryCreateExclusiveTo(value1, value2, key) != Range.CreateExclusiveTo<int?, string>(1, 2, key2));
		}

		/// <summary>Tests the range to string.</summary>
		[Test]
		public static void TestRangeToString()
		{
			int? value1 = 1;
			int? empty = null;
			var key = "Hello!";

			AreEqual(Range<int>.Empty.ToString(CultureInfo.InvariantCulture), "∅");
			AreEqual(Range<int>.Infinite.ToString(CultureInfo.InvariantCulture), "(-∞..+∞)");
			AreEqual(Range.Create(1, 1).ToString(CultureInfo.InvariantCulture), "[1..1]");
			AreEqual(Range.TryCreate(1, 1).ToString(CultureInfo.InvariantCulture), "[1..1]");
			AreEqual(Range.Create(1, 2).ToString(CultureInfo.InvariantCulture), "[1..2]");
			AreEqual(Range.TryCreate(1, 2).ToString(CultureInfo.InvariantCulture), "[1..2]");
			AreEqual(Range.CreateExclusive(1, 2).ToString(CultureInfo.InvariantCulture), "(1..2)");
			AreEqual(Range.TryCreateExclusive(1, 2).ToString(CultureInfo.InvariantCulture), "(1..2)");
			AreEqual(Range.CreateExclusiveFrom(1, 2).ToString(CultureInfo.InvariantCulture), "(1..2]");
			AreEqual(Range.TryCreateExclusiveFrom(1, 2).ToString(CultureInfo.InvariantCulture), "(1..2]");
			AreEqual(Range.CreateExclusiveTo(1, 2).ToString(CultureInfo.InvariantCulture), "[1..2)");
			AreEqual(Range.TryCreateExclusiveTo(1, 2).ToString(CultureInfo.InvariantCulture), "[1..2)");
			AreEqual(Range.CreateExclusive(value1, empty).ToString("000", CultureInfo.InvariantCulture), "(001..+∞)");
			AreEqual(Range.TryCreateExclusive(value1, empty).ToString("000", CultureInfo.InvariantCulture), "(001..+∞)");

			AreEqual(Range.Create(RangeBoundaryFrom<int?>.Empty, RangeBoundaryTo<int?>.Empty, key).ToString(CultureInfo.InvariantCulture), "'Hello!':∅");
			AreEqual(Range.TryCreate(RangeBoundaryFrom<int?>.Empty, RangeBoundaryTo<int?>.Empty, key).ToString(CultureInfo.InvariantCulture), "'Hello!':∅");
			AreEqual(Range.Create(empty, empty, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(-∞..+∞)");
			AreEqual(Range.TryCreate(empty, empty, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(-∞..+∞)");
			AreEqual(Range.Create(empty, empty, (string?)null).ToString(CultureInfo.InvariantCulture), "'':(-∞..+∞)");
			AreEqual(Range.TryCreate(empty, empty, (string?)null).ToString(CultureInfo.InvariantCulture), "'':(-∞..+∞)");
			AreEqual(Range.Create(1, 1, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..1]");
			AreEqual(Range.TryCreate(1, 1, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..1]");
			AreEqual(Range.Create(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..2]");
			AreEqual(Range.TryCreate(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..2]");
			AreEqual(Range.CreateExclusive(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(1..2)");
			AreEqual(Range.TryCreateExclusive(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(1..2)");
			AreEqual(Range.CreateExclusiveFrom(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(1..2]");
			AreEqual(Range.TryCreateExclusiveFrom(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':(1..2]");
			AreEqual(Range.CreateExclusiveTo(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..2)");
			AreEqual(Range.TryCreateExclusiveTo(1, 2, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..2)");
			AreEqual(Range.CreateExclusive(value1, empty, 3).ToString("000", CultureInfo.InvariantCulture), "'3':(001..+∞)");
			AreEqual(Range.TryCreateExclusive(value1, empty, 3).ToString("000", CultureInfo.InvariantCulture), "'3':(001..+∞)");
			AreEqual(Range.Create((int?)1, null, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..+∞)");
			AreEqual(Range.TryCreate((int?)1, null, key).ToString(CultureInfo.InvariantCulture), "'Hello!':[1..+∞)");
			var cultureRu = new CultureInfo("ru-RU");
			var cultureEn = new CultureInfo("en-US");
			AreEqual(Range.Create(1.5, 2.5, 1.1).ToString("000.000", cultureRu), "'1,1':[001,500..002,500]");
			AreEqual(Range.TryCreate(1.5, 2.5, 1.1).ToString("000.000", cultureRu), "'1,1':[001,500..002,500]");
			AreEqual(Range.Create(1.5, 2.5, 1.1).ToString("000.000", cultureEn), "'1.1':[001.500..002.500]");
			AreEqual(Range.TryCreate(1.5, 2.5, 1.1).ToString("000.000", cultureEn), "'1.1':[001.500..002.500]");
			AreEqual(Range.Create(1.5, 2.5, (string?)null).ToString(null, cultureRu), "'':[1,5..2,5]");
			AreEqual(Range.TryCreate(1.5, 2.5, (string?)null).ToString(null, cultureRu), "'':[1,5..2,5]");
			AreEqual(Range.Create(1.5, 2.5, (string?)null).ToString(null, cultureEn), "'':[1.5..2.5]");
			AreEqual(Range.TryCreate(1.5, 2.5, (string?)null).ToString(null, cultureEn), "'':[1.5..2.5]");
		}

		[Test]
		public static void TestKeyedRangeKeyFlow()
		{
			int? value1 = 1;
			int? empty = null;
			var key = "Hello!";

			var range = Range.Create(value1, empty).WithKey("A");

			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'A':[1..+∞)");
			AreNotEqual(range.Key, key);

			var prevRange = range.WithKey(key);

			range = prevRange;
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':[1..+∞)");
			AreEqual(range.Key, key);
			AreEqual(range, prevRange);

			range = range.Intersect(1, 2);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':[1..2]");
			AreEqual(range.Key, key);
			AreNotEqual(range, prevRange);

			range = range.Union(4, 8);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':[1..8]");
			AreEqual(range.Key, key);
			AreNotEqual(range, prevRange);

			range = range.Intersect(10, 20);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':∅");
			AreEqual(range.Key, key);
			AreNotEqual(range, prevRange);

			range = range.Union(null, null);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':(-∞..+∞)");
			AreEqual(range.Key, key);
			AreNotEqual(range, prevRange);

			range = range.TrimFrom(1);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'Hello!':[1..+∞)");
			AreEqual(range.Key, key);
			AreEqual(range, prevRange);

			range = Range.Create(value1, value1)
				.WithKey("B")
				.Intersect(range);
			AreEqual(range.ToString(CultureInfo.InvariantCulture), "'B':[1..1]");
			AreNotEqual(range.Key, key);
			AreNotEqual(range, prevRange);
		}

		[Test]
		[TestCase("[1..2]", "[1..2]", true)]
		[TestCase("[1..2]", "(1..2)", true)]
		[TestCase("[1..2]", "(1..2]", true)]
		[TestCase("[1..2]", "[1..2)", true)]
		[TestCase("(1..2)", "[1..2]", false)]
		[TestCase("(1..2)", "(1..2)", true)]
		[TestCase("(1..2)", "(1..2]", false)]
		[TestCase("(1..2)", "[1..2)", false)]
		[TestCase("[1..2]", "(0..3)", false)]
		[TestCase("[1..2]", "[2..3)", false)]
		[TestCase("[1..2]", "(2..3)", false)]
		[TestCase("[1..2]", "(0..1)", false)]
		[TestCase("[1..2]", "(0..1]", false)]
		[TestCase("[1..2]", "(-∞..+∞)", false)]
		[TestCase("(1..2)", "∅", false)]
		[TestCase("(-∞..+∞)", "∅", false)]
		[TestCase("(-∞..+∞)", "[1..2]", true)]
		[TestCase("(-∞..+∞)", "(-∞..+∞)", true)]
		[TestCase("∅", "(-∞..+∞)", false)]
		[TestCase("∅", "[1..2)", false)]
		[TestCase("∅", "∅", true)]
		public static void TestRangeContainsRange(string rangeA, string rangeB, bool expected)
		{
			var a = ParseRangeDouble(rangeA);
			var b = ParseRangeDouble(rangeB);
			var a2 = a.WithKey("1");
			var b2 = b.WithKey(1);

			AreEqual(a.Contains(b), expected);
			AreEqual(a.Contains(b2), expected);
			AreEqual(a2.Contains(b), expected);
			AreEqual(a2.Contains(b2), expected);
			if (b.IsNotEmpty && !b.From.IsExclusiveBoundary && !b.To.IsExclusiveBoundary)
			{
				AreEqual(a.Contains(b.From.GetValueOrDefault(), b.To.GetValueOrDefault()), expected);
			}
		}

		[Test]
		[TestCase("[1..2]", 2.0, 1.0)]
		[TestCase("[1..2]", double.PositiveInfinity, double.PositiveInfinity)]
		[TestCase("[1..2]", double.PositiveInfinity, null)]
		[TestCase("[1..2]", 1.0, double.NaN)]
		public static void TestRangeContainsThrows(string rangeA, double? from, double to)
		{
			var a = ParseRangeDouble(rangeA);
			var a2 = a.WithKey("1");

			Throws<ArgumentException>(() => a.Contains(from, to));
			Throws<ArgumentException>(() => a2.Contains(from, to));
		}
	}
}