using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Ranges
{
	[TestFixture(Category = "Ranges")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	public class RangeBoundaryFromToTests
	{
		[Test]
		public void TestBoundaryToString()
		{
			int? value1 = 1;

			AreEqual(RangeBoundaryFrom<int?>.Empty.ToString(), "∅");
			AreEqual(RangeBoundaryTo<int?>.Empty.ToString(), "∅");
			AreEqual(RangeBoundaryFrom<int?>.NegativeInfinity.ToString(), "(-∞");
			AreEqual(RangeBoundaryTo<int?>.PositiveInfinity.ToString(), "+∞)");

			AreEqual(Range.BoundaryFrom(value1).ToString(), "[1");
			AreEqual(Range.BoundaryFromExclusive(value1).ToString(), "(1");
			AreEqual(Range.BoundaryTo(value1).ToString(), "1]");
			AreEqual(Range.BoundaryToExclusive(value1).ToString(), "1)");

			AreEqual(Range.BoundaryToExclusive(value1).ToString("000"), "001)");
			AreEqual(RangeBoundaryTo<int?>.Empty.ToString("000"), "∅");
			AreEqual(RangeBoundaryFrom<int?>.NegativeInfinity.ToString("000"), "(-∞");
		}

		[Test]
		public void TestBoundaryKindComparison()
		{
			int? value1 = 1;

			var eFrom = RangeBoundaryFrom<int?>.Empty;
			var eTo = RangeBoundaryTo<int?>.Empty;
			var nInf = Range.BoundaryFromInfinity<int?>();
			var pInf = Range.BoundaryToInfinity<int?>();
			var fromInc = Range.BoundaryFrom(value1);
			var fromEx = Range.BoundaryFromExclusive(value1);
			var toInc = Range.BoundaryTo(value1);
			var toEx = Range.BoundaryToExclusive(value1);

			AreNotEqual(fromInc, fromEx);
			AreNotEqual(fromInc, toInc);
			AreNotEqual(toInc, toEx);
			AreEqual(fromInc.Value, fromEx.Value);
			AreEqual(fromInc.Value, toInc.Value);
			AreEqual(toInc.Value, toEx.Value);

			// priority:
			// '∅' < '-∞' < 'a)' < '[a' == 'a]' < '(a' < '+∞'
			AreEqual(eFrom.CompareTo(eTo), 0);
			AreEqual(eTo.CompareTo(eFrom), 0);
			Less(eFrom, nInf);
			Less(nInf, toEx);
			Less(toEx, fromInc);
			AreEqual(fromInc.CompareTo(toInc), 0);
			AreEqual(toInc.CompareTo(fromInc), 0);
			Less(toInc, fromEx);
			Less(fromEx, pInf);

			IsTrue(eTo <= eFrom);
			IsTrue(eFrom < nInf);
			IsTrue(nInf < toEx);
			IsTrue(toEx < fromInc);
			IsTrue(toInc <= fromInc && toInc >= fromInc);
			IsTrue(fromInc < fromEx);
			IsTrue(fromEx < pInf);

			IsTrue(pInf >= fromEx);
			IsTrue(fromEx >= fromInc);
			IsTrue(fromInc >= toInc);
			IsTrue(fromInc >= toEx);
			IsTrue(toEx >= nInf);
			IsTrue(nInf >= eFrom);
			IsTrue(eFrom >= eTo);
		}

		[Test]
		public void TestBoundaryValueComparison()
		{
			int? value1 = 1;
			int? value2 = 2;

			var eFrom = RangeBoundaryFrom<int?>.Empty;
			var nInf = Range.BoundaryFromInfinity<int?>();
			var pInf = Range.BoundaryToInfinity<int?>();

			var fromInc1 = Range.BoundaryFrom(value1);
			var fromEx1 = Range.BoundaryFromExclusive(value1);
			var toInc1 = Range.BoundaryTo(value1);
			var toEx1 = Range.BoundaryToExclusive(value1);

			var fromInc2 = Range.BoundaryFrom(value2);
			var fromEx2 = Range.BoundaryFromExclusive(value2);
			var toInc2 = Range.BoundaryTo(value2);
			var toEx2 = Range.BoundaryToExclusive(value2);

			IsTrue(eFrom < nInf);
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
		public void TestBoundaryRawValueComparison()
		{
			int? value1 = 1;
			int? value2 = 2;
			int? empty = null;

			var eFrom = RangeBoundaryFrom<int?>.Empty;
			var nInf = Range.BoundaryFromInfinity<int?>();
			var pInf = Range.BoundaryToInfinity<int?>();

			var fromInc1 = Range.BoundaryFrom(value1);
			var fromEx1 = Range.BoundaryFromExclusive(value1);
			var toInc1 = Range.BoundaryTo(value1);
			var toEx1 = Range.BoundaryToExclusive(value1);

			var fromInc2 = Range.BoundaryFrom(value2);
			var fromEx2 = Range.BoundaryFromExclusive(value2);
			var toInc2 = Range.BoundaryTo(value2);
			var toEx2 = Range.BoundaryToExclusive(value2);

			IsTrue(eFrom < empty);
			IsTrue(nInf <= empty);
			IsTrue(nInf >= empty);
			IsTrue(pInf > empty);

			IsTrue(toEx1 > empty);
			IsTrue(toInc1 > empty);
			IsTrue(fromEx2 > empty);
			IsTrue(fromInc2 > empty);

			IsTrue(empty < toEx2);
			IsTrue(empty < toInc2);
			IsTrue(empty < fromEx1);
			IsTrue(empty < fromInc1);

			IsTrue(nInf < value1);
			IsTrue(toEx1 < value1);
			IsTrue(fromEx1 > value1);
			IsTrue(toInc1 <= value1);
			IsTrue(fromInc1 >= value1);

			IsTrue(toEx1 < value2);
			IsTrue(fromEx1 < value2);
			IsTrue(toInc1 < value2);
			IsTrue(fromInc1 < value2);

			IsTrue(value2 < pInf);
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