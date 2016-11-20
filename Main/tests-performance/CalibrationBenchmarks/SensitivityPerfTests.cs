using System;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam
{
	/// <summary>
	/// Proof test: benchmark is sensitive enough to spot a minimal method change
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Self-testing")]
	[CompetitionBurstMode]
	//[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	public class SensitivityPerfTests
	{
		[Params(5000, 10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; }

		[Test]
		public void RunSensitivityPerfTests()
		{
			// The test could fail with "too fast" warning, it's ok
			var overrideConfig = CompetitionHelpers.ConfigForAssembly.WithReportWarningsAsErrors(false);
			Competition.Run(this, overrideConfig);
		}

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

		[CompetitionBenchmark(1.2, 5.2)]
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