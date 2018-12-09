using BenchmarkDotNet.Attributes;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionAccuracyTests
	{
		[Test]
		public static void CompetitionTooFastBenchmark()
		{
			var runState = SelfTestCompetition.Run<TooFastBenchmark>();
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary?.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 3);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[0].MessageText,
				"Benchmarks TooFast, TooFastX5: measured run time is less than 1.00 us. " +
					"Timings are imprecise as they are too close to the timer resolution.");


			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[1].MessageText,
				".Descriptor TooFast. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");


			Assert.AreEqual(messages[2].RunNumber, 1);
			Assert.AreEqual(messages[2].RunMessageNumber, 3);
			Assert.AreEqual(messages[2].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[2].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[2].MessageText,
				".Descriptor TooFastX5. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");
		}

		[Test]
		public static void CompetitionTooSlowBenchmark()
		{
			var runState = SelfTestCompetition.Run<TooSlowBenchmark>();
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary?.ValidationErrors.Length, 0);
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
				"BenchmarkCase TooSlow: measured run time is greater than 500.0 ms. " +
					"There's a risk the peak timings were hidden by averages. " +
					"Consider to reduce the number of iterations performed per each measurement.");
		}

		[Test]
		public static void CompetitionTooSlowOk()
		{
			var overrideConfig = CompetitionHelpers
				.CreateConfig(typeof(TooSlowBenchmark))
				.WithLongRunningBenchmarkLimit(TimeSpan.FromMinutes(2));

			var runState = SelfTestCompetition.Run<TooSlowBenchmark>(overrideConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(summary?.ValidationErrors.Length, 0);
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

			var runState = SelfTestCompetition.Run<HighAccuracyBenchmark>();
			var messages = runState.GetMessages();
			if (messages.All(m => m.MessageText != "All competition metrics are ok."))
			{
				Assert.Ignore("The environment does not provide accurate timings. Test results cannot be trusted.");
			}
		}

		#region BenchmarkCase classes
		[PublicAPI]
		[CompetitionModifier(typeof(CompetitionHighAccuracyModifier))]
		public class TooFastBenchmark
		{
			[Benchmark(Baseline = true)]
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
			public int TooFastX5()
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

		[CompetitionModifier(typeof(CompetitionHighAccuracyModifier))]
		public class HighAccuracyBenchmark
		{
			private static readonly int _count = CompetitionRunHelpers.BurstModeLoopCount;

			[CompetitionBaseline]
			public void Baseline() => CompetitionRunHelpers.Delay(_count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run1() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run2() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run3() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(4.65, 5.35)]
			public void SlowerX5() => CompetitionRunHelpers.Delay(5 * _count);
		}

		public class HighAccuracyBenchmarkOutOfProcess
		{
			private static readonly int _count = CompetitionRunHelpers.BurstModeLoopCount;

			[CompetitionBaseline]
			public void Baseline() => CompetitionRunHelpers.Delay(_count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run1() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run2() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(1.85, 2.15)]
			public void SlowerX2Run3() => CompetitionRunHelpers.Delay(2 * _count);

			[CompetitionBenchmark(4.65, 5.35)]
			public void SlowerX5() => CompetitionRunHelpers.Delay(5 * _count);
		}
		#endregion
	}
}