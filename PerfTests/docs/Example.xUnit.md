## TL;DR (xUnit version)

> ***~WARNING~***
>
> The xUnit integration is very-alpha version and has some known issues ([xunit/#908](https://github.com/xunit/xunit/issues/908) as example) we're going to fix sooner or later.

1. Create a new unit test project (*~Set targeting to .net 4.5.2+, previous FW versions are not supported for now~*).
2. Add a reference to the [CodeJam.PerfTests.xUnit](https://www.nuget.org/packages/CodeJam.PerfTests.xUnit) nuget package.
3. Add a file with the following code:
```c#
using System;
using System.Threading;

using CodeJam.PerfTests;

using Xunit;

namespace CodeJam.Examples.PerfTests
{
	// A perf test class.
	[Trait("Category", "PerfTests: xUnit examples")]
	[CompetitionAnnotateSources] // Opt-in feature: source annotations.
	public class SimplePerfTest
	{
		private const int Count = 200;

		// Perf test runner method.
		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this);

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
		[CompetitionBenchmark(2.96, 3.08)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.94, 5.14)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.89, 7.17)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
```
yep, it's magic:)

 > **NOTE**
 >
 > This test is known to provide inaccurate results on on low-end notebooks / nettops with mobile CPUs due to aggressive frequency scaling and throttling.
 >
 > There're two workarounds. First, you can set `Count` to `CompetitionHelpers.RecommendedFastSpinCount`. Second, you can use `CompetitionHelpers.RecommendedSpinCount` together with `[CompetitionBurstMode]` attribute. 

5. After `[CompetitionBenchmark]` attributes are filled with timing limits, you can disable source auto-annotation. To do this, just remove the `[CompetitionAnnotateSources]` attribute.

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
