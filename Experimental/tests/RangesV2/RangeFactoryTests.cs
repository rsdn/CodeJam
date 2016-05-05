using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.RangesV2
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public class RangeFactoryTests
	{
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

			AreEqual(
				Range<int?>.Empty,
				new Range<int?>(RangeBoundaryFrom<int?>.Empty, RangeBoundaryTo<int?>.Empty));
			AreEqual(
				Range<int?>.Infinity,
				Range.Create(empty, empty));
			AreNotEqual(
				Range<int?>.Infinity,
				Range.Create(empty, empty, key));

			AreEqual(
				Range.TryCreate(value2, value1),
				Range<int?>.Empty);

			IsTrue(Range.TryCreate(value2, value1, key).IsEmpty);
			IsFalse(Range.TryCreate(value1, value2, key).IsEmpty);
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
			IsFalse(a.IsInfinity);
			IsFalse(a.IsSinglePoint);

			a = Range<int?>.Infinity;
			Throws<InvalidOperationException>(() => a.FromValue.ToString());
			Throws<InvalidOperationException>(() => a.ToValue.ToString());
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Infinite);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Infinite);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.IsInfinity);
			IsFalse(a.IsSinglePoint);

			a = Range.CreateExclusiveTo(empty, value2);
			Throws<InvalidOperationException>(() => a.FromValue.ToString());
			AreEqual(a.ToValue, value2);
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Infinite);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Exclusive);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsInfinity);
			IsFalse(a.IsSinglePoint);

			a = Range.Create(value1, value2);
			AreEqual(a.FromValue, value1);
			AreEqual(a.ToValue, value2);
			AreEqual(a.From.Kind, RangeBoundaryFromKind.Inclusive);
			AreEqual(a.To.Kind, RangeBoundaryToKind.Inclusive);
			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.IsInfinity);
			IsFalse(a.IsSinglePoint);

			var b = Range.Create(value1, value1, key);
			AreEqual(b.FromValue, value1);
			AreEqual(b.ToValue, value1);
			AreEqual(b.Key, key);
			AreEqual(b.From.Kind, RangeBoundaryFromKind.Inclusive);
			AreEqual(b.To.Kind, RangeBoundaryToKind.Inclusive);
			IsFalse(b.IsEmpty);
			IsTrue(b.IsNotEmpty);
			IsFalse(b.IsInfinity);
			IsTrue(b.IsSinglePoint);
		}

		//[Test]
		public static void TestBoundaryFromEquality()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;

			var e = new RangeBoundaryFrom<int?>();
			var inf = Range.BoundaryFrom(empty);
			var a1 = Range.BoundaryFrom(value1);
			var a12 = Range.BoundaryFrom(value1);
			var a2 = Range.BoundaryFrom(value2);
			var b1 = Range.BoundaryTo(value1);

			AreEqual(e, RangeBoundaryFrom<int?>.Empty);
			IsTrue(e == RangeBoundaryFrom<int?>.Empty);
			IsFalse(e != RangeBoundaryFrom<int?>.Empty);

			AreEqual(inf, RangeBoundaryFrom<int?>.NegativeInfinity);
			IsTrue(inf == RangeBoundaryFrom<int?>.NegativeInfinity);

			AreNotEqual(a1, empty);
			AreNotEqual(a1, inf);

			AreEqual(a1, a12);
			IsTrue(a1 == a12);
			IsFalse(a1 != a12);

			AreNotEqual(a1, a2);
			IsFalse(a1 == a2);
			IsTrue(a1 != a2);

			AreEqual(a1.Value, value1);
			AreNotEqual(a1.Value, value2);
			AreNotEqual(a1, value1);
			AreNotEqual(a1, value2);

			AreNotEqual(a1, b1);
			AreEqual(b1.Value, value1);
		}
	}
}