using System;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.CompetitionLimitProviders;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.IntegrationTests
{
	[PublicAPI]
	public static class PerfTestHelpers
	{
		public const int SpinCount = 100 * 1000;

		// BUG: Jitting performed twice, https://github.com/PerfDotNet/BenchmarkDotNet/issues/184
		// Jitting = 2, WarmupCount = 2, TargetCount = 1
		public const int ExpectedSingleRunCount = 6;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);

		public static readonly ICompetitionConfig SingleRunConfig = CreateSingleRunConfig(Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig HighAccuracyConfig = CreateHighAccuracyConfig().AsReadOnly();

		private static ManualCompetitionConfig CreateConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				AllowDebugBuilds = true,
				RerunIfLimitsFailed = true,
				CompetitionLimitProvider = ConfidenceIntervalLimitProvider.Instance
			};

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());

			return result;
		}

		public static ManualCompetitionConfig CreateSingleRunConfig(Platform platform)
		{
			var result = CreateConfigCore();

			result.Add(
				new Job
				{
					Affinity = -1,
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 2,
					TargetCount = 2,
					Platform = platform,
					Toolchain = InProcessToolchain.Instance
				});

			return result;
		}


		public static ManualCompetitionConfig CreateHighAccuracyConfig(bool outOfProcess = false)
		{
			var result = CreateConfigCore();
			result.LogCompetitionLimits = true;
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 50,
					TargetCount = 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = outOfProcess ? null : InProcessToolchain.Instance
				});

			return result;
		}
	}
}