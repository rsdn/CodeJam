# CodeJam.PerfTests

> **META-NOTE**
>
> Places to update are marked with *~…~*.

[TOC]

## What is it? (short version)

CodeJam.PerfTests is performance testing framework for .Net projects.

It allows to compare multiple implementations by execution time (*~memory limits coming soon~*), to annotate test methods with timing limits and to check the limits each time the test is run.

## TL;DR (NUnit version)

>**NOTE**
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
	[CompetitionAnnotateSources] // Opt-in feature: source annotations.
	public class SimplePerfTest
	{
		private const int Count = 200;

		// Perf test runner method.
		[Test]
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


## Ok, what is it? (longer one)

The CodeJam.PerfTest framework is built on top of amazing [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet), the best and most mature benchmarking framework for .Net.

Of course, there are another decent solutions such as [NBench](https://github.com/petabridge/NBench) or [SimpleSpeedTester](https://github.com/theburningmonk/SimpleSpeedTester) (check [awesome-dot-net-performance list](https://github.com/adamsitnik/awesome-dot-net-performance#performance-measurement) by Adam Sitnik for more) but, well, BenchmarkDotNet is the best one. Kudos to authors!

As a benchmarking framework, CodeJam.PerfTests do not provide anything that BenchmarkDotNet cannot do. However, as a testing framework it has some key features that other frameworks are missing:

1. **Stable and comparable results.** Perftests are meant to be run constantly, on multiple machines, varying hardware, different OS and so on. At the same time you do want to be able to compare current run results with previous ones, even if previous run was performed in completely different environment. CodeJam.PerfTests uses relative timings (timings scaled to baseline method results) to ensure that.

2. **Fast results.** In contrary to benchmarks (you'd rarely need dozen of them), it's normal to have tens or hundreds of perftests per project. This means perftests had to be fast as you do not want to [wait for hours](https://twitter.com/jonskeet/status/735415336825192448) for the tests completion. Current implementation provides accurate results in no more than *~2-3 seconds per small tests with 2-5 competing methods and 10-12 seconds for large perftests with 20 methods*~.

3. **It just works: configuration.** In short, [benchmarking](http://blogs.microsoft.co.il/sasha/2012/06/22/micro-benchmarking-done-wrong-and-for-the-wrong-reasons/) [is](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/) [hard](http://www.hanselman.com/blog/ProperBenchmarkingToDiagnoseAndSolveANETSerializationBottleneck.aspx). BenchmarkDotNet does [a huge amount of work undercover](http://benchmarkdotnet.org/HowItWorks.htm) to fight against various side-effects but its default configuration does not suit well for short-running perftests. CodeJam.PerfTests includes heavily-tuned presets for common use cases that provide comparable accuracy with a much faster test runs (*~only CPU-bound benchmarks are supported in preview version, memory and IO-related perftests coming soon~*) . Of course, you're still able to configure anything by yourself if defaults are not fine for your code.

4. **It just works: automation.** Writing correct perftest is very boring thing. You had to estimate the timings, run the test, update the limits, run in CI server, update the limits from CI server log and so on and so on.  Repeat this for each perftest (remember, there're a hundreds of them usually) and the whole idea quickly becomes "heck it, I quit" thing. CodeJam.PerfTests fills limit annotations for you (it's an opt-in feature and is disabled by default). If perftest fails, limits are adjusted, sources are updated with new limits and the test is run few more times to proof new limits are accurate. The same is performed if the test was failed during last run on CI server.

   > **WARNING**
   >
   > Be sure to use source control before enabling the source annotation feature. If not you'll lose your current perftest limits as they will be overwritten without confirmation.

5. **It just works: diagnostics.** As said above, benchmarking is hard. CodeJam.PerfTests tries to take the burden from you by providing various diagnostic messages and warnings when something goes wrong. Running a debug build, benchmark results are inaccurate or the source version does not match to binaries? There will be warning about it. At the same time we are trying not to bother you with garbage messages: by default output log contains only really important things. Of course, it's configurable.

6. **It just works: real-world use cases.**  The design of the framework is heavily inspired by dogfooding experience and use cases from our real projects. If there're sane use cases uncovered we're going to add them. What's done so far:

   * Support for most popular testing frameworks and base types for adding new ones. NUnit, xUnit (*~not stable yet~*) and MS Test perftest packages are available out of the box.
   * Support for perftests for dynamically generated or emitted codebase. Code annotations for such tests are stored as xml resource files, therefore you can change the code without transferring the competition limits from old code to new one.
   * Continuous Integration support. CodeJam.PerfTests detects common CI services automatically (AppVeyor, Jenkins, TFS, TeamCity and Travis CI are supported for now) and adjusts its settings appropriately. E.g., by default we do not update source annotations during CI runs, as they can be unavailable. Instead, adjusted limits are logged and will be applied during run on a developer's machine.
   * Configuration via per-assembly `.appconfig` files, assembly- and type-level attributes. Setup once, use everywhere.
   * Competition configuration system was rewritten. It is much simpler to create custom competition configs and to modify existing ones now.
   * Console perftest runner. In case you need it.

    What's next?

   * Memory and IO perftests including ability to set limits for various performance-related characteristics, from allocations up to page faults.
   * Advanced diagnostics. Warning about outdated limits, LOH allocations and so on.
   * .Net Core support and .Net 4.5 support.
   * F# and VB source annotations support.
   * Have we missed anything? Create an issue for it!

   ​


That's all. Go and write some code! : )