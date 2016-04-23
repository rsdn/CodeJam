using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Ranges
{
	[TestFixture]
	public class RangeTests
	{
		[Test]
		public void Contains()
		{
			var range = Range.Create(0, 2);

			Assert.IsTrue(range.Contains(0));
			Assert.IsTrue(range.Contains(1));
			Assert.IsTrue(range.Contains(2));

			range = Range.Create(0, 2, false);

			Assert.IsFalse(range.Contains(0));
			Assert.IsTrue(range.Contains(1));
			Assert.IsFalse(range.Contains(2));

			Assert.IsFalse(Range.Empty<int>().Contains(10));
		}

		[Test]
		public void CompareTo()
		{
			Assert.AreEqual(-1, Range.StartsWith(1).CompareTo(Range.StartsWith(1, false)));
			Assert.AreEqual(0, Range.StartsWith(1).CompareTo(Range.StartsWith(1)));
			Assert.AreEqual(0, Range.StartsWith(1, false).CompareTo(Range.StartsWith(1, false)));
			Assert.AreEqual(1, Range.StartsWith(1, false).CompareTo(Range.StartsWith(1, true)));

			Assert.AreEqual(0, Range.Create(1, 3, true).CompareTo(Range.Create(1, 3, true)));
			Assert.AreEqual(0, Range.Create(1, 3, false).CompareTo(Range.Create(1, 3, false)));

			Assert.AreEqual(1, Range.Create(1, 3, true, true).CompareTo(Range.Create(1, 3, true, false)));
			Assert.AreEqual(-1, Range.Create(1, 3, true, false).CompareTo(Range.Create(1, 3, true, true)));
		}

		[Test]
		public void IntersectsWith()
		{
			var range1 = Range.Create(0, 3);

			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(1, 5)));
			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(1, 3)));
			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(3, 5)));
			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(0, 0)));
			Assert.IsFalse(range1.IsIntersectsWith(Range.Create(-2, -1)));

			range1 = Range.Create(0, 3, false);

			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(1, 5, false)));
			Assert.IsTrue(range1.IsIntersectsWith(Range.Create(1, 3, false)));
			Assert.IsFalse(range1.IsIntersectsWith(Range.Create(3, 5, false)));
			Assert.IsFalse(range1.IsIntersectsWith(Range.Create(0, 0, false)));
			Assert.IsFalse(range1.IsIntersectsWith(Range.Create(-2, -1, false)));

			Assert.IsFalse(range1.IsIntersectsWith(Range.Empty<int>()));
			Assert.IsTrue(Range.StartsWith(0, false).IsIntersectsWith(Range.StartsWith(-2, false)));
		}

		[Test]
		[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
		public void Intersect()
		{
			CheckIntersect(Range.Full<int>(), Range.Full<int>(), Range.Full<int>());
			CheckIntersect(Range.Full<int>(), Range.Empty<int>(), Range.Empty<int>());
			CheckIntersect(Range.Full<int>(), Range.StartsWith(10), Range.StartsWith(10));
			CheckIntersect(Range.Empty<int>(), Range.Empty<int>(), Range.Empty<int>());

			CheckIntersect(Range.EndsWith(10, true), Range.Create(0, 10, true, true), Range.Create(0, 10, true));
			CheckIntersect(Range.EndsWith(10, false), Range.Create(0, 10, true, true), Range.Create(0, 10, true, false));

			CheckIntersect(Range.EndsWith(10, false), Range.StartsWith(10, true));
			CheckIntersect(Range.EndsWith(10, false), Range.StartsWith(10, false));

			CheckIntersect(Range.EndsWith(10, true), Range.StartsWith(10, true), Range.Simple(10));

			CheckIntersect(Range.EndsWith(10, true), Range.Create(0, 9, true), Range.Create(0, 9, true));
			CheckIntersect(Range.EndsWith(10, true), Range.Create(0, 9, true, false), Range.Create(0, 9, true, false));

			CheckIntersect(Range.Create(0, 10, true), Range.Create(0, 2, false, false), Range.Create(0, 2, false, false));
		}

		[Test]
		public void Exclude()
		{
			CheckExclude(Range.EndsWith(5, true), Range.Create(2, 3, true, true), "..(2)", "(3)..5");
			CheckExclude(Range.EndsWith(5, true), Range.Create(2, 5, true, false), "..(2)", "5");
			CheckExclude(Range.EndsWith(5, true), Range.Create(2, 5, true, true), "..(2)");
			CheckExclude(Range.EndsWith(5, false), Range.Create(2, 5, true, true), "..(2)");
			CheckExclude(Range.EndsWith(5, false), Range.Create(2, 5, true, false), "..(2)");

			CheckExclude(Range.Create(0, 5, true, true), Range.Create(2, 5, true, true), "0..(2)");
			CheckExclude(Range.Create(0, 5, true, true), Range.Create(2, 10, true, true), "0..(2)");
			CheckExclude(Range.Create(0, 5, true, true), Range.StartsWith(2, true), "0..(2)");
			CheckExclude(Range.Create(0, 5, true, true), Range.StartsWith(5, false), "0..5");
			CheckExclude(Range.Create(0, 5, true, true), Range.StartsWith(5, true), "0..(5)");
		}

		[Test]
		public void Union()
		{
			CheckUnion(Range<int>.Empty, Range<int>.Full, "...");

			CheckUnion(Range<int>.Empty, Range.StartsWith(5, true), "5..");
			CheckUnion(Range<int>.Empty, Range.StartsWith(5, false), "(5)..");

			CheckUnion(Range.StartsWith(5, false), Range.EndsWith(10, true), "...");
			CheckUnion(Range.StartsWith(5, false), Range.EndsWith(10, false), "...");

			CheckUnion(Range.StartsWith(5, true), Range.StartsWith(10, true), "5..");
			CheckUnion(Range.StartsWith(5, false), Range.StartsWith(10, true), "(5)..");

			CheckUnion(Range.EndsWith(5, true), Range.EndsWith(10, true), "..10");
			CheckUnion(Range.EndsWith(5, false), Range.EndsWith(10, false), "..(10)");
		}

		private static void CheckUnion<TValue>(Range<TValue> range1, Range<TValue> range2, string representation)
			where TValue : IComparable<TValue>
		{
			Assert.AreEqual(range1.Union(range2).GetDisplayValue(), representation);
			Assert.AreEqual(range2.Union(range1).GetDisplayValue(), representation);
		}

		private static void CheckExclude<TValue>(Range<TValue> range1, Range<TValue> range2, params string[] representation)
			where TValue : IComparable<TValue>
		{
			var result = range1.Exclude(range2).ToArray();
			Assert.AreEqual(representation.Length, result.Length);

			for (var i = 0; i < result.Length; i++)
				Assert.AreEqual(representation[i], result[i].GetDisplayValue());
		}

		private static void CheckIntersect<TValue>(Range<TValue> range1, Range<TValue> range2, Range<TValue> expected)
			where TValue : IComparable<TValue>
		{
			var intesection = range1.Intersect(range2);
			Assert.AreEqual(expected, intesection);

			intesection = range2.Intersect(range1);
			Assert.AreEqual(expected, intesection);
		}

		private static void CheckIntersect<TValue>(Range<TValue> range1, Range<TValue> range2)
			where TValue : IComparable<TValue>
		{
			var intesection = range1.Intersect(range2);
			Assert.AreEqual(true, intesection.IsEmpty, "Intesection should be empty");

			intesection = range2.Intersect(range1);
			Assert.AreEqual(true, intesection.IsEmpty, "Intesection should be empty");
		}
	}
}