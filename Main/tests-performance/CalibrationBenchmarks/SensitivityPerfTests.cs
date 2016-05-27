using System;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: benchmark is sensitive enough to spot a minimal method change
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[PublicAPI]
	//[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class SensitivityPerfTests
	{
		[Params(1000, 10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; }

		[Test]
		public void RunSensitivityPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00Baseline()
		{
			var a = 0;
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				a += i;
			}
			return a;
		}

		[CompetitionBenchmark(1.42, 3.2)]
		public int Test01PlusOne()
		{
			var a = 0;
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				a += i + 1;
			}
			return a;
		}
	}
}