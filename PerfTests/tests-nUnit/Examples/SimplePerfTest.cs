using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace CodeJam.Examples
{
	[Category("PerfTests: NUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.91, 3.12)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.84, 5.25)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.78, 7.30)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}