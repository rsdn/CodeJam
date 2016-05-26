using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Attributes;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class InProcessToolchainTest
	{
		#region TestInProcess
		private static int _callCounter;
		private static int _afterSetupCounter;

		[Test]
		public static void TestInProcess()
		{
			var config = PerfTestConfig.Default;

			Interlocked.Exchange(ref _callCounter, 0);
			Interlocked.Exchange(ref _afterSetupCounter, 0);
			var summary = CompetitionBenchmarkRunner.Run<InProcessBenchmark>(config);
			Assert.AreEqual(_callCounter, PerfTestConfig.ExpectedRunCount);
			Assert.AreEqual(_afterSetupCounter, 1);

			Assert.IsFalse(summary.ValidationErrors.Any());
		}

		public class InProcessBenchmark
		{
			[Setup]
			public void Setup() => Interlocked.Exchange(ref _afterSetupCounter, 0);

			[Benchmark]
			public void InvokeOnce()
			{
				Interlocked.Increment(ref _callCounter);
				Interlocked.Increment(ref _afterSetupCounter);
				Thread.Sleep(1);
			}
		}
		#endregion

		#region TestInProcessWithValidation
		[Test]
		public static void TestInProcessWithValidation()
		{
			// DONTTOUCH: config SHOULD NOT match the platform.
			var config = IntPtr.Size == 8
				? PerfTestConfig.X86
				: PerfTestConfig.X64;

			var summary = CompetitionBenchmarkRunner.Run<InProcessWithValidationBenchmark>(config);
			Assert.AreEqual(summary.ValidationErrors.Length, 1);
			Assert.IsFalse(summary.ValidationErrors[0].IsCritical);
			Assert.That(summary.ValidationErrors[0].Message, Does.Contain(", property Platform:"));
		}

		public class InProcessWithValidationBenchmark
		{
			[Benchmark]
			public void InvokeOnce() => Thread.Sleep(1);
		}
		#endregion
	}
}