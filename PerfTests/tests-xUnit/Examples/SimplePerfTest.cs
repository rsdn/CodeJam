using System;
using System.Threading;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using Xunit;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples.PerfTests
{
	// A perf test class.
	[Trait("Category", "PerfTests: xUnit examples")]
	[CompetitionBurstMode]
	[CompetitionRerunsModifier(10)]
	public class SimplePerfTest
	{
		private static readonly int _count = CompetitionHelpers.RecommendedSpinCount;

		// Perf test runner method.
		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this);

		// Baseline competition member. Other competition members will be compared with this. Accuracy ±10%
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(_count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.70, 3.30)]
		public void SlowerX3() => Thread.SpinWait(3 * _count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.50, 5.50)]
		public void SlowerX5() => Thread.SpinWait(5 * _count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.30, 7.70)]
		public void SlowerX7() => Thread.SpinWait(7 * _count);
	}
}