using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

using CodeJam.PerfTests.Configs;

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
		public static void TestInProcessBenchmarkStandardEngine()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);

			var config = SelfTestConfig.Default.WithJobModifier(
				new Job(new InfrastructureMode { EngineFactory = new EngineFactory() }),
				true);
			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(SelfTestConfig.Default)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupCounter, ExpectedSelfTestRunCount);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkBurstMode()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);

			var config = SelfTestConfig.Default.WithJobModifier(
				new Job(new InfrastructureMode { EngineFactory = BurstModeEngineFactory.Instance }),
				true);
			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupCounter, 2);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkWithValidation()
		{
			// DONTTOUCH: config SHOULD NOT match the default platform (x64).
			var config = new ManualCompetitionConfig(new SelfTestConfig(Platform.X86));
			config.Add(InProcessValidator.FailOnError);

			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);

			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(_afterSetupCounter, 0);

			Assert.AreEqual(summary?.ValidationErrors.Length, 1);
			Assert.IsTrue(summary?.ValidationErrors[0].IsCritical);
			Assert.AreEqual(
				summary?.ValidationErrors[0].Message,
				"Job SelfTestConfigX86, property EnvMode.Platform: value (X86) does not match environment (X64).");
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