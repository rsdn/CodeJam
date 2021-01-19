using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class MinMaxTest
	{
		public struct Comparable : IComparable
		{
			public Comparable(int value)
			{
				Value = value;
			}

			public int Value { get; }

			#region Implementation of IComparable
			public int CompareTo([AllowNull] object obj)
			{
				if (obj is not Comparable other) return 1;

				return Value.CompareTo(other.Value);
			}
			#endregion
		}

		[Test]
		public void TestMinMaxTrivial()
		{
			var c1 = 1M;
			var c2 = 2M;

			Assert.AreEqual(c1, Algorithms.Min(c1, c2));
			Assert.AreEqual(c1, Algorithms.Min(c2, c1));
			Assert.AreEqual(c2, Algorithms.Max(c1, c2));
			Assert.AreEqual(c2, Algorithms.Max(c2, c1));
		}

		[Test]
		public void TestMinMaxOverIComparable()
		{
			var c1 = new Comparable(1);
			var c2 = new Comparable(2);

			Assert.AreEqual(c1, Algorithms.Min(c1, c2));
			Assert.AreEqual(c1, Algorithms.Min(c2, c1));
			Assert.AreEqual(c2, Algorithms.Max(c1, c2));
			Assert.AreEqual(c2, Algorithms.Max(c2, c1));
		}

		[Test]
		public void TestMinMaxOverNullable()
		{
			Comparable? c1 = new Comparable(1);
			Comparable? cMinus1 = new Comparable(-1);
			Comparable? c2 = new Comparable(2);

			Assert.AreEqual(c1, Algorithms.Min(c1, c2));
			Assert.AreEqual(c1, Algorithms.Min(c2, c1));
			Assert.AreEqual(c2, Algorithms.Max(c1, c2));
			Assert.AreEqual(c2, Algorithms.Max(c2, c1));

			Assert.AreEqual(null, Algorithms.Min(c1, null));
			Assert.AreEqual(null, Algorithms.Min(null, c1));
			Assert.AreEqual(c1, Algorithms.Max(null, c1));
			Assert.AreEqual(c1, Algorithms.Max(c1, null));

			Assert.AreEqual(null, Algorithms.Min(cMinus1, null));
			Assert.AreEqual(null, Algorithms.Min(null, cMinus1));
			Assert.AreEqual(cMinus1, Algorithms.Max(null, cMinus1));
			Assert.AreEqual(cMinus1, Algorithms.Max(cMinus1, null));
		}

		[Test]
		public void TestMinMaxOverNullableEnum()
		{
			ConsoleColor? c1 = (ConsoleColor)1;
			ConsoleColor? cMinus1 = (ConsoleColor)(-1);
			ConsoleColor? c2 = (ConsoleColor)2;

			Assert.AreEqual(c1, Algorithms.Min(c1, c2));
			Assert.AreEqual(c1, Algorithms.Min(c2, c1));
			Assert.AreEqual(c2, Algorithms.Max(c1, c2));
			Assert.AreEqual(c2, Algorithms.Max(c2, c1));

			Assert.AreEqual(null, Algorithms.Min(c1, null));
			Assert.AreEqual(null, Algorithms.Min(null, c1));
			Assert.AreEqual(c1, Algorithms.Max(null, c1));
			Assert.AreEqual(c1, Algorithms.Max(c1, null));

			Assert.AreEqual(null, Algorithms.Min(cMinus1, null));
			Assert.AreEqual(null, Algorithms.Min(null, cMinus1));
			Assert.AreEqual(cMinus1, Algorithms.Max(null, cMinus1));
			Assert.AreEqual(cMinus1, Algorithms.Max(cMinus1, null));
		}
	}
}