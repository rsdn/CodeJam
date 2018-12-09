CodeJam.PerfTests Intro

Please check TBD for intro and samples

Release Notes

-------------------------------
# 0.2.0-beta
* Update BenchmarkDotNet dependency to 0.10.9
* Experimental ETW metrics support. Use CodeJam.PerfTests.Etw package
* .NET Core 2 support.

# 0.1.4-beta
* First beta release. Docs actualised, improvements for diagnostics and log output.

# 0.1.1-beta
* Update BenchmarkDotNet to fix false allocation detection

# 0.1.0-beta
* Major rewrite of perftests that enables new features.
* New: custom metrics API.
* New: Gc allocations metric.
* New: hints for common warnings / errors.
* Upd: default output is simplified.
* Upd: In-process benchmarks accuracy improved.

# 0.0.11-beta
* Annotation fails on damaged/invalid encoding files.
* Auto-adjust limit scale for annotations (more decimal places for too small values).

# 0.0.10-beta
* Doc files. First public preview.

# 0.0.9-alpha
* Public API improvements: allow to pass CompetitionFeatures as an arg to `Competition.Run()`.
* Test output: readability improved.
* CompetitionPreviewMetricsAttribute added.

# 0.0.8-alpha
* Minor improvements for idle measurement accuracy.

# 0.0.7-alpha
* Better handling of empty limits, project switched to VS 2017 RC.

# 0.0.6-alpha
* Futher imporvements for measurement accuracy and support for Task-returning benchmarks.

# 0.0.5-alpha
* Default config do not require for loops in the body of perftest methods.
* Runs under CI are detected automatically and settings are adjusted for CI runs.

# 0.0.4-alpha
* CompetitionConfig APIs: annotation using attributes.

# 0.0.3-alpha
* BurstModeEngine, ThreadCycles/ProcessCycles clocks added.
* CompetitionConfig APIs: major rewrite & simplification.
* Use defaults from BenchmarkDotNet where possible.
* Remove dependency on Microsoft.DiaSymReader package

# 0.0.2-alpha
* initial nuget package
