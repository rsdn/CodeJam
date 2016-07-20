## TL;DR (xUnit version):
1. Create a new unit test project.
2. Add a reference to the CodeJam.PerfTests.xUnit nuget package.
3. Add a file with the following code:
 ```c#
using System;
using System.Threading;

using CodeJam.PerfTests;

using Xunit;

namespace CodeJam.Examples
{
	// A perf test class.
	[Trait("Category", "PerfTests: xUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

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
```

4. Run the `RunSimplePerfTest` test for the release build. You should get something like this
 (look at `[CompetitionBenchmark]` parameters):
 ```c#
	// A perf test class.
	[Trait("Category", "PerfTests: xUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.93, 3.05)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.89, 5.14)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.82, 7.21)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
```
 yep, it's a magic:)

5. After the `[CompetitionBenchmark]` attributes are filled with actual timing limits 
you can disable source annotation. To do this, use the `CompetitionHelpers.DefaultConfig`:
 ```c#
		[CompetitionFact]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfig);
```
6. Now the test will fail if timings do not fit into limits. To proof, set wrong limit for any competiton method and run the test.
As example:
 ```c#
		[CompetitionBenchmark(1, 1)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
```
 The test will fail with text like this:
 ```
Test failed, details below.
Failed assertions:
    * Run #3: Method SlowerX7 [6.99..6.99] does not fit into limits [1.00..1.00]
Warnings:
    * Run #3: The benchmark was run 3 time(s) (read log for details). Try to loose competition limits.
Diagnostic messages:
    * Run #1: Requesting 1 run(s): Limit checking failed.
    * Run #2: Requesting 1 run(s): Limit checking failed.
```

Well, that's all.