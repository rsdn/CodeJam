using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Use this to run fast but inaccurate competitons</summary>
	[PublicAPI]
	public class FastRunConfig : ManualConfig
	{
		/// <summary>Instance of the config</summary>
		public static readonly IConfig Instance = new FastRunConfig();

		/// <summary>Constructor</summary>
		public FastRunConfig()
		{
			Add(
				new Job
				{
					IterationTime = 10,
					LaunchCount = 2,
					WarmupCount = 5,
					TargetCount = 10,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = InProcessToolchain.Instance
				});
		}
	}

	/// <summary>Use this to run very slow but proven-to-be-accurate perf tests</summary>
	[PublicAPI]
	public class TestProofConfig : ManualConfig
	{
		/// <summary>Instance of the config</summary>
		public static readonly IConfig Instance = new TestProofConfig();

		/// <summary>Constructor</summary>
		public TestProofConfig()
		{
			Add(
				new Job
				{
					TargetCount = 10
				});
		}
	}
}