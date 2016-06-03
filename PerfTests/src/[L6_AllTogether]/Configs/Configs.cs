using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	// TODO: tune job settings

	/// <summary>Use this to run fast but inaccurate competitions.</summary>
	[PublicAPI]
	public sealed class FastRunConfig : ReadOnlyConfig
	{
		/// <summary>Instance of the config.</summary>
		public static readonly IConfig Instance = new FastRunConfig();

		/// <summary>Constructor.</summary>
		public FastRunConfig(): base(Create())
		{
		}

		private static ManualConfig Create()
		{
			var result = new ManualConfig();

			result.Add(
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

			return result;
		}
	}

	/// <summary>Use this to run very slow but proven-to-be-accurate perf tests.</summary>
	[PublicAPI]
	public sealed class TestProofConfig : ReadOnlyConfig
	{
		/// <summary>Instance of the config.</summary>
		public static readonly IConfig Instance = new TestProofConfig();

		/// <summary>Constructor.</summary>
		public TestProofConfig() : base(Create())
		{
		}

		private static ManualConfig Create()
		{
			var result = new ManualConfig();

			result.Add(
				new Job
				{
					TargetCount = 10
				});

			return result;
		}
	}
}