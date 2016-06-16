using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Exporters;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.PerfTestHelpers;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionAnalyserAccuracyTests
	{

		[Test]
		public static void TestCompetitionAnalyserTooFastBenchmark()
		{
			var config = CreateHighAccuracyConfig();
			config.DetailedLogging = true;

			var runState = new PerfTestRunner().Run<TooFastBenchmark>(config);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[0].MessageText,
				"The benchmarks TooFast, TooFast2 run faster than 0.0015ms. Results cannot be trusted.");
		}

		[Test]
		public static void TestCompetitionAnalyserTooSlowBenchmark()
		{
			var runState = new PerfTestRunner().Run<TooSlowBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[0].MessageText,
				"The benchmarks TooSlow run longer than 0.5s." +
					" Consider to rewrite the test as the peek timings will be hidden by averages" +
					" or enable long running benchmarks support in the config.");
		}

		[Test]
		public static void TestCompetitionAnalyserTooSlowOk()
		{
			var overrideConfig = new ManualCompetitionConfig(SingleRunConfig)
			{
				AllowLongRunningBenchmarks = true
			};

			var runState = new PerfTestRunner().Run<TooSlowBenchmark>(overrideConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void TestCompetitionAnalyserHighAccuracyBenchmark()
		{
			var overrideConfig = new ManualCompetitionConfig(HighAccuracyConfig);
			overrideConfig.DetailedLogging = true;
			overrideConfig.LimitMetricProvider = RatioCIMetricProvider.Instance;
			overrideConfig.Add(TimingsExporter.Instance);

			var runState = new PerfTestRunner().Run<HighAccuracyBenchmark>(overrideConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 14, "Timeout failed");
		}

		#region Benchmark classes
		[PublicAPI]
		public class TooFastBenchmark
		{
			[Benchmark]
			public int TooFast()
			{
				var a = 0;
				for (var i = 0; i < 10; i++)
				{
					a = a + i;
				}
				return a;
			}

			[Benchmark]
			public int TooFast2()
			{
				var a = 0;
				for (var i = 0; i < 100; i++)
				{
					a = a + i;
				}
				return a;
			}
		}

		public class TooSlowBenchmark
		{
			[CompetitionBenchmark(DoesNotCompete = true)]
			public void TooSlow() => Thread.Sleep(550);
		}

		public class HighAccuracyBenchmark
		{
			//[Params(SpinCount / 10, SpinCount)]
			public int Count { get; set; } = SpinCount;

			[CompetitionBaseline]
			public void Baseline() => Delay(Count);

			[CompetitionBenchmark(1.93, 2.08)]
			public void SlowerX2Run1() => Delay(2 * Count);

			[CompetitionBenchmark(1.91, 2.06)]
			public void SlowerX2Run2() => Delay(2 * Count);

			[CompetitionBenchmark(1.92, 2.06)]
			public void SlowerX2Run3() => Delay(2 * Count);

			[CompetitionBenchmark(4.81, 5.15)]
			public void SlowerX5() => Delay(5 * Count);
		}
		#endregion
	}
}