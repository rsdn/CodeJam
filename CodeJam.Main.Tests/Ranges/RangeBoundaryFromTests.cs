﻿using System;
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
	public class RangeBoundaryFromTests
	{
		[Test]
		public void TestBoundaryFromCreation()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;

			DoesNotThrow(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Empty));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Infinite));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Exclusive));
			DoesNotThrow(() => new RangeBoundaryFrom<int?>(value2, RangeBoundaryFromKind.Inclusive));
			DoesNotThrow(() => new RangeBoundaryFrom<double?>(double.NaN, RangeBoundaryFromKind.Empty));

			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Empty));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(value2, RangeBoundaryFromKind.Infinite));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Inclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<int?>(empty, RangeBoundaryFromKind.Exclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(double.NaN, RangeBoundaryFromKind.Exclusive));

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
			AreNotEqual(
				RangeBoundaryFrom<int>.NegativeInfinity,
				Range.BoundaryFrom(0));
			AreEqual(
				new RangeBoundaryFrom<int?>(value1, RangeBoundaryFromKind.Exclusive),
				Range.BoundaryFromExclusive(value1));
		}

		[Test]
		public void TestBoundaryNegativeInfinityValue()
		{
			double? infOk = double.NegativeInfinity;
			double? infFail = double.PositiveInfinity;
			double? empty = null;

			DoesNotThrow(() => new RangeBoundaryFrom<double?>(infOk, RangeBoundaryFromKind.Infinite));

			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infOk, RangeBoundaryFromKind.Empty));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infFail, RangeBoundaryFromKind.Empty));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infFail, RangeBoundaryFromKind.Infinite));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infOk, RangeBoundaryFromKind.Inclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infFail, RangeBoundaryFromKind.Inclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infOk, RangeBoundaryFromKind.Exclusive));
			Throws<ArgumentException>(() => new RangeBoundaryFrom<double?>(infFail, RangeBoundaryFromKind.Exclusive));

			AreEqual(
				RangeBoundaryFrom<double?>.NegativeInfinity,
				Range.BoundaryFrom(infOk));
			AreEqual(
				RangeBoundaryFrom<double?>.NegativeInfinity,
				Range.BoundaryFromExclusive(infOk));

			AreEqual(
				RangeBoundaryFrom<double>.NegativeInfinity,
				Range.BoundaryFrom(infOk.Value));
			AreEqual(
				RangeBoundaryFrom<double>.NegativeInfinity,
				Range.BoundaryFromExclusive(infOk.Value));

			AreEqual(
				Range.BoundaryFromExclusive(infOk).GetValueOrDefault(),
				empty);
		}

		[Test]
		public void TestBoundaryFromProperties()
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
		public void TestBoundaryFromEquality()
		{
			double? value1 = 1;
			double? value2 = 2;
			double? empty = null;

			var e = new RangeBoundaryFrom<double?>();
			var inf = Range.BoundaryFrom(empty);
			var a1 = Range.BoundaryFrom(value1);
			var a12 = Range.BoundaryFrom(value1);
			var a2 = Range.BoundaryFrom(value2);
			var b1 = Range.BoundaryTo(value1);

			AreEqual(e, RangeBoundaryFrom<double?>.Empty);
			IsTrue(e == RangeBoundaryFrom<double?>.Empty);
			IsFalse(e != RangeBoundaryFrom<double?>.Empty);

			AreEqual(inf, RangeBoundaryFrom<double?>.NegativeInfinity);
			IsTrue(inf == RangeBoundaryFrom<double?>.NegativeInfinity);

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
		public void TestBoundaryFromComplementation()
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
		public void TestBoundaryFromWithValue()
		{
			int? value1 = 1;
			int? value2 = 2;

			var boundary = Range.BoundaryFrom(value1);
			var boundary2 = boundary.WithValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, value2);

			boundary = Range.BoundaryFromExclusive(value1);
			boundary2 = boundary.WithValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, value2);

			boundary = RangeBoundaryFrom<int?>.Empty;
			boundary2 = boundary.WithValue(i => i + 1);
			AreEqual(boundary, boundary2);
			boundary = RangeBoundaryFrom<int?>.NegativeInfinity;
			boundary2 = boundary.WithValue(i => i + 1);
			AreEqual(boundary, boundary2);
		}

		[Test]
		public void TestBoundaryFromToExclusive()
		{
			int? value1 = 1;
			int? empty = null;

			var boundary = RangeBoundaryFrom<int?>.Empty;
			var boundary2 = boundary.ToExclusive();
			IsTrue(boundary2.IsEmpty);

			boundary = Range.BoundaryFrom(empty);
			boundary2 = boundary.ToExclusive();
			IsTrue(boundary2.IsNegativeInfinity);

			boundary = Range.BoundaryFrom(value1);
			boundary2 = boundary.ToExclusive();
			AreEqual(boundary2.Value, boundary.Value);
			AreEqual(boundary2.Kind, RangeBoundaryFromKind.Exclusive);

			boundary = Range.BoundaryFromExclusive(value1);
			boundary2 = boundary.ToExclusive();
			AreEqual(boundary2.Value, boundary.Value);
			AreEqual(boundary2.Kind, RangeBoundaryFromKind.Exclusive);
		}

		[Test]
		public void TestBoundaryFromToInclusive()
		{
			int? value1 = 1;
			int? empty = null;

			var boundary = RangeBoundaryFrom<int?>.Empty;
			var boundary2 = boundary.ToInclusive();
			IsTrue(boundary2.IsEmpty);

			boundary = Range.BoundaryFrom(empty);
			boundary2 = boundary.ToInclusive();
			IsTrue(boundary2.IsNegativeInfinity);

			boundary = Range.BoundaryFrom(value1);
			boundary2 = boundary.ToInclusive();
			AreEqual(boundary2.Value, boundary.Value);
			AreEqual(boundary2.Kind, RangeBoundaryFromKind.Inclusive);

			boundary = Range.BoundaryFromExclusive(value1);
			boundary2 = boundary.ToInclusive();
			AreEqual(boundary2.Value, boundary.Value);
			AreEqual(boundary2.Kind, RangeBoundaryFromKind.Inclusive);
		}
	}
}