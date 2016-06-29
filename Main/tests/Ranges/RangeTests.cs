using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Ranges
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static partial class RangeTests
	{
		private const char RangeKey = '_';
		private const string RangeKey2 = ">";

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
			AreNotEqual(
				Range<int?>.Infinite,
				Range.Create(empty, empty, key));

			AreEqual(
				Range.TryCreate(value2, value1),
				Range<int?>.Empty);

			IsTrue(Range.TryCreate(value2, value1, key).IsEmpty);
			IsFalse(Range.TryCreate(value1, value2, key).IsEmpty);
			IsTrue(Range.TryCreate(double.NegativeInfinity, double.NegativeInfinity, key).IsEmpty);
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
			int? empty = null;

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
			IsFalse(Range.Create(value1, value2) == Range.Create(value1, value1));
		}

		/// <summary>Tests the range to string.</summary>
		[Test]
		public static void TestRangeToString()
		{
			int? value1 = 1;
			int? empty = null;
			var key = "Hello";

			AreEqual(Range<int>.Empty.ToString(), "∅");
			AreEqual(Range<int>.Infinite.ToString(), "(-∞..+∞)");
			AreEqual(Range.Create(1, 1).ToString(), "[1..1]");
			AreEqual(Range.Create(1, 2).ToString(), "[1..2]");
			AreEqual(Range.CreateExclusive(1, 2).ToString(), "(1..2)");
			AreEqual(Range.CreateExclusiveFrom(1, 2).ToString(), "(1..2]");
			AreEqual(Range.CreateExclusiveTo(1, 2).ToString(), "[1..2)");
			AreEqual(Range.CreateExclusive(value1, empty).ToString("000"), "(001..+∞)");

			AreEqual(Range.Create(RangeBoundaryFrom<int>.Empty, RangeBoundaryTo<int>.Empty, key).ToString(), "'Hello': ∅");
			AreEqual(Range.Create(empty, empty, key).ToString(), "'Hello': (-∞..+∞)");
			AreEqual(Range.Create(1, 1, key).ToString(), "'Hello': [1..1]");
			AreEqual(Range.Create(1, 2, key).ToString(), "'Hello': [1..2]");
			AreEqual(Range.CreateExclusive(1, 2, key).ToString(), "'Hello': (1..2)");
			AreEqual(Range.CreateExclusiveFrom(1, 2, key).ToString(), "'Hello': (1..2]");
			AreEqual(Range.CreateExclusiveTo(1, 2, key).ToString(), "'Hello': [1..2)");
			AreEqual(Range.CreateExclusive(value1, empty, 3).ToString("000"), "'3': (001..+∞)");
		}
	}
}