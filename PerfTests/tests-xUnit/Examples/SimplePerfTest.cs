using System;
using System.Threading;

using CodeJam.PerfTests;

using Xunit;

namespace CodeJam.Examples
{
	[Trait("Category", "PerfTests: xUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.89, 3.01)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.88, 5.21)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.77, 7.05)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}