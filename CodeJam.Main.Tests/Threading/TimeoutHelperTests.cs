using NUnit.Framework;

using System;

namespace CodeJam.Threading
{
	[TestFixture]
	public class TimeoutHelperTests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestAdjustTimeout(bool infiniteIfDefault)
		{
			var d1 = TimeSpan.FromDays(1);
			var dMinus1 = TimeSpan.FromDays(-1);
			var infiniteValue = TimeoutHelper.InfiniteTimeSpan;
			var adjustedZero = infiniteIfDefault ? infiniteValue : TimeSpan.Zero;

			Assert.AreEqual(
				d1.AdjustTimeout(infiniteIfDefault),
				d1);
			Assert.AreEqual(
				dMinus1.AdjustTimeout(infiniteIfDefault),
				infiniteValue);
			Assert.AreEqual(
				TimeSpan.Zero.AdjustTimeout(infiniteIfDefault),
				adjustedZero);
			Assert.AreEqual(
				TimeoutHelper.InfiniteTimeSpan.AdjustTimeout(infiniteIfDefault),
				infiniteValue);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestAdjustTimeoutDefaultValue(bool infiniteIfDefault)
		{
			var d1 = TimeSpan.FromDays(1);
			var dMinus1 = TimeSpan.FromDays(-1);
			var infiniteValue = TimeSpan.FromSeconds(60);
			var adjustedZero = infiniteIfDefault ? infiniteValue : TimeSpan.Zero;

			Assert.AreEqual(
				d1.AdjustTimeout(infiniteValue, infiniteIfDefault),
				d1);
			Assert.AreEqual(
				dMinus1.AdjustTimeout(infiniteValue, infiniteIfDefault),
				infiniteValue);
			Assert.AreEqual(
				TimeSpan.Zero.AdjustTimeout(infiniteValue, infiniteIfDefault),
				adjustedZero);
			Assert.AreEqual(
				TimeoutHelper.InfiniteTimeSpan.AdjustTimeout(infiniteValue, infiniteIfDefault),
				infiniteValue);
		}



		[TestCase(true)]
		[TestCase(false)]
		public void TestAdjustAnLimitTimeout(bool infiniteIfDefault)
		{
			var d1 = TimeSpan.FromDays(1);
			var d2 = TimeSpan.FromDays(2);
			var dMinus1 = TimeSpan.FromDays(-1);
			var maxValue = TimeSpan.FromHours(36);
			var infiniteValue = TimeoutHelper.InfiniteTimeSpan;
			var adjustedZero = infiniteIfDefault ? maxValue : TimeSpan.Zero;

			Assert.AreEqual(
				d1.AdjustAndLimitTimeout(maxValue, infiniteIfDefault),
				d1);
			Assert.AreEqual(
				d2.AdjustAndLimitTimeout(maxValue, infiniteIfDefault),
				maxValue);
			Assert.AreEqual(
				dMinus1.AdjustAndLimitTimeout(maxValue, infiniteIfDefault),
				maxValue);
			Assert.AreEqual(
				TimeSpan.Zero.AdjustAndLimitTimeout(maxValue, infiniteIfDefault),
				adjustedZero);
			Assert.AreEqual(
				TimeoutHelper.InfiniteTimeSpan.AdjustAndLimitTimeout(maxValue, infiniteIfDefault),
				maxValue);
		}

		[TestCase(0, 1)]
		[TestCase(1, 1)]
		[TestCase(4, 8)]
		[TestCase(6, 32)]
		[TestCase(8, 60)]
		[TestCase(10, 60)]
		[TestCase(100, 60)]
		public void TestBackoff(int retry, int resultSeconds)
		{
			var delay = TimeSpan.FromSeconds(1);
			var max = TimeSpan.FromMinutes(1);
			var timeoutSeconds = TimeoutHelper.ExponentialBackoffTimeout(retry, delay, max).TotalSeconds;

			Assert.That(timeoutSeconds, Is.InRange(resultSeconds * 0.8, resultSeconds * 1.2));
		}
	}
}