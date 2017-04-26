# CodeJam.PerfTests

> **META-NOTE**
>
> Places to update are marked with *--…--*.

[TOC]

## What is it? (short version)

CodeJam.PerfTests is a framework for .net apps aimed to make perfomance testing as easy as normal unit tests are.

It allows to collect and to compare various metrics (such as execution time, memory allocations, GC collection count and so on) for competing implementations.

## TL;DR (NUnit version)

>**NOTE**
>
>Here and below all samples are based on NUnit framework. Actually there's no significant difference, only things to change are package name and test frameworks attributes. For example how to use CodeJam.PerfTests with other test frameworks check  [MS perftest example](Example.MSTest.md) and [xUnit perftest test example](Example.xUnit.md).

1. Create a new unit test project (*--Set targeting to .net 4.6+, previous FW versions are not supported for now--*).
2. Add a reference to the [CodeJam.PerfTests.NUnit](https://www.nuget.org/packages/CodeJam.PerfTests.NUnit) nuget package.
3. Add a file with the following code:
```c#
using System;
using System.Threading;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.Examples.PerfTests
{
	// A perf test class.
	[Category("PerfTests: NUnit examples")]
	[CompetitionAnnotateSources] // Opt-in feature: source annotations.
	public class SimplePerfTest
	{
		private const int Count = 200;

		// Perf test runner method.
		[Test]
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


## Ok, what is it? (longer one)

The CodeJam.PerfTest framework is built on top of amazing [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet), the best and most mature benchmarking framework for .Net.

Of course, there are another decent solutions such as [NBench](https://github.com/petabridge/NBench) or [SimpleSpeedTester](https://github.com/theburningmonk/SimpleSpeedTester) (check [awesome-dot-net-performance list](https://github.com/adamsitnik/awesome-dot-net-performance#performance-measurement) by Adam Sitnik for more) but, well, BenchmarkDotNet is the best one. Kudos to authors!

As a benchmarking framework, CodeJam.PerfTests do not provide anything that BenchmarkDotNet cannot do. However, as a testing framework it has some key features that other frameworks are missing:

1. **Stable and comparable results.** Perftests are meant to be run constantly, on multiple machines, varying hardware, different OS and so on. As a result, some of collected metrics (timings, as example) cannot be compared by absolute values. CodeJam.PerfTests allows to use relative metrics (scaled to baseline method) to keep metrics comparable.

2. **Fast results.** In contrary to benchmarks (you'd rarely need dozen of them), it's normal to have tens or hundreds of perftests per project. This means perftests had to be fast as you do not want to [wait for hours](https://twitter.com/jonskeet/status/735415336825192448) for tests completion. Current implementation provides accurate results in no more than 2-3 seconds per small tests with 2-5 competing methods and 10-12 seconds for large perftests with 20+ methods.

3. **It just works: configuration.** In short, [benchmarking](http://blogs.microsoft.co.il/sasha/2012/06/22/micro-benchmarking-done-wrong-and-for-the-wrong-reasons/) [is](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/) [hard](http://www.hanselman.com/blog/ProperBenchmarkingToDiagnoseAndSolveANETSerializationBottleneck.aspx). BenchmarkDotNet does [a huge amount of work undercover](http://benchmarkdotnet.org/HowItWorks.htm) to fight against various side-effects but its default configuration does not works well if you have a lot of benchmarks to run. CodeJam.PerfTests includes heavily-tuned presets for common use cases that provide comparable accuracy with a much faster test runs . Of course, you're still able to configure anything by yourself if defaults are not fine for your code.

4. **It just works: automation.** Writing correct perftest is very boring thing. You had to estimate the limits, run the test, update the limits, run on CI server, update the limits from CI server log and so on and so on.  Repeat this for each perftest (remember, there're a hundreds of them usually) and the whole idea quickly becomes "heck it, I quit" thing. CodeJam.PerfTests performs limit annotations for you (it's an opt-in feature and is disabled by default). If actual metric values do not fit into limits, limits are adjusted, sources are annotated with adjusted metric limits and the test is run few more times to proof new limits are accurate. The same is performed if the perftest was failed during last run on CI server.

   > **WARNING**
   >
   > Be sure to use source control before enabling the source annotation feature. If not you'll lose your current perftest limits as they will be overwritten without confirmation.

5. **It just works: diagnostics.** As said above, benchmarking is hard. CodeJam.PerfTests tries to take the burden from you by providing various diagnostic messages and warnings when something goes wrong. Running a debug build, benchmark results are inaccurate or the source version does not match to binaries? There is warning for it. At the same time we are trying not to bother you with garbage messages: by default output contains only really important things. Of course, it's configurable.

6. **It just works: real-world use cases.**  The design of the framework is heavily inspired by dogfooding experience and use cases from our real projects. If there're sane use cases uncovered we're going to add them. What's done so far:

   * Support for most popular testing frameworks and base types for adding new ones. NUnit, xUnit (*--not stable yet--*) and MS Test perftest packages are available out of the box.
   * Support for perftests over dynamically generated or emitted codebase. Code annotations for such tests are stored as xml resource files, therefore you can change the code without transferring the competition limits from old code to new one.
   * Continuous Integration support. CodeJam.PerfTests detects common CI services automatically (AppVeyor, Jenkins, TFS, TeamCity and Travis CI are supported for now) and adjusts its settings appropriately. E.g., by default we do not update source annotations during CI runs, as they may be unavailable. Instead, adjusted limits are logged and will be applied during perftest run on a developer's machine.
   * Custom metrics support (*--experimental as we did not document it yet --*). Want to compare implementations by sql queries time, network activity or JIT time? Bring your own metric for it:)
   * Configuration via per-assembly `.appconfig` files, assembly- and type-level attributes. Setup once, use everywhere.
   * Competition configuration system was rewritten. It is much simpler to create custom competition configs and to modify existing ones now.
   * Console perftest runner. In case you need it.

   What's next?

   * More performance-related metrics out of the box.
   * Advanced diagnostics. Warnings about inactual metric limits, LOH allocations and so on.
   * .Net Core, .Net 4.5 and UAP support.
   * F# and VB source annotations support.
   * Have we missed anything? Create an issue for it!

   ​


That's all. Go and write some code! : )