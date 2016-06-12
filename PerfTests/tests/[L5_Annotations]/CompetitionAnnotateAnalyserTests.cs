using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

using static CodeJam.PerfTests.PerfTestHelpers;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionAnnotateAnalyserTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = new ManualCompetitionConfig(HighAccuracyConfig)
		{
			AllowDebugBuilds = true,
			DetailedLogging = true,
			RerunIfLimitsFailed = true,
			LogCompetitionLimits = true,
			UpdateSourceAnnotations = true,
			IgnoreExistingAnnotations = true,
			PreviousRunLogUri =
				"https://gist.githubusercontent.com/ig-sinicyn/ceeef64a6d91f22499bc05f388bb4b48/raw/74012e12059a096c76db5d2241ee080dd4221243/CompetitionAnnotateAnalyserTests.log.txt"
		};

		private static readonly ICompetitionConfig _localLogConfig = new ManualCompetitionConfig(_remoteLogConfig)
		{
			PreviousRunLogUri = @"[L5_Annotations]\CompetitionAnnotateAnalyserTests.log.txt"
		};

		[Test]
		public static void TestAnnotateFromRemoteLog()
		{
			var runState = new PerfTestRunner().Run<HighAccuracyBenchmark>(_remoteLogConfig);
			var messages = runState.GetMessages();
			Assert.IsTrue(runState.HighestMessageSeverityInRun <= MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 5);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 10, "Timeout failed");
		}

		[Test]
		[Explicit("Temporary disabled, fails on buildserver")]
		// TODO: troubleshoot using https://www.appveyor.com/docs/how-to/rdp-to-build-worker
		public static void TestAnnotateFromLocalLog()
		{
			var runState = new PerfTestRunner().Run<HighAccuracyBenchmark>(_localLogConfig);
			var messages = runState.GetMessages();
			Assert.IsTrue(runState.HighestMessageSeverityInRun <= MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 5);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 7, "Timeout failed");
		}

		#region Benchmark classes
		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Delay(SpinCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX10() => Delay(20 * SpinCount);
		}
		#endregion
	}
}