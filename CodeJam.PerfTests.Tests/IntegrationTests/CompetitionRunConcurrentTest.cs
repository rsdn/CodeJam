using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[PublicAPI]
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[CompetitionModifier(typeof(CompetitionHighAccuracyModifier))]
	public static class CompetitionRunConcurrentTest
	{
		[TestCase(ConcurrentRunBehavior.Lock)]
		[TestCase(ConcurrentRunBehavior.Skip)]
		[TestCase(ConcurrentRunBehavior.Default)]
		[Ignore("Will be enabled after concurrency runs fill be fixed")]
		public static void CompetitionRunConcurrent(ConcurrentRunBehavior concurrentRunBehavior)
		{
			return;
			/*var benchmarks = new[]
			{
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark)
			};
			var config = CompetitionHelpers.CreateConfig(typeof(CompetitionRunConcurrentTest))
				.WithModifier(
					new CompetitionOptions
					{
						RunOptions =
						{
							Concurrent = concurrentRunBehavior
						}
					});

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

			var concurrentRunsCount = benchmarks.Length;
			var errorResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.SetupError);
			var warningResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.Warning);
			var okResultCount = runResults.Count(r => r.HighestMessageSeverity == MessageSeverity.Informational);

			var messages = runResults.SelectMany(r => r.GetMessages()).ToArray();
			var warningMessageCount = messages.Count(
				m => m.MessageText ==
					"Competition run skipped. Competitions cannot be run in parallel, be sure to disable parallel test execution.");
			var okCount = messages.Count(m => m.MessageText == "All competition metrics are ok.");

			switch (concurrentRunBehavior)
			{
				case ConcurrentRunBehavior.Lock:
					Assert.AreEqual(errorResultCount, 0);
					Assert.AreEqual(warningResultCount, 0);
					Assert.AreEqual(okResultCount, concurrentRunsCount);

					Assert.AreEqual(warningMessageCount, 0);
					Assert.AreEqual(okCount, concurrentRunsCount);
					break;
				case ConcurrentRunBehavior.Skip:
					Assert.AreEqual(errorResultCount, 0);
					Assert.Greater(warningResultCount, 0);
					Assert.Less(okResultCount, concurrentRunsCount);

					Assert.Greater(warningMessageCount, 0);
					Assert.Less(okCount, concurrentRunsCount);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}*/
		}

		#region Perf test helpers
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSelfTestRunCount;

		public class ConcurrentRunBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);

			[CompetitionBenchmark(5, 20)]
			public void SlowerX10() => CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
		}
		#endregion
	}
}