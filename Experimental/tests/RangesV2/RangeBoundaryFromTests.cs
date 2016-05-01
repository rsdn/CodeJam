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
	public class RangeBoundaryFromTests
	{
		[Test]
		public static void Test00BoundaryFromCreation()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;

			DoesNotThrow(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Empty));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Infinite));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Exclusive));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(value2, RangeBoundaryFromKind.Inclusive));

			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Empty));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(value2, RangeBoundaryFromKind.Infinite));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Exclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Inclusive));

			AreEqual(
				RangeBoundaryFrom<int?>.NegativeInfinity,
				Range.BoundaryFromInfinity<int?>());
			AreEqual(
				RangeBoundaryFrom<int?>.NegativeInfinity,
				Range.BoundaryFrom(empty));
			AreEqual(
				RangeBoundaryFrom<int?>.NegativeInfinity,
				Range.BoundaryFromExclusive(empty));
			AreEqual(
				new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Inclusive),
				Range.BoundaryFrom(value1));
			AreEqual(
				new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Exclusive),
				Range.BoundaryFromExclusive(value1));
			AreNotEqual(
				new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Exclusive),
				Range.BoundaryFromExclusive(value2));
		}

		[Test]
		public static void Test01BoundaryFromProperties()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;

			var a = new RangeBoundaryFrom<int?>();
			AreEqual(a.Kind, RangeBoundaryFromKind.Empty);
			AreEqual(a.GetValueOrDefault(), empty);
			Throws<InvalidOperationException>(() => a.Value.ToString());

			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsFalse(a.HasValue);
			IsFalse(a.IsNegativeInfinity);
			IsFalse(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);

			a = Range.BoundaryFromInfinity<int?>();
			AreEqual(a.Kind, RangeBoundaryFromKind.Infinite);
			AreEqual(a.GetValueOrDefault(), empty);
			Throws<InvalidOperationException>(() => a.Value.ToString());

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.HasValue);
			IsTrue(a.IsNegativeInfinity);
			IsFalse(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);

			a = Range.BoundaryFrom(value1);
			AreEqual(a.Kind, RangeBoundaryFromKind.Inclusive);
			AreEqual(a.Value, value1);
			AreNotEqual(a.Value, value2);

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.HasValue);
			IsFalse(a.IsNegativeInfinity);
			IsTrue(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);

			a = Range.BoundaryFromExclusive(value1);
			AreEqual(a.Kind, RangeBoundaryFromKind.Exclusive);
			AreEqual(a.Value, value1);
			AreNotEqual(a.Value, value2);

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.HasValue);
			IsFalse(a.IsNegativeInfinity);
			IsFalse(a.IsInclusiveBoundary);
			IsTrue(a.IsExclusiveBoundary);
		}

		[Test]
		public static void Test02BoundaryFromEquality()
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

		[Test]
		public static void Test03BoundaryFromComplementation()
		{
			int? value1 = 1;
			int? value2 = 2;

			var boundary = Range.BoundaryFrom(value1);
			var boundaryCompl = boundary.GetComplementation();
			IsTrue(boundaryCompl.IsComplementationFor(boundary));
			IsFalse(boundaryCompl.IsComplementationFor(Range.BoundaryFrom(value2)));
			IsFalse(boundaryCompl.IsComplementationFor(RangeBoundaryFrom<int?>.Empty));
			IsFalse(boundaryCompl.IsComplementationFor(RangeBoundaryFrom<int?>.NegativeInfinity));
			AreEqual(boundaryCompl.Value, 1);
			AreEqual(boundaryCompl.Kind, RangeBoundaryToKind.Exclusive);
			AreEqual(boundaryCompl.GetComplementation(), boundary);

			boundary = Range.BoundaryFromExclusive(value1);
			boundaryCompl = boundary.GetComplementation();
			IsTrue(boundaryCompl.IsComplementationFor(boundary));
			IsFalse(boundaryCompl.IsComplementationFor(Range.BoundaryFrom(value2)));
			IsFalse(boundaryCompl.IsComplementationFor(RangeBoundaryFrom<int?>.Empty));
			IsFalse(boundaryCompl.IsComplementationFor(RangeBoundaryFrom<int?>.NegativeInfinity));
			AreEqual(boundaryCompl.Value, 1);
			AreEqual(boundaryCompl.Kind, RangeBoundaryToKind.Inclusive);
			AreEqual(boundaryCompl.GetComplementation(), boundary);

			boundary = RangeBoundaryFrom<int?>.Empty;
			Throws<InvalidOperationException>(() => boundary.GetComplementation());
			boundary = RangeBoundaryFrom<int?>.NegativeInfinity;
			Throws<InvalidOperationException>(() => boundary.GetComplementation());
		}

		[Test]
		public static void Test04BoundaryFromUpdate()
		{
			int? value1 = 1;
			int? value2 = 2;

			var boundary = Range.BoundaryFrom(value1);
			var boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, value2);

			boundary = Range.BoundaryFromExclusive(value1);
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, value2);

			boundary = RangeBoundaryFrom<int?>.Empty;
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary, boundary2);
			boundary = RangeBoundaryFrom<int?>.NegativeInfinity;
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary, boundary2);
		}
	}
}