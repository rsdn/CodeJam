using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;

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
	public static class CompetitionAnalyserAccuracyTests
	{
		private static readonly ICompetitionConfig _accurateConfig = new ManualCompetitionConfig(FastRunConfig.Instance)
		{
			DebugMode = true,
			EnableReruns = true
		};

		// TODO: the test takes too long time to complete. Speedup if possible.
		[Test]
		public static void TestCompetitionAnalyserTooFastTooSlowBenchmark()
		{
			var summary = CompetitionBenchmarkRunner.Run<TooFastTooSlowBenchmark>(_accurateConfig);
			var runState = CompetitionCore.RunState[summary];
			var messages = runState.GetMessages();
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.AreEqual(messages.Length, 2);

			Assert.AreEqual(messages[0].RunNumber, 1);
			Assert.AreEqual(messages[0].RunMessageNumber, 1);
			Assert.AreEqual(messages[0].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[0].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[0].MessageText,
				"The benchmarks TooFast, TooFast2 run faster than 400 nanoseconds. Results cannot be trusted.");

			Assert.AreEqual(messages[1].RunNumber, 1);
			Assert.AreEqual(messages[1].RunMessageNumber, 2);
			Assert.AreEqual(messages[1].MessageSeverity, MessageSeverity.Warning);
			Assert.AreEqual(messages[1].MessageSource, MessageSource.Analyser);
			Assert.AreEqual(
				messages[1].MessageText,
				"The benchmarks TooSlow run longer than 0.5 sec." +
					" Consider to rewrite the test as the peek timings will be hidden by averages" +
					" or set the AllowSlowBenchmarks to true.");
		}

		[Test]
		public static void TestCompetitionAnalyserHighAccuracyBenchmark()
		{
			var stopwatch = Stopwatch.StartNew();
			var summary = CompetitionBenchmarkRunner.Run<HighAccuracyBenchmark>(_accurateConfig);
			stopwatch.Stop();
			var runState = CompetitionCore.RunState[summary];
			Assert.AreEqual(runState.RunNumber, 1);
			Assert.AreEqual(runState.RunsLeft, 0);
			Assert.AreEqual(runState.RunLimitExceeded, false);
			Assert.AreEqual(runState.LooksLikeLastRun, true);
			Assert.LessOrEqual(stopwatch.Elapsed.TotalSeconds, 8);
		}

		#region Benchmark classes
		public class TooFastTooSlowBenchmark
		{
			[Benchmark]
			public int TooFast()
			{
				var a = 0;
				for (var i = 0; i < 10; i++)
				{
					a = a + i;
				}
				return a;
			}

			[Benchmark]
			public int TooFast2()
			{
				var a = 0;
				for (var i = 0; i < 100; i++)
				{
					a = a + i;
				}
				return a;
			}

			[CompetitionBenchmark(DoesNotCompete = true)]
			public void TooSlow() => Thread.Sleep(550);
		}

		public class HighAccuracyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline() => Thread.SpinWait(SpinCount);

			[CompetitionBenchmark(8.2, 11.8)]
			public void SlowerX10() => Thread.SpinWait(10 * SpinCount);
		}
		#endregion
	}
}