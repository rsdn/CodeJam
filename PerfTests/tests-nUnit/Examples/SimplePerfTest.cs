using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace CodeJam.Examples
{
	[Category("PerfTests: examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.74, 3.50)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.58, 5.51)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.53, 8.15)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}