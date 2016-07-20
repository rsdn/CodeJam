using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.IntegrationTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.SelfTestHelpers;

[assembly: SelfTestCompetitionConfig]

namespace CodeJam.PerfTests.IntegrationTests
{
	public class CustomCompetitionConfigAttribute : CompetitionConfigAttribute
	{
		public CustomCompetitionConfigAttribute() : base(Create) { }

		public static ManualCompetitionConfig Create()
		{
			var result = CreateSelfTestConfig(Platform.Host);
			result.Add(BaselineDiffColumn.Scaled95);
			return result;
		}
	}

	public class SelfTestCompetitionConfigAttribute : CompetitionConfigAttribute
	{
		public SelfTestCompetitionConfigAttribute() : base(Create) { }

		private static ManualCompetitionConfig Create()
		{
			var result = CreateSelfTestConfig(Platform.Host);
			result.Add(BaselineDiffColumn.Delta);
			return result;
		}
	}

	[PublicAPI]
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionConfigAttributeTest
	{
		[Test]
		public static void CompetitionExplicitConfig()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BenchmarkWithoutConfig>(SelfTestConfig);
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Delta), 0);
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Scaled95), 0);

			Interlocked.Exchange(ref _callCounter, 0);

			runState = SelfTestCompetition.Run<BenchmarkWithoutConfig>(CustomCompetitionConfigAttribute.Create());
			messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Delta), 0);
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Scaled95), 1);
		}

		[Test]
		public static void CompetitionTypeLevelConfig()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BenchmarkWithConfig>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Delta), 0);
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Scaled95), 1);

			Interlocked.Exchange(ref _callCounter, 0);

			runState = SelfTestCompetition.Run<Nested.BenchmarkWithConfig>();
			messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Delta), 0);
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Scaled95), 1);
		}

		[Test]
		public static void CompetitionAssemblyLevelConfig()
		{
			Interlocked.Exchange(ref _callCounter, 0);

			var runState = SelfTestCompetition.Run<BenchmarkWithoutConfig>();
			var messages = runState.GetMessages();

			Assert.AreEqual(_callCounter, ExpectedRunCount);
			Assert.AreEqual(messages.Length, 1);
			Assert.AreEqual(messages[0].MessageText, "CompetitionAnalyser: All competition limits are ok.");
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Delta), 1);
			Assert.AreEqual(runState.Config.GetColumns().Count(c => c == BaselineDiffColumn.Scaled95), 0);
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
				CompetitionHelpers.Delay(CompetitionHelpers.DefaultCount);
			}

			[CompetitionBenchmark(5, 20)]
			public void SlowerX10()
			{
				Interlocked.Increment(ref _callCounter);
				CompetitionHelpers.Delay(10 * CompetitionHelpers.DefaultCount);
			}
		}

		[CustomCompetitionConfig]
		public class BenchmarkWithConfig : BenchmarkWithoutConfig { }

		[CustomCompetitionConfig]
		public class Nested
		{
			// ReSharper disable once MemberHidesStaticFromOuterClass
			[UsedImplicitly]
			public class BenchmarkWithConfig : BenchmarkWithoutConfig { }
		}
		#endregion
	}
}