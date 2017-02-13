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
		private static int _afterSetupCounter;
		private static int _setupCounter;
		private static int _cleanupCounter;

		private static void ResetCounters()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);
			Interlocked.Exchange(ref _setupCounter, 0);
			Interlocked.Exchange(ref _cleanupCounter, 0);
		}

		[Test]
		public static void TestInProcessBenchmarkStandardEngine()
		{
			ResetCounters();

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

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_setupCounter, MethodsCount);
			Assert.AreEqual(_cleanupCounter, MethodsCount);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkStandardEngineUnroll()
		{
			ResetCounters();

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
							InvocationCount = 20,
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

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_setupCounter, MethodsCount);
			Assert.AreEqual(_cleanupCounter, MethodsCount);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkBurstMode()
		{
			ResetCounters();

			var config = CompetitionHelpers.ConfigForAssembly
				.WithModifier(
					new Job
					{
						Infrastructure =
						{
							EngineFactory = BurstModeEngineFactory.Instance
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

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_setupCounter, MethodsCount);
			Assert.AreEqual(_cleanupCounter, MethodsCount);

			Assert.IsFalse(summary?.ValidationErrors.Any());
		}

		[Test]
		public static void TestInProcessBenchmarkBurstModeUnroll()
		{
			ResetCounters();

			var config = CompetitionHelpers.ConfigForAssembly
				.WithModifier(
					new Job
					{
						Infrastructure =
						{
							EngineFactory = BurstModeEngineFactory.Instance
						},
						Run =
						{
							InvocationCount = 10, // BUG: works fine with ZERO!!!
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

			Assert.AreEqual(_callCounter, GetExpectedCount(config, MethodsCount));
			Assert.AreEqual(_afterSetupCounter, GetExpectedCount(config, 1));
			Assert.AreEqual(_setupCounter, MethodsCount);
			Assert.AreEqual(_cleanupCounter, MethodsCount);

			Assert.IsFalse(summary?.ValidationErrors.Any());
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
			Assert.AreEqual(_afterSetupCounter, 0);
			Assert.AreEqual(_setupCounter, 0);
			Assert.AreEqual(_cleanupCounter, 0);

			Assert.AreEqual(summary?.ValidationErrors.Length, 1);
			Assert.IsTrue(summary?.ValidationErrors[0].IsCritical);
			Assert.AreEqual(
				summary?.ValidationErrors[0].Message,
				"Job SelfTestConfigX86, EnvMode.Platform was run as X64 (X86 expected). Fix your test runner options.");
		}

		public class InProcessBenchmarkAllCases
		{
			// TODO: custom type results.
			[Setup]
			public static void Setup()
			{
				//await Task.Yield();
				Interlocked.Increment(ref _setupCounter);
				Interlocked.Exchange(ref _afterSetupCounter, 0);
			}

			[Cleanup]
			public void /*decimal*/ Cleanup() => Interlocked.Increment(ref _cleanupCounter);

			#region Instance members
			[Benchmark]
			public void InvokeOnceVoid()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
			}

			[Benchmark]
			public async Task InvokeOnceTaskAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
			}

			[Benchmark]
			public string InvokeOnceRefType()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return "";
			}

			[Benchmark]
			public decimal InvokeOnceValueType()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return 0;
			}

			[Benchmark]
			public async Task<string> InvokeOnceTaskOfTAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return "";
			}

			[Benchmark]
			public ValueTask<decimal> InvokeOnceValueTaskOfT()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return new ValueTask<decimal>(11);
			}
			#endregion

			#region Static members
			[Benchmark]
			public static void InvokeOnceVoidStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
			}

			[Benchmark]
			public static async Task InvokeOnceTaskStaticAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
			}

			[Benchmark]
			public static string InvokeOnceRefTypeStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return "";
			}

			[Benchmark]
			public static decimal InvokeOnceValueTypeStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return 0;
			}

			[Benchmark]
			public static async Task<string> InvokeOnceTaskOfTStaticAsync()
			{
				await Task.Yield();
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return "";
			}

			[Benchmark]
			public static ValueTask<decimal> InvokeOnceValueTaskOfTStatic()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				return new ValueTask<decimal>(11);
			}
			#endregion
		}
	}
}