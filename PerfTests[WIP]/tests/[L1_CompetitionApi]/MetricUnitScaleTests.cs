using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.PerfTests
{
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class MetricUnitScaleTests
	{
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private enum MetricEnumOk
		{
			Ones = 1,

			[MetricUnit("* 10", AppliesFrom = 8, RoundingDigits = 15)]
			Tens = 10,

			[MetricUnit("K", AppliesFrom = 900, ScaleCoefficient = 1000)]
			Thousands = 1000,

			[MetricUnit("x5K", AppliesFrom = 4900, ScaleCoefficient = 5000, RoundingDigits = 0)]
			FiveThousands = 5000,

			[MetricUnit("M", AppliesFrom = 1000 * 1000, ScaleCoefficient = 1000 * 1000, RoundingDigits = -1)]
			Millions = 5001 // BAD IDEA. DONT DO so in a real code. Used only to proof that 
		}
		private enum MetricEnumZero
		{
			[MetricUnit()]
			Zero = 0
		}

		[UsedImplicitly]
		private enum MetricEnumEmpty
		{ }

		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private enum MetricEnumNextLessThanPrevious
		{
			One = 1,
			Two = 1
		}

		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private enum MetricEnumNextLessThanPreviousAtt
		{
			One = 1,

			[MetricUnit(AppliesFrom = 0)]
			Two = 2
		}

		private enum MetricEnumNegativeApplies
		{
			[MetricUnit(AppliesFrom = -1)]
			Two = 2
		}

		private enum MetricEnumNegativeScale
		{
			[MetricUnit(ScaleCoefficient = -1)]
			Two = 2
		}
		private enum MetricEnumNegativeRounding
		{
			[MetricUnit(RoundingDigits = -2)]
			Two = 2
		}
		private enum MetricEnumRoundingTooLarge
		{
			[MetricUnit(RoundingDigits = 17)]
			Two = 2
		}

		[Test]
		public static void TestMetricUnitScaleArgValidation()
		{
			DoesNotThrow(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumOk)));
			DoesNotThrow(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumZero)));

			// DONTTOUCH: called twice to proof that error is cached.
			Throws<ArgumentException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumEmpty)));
			Throws<ArgumentException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumEmpty)));

			Throws<ArgumentException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumNextLessThanPrevious)));
			Throws<ArgumentException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumNextLessThanPreviousAtt)));
			Throws<ArgumentException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumNegativeApplies)));

			Throws<CustomAttributeFormatException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumNegativeScale)));
			Throws<CustomAttributeFormatException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumNegativeRounding)));
			Throws<CustomAttributeFormatException>(() => MetricUnitScale.FromEnumValues(typeof(MetricEnumRoundingTooLarge)));
		}

		[Test]
		public static void TestEmptyMetricUnitScale()
		{
			var scale = MetricUnitScale.Empty;
			AreSame(scale, MetricUnitScale.FromEnumValues(null));

			var unit = scale[0];
			IsTrue(unit.IsEmpty);
			AreSame(unit, MetricUnit.Empty);
			AreEqual(unit.EnumValue, null);
			AreEqual(unit.AppliesFrom, 0);
			AreEqual(unit.ScaleCoefficient, 1);
			AreEqual(unit.RoundingDigits, null);
		}

		[Test]
		public static void TestMetricUnitScaleOnes()
		{
			var scale = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			var scale2 = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			AreSame(scale, scale2);

			var unit = scale[0];
			AreSame(unit, scale[MetricEnumOk.Ones]);
			AreSame(unit, scale["onES"]); // case-insensitive
			AreSame(unit, scale[1]);
			AreSame(unit, scale[2]);
			AreSame(unit, scale[-2]);
			AreSame(unit, scale[double.NaN]);
			AreSame(unit, scale[MetricRange.Empty]);
			AreSame(unit, scale[new MetricRange(-1, 20000)]);
			AreSame(unit, scale[new MetricRange(-20000, 1)]);
			IsFalse(unit.IsEmpty);
			AreEqual(unit.EnumValue, MetricEnumOk.Ones);
			AreEqual(unit.AppliesFrom, 1);
			AreEqual(unit.ScaleCoefficient, 1);
			AreEqual(unit.RoundingDigits, null);
			AreEqual(unit.DisplayName, "Ones");
		}

		[Test]
		public static void TestMetricUnitScaleTens()
		{
			var scale = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			var unit = scale[8];
			AreSame(unit, scale[MetricEnumOk.Tens]);
			AreSame(unit, scale["* 10"]);
			AreNotSame(unit, scale[7.9999]);
			AreSame(unit, scale[10]);
			AreSame(unit, scale[505]);
			AreSame(unit, scale[-505]);
			AreNotSame(unit, scale[double.NaN]);
			AreSame(unit, scale[new MetricRange(-10, MetricRange.ToPositiveInfinity)]);
			AreSame(unit, scale[new MetricRange(MetricRange.FromNegativeInfinity, 12)]);
			IsFalse(unit.IsEmpty);
			AreEqual(unit.EnumValue, MetricEnumOk.Tens);
			AreEqual(unit.AppliesFrom, 8);
			AreEqual(unit.ScaleCoefficient, 10);
			AreEqual(unit.RoundingDigits, 15);
			AreEqual(unit.DisplayName, "* 10");
		}

		[Test]
		public static void TestMetricUnitScaleThousands()
		{
			var scale = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			var unit = scale[900];
			AreSame(unit, scale[MetricEnumOk.Thousands]);
			AreSame(unit, scale["k"]);
			AreNotSame(unit, scale[899.9999]);
			AreSame(unit, scale[1000]);
			AreSame(unit, scale[1505]);
			AreSame(unit, scale[-1505]);
			AreSame(unit, scale[new MetricRange(-10000, 900)]);
			AreSame(unit, scale[new MetricRange(MetricRange.FromNegativeInfinity, 907)]);
			IsFalse(unit.IsEmpty);
			AreEqual(unit.EnumValue, MetricEnumOk.Thousands);
			AreEqual(unit.AppliesFrom, 900);
			AreEqual(unit.ScaleCoefficient, 1000);
			AreEqual(unit.RoundingDigits, null);
			AreEqual(unit.DisplayName, "K");
		}

		[Test]
		public static void TestMetricUnitScaleFiveThousands()
		{
			var scale = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			var unit = scale[4900];
			AreSame(unit, scale[MetricEnumOk.FiveThousands]);
			AreSame(unit, scale["x5K"]);
			AreNotSame(unit, scale[4899.9999]);
			AreSame(unit, scale[5000]);
			AreSame(unit, scale[11505]);
			AreSame(unit, scale[-33000]);
			AreSame(unit, scale[new MetricRange(-20000, 10000)]);
			AreSame(unit, scale[new MetricRange(MetricRange.FromNegativeInfinity, 6543.21)]);
			IsFalse(unit.IsEmpty);
			AreEqual(unit.EnumValue, MetricEnumOk.FiveThousands);
			AreEqual(unit.AppliesFrom, 4900);
			AreEqual(unit.ScaleCoefficient, 5000);
			AreEqual(unit.RoundingDigits, 0);
			AreEqual(unit.DisplayName, "x5K");
		}

		[Test]
		public static void TestMetricUnitScaleMillions()
		{
			var m = 1000 * 1000;
			var scale = MetricUnitScale.FromEnumValues(typeof(MetricEnumOk));
			var unit = scale[m];
			AreSame(unit, scale[MetricEnumOk.Millions]);
			AreSame(unit, scale["M"]);
			AreNotSame(unit, scale[0.99 * m]);
			AreSame(unit, scale[-2 * m]);
			AreSame(unit, scale[double.NegativeInfinity]);
			AreSame(unit, scale[double.PositiveInfinity]);
			AreSame(unit, scale[MetricRange.Infinite]);
			AreSame(unit, scale[new MetricRange(-4 * m, 1 * m)]);
			IsFalse(unit.IsEmpty);
			AreEqual(unit.EnumValue, MetricEnumOk.Millions);
			AreEqual(unit.AppliesFrom, m);
			AreEqual(unit.ScaleCoefficient, m);
			AreEqual(unit.RoundingDigits, null);
			AreEqual(unit.DisplayName, "M");
		}
	}
}