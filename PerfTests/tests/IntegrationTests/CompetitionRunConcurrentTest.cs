using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CodeJam.PerfTests.Configs;
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
	[CompetitionModifier(typeof(CompetitionHighAccuracyModifier))]
	public static class CompetitionRunConcurrentTest
	{
		[TestCase(ConcurrentRunBehavior.Lock)]
		[TestCase(ConcurrentRunBehavior.Skip)]
		[TestCase(ConcurrentRunBehavior.Default)]
		public static void CompetitionRunConcurrent(ConcurrentRunBehavior concurrentRunBehavior)
		{
			var benchmarks = new[]
			{
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark),
				typeof(ConcurrentRunBenchmark)
			};
			var config = CompetitionHelpers.CreateConfig(typeof(CompetitionRunConcurrentTest))
				.WithCompetitionOptions(
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
			var okCount = messages.Count(m => m.MessageText == "CompetitionAnalyser: All competition limits are ok.");

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
			}
		}

		#region Perf test helpers
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSelfTestRunCount;

		public class ConcurrentRunBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.DefaultCount);

			[CompetitionBenchmark(5, 20)]
			public void SlowerX10() => CompetitionHelpers.Delay(10 * CompetitionHelpers.DefaultCount);
		}
		#endregion
	}
}