using System;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;

namespace CodeJam.PerfTests
{
	public class PerfTestConfig : ManualCompetitionConfig
	{
		// BUG: Jitting performed twice, https://github.com/PerfDotNet/BenchmarkDotNet/issues/184
		// Jitting = 2, TargetCount = 1
		public const int ExpectedRunCountNoWarmup = 3;
		// Jitting = 2, WarmupCount = 1, TargetCount = 1
		public const int ExpectedRunCount = 4;

		public static readonly new ICompetitionConfig Default = new PerfTestConfig();
		public static readonly ICompetitionConfig NoWarmup = new PerfTestConfig(Platform.Host, true);

		public static readonly ICompetitionConfig X64 = new PerfTestConfig(Platform.X64, false);
		public static readonly ICompetitionConfig X86 = new PerfTestConfig(Platform.X86, false);

		public PerfTestConfig() : this(Platform.Host, false) { }

		private PerfTestConfig(Platform platform, bool noWarmup)
		{
			var job = new Job
			{
				LaunchCount = 1,
				Mode = Mode.SingleRun,
				TargetCount = 1,
				Platform = platform,
				Toolchain = InProcessToolchain.Instance,
				WarmupCount = noWarmup ? 0 : 1
			};
			Add(job);
			DebugMode = true;
			EnableReruns = true;
		}
	}
}