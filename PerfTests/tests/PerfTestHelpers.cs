using System;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	public static class PerfTestHelpers
	{
		public const int SpinCount = 100 * 1000;
		public const int LoopCount = 10 * 1000;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);
		// BUG: Jitting performed twice, https://github.com/PerfDotNet/BenchmarkDotNet/issues/184
		// Jitting = 2, WarmupCount = 2, TargetCount = 1

		public static readonly ICompetitionConfig SingleRunConfig =
			CreateSingleRunConfig(IntPtr.Size == 4 ? Platform.X86 : Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig OtherPlatformSingleRunConfig =
			CreateSingleRunConfig(IntPtr.Size == 8 ? Platform.X86 : Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig HighAccuracyConfig = CreateHighAccuracyConfig().AsReadOnly();

		public const int ExpectedSingleRunCount = 6;

		public static ManualCompetitionConfig CreateSingleRunConfig(Platform platform)
		{
			var result = new ManualCompetitionConfig();

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 2,
					TargetCount = 2,
					Platform = platform,
					Toolchain = InProcessToolchain.Instance
				});
			result.AllowDebugBuilds = true;
			result.RerunIfLimitsFailed = true;

			return result;
		}

		public static ManualCompetitionConfig CreateHighAccuracyConfig()
		{
			var result = new ManualCompetitionConfig();

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 50,
					TargetCount = 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = InProcessToolchain.DontLogOutput
				});
			result.AllowDebugBuilds = true;
			result.DetailedLogging = false;
			result.LogCompetitionLimits = true;
			result.RerunIfLimitsFailed = true;

			return result;
		}
	}
}