# CodeJam.PerfTests Source Annotations

> **META-NOTE**
>
> Places to update are marked with *--…--*.

[TOC]

Source annotations allows to set metric limits for competition members. By default metric limits are applied as c# attributes  but there may be other metric storage providers. Out of the box there's XML annotation provider that allows to store source annotations as embedded XML resources. *--Future releases--* will include public API for third-party storage providers to enable advanced scenarios such as storing results from all runs in a database.





## Annotate sources using attributes

Each competition methic comes with attribute (`[ExpectedTime]` for measuring expected execution time, `[GcAllocations]` for guess what and so on). All of the attributes have constructor overloads with same signatures. Most full one allows to specify the minimum limit value, the maximum limit value and a scaling factor (unit of measurement) applied to both min and max limits. The scaling factor is omitted if the measurements are dimensionless. As example,

```c#
[CompetitionBaseline]
[ExpectedTime(2.4, 2.8, TimeUnit.Millisecond)] // takes 2.4..2.8 ms to run
public void DoSomething() { ... }

[CompetitionBenchmark(950, 1050)]         // ~1000x slower, the metric value is dimensionless
[ExpectedTime(2.4, 2.8, TimeUnit.Second)] // takes 2.4..2.8 second to run
public void DoSomethingElse() { ... }
```

If you want to skip setting the min or the max limits you may pass `double.NegativeInfinity` / `double.PositiveInfinity` respectively. For convenience these constants are exposed as a members of `MetricRange` type:

```c#
// takes at least 2.4 ms to run
[CompetitionBenchmark]
[ExpectedTime(2.4, MetricRange.ToPositiveInfinity, TimeUnit.Millisecond)]
public void DoSomething() { ... }

// takes no more than 2.8 us to run
[CompetitionBenchmark]
[ExpectedTime(MetricRange.FromNegativeInfinity, 2.8, TimeUnit.Microsecond)]
public void DoSomething() { ... }
```

Most of metrics allows to omit minimum limit value, so this one will work too:

```c#
// takes no more than 2.8 us to run
[CompetitionBenchmark]
[ExpectedTime(2.8, TimeUnit.Microsecond)]
public void DoSomething() { ... }
```

The exception of this rule are high precision metrics such as GC Allocation metric. They assume single-value limit denotes both min and max boundaries:

```c#
// Allocates EXACTLY 48 bytes
[CompetitionBenchmark]
[GcAllocation(48, BinarySizeUnit.Byte)]
public void PleaseAllocate42Bytes() { ... }

// Allocates EXACTLY 1 MB
[CompetitionBenchmark]
[GcAllocation(1.0, BinarySizeUnit.Megabyte)]
public void PleaseAllocate1Mb() { ... }
```

Finally, if the scaling factor is 1 (bytes for BinarySizeUnit, nanoseconds for TimeUnit and so on) the measurement unit arg may be omitted too:

```c#
// Allocates EXACTLY 48 bytes
[CompetitionBenchmark]
[GcAllocation(48)]
public void PleaseAllocate42Bytes() { ... }
```

Please note that all metrics you do want to measure should present in collection returned by `ICompetitionConfig.GetMetrics()` method. If you run perftest with some metric applied as an attribute but not listed in config you'll get the following message:

```
// Metric ExpectedTime is not listed in config and therefore is ignored.
// ! Hint: List of metrics is exposed as ICompetitionConfig.GetMetrics().
```

Read [Configuration System](ConfigurationSystem.md) document for information about how to alter competition configuration.



### Other attributes used by competitions

In addition to metric attributes there are few more used to alter execution of perftests. First, there are [config modifier attributes](ConfigurationSystem.md) that are applied on type- or assembly-level and allow to change competition configuration. Second, there are standard  `[GlobalSetup]`, `[IterationSetup]`, `[GlobalCleanup]` and `[IterationCleanup]` [BenchmarkDotNet attributes](http://benchmarkdotnet.org/Advanced/Setup.htm) that mark setup and cleanup methods. And finally, there are `[CompetitionBenchmark]`/`[CompetitionBaseline]` attributes that allows to include methods competition. 

#### Including methods in competition

Each method you want to be run and to be measured during competition run should be annotated with `[CompetitionBenchmark]` or `[CompetitionBaseline]` attribute. The latter one should be used in case the competition includes one or more relative competition metrics. If there's no baseline attribute the test will fail with following message:

```
Test completed with errors, details below.
Errors:
    * Run #1: No baseline member found. Apply CompetitionBaselineAttribute to the one of benchmark methods.
```

#### Excluding members from competition

If you want to exclude member from competition completely  you should remove it-is-benchmark attribute (one of `[CompetitionBenchmark]`, `[CompetitionBaseline]` or `[Benchmark]`) from it. If you want to inlude a member but ignore it's metric values there's `[CompetitionBenchmark(DoesNotCompete = true)]`. As example:

```c#
[CompetitionBenchmark(DoesNotCompete = true]
[ExpectedTime(10, 20, TimeUnit.Second)]
public void DoSomething() { ... }
```

The competition result log will you inform you about ignored methods with messages like this:

```
    * Run #1: Target DoSomething. Metric validation skipped as the method is marked with CompetitionBenchmarkAttribute.DoesNotCompete set to true.
```





## Annotate sources using XML file

Setting competition limits via attribute annotations does not work well with dynamically emitted or generated code. Code generation does not preserve limit annotations so you need to store the limits somewhere else. Meet the XML source annotations.

### Adding empty XML annotation file

To enable XML annotations you will need to add a new XML file with following content

```xml
<?xml version="1.0" encoding="utf-8"?>
<CompetitionBenchmarks>
</CompetitionBenchmarks>
```

It's recommended to locate the file at the same directory the perftest source is located and to give it the same name the source file has. Next, set Build Action for the file to the Embedded Resource. Like this:

![XmlAnnotationsFile](images/XmlAnnotationsFile.png)

Finally, annotate the perftest with `CompetitionXmlAnnotationAttribute`:

```c#
	[CompetitionXmlAnnotation(
		// Case-sensitive name of the manifest resource stream
		"CodeJam.Examples.PerfTests.SimplePerfTest.xml")]
	public class SimplePerfTest
	{
		...
	}
```

> **HINT** 
>
> As far as I know the exact logic the resource names are generated is undocumented. You can use [this answer](https://social.msdn.microsoft.com/Forums/vstudio/en-US/632d6914-8c90-450e-8ea0-fa60d2c3b6b6/manifest-name-for-embedded-resources?forum=msbuild) as a quick reference or [check the implementation](https://github.com/Microsoft/msbuild/blob/master/src/XMakeTasks/CreateCSharpManifestResourceName.cs) for more details.

Next, copy limits obtained from run with `[CompetitionPreviewLimits]` attribute into XML file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<CompetitionBenchmarks>
	<Competition Target="CodeJam.Examples.PerfTests.SimplePerfTest, CodeJam.Examples.PerfTests">
		<Candidate Target="Baseline" Baseline="true" />
		<Candidate Target="SlowerX3" MinRatio="2.97" MaxRatio="3.09" />
		<Candidate Target="SlowerX5" MinRatio="4.90" MaxRatio="5.10" />
		<Candidate Target="SlowerX7" MinRatio="6.85" MaxRatio="7.13" />
	</Competition>
</CompetitionBenchmarks>
```

By default target type name format is full type name followed with name of type's assembly. You may shorten it to simple type name, `Target="SimplePerfTest"`. Don't forget to remove the `UseFullTypeName = true` from `[CompetitionMetadata]` annotation then.

> **HINT**
>
> Nested type names uses '+' symbol as a separator, as example
>
> General rule is, if you're unsure what the target name should be, just run the perftest with `CompetitionPreviewLimitsAttribute` applied and check the output.



### Using existing XML annotations file

If you want to reuse existing XML annotations file you had to specify path to it explicitly. The same applies if name of the XML annotations file does not match to the sources file name. Here's how to do it:

Let's say you have source file 'PerfTests\SimplePerfTest.cs' and the XML annotations file is located at `Assets\PerfTestAnnotations.xml`.

![XmlAnnotationsReuseFile](images/XmlAnnotationsReuseFile.png)

Use the `MetadataResourcePath` property to set path to the XML annotations file.

```c#
	[CompetitionMetadata(
		// Case-sensitive name of the manifest resource stream
		"CodeJam.Examples.Assets.PerfTestAnnotations.xml",
		// Path (should be relative to the perftest source file path)
		MetadataResourcePath = @"..\Assets\PerfTestAnnotations.xml",
		UseFullTypeName = true)]
	public class SimplePerfTest
	{
		...
	}
```



Handling missing annotations

## Perform manual source annotation using attributes

 In addition you should annotate each of the methods with attribute for each metric you want to collect and analyze, result should look like this:

```c#
		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.98, 3.18)]
		[GcAllocations(0), Gc0(0), Gc1(0), Gc2(0)]
		[ExpectedTime(271.4, 321.0, TimeUnit.Microsecond)]
		public void SlowerX3() => Thread.SpinWait(3 * 16 * 1024);
```

In addition, if you have relative metrics to measure, the competition should include a method marked with a `[CompetitionBaseline]` attribute,

```c#
		// Baseline competition member. Other competition members will be compared with this.
		[CompetitionBaseline]
		[GcAllocations(0), Gc0(0), Gc1(0), Gc2(0)]
		[ExpectedTime(82.09, 101.26, TimeUnit.Microsecond)]
		public void Baseline() => Thread.SpinWait(Count);
```

This method cannot be annotated with attributes for relative metrics. Instead of this all relative metrics for all other methods in competition will be calculated based on the baseline results. If there's no baseline attribute the test will fail with

### Choosing a baseline method

You can use any competition method as a baseline, but if you'll change the baseline in the future you will had to reset all relative-to-baseline limits. 

It's a good idea to choose baseline method with stable timings as it improve accuracy of the measurements. If there are unpredictable delays in the baseline method body (IO or GC due to excessive allocations as example) baseline timings will vary a lot. This is bad as rest of competition methods use relative-to-baseline timings and theirs results will vary too even if the specific competition methods provides stable timings.

### Annotating rest of the code

Default constructor of `CompetitionBenchmarkAttribute` does not specify any limit for the method so there will be warning message like this:

```
The benchmark %BenchmarkMethodName% ignored as it has empty limit. Update limits to include benchmark in the competition.
```

There're multiple ways to specify limits for the competition. First and the recommended one is to enable source annotations using `[CompetitionAnnotateSources]` or `[CompetitionReannotateSources]`(use it to ignore existing source annotations) attribute. Run the perftest and the code will be updated with actual competition limits.

> **WARNING**
>
> Be sure to use source control before enabling the source annotation feature. If not you'll lose your current perftest limits as they will be overwritten without confirmation.

Second, you can set limits explicitly, 

```c#
		// Competition member #1. Should take ~3x more time to run.
		[CompetitionBenchmark(2.96, 3.08)]
		public void SlowerX3() => Thread.SpinWait(3 * Count);
```

Third, if you do want to obtain actual limits without updating the sources, apply the `CompetitionPreviewLimitsAttribute` and run the perftest. Here's part of the output:

```
---- Run 3, total runs (expected): 3 -----
// ? #3.1  04.123s, Informational@Analyser: CompetitionAnnotateAnalyser: All competition limits are ok.
// !<-- ------ xml_annotation_begin ------
<CompetitionBenchmarks>
	<Competition Target="CodeJam.Examples.PerfTests.SimplePerfTest, CodeJam.Examples.PerfTests">
		<Candidate Target="Baseline" Baseline="true" />
		<Candidate Target="SlowerX3" MinRatio="2.97" MaxRatio="3.09" />
		<Candidate Target="SlowerX5" MinRatio="4.95" MaxRatio="5.15" />
		<Candidate Target="SlowerX7" MinRatio="6.86" MaxRatio="7.14" />
	</Competition>
</CompetitionBenchmarks>
// !--> ------- xml_annotation_end -------
```

Now you can add new limits into `[CompetitionBenchmark]` annotations manually or store the annotations as a XML file





## Annotate source from previous run log file

Sometimes you have to annotate sources with results obtained from another machine.

To do so:

1. Enable both `ContinuousIntegrationMode` and `AnnotateSources` features for runs performed on another machine. You can do it via app.config (check [ConfigurationSystem](ConfigurationSystem.md) document for more information). If the perftest is run under continuous integration service the `ContinuousIntegrationMode` is enabled automatically.

2. Obtain the URI to the log file containing XML annotations. It can be local file (or network share) path or URL to the CI build log. If the CI service does not provide one you can include `%assemblyname%.ImportantOnly.PerfTests.log` file into build artifacts. Here's [example](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/appveyor.yml) of such config for AppVeyor build (note the `# artifacts for perftests` section). After done, save the URL to the last build log file. For example, AppVeyor url format is

   ```
   https://ci.appveyor.com/api/projects/%owner%/%project%/artifacts/%assemblyname%.ImportantOnly.PerfTests.log?all=true 
   ```

   and TeamCity url format will be

   ```
   https://%ci_server%/repository/download/%project%/.lastFinished/%assemblyname%.ImportantOnly.PerfTests.log
   ```

   Check documentation for your CI service for exact syntax.	


3. Pass the URI as a value of the `PreviousRunLogUri` feature. You can use app.config or pass it as constructor app to the `CompetitionAnnotateSourcesAttribute`
```c#
		[CompetitionAnnotateSources(@"d:\runs\last.log")]
		// -or-
		[CompetitionAnnotateSources(@"\\network\share\runs\last.log")]
		// -or-
		[CompetitionAnnotateSources(@"https://www.example.com/url.to.last.log")]
		public class SimplePerfTest
		{
			...
		}
```

4. Run the test **with enabled`AnnotateSources` feature**. If limits were updated there will be message like this:
```
Test completed with warnings, details below.
Warnings:
    * Run #1: The sources were updated with new annotations. Please check them before commiting the changes.
Diagnostic messages:
    * Run #1: Competition limits were updated from log file 'url-to-log-file'.
```

5. Done!


## Auto-annotate sources from runs under continuous integration service

You have two options there. 

**First**, you can enable source annotations mode, add the log into build artifacts, setup previous run log URI, and just run perftests. Limit adjustments performed during CI run will be applied to sources on local run. Check previous section for more.

**Second**, you can explicitly disable the `ContinuousIntegrationMode` feature, enable source annotations and setup CI server to auto-commit sources if tests succeed. As example, here's how to do it [with AppVeyor builds](https://www.appveyor.com/docs/how-to/git-push/). We do not recommend this approach. If performance will degrade unexpectedly there will be's no notifications and the limits will be silently overwritten.







Source annotations store expected values for each metric used in competition. By default CodeJam.PerfTests uses c# attributes to annotate sources but there are other metric storage providers. Out of the box there's an XML annotation provider that allows to store annotations out of the code, as embedded XML resource. XML annotations provider is especially useful for perftests over dynamically generated code as the annotations will not be overwritten during codegen. *--Future releases--* will include public API for third-party storage providers to enable advanced scenarios such as storing results from all runs in a database.

All storage providers support value updates so stored limits may be updated with actual values automatically.