using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Messages;

using NUnit.Framework;

namespace CodeJam.BenchmarkDotNet
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionLimitsAnalyserTests
	{
		[Test]
		public static void TestCompetitionLimitsAnalyserEmptyBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<EmptyBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, 0);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "Analyser CompetitionLimitsAnnotateAnalyser: no warnings.");
		}

		[Test]
		public static void TestCompetitionLimitsAnalyserNoBaselineOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<NoBaselineOkBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "Analyser CompetitionLimitsAnnotateAnalyser: no warnings.");
		}

		[Test]
		public static void TestCompetitionLimitsAnalyserOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<OkBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "Analyser CompetitionLimitsAnnotateAnalyser: no warnings.");
		}

		[Test]
		public static void TestCompetitionLimitsAnalyserXmlOkBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<XmlOkBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "Analyser CompetitionLimitsAnnotateAnalyser: no warnings.");
		}

		[Test]
		public static void TestCompetitionLimitsAnalyserHighAccuracyBenchmark()
		{
			var config = new ManualCompetitionConfig(FastRunConfig.Instance)
			{
				DebugMode = true,
				EnableReruns = true
			};

			var stopwatch = Stopwatch.StartNew();
			var summary = CompetitionBenchmarkRunner.Run<HighAccuracyBenchmark>(config);
			stopwatch.Stop();
			var runState = CompetitionCore.RunState[summary];
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 3);
		}

		[Test]
		public static void TestCompetitionLimitsAnalyserNoBaselineFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<NoBaselineFailBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
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
		public static void TestCompetitionLimitsAnalyserFailBenchmark()
		{
			Interlocked.Exchange(ref _callCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<CompetitionLimitsFailBenchmark>(_config);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(_callCounter, 3 * ExpectedRunCount); // 3x rerun
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
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
			Assert.AreEqual(messages[1].MessageSource, MessageSource.BenchmarkRunner);
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
			Assert.AreEqual(messages[3].MessageSource, MessageSource.BenchmarkRunner);
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
			Assert.AreEqual(messages[5].MessageSource, MessageSource.BenchmarkRunner);
			Assert.AreEqual(
				messages[5].MessageText,
				"The benchmark was run 3 times (read log for details). Consider to adjust competition setup.");
		}

		#region Benchmark classes
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * PerfTestConfig.ExpectedRunCountNoWarmup;
		private static readonly ICompetitionConfig _config = PerfTestConfig.NoWarmup;

		private const int SpinCount = 100 * 1000;
		private static int _callCounter;

		public class EmptyBenchmark { }

		public class NoBaselineOkBenchmark : EmptyBenchmark
		{
			[Benchmark]
			public void WillRun() => Interlocked.Increment(ref _callCounter);

			[CompetitionBenchmark(DoesNotCompete = true)]
			public void WillRun2() => Interlocked.Increment(ref _callCounter);
		}

		public class OkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				Thread.SpinWait(SpinCount);
			}

			[CompetitionBenchmark(6, 14)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				Thread.SpinWait(10 * SpinCount);
			}
		}

		[CompetitionMetadata("CodeJam.BenchmarkDotNet._L4_CompetitionLimits_.CompetitionLimitsAnalyserTests.xml")]
		public class XmlOkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				Thread.SpinWait(SpinCount);
			}

			[CompetitionBenchmark]
			public void SlowerX20()
			{
				Interlocked.Increment(ref _callCounter);
				Thread.SpinWait(20 * SpinCount);
			}
		}

		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Thread.SpinWait(SpinCount);

			[CompetitionBenchmark(8.5, 11.5)]
			public void SlowerX10() => Thread.SpinWait(10 * SpinCount);
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
				Thread.SpinWait(SpinCount);
			}

			[CompetitionBenchmark(1, 1)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				Thread.SpinWait(10 * SpinCount);
			}
		}
		#endregion
	}
}