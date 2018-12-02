using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using CodeJam.PerfTests.Metrics;

using NUnit.Framework;

using static NUnit.Framework.Assert;
using static CodeJam.PerfTests.Metrics.MetricRange;

namespace CodeJam.PerfTests
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class MetricRangeTests
	{
		[Test]
		public static void TestEmptyMetricRange()
		{
			var r = new MetricRange(EmptyMetricValue, EmptyMetricValue);
			var r2 = new MetricRange(1, 2);

			AreEqual(r.Min, EmptyMetricValue);
			AreEqual(r.Max, EmptyMetricValue);
			IsTrue(r.IsEmpty);
			IsTrue(r.Equals(Empty));
			IsFalse(r.Equals(r2));
			IsTrue(r == Empty);
			IsTrue(r != r2);
			IsFalse(r.Contains(r2));
			IsTrue(r.Contains(Empty));
			IsTrue(r.Union(r2) == r2);
			IsTrue(r.Union(r2).IsNotEmpty);
			IsTrue(r.Union(r2) != r);
			IsTrue(r.Union(Empty) == r);
			AreEqual(r.ToString("F2"), "∅");
		}

		[Test]
		public static void TestInfiniteMetricRange()
		{
			var r = new MetricRange(FromNegativeInfinity, ToPositiveInfinity);
			var r2 = new MetricRange(1, 2);

			AreEqual(r.Min, FromNegativeInfinity);
			AreEqual(r.Max, ToPositiveInfinity);
			IsFalse(r.IsEmpty);
			IsFalse(r.Equals(Empty));
			IsFalse(r.Equals(r2));
			IsTrue(r == Infinite);
			IsTrue(r != r2);
			IsTrue(r.Contains(r2));
			IsFalse(r.Contains(Empty));
			IsTrue(r.Union(r2) == r);
			IsTrue(r.Union(r2).IsNotEmpty);
			IsTrue(r.Union(r2) != r2);
			IsTrue(r.Union(Empty) == r);
			AreEqual(r.ToString("F2"), "(-∞..+∞)");
		}

		[Test]
		public static void TestMetricRangeWithValues()
		{
			var r = new MetricRange(1, 5);
			var r2 = new MetricRange(1, 2);
			var r3 = new MetricRange(10, 20);

			AreEqual(r.Min, 1);
			AreEqual(r.Max, 5);
			IsFalse(r.IsEmpty);
			IsFalse(r.Equals(Empty));
			IsFalse(r.Equals(r2));
			IsFalse(r == Infinite);
			IsTrue(r != r2);
			IsTrue(r.Contains(r2));
			IsFalse(r.Contains(Empty));
			IsTrue(r.Union(r2) == r);
			IsTrue(r.Union(r2).IsNotEmpty);
			IsTrue(r.Union(r2) != r2);
			IsTrue(r.Union(Empty) == r);
			AreEqual(r.ToString("F2", CultureInfo.InvariantCulture), "[1.00..5.00]");
			AreEqual(r.Union(r3).ToString("F0", CultureInfo.InvariantCulture), "[1..20]");
		}

		[Test]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		public static void TestMetricRangeArgValidation()
		{
			Throws<ArgumentException>(() => new MetricRange(2, 1));
			Throws<ArgumentException>(() => new MetricRange(ToPositiveInfinity, 1));
			Throws<ArgumentException>(() => new MetricRange(1, FromNegativeInfinity));
			DoesNotThrow(() => new MetricRange(1, 1));
			DoesNotThrow(() => new MetricRange(-1, -1));
		}
	}
}