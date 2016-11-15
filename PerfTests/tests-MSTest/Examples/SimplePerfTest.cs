using System;
using System.Threading;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Columns;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Limits;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	// A perf test class.
	[TestClass]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[TestMethod]
		[TestCategory("PerfTests: MSTest examples")]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionConfig.Default);

		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.85, 3.15)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.75, 5.25)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.70, 7.30)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}