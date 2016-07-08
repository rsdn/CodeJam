using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
	public static class CompetitionAccuracyTests
	{
		private static readonly ICompetitionConfig _debugConfig = new ManualCompetitionConfig(RunConfig)
		{
			AllowDebugBuilds = true
		};

		[Test]
		public static void CompetitionTooFastBenchmark()
		{
			var runState = SelfTestRunner.Run<TooFastBenchmark>(_debugConfig);
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
				"The benchmark(s) TooFast, TooFast2 run faster than 0.0015ms. Results cannot be trusted.");
		}

		[Test]
		public static void CompetitionTooSlowBenchmark()
		{
			var runState = SelfTestRunner.Run<TooSlowBenchmark>(SelfTestConfig);
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
				"The benchmark(s) TooSlow run longer than 0.5s." +
					" Consider to rewrite the test as the peek timings will be hidden by averages" +
					" or enable long running benchmarks support in the config.");
		}

		[Test]
		public static void CompetitionTooSlowOk()
		{
			var overrideConfig = new ManualCompetitionConfig(SelfTestConfig)
			{
				AllowLongRunningBenchmarks = true
			};

			var runState = SelfTestRunner.Run<TooSlowBenchmark>(overrideConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 0);
		}

		[Test]
		public static void CompetitionHighAccuracyBenchmark()
		{
			IgnoreIfDebug();

			var runState = SelfTestRunner.Run<HighAccuracyBenchmark>(RunConfig);
			var messages = runState.GetMessages();
			if (messages.All(m => m.MessageText != "CompetitionAnalyser: All competition limits are ok."))
			{
				Assert.Ignore("The environment does not provide accurate timings. Test results cannot be trusted.");
			}
		}

		[Test]
		public static void CompetitionHighAccuracyBenchmarkOutOfProcess()
		{
			IgnoreIfDebug();

			var overrideConfig = CreateRunConfig(outOfProcess: true);

			var runState = SelfTestRunner.Run<HighAccuracyBenchmarkOutOfProcess>(overrideConfig);
			var messages = runState.GetMessages();
			if (messages.All(m => m.MessageText != "CompetitionAnalyser: All competition limits are ok."))
			{
				Assert.Ignore("The environment does not provide accurate timings. Test results cannot be trusted.");
			}
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
				for (var i = 0; i < 50; i++)
				{
					a = a + i;
				}
				return a;
			}
		}

		public class TooSlowBenchmark
		{
			[CompetitionBaseline]
			public void TooSlow() => Thread.Sleep(550);
		}

		public class HighAccuracyBenchmark
		{
			private const int Count = 4 * CompetitionHelpers.DefaultCount;

			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run1() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run2() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run3() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(4.65, 5.35)]
			public void SlowerX5() => CompetitionHelpers.Delay(5 * Count);
		}

		public class HighAccuracyBenchmarkOutOfProcess
		{
			private const int Count = CompetitionHelpers.DefaultCount;

			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run1() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run2() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run3() => CompetitionHelpers.Delay(2 * Count);

			[CompetitionBenchmark(4.65, 5.35)]
			public void SlowerX5() => CompetitionHelpers.Delay(5 * Count);
		}
		#endregion
	}
}