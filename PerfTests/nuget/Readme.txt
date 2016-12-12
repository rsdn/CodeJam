CodeJam.PerfTests Release Notes
-------------------------------

# 0.0.11-beta
* Annotation fails on damaged/invalid encoding files.
* Auto-adjust limit scale for annotations (more decimal places for too small values).

# 0.0.10-beta
* Doc files complete. First public preview.

# 0.0.9-alpha
* Public API improvements: allow to pass CompetitionFeatures as an arg to `Competition.Run()`.
* Test output: readability improved.
* CompetitionPreviewLimitsAttribute added.

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
