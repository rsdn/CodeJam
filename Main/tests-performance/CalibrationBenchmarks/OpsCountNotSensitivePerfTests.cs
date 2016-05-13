using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.UnitTesting;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: benchmark is not sensitive enough if OperationsPerInvoke is used instead of tight loop.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[PublicAPI]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class OpsCountNotSensitivePerfTests
	{
		#region PerfTest helpers
		private int _result;

		[Setup]
		public void Setup() => _result = 0;
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		public void RunOpsCountNotSensitivePerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[Benchmark(Baseline = true, OperationsPerInvoke = Count)]
		public int Test00Baseline() => _result = ++_result;

		[CompetitionBenchmark(0.22, 6.64, OperationsPerInvoke = Count)]
		public int Test01PlusTwo() => _result = ++_result + 1;
	}
}