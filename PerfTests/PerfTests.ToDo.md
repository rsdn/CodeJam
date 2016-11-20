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
 * `Task`/`Task<T>` support.
 * detailing logging - fix toolchain
 * Test for standard analyser warnings - are they logged as messages?
 * Logging: write validator messages immediately?
 * Source annotations: test for partial files / methods
 * CompetitonOptions: predefined
 * xUnit: tests: run as x64?
 * out of process test
 * concurrency: lock should be performed on entire benchmark run.
 * high-priority test for TestProcessCycleTimeClock
 * app.config in the test integration projects: do we need it?
 * Log resulting competition features / competition options?
 * LogColors.Hint: use it for something?
 * better message for "X has empty limit. Please fill it."
 * Test for run under CI

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
