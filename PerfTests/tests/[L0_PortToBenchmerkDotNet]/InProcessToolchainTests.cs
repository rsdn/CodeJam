using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class InProcessToolchainTests
	{
		private const int MethodsCount = 12; // 12 benchmark methods.

		private static int _callCounter;
		private static int _afterGlobalSetupCounter;
		private static int _afterIterationSetupCounter;
		private static int _globalSetupCounter;
		private static int _globalSetupTargetCounter;
		private static int _iterationSetupCounter;
		private static int _globalCleanupCounter;
		private static int _iterationCleanupCounter;
		private static int _iterationCleanupTargetCounter;

		private static void ResetCounters()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterGlobalSetupCounter, 0);
			Interlocked.Exchange(ref _afterIterationSetupCounter, 0);
			Interlocked.Exchange(ref _globalSetupCounter, 0);
			Interlocked.Exchange(ref _globalSetupTargetCounter, 0);
			Interlocked.Exchange(ref _iterationSetupCounter, 0);
			Interlocked.Exchange(ref _globalCleanupCounter, 0);
			Interlocked.Exchange(ref _iterationCleanupCounter, 0);
			Interlocked.Exchange(ref _iterationCleanupTargetCounter, 0);
		}

		[Test]
		public static void TestInProcessBenchmarkStandardEngine()
		{
			ResetCounters();

			var invocationCount = 1;

			var config = CompetitionHelpers.ConfigForAssembly
				.WithModifier(
					new Job
					{
						Infrastructure =
						{
							EngineFactory = new EngineFactory()
						}
					})
				.WithModifier(
					new CompetitionOptions()
					{
						Checks = { TooFastBenchmarkLimit = TimeSpan.Zero }
					});
			var summary = SelfTestCompetition
				.Run<InProcessBenchmarkAllCases>(config)
				.LastRunSummary;

			// TODO: uncomment after https://github.com/dotnet/BenchmarkDotNet/issues/464
			//var iterationsNoUnroll = GetExpectedCountIgnoreUnroll(config, 1);

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterGlobalSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_afterIterationSetupCounter, invocationCount);
			Assert.AreEqual(_globalSetupCounter, MethodsCount - 1);
			Assert.AreEqual(_globalSetupTargetCounter, 1);
			//Assert.AreEqual(_iterationSetupCounter, MethodsCount * iterationsNoUnroll);
			Assert.AreEqual(_globalCleanupCounter, MethodsCount);
			//Assert.AreEqual(_iterationCleanupCounter, (MethodsCount - 1) * iterationsNoUnroll);
			//Assert.AreEqual(_iterationCleanupTargetCounter, 1 * iterationsNoUnroll);

			var a = summary?.ValidationErrors.ToArray() ?? Array<ValidationError>.Empty;
			foreach (var validationError in a)
			{
				Console.WriteLine(validationError.Message);
			}
			Assert.AreEqual(a.Length, 0);
		}

		[Test]
		public static void TestInProcessBenchmarkStandardEngineUnroll()
		{
			ResetCounters();

			var invocationCount = 20;

			var config = CompetitionHelpers.ConfigForAssembly
				.WithModifier(
					new Job
					{
						Infrastructure =
						{
							EngineFactory = new EngineFactory()
						},
						Run =
						{
							InvocationCount = invocationCount,
							UnrollFactor = 5
						}
					})
				.WithModifier(
					new CompetitionOptions()
					{
						Checks = { TooFastBenchmarkLimit = TimeSpan.Zero }
					});
			var summary = SelfTestCompetition
				.Run<InProcessBenchmarkAllCases>(config)
				.LastRunSummary;

			// TODO: uncomment after https://github.com/dotnet/BenchmarkDotNet/issues/464
			//var iterationsNoUnroll = GetExpectedCountIgnoreUnroll(config, 1);

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterGlobalSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_afterIterationSetupCounter, invocationCount);
			Assert.AreEqual(_globalSetupCounter, MethodsCount - 1);
			Assert.AreEqual(_globalSetupTargetCounter, 1);
			//Assert.AreEqual(_iterationSetupCounter, MethodsCount * iterationsNoUnroll);
			Assert.AreEqual(_globalCleanupCounter, MethodsCount);
			//Assert.AreEqual(_iterationCleanupCounter, (MethodsCount - 1) * iterationsNoUnroll);
			//Assert.AreEqual(_iterationCleanupTargetCounter, 1 * iterationsNoUnroll);

			Assert.IsFalse(Enumerable.Any(summary?.ValidationErrors));
		}

		[Test]
		public static void TestInProcessBenchmarkWithValidation()
		{
			ResetCounters();

			// DONTTOUCH: config SHOULD NOT match the default platform (x64).
			var config = new ManualCompetitionConfig(
				CompetitionHelpers.CreateConfig(
					typeof(InProcessBenchmarkAllCases),
					new CompetitionFeatures
					{
						Platform = Platform.X86
					}));
			config.Add(InProcessValidator.FailOnError);

			var summary = SelfTestCompetition
				.Run<InProcessBenchmarkAllCases>(config)
				.LastRunSummary;

			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(_afterGlobalSetupCounter, 0);
			Assert.AreEqual(_afterIterationSetupCounter, 0);
			Assert.AreEqual(_globalSetupCounter, 0);
			Assert.AreEqual(_globalSetupTargetCounter, 0);
			Assert.AreEqual(_iterationSetupCounter, 0);
			Assert.AreEqual(_globalCleanupCounter, 0);
			Assert.AreEqual(_iterationCleanupCounter, 0);
			Assert.AreEqual(_iterationCleanupTargetCounter, 0);

			Assert.AreEqual(summary?.ValidationErrors.Length, 1);
			Assert.IsTrue(summary?.ValidationErrors[0].IsCritical);
			Assert.AreEqual(
				summary?.ValidationErrors[0].Message,
				"Job SelfTestConfigX86, EnvMode.Platform was run as X64 (X86 expected). Fix your test runner options.");
		}

		public class InProcessBenchmarkAllCases
		{
			// TODO: custom type results.
			[GlobalSetup(Target = nameof(InvokeOnceVoid))]
			public static void GlobalSetupInvokeOnceVoid()
			{
				//await Task.Yield();
				Interlocked.Increment(ref _globalSetupTargetCounter);
				Interlocked.Exchange(ref _afterGlobalSetupCounter, 0);
			}

			[GlobalSetup]
			public static void GlobalSetup()
			{
				//await Task.Yield();
				Interlocked.Increment(ref _globalSetupCounter);
				Interlocked.Exchange(ref _afterGlobalSetupCounter, 0);
			}

			[GlobalCleanup]
			public void /*decimal*/ GlobalCleanup() => Interlocked.Increment(ref _globalCleanupCounter);

			[IterationSetup]
			public static void IterationSetup()
			{
				//await Task.Yield();
				Interlocked.Increment(ref _iterationSetupCounter);
				Interlocked.Exchange(ref _afterIterationSetupCounter, 0);
			}

			[IterationCleanup(Target = nameof(InvokeOnceVoid))]
			public void /*decimal*/ IterationCleanupInvokeOnceVoid() => Interlocked.Increment(ref _iterationCleanupTargetCounter);

			[IterationCleanup]
			public void /*decimal*/ IterationCleanup() => Interlocked.Increment(ref _iterationCleanupCounter);

			#region Instance members
			[Benchmark]
			public void InvokeOnceVoid()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
			}

			[Benchmark]
			public async Task InvokeOnceTaskAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
			}

			[Benchmark]
			public string InvokeOnceRefType()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return "";
			}

			[Benchmark]
			public decimal InvokeOnceValueType()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return 0;
			}

			[Benchmark]
			public async Task<string> InvokeOnceTaskOfTAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return "";
			}

			[Benchmark]
			public ValueTask<decimal> InvokeOnceValueTaskOfT()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return new ValueTask<decimal>(11);
			}
			#endregion

			#region Static members
			[Benchmark]
			public static void InvokeOnceVoidStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
			}

			[Benchmark]
			public static async Task InvokeOnceTaskStaticAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
			}

			[Benchmark]
			public static string InvokeOnceRefTypeStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return "";
			}

			[Benchmark]
			public static decimal InvokeOnceValueTypeStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return 0;
			}

			[Benchmark]
			public static async Task<string> InvokeOnceTaskOfTStaticAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return "";
			}

			[Benchmark]
			public static ValueTask<decimal> InvokeOnceValueTaskOfTStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterGlobalSetupCounter);
				Interlocked.Increment(ref _afterIterationSetupCounter);
				return new ValueTask<decimal>(11);
			}
			#endregion
		}
	}
}