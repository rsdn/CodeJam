using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Running.Core;

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

			Assert.AreEqual(messages[0].MessageText, "Nothing to check as there is no methods in benchmark.");
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

			Assert.AreEqual(messages[0].MessageText, "All competition metrics are ok.");
		}

		[Test]
		public static void CompetitionXmlTaskOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<XmlTaskOkBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, 2 * ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.Informational);

			Assert.AreEqual(messages.Length, 3);

			Assert.AreEqual(messages[0].MessageText, "Target SlowerX2Async. Metric validation skipped as the method is marked with CompetitionBenchmarkAttribute.DoesNotCompete set to true.");
			Assert.AreEqual(messages[1].MessageText, "Target SlowerX3Async. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");
			Assert.AreEqual(messages[2].MessageText, "All competition metrics are ok.");
		}

		[Test]
		public static void CompetitionXmlBaselineChangedBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<XmlBaselineChangedBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.SetupError);

			Assert.AreEqual(messages.Length, 2);

			Assert.AreEqual(
				messages[0].MessageText,
				"Target Baseline. Baseline flag on the method and in the annotation do not match.");
			Assert.AreEqual(
				messages[1].MessageText,
				"Target SlowerX20. Baseline flag on the method and in the annotation do not match.");
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

			Assert.AreEqual(messages[0].MessageText, "All competition metrics are ok.");
		}
		[Test]
		public static void CompetitionNoBaselineOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<NoBaselineOkBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.Informational);

			Assert.AreEqual(messages.Length, 2);
			Assert.AreEqual(messages[0].MessageText, "Target Benchmark1. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");
			Assert.AreEqual(messages[1].MessageText, "Target Benchmark2. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");
		}

		[Test]
		public static void CompetitionNoBaselineFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<NoBaselineFailBenchmark>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			AssertCompetitionCompleted(runState, MessageSeverity.SetupError);

			Assert.AreEqual(messages.Length, 2);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[0].MessageText,
				"Target Benchmark1. Metric validation skipped as the method is not marked with CompetitionBenchmarkAttribute.");

			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.SetupError);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[1].MessageText,
				"No baseline member found. Apply CompetitionBaselineAttribute to the one of benchmark methods.");
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
					Does.StartWith("Benchmark BadLimitsBenchmark failed. Exception: Invalid range [20.2..5.5]."));
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
			Assert.That(messages[0].MessageText, Does.StartWith("Target SlowerX10. Metric Scaled"));
			Assert.That(messages[0].MessageText, Does.Contain(" is out of limit "));

			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[1].MessageText, "Metrics check failed. Requesting 1 run(s).");

			Assert.AreEqual(messages[2].RunNumber, 2);
			Assert.AreEqual(messages[2].RunMessageNumber, 1);
			Assert.AreEqual(messages[2].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[2].MessageSource, MessageSource.Analyser);
			Assert.That(messages[2].MessageText, Does.StartWith("Target SlowerX10. Metric Scaled"));
			Assert.That(messages[2].MessageText, Does.Contain(" is out of limit "));

			Assert.AreEqual(messages[3].RunNumber, 2);
			Assert.AreEqual(messages[3].RunMessageNumber, 2);
			Assert.AreEqual(messages[3].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[3].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[3].MessageText, "Metrics check failed. Requesting 1 run(s).");

			Assert.AreEqual(messages[4].RunNumber, 3);
			Assert.AreEqual(messages[4].RunMessageNumber, 1);
			Assert.AreEqual(messages[4].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[4].MessageSource, MessageSource.Analyser);
			Assert.That(messages[4].MessageText, Does.StartWith("Target SlowerX10. Metric Scaled"));
			Assert.That(messages[4].MessageText, Does.Contain(" is out of limit "));

			Assert.AreEqual(messages[5].RunNumber, 3);
			Assert.AreEqual(messages[5].RunMessageNumber, 2);
			Assert.AreEqual(messages[5].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[5].MessageSource, MessageSource.Runner);
			Assert.AreEqual(
				messages[5].MessageText,
				"The benchmark was run 3 time(s), check log for details.");
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
			Assert.AreEqual(
				messages[0].MessageText,
				"Some benchmark metrics are empty and were ignored. Empty metrics are: SlowerX10: Scaled.");
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
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			// Limits loosed as perftest is run only four times and timings on appveyor are very inaccurate
			[CompetitionBenchmark(3, 30)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		[CompetitionXmlAnnotation(
			"CodeJam.PerfTests.Assets.CompetitionRunTests.xml",
			ResourcePath = @"..\Assets\CompetitionRunTests.xml")]
		public class XmlTaskOkBenchmark
		{
			private const int AwaitDelayMs = 50;

			[CompetitionBaseline]
			public async Task BaselineAsync()
			{
				await Task.Delay(AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
			}

			[CompetitionBenchmark(DoesNotCompete = true)]
			public async Task<int> SlowerX2Async()
			{
				await Task.Delay(2 * AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
				return _callCounter;
			}

			[Benchmark]
			public async Task<int> SlowerX3Async()
			{
				await Task.Delay(3 * AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
				return _callCounter;
			}

			[CompetitionBenchmark]
			public async Task<int> SlowerX5Async()
			{
				await Task.Delay(5 * AwaitDelayMs);
				Interlocked.Increment(ref _callCounter);
				return _callCounter;
			}
		}

		[CompetitionXmlAnnotation(
			"CodeJam.PerfTests.Assets.CompetitionRunTests.xml",
			ResourcePath = @"..\Assets\CompetitionRunTests.xml")]
		public class XmlBaselineChangedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark]
			public void SlowerX20()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		[CompetitionXmlAnnotation(
			"CodeJam.PerfTests.Assets.CompetitionRunTests.xml",
			ResourcePath = @"..\Assets\CompetitionRunTests.xml",
			UseFullTypeName = true)]
		public class XmlFullAnnotationBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark]
			public void SlowerX20()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		public class NoBaselineOkBenchmark
		{
			[Benchmark]
			public void Benchmark1()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[Benchmark]
			public void Benchmark2()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		public class NoBaselineFailBenchmark
		{
			[Benchmark]
			public void Benchmark1()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark]
			public void Benchmark2()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		public class BadLimitsBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark(20.2, 5.5)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		public class LimitsFailBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark(1, 1)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		public class LimitsEmptyFailBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}
		#endregion
	}
}