using System;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;

namespace CodeJam.PerfTests
{
	public sealed class PerfTestConfig : ReadOnlyCompetitionConfig
	{
		public const int SpinCount = 100 * 1000;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);

		// BUG: Jitting performed twice, https://github.com/PerfDotNet/BenchmarkDotNet/issues/184
		// Jitting = 2, WarmupCount = 2, TargetCount = 1
		public const int ExpectedRunCount = 6;

		public static readonly ICompetitionConfig SingleRunConfig = new PerfTestConfig();

		public static readonly ICompetitionConfig X64 = new PerfTestConfig(Platform.X64);
		public static readonly ICompetitionConfig X86 = new PerfTestConfig(Platform.X86);

		public PerfTestConfig() : this(Platform.Host) { }

		private PerfTestConfig(Platform platform) : base(Create(platform)) { }

		private static ManualCompetitionConfig Create(Platform platform)
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
	}
}