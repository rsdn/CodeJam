using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests.IntegrationTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionMetricAccuracyTests
	{
		private static long ReadFileNoCache(string path)
		{
			var FileFlagNoBuffering = (FileOptions)0x20000000;
			var b = new byte[4096];
			using (var f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileFlagNoBuffering))
			{
				while (f.CanRead)
				{
					var l = f.Read(b, 0, b.Length);
					if (l < b.Length) break;
				}
				return f.Length;
			}
		}

		[Test]
		[Ignore("Requires elevated run")]
		public static void TestIoReadMetricAccuracy()
		{
			var runState = SelfTestCompetition.Run<IoReadBenchmark>();
			var summary = runState.LastRunSummary;
			var readBytes = MetricInfo.FromAttribute<FileIoReadAttribute>().ValuesProvider.TryGetMeanValue(summary.Benchmarks[0], summary);
			Assert.Greater(readBytes, 0);
		}

		[Test]
		public static void TestClrExceptionsAccuracy()
		{
			var runState = SelfTestCompetition.Run<ThrowExceptionsBenchmark>();
			var summary = runState.LastRunSummary;
			var exceptions = MetricInfo.FromAttribute<ClrExceptionsAttribute>().ValuesProvider.TryGetMeanValue(summary.Benchmarks[0], summary);
			//Assert.True(runState.CompletedSuccessfully);
			Assert.Greater(exceptions ?? 1, 0);
		}

		#region Benchmark classes
		[PublicAPI]
		[CompetitionModifier(typeof(CompetitionHighAccuracyBurstModeModifier))]
		[CompetitionMeasureAll]
		[UseFileIoMetricModifier]
		[TraceClrExceptionsModifier]
		[CompetitionNoRelativeTime]
		[CompetitionAnnotateSources]
		public class IoReadBenchmark
		{
			private static readonly string _filename = "c:\\Sample.log";
			private string _fileFullPath = "c:\\Sample.log";

			[GlobalSetup]
			public void Setup()
			{
				var bytes = new byte[32 * 1024];
				_fileFullPath = Path.GetFullPath(_filename);
				File.WriteAllBytes(_filename, bytes);
			}

			[GlobalCleanup]
			public void Cleanup()
			{
				File.Delete(_fileFullPath);
				_fileFullPath = null;
			}

			[CompetitionBenchmark]
			[ExpectedTime(0.80, 1.19, TimeUnit.Millisecond)]
			[GcAllocations(4.80, 5.60, BinarySizeUnit.Kilobyte), Gc0(0), Gc1(0), Gc2(0)]
			[FileIoRead(36.00, BinarySizeUnit.Kilobyte), FileIoWrite(0)]
			[ClrExceptions(0)]
			public long ReadFile()
			{
				return ReadFileNoCache(_fileFullPath);
			}
		}


		[PublicAPI]
		[CompetitionModifier(typeof(CompetitionHighAccuracyBurstModeModifier))]
		[CompetitionMeasureAll]
		[TraceClrExceptionsModifier]
		[CompetitionNoRelativeTime]
		[CompetitionAnnotateSources]
		public class ThrowExceptionsBenchmark
		{

			[CompetitionBenchmark]
			[ExpectedTime(19.97, 293.40, TimeUnit.Microsecond)]
			[GcAllocations(0, 320, BinarySizeUnit.Byte), Gc0(0), Gc1(0), Gc2(0)]
			[ClrExceptions(1.00)]
			public int WithNullRefException()
			{
				try
				{
					string s = null;
					return s.Length;
				}
				catch (NullReferenceException)
				{
				}
				return 0;
			}
		}
		#endregion
	}
}