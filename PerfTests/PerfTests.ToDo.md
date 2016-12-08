## TODO, Bench.Net:
 * Support for `IFlushable.Flush()`.
 * No file logger for tests https://github.com/dotnet/BenchmarkDotNet/issues/198
 * Run state for issues like this. https://github.com/dotnet/BenchmarkDotNet/issues/180
 * ReadOnlyConfig
 * ManualConfig.Add - backport ManualCompetitionConfig design
 * Validators: access to config, in-process validators
 * remove calls instance.setupAction(), instance.targetAction() from BenchmarkProgram.txt
   https://github.com/dotnet/BenchmarkDotNet/issues/184 ?
 * https://github.com/dotnet/BenchmarkDotNet/issues/307

## TODOs:
 * Check order provider usage, execution order benchmarks vs display order benchmarks.
 * Warning if baseline was changed!!!
 * Warning if there's log uri set, but source annotation updates are disabled.
 * Introduce logging for config factory
   -or-
   Run source annotations analyser even if annotations are disabled
   -or-
   Check on preconditions (bad idea, leaking abstraction)
 * Check WriteVerboseHint for source annotations
 * Check `+ Environment.NewLine` usages in `XunitCompetitionRunner.ReportXxx()` methods
 * Skip annotation on 1st run (CI mode only?)
 * Exclude nunit-related assembly from tests
 Prepare PR, https://github.com/nunit/nunit-console/issues/62#issuecomment-262599181
 * Message with absolute timings if limits failed: improve readability
 * Message about updated annotations: improve readability
 * Burst mode feature: rename to LargeSampleSet?
 * AnnotateSourcesOnRun: rename to something like skipFirstRuns
 * Output: option to not log output from toolchain?
 * Concurrency: lock should be performed on entire benchmark run.
 * Logging: write validator messages immediately?
 * Log resulting competition features / competition options?
 * LogColors.Hint: use it for something?
 * Better message for "X has empty limit. Please fill it." + do not rerun if empty limit
 * Better message for "run faster / slower than". Provide some suggestions?
 * Warning if job count > 1
 * Apply with id for Competition options / features
 * Metadata attributes - order by inheritance
 * Limit annotations: remove special values (Zero and -1)

## TODOs (tests):
 * Source annotations: test for partial files / methods
 * High-priority test for TestProcessCycleTimeClock
 * Tests for broken log annotations.
 * app.config in the test integration projects: do we need it?
 * xUnit: tests: run as x64?
 * Test for standard analyser warnings - are they logged as messages?
 * out of process test
 * Test for run under CI

## Long-term TODOs:
 * Support for multi-case benchmarks (separate limits)
 * Validate the return results!!!
 * Support for concurrent competiton runs (stub code were removed at master afd9977, restore, then fix).
 * replace LooksLikeLastRun property usages with some extension point that should run on competition test completion
 * Memory limits + diagnoser - whoops, https://github.com/dotnet/BenchmarkDotNet/issues/200 . Will need to replace MethodInvoker, delayed.

## Issues:
https://github.com/dotnet/BenchmarkDotNet/issues/307
https://github.com/nunit/nunit-console/issues/62#issuecomment-262599181
https://github.com/dotnet/BenchmarkDotNet/issues/234
https://github.com/nunit/nunit/issues/668
https://github.com/nunit/nunit/issues/1586
https://github.com/xunit/xunit/issues/908

## layered design: Bench.NET part

### Layer 0: Bench.Net helpers
 * Simple API fixes / additions

### Layer 1: In-process toolchain
 * Allows to run benchmark in process
 * Includes validator to proof that current process matches the job.
 * Adds BurstModeEngine for more stable results

### Layer 2: RunState
 * Helper to store state during benchmark run.

### Layer 3: CompetitionsCore
 * Core logic & api for competition runs.
 * Competition options
 * Competition limits, limit providers, limit columns
 * Competition attributes
 * Competition state (api to be used during competition run)
 * Messages to be reported to user
 * CsvTimingsExporter
 * HostLogger (log filtering)

### Layer 4: CompetitionAnalyser
 * Analyser to check the limits.
 * API to read source annotations

### Layer 5: CompetitionAnnotateAnalyser
 * Extended version of analyser from L4 with ability to annotate the source from actual running results

### Layer 6: Configuration
 * Base CompetitionConfig APIs and attributes

### Layer 7: Reusable parts of the runners
 * Wrapping all of above into simple, configurable and reusable API


##Long-term task: reusable limits, draft notes
* Support for third-party limits, use limit provider + id
* Target stores limits as a `Dictionary<provider_id, Range<double>>`
* Limit provider specifies attribute name and additional parameters to be applied
  TODO: exact format?
  TODO: Use same properties for XML annotations or prefer something better?
  TODO: Range extension method: Min/MaxValue to infinity?