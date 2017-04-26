## TL;DR (MS Test version)

1. Create a new MS unit test project (*--Set targeting to .net 4.6+, previous FW versions are not supported for now--*).
2. Add a reference to the [CodeJam.PerfTests.MSTest](https://www.nuget.org/packages/CodeJam.PerfTests.MSTest) nuget package.
3. Add a file with the following code:
```c#
using System;
using System.Threading;

using CodeJam.PerfTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeJam.Examples.PerfTests
{
	// A perf test class.
	[TestClass]
	[CompetitionAnnotateSources] // Opt-in feature: source annotations.
	public class SimplePerfTest
	{
		private const int Count = 200;

		// Perf test runner method.
		[TestMethod]
		public void RunSimplePerfTest() => Competition.Run(this);

		// Baseline competition member.
		// All relative metrics will be compared with metrics of the baseline method.
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

4. Switch to **Release** configuration and run the `RunSimplePerfTest` test. You should get something like this (look at attribute parameters):
```c#
		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		[GcAllocations(0)]                 // does not allocate
		public void Baseline() => Thread.SpinWait(Count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.96, 3.08)] // ~3x to baseline
		[GcAllocations(0)]                 // does not allocate
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.94, 5.14)] // ~5x to baseline
		[GcAllocations(0)]                 // does not allocate
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.89, 7.17)] // ~7x to baseline
		[GcAllocations(0)]                 // does not allocate
		public void SlowerX7() => Thread.SpinWait(7 * Count);
```
yep, it's magic:)

 > **NOTE**
 >
 > This test is known to provide inaccurate results on notebooks / nettops with mobile CPUs due to aggressive frequency scaling and throttling. There're two workarounds:
 >
 > First, you can set loop count ( `Count`) to the `CompetitionRunHelpers.SmallLoopCount`. That constant (currently it is equal to 256, was found empirically) provides most accurate results for small loops on different hardware 
 >
 > As a second option, you can set loop count ( `Count`) to the `CompetitionRunHelpers.BurstModeLoopCount` that should be used for large loops (10k or so). Large methods (single run takes 1 ms or more) should be used together with `[CompetitionBurstMode]` attribute applied to the competition class. The attribute adjusts competition options to improve running time for long-executing competition members.

5. After competition members are annotated with actual metrics you can disable source auto-annotation. To do this, just remove the `[CompetitionAnnotateSources]` attribute.

6. Now the test will fail if metrics do not fit into limits. To proof, change implementation for any competition method and run the test. As example:
```c#
		[CompetitionBenchmark(6.89, 7.17)]
		[GcAllocations(10, BinarySizeUnit.Gigabyte)]
		public void SlowerX7() => Thread.SpinWait(10 * Count); // 10x slower
```
 The test should fail with text like this:
 ```
Test failed, details below.
Failed assertions:
	* Run #3: Target SlowerX7. Metric Scaled [10.15..10.15] is out of limit [6.89..7.17].
	* Run #3: Target SlowerX7. Metric GcAllocations [0..0] B is out of limit [10.00..10.00] GB.
Warnings:
	* Run #3: The benchmark was run 3 time(s), check log for details.
Diagnostic messages:
	* Run #1: Metrics check failed, requesting 1 run(s).
	* Run #2: Metrics check failed, requesting 1 run(s).
 ```

7. Well, that's all.
