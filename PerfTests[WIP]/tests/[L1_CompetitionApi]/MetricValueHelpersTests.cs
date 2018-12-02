using System;
using System.Globalization;
using System.Linq;

using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Metrics;

using NUnit.Framework;

using static NUnit.Framework.Assert;
using static CodeJam.PerfTests.Metrics.MetricRange;
namespace CodeJam.PerfTests
{
	public static class MetricValueHelpersTests
	{
		// starting from millimeter
		private enum Distance : long
		{
			[MetricUnit("um", AppliesFrom = 0.001, ScaleCoefficient = 0.001)]
			Micrometer = 0,
			[MetricUnit("mm")]
			Millimeter = 1,
			[MetricUnit("cm")]
			Centimeter = 10,
			[MetricUnit("in", AppliesFrom = 25.4, ScaleCoefficient = 25.4)]
			Inch = 25,
			[MetricUnit("ft", AppliesFrom = 12 * 25.4, ScaleCoefficient = 12 * 25.4)]
			Feet = 12 * Inch,
			[MetricUnit("m")]
			Meter = 1000,
			[MetricUnit("yd", AppliesFrom = 36 * 25.4, ScaleCoefficient = 36 * 25.4)]
			Yard = 3 * Feet,
			[MetricUnit("km")]
			Kilometer = 1000 * 1000,
			[MetricUnit("mi", AppliesFrom = 5280 * 12 * 25.4, ScaleCoefficient = 5280 * 12 * 25.4)]
			Mile = 5280 * Feet
		}

		[TestCase("0", "F2")]
		[TestCase("0.0002;0.0004;0.0008;0.001;0.0015", "F5")]
		[TestCase("-0.0002;-0.0004;-0.0008;-0.001;-0.0015", "F5")]
		[TestCase("0.002;0.004;0.008;0.01;0.015", "F4")]
		[TestCase("-0.002;-0.004;-0.008;-0.01;-0.015", "F4")]
		[TestCase("0.02;0.04;0.08;0.1;0.15", "F3")]
		[TestCase("-0.02;-0.04;-0.08;-0.1;-0.15", "F3")]
		[TestCase("-0.2;-0.4;-0.8", "F2")]
		[TestCase("0.2;0.4;0.8", "F2")]
		[TestCase("-0.2;-0.4;-0.8", "F2")]
		[TestCase("1;2;4;8", "F2")]
		[TestCase("-1;-2;-4;-8", "F2")]
		[TestCase("10;20;40;80", "F2")]
		[TestCase("-10;-20;-40;-80", "F2")]
		[TestCase("100;200;400;800", "F1")]
		[TestCase("-100;-200;-400;-800", "F1")]
		[TestCase("1000;2000;4000;8000", "F1")]
		[TestCase("-1000;-2000;-4000;-8000", "F1")]
		[TestCase("10000;20000;40000;80000", "F1")]
		[TestCase("-10000;-20000;-40000;-80000", "F1")]
		public static void TestAutoformat(string values, string expected)
		{
			var valuesParsed = values.Split(';').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
			foreach (var value in valuesParsed)
			{
				AreEqual(MetricValueHelpers.GetAutoscaledFormat(value), expected);
			}
		}

		[TestCase(1.0, 2.0, "m", 1000.0, 2000.0)]
		[TestCase(10.0, 20.0, "in", 254.0, 508.0)]
		[TestCase(3.0, 5.5, "um", 0.003, 0.0055)]
		[TestCase(30000.0, 30001, "um", 30.0, 30.001)]
		[TestCase(FromNegativeInfinity, 3.0, "cm", FromNegativeInfinity, 30)]
		[TestCase(FromNegativeInfinity, ToPositiveInfinity, "cm", FromNegativeInfinity, ToPositiveInfinity)]
		[TestCase(EmptyMetricValue, EmptyMetricValue, "km", EmptyMetricValue, EmptyMetricValue)]
		public static void TestCreateMetricRange(double min, double max, string unit, double expectedMin, double expectedMax)
		{
			var unitScale = MetricUnitScale.FromEnumValues(typeof(Distance));
			var metricUnit = unitScale[unit];
			var range = MetricValueHelpers.CreateMetricRange(min, max, metricUnit);
			AreEqual(range.Min, expectedMin);
			AreEqual(range.Max, expectedMax);
		}

		[Test]
		public static void TestContainsWithRounding()
		{
			var unitScale = MetricUnitScale.FromEnumValues(typeof(Distance));

			var valueX = 1002;
			var valueY = 1005;
			var valueZ = 1008;

			var rangeX = new MetricRange(valueX, valueX);
			var rangeY = new MetricRange(valueY, valueY);
			var rangeZ = new MetricRange(valueZ, valueZ);
			var rangeYz = new MetricRange(valueY, valueZ);

			var unit = unitScale[Distance.Meter];

			AssertRoundtripInvariant(rangeY, rangeX, unit);
			AssertRoundtripInvariant(rangeY, rangeZ, unit);
			AssertRoundtripInvariant(rangeY, rangeYz, unit);

			IsFalse(rangeY.ContainsWithRounding(rangeX, unit));
			IsTrue(rangeY.ContainsWithRounding(rangeZ, unit));
			IsTrue(rangeY.ContainsWithRounding(rangeYz, unit));
		}

		[Test]
		public static void TestContainsWithRoundingCase2()
		{
			var unitScale = MetricUnitScale.FromEnumValues(typeof(Distance));

			var valueX = 99986;
			var valueY = 99996;
			var valueZ = 100004;

			var rangeX = new MetricRange(valueX, valueX);
			var rangeY = new MetricRange(valueY, valueY);
			var rangeZ = new MetricRange(valueZ, valueZ);
			var rangeYz = new MetricRange(valueY, valueZ);

			var unit = unitScale[Distance.Meter];

			AssertRoundtripInvariant(rangeY, rangeX, unit);
			AssertRoundtripInvariant(rangeY, rangeZ, unit);
			AssertRoundtripInvariant(rangeY, rangeYz, unit);

			IsFalse(rangeY.ContainsWithRounding(rangeX, unit));
			IsTrue(rangeY.ContainsWithRounding(rangeZ, unit));
			IsTrue(rangeY.ContainsWithRounding(rangeYz, unit));
		}

		// DONTTOUCH: this test is critical for entire metric storage subsystem.
		// if it is broken there's a chance that metric comparison will fail due to rounding errors.
		[Test]
		public static void TestCriticalProofContainsWithRoundingBruteForce()
		{
			var unitScale = MetricUnitScale.FromEnumValues(typeof(Distance));

			var lastRange = MetricValueHelpers.CreateMetricRange(0, 0);
			int repeats = 0;
			int increment = 3;
			for (long i = 0; i < int.MaxValue; i += increment)
			{
				// exponential growth as the scale is exponential
				if (++repeats >= 1000 && increment < int.MaxValue / 10)
				{
					repeats = 0;
					increment *= 2;
				}

				lastRange = AssertAssertRoundtripInvariant(i, lastRange, unitScale);
			}

			lastRange = MetricValueHelpers.CreateMetricRange(int.MaxValue, int.MaxValue);
			repeats = 0;
			increment = int.MaxValue / 200;
			for (long i = int.MaxValue; i > 0; i -= increment) // reverse order
			{
				// logarithmic decrease as the scale is enumerated in reverse order
				if (++repeats >= 1000 && increment > 10)
				{
					repeats = 0;
					increment /= 2;
				}

				lastRange = AssertAssertRoundtripInvariant(i, lastRange, unitScale);
			}
		}

		private static MetricRange AssertAssertRoundtripInvariant(
			long i,
			MetricRange lastRange,
			MetricUnitScale unitScale)
		{
			// best applicable unit
			var unit = unitScale[i];

			// new ranges
			var newRange = MetricValueHelpers.CreateMetricRange(i, i);
			var minMaxRange = lastRange.Union(newRange);

			AssertRoundtripInvariant(newRange, lastRange, unit);
			AssertRoundtripInvariant(newRange, minMaxRange, unit);
			AssertRoundtripInvariant(lastRange, minMaxRange, unit);

			return newRange.ContainsWithRounding(lastRange, unit) ? lastRange : newRange;
		}

		private static void AssertRoundtripInvariant(MetricRange a, MetricRange b, MetricUnit metricUnit)
		{
			var c = HostEnvironmentInfo.MainCultureInfo;

			a.GetMinMaxString(metricUnit, out var aMin, out var aMax);
			b.GetMinMaxString(metricUnit, out var bMin, out var bMax);

			var a2 = MetricValueHelpers.CreateMetricRange(
				double.Parse(aMin, c),
				double.Parse(aMax, c),
				metricUnit);

			var b2 = MetricValueHelpers.CreateMetricRange(
				double.Parse(bMin, c),
				double.Parse(bMax, c),
				metricUnit);

			IsTrue(a.ContainsWithRounding(a2, metricUnit));
			IsTrue(a2.ContainsWithRounding(a, metricUnit));
			IsTrue(b.ContainsWithRounding(b2, metricUnit));
			IsTrue(b2.ContainsWithRounding(b, metricUnit));

			if (a.ContainsWithRounding(b, metricUnit))
			{
				IsTrue(a2.ContainsWithRounding(b2, metricUnit));
			}
		}
	}
}