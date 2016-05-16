## TODO
 * Memory limits + analyser
 * Fix analyser support in in-process toolchain
 * validate culture usage
 * validate null values on percentiles
 * ValidatePreconditions / postconditions - throw => warnings.
 * Fix inProcessValidator (does not run).
 * Fix messages format.
 * Fix log write lines.


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
 * Extended version of analyser from L5 wit ability to annotate the source with actual limits

### Layer 6: Reusable parts of the runners
 * Wrapping all of above into simple, configurable and reusable API

### Layer 7: NUnitTestRunner (To be paired with xUnit)
 * public API + reporting messages as nUnit errors / warnings.
