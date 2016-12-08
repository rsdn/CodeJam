using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

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
	[SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
	public static class CompetitionRunTests
	{
		private static void AssertCompetitionCompleted(
			CompetitionState runState, MessageSeverity expectedSeverity, int runNumber = 1, bool skipSummary = true)
		{
			if (!skipSummary)
			{
				var summary = runState.LastRunSummary;

				Assert.AreEqual(summary?.ValidationErrors.Length, 0);
			}

			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.HighestMessageSeverity, expectedSeverity);
			Assert.AreEqual(runState.RunNumber, runNumber);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
		}

		[Test]
		public static void CompetitionEmptyBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<EmptyBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, 0);
			AssertCompetitionCompleted(runState, MessageSeverity.SetupError);
			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(
				messages[0].MessageText,
				"No methods in benchmark. Apply CompetitionBenchmarkAttribute / " +
					"CompetitionBaselineAttribute to the benchmark methods.");
		}

		[Test]
		public static void CompetitionOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<OkBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.Informational);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void CompetitionXmlTaskOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<XmlTaskOkBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.Informational);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void CompetitionXmlFullAnnotationBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<XmlFullAnnotationBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.Informational);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void CompetitionNoBaselineFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<NoBaselineFailBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.SetupError);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.SetupError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText,
				"No baseline method for benchmark. Apply CompetitionBaselineAttribute to the one of benchmark methods.");

			Interlocked.Exchange(ref _callCounter, 0);

			runState = SelfTestCompetition.Run<NoBaselineFail2Benchmark>();
			messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.SetupError);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.SetupError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText,
				"No baseline method for benchmark. Apply CompetitionBaselineAttribute to the one of benchmark methods.");

		}

		[Test]
		public static void CompetitionBadLimitsBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BadLimitsBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.ExecutionError, skipSummary: true);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.ExecutionError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Runner);
			Assert.That(
				messages[0].MessageText,
				Does.StartWith(
					"Benchmark BadLimitsBenchmark failed. Exception: Please check competition limits. " +
						"The minRatio (20.2) should be not zero and it should be greater than the maxRatio (5.5)."));
		}

		[Test]
		public static void CompetitionLimitsFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<LimitsFailBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, 3 * ExpectedRunCount); // 3x rerun
			AssertCompetitionCompleted(runState, MessageSeverity.TestError, runNumber: 3);

			Assert.AreEqual(messages.Length, 6);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.That(messages[0].MessageText, Does.StartWith("Method SlowerX10"));
			Assert.That(messages[0].MessageText, Does.Contain(" does not fit into limits "));

			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[1].MessageText, "Requesting 1 run(s): Limit checking failed.");

			Assert.AreEqual(messages[2].RunNumber, 2);
			Assert.AreEqual(messages[2].RunMessageNumber, 1);
			Assert.AreEqual(messages[2].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[2].MessageSource, MessageSource.Analyser);
			Assert.That(messages[2].MessageText, Does.StartWith("Method SlowerX10"));
			Assert.That(messages[2].MessageText, Does.Contain(" does not fit into limits "));

			Assert.AreEqual(messages[3].RunNumber, 2);
			Assert.AreEqual(messages[3].RunMessageNumber, 2);
			Assert.AreEqual(messages[3].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[3].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[3].MessageText, "Requesting 1 run(s): Limit checking failed.");

			Assert.AreEqual(messages[4].RunNumber, 3);
			Assert.AreEqual(messages[4].RunMessageNumber, 1);
			Assert.AreEqual(messages[4].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[4].MessageSource, MessageSource.Analyser);
			Assert.That(messages[4].MessageText, Does.StartWith("Method SlowerX10"));
			Assert.That(messages[4].MessageText, Does.Contain(" does not fit into limits "));

			Assert.AreEqual(messages[5].RunNumber, 3);
			Assert.AreEqual(messages[5].RunMessageNumber, 2);
			Assert.AreEqual(messages[5].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[5].MessageSource, MessageSource.Runner);
			Assert.AreEqual(
				messages[5].MessageText,
				"The benchmark was run 3 time(s) (read log for details). Try to loose competition limits.");
		}

		[Test]
		public static void CompetitionLimitsEmptyFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<LimitsEmptyFailBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount); // 3x rerun
			AssertCompetitionCompleted(runState, MessageSeverity.Warning, runNumber: 1);

			Assert.AreEqual(messages.Length, 1);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText,
				"The benchmark SlowerX10 ignored as it has empty limit. Update limits to include benchmark in the competition.");
		}

		#region Perf test helpers
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSelfTestRunCount;

		private static int _callCounter;

		public class EmptyBenchmark { }

		public class OkBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);
			}

			// Limits loosed as perftest is run only four times and timings on appveyor are very inaccurate
			[CompetitionBenchmark(3, 30)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(10 * CompetitionHelpers.RecommendedSpinCount);
			}
		}

		[CompetitionMetadata(
			"CodeJam.PerfTests.Assets.CompetitionRunTests.xml",
			MetadataResourcePath = @"..\Assets\CompetitionRunTests.xml")]
		public class XmlTaskOkBenchmark
		{
			private const int AwaitDelayMs = 50;
			[CompetitionBaseline]
			public async Task BaselineAsync()
			{
				await Task.Delay(AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
			}

			[CompetitionBenchmark]
			public async Task<int> SlowerX5Async()
			{
				await Task.Delay(5 * AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
				return _callCounter;
			}
		}

		[CompetitionMetadata(
			"CodeJam.PerfTests.Assets.CompetitionRunTests.xml",
			MetadataResourcePath = @"..\Assets\CompetitionRunTests.xml",
			UseFullTypeName = true)]
		public class XmlFullAnnotationBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);
			}

			[CompetitionBenchmark]
			public void SlowerX20()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(20 * CompetitionHelpers.RecommendedSpinCount);
			}
		}

		public class NoBaselineFailBenchmark
		{
			[Benchmark]
			public void Benchmark1() => Interlocked.Increment(ref _callCounter);

			[CompetitionBenchmark]
			public void Benchmark2() => Interlocked.Increment(ref _callCounter);
		}

		public class NoBaselineFail2Benchmark
		{
			[Benchmark]
			public void Benchmark1() => Interlocked.Increment(ref _callCounter);

			[Benchmark]
			public void Benchmark2() => Interlocked.Increment(ref _callCounter);
		}

		public class BadLimitsBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);
			}

			[CompetitionBenchmark(20.2, 5.5)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(10 * CompetitionHelpers.RecommendedSpinCount);
			}
		}

		public class LimitsFailBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);
			}

			[CompetitionBenchmark(1, 1)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(10 * CompetitionHelpers.RecommendedSpinCount);
			}
		}

		public class LimitsEmptyFailBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);
			}

			[CompetitionBenchmark]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(10 * CompetitionHelpers.RecommendedSpinCount);
			}
		}
		#endregion
	}
}