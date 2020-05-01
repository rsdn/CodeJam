using System;

using NUnit.Framework;

namespace CodeJam.Threading
{
	[TestFixture]
	public class TimeoutHelperTests
	{
		[Test]
		public void TestAdjustTimeout()
		{
			var d1 = TimeSpan.FromDays(1);
			var dMinus1 = TimeSpan.FromDays(-1);

			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(), TimeSpan.Zero);
			Assert.AreEqual(d1.AdjustTimeout(), d1);
			Assert.AreEqual(dMinus1.AdjustTimeout(), TimeoutHelper.InfiniteTimeSpan);
		}

		[Test]
		public void TestAdjustTimeoutInfinite()
		{
			const bool infiniteIfDefault = true;
			var d1 = TimeSpan.FromDays(1);
			var dMinus1 = TimeSpan.FromDays(-1);

			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);
			Assert.AreEqual(d1.AdjustTimeout(infiniteIfDefault), d1);
			Assert.AreEqual(dMinus1.AdjustTimeout(infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);
		}

		[Test]
		public void TestAdjustTimeoutLimit()
		{
			var d1 = TimeSpan.FromDays(1);
			var d2 = TimeSpan.FromDays(2);
			var dMinus1 = TimeSpan.FromDays(-1);

			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(TimeSpan.Zero), TimeSpan.Zero);
			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(d1), TimeSpan.Zero);
			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(dMinus1), TimeSpan.Zero);

			Assert.AreEqual(d1.AdjustTimeout(TimeSpan.Zero), TimeSpan.Zero);
			Assert.AreEqual(d1.AdjustTimeout(d1), d1);
			Assert.AreEqual(d1.AdjustTimeout(d2), d1);
			Assert.AreEqual(d2.AdjustTimeout(d1), d1);
			Assert.AreEqual(d1.AdjustTimeout(dMinus1), d1);

			Assert.AreEqual(dMinus1.AdjustTimeout(TimeSpan.Zero), TimeSpan.Zero);
			Assert.AreEqual(dMinus1.AdjustTimeout(d1), d1);
			Assert.AreEqual(dMinus1.AdjustTimeout(dMinus1), TimeoutHelper.InfiniteTimeSpan);
		}

		[Test]
		public void TestAdjustTimeoutLimitInfiniteIfDefault()
		{
			const bool infiniteIfDefault = true;
			var d1 = TimeSpan.FromDays(1);
			var d2 = TimeSpan.FromDays(2);
			var dMinus1 = TimeSpan.FromDays(-1);

			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(TimeSpan.Zero, infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);
			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(d1, infiniteIfDefault), d1);
			Assert.AreEqual(TimeSpan.Zero.AdjustTimeout(dMinus1, infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);

			Assert.AreEqual(d1.AdjustTimeout(TimeSpan.Zero, infiniteIfDefault), d1);
			Assert.AreEqual(d1.AdjustTimeout(d1, infiniteIfDefault), d1);
			Assert.AreEqual(d1.AdjustTimeout(d2, infiniteIfDefault), d1);
			Assert.AreEqual(d2.AdjustTimeout(d1, infiniteIfDefault), d1);
			Assert.AreEqual(d1.AdjustTimeout(dMinus1, infiniteIfDefault), d1);

			Assert.AreEqual(dMinus1.AdjustTimeout(TimeSpan.Zero, infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);
			Assert.AreEqual(dMinus1.AdjustTimeout(d1, infiniteIfDefault), d1);
			Assert.AreEqual(dMinus1.AdjustTimeout(dMinus1, infiniteIfDefault), TimeoutHelper.InfiniteTimeSpan);
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