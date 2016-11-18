using System;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam
{
	/// <summary>
	/// Proof test: benchmark is not sensitive enough if OperationsPerInvoke is used instead of tight loop.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Self-testing")]
	[PublicAPI]
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	public class OpsCountNotSensitivePerfTests
	{
		#region PerfTest helpers
		private int _result;

		[Setup]
		public void Setup() => _result = 0;
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		public void RunOpsCountNotSensitivePerfTests()
		{
			// The test will fail with "too fast" warning, it's ok
			var overrideConfig = CompetitionHelpers.ConfigForAssembly.WithReportWarningsAsErrors(false);
			Competition.Run(this, overrideConfig);
		}

		[Benchmark(Baseline = true, OperationsPerInvoke = Count)]
		public int Test00Baseline() => _result = ++_result;

		[CompetitionBenchmark(0.22, 6.64, OperationsPerInvoke = Count)]
		public int Test01PlusTwo() => _result = ++_result + 1;
	}
}