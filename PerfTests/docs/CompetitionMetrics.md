# CodeJam.PerfTests Metrics

> **META-NOTE**
>
> Places to update are marked with *--…--*.

> **DRAFT**
>
> This is draft version of document containing things not implemented yet or partially not working.
>
> Ignore it if you're not me.

[TOC]

Competition metrics system enables various measurements to be used for competition: timings, memory, IO,  etcs. Rest of document describes metrics subsystem design and provides information about adding your own metrics.



# Metrics basics

Draft design decisions:

* Each metric is in effect a typed pair of double values representing min / max value of the metric combined with unit of measurement used to format the values. One (or both) of values can be infinite or there can be no metric stored at all (empty metric). No additional properties allowed.
* There can be only one instance of metric of each type per competition, duplicates will result in `Error`/`Warning` messages.
* List of metrics to use is obtained via `ICompetitionConfig.GetMetrics()`. Addition of third-party metrics can be performed via `CompetitionModifierAttribute`. 
* Each metric are described via instance of `ICompetitionMetricInfo`. The metric info is the key type of entire metric subsystem that provides access to
  * Storage information (attribute type/xml node name, metric scale etc)
  * Kind of metric: absolute/relative
  * Metric statistics that calcs mean/stdev/ranges of actual/limit values
  * Metric values provider that provides array of double - measured values.
* Competition metrics can be stored as attribute annotations or as XML annotations. Either way there is one-to-one mapping between attribute type and the `ICompetitionMetricInfo`.
* If there are annotations not matching to metrics in `ICompetitionConfig`, they are ignored.
* During the run metrics are calculated on per-benchmark basis. However, actual limits are stored per-target. If there are multiple benchmarks for the target, limits are merged together and are stored as a single continuous range.
* Coefficient scales: see below



## Coefficient scales:

Measurement scale stores ordered list of pairs: key name + coefficient. These are described via enums like this:

```c#
enum TimeScale
{
  [ScaleItem("ns")]
  Nanosecond = 1,
  [ScaleItem("us")]
  Microsecond = 1000 * Nanosecond,
  [ScaleItem("ms")]
  Millisecond = 1000 * Microsecond, 
  [ScaleItem("s")]
  Second = 1000 * Millisecond,  
}
```

Scale coefficient is stored as enum value, name - as enum name (or as attribute value)

Coefficient to use is determined by highest (to) limit value (or by lowest if the to value is infinite). Competition uses max coefficient value that is less than the abs(limit value) (or the first scale value if none found)

Coefficients are used only for storage and for display values

## Examples:

```c#
class Annotated
{
  [CompetitionBaseline]  
  [Memory(1.33, 2.80, Binary.Kb), Duration(0.7, 3.99, Time.Sec), SqlQueriesTime(0), IOTime(0)]
  void ImplA() {}
  
  [CompetitionBenchmark(10, 30)]
  [Memory(1.33, 2.80, Binary.Kb), Duration(7.0, 119.7, Time.Sec), SqlQueriesTime(0), IOTime(0)]
  [MemoryRelative(0.9, 1.1), SqlQueriesRelative(0)]
  void ImplB() {}
}
```

Xml:

```xml
<Competition Target="Annotated">
	<ImplA Baseline="true">
		<Memory Min="1.33" Max="2.80" Unit="Kb" />
		<Duration Min="0.7" Max="3.99" Unit="s" />
		<SqlQueriesTime Max="0" />
		<IOTime Max="0" />
	</ImplA>
	<ImplB Min="10" Max="30" >
		<Memory Min="1.33" Max="2.80" Unit="Kb" />
		<Duration Min="7.0" Max="119.7" Unit="s" />
		<SqlQueriesTime Max="0" />
		<IOTime Max="0" />      
		<MemoryRelative Min="0.9" Max="1.1" />
		<SqlQueriesRelative Max="0" />
	</ImplB>
</Competition>
```
