using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Ranges.v2
{
	using IntBoundary = RangeBoundary<int?>;

	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	public class RangeBoundaryTests
	{
		[Test]
		public static void Test01BoundaryProperties()
		{
			var value1 = 1;
			var value2 = 2;
			var empty = default(int?);

			var a = new IntBoundary();
			AreEqual(a.Kind, RangeBoundaryKind.Empty);
			AreEqual(a.GetValueOrDefault(), empty);
			Throws<InvalidOperationException>(() => a.Value.ToString());

			IsTrue(a.IsEmpty);
			IsFalse(a.IsNotEmpty);
			IsFalse(a.HasValue);

			IsFalse(a.IsInfinity);
			IsFalse(a.IsNegativeInfinity);
			IsFalse(a.IsPositiveInfinity);

			IsFalse(a.IsFromBoundary);
			IsFalse(a.IsToBoundary);
			IsFalse(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);

			a = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			AreEqual(a.Kind, RangeBoundaryKind.FromInclusive);
			AreEqual(a.Value, value1);

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.HasValue);

			IsFalse(a.IsInfinity);
			IsFalse(a.IsNegativeInfinity);
			IsFalse(a.IsPositiveInfinity);

			IsTrue(a.IsFromBoundary);
			IsFalse(a.IsToBoundary);
			IsTrue(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);

			a = new IntBoundary(value2, RangeBoundaryKind.ToExclusive);
			AreEqual(a.Kind, RangeBoundaryKind.ToExclusive);
			AreNotEqual(a.Value, value1);
			AreEqual(a.Value, value2);

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsTrue(a.HasValue);

			IsFalse(a.IsInfinity);
			IsFalse(a.IsNegativeInfinity);
			IsFalse(a.IsPositiveInfinity);

			IsFalse(a.IsFromBoundary);
			IsTrue(a.IsToBoundary);
			IsFalse(a.IsInclusiveBoundary);
			IsTrue(a.IsExclusiveBoundary);

			a = new IntBoundary(empty, RangeBoundaryKind.PositiveInfinity);
			AreEqual(a.Kind, RangeBoundaryKind.PositiveInfinity);
			AreEqual(a.GetValueOrDefault(), empty);
			Throws<InvalidOperationException>(() => a.Value.ToString());

			IsFalse(a.IsEmpty);
			IsTrue(a.IsNotEmpty);
			IsFalse(a.HasValue);

			IsTrue(a.IsInfinity);
			IsFalse(a.IsNegativeInfinity);
			IsTrue(a.IsPositiveInfinity);

			IsFalse(a.IsFromBoundary);
			IsTrue(a.IsToBoundary);
			IsFalse(a.IsInclusiveBoundary);
			IsFalse(a.IsExclusiveBoundary);
		}

		[Test]
		public static void Test02CtorValidation()
		{
			var value1 = 1;
			var value2 = 2;
			var empty = default(int?);

			Throws<ArgumentException>(() => new IntBoundary(value1, RangeBoundaryKind.Empty));
			Throws<ArgumentException>(() => new IntBoundary(value2, RangeBoundaryKind.NegativeInfinity));
			Throws<ArgumentException>(() => new IntBoundary(empty, RangeBoundaryKind.FromExclusive));
			Throws<ArgumentException>(() => new IntBoundary(empty, RangeBoundaryKind.ToInclusive));

			DoesNotThrow(() => new IntBoundary(empty, RangeBoundaryKind.Empty));
			DoesNotThrow(() => new IntBoundary(empty, RangeBoundaryKind.NegativeInfinity));
			DoesNotThrow(() => new IntBoundary(value1, RangeBoundaryKind.FromExclusive));
			DoesNotThrow(() => new IntBoundary(value2, RangeBoundaryKind.ToInclusive));
		}

		[Test]
		public static void Test03ToString()
		{
			var value1 = 1;

			AreEqual(IntBoundary.Empty.ToString(), "∅");
			AreEqual(IntBoundary.NegativeInfinity.ToString(), "(-∞");
			AreEqual(IntBoundary.PositiveInfinity.ToString(), "+∞)");

			AreEqual(new IntBoundary(value1, RangeBoundaryKind.FromInclusive).ToString(), "[" + value1);
			AreEqual(new IntBoundary(value1, RangeBoundaryKind.FromExclusive).ToString(), "(" + value1);
			AreEqual(new IntBoundary(value1, RangeBoundaryKind.ToInclusive).ToString(), value1 + "]");
			AreEqual(new IntBoundary(value1, RangeBoundaryKind.ToExclusive).ToString(), value1 + ")");
		}

		[Test]
		public static void Test04Equality()
		{
			var value1 = 1;
			var value2 = 2;
			var empty = default(int?);

			var e = new IntBoundary();
			var nInf = new IntBoundary(empty, RangeBoundaryKind.NegativeInfinity);
			var pInf = new IntBoundary(empty, RangeBoundaryKind.PositiveInfinity);
			var a = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			var a2 = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			var b = new IntBoundary(value2, RangeBoundaryKind.ToInclusive);
			var b2 = new IntBoundary(value2, RangeBoundaryKind.FromInclusive);

			AreEqual(e, IntBoundary.Empty);
			IsTrue(e == IntBoundary.Empty);
			IsFalse(e != IntBoundary.Empty);
			IsTrue(IntBoundary.Equals(e, IntBoundary.Empty));

			AreEqual(nInf, IntBoundary.NegativeInfinity);
			IsTrue(pInf == IntBoundary.PositiveInfinity);

			AreNotEqual(a, empty);
			AreNotEqual(a, nInf);
			AreNotEqual(a, pInf);

			AreEqual(a, a2);
			IsTrue(a == a2);
			IsFalse(a != a2);
			IsTrue(IntBoundary.Equals(a, a2));

			AreNotEqual(a, b);
			IsFalse(a == b);
			IsTrue(a != b);
			IsFalse(IntBoundary.Equals(a, b));

			AreEqual(a.Value, value1);
			AreNotEqual(a.Value, value2);
			AreNotEqual(a, value1);
			AreNotEqual(a, value2);

			AreNotEqual(b, b2);
			AreEqual(b.Value, b2.Value);
		}

		[Test]
		public static void Test05ComparisonBoundaryKinds()
		{
			var value1 = 1;

			var e = IntBoundary.Empty;
			var nInf = IntBoundary.NegativeInfinity;
			var pInf = IntBoundary.PositiveInfinity;
			var fromInc = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			var fromEx = new IntBoundary(value1, RangeBoundaryKind.FromExclusive);
			var toInc = new IntBoundary(value1, RangeBoundaryKind.ToInclusive);
			var toEx = new IntBoundary(value1, RangeBoundaryKind.ToExclusive);
			var toInc2 = new IntBoundary(value1, RangeBoundaryKind.ToInclusive);

			// Proofs that same value but different boundary kind are not equal
			AreNotEqual(fromInc, fromEx);
			AreNotEqual(fromInc, toInc);
			AreNotEqual(toInc, toEx);
			IsFalse(fromInc == fromEx);
			IsFalse(fromInc == toInc);
			IsFalse(toInc == toEx);
			AreEqual(fromInc.Value, fromEx.Value);
			AreEqual(fromInc.Value, toInc.Value);
			AreEqual(toInc.Value, toEx.Value);

			// priority:
			// '∅' < '-∞' < 'a)' < '[a' == 'a]' < '(a' < '+∞'
			Less(e, nInf);
			Less(nInf, toEx);
			Less(toEx, fromInc);
			AreEqual(fromInc.CompareTo(toInc), 0);
			Less(toInc, fromEx);
			Less(fromEx, pInf);
			// Additional proof for '[a' == 'a]' comparison
			LessOrEqual(fromInc, toInc);
			GreaterOrEqual(fromInc, toInc);

			IsTrue(e < nInf);
			IsTrue(nInf < toEx);
			IsTrue(toEx < fromInc);
			IsTrue(toInc <= fromInc && toInc >= fromInc);
			IsFalse(toInc == fromInc);
			IsTrue(fromInc < fromEx);
			IsTrue(fromEx < pInf);

			IsTrue(pInf >= fromEx);
			IsTrue(fromEx >= fromInc);
			IsTrue(fromInc >= toInc);
			IsTrue(fromInc >= toEx);
			IsTrue(toEx >= nInf);
			IsTrue(nInf >= e);

			IsTrue(IntBoundary.CompareTo(toInc, toInc2) == 0);
			IsTrue(IntBoundary.CompareTo(toInc, nInf) > 0);
			IsTrue(IntBoundary.CompareTo(toInc, pInf) < 0);
		}

		[Test]
		public static void Test06ComparisonValues()
		{
			var value1 = 1;
			var value2 = 2;

			var e = IntBoundary.Empty;
			var nInf = IntBoundary.NegativeInfinity;
			var pInf = IntBoundary.PositiveInfinity;

			var fromInc1 = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			var fromEx1 = new IntBoundary(value1, RangeBoundaryKind.FromExclusive);
			var toInc1 = new IntBoundary(value1, RangeBoundaryKind.ToInclusive);
			var toEx1 = new IntBoundary(value1, RangeBoundaryKind.ToExclusive);

			var fromInc2 = new IntBoundary(value2, RangeBoundaryKind.FromInclusive);
			var fromEx2 = new IntBoundary(value2, RangeBoundaryKind.FromExclusive);
			var toInc2 = new IntBoundary(value2, RangeBoundaryKind.ToInclusive);
			var toEx2 = new IntBoundary(value2, RangeBoundaryKind.ToExclusive);

			IsTrue(e < nInf);
			IsTrue(nInf < toEx1);
			IsTrue(toEx1 < fromInc1);
			IsTrue(fromInc1 <= toInc1);
			IsTrue(toInc1 < fromEx1);
			IsTrue(fromEx1 < toEx2);
			IsTrue(toEx2 < fromInc2);
			IsTrue(fromInc2 <= toInc2);
			IsTrue(toInc2 < fromEx2);
			IsTrue(fromEx2 < pInf);
		}


		[Test]
		public static void Test07ComparisonRawValue()
		{
			var value1 = 1;
			var value2 = 2;

			var e = IntBoundary.Empty;
			var nInf = IntBoundary.NegativeInfinity;
			var pInf = IntBoundary.PositiveInfinity;

			var fromInc1 = new IntBoundary(value1, RangeBoundaryKind.FromInclusive);
			var fromEx1 = new IntBoundary(value1, RangeBoundaryKind.FromExclusive);
			var toInc1 = new IntBoundary(value1, RangeBoundaryKind.ToInclusive);
			var toEx1 = new IntBoundary(value1, RangeBoundaryKind.ToExclusive);

			var fromInc2 = new IntBoundary(value2, RangeBoundaryKind.FromInclusive);
			var fromEx2 = new IntBoundary(value2, RangeBoundaryKind.FromExclusive);
			var toInc2 = new IntBoundary(value2, RangeBoundaryKind.ToInclusive);
			var toEx2 = new IntBoundary(value2, RangeBoundaryKind.ToExclusive);

			IsTrue(value1 < value2);

			IsTrue(toEx1 < value1);
			IsTrue(fromEx1 > value1);
			IsTrue(toInc1 <= value1);
			IsTrue(fromInc1 >= value1);

			IsTrue(toEx1 < value2);
			IsTrue(fromEx1 < value2);
			IsTrue(toInc1 < value2);
			IsTrue(fromInc1 < value2);

			IsTrue(value2 > toEx2);
			IsTrue(value2 < fromEx2);
			IsTrue(value2 >= toInc2);
			IsTrue(value2 <= fromInc2);

			IsTrue(value1 < toEx2);
			IsTrue(value1 < fromEx2);
			IsTrue(value1 < toInc2);
			IsTrue(value1 < fromInc2);
		}
	}
}
