# [WORK IN PROGRESS] CodeJam.PerfTests - performance tests framework for .Net projects.
Please do not remove [WORK IN PROGRESS] modifier until the doc is complete.
Until this use with care.

## What it is? (short version)

CodeJam.PerfTests is performance testing framework for .Net projects. It allows to compare multiple implementations by time (memory limits coming soon),
annotate the methods with timning limits and check the limits each time the test is run.

## TL;DR:
0. Create a new unit test project (there and below NUnit framework is used as a test runner.
Documentation for another testing frameworks will be added later).
1. Add a reference to the CodeJam.PerfTests.NUnit nuget package.
2. Add a file with the following code:
```cs
using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.Examples
{
	[Category("PerfTests: examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() =>
			Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
}
```

3. Run the test for the release build. You should get something like this:
```cs
	[Category("PerfTests: examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		[Test]
		public void RunSimplePerfTest() => 
			Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(Count);

		[CompetitionBenchmark(2.92, 3.05)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		[CompetitionBenchmark(4.87, 5.07)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		[CompetitionBenchmark(6.75, 7.05)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
```
yep, it's a magic:)

After the `[CompetitionBenchmark]` attributes are filled with actual timing limits
you can disable source annotation by using 
```cs
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfig);
```
Now the test will fail if timings do not fit into limits. To proof, set 
```
		[CompetitionBenchmark(1, 1)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
```
and run the test. It will fail with text like this:
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

## Ok, what it is? (longer one)

The CodeJam.PerfTest framework is built on top of amazing [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet),
the best and most mature benchmarking framework for .Net.

Of course, there are another decent benchmarking frameworks such as 
[NBench](https://github.com/petabridge/NBench) or [SimpleSpeedTester](https://github.com/theburningmonk/SimpleSpeedTester) 
but, well, BenchmarkDotNet is the best one. Kudos to authors!

Ok, back to topic. Why not just use one of these? Is there a room for another testing framework or it's all about https://xkcd.com/927/ ?

Well, it turns out that benchmarks and perftests ARE NOT the same.

Benchmark runners are aimed to measure perf metrics of multiple implementations and to do the measurements as precise as it's possible.
This means that a huge amount of work [should be done](https://github.com/PerfDotNet/BenchmarkDotNet#how-it-works) to fight against various side-effects.
In short, [benchmarking](https://andreyakinshin.gitbooks.io/performancebookdotnet/content/science/microbenchmarking.html) [is](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/) [hard](http://www.hanselman.com/blog/ProperBenchmarkingToDiagnoseAndSolveANETSerializationBottleneck.aspx) :)

The goals for performance testing are different. This is by design:
 there will be a lot of perftests and these will be run continiously (and you do not want to [wait for hours](https://twitter.com/jonskeet/status/735415336825192448) for the tests completion),
 the tests will be run in different environments,
 and, to be honest, you're not interested in preciseness. There's no point in direct comparison of
ImplA that takes 0.1 sec when run on a tablet and ImplB that takes 0.05 sec when run on dedicated testserver.
The absolute timings means nothing until you're sure that all methods in benchmark are run under exactly same conditions.
In actual, ImplA from example above takes 0.015 sec to complete when run on same hardware the ImplB was run.

And here's one more thing: you're not interested in benchmark results obtained from code being run in clean room.
In production your code will be influenced by the environment and benchmark ignoring these side-effects will lie to you.
If your code does a lot of memory reads/writes it will suffer from CPU cache hits caused by concurrent reads.
If you do a lot of short-lived allocations, GC collection caused by another code will promote your objects into higher generation increasing the gc overhead.
Reading from HDD/Network? Still the same - unexpected latencies will hurt you.

All of the above means that:
* You do want to get reproducible results that are as close to the results from production as it's possible.
* You do want to be able to compare the results obtained from different test runs, from different machines and from different implementations.
* You do want to have a lot (a lot means hundreds and thousands) of tests and the maintenance should be not harder than for usual unit-tests.

CodeJam.PerfTest covers all of the above.

