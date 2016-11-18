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
		private static int _callReturnCounter;
		private static int _afterSetupReturnCounter;
		private static int _setupCounter;
		private static int _cleanupCounter;

		private static void ResetCounters()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);
			Interlocked.Exchange(ref _callReturnCounter, 0);
			Interlocked.Exchange(ref _afterSetupReturnCounter, 0);
			Interlocked.Exchange(ref _setupCounter, 0);
			Interlocked.Exchange(ref _cleanupCounter, 0);
		}

		[Test]
		public static void TestInProcessBenchmarkStandardEngine()
		{
			ResetCounters();

			var config = CompetitionHelpers.ConfigForAssembly.WithJobModifier(
				new Job
				{
					Infrastructure =
					{
						EngineFactory = new EngineFactory()
					}
				},
				true);
			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupCounter, 0); // no calls as InvokeOnceReturn is performed
			Assert.AreEqual(_callReturnCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupReturnCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_setupCounter, 2);
			Assert.AreEqual(_cleanupCounter, 2);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkBurstMode()
		{
			ResetCounters();

			var config = CompetitionHelpers.ConfigForAssembly.WithJobModifier(
				new Job
				{
					Infrastructure =
					{
						EngineFactory = BurstModeEngineFactory.Instance
					}
				},
				true);
			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupCounter, 0); // no calls as InvokeOnceReturn is performed
			Assert.AreEqual(_callReturnCounter, ExpectedSelfTestRunCount);
			Assert.AreEqual(_afterSetupReturnCounter, 2);
			Assert.AreEqual(_setupCounter, 6);
			Assert.AreEqual(_cleanupCounter, 6);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkBurstModeUnroll()
		{
			ResetCounters();

			var config = CompetitionHelpers.ConfigForAssembly.WithJobModifier(
				new Job
				{
					Infrastructure =
					{
						EngineFactory = BurstModeEngineFactory.Instance
					},
					Run =
					{
						InvocationCount = 10,
						UnrollFactor = 5
					}
				},
				true);
			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, 205);
			Assert.AreEqual(_afterSetupCounter, 0); // no calls as InvokeOnceReturn is performed
			Assert.AreEqual(_callReturnCounter, 205);
			Assert.AreEqual(_afterSetupReturnCounter, 100);
			Assert.AreEqual(_setupCounter, 6);
			Assert.AreEqual(_cleanupCounter, 6);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkWithValidation()
		{
			ResetCounters();

			// DONTTOUCH: config SHOULD NOT match the default platform (x64).
			var config = new ManualCompetitionConfig(
				CompetitionHelpers.CreateConfig(
					typeof(InProcessBenchmark),
					new CompetitionFeatures
					{
						TargetPlatform = Platform.X86
					}));
			config.Add(InProcessValidator.FailOnError);

			var summary = SelfTestCompetition
				.Run<InProcessBenchmark>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(_afterSetupCounter, 0);
			Assert.AreEqual(_callReturnCounter, 0);
			Assert.AreEqual(_afterSetupReturnCounter, 0);
			Assert.AreEqual(_setupCounter, 0);
			Assert.AreEqual(_cleanupCounter, 0);

			Assert.AreEqual(summary?.ValidationErrors.Length, 1);
			Assert.IsTrue(summary?.ValidationErrors[0].IsCritical);
			Assert.AreEqual(
				summary?.ValidationErrors[0].Message,
				"Job SelfTestConfigX86, property EnvMode.Platform: value (X86) does not match environment (X64).");
		}

		public class InProcessBenchmark
		{
			[Setup]
			public static void Setup()
			{
				Interlocked.Increment(ref _setupCounter);
				Interlocked.Exchange(ref _afterSetupCounter, 0);
				Interlocked.Exchange(ref _afterSetupReturnCounter, 0);
			}

			[Benchmark]
			public void InvokeOnce()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				Thread.Sleep(10);
			}

			[Benchmark]
			public static int InvokeOnceReturn()
			{
				Interlocked.Increment(ref _callReturnCounter);
				Interlocked.Increment(ref _afterSetupReturnCounter);
				Thread.Sleep(10);
				return 0;
			}

			[Cleanup]
			public static void Cleanup() => Interlocked.Increment(ref _cleanupCounter);
		}
	}
}