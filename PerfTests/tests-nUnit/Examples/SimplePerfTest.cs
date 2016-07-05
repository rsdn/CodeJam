using System;
using System.Threading;

using NUnit.Framework;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.PerfTests.Examples
{
	[Category("PerfTests: examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() => Competition.Run(this, CreateRunConfigAnnotate());

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.91, 3.09)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.84, 5.14)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.78, 7.20)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}