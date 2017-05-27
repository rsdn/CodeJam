# CodeJam.PerfTests overview

> **META-NOTE**
>
> Places to update are marked with *--…--*.

> **IMPORTANT**
>
> This document assumes you're familiar with [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read the link first.

[TOC]

## PerfTest terminology

Each perftest has some key parts, listed in comments:

```c#
	// Competition config modifier attribute.
	// Check Configuration system section for more
	[CompetitionBurstMode] 
	// A perf test class.
	// Contains runner method, baseline, implementation, setup and cleanup methods.
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
		// As an option, the runner method may be located outside of the per test class.
		// Use case for it: you may locate related perftests classes as nested classes
		// and leave runner methods for them in a container class.
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// Marks method as a Baseline competition member. 
		// Other competition members will be compared with this.
		[CompetitionBaseline]
         // GC Allocations metric annotation. Assumes that the method does not allocate.
		[GcAllocations(0)]
		public void Baseline() => Thread.SpinWait(Count);

		// Marks method as a competition member.
		// Also acts as annotation for a relative execution time metric.
		// Metric limit is [2.96..3.08] compared to the baseline.
		[CompetitionBenchmark(2.96, 3.08)]
		// Same as above, no allocations. Short form of annotation.
		[GcAllocations(0)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);

		// Marks method as a competition member. ~5x compared to the baseline.
		[CompetitionBenchmark(4.94, 5.14)]
		// Same as above, full form of annotation that includes unit of measurrement
		[GcAllocations(0, 0, BinarySizeUnit.Gigabyte)]
		public void SlowerX5() => Thread.SpinWait(5 * Count);

		// Another member, ~7x to baseline, no allocations, all attributes in a single line.
		[CompetitionBenchmark(6.89, 7.17), GcAllocations(0)]
		public void SlowerX7() => Thread.SpinWait(7 * Count);
	}
```



## Compared to BenchmarkDotNet

The main thing introduced by CodeJam.PerfTests is a concept of _competition_. Competition classes are pretty match the same BenchmarkDotNet benchmark classes; they contain benchmark methods, setup/cleanup methods and may be annotated with config attributes to modify default setup. The main difference is, there are additional subsystems that ease usage of the BenchmarkDotNet as a performance tests runner.

### Competition metrics

CodeJam.PerfTests allows to collect, store and validate a bunch of performance metrics for each member of competition. Out of the box there are metric value providers for execution time, memory allocations, GC calls and so on. Also there's public API for adding third-party metric value providers (*--not stable; not documented yet--*) so if you're missing something (SQL execution time? network IO?) you'll be able to measure it.

Some of providers do support so-called relative metrics: instead of reporting absolute values results are scaled to baseline competition member. Relative metrics are especially useful when absolute metric values highly depends on environment or hardware and therefore may vary from run to run. A great example of such metric is execution time. Default config does not ensure absolute execution time at all. Instead of this competitions use Scaled metric that exposes execution time of the member compared to the baseline member. 

> **ITS MATH TIME**
>
> The Scaled metric provided by CodeJam.Perftests assumes that timing measurements do belong to lognormal distribution. In contrast to normal distribution model used in BenchmarkDotNet the former one provides more accurate results on measurements with long tail distribution.
>
> Explanation for humans: the results from Scaled columns in the output of BenchmarkDotNet and CodeJam.PerfTests may differ and its fine.

If competition uses a relative metric but there's no baseline member in competition the perftest will fail with

```
No baseline method for benchmark. Apply CompetitionBaselineAttribute to the one of benchmark methods.
```

message.

List of metrics used in competition run can be set up via configuration API, see "Configuration system" section. 

### Competition runner

There are `Competition.Run()` methods that should be used instead of BenchmarkDotNet's `BenchmarkRunner.Run()` method. This is required because CodeJam.Perftest extends default benchmark execution logic in a miltiple ways. As example, single competition run may include multiple benchmark runs performed after metric limit adjustment or simply to ignore occasionally out-of-limit runs that are common on cloud VMs or on a low-end hardware. 

In fact, there are multiple competition runner classes. There's one for each test framework supported and also there is a `ConsoleCompetition` runner that allows to run perftests as a console app. All runners do expose same public API so you can switch from one test framework to another by changing a reference to the unit-test-integration assembly (as example, from `CodeJam.PerfTests.NUnit` to `CodeJam.PerfTests.xUnit`).

### Source annotations

Each member of competition should be annotated with expected values for each metric used in competition. By default metric limits are performed via c# attributes but there may be other metric storage providers. Out of the box there's XML annotation provider that allows to store source annotations as embedded XML resources. *--Future releases--* will include public API for third-party storage providers to enable advanced scenarios such as storing results from all runs in a database.

The annotations may be applied by developer or added (and updated) automatically depending on competition configuration. Check [Source Annotations](SourceAnnotations.md) document for more.

### Configuration system

> **WARNING**
>
> Please do not use the BenchmarkDotNet config attributes for competitions. As it is for now they are unsupported and result behavior is undefined.

CodeJam.PerfTests uses own configuration system that allows to reuse custom configuration on solution-level or assembly-level and alter it on multiple levels down to perf test class.

Almost always there's no need to perform any setup as CodeJam.PerfTests is designed to work out-of-the-box. In case the defaults are not good for you, most frequently used features may be turned on or off via `CompetitionFeaturesAttribute` (or derived attributes) like this:

```c#
	// Enable the source annotations feature
	[CompetitionFeatures(AnnotateSources = true)]
	// Or, the same:
	[CompetitionAnnotateSources]
	public class SimplePerfTest
	{
		// ...
	}
```

If you want to have full control over configuration there's advanced API available. Read [Configuration System](ConfigurationSystem.md) document for additional information.



## Limitations

### Accuracy of measurements

By default CodeJam.PerfTests is set up to perform very fast perftest runs, average timing for each test is seconds, not minutes. 

This is critical if you have a several hundreds or thousands of perftest as no one wants to wait several hours for tests completion. Also, these quick-snap results are closer to what you will see in production as measured metrics do include worst-case runs, in contrast to clean-room conditions created by default BenchmarkDotNet configuration. Remember, perftests are not about accuracy, they are about to proof your code will not break performance contract described as a set of metric annotations.

If the default behavior is not what you want you may apply your own competition config on solution or assembly level. Check the "Configuration system" section for more.

### Out-of-proc perftests

By default all perftests are run in-process as this allows to speedup test execution. *--As it is for now there's no user-friendly API--* to run the perftest out-of-process. As a workaround you may add a reference to the [BenchmarkDotNet.Toolchains.Roslyn](https://www.nuget.org/packages/BenchmarkDotNet.Toolchains.Roslyn/) package, add the following config modifier:

```c#
	public class CompetitionOutOfProcessAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig config)
			{
				// Using default BenchmarkDotNet job
				// as PerfTests's one does not work well with out-of-process runs.
				config.Jobs.Clear();
				config.Jobs.Add(Job.Default.With(new RoslynToolchain()));

				// Same level of output that is used by BenchmarkDotNet
				config.ApplyModifier(new CompetitionOptions()
				{
					RunOptions = { DetailedLogging = true }
				});
			}
		}

		public CompetitionOutOfProcessAttribute() : base(typeof(ModifierImpl)) { }
	}
```

and then apply the attribute to the perftest class or to the entire assembly. 

Note that modifier replaces job supplied by CodeJam.PerfTests with default one from BenchmarkDotNet. This is done because we currently do not provide any good job settings for out-of proc toolchain.

### Multi-case perf tests.

CodeJam.PerfTests do not support configurations that uses multiple jobs and report inaccurate metric values for configs with multiple parameter values. It is not a thing to be fixed in near future as there is no easy way to provide maintainable source annotations for such benchmarks.

> **EXPLANATION**
>
> Let's say we have `[Params(SomeEnum.Slow, SomeEnum.Fast)]` and a config with two jobs (with forced GC and without GC Force option).
>
> There are four possible combinations so annotation for each method in competition will look like this:
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
> Current implementation will merge annotations from example above into
>
> ```c#
> [CompetitionBenchmark(2.35, 15.45)]
> [GcAllocations(0.41, 1.25, BinarySizeUnit.Kilobyte]
> public void SomeMethod() => ...
> ```
>
> Not a best solution but at least it does not tease your brain with "What values should I rely on?".
>
> If you want to do detailed investigation on multiple cases, consider to use raw BenchmarkDotNet benchmark.
>
> Have a idea how to make it better? Please, [create issue for it](https://github.com/rsdn/CodeJam/issues)!

### BenchmarkDotNet configuration attributes

CodeJam.PerfTests uses own configuration system that is incompatible with BenchmarkDotNet configuration attributes. *--This may change in future--* but until this is fixed please do not use the BenchmarkDotNet config attributes for competitions. As it is for now they are unsupported and result behavior is undefined.

>  **EXPLANATION**
>
>  The competition infrastructure requires the config to be created and all config modifiers to be applied exactly one time before the competition is actually run. This is a principal design decision that cannot be changed or there will be a risk of accidentally breaking the config in a way that cannot be easily detected. We had such experience and it was very disappointing to find some of our tests were reporting false postives for a month or so.
>
>  *--This may be fixed on BenchmarkDotNet side and we're going to do a PR for it some time later--*.