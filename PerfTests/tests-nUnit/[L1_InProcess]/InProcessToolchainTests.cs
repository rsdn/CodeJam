using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class InProcessToolchainTests
	{
		private static int _callCounter;
		private static int _afterSetupCounter;

		[Test]
		public static void TestInProcessBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);

			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(SelfTestConfig)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupCounter, 2);

			Assert.IsFalse(summary.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkWithValidation()
		{
			// DONTTOUCH: config SHOULD NOT match the default platform (x64).
			var config = CreateSelfTestConfig(Platform.X86);
			config.Add(InProcessValidator.FailOnError);

			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);

			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(_afterSetupCounter, 0);

			Assert.AreEqual(summary.ValidationErrors.Length, 1);
			Assert.IsTrue(summary.ValidationErrors[0].IsCritical);
			Assert.That(summary.ValidationErrors[0].Message, Does.Contain(", property Platform:"));
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
	}
}