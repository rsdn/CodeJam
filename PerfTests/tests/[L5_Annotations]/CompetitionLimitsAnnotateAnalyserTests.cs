using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

using static CodeJam.PerfTests.PerfTestConfig;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionLimitsAnnotateAnalyserTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = new ManualCompetitionConfig(FastRunConfig.Instance)
		{
			UpdateSourceAnnotations = true,
			DebugMode = true,
			EnableReruns = true,
			IgnoreExistingAnnotations = true,
			LogAnnotationResults = true,
			PreviousLogUri =
				"https://gist.github.com/ig-sinicyn/ceeef64a6d91f22499bc05f388bb4b48/raw/97e4465db14c1f90b4d81181044a6a5ad3b816f5/CompetitionLimitsAnnotateAnalyserTests.log.txt"
		};

		private static readonly ICompetitionConfig _localLogConfig = new ManualCompetitionConfig(_remoteLogConfig)
		{
			PreviousLogUri = @"[L5_Annotations]\CompetitionLimitsAnnotateAnalyserTests.log.txt"
		};

		[Test]
		public static void TestAnnotateFromRemoteLog()
		{
			var stopwatch = Stopwatch.StartNew();
			var summary = CompetitionBenchmarkRunner.Run<HighAccuracyBenchmark>(_remoteLogConfig);
			stopwatch.Stop();
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.IsTrue(runState.HighestMessageSeverityInRun <= MessageSeverity.Warning);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 5);
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 7);
		}

		[Test]
		[Explicit("Temporary disabled, fails on buildserver")]
		// TODO: troubleshoot using https://www.appveyor.com/docs/how-to/rdp-to-build-worker
		public static void TestAnnotateFromLocalLog()
		{
			var stopwatch = Stopwatch.StartNew();
			var summary = CompetitionBenchmarkRunner.Run<HighAccuracyBenchmark>(_localLogConfig);
			stopwatch.Stop();
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.IsTrue(runState.HighestMessageSeverityInRun <= MessageSeverity.Warning);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 5);
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 7);
		}

		#region Benchmark classes
		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Thread.SpinWait(SpinCount);

			[CompetitionBenchmark(9.70, 30.90)]
			public void SlowerX10() => Thread.SpinWait(20 * SpinCount);
		}
		#endregion
	}
}