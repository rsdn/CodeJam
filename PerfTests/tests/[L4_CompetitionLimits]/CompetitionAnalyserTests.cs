using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

using static CodeJam.PerfTests.PerfTestHelpers;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionAnalyserTests
	{
		[Test]
		public static void TestCompetitionAnalyserEmptyBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<EmptyBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void TestCompetitionAnalyserNoBaselineOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<NoBaselineOkBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void TestCompetitionAnalyserOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<OkBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void TestCompetitionAnalyserXmlOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<XmlOkBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnnotateAnalyser: All competition limits are ok.");
		}

		[Test]
		public static void TestCompetitionAnalyserNoBaselineFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<NoBaselineFailBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.SetupError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(messages[0].MessageText, "The competition has no baseline");
		}

		[Test]
		public static void TestCompetitionAnalyserFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var runState = new PerfTestRunner().Run<CompetitionLimitsFailBenchmark>(SingleRunConfig);
			var messages = runState.GetMessages();
			var summary = runState.LastRunSummary;
			Assert.AreEqual(_callCounter, 3 * ExpectedRunCount); // 3x rerun
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 3);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 6);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.That(
				messages[0].MessageText, Does.StartWith("Method SlowerX10 runs slower than 1.00x baseline. Actual ratio: "));

			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[1].MessageText, "Requesting 1 run(s): Competition validation failed.");

			Assert.AreEqual(messages[2].RunNumber, 2);
			Assert.AreEqual(messages[2].RunMessageNumber, 1);
			Assert.AreEqual(messages[2].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[2].MessageSource, MessageSource.Analyser);
			Assert.That(
				messages[2].MessageText, Does.StartWith("Method SlowerX10 runs slower than 1.00x baseline. Actual ratio: "));

			Assert.AreEqual(messages[3].RunNumber, 2);
			Assert.AreEqual(messages[3].RunMessageNumber, 2);
			Assert.AreEqual(messages[3].MessageSeverity, MessageSeverity.Informational);
			Assert.AreEqual(messages[3].MessageSource, MessageSource.Runner);
			Assert.AreEqual(messages[3].MessageText, "Requesting 1 run(s): Competition validation failed.");

			Assert.AreEqual(messages[4].RunNumber, 3);
			Assert.AreEqual(messages[4].RunMessageNumber, 1);
			Assert.AreEqual(messages[4].MessageSeverity, MessageSeverity.TestError);
			Assert.AreEqual(messages[4].MessageSource, MessageSource.Analyser);
			Assert.That(
				messages[4].MessageText, Does.StartWith("Method SlowerX10 runs slower than 1.00x baseline. Actual ratio: "));

			Assert.AreEqual(messages[5].RunNumber, 3);
			Assert.AreEqual(messages[5].RunMessageNumber, 2);
			Assert.AreEqual(messages[5].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[5].MessageSource, MessageSource.Runner);
			Assert.AreEqual(
				messages[5].MessageText,
				"The benchmark was run 3 times (read log for details). Consider to adjust competition setup.");
		}

		#region Benchmark classes
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSingleRunCount;

		private static int _callCounter;

		public class EmptyBenchmark { }

		public class NoBaselineOkBenchmark : EmptyBenchmark
		{
			[Benchmark]
			public void WillRun()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(SpinCount);
			}

			[CompetitionBenchmark(DoesNotCompete = true)]
			public void WillRun2()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(SpinCount);
			}
		}

		public class OkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(SpinCount);
			}

			[CompetitionBenchmark(5, 15)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(10 * SpinCount);
			}
		}

		[CompetitionMetadata("CodeJam.PerfTests._L4_CompetitionLimits_.CompetitionAnalyserTests.xml")]
		public class XmlOkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(SpinCount);
			}

			[CompetitionBenchmark]
			public void SlowerX20()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(20 * SpinCount);
			}
		}

		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Delay(SpinCount);

			[CompetitionBenchmark(9.5, 10.5)]
			public void SlowerX10() => Delay(10 * SpinCount);
		}

		public class NoBaselineFailBenchmark : EmptyBenchmark
		{
			[Benchmark]
			public void WillNotRun() => Interlocked.Increment(ref _callCounter);

			[CompetitionBenchmark]
			public void WillNotRun2() => Interlocked.Increment(ref _callCounter);
		}

		public class CompetitionLimitsFailBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(SpinCount);
			}

			[CompetitionBenchmark(1, 1)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				Delay(10 * SpinCount);
			}
		}
		#endregion
	}
}