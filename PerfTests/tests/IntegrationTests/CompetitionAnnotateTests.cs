using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionAnnotateTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = new ManualCompetitionConfig(CreateRunConfigReAnnotate())
		{
			AllowDebugBuilds = true,
			PreviousRunLogUri =
				"https://gist.githubusercontent.com/ig-sinicyn/91ac0badca95b19fc7de6f683a51b9d2/raw/38686c437887a782de48918a632001b410fc47b0/CompetitionAnnotateTests.log"
		};

		private static readonly ICompetitionConfig _localLogConfig = new ManualCompetitionConfig(_remoteLogConfig)
		{
			PreviousRunLogUri = @"Assets\CompetitionAnnotateTests.log.txt"
		};

		[Test]
		public static void TestAnnotateFromRemoteLog()
		{
			var runState = SelfTestCompetition.Run<AnnotatedBenchmark>(_remoteLogConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverity, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 4);
		}

		[Test]
		public static void TestAnnotateFromLocalLog()
		{
			// TODO: message if no XML annotation
			// TODO: exact message validation
			var runState = SelfTestCompetition.Run<AnnotatedBenchmark>(_localLogConfig);
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverity, MessageSeverity.Warning);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 4);
		}

		#region Benchmark classes
		public class AnnotatedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.DefaultCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX20() => CompetitionHelpers.Delay(20 * CompetitionHelpers.DefaultCount);
		}
		#endregion
	}
}