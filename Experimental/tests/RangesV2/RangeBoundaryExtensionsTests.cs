using System;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.RangesV2
{
	using IntBoundary = RangeBoundary<int?>;

	[TestFixture(Category = "Ranges")]
	public class RangeBoundaryExtensionsTests
	{
		[Test]
		public static void Test01Complementation()
		{
			var boundary = new IntBoundary(1, RangeBoundaryKind.FromExclusive);
			var boundaryCompl = boundary.GetComplementation();
			IsTrue(boundaryCompl.IsComplementationFor(boundary));
			IsFalse(boundaryCompl.IsComplementationFor(boundaryCompl));
			IsFalse(boundaryCompl.IsComplementationFor(new IntBoundary(2, RangeBoundaryKind.FromExclusive)));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.NegativeInfinity));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.PositiveInfinity));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.Empty));
			AreEqual(boundaryCompl.Value, 1);
			AreEqual(boundaryCompl.Kind, RangeBoundaryKind.ToInclusive);
			AreEqual(boundaryCompl.GetComplementation(), boundary);

			boundary = new IntBoundary(1, RangeBoundaryKind.ToInclusive);
			boundaryCompl = boundary.GetComplementation();
			IsTrue(boundaryCompl.IsComplementationFor(boundary));
			IsFalse(boundaryCompl.IsComplementationFor(boundaryCompl));
			IsFalse(boundaryCompl.IsComplementationFor(new IntBoundary(2, RangeBoundaryKind.ToInclusive)));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.NegativeInfinity));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.PositiveInfinity));
			IsFalse(boundaryCompl.IsComplementationFor(IntBoundary.Empty));
			AreEqual(boundaryCompl.Value, 1);
			AreEqual(boundaryCompl.Kind, RangeBoundaryKind.FromExclusive);
			AreEqual(boundaryCompl.GetComplementation(), boundary);

			boundary = IntBoundary.Empty;
			Assert.Throws<ArgumentOutOfRangeException>(() => boundary.GetComplementation());
			boundary = IntBoundary.PositiveInfinity;
			Assert.Throws<ArgumentOutOfRangeException>(() => boundary.GetComplementation());
			boundary = IntBoundary.NegativeInfinity;
			Assert.Throws<ArgumentOutOfRangeException>(() => boundary.GetComplementation());
		}

		[Test]
		public static void Test02Update()
		{
			var boundary = new IntBoundary(1, RangeBoundaryKind.FromExclusive);
			var boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, 2);

			boundary = new IntBoundary(1, RangeBoundaryKind.ToInclusive);
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary.Kind, boundary2.Kind);
			AreEqual(boundary2.Value, 2);

			boundary = IntBoundary.Empty;
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary, boundary2);
			boundary = IntBoundary.NegativeInfinity;
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary, boundary2);
			boundary = IntBoundary.PositiveInfinity;
			boundary2 = boundary.UpdateValue(i => i + 1);
			AreEqual(boundary, boundary2);
		}
	}
}