# [WORK IN PROGRESS] CodeJam.PerfTests.
Please do not remove [WORK IN PROGRESS] modifier until the doc is complete.
Until this use with care.

>  **META-NOTE**
>
>  Places to update are marked with *~…~*.

[TOC]

## What is it? (short version)

CodeJam.PerfTests is performance testing framework for .Net projects.

It allows to compare multiple implementations by execution time (*~memory limits coming soon~*), to annotate test methods with timing limits and to check the limits each time the test is run.

## TL;DR (NUnit version):

>**SIDENOTE**
>
>Here and below all samples are based on NUnit framework. Actually there's no significant difference, only things to change are package name and test frameworks attributes.
>For example how to use CodeJam.PerfTests with other test frameworks see 
>the [MS test version](Example.MSTest.md) and [xUnit version](Example.xUnit.md).

1. Create a new unit test project (*~Set targeting to .net 4.6+, previous FW versions are not supported for now~*).
2. Add a reference to the [CodeJam.PerfTests.NUnit](https://www.nuget.org/packages/CodeJam.PerfTests.NUnit) nuget package.
3. Add a file with the following code:
```c#
using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.Examples
{
	// A perf test class.
	[Category("PerfTests: NUnit examples")]
	public class SimplePerfTest
	{
		private const int Count = 10 * 1000;

		// Perf test runner method.
		[Test]
		public void RunSimplePerfTest() => Competition.Run(
			this, CompetitionHelpers.DefaultConfigAnnotate);

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
> We have an issue with perftest being run on low-end notebooks / nettops with mobile CPUs. Current implementation provides inaccurate competition limits occasionally (too high / too low timing values). We're going to fix it in the near future. Sorry!

5. After `[CompetitionBenchmark]` attributes are filled with timing limits, you can disable source auto-annotation. To do this,  use the `CompetitionHelpers.DefaultConfig`:
```c#
		[Test]
		public void RunSimplePerfTest() => Competition.Run(
			this, CompetitionHelpers.DefaultConfig);
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



## Ok, what is it? (longer one)

The CodeJam.PerfTest framework is built on top of amazing [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet), the best and most mature benchmarking framework for .Net.

Of course, there are another decent solutions such as [NBench](https://github.com/petabridge/NBench) or [SimpleSpeedTester](https://github.com/theburningmonk/SimpleSpeedTester) (check [awesome-dot-net-performance list](https://github.com/adamsitnik/awesome-dot-net-performance#performance-measurement) by Adam Sitnik for more) but, well, BenchmarkDotNet is the best one. Kudos to authors!

As a benchmarking framewotk, CodeJam.PerfTests do not provide anything that BenchmarkDotNet cannot do. However, as a testing framework it has some key features that other frameworks are missing:

1. **Stable and comparable results.** The perftests are meant to ran constantly, with different settings, on different machines, different hardware, different OSes and so on. At the same time you do want to be able to compare current run results with previous ones. Even if previous run was performed in completely different environment. CodeJam.PerfTests uses relative timings (timings scaled to baseline method results) to ensure that.

2. **Fast results.** In contrary to benchmarks (you'd rarely need dozen of them), it's normal to have tens or hundreds of perftests per project. This means the preftests had to be fast as you do not want to [wait for hours](https://twitter.com/jonskeet/status/735415336825192448) for the tests completion. Current implementation provides accurate results in no more than *~2-3 seconds per small tests with 2-5 competing methods and 10-12 seconds for large perftests*~ with 20 methods in benchmark.

3. **It just works: configuration.** In short, [benchmarking](http://blogs.microsoft.co.il/sasha/2012/06/22/micro-benchmarking-done-wrong-and-for-the-wrong-reasons/) [is](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/) [hard](http://www.hanselman.com/blog/ProperBenchmarkingToDiagnoseAndSolveANETSerializationBottleneck.aspx). BenchmarkDotNet does [a huge amount of work undercover](http://benchmarkdotnet.org/HowItWorks.htm) to fight against various side-effects but its default configuration does not suit well for short-running perftests. CodeJam.PerfTests includes heavily-tuned presets for common use cases that provide comparable accuracy with a much faster test runs (*~only microbenchmarks are supported in preview version, memory and IO-related perftests coming soon~*) . Of course, you're still able to configure anything by yourself, we do not limit anything.

4. **It just works: automation.** Writing correct perftest is very boring thing. You had to estimate the timings, run the test, update the limits, run in CI server, update the limits from CI server log and so on and so on.  Repeat this for each perftest (remember, there're a hundreds of them usually) and the whole idea quickly becomes "heck it, I quit" thing. CodeJam.PerfTests automates this for you (it's an opt-in feature and is disabled by default). If perftest fails, the limits are adjusted, sources are updated with new limits and the test is run few more times to proof new limits are accurate. The same is performed if the test was failed during last run on CI server.

   > **WARNING**
   >
   > Be sure to use source control before enabling the source annotation feature. If not you'll lose your current perftest limits as they will be overwritten without confirmation.

5. **It just works: diagnostics.** As said above, benchmarking is hard. CodeJam.PerfTests tries to take the burden from you by providing various diagnostic messages and warnings in case something goes wrong. Running a debug build, benchmark results are inaccurate or the source version does not match to binaries? We'll warn about it. At the same time we are trying not to bother you with garbage messages: by default output log contains only really important things. Of course, it's configurable.

6. **It just works: real-world use cases.**  The design of the framework is heavily inspired by dogfooding experience and use cases from our real projects. If there're sane use cases uncovered we're going to add them. What's done so far:

   * Support for most popular testing frameworks and base types for adding new ones. NUnit, xUnit (*~not stable yet~*) and MS Test are covered for now.
   * Support for perftests over dynamically generated or emitted codebase. Code annotations for such tests are stored as xml resource files, therefore you can change the code without transferring the competition limits from old code to new one.
   * Continuous integration support. If you do not want to update source annotations during CI test run (who does?) you can enable limits logging. Next run on a developer machine will catch up changed limits and update sources for you.
   * Configuration via per-assembly `.appconfig` files, assembly- and type-level attributes. Setup once, use everywhere.
   * Console perftest runner. In case you need it.

    What's next?

   * Memory and IO perftests including ability to set limits for various performance-related characteristics, from allocations up to page faults.
   * Advanced diagnostics. Warning about outdated limits, LOH allocations and so on
   * .Net Core support
   * F# and VB source annotations support
   * Have we missed anything? Create an issue for it!







Ok, back to topic. Why not just use one of these? Is there a room for another testing framework or it's all about https://xkcd.com/927/ ?

Well, it turns out that benchmarks and perftests ARE NOT the same.

Benchmark runners are aimed to measure perf metrics of multiple implementations and to do the measurements as precise as it's possible. This means that a huge amount of work [should be done](http://benchmarkdotnet.org/HowItWorks.htm) to fight against various side-effects. In short, [benchmarking](https://andreyakinshin.gitbooks.io/performancebookdotnet/content/science/microbenchmarking.html) [is](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/) [hard](http://www.hanselman.com/blog/ProperBenchmarkingToDiagnoseAndSolveANETSerializationBottleneck.aspx) :)

The goals for performance testing are different. This is by design:

* there will be a lot of perftests and these will be run continiously (and you do not want to [wait for hours](https://twitter.com/jonskeet/status/735415336825192448) for the tests completion),
* the tests will be run on different hardware from tablets up to CI servers,
* and, to be honest, you're not interested in preciseness. There's no point in direct comparison of
  `FastMethod` that takes **0.1 sec** when run on nettop and `UltraFastMethod` that takes **0.05 sec** when run on dedicated testserver.

Absolute timings means nothing until you're sure that benchmarks are run under exactly same conditions.
For example: on testserver the `UltraFastMethod` was run the `FastMethod` takes  **0.015 sec** (is ~3x faster).

And here's one more thing: you cannot trust benchmark results obtained from code being run in clean room conditions.

In production your code will be influenced by the environment and benchmark ignoring these side effects will lie to you.  
If your code does a lot of memory reads/writes it will suffer from CPU cache hits caused by concurrent reads.  
If you do a lot of short-lived allocations, GC collection caused by another code will promote your objects into higher 
generation increasing the gc overhead.  
Reading from HDD/Network? Still the same - unexpected latencies will hurt you.

All of the above means that:
* You do want to get reproducible results that are as close to the results from production as it's possible.
* You do want to be able to compare benchmark results obtained from different test runs, from different machines 
  and from different implementations.
* You do want to have a lot (a lot means hundreds and thousands) of tests and to maintain them as easy as usual unit tests.

CodeJam.PerfTest handles all of the above.


## Creating a new performance test

### Code for performance test
Let's say we want to write a perftest that measures cost of list resizing.
We will start with a class with two methods:
```cs
	public class ListCapacityPerfTest
	{
		private const int Count = 10000;

		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (int i = 0; i < Count; i++)
				data.Add(i);

			return data.Count;
		}

		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (int i = 0; i < Count; i++)
				data.Add(i);

			return data.Count;
		}
	}
```

Next, we should choose the baseline (or reference) implementation all other benchmark members will be compared to.
In simple tests like this it's actually doesn't matter which one to choose. However, in large and complex perftests 
baseline should be chosen with caution. Time of execution of the baseline should be stable,
it should not be in order of magnitude slower (or faster) than another competitors and so on.

**TODO:** link to the guidelines for the perftests.

So, let's mark the `ListWithoutCapacity()` as `[CompetitionBaseline]` and `ListWithCapacity()` as a `[CompetitionBenchmark]`

### Code that runs the performance test
Next, you should write code that runs the perftest. Here's how to do it with NUnit:
```
		[Test]
		public static void RunListCapacityPerfTest() => 
			Competition.Run<ListCapacityPerfTest>(CompetitionHelpers.DefaultConfig);
// or 
		[Test]
		public void RunListCapacityPerfTest() => 
			Competition.Run(this, CompetitionHelpers.DefaultConfig);
```

The first `Competition.Run()` overload is universal - it can be placed inside the class or outside,
 but it requires to explicitly the type of the perftest class explicitly.

The second one infers the type from `this` parameter. Of course, there's no point
 in using it if the `ListCapacityPerfTest` method is not member of the perftest class.

Also, you can use default console runner to run the performance test:
```
		private static void Main()
		{
			ConsoleCompetitionRunner.Run<ListCapacityPerfTest>(CompetitionHelpers.DefaultConfig);

			Console.ReadKey();
		}
```

output will look like this:
![ConsoleRun.png](ConsoleRun.png)

### Setting the limits:
As you can see the perftest was failed with message
```
Test failed, details below.
Failed assertions:
    * Run #3: Method ListWithCapacity [0.77..0.77] has empty limit. Please fill it.
Warnings:
    * Run #3: The benchmark was run 3 time(s) (read log for details). Try to loose competition limits.
Diagnostic messages:
    * Run #1: Requesting 1 run(s): Limit checking failed.
    * Run #2: Requesting 1 run(s): Limit checking failed.
```

As you can see from the screen above the timings when the test is run a are somewhere between [0.76..0.78].

## Running existing perftests

### Competition limits

CodeJam.PerfTest uses so called competition performance testing approach. 
All methods in benchmark are not compared directly to each other. Instead of this an additional baseline 
implementation method is included into benchmark (it should be annotated with `[CompetitionBaseline]` attribute)
and the relative-to-baseline timings are used.

By default limits for each method in the competition are stored as attribute annotation. The 
```cs
		[CompetitionBenchmark(2.92, 3.05)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);
```
line means that method is expected to take 2.92x..3.05x relative to the baseline.

If there's no baseline method in the competition the test fill fail with the following message
 (comment out the `[CompetitionBaseline]` attribute to check):
```
Test completed with errors, details below.
Errors:
    * Run #1: The benchmark SimplePerfTest has no baseline.
```

### Viewing the output: 
In addition to short message CodeJam.PerfTest provides detailed information that simplifies troubleshooting.
It can be viewed via test output view. Output for passed tests looks like this:
```
BenchmarkDotNet=v0.9.7.0
OS=Microsoft Windows NT 10.0.10586.0
Processor=Intel(R) Core(TM) i5-2550K CPU 3.40GHz, ProcessorCount=4
Frequency=3312787 ticks, Resolution=301.8606 ns, Timer=TSC
HostCLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
JitModules=clrjit-v4.6.1080.0

Type=SimplePerfTest  Mode=SingleRun  Toolchain=InProcessToolchain  
LaunchCount=1  WarmupCount=100  TargetCount=300  

   Method |      Median |     StdDev | Scaled |         Min | Lnml(min) | Lnml(max) |         Max |
--------- |------------ |----------- |------- |------------ |---------- |---------- |------------ |
 Baseline |  45.8828 us |  3.6272 us |   1.00 |  44.3735 us |      1.00 |      1.00 |  74.8614 us |
 SlowerX3 | 134.9317 us |  6.6991 us |   2.94 | 126.4796 us |      2.95 |      2.95 | 171.7587 us |
 SlowerX5 | 229.4141 us | 15.8135 us |   5.00 | 222.4713 us |      5.00 |      5.00 | 360.1197 us |
 SlowerX7 | 320.8779 us | 17.5443 us |   6.99 | 295.5216 us |      6.96 |      6.96 | 428.9440 us |


// !====================================
// !Run 1, total runs (expected): 1.
// ? #1.1  00,659s, Informational@Analyser: CompetitionAnalyser: All competition limits are ok.
```

### If limits are failed

In case when some of methods that participating in limits do not fit into limits the benchmark is rerun again (up to three times by default).
It helps to detect the case when limits are too tight and test fails time-to-time. In this case the test will complete with following warnig:
```
Test completed with warnings, details below.
Warnings:
    * Run #2: The benchmark was run 2 time(s) (read log for details). Try to loose competition limits.
Diagnostic messages:
    * Run #1: Requesting 1 run(s): Limit checking failed.
    * Run #2: CompetitionAnalyser: All competition limits are ok.
```

If the limits were failed three times in a row, the test will fail with message
```
Test failed, details below.
Failed assertions:
    * Run #3: Method SlowerX3 [3.01..3.01] does not fit into limits [2.90..2.90]
Warnings:
    * Run #3: The benchmark was run 3 time(s) (read log for details). Try to loose competition limits.
Diagnostic messages:
    * Run #1: Requesting 1 run(s): Limit checking failed.
    * Run #2: Requesting 1 run(s): Limit checking failed.
```


### Ignoring limits for some of methods:
If you want to temporary exclude the method from the competition just add `DoesNotCompete = true` to the annotation, like this
```cs
		[CompetitionBenchmark(2.92, 3.05, DoesNotCompete = true)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);
```
or use `[Benchmark]` attribute instead of `[CompetitionBenchmark]`.
