using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;

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
				competitionConfig.ApplyModifier(
					new CompetitionOptions()
					{
						Adjustments =
						{
							AdjustMetrics = false
						},
						Annotations =
						{
							IgnoreExistingAnnotations = true,
							PreviousRunLogUri = _previousRunLogUri
						}
					});
		}

		public CompetitionRerannotateFromLogModifier(string previousRunLogUri)
			: base(() => new ModifierImpl(previousRunLogUri)) { }
	}

	public sealed class CompetitionMeasurementsFromLogModifier : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(WellKnownMetrics.ExpectedTime);
				competitionConfig.Metrics.Add(WellKnownMetrics.GcAllocations);
			}
		}

		public CompetitionMeasurementsFromLogModifier() : base(() => new ModifierImpl()) { }
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
		public static void TestAnnotateFromLocalLogTimeAndGc()
		{
			var runState = SelfTestCompetition.Run<AnnotatedWithTimeAndGcBenchmark>();
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
		public static void TestAnnotateFromLocalLogTimeAndGcNoRelative()
		{
			var runState = SelfTestCompetition.Run<AnnotatedWithTimeAndGcNoRelativeBenchmark>();
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

		#region BenchmarkCase classes
		public class AnnotatedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);

			[CompetitionBenchmark(10.00, 30.00)]
			public void SlowerX20() => CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
		}

		public class AnnotatedBaselineChangedBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);

			[CompetitionBenchmark]
			public void SlowerX20() => CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
		}

		[CompetitionMeasurementsFromLogModifier]
		public class AnnotatedWithTimeAndGcBenchmark
		{
			[CompetitionBaseline]
			[GcAllocations(0.00, 6.00, BinarySizeUnit.Kilobyte)]
			[ExpectedTime(0.00, 10.00, TimeUnit.Second)]
			public void Baseline() => CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);

			[CompetitionBenchmark(10.00, 30.00)]
			[GcAllocations(0.00, 3.00, BinarySizeUnit.Gigabyte)]
			[ExpectedTime(0.00, 10.00, TimeUnit.Second)]
			public void SlowerX20() => CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
		}

		[CompetitionMeasurementsFromLogModifier, CompetitionNoRelativeTime]
		public class AnnotatedWithTimeAndGcNoRelativeBenchmark
		{
			[CompetitionBenchmark]
			[ExpectedTime(0.00, 5.00, TimeUnit.Second)]
			[GcAllocations(0.00, 5.00, BinarySizeUnit.Kilobyte)]
			public void BaseX1() => CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);

			[CompetitionBenchmark]
			[ExpectedTime(0.00, 5.00, TimeUnit.Second)]
			[GcAllocations(0.00, 1.00, BinarySizeUnit.Gigabyte)]
			public void SlowerX20() => CompetitionRunHelpers.Delay(20 * CompetitionRunHelpers.BurstModeLoopCount);
		}
		#endregion
	}
}