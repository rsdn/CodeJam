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
	public static class CompetitionAnnotateAnalyserTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = new ManualCompetitionConfig(FastRunConfig.Instance)
		{
			UpdateSourceAnnotations = true,
			DebugMode = true,
			EnableReruns = true,
			IgnoreExistingAnnotations = true,
			LogAnnotationResults = true,
			PreviousLogUri =
				"https://gist.githubusercontent.com/ig-sinicyn/ceeef64a6d91f22499bc05f388bb4b48/raw/74012e12059a096c76db5d2241ee080dd4221243/CompetitionAnnotateAnalyserTests.log.txt"
		};

		private static readonly ICompetitionConfig _localLogConfig = new ManualCompetitionConfig(_remoteLogConfig)
		{
			PreviousLogUri = @"[L5_Annotations]\CompetitionAnnotateAnalyserTests.log.txt"
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
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 12, "Timeout failed");
		}

		#region Benchmark classes
		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Thread.SpinWait(SpinCount);

			[CompetitionBenchmark(9.50, 31.50)]
			public void SlowerX10() => Thread.SpinWait(20 * SpinCount);
		}
		#endregion
	}
}