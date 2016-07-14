using System;
using System.Threading;

using CodeJam.PerfTests;

using Xunit;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	[Trait("Category", "PerfTests: xUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// WAITINGFOR: https://github.com/xunit/xunit/issues/490
#if (!CI)
		[CompetitionFact]
#endif
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.88, 3.06)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.84, 5.26)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.76, 7.36)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}