using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

namespace CodeJam.BenchmarkDotNet
{
	public class PerfTestConfig : ManualCompetitionConfig
	{
		// Jitting = 1, TargetCount = 1
		public const int ExpectedRunCountNoWarmup = 2;
		// Jitting = 1, WarmupCount = 1, TargetCount = 1
		public const int ExpectedRunCount = 3;

		public static readonly new ICompetitionConfig Default = new PerfTestConfig();
		public static readonly ICompetitionConfig NoWarmup = new PerfTestConfig(Platform.Host, true);

		public static readonly ICompetitionConfig X64 = new PerfTestConfig(Platform.X64, false);
		public static readonly ICompetitionConfig X86 = new PerfTestConfig(Platform.X86, false);

		public PerfTestConfig() : this(Platform.Host, false) { }

		private PerfTestConfig(Platform platform, bool noWarmup)
		{
			var job = new Job()
			{
				LaunchCount = 1,
				Mode = Mode.SingleRun,
				TargetCount = 1,
				Platform = platform,
				Toolchain = InProcessToolchain.Default,
				WarmupCount = noWarmup ? 0 : 1
			};
			Add(job);
			DebugMode = true;
			EnableReruns = true;
		}
	}
}