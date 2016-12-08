# CodeJam.PerfTests overview

> **META-NOTE**
>
> Places to update are marked with *~…~*.

> **IMPORTANT**
>
> This document assumes you're familiar with [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read the link before continue

[TOC]

## PerfTest terminology

Each perftest has some key parts:

```c#
	// PerfTest attribute annotations.
	// Check Configuration system and Source annotations sections for more
	[CompetitionBurstMode] 
	// A perf test competition class.
	// Contains runner method, baseline method, implementations to benchmark, setup and cleanup methods.
	public class SimplePerfTest
	{
		// Wrap all helpers into a region to improve test readability
		// (Visual Studio collapses regions by default).
		#region Helpers
		// Constants / fields
		private static readonly int _count = CompetitionHelpers.RecommendedSpinCount;

		// Optional setup method. Same as in BenchmarkDotNet.
		[Setup]
		public void Setup() { /* We have nothing to do here. */ }

		// Optional cleanup method. Same as in BenchmarkDotNet.
		[Cleanup]
		public void Cleanup() { /* We have nothing to do here. */ }
		#endregion

		// Perftest runner method. Naming pattern is $"Run{nameof(PerfTestClass)}".
		// You may use it to write additional assertions after the perftest is completed.
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		public void Baseline() => Thread.SpinWait(_count);

		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.88, 3.12)]
		public void SlowerX3() => Thread.SpinWait(3 * _count);

		// Competition member #2. Should take ~5x more time to run.
		[CompetitionBenchmark(4.80, 5.20)]
		public void SlowerX5() => Thread.SpinWait(5 * _count);

		// Competition member #3. Should take ~7x more time to run.
		[CompetitionBenchmark(6.72, 7.28)]
		public void SlowerX7() => Thread.SpinWait(7 * _count);
	}
```



## Compared to BenchmarkDotNet

The main thing introduced by CodeJam.PerfTests is a concept of _competition_. Competiton plays the same role the Benchmark class in BenchmarkDotNet does: it contains methods to measure. Main differences between benchmarks and competitions are:

* Competitions always include a baseline method. Baseline is required to provide relative timings (see below). Use `[CompetitionBaseline]` attribute to mark the baseline method.
* Competitions are meant to run multiple times and their results should be comparable even if previous run was performed on another machine. Therefore competition results are stored as a relative-to-baseline timings.
* Competition methods (except baseline method) are annotated with competition limits that describe expected execution time (relative-to-baseline time is used). Use `[CompetitionBenchmark]` to mark the competition method and set limits for it.
* The `Competition.Run()` method should be used to run the competition (BenchmarkDotNet uses `BenchmarkRunner.Run()`).
* Single competition run can invoke `BenchmarkRunner.Run()`multiple times (for example, additional runs are performed if competition limits were adjusted).





> ***~THINGS TO CHANGE~***
>
> We're going to get rid of these, documentation will be updated.

In additional to the list above there are some limitations:

* Competitions use its own configuration system. Please do not apply BenchmarkDotNet's `[Config]` attributes to the competition classes, resulting behavior is undefined.

* Competitions do not support diagnosers by default. You need to set up toolchain from BenchmarkDotNet to enable diagnosers.

* Competitions do not work well with configs with multiple jobs or parameter values. It is not a thing to be fixed in near future as there is no easy way to provide useful limits for such benchmarks.

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
  > public void SomeMethod() => ...
  > ```
  >
  > Well, I see no point in such annotations as it's too hard to make conclusions from it and it goes worse and worse as count of cases explodes very quickly. Note that additional attributes (e.g. memory allocation limits and IO limits) do not make the situation anyway better.
  >
  > Current implementation will merge these annotations into
  >
  > ```c#
  > [CompetitionBenchmark(2.35, 15.45)]
  > public void SomeMethod() => ...
  > ```
  >
  > Not a best solution, I do agree. But at least it does not tease your brain with "What limits should I rely on?".
  >
  > If you want to do quick investigation on multiple cases, consider to use raw BenchmarkDotNet benchmark.
  >
  > Have a idea how to make it better? Great, *~create an issue for it! TODO: link~*




## Configuration system 

CodeJam.PerfTests is designed to not require any configuration by default. You can add (or disable) specific features using `CompetitionFeaturesAttribute` (or derived attributes) like this:

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

Source annotations provides a way to specify limits for the competition. Current implementation supports limits for relative-to-baseline timings only, *~memory limits coming soon~*. By default CodeJam.PerfTests will only check the limits you've set but there's auto-annotation feature that will adjust limits with actual values and update sources for you. Check [Source Annotations](SourceAnnotations.md) for more.



