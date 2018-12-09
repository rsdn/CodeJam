## TODO, Bench.Net:
 * Support for `IFlushable.Flush()`.
 * No file logger for tests https://github.com/dotnet/BenchmarkDotNet/issues/198
 * Run state for issues like this. https://github.com/dotnet/BenchmarkDotNet/issues/180
 * ReadOnlyConfig
 * ManualConfig.Add - backport ManualCompetitionConfig design
 * remove calls instance.setupAction(), instance.targetAction() from BenchmarkProgram.txt
   https://github.com/dotnet/BenchmarkDotNet/issues/184 ?

## TODOs:

### TODO now:
* Update ReadMe & docs
* check metric if no annotations (test succeed???)
* check exception if stored metrics are invalid (min > max)
* Cache metric values (same as annotation context but for parsing the anotations).
* Output contains messages printed under `*** Warnings ***` section. Have no idea what to do with it.
  Need non-printable analyzer warnings :)
* Config modifier attributes. Do some thing with names and discoverability. May be some naming convention for modifiers?

### Types:
 * ??? Rename Xxx annotations to metric annotations? Not sure, may be a bad idea if we're going to store something except metrics.'
 * Sealed types: unseal where it looks like a good idea.
 * Simplify collections of types. MappedCollection / Keyed collection where possible.
 
### Validations:
 * No Benchmark / Setup attribute duplication

### Behavior:
 * Annotation adjustments: options for metric unt selection: preserve / autoscale; per-metric adjustment?
 * Future versions of BDN: each member in competition may have its own target type (multiple targets for single run). Ensure that our code is prepared to it.
 * "Cannot detect encoding" checks for anything that reads streams as text?
 * Concurrency: lock should be performed on entire benchmark run.
   + Subtask: Support for concurrent competiton run s (stub code were removed at master afd9977, restore, then fix). Use case: load testing.
   + Subtask: concurrent runs. Disallowed as it will break concurrent file annotations
 * metric info: flags enum of column kinds to add into summary
 * Rule to ignore individual metric annotations (do not annotate / current only etc?)
 * Merge failed & adjusted messages for multiple benchmarks into single message?
 * ??? Integer metrics without unit scale (for total GC count)
 * Revisit caching methods for analyser (all those members should contain `cache` in name).
 * Add cache invalidation policy (limit of 1000)
 * ??? Diagnosers: faster options for diagnosers run?
 * Variance for SingleValueMetricCalculator / PercentileMetricCalculator
 * Optional relative time metric - better diagnostic if there's not empty attribute for it but the metric is not listed in config.
 * Check Code.BugIf assertions. Replace with messages where possible.
 * Rerun if sources were adjusted from log?
 * Prev run log : cache + reread only if local & size/date/checksum(?) changed!!!
 * Display-only metrics (only as columns? .ctor arg to the metric? bad idea as these will be removed silently as duplicates (dups removed by AttributeType)

### Messages:
 * 'No logged XML annotation for Test00Baseline found. Check if the method was renamed.' => add metric names to message
 * Validate all messages, check that origin (benchmark target) is logged, add hints to them.
 * Add typed method to force same naming for source of the messages (Target, Type, xmlFile etc).
 * Check WriteVerboseHint for source annotations
 * ??? Write hint with absolute values if relative limits failed? (discarded)
   (will require bool arg for metricValuesProvider, recheck columns and diagnosers, maybe they will have to honor this flag too)).
 * Message about updated annotations: improve readability
 * Message about ignored empty metrics: improve readability

### Logging:
 * Add advanced diagnostic for config-adjusting-time (logger as an arg of ConfigFactory + fix methods).
 * Check `+ Environment.NewLine` usages in `XunitCompetitionRunner.ReportXxx()` methods
 * LogColors.Hint: use it for something?

### Tests:
 * xUnit: tests: force run as x64 (appveyor may run as x86)?
 * Source annotations: test for partial files / methods
 * High-priority test for TestProcessCycleTimeClock
 * Tests for broken log annotations.
 * app.config in the test integration projects: do we need it?
 * Test for standard analyser warnings - are they logged as messages?
 * out of process test
 * local test for run under CI (CI mode on) (+ 1!)
 * Test for bad encoding
 * Memory limits + diagnoser - test for accuracy
 * Ensure that unknown unit in prev run log results in warning
 * Ensure that all attribute lines are read during source file parsing.
 * Validate the return results!!! (partially done with introduction of IHostApi)

### Cleanup
 * Remove and re-add resharper suppressions

### Features to test & to document
* FAQ (minimum metric values)
* Gc metrics (byte-precise allocation monitoring, gc collection metrics)

## Long-term TODOs:
* Attach detailed reports to tests

## Issues:
https://github.com/dotnet/BenchmarkDotNet/issues/361
https://github.com/dotnet/BenchmarkDotNet/issues/360
https://github.com/dotnet/BenchmarkDotNet/issues/327
https://github.com/dotnet/BenchmarkDotNet/issues/324
https://github.com/dotnet/BenchmarkDotNet/issues/136

https://github.com/xunit/xunit/issues/908

## DOCS:
* guidleines for use cases:
  - dead code elimination : use prev value
  - relatively cost action such as method invocation: use arrays.


## layered design: Bench.NET part

### Layer 0: things to port to BDN
 Thats it.

### Layer 1: Common competition api to be used by end customers and by the competition itself.
 Options, metrics, various helpers.

### Layer 2: CompetitionCore
 Core api to be used during competition run. Not meant to be exposed to end users but is available for third-party extensions. Icludes logging and messages.

### Layer 3: Competition analysers
 Competition metric related things: analysers and annotation storages.

### Layer 4: Configuration
 Configuration subsystem & predefined implementations of exporters, metric providers etc.

### Layer 5: All together
 Reusable competition run API.


## Long-term plans for v2
 * Advanced metrics (proof that api is done with package that uses GLAD)
 * Backup for auto-annotations
 * Make annotations storage API public. Use case: to store them as a database file
   + ??? Subtask: exporter-only version to collect actual metric values
   + ??? Advanced subtask: collect raw metric values (discarded, too much data without a gain).
 * Support for multi-case benchmarks (discarded)