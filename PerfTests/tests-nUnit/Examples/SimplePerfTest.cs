using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.Examples
{
	[Category("PerfTests: examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfig);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.92, 3.05)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.87, 5.07)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.77, 7.05)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}