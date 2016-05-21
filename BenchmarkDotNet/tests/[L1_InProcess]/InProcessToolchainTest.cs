using System;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.BenchmarkDotNet
{
	[PublicAPI]
	[TestFixture(Category = "BenchmarkDotNet")]
	public static class InProcessToolchainTest
	{
		#region Configs
		public class InProcessConfig : ManualCompetitionConfig
		{
			[UsedImplicitly]
			public static readonly new ICompetitionConfig Default = new InProcessConfig();
			public static readonly ICompetitionConfig X64 = new InProcessConfig(Platform.X64);
			public static readonly ICompetitionConfig X86 = new InProcessConfig(Platform.X86);

			public InProcessConfig() : this(Platform.Host)
			{ }

			private InProcessConfig(Platform platform)
			{
				Add(
					Job.Dry
					.With(platform)
					.With(Mode.SingleRun)
					.With(InProcessToolchain.Default));
				Add(InProcessValidator.DontFailOnError);
				DebugMode = true;
				UnionRule = ConfigUnionRule.AlwaysUseLocal;
			}
		}
		#endregion

		#region TestInProcess
		private static int _testCount;
		private static int _setupTestCount;

		[Test]
		public static void TestInProcess()
		{
			// Call for jit = 1, LaunchCount = 1, WarmupCount = 1, TargetCount = 1
			const int expectedCount = 4;

			var summary = CompetitionBenchmarkRunner.Run<InProcessBenchmark>();
			Assert.AreEqual(_testCount, expectedCount);
			Assert.AreEqual(_setupTestCount, 1);

			Assert.IsFalse(summary.ValidationErrors.Any());
		}

		[PublicAPI]
		[Config(typeof(InProcessConfig))]
		public class InProcessBenchmark
		{
			[Setup]
			public void Setup() => Interlocked.Exchange(ref _setupTestCount, 0);

			[Benchmark]
			public static void InvokeOnce()
			{
				Interlocked.Increment(ref _testCount);
				Interlocked.Increment(ref _setupTestCount);
				Thread.Sleep(1);
			}
		}
		#endregion

		#region TestInProcessWithValidation
		[Test]
		public static void TestInProcessWithValidation()
		{
			// DONTTOUCH: config SHOULD NOT match the platform.
			var config = IntPtr.Size == 8
				? InProcessConfig.X86
				: InProcessConfig.X64;

			var summary = CompetitionBenchmarkRunner.Run<InProcessWithValidationBenchmark>(config);
			Assert.AreEqual(summary.ValidationErrors.Length, 1);
		}

		[PublicAPI]
		public class InProcessWithValidationBenchmark
		{
			[Benchmark]
			public static void InvokeOnce() => Thread.Sleep(1);
		}
		#endregion
	}
}