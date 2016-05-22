using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Competitions;

using NUnit.Framework;

namespace CodeJam.BenchmarkDotNet
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CompetitionLimitsAnalyserTest
	{
		#region TestInProcess
		private static int _testCount;

		[Test]
		public static void TestCompetitionLimitsAnalyser()
		{
			var config = PerfTestConfig.NoWarmup;
			var expectedRunCount = 2 * PerfTestConfig.ExpectedRunCountNoWarmup;

			Interlocked.Exchange(ref _testCount, 0);
			var summary = CompetitionBenchmarkRunner.Run<EmptyBenchmark>(config);
			Assert.AreEqual(_testCount, 0);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);

			Interlocked.Exchange(ref _testCount, 0);
			summary = CompetitionBenchmarkRunner.Run<NoBaselineOkBenchmark>(config);
			Assert.AreEqual(_testCount, expectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);

			// TODO: check analyser warnings
			Interlocked.Exchange(ref _testCount, 0);
			summary = CompetitionBenchmarkRunner.Run<NoBaselineFailBenchmark>(config);
			Assert.AreEqual(_testCount, expectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);

			Interlocked.Exchange(ref _testCount, 0);
			summary = CompetitionBenchmarkRunner.Run<CompetitionLimitsOkBenchmark>(config);
			Assert.AreEqual(_testCount, expectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);

			Interlocked.Exchange(ref _testCount, 0);
			summary = CompetitionBenchmarkRunner.Run<CompetitionLimitsXmlOkBenchmark>(config);
			Assert.AreEqual(_testCount, expectedRunCount);
			Assert.AreEqual(summary.ValidationErrors.Length, 0);

			Interlocked.Exchange(ref _testCount, 0);
			summary = CompetitionBenchmarkRunner.Run<CompetitionLimitsFailBenchmark>(config);
			Assert.AreEqual(_testCount, 3 * expectedRunCount); // 3x rerun
			Assert.AreEqual(summary.ValidationErrors.Length, 0);
		}

		public class EmptyBenchmark
		{
		}

		public class NoBaselineOkBenchmark : EmptyBenchmark
		{
			[Benchmark]
			public void WillRun() => Interlocked.Increment(ref _testCount);

			[CompetitionBenchmark(DoesNotCompete = true)]
			public void WillRun2() => Interlocked.Increment(ref _testCount);
		}

		public class NoBaselineFailBenchmark : EmptyBenchmark
		{
			[Benchmark]
			public void WillNotRun() => Interlocked.Increment(ref _testCount);

			[CompetitionBenchmark]
			public void WillNotRun2() => Interlocked.Increment(ref _testCount);
		}

		public class CompetitionLimitsOkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(30);
			}

			[CompetitionBenchmark(9, 11)]
			public  void WithinLimits()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(300);
			}
		}

		[CompetitionMetadata("CodeJam.BenchmarkDotNet._L4_CompetitionLimits_.CompetitionLimitsAnalyserTest.xml")]
		public class CompetitionLimitsXmlOkBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(30);
			}

			[CompetitionBenchmark]
			public void WithinLimits()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(600);
			}
		}

		public class CompetitionLimitsFailBenchmark : EmptyBenchmark
		{
			[CompetitionBaseline]
			public void Baseline()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(30);
			}

			[CompetitionBenchmark(1, 1)]
			public void OutOfLimits()
			{
				Interlocked.Increment(ref _testCount);
				Thread.Sleep(300);
			}
		}
		#endregion
	}
}