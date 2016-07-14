using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[PublicAPI]
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
	public static class CompetitionRunConcurrentTest
	{
		[TestCase(ConcurrentRunBehavior.Lock)]
		[TestCase(ConcurrentRunBehavior.Fail)]
		[TestCase(ConcurrentRunBehavior.Default)]
		public static void CompetitionRunConcurrent(ConcurrentRunBehavior concurrentRunBehavior)
		{
			var benchmarks = new[]
			{
				typeof(ConcurrentRunBenchmark1),
				typeof(ConcurrentRunBenchmark2),
				typeof(ConcurrentRunBenchmark3),
				typeof(ConcurrentRunBenchmark4),
				typeof(ConcurrentRunBenchmark5)
			};
			var config = CreateHighAccuracyConfig();
			config.ConcurrentRunBehavior = concurrentRunBehavior;

			CompetitionState[] runResults;
			using (var barrier = new Barrier(benchmarks.Length))
			{
				var tasks = benchmarks.Select(
					benchmarkType => Task.Run(
						() =>
						{
							// ReSharper disable once AccessToDisposedClosure
							barrier.SignalAndWait();
							return SelfTestCompetition.Run(benchmarkType, config);
						}));

				runResults = Task.WhenAll(tasks).Result;
			}

			int concurrentRunsCount = benchmarks.Length;
			var errorResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.SetupError);
			var warningResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.Warning);
			var okResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.Informational);

			var messages = runResults.SelectMany(r => r.GetMessages()).ToArray();
			var errorMessageCount = messages.Count(
				m => m.MessageText ==
					"Competitions cannot be run in parallel. Be sure to disable parallel test execution. Competition run skipped.");
			var warningMessageCount = messages.Count(
				m => m.MessageText ==
					"There are another competitions being run in parallel. The timings can be affected by them.");
			var okCount = messages.Count(m => m.MessageText == "CompetitionAnalyser: All competition limits are ok.");

			switch (concurrentRunBehavior)
			{
				case ConcurrentRunBehavior.Lock:
					Assert.AreEqual(errorResultCount, 0);
					Assert.AreEqual(warningResultCount, 0);
					Assert.AreEqual(okResultCount, concurrentRunsCount);

					Assert.AreEqual(errorMessageCount, 0);
					Assert.AreEqual(warningMessageCount, 0);
					Assert.AreEqual(okCount, concurrentRunsCount);
					break;
				case ConcurrentRunBehavior.Fail:
					Assert.Greater(errorResultCount, 0);
					Assert.AreEqual(warningResultCount, 0);
					Assert.Less(okResultCount, concurrentRunsCount);

					Assert.Greater(errorMessageCount, 0);
					Assert.AreEqual(warningMessageCount, 0);
					Assert.Less(okCount, concurrentRunsCount);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Perf test helpers
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSelfTestRunCount;

		public class ConcurrentRunBenchmarkBase
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.DefaultCount);

			[CompetitionBenchmark(5, 20)]
			public void SlowerX10() => CompetitionHelpers.Delay(10 * CompetitionHelpers.DefaultCount);
		}

		public class ConcurrentRunBenchmark1 : ConcurrentRunBenchmarkBase { }
		public class ConcurrentRunBenchmark2 : ConcurrentRunBenchmarkBase { }
		public class ConcurrentRunBenchmark3 : ConcurrentRunBenchmarkBase { }
		public class ConcurrentRunBenchmark4 : ConcurrentRunBenchmarkBase { }
		public class ConcurrentRunBenchmark5 : ConcurrentRunBenchmarkBase { }
		#endregion
	}
}