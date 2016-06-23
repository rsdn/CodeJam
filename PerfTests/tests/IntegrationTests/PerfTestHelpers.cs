using System;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Exporters;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.IntegrationTests
{
	[PublicAPI]
	public static class PerfTestHelpers
	{
		public const int SpinCount = 10 * 1000;

		// BUG: Jitting performed twice, https://github.com/PerfDotNet/BenchmarkDotNet/issues/184
		// Jitting = 2, WarmupCount = 2, TargetCount = 1
		public const int ExpectedSingleRunCount = 6;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);

		public static readonly ICompetitionConfig SingleRunConfig = CreateSingleRunConfig(Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig DefaultRunConfig = CreateRunConfig().AsReadOnly();

		private static ManualCompetitionConfig CreateRunConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				AllowDebugBuilds = true,
				RerunIfLimitsFailed = true
			};

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());
			result.Add(TimingsExporter.Instance);

			return result;
		}

		public static ManualCompetitionConfig CreateSingleRunConfig(Platform platform)
		{
			var result = CreateRunConfigCore();
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

		public static ManualCompetitionConfig CreateAltConfig()
		{
			var result = CreateRunConfigCore();

			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 20,
					TargetCount = 50,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = InProcessToolchain.Instance
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfig(bool detailedLogging = false, bool outOfProcess = false)
		{
			var result = CreateRunConfigCore();

			result.DetailedLogging = detailedLogging;
			result.LogCompetitionLimits = detailedLogging;

			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 50,
					TargetCount = 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = outOfProcess
						? null :
						(detailedLogging ? InProcessToolchain.Instance : InProcessToolchain.DontLogOutput)
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigAnnotate(bool detailedLogging = false)
		{
			var result = CreateRunConfig(detailedLogging);
			result.RerunIfLimitsFailed = true;
			result.UpdateSourceAnnotations = true;
			result.MaxRunsAllowed = 6;
			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigReAnnotate(bool detailedLogging = false)
		{
			var result = CreateRunConfigAnnotate(detailedLogging);
			result.IgnoreExistingAnnotations = true;
			return result;
		}
	}
}