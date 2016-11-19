using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	// A perf test class.
	[Category("PerfTests: NUnit examples")]
	//[CompetitionReannotateSources]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.96, 3.08)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.94, 5.14)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.89, 7.17)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}