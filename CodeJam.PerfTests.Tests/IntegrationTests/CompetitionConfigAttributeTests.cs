using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Columns;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

namespace CodeJam.PerfTests.IntegrationTests
{
	public class AddColumnWelchTTestPValueModifier : ICompetitionModifier
	{
		public void Modify(ManualCompetitionConfig competitionConfig) =>
			competitionConfig.Add(StatisticColumn.P67);
	}

	public class AddColumnKurtosisModifier : ICompetitionModifier
	{
		public void Modify(ManualCompetitionConfig competitionConfig) =>
			competitionConfig.Add(StatisticColumn.Kurtosis);
	}

	public class AddColumnSkewnessModifier : ICompetitionModifier
	{
		public void Modify(ManualCompetitionConfig competitionConfig) =>
			competitionConfig.Add(StatisticColumn.Skewness);
	}

	[PublicAPI]
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[CompetitionModifier(typeof(AddColumnSkewnessModifier))]
	public static class CompetitionConfigAttributeTest
	{
		private static void AssertColumn(CompetitionState runState, IColumn column, int expectedValue) =>
			Assert.AreEqual(
				runState.Config
					.GetColumnProviders().SelectMany(p => p.GetColumns(runState.LastRunSummary))
					.Count(c => c == column),
				expectedValue);

		[Test]
		public static void CompetitionExplicitConfig()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BenchmarkWithoutConfig>(CompetitionHelpers.ConfigForAssembly);
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "All competition metrics are ok.");
			AssertColumn(runState, StatisticColumn.Kurtosis, 0);
			AssertColumn(runState, StatisticColumn.P67, 0);
			AssertColumn(runState, StatisticColumn.Skewness, 0);

			Interlocked.Exchange(ref _callCounter, 0);
		}

		[Test]
		public static void CompetitionTypeLevelConfig()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BenchmarkWithConfig>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "All competition metrics are ok.");
			AssertColumn(runState, StatisticColumn.P67, 1);
			AssertColumn(runState, StatisticColumn.Kurtosis, 0);
			AssertColumn(runState, StatisticColumn.Skewness, 1);

			Interlocked.Exchange(ref _callCounter, 0);

			runState = SelfTestCompetition.Run<Nested.BenchmarkWithConfig2>();
			messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "All competition metrics are ok.");
			AssertColumn(runState, StatisticColumn.P67, 1);
			AssertColumn(runState, StatisticColumn.Kurtosis, 1);
			AssertColumn(runState, StatisticColumn.Skewness, 1);

			Interlocked.Exchange(ref _callCounter, 0);
		}

		#region Perf test helpers
		// two methods in each benchmark
		private const int ExpectedRunCount = 2 * ExpectedSelfTestRunCount;

		private static int _callCounter;

		public class BenchmarkWithoutConfig
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(CompetitionRunHelpers.BurstModeLoopCount);
			}

			[CompetitionBenchmark(4, 20)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionRunHelpers.Delay(10 * CompetitionRunHelpers.BurstModeLoopCount);
			}
		}

		[CompetitionModifier(typeof(AddColumnWelchTTestPValueModifier))]
		public class BenchmarkWithConfig : BenchmarkWithoutConfig { }

		[CompetitionModifier(typeof(AddColumnKurtosisModifier))]
		public class Nested
		{
			// ReSharper disable once MemberHidesStaticFromOuterClass
			[UsedImplicitly]
			public class BenchmarkWithConfig2 : BenchmarkWithConfig { }
		}
		#endregion
	}
}