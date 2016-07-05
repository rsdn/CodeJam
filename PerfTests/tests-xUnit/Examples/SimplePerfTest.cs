using System;
using System.Threading;

using Xunit;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.PerfTests.Examples
{
	[Trait("Category", "PerfTests: xUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[SkippableFact]
		public void RunSimplePerfTest() => Competition.Run(this, CreateRunConfigAnnotate());

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.91, 3.09)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.81, 5.11)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.65, 7.07)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}