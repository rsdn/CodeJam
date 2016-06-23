using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

using static CodeJam.PerfTests.IntegrationTests.PerfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionAnnotateTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = new ManualCompetitionConfig(DefaultRunConfig)
		{
			AllowDebugBuilds = true,
			DetailedLogging = true,
			RerunIfLimitsFailed = true,
			LogCompetitionLimits = true,
			UpdateSourceAnnotations = true,
			IgnoreExistingAnnotations = true,
			PreviousRunLogUri =
				"https://gist.githubusercontent.com/ig-sinicyn/91ac0badca95b19fc7de6f683a51b9d2/raw/fd760290009a92c372ae872de6b3906e4ef5d0ec/CompetitionAnnotateTests.log"
		};

		private static readonly ICompetitionConfig _localLogConfig = new ManualCompetitionConfig(_remoteLogConfig)
		{
			PreviousRunLogUri = @"Resources\CompetitionAnnotateTests.log.txt"
		};

		[Test]
		public static void TestAnnotateFromRemoteLog()
		{
			var runState = new PerfTestRunner().Run<AnnotatedBenchmark>(_remoteLogConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverityInRun, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 4);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 20, "Timeout failed");
		}

		[Test]
		public static void TestAnnotateFromLocalLog()
		{
			// TODO: message if no XML annotation
			// TODO: exact message validation
			var runState = new PerfTestRunner().Run<AnnotatedBenchmark>(_localLogConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverityInRun, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 4);
			Assert.LessOrEqual(runState.Elapsed.TotalSeconds, 20, "Timeout failed");
		}

		#region Benchmark classes
		public class AnnotatedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Delay(SpinCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX20() => Delay(20 * SpinCount);
		}
		#endregion
	}
}