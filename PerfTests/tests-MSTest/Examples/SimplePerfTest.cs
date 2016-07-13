using System;
using System.Threading;

using CodeJam.PerfTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace CodeJam.Examples
{
	[TestClass]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[TestMethod]
		[TestCategory("PerfTests: MSTest examples")]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.84, 3.07)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.71, 5.21)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.60, 7.16)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}