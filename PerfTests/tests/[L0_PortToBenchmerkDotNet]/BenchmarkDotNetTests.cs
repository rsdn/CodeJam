using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Horology;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	public static class BenchmarkHelpersTests
	{
		[Test]
		public static void TestBestClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockB = Chronometer.BestClock;

			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s2 = c2.Stop();
			var t2 = s2.GetSeconds();

			var c3 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c2.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t2 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t2 - t3) / t2, Is.LessThanOrEqualTo(0.99));
		}
		[Test]
		public static void TestStopwatchClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockB = Chronometer.Stopwatch;

			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s2 = c2.Stop();
			var t2 = s2.GetSeconds();

			var c3 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c2.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t2 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t2 - t3) / t2, Is.LessThanOrEqualTo(0.99));
		}

		[Test]
		public static void TestProcessCycleTimeClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockA = ProcessCycleTimeClock.Instance;
			var clockB = Chronometer.BestClock;

			var c1 = clockA.Start();
			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s1 = c1.Stop();
			var s2 = c2.Stop();
			var t1 = s1.GetSeconds();
			var t2 = s2.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c2.Clock.GetType().Name}: {t1 / t2:P}");

			var c3 = clockA.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t1 / t3:P}");

			// If all CPU cores are busy all threads of the process can be suspended.
			// We're not interested in accurate absolute values +/- 50% is acceptable.
			Assume.That(Math.Abs(t1 - t2) / t2, Is.LessThanOrEqualTo(0.5));
			Assume.That(Math.Abs(t1 - t3) / t1, Is.LessThanOrEqualTo(0.06)); // At the same time relative time should be precise enough.
		}

		[Test]
		public static void TestThreadCycleTimeClock()
		{
			Thread.SpinWait(30 * 1000 * 1000);

			var clockA = ThreadCycleTimeClock.Instance;
			var clockB = Chronometer.BestClock;

			var c1 = clockA.Start();
			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s1 = c1.Stop();
			var s2 = c2.Stop();
			var t1 = s1.GetSeconds();
			var t2 = s2.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c2.Clock.GetType().Name}: {t1 / t2:P}");

			var c3 = clockA.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t1 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t1 - t2) / t2, Is.LessThanOrEqualTo(0.99));
			Assume.That(Math.Abs(t1 - t3) / t1, Is.LessThanOrEqualTo(0.06)); // At the same time relative time should be precise enough.
		}

		[Test]
		public static void TestGetTotalNanoseconds()
		{
			var oneSecondNs = (int)TimeSpan.FromSeconds(1).TotalNanoseconds();
			AreEqual(oneSecondNs, 1000 * 1000 * 1000);

			var twelveMsNs = (int)TimeSpan.FromMilliseconds(12).TotalNanoseconds();
			AreEqual(twelveMsNs, 12 * 1000 * 1000);

			var oneSecondUs = (int)TimeSpan.FromSeconds(1).TotalMicroseconds();
			AreEqual(oneSecondUs, 1000 * 1000);

			var twelveMsUs = (int)TimeSpan.FromMilliseconds(12).TotalMicroseconds();
			AreEqual(twelveMsUs, 12 * 1000);
		}

		[Test]
		public static void TestFromTotalNanoseconds()
		{
			var twelveMs = TimeSpan.FromMilliseconds(12);
			AreEqual(twelveMs, BenchmarkHelpers.TimeSpanFromNanoseconds(12 * 1000 * 1000));
			AreEqual(twelveMs, BenchmarkHelpers.TimeSpanFromMicroseconds(12 * 1000));
		}
	}
}