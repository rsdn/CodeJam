using System;

using CodeJam.PerfTests.Metrics;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	public static class MetricRangeTests
	{
		[Test]
		public static void TestScaledValuesRoundedAndToString()
		{
			var metric = CompetitionMetricInfo.GcAllocations;

			var lastRange = MetricRange.Create(0, 0);
			int repeats = 0;
			int increment = 3;
			for (long i = 0; i < int.MaxValue; i += increment)
			{
				if (++repeats >= 100)
				{
					repeats = 0;
					increment *= 2;
				}

				var unit = metric.MetricUnits[i];
				var newRange = MetricRange.Create(i, i);
				var minMaxRange = MetricRange.Create(lastRange.Min, newRange.Max);
				Assert.IsTrue(minMaxRange == lastRange.Union(newRange));

				var newRangeRounded = newRange.ToScaledValuesRounded(unit);
				var lastRangeRounded = lastRange.ToScaledValuesRounded(unit);

				if (lastRangeRounded == newRangeRounded)
				{
					Assert.IsTrue(newRange.ToString(unit) == lastRange.ToString(unit));
					Assert.IsTrue(lastRangeRounded == newRangeRounded);
					Assert.IsTrue(minMaxRange.ToScaledValuesRounded(unit) == newRangeRounded);
				}
				else
				{
					Assert.IsFalse(newRange.ToString(unit) == lastRange.ToString(unit));
					Assert.IsFalse(lastRangeRounded == newRangeRounded);
					Assert.IsFalse(minMaxRange.ToScaledValuesRounded(unit) == newRangeRounded);

					lastRange = newRange;
				}
			}
		}

		private static void TestMetricRangeWithUnitCore(double min, double max, Enum unit, string expected, string expectedAutoscaled)
		{
			var units = MetricUnits.GetMetricUnits(unit?.GetType());
			var metricUnit = units[unit];
			var range = MetricRange.Create(min, max).ToNormalizedMetricValues(metricUnit);

			Assert.AreEqual(
				range.ToString(metricUnit),
				expected);
			Assert.AreEqual(
				range.ToString(units),
				expectedAutoscaled);
		}
		[TestCase(double.NaN, double.NaN, "∅")]
		[TestCase(double.NegativeInfinity, 1000, "(-∞..1000.0]")]
		[TestCase(double.NegativeInfinity, double.PositiveInfinity, "(-∞..+∞)")]
		[TestCase(1.0, 2.004, "[1.00..2.00]")]
		[TestCase(0.001, 1.1,  "[0.00100..1.10000]")]
		[TestCase(0.0, 0.0, "[0.00..0.00]")]
		public static void RelativeMetricRangeTests(double min, double max, string expected) =>
			TestMetricRangeWithUnitCore(min, max, null, expected, expected);

		[TestCase(double.NaN, double.NaN, TimeUnit.Second, "∅", "∅")]
		[TestCase(double.NegativeInfinity, 1000, TimeUnit.Millisecond, "(-∞..1000.0] ms", "(-∞..1.00] sec")]
		[TestCase(double.NegativeInfinity, 1000, TimeUnit.Second, "(-∞..1000.0] sec", "(-∞..1000.0] sec")]
		[TestCase(double.NegativeInfinity, double.PositiveInfinity, TimeUnit.Microsecond, "(-∞..+∞) us", "(-∞..+∞) sec")]
		[TestCase(1.0, 2.004, TimeUnit.Second, "[1.00..2.00] sec", "[1.00..2.00] sec")]
		[TestCase(0.001, 1.1, TimeUnit.Microsecond, "[0.00100..1.10000] us", "[1.00..1100.00] ns")]
		[TestCase(0.0, 0.0, TimeUnit.Microsecond, "[0.00..0.00] us", "[0.00..0.00] ns")]
		public static void TimeMetricRangeTests(double min, double max, TimeUnit timeUnit, string expected, string expectedAutoscaled) => 
			TestMetricRangeWithUnitCore(min, max, timeUnit, expected, expectedAutoscaled);



		[TestCase(double.NaN, double.NaN, BinarySizeUnit.Gigabyte, "∅", "∅")]
		[TestCase(double.NegativeInfinity, 1024, BinarySizeUnit.Megabyte, "(-∞..1024.0] MB", "(-∞..1.00] GB")]
		[TestCase(double.NegativeInfinity, 1024, BinarySizeUnit.Petabyte, "(-∞..1024.0] PB", "(-∞..1024.0] PB")]
		[TestCase(double.NegativeInfinity, double.PositiveInfinity, BinarySizeUnit.Kilobyte, "(-∞..+∞) KB", "(-∞..+∞) PB")]
		[TestCase(1.0, 2.004, BinarySizeUnit.Byte, "[1..2] B", "[1..2] B")]
		[TestCase(0.001, 1.1, BinarySizeUnit.Megabyte, "[0.00100..1.10000] MB", "[1.02..1126.40] KB")]
		[TestCase(0.0, 0.0, BinarySizeUnit.Kilobyte, "[0.00..0.00] KB", "[0..0] B")]
		public static void BinarySizeMetricRangeTests(double min, double max, BinarySizeUnit binarySizeUnit, string expected, string expectedAutoscaled) =>
			TestMetricRangeWithUnitCore(min, max, binarySizeUnit, expected, expectedAutoscaled);
	}
}