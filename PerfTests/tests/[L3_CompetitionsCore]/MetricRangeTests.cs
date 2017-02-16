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
	}
}