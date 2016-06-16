using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class InProcessToolchainTests
	{
		#region TestInProcessBenchmark
		private static int _callCounter;
		private static int _afterSetupCounter;

		[Test]
		public static void TestInProcessBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);
			var summary = new PerfTestRunner()
				.Run<InProcessBenchmark>(PerfTestHelpers.SingleRunConfig)
				.LastRunSummary;
			Assert.AreEqual(_callCounter, PerfTestHelpers.ExpectedSingleRunCount);
			Assert.AreEqual(_afterSetupCounter, 1);

			Assert.IsFalse(summary.ValidationErrors.Any());
		}

		public class InProcessBenchmark
		{
			[Setup]
			public void Setup() => Interlocked.Exchange(ref _afterSetupCounter, 0);

			[Benchmark]
			public void InvokeOnce()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				Thread.Sleep(10);
			}
		}
		#endregion

		#region TestInProcessWithValidationBenchmark
		[Test]
		public static void TestInProcessWithValidationBenchmark()
		{
			// DONTTOUCH: config SHOULD NOT match the platform.
			var config = PerfTestHelpers.OtherPlatformSingleRunConfig;

			var summary = new PerfTestRunner()
				.Run<InProcessWithValidationBenchmark>(config)
				.LastRunSummary;
			Assert.AreEqual(summary.ValidationErrors.Length, 1);
			Assert.IsFalse(summary.ValidationErrors[0].IsCritical);
			Assert.That(summary.ValidationErrors[0].Message, Does.Contain(", property Platform:"));
		}

		public class InProcessWithValidationBenchmark
		{
			[Benchmark]
			public void InvokeOnce() => Thread.Sleep(10);
		}
		#endregion
	}
}