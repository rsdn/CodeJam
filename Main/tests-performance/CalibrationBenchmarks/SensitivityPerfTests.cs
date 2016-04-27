using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.NUnit;

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
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class SensitivityPerfTests
	{
		[Params(1000, 10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; }

		[Test]
		public void RunSensitivityPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00Baseline()
		{
			var sum = 0;
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				sum += i;
			}

			return sum;
		}

		[CompetitionBenchmark(1.46, 1.79)]
		public int Test01PlusOne()
		{
			var sum = 0;
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				sum += i + 1;
			}

			return sum;
		}
	}
}