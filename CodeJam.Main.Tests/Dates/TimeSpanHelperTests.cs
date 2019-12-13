using System;

using NUnit.Framework;

namespace CodeJam.Dates
{
	public static class TimeSpanHelperTests
	{
		[TestCase(1, 2)]
		[TestCase(1, 0)]
		[TestCase(1, -1)]
		[TestCase(100, 0.5)]
		[TestCase(100, -0.25)]
		[TestCase(1, 0.125)]
		[TestCase(123, 456.25)]
		public static void TestMultiplySuccess(int time, double scale)
		{
			// Too small for timespan resolution
			Assert.AreEqual(
				TimeSpan.FromTicks(time * 10000).Multiply(scale),
				TimeSpan.FromTicks((long)(time * 10000 * scale)));

			// Too small for timespan resolution
			Assert.AreEqual(
				TimeSpanHelpers.FromNanoseconds(time * 10000).Multiply(scale),
				TimeSpanHelpers.FromNanoseconds(time * 10000 * scale));

			// Too small for timespan resolution
			Assert.AreEqual(
				TimeSpanHelpers.FromMicroseconds(time * 10000).Multiply(scale),
				TimeSpanHelpers.FromMicroseconds(time * 10000 * scale));

			// HACK: see https://stackoverflow.com/a/5450495 for explanation
			Assert.AreEqual(
				Math.Round(TimeSpan.FromMilliseconds(time).Multiply(scale).TotalMilliseconds),
				Math.Round(TimeSpan.FromMilliseconds(time * scale).TotalMilliseconds));

			Assert.AreEqual(
				TimeSpan.FromSeconds(time).Multiply(scale),
				TimeSpan.FromSeconds(time * scale));

			Assert.AreEqual(
				TimeSpan.FromMinutes(time).Multiply(scale),
				TimeSpan.FromMinutes(time * scale));

			Assert.AreEqual(
				TimeSpan.FromHours(time).Multiply(scale),
				TimeSpan.FromHours(time * scale));

			Assert.AreEqual(
				TimeSpan.FromDays(time).Multiply(scale),
				TimeSpan.FromDays(time * scale));
		}

		[TestCase(16, 2)]
		[TestCase(145, 5)]
		[TestCase(14, -1)]
		[TestCase(100, 0.5)]
		[TestCase(100, -0.25)]
		[TestCase(1, 0.125)]
		[TestCase(11295, 125.5)]
		public static void TestDivideSuccess(int time, double divisor)
		{
			Assert.AreEqual(
				TimeSpan.FromTicks(time).Divide(divisor),
				TimeSpan.FromTicks((long)(time / divisor)));

			// Too small for timespan resolution
			Assert.AreEqual(
				TimeSpanHelpers.FromNanoseconds(time * 10000).Divide(divisor),
				TimeSpanHelpers.FromNanoseconds(time * 10000 / divisor));

			// Too small for timespan resolution
			Assert.AreEqual(
				TimeSpanHelpers.FromMicroseconds(time * 10000).Divide(divisor),
				TimeSpanHelpers.FromMicroseconds(time * 10000 / divisor));

			// HACK: see https://stackoverflow.com/a/5450495 for explanation
			Assert.AreEqual(
				Math.Round(TimeSpan.FromMilliseconds(time).Divide(divisor).TotalMilliseconds),
				Math.Round(TimeSpan.FromMilliseconds(time / divisor).TotalMilliseconds));

			Assert.AreEqual(
				TimeSpan.FromSeconds(time).Divide(divisor),
				TimeSpan.FromSeconds(time / divisor));

			Assert.AreEqual(
				TimeSpan.FromMinutes(time).Divide(divisor),
				TimeSpan.FromMinutes(time / divisor));

			Assert.AreEqual(
				TimeSpan.FromHours(time).Divide(divisor),
				TimeSpan.FromHours(time / divisor));

			Assert.AreEqual(
				TimeSpan.FromDays(time).Divide(divisor),
				TimeSpan.FromDays(time / divisor));
		}

		[Test]
		public static void TestMultiplyFail()
		{
			Assert.Throws<ArgumentException>(() => TimeSpan.FromDays(1).Multiply(double.NaN));
			Assert.Throws<OverflowException>(() => TimeSpan.FromDays(1).Multiply(1e8));
			Assert.Throws<OverflowException>(() => TimeSpan.FromDays(1).Multiply(double.NegativeInfinity));
			Assert.Throws<OverflowException>(() => TimeSpan.FromDays(1).Multiply(double.PositiveInfinity));
		}

		[Test]
		public static void TestDivideFail()
		{
			Assert.Throws<ArgumentException>(() => TimeSpan.FromDays(1).Divide(double.NaN));
			Assert.Throws<OverflowException>(() => TimeSpan.FromDays(1).Divide(1e-8));
		}

		[TestCase(0.5)]
		[TestCase(1)]
		[TestCase(-100)]
		[TestCase(123)]
		[TestCase(0)]
		public static void TestFromMicroseconds(double timeMs)
		{
			var accurateMs = TimeSpan.FromTicks((long)(timeMs * TimeSpan.TicksPerMillisecond));
			Assert.AreEqual(
				TimeSpanHelpers.FromMicroseconds(timeMs * 1000),
				accurateMs);

			Assert.AreEqual(
				accurateMs.TotalMicroseconds(),
				timeMs * 1000);
		}

		[TestCase(0.5)]
		[TestCase(1)]
		[TestCase(-100)]
		[TestCase(123)]
		[TestCase(0)]
		public static void TestFromNanoseconds(double timeMs)
		{
			var accurateMs = TimeSpan.FromTicks((long)(timeMs * TimeSpan.TicksPerMillisecond));
			Assert.AreEqual(
				TimeSpanHelpers.FromNanoseconds(timeMs * 1000 * 1000),
				accurateMs);

			Assert.AreEqual(
				accurateMs.TotalNanoseconds(),
				timeMs * 1000 * 1000);
		}
	}
}