using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
				"https://gist.githubusercontent.com/ig-sinicyn/ceeef64a6d91f22499bc05f388bb4b48/raw/84e23d5478c941cd924076d7039eee6e63072fea/CompetitionAnnotateAnalyserTests.log.txt"
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
			Assert.AreEqual(runState.HighestMessageSeverityInRun, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 7);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 12, "Timeout failed");
		}

		[Test]
		public static void TestAnnotateFromLocalLog()
		{
			Console.WriteLine("!!! " + Environment.CurrentDirectory);
			Console.WriteLine("!!! " + TestContext.CurrentContext.TestDirectory);
			Console.WriteLine("!!! " + TestContext.CurrentContext.WorkDirectory);
			Console.WriteLine("!!! " + File.Exists(_localLogConfig.PreviousRunLogUri));
			Console.WriteLine("!!! " + 
				File.Exists(TestContext.CurrentContext.TestDirectory + "\\" + 
				_localLogConfig.PreviousRunLogUri));

			var runState = new PerfTestRunner().Run<HighAccuracyBenchmark>(_localLogConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverityInRun, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.GreaterOrEqual(messages.Length, 7);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 12, "Timeout failed");
		}

		#region Benchmark classes
		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Delay(SpinCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX20() => Delay(20 * SpinCount);
		}
		#endregion
	}
}