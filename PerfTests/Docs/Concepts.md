# CodeJam.PerfTests.

> **META-NOTE**
>
> Places to update are marked with *~…~*.
>
> **IMPORTANT**
>
> This document assumes you're familiar with [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read it before continue

[TOC]

## Compared to BenchmarkDotNet

The main thing introduced by CodeJam.PerfTests is a concept of _competition_.  Competiton plays the same role the Benchmark class in BenchmarkDotNet does: it contains methods to measure. The main differences from usual benchmarks are the following:

* Competitions always include a baseline method. Baseline is required to provide relative timings (see below). Use `[CompetitionBaseline]` attribute to mark the baseline method.
* Competitions are meant to be run multiple times and competition results should be comparable even if  previous run was performed on another machine. Therefore competition results are stored as a relative-to-baseline timings.
* Competition methods except baseline method are annotated with competition limits that describe expected execution time (relative-to-baseline time is used). Use `[CompetitionBenchmark]` to mark the competition method.
* The `Competition.Run()` method should be used to run the competition (BenchmarkDotNet uses `BenchmarkRunner.Run()`).
* Single competition run can invoke `BenchmarkRunner.Run();`multiple times (for example, additional runs are performed if competition limits were adjusted).



> ***~THINGS TO CHANGE~***
>
> We're going to get rid of these, documentation will be updated.

In additional to the list above there are some limitations:

* Competitions use its own configuration system. Please do not apply `[Config]` attributes to the competition classes, the behavior is undefined.

* Competitions do not support diagnosers by default. You need to use toolchain from BenchmarkDotNet to enable diagnosers.

* Competitions do not work well with configs with multiple jobs or parameters. It is not a thing to be fixed in near future as there is no easy way to provide useful limits for such jobs

  > **EXPLANATION**
  >
  > Let's say we have Parameters = `SomeEnum.Slow`,`SomeEnum.Fast` and jobs with GC and without GC.
  >
  > Annotation for these  will look like this
  >
  > ```c#
  > [CompetitionBenchmark(2.88, 3.11, Case="Parameters=Fast, GC.Force=True")]
  > [CompetitionBenchmark(12.21, 15.45, Case="Parameters=Slow, GC.Force=True")]
  > [CompetitionBenchmark(2.35, 2.88, Case="Parameters=Fast, GC.Force=False")]
  > [CompetitionBenchmark(10.81, 13.40, Case="Parameters=Slow, GC.Force=False")]
  > public void SomeMethod() => ...
  > ```
  >
  > Well, I see no point in such annotations as it's too hard to make some conclusions from it and it goes worse and worse as count of cases explodes very quickly.
  >
  > Current implementation will merge these annotations into
  >
  > ```c#
  > [CompetitionBenchmark(2.35, 15.45)]
  > public void SomeMethod() => ...
  > ```
  >
  > Not a best solution, I agree. But at least it does not explode your brain with "What limits should I account on?"
  >
  > If you want to do quick investigation on multiple cases, consider to use raw BenchmarkDotNet benchmark instead. Have a idea how to make it better? Great, create an issue for it!



## Configuration system 

CodeJam.PerfTests configuration uses almost same approach the BenchmarkDotNet do. However, there are extensions to ease configuration of large projects with hundreds or thousands of perftetests. Here's how it works:



### 1. Run competition with explicit config

>  **NOTE**
>
> Explicit config passing is an advanced technique and should be used only when you want to have a perfect control over the configuration. It skips entire configuration pipeline and therefore it's up to you to pass correct config into competition.



#### 1.1. Pass config as competition arg

It just works. No additional adjustments are performed, competition will use the config you're passing to the test. Use  it like this

```c#
		[Test]
		public void RunSimplePerfTest()
		{
			var config = CompetitionHelpers.ConfigForAssembly;
			config = config
				.WithAllowDebugBuilds(true) // No warning when run on default builds
				.WithDetailedLogging(true); // Enable detailed test console output

			Competition.Run(this, config); // Pass the config to the competition
		}
```

Note that this is advanced scenario and it should be used only  when you want to have a perfect control over the config.

#### 1.2. Use custom config attribute

If you do want to reuse the config across multiple tests you can define custom config attribute

```c#
	public class MyCompetitionAttribute : CompetitionConfigAttribute
	{
		public MyCompetitionAttribute() : base(Create)
		{ }

		private static ICompetitionConfig Create()
		{
			var config = new ManualCompetitionConfig(
				CompetitionHelpers.CreateConfig(typeof(MyCompetitionAttribute).Assembly));

			config.ApplyModifier(new CompetitionOptions
			{
				RunOptions = { ReportWarningsAsErrors = true }, // Fail on warnings
				Limits = { LongRunningBenchmarkLimit = TimeSpan.FromSeconds(5) } // no long runs allowed
			});

			config.ApplyModifier(new Job()
			{
				Run = { LaunchCount = 2 } // Run twice
			});

			return config;
		}
	}
```

Then, apply it to the benchmark class, it's container class (if the benchmark class is nested) or to the benchmark's assembly:

```c#
	// A perf test class.
	[MyCompetition]
	// -or-
	[assembly:MyCompetition]
	public class SimplePerfTest
	{
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);
	}
```

First found config attribute wins.

### 2. Declarative approach. Use competition features

It should be obvious for now that CodeJam.PerfTests has very complex configuration system. At the same time most end-user use cases are very simple. You may want to enable/disable source annotations or specify target platform or just enable troubleshooting mode. You do not want to know anything about the configs or what properties should be changed to enable particular scenario.

