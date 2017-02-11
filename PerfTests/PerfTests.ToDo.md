## TODO, Bench.Net:
 * Support for `IFlushable.Flush()`.
 * No file logger for tests https://github.com/dotnet/BenchmarkDotNet/issues/198
 * Run state for issues like this. https://github.com/dotnet/BenchmarkDotNet/issues/180
 * ReadOnlyConfig
 * ManualConfig.Add - backport ManualCompetitionConfig design
 * remove calls instance.setupAction(), instance.targetAction() from BenchmarkProgram.txt
   https://github.com/dotnet/BenchmarkDotNet/issues/184 ?

## TODOs:

### Types:
 * IStoredMetricSource: add MetricName attribute, EnumName attribute => simplify parse logic as merging will be moved to the analyser.
   +Subtask: Attribute annotation: analyse existing overloads (add string arg to IStoredMetricSource???)
 * ??? Naming, IStoredMetricSource.MetricAttributeType => AttributeType, IMetricValuesProvider => IMetricProvider
 * public metric atttributes etc => move to AllTogether level
 * Namespace 4 attributes => CodeJam.PerfTests.
 * !!! dump CompetitionOptions to summary (as columns)
 * BurstModeEngineFactory: sync with code in BDN
 * HACK: Remove console capture after update to BDN 10.3
 * TargetCacheKey: remove?
 * Sealed types: unseal where it looks like a good idea.
 * Simplify collections of types. MappedCollection / Keyed collection where possible.

### Behavior: 
 * Revisit caching methods for analyser (all those members should contain `cache` in name).
 * Add cache invalidation policy (limit of 1000)
 * xUnit: improve capture of console output. Pass xUnitWriter as a logger instead.
 * Auto annotate empty feature
 * Metric annotation: no unit for default unit of measurement, to enable [GcAllocations(0)]
 * Diagnosers: faster options for diagnosers run??
 * Variance for SingleValueMetricCalculator / PercentileMetricCalculator
 * Optional relative time metric - better diagnostic if there's not empty attribute for it but the metric is not listed in config.
 * Check Code.BugIf assertions. Replace with messages where possible.
 * Metric columns: variance only on troubleshooting mode? Create a metric column provider? Add arg for MetricValuesProvider.GetColumnProvider?
 * Concurrency: lock should be performed on entire benchmark run.
 * Rerun if sources were adjusted from log?
 * Prev run log : cache + reread only if local & size/date/checksum(?) changed!!!
 * Display-only metrics (only as columns? .ctor arg to the metric? bad as will silently remove as duplicates (dups removed by AttributeType)

### Messages:
 * Validate all messages, check that origin (benchmark target) is logged, add hints (tyed arg) to them.
 * Add typed method to force same naming for source of the messages (Target, Type, xmlFile etc).
 * Check WriteVerboseHint for source annotations
 * Write hint with absolute values if relative limits failed? 
   (will require bool arg for metricValuesProvider, recheck columns and diagnosers, possible, they will have to honor this flag too)).
 * Message about updated annotations: improve readability

### Logging:
 * Add advanced diagnostic for config-adjusting-time (logger as an arg of ConfigFactory + fix methods).
 * Logging: write validator messages immediately?
 * Print resulting competition options as common columns after https://github.com/dotnet/BenchmarkDotNet/pull/341
 * Check `+ Environment.NewLine` usages in `XunitCompetitionRunner.ReportXxx()` methods
 * LogColors.Hint: use it for something?

### Tests:
 * Source annotations: test for partial files / methods
 * High-priority test for TestProcessCycleTimeClock
 * Tests for broken log annotations.
 * app.config in the test integration projects: do we need it?
 * xUnit: tests: force run as x64 (appveyor may run as x86)?
 * Test for standard analyser warnings - are they logged as messages?
 * out of process test
 * local test for run under CI (CI mode on) (+ 1!)
 * Test for bad encoding
 * Memory limits + diagnoser - test for accuracy

## Cleanup
 * Remove and re-add resharper suppressions

## Long-term TODOs:
 * Support for multi-case benchmarks (separate limits - discarded)
 * Validate the return results!!! (partially done with introduction of IHostApi)
 * Support for concurrent competiton runs (stub code were removed at master afd9977, restore, then fix).
 * replace LooksLikeLastRun property usages with some extension point that should run on competition test completion

## Issues:
https://github.com/dotnet/BenchmarkDotNet/issues/324
https://github.com/dotnet/BenchmarkDotNet/issues/319
https://github.com/dotnet/BenchmarkDotNet/issues/307
https://github.com/dotnet/BenchmarkDotNet/issues/234
https://github.com/dotnet/BenchmarkDotNet/pull/341

https://github.com/nunit/nunit-console/issues/62#issuecomment-262599181
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
 * BenchmarkActionFactory - codegen free benchmark runner factory (should work for .Net Native too).

### Layer 2: RunState
 * Helper to store state during benchmark run.

### Layer 3: CompetitionsCore
 * Core logic & api for competition runs.
 * Competition options
 * Limit range & limit providers
 * Competition attributes
 * Competition state (api to be used during competition run)
 * Messages to be reported to user
 * CsvTimingsExporter
 * HostLogger (log filtering)

### Layer 4: CompetitionAnalyser
 * Analyser to check the limits.
 * Limit columns
 * API to read source annotations

### Layer 5: CompetitionAnnotateAnalyser
 * Extended version of analyser from L4 with ability to annotate sources from actual running results

### Layer 6: Configuration
 * Base CompetitionConfig APIs and attributes

### Layer 7: Reusable parts of the runners
 * Cpmpetition runners & helpers.


##Long-term task: reusable limits, draft notes
* Support for third-party limits, use limit provider + id
* Target stores limits as a `Dictionary<provider_id, LimitRange>`
* Limit provider specifies attribute name and additional parameters to be applied
  TODO: exact format?
  TODO: Use same properties for XML annotations or prefer something better?
  TODO: Range extension method: Min/MaxValue to infinity?