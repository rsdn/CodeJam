using System;
using System.IO;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using NUnit.Framework;

namespace CodeJam
{
	// DONTUSE: this perftest is just a prototype that proofs entire idea of ETW metrics works.
	// The metrics are very inaccurate and there is A LOT of work before the feature will go in production.

	// First, all ETW diagnosers should be merged into pair of kernel + userspace diagnosers to reduce amount of sessions to a minimum.
	//   There should be no ETW parsing in realtime to reduce side-effects. Use etl file logging?
	// Second, there should be a good diagnostic and documentation about how to run perftest elevated (kernel events will not work otherwise)
	// Third, I'm not sure about accuracy of our metrics.
	//   If we're going to keep them in-process there should be black- and whitelists for resources
	//      and this means entire idea of singleton metrics is doomed.
	//      I'm still not sure how we can keep "Only one metric of each type" restriction and should we do it at all.
	//   As an alternative, the perftest should be run outofproc for diagnosers run
	//      and _this_ means there is even more work as we have no friendly API for outofproc perfrtests
	// Heck, the whole thing is getting more and more complicated so I'm going to stop right now.

	[Ignore("Should run elevated")]
	[CompetitionNoRelativeTime]
	[CompetitionBurstMode]
	[CompetitionAnnotateSources]
	[UseIoMetricModifier]
	public class IoPerfTest
	{
		[Test]
		public void RunIoPerfTest() => Competition.Run(this);

		private string _filename;
		[GlobalSetup]
		public void Setup()
		{
			Environment.CurrentDirectory = @"C:\Users\igors\Source\Repos\CodeJam\Experimental\tests-performance\bin\Release";
			_filename = Path.GetTempFileName();
			File.WriteAllText(_filename, "ABCDE");
		}
		[GlobalCleanup]
		public void Cleanup()
		{
			File.Delete(_filename);
		}

		[CompetitionBenchmark]
		[GcAllocations(7.71, 10.06, BinarySizeUnit.Kilobyte)]
		[IoRead(8.02, 9.29, BinarySizeUnit.Kilobyte)]
		public void ReadBytes() => File.ReadAllText(_filename);
	}
}