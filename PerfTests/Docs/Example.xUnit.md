## TL;DR (xUnit version):

> ***~WARNING~***
>
> The xUnit integration is very-alpha version and has some known issues ([xunit/#908](https://github.com/xunit/xunit/issues/908) as example) we're going to fix sooner or later.

1. Create a new unit test project (*~Set targeting to .net 4.6+, previous FW versions are not supported for now~*).
2. Add a reference to the [CodeJam.PerfTests.xUnit](https://www.nuget.org/packages/CodeJam.PerfTests.xUnit) nuget package.
3. Add a file with the following code:
 ```c#
using System;
using System.Threading;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeJam.Examples
{
	// A perf test class.
	[TestClass]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[TestMethod]
		public void RunSimplePerfTest() => Competition.Run(
			this, CompetitionConfig.AnnotateSources);

		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}
 ```

4. Switch to **Release** configuration and run the `RunSimplePerfTest` test. You should get something like this (look at `[CompetitionBenchmark]` parameters):
 ```c#
		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.93, 3.05)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.89, 5.14)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.82, 7.21)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
 ```
 yep, it's a magic:)

 > ***~OOPS!~***
 >
 > We have an issue with perftest being run on low-end notebooks / nettops with mobile CPUs. Current implementation provides inaccurate competition limits occasionally (too high / too low timing values). We're going to fix it in near future. Sorry!

5. After `[CompetitionBenchmark]` attributes are filled with timing limits, you can disable source auto-annotation. To do this,  use the `CompetitionConfig.Default`:
 ```c#
		[Test]
		public void RunSimplePerfTest() => Competition.Run(
			this, CompetitionConfig.Default);
 ```
6. Now the test will fail if timings do not fit into limits. To proof, change implementation for any competiton method and run the test. As example:
 ```c#
		[CompetitionBenchmark(6.82, 7.21)]
		public void SlowerX7() => Thread.SpinWait(10 * Count); // 10x slower
 ```
 The test should fail with text like this:
 ```
Test failed, details below.
Failed assertions:
    * Run #3: Method SlowerX7 [9.96..9.96] does not fit into limits [6.82..7.21]
Warnings:
    * Run #3: The benchmark was run 3 time(s) (read log for details). Try to loose competition limits.
Diagnostic messages:
    * Run #1: Requesting 1 run(s): Limit checking failed.
    * Run #2: Requesting 1 run(s): Limit checking failed.
 ```

7. Well, that's all.
