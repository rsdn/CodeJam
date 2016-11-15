## TODO, Bench.Net:
* Support for `IFlushable.Flush()`.
* No file logger for tests https://github.com/PerfDotNet/BenchmarkDotNet/issues/198
* Run state for issues like this. https://github.com/PerfDotNet/BenchmarkDotNet/issues/180
* ReadOnlyConfig
* ManualConfig.Add - backport ManualCompetitionConfig design
* Validators: access to config, in-process validators
* remove calls instance.setupAction(), instance.targetAction() from BenchmarkProgram.txt
  https://github.com/PerfDotNet/BenchmarkDotNet/issues/184 ?

## TODOs:
 * Tests for broken log annotations.
 * Task/Task<T> support.
 * detailing logging - fix toolchain
 * Test for standard analyser warnings - are they logged?
 * Logging: write validator messages immediately?

## Long-term TODOs:
 * Support for concurrent competiton runs (stub code were removed at master afd9977, restore, then fix).
 * replace LooksLikeLastRun property usages with some extension point that should run on competition test completion
 * Memory limits + diagnoser - whoops, https://github.com/PerfDotNet/BenchmarkDotNet/issues/200 . Will need to replace MethodInvoker, delayed.

## Issues:
https://github.com/PerfDotNet/BenchmarkDotNet/issues/234
https://github.com/nunit/nunit/issues/668
https://github.com/nunit/nunit/issues/1586
https://github.com/xunit/xunit/issues/908

## layered design: Bench.NET part

### Layer 0: Bench.Net helpers
 * Simple API fixes / additions

### Layer 1: In-process toolchain
 * Allows to run benchmark in process
 * Includes validator to proof that current process matches the job.

### Layer 2: RunState
 * Helper to store state during benchmark run.

### Layer 3: CompetitionsCore
 * Core logic & api for competition runs.
 * Ability to rerun benchmark
 * Messages to be reported to user

### Layer 4: CompetitionLimits
 * Classes to describe the limits of the benchmark methods.
 * Analyser to check the limits.

### Layer 5: CompetitionLimits + Annotations
 * Extended version of analyser from L5 with ability to annotate the source from actual running results

### Layer 6: Reusable parts of the runners
 * Wrapping all of above into simple, configurable and reusable API

### Layer 7: NUnitTestRunner (To be paired with xUnit)
 * public API + reporting messages as nUnit errors / warnings.


## NEW opt-in feature: annotations from log

Use case: we use very different hardware to run the unit-tests, from tablets to appveyor buildservers.
And of course, the perftests' results are depend on hardware. So, we need a way to collect the results and to update perftest limits with them.

Initial plan looks like this:

1. Dump the CompetitionTargets on last run. Dump only ones that have been updated and dump only on last run.
It looks like a job for CompetitionAnnotateAnalyser
2. On the run - add ability to specify log source to merge annotations from
CompetitionAnnotateAnalyser again
3. After that all works like usual.

The dump uses same format that xml anotations uses. The only difference is,
the Target uses full assembly qualified name of the type, not type name only.
We should to it because there can be a multiple perftests with same type name but in different namespaces / assemblies in the log.

UPD: Done just as planned