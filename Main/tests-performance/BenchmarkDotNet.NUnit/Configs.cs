using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	/// <summary>
	/// Use this to run fast but inaccurate measures
	/// </summary>
	[PublicAPI]
	public class FastRunConfig : ManualConfig
	{
		/// <summary>
		/// Instance of the config
		/// </summary>
		public static readonly IConfig Instance = new FastRunConfig();

		/// <summary>
		/// Constructor
		/// </summary>
		public FastRunConfig()
		{
			Add(
				new Job
				{
					IterationTime = 100,
					LaunchCount = 1,
					WarmupCount = 3,
					TargetCount = 10,
					Jit = Jit.RyuJit
				});
		}
	}

	/// <summary>
	/// Use this to run slower but proven-to-be-accurate perf tests
	/// </summary>
	[PublicAPI]
	public class TestProofConfig : ManualConfig
	{
		/// <summary>
		/// Instance of the config
		/// </summary>
		public static readonly IConfig Instance = new TestProofConfig();

		/// <summary>
		/// Constructor
		/// </summary>
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