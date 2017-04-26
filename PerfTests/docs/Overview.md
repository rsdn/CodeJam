# CodeJam.PerfTests overview

> **META-NOTE**
>
> Places to update are marked with *--…--*.

> **IMPORTANT**
>
> This document assumes you're familiar with [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read the link first.

[TOC]

## PerfTest terminology

Each perftest has some key parts:

```c#
	// Competition config modifier attribute.
	// Check Configuration system section for more
	[CompetitionBurstMode] 
	// A perf test class.
	// Contains runner method, baseline method, implementations to benchmark, setup and cleanup methods.
	public class SimplePerfTest
	{
		// RECOMMENDED: Wrap all helpers into a region to improve test readability
		// (Visual Studio collapses regions by default).
		#region Helpers
		// Constants / fields
		private const int Count = CompetitionRunHelpers.BurstModeLoopCount;

		// Optional setup method. Same as in BenchmarkDotNet.
		[Setup]
		public void Setup() { /* We have nothing to do here. */ }

		// Optional cleanup method. Same as in BenchmarkDotNet.
		[Cleanup]
		public void Cleanup() { /* We have nothing to do here. */ }
		#endregion

		// Perftest runner method. Recommended naming pattern is $"Run{nameof(PerfTestClass)}".
		// You may add additional assertions to the body of perftest runner.
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

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
	}
```



## Compared to BenchmarkDotNet

The main thing introduced by CodeJam.PerfTests is a concept of _competition_. Competiton plays the same role the Benchmark class in BenchmarkDotNet does: it contains methods to measure. Main differences between benchmarks and competitions are:

* All members of the competition are annotated with expected metric values. Examples of such annotations are `[CompetitionBenchmark(6.89, 7.17)]` and `[GcAllocations(0)]` in the listing above.
* If competition configuration includes relative metrics (there is relative time metric enabled by default) the competition should include a baseline member. Use `[CompetitionBaseline]` attribute to mark the baseline method.
* The `Competition.Run()` method should be used to run the competition (BenchmarkDotNet uses `BenchmarkRunner.Run()`).
* Single competition run can involve multiple benchmark runs. As example, additional runs are performed if competition metrics were adjusted to proof accuracy of updated metric values.




In addition to the list above there are some limitations:

* Competitions use its own configuration system. Please do not apply BenchmarkDotNet's `[Config]` attributes to the competition classes, resulting behavior is undefined.

* Competitions do not support configs with multiple jobs and provide inaccurate results for configs with multiple parameter values. It is not a thing to be fixed in near future as there is no easy way to provide useful metric annotations for such benchmarks.

  > **EXPLANATION**
  >
  > Let's say we have `[Params(SomeEnum.Slow, SomeEnum.Fast)]` and a config with two jobs (with GC and without GC).
  >
  > There're four possible combinations and annotation for each method in competition will look like this:
  >
  > ```c#
  > [CompetitionBenchmark(2.88, 3.11, Case="Parameters=Fast, GC.Force=True")]
  > [CompetitionBenchmark(12.21, 15.45, Case="Parameters=Slow, GC.Force=True")]
  > [CompetitionBenchmark(2.35, 2.88, Case="Parameters=Fast, GC.Force=False")]
  > [CompetitionBenchmark(10.81, 13.40, Case="Parameters=Slow, GC.Force=False")]
  > [GcAllocations(1.22, 1.25, BinarySizeUnit.Kilobyte,  Case="Parameters=Fast, GC.Force=True")]
  > [GcAllocations(0.41, 0.45, BinarySizeUnit.Kilobyte, Case="Parameters=Slow, GC.Force=True")]
  > [GcAllocations(1.22, 1.25, BinarySizeUnit.Kilobyte, Case="Parameters=Fast, GC.Force=False")]
  > [GcAllocations(0.41, 0.45, BinarySizeUnit.Kilobyte, Case="Parameters=Slow, GC.Force=False")] 
  > public void SomeMethod() => ...
  > ```
  >
  > Well, I see no point in such annotations as it's too hard to make conclusions from it and it goes worse and worse as count of cases explodes very quickly. Note that additional attributes (e.g. memory allocation limits and IO limits) do not make the situation anyway better.
  >
  > Current implementation will merge these annotations into
  >
  > ```c#
  > [CompetitionBenchmark(2.35, 15.45)]
  > [GcAllocations(0.41, 1.25, BinarySizeUnit.Kilobyte]
  > public void SomeMethod() => ...
  > ```
  >
  > Not a best solution but at least it does not tease your brain with "What limits should I rely on?".
  >
  > If you want to do detailed investigation on multiple cases, consider to use raw BenchmarkDotNet benchmark.
  >
  > Have a idea how to make it better? Please, [create issue for it](https://github.com/rsdn/CodeJam/issues)!





## Configuration system 

CodeJam.PerfTests is designed to work out of the box without configuration. You can add (or disable) common features using `CompetitionFeaturesAttribute` (or derived attributes) like this:

```c#
	// Enable the source annotations feature
	[CompetitionAnnotateSources]
	public class SimplePerfTest
	{
		// ...
	}
```

However, if you want to have more control over the configuration there's advanced API available. Read [Configuration System](ConfigurationSystem.md) document for additional information.



## Source annotations

Source annotations provides a way to specify limits for the competition. By default CodeJam.PerfTests will only check the limits you've set but there's auto-annotation feature that will adjust limits with actual values and update sources for you. Check [Source Annotations](SourceAnnotations.md) for more.



