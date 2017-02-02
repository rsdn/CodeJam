using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Messages;

using NUnit.Framework;

namespace CodeJam.PerfTests.IntegrationTests
{
	public sealed class CompetitionRerannotateFromLogModifier : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			private readonly string _previousRunLogUri;

			public ModifierImpl(string previousRunLogUri)
			{
				_previousRunLogUri = previousRunLogUri;
			}

			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.ApplyModifier(new CompetitionOptions()
				{
					Adjustments =
					{
						AdjustLimits = true
					},
					Annotations =
					{
						IgnoreExistingAnnotations = true,
						PreviousRunLogUri = _previousRunLogUri
					}
				});
		}

		public CompetitionRerannotateFromLogModifier(string previousRunLogUri) : base(() => new ModifierImpl(previousRunLogUri)) { }
	}

	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[CompetitionRerannotateFromLogModifier(@"Assets\CompetitionAnnotateTests.log.txt")]
	public static class CompetitionAnnotateTests
	{
		private static readonly ICompetitionConfig _remoteLogConfig = CompetitionHelpers
			.CreateConfig(typeof(AnnotatedBenchmark))
			.WithPreviousRunLogUri(
				"https://gist.githubusercontent.com/ig-sinicyn/91ac0badca95b19fc7de6f683a51b9d2/raw/1995d15f0a433324c6c61af195bdf68f4c2833b3/CompetitionAnnotateTests.log");

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
			var runState = SelfTestCompetition.Run<AnnotatedBenchmark>();
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
		public static void TestAnnotateFromLocalLogTime()
		{
			var runState = SelfTestCompetition.Run<AnnotatedWithTimeBenchmark>();
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
		public static void TestAnnotateFromLocalLogTimeOnly()
		{
			var runState = SelfTestCompetition.Run<AnnotatedWithTimeOnlyBenchmark>();
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
		public static void TestAnnotateBaselineChangedFromLocalLog()
		{
			var runState = SelfTestCompetition.Run<AnnotatedBaselineChangedBenchmark>();
			var messages = runState.GetMessages();
			Assert.AreEqual(runState.HighestMessageSeverity, MessageSeverity.SetupError);
			Assert.IsTrue(runState.Completed);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 3);
		}

		#region Benchmark classes
		public class AnnotatedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX20() => CompetitionHelpers.Delay(20 * CompetitionHelpers.RecommendedSpinCount);
		}

		public class AnnotatedBaselineChangedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);

			[CompetitionBenchmark]
			public void SlowerX20() => CompetitionHelpers.Delay(20 * CompetitionHelpers.RecommendedSpinCount);
		}

		[CompetitionMeasureTime]
		public class AnnotatedWithTimeBenchmark
		{
			[CompetitionBaseline]
			[ExpectedTime(0.00, 10.00, TimeUnit.Second)]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);

			[CompetitionBenchmark(10.00, 30.00)]
			[ExpectedTime(0.00, 10.00, TimeUnit.Second)]
			public void SlowerX20() => CompetitionHelpers.Delay(20 * CompetitionHelpers.RecommendedSpinCount);
		}

		[CompetitionMeasureTime, CompetitionNoRelativeTime]
		public class AnnotatedWithTimeOnlyBenchmark
		{
			[CompetitionBaseline]
			[ExpectedTime(0.00, 5.00, TimeUnit.Second)]
			public void Baseline() => CompetitionHelpers.Delay(CompetitionHelpers.RecommendedSpinCount);

			[CompetitionBenchmark]
			[ExpectedTime(0.00, 5.00, TimeUnit.Second)]
			public void SlowerX20() => CompetitionHelpers.Delay(20 * CompetitionHelpers.RecommendedSpinCount);
		}
		#endregion
	}
}