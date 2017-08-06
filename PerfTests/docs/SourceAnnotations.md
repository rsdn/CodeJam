# CodeJam.PerfTests Source Annotations

> **META-NOTE**
>
> Places to update are marked with *--…--*.

[TOC]

Source annotations used to store performance metric limits for members of competitions. By default metric limits are persisted as c# attributes but there may be other metric storage providers. Out of the box there's XML annotation provider that allows to store source annotations as embedded XML resources. *--Future releases--* will include public API for third-party storage providers to enable advanced scenarios such as storing results from all runs in a database.





## Annotate sources using attributes

Each competition methic comes with attribute (`[ExpectedTime]` for measuring expected execution time, `[GcAllocations]` for guess what and so on). All of these attributes have multiple constructor overloads with similar signatures. Most full one allows to specify the minimum limit value, the maximum limit value and a scaling factor (unit of measurement) applied to both min and max limits. The scaling factor is omitted if the measurements are dimensionless. As example,

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
public void PleaseAllocate48Bytes() { ... }

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
public void PleaseAllocate48Bytes() { ... }
```

Please note that all metrics you do want to measure should present in collection returned by `ICompetitionConfig.GetMetrics()` method. If you run perftest with some metric applied as an attribute but not listed in config you'll get the following message:

```
// Metric ExpectedTime is not listed in config and therefore is ignored.
// ! Hint: List of metrics is exposed as ICompetitionConfig.GetMetrics().
```

Read [Configuration System](ConfigurationSystem.md) document for information about how to alter competition configuration.



### Other attributes used by competitions

In addition to metric attributes there are few more used to alter execution of perftests. First, there are [config modifier attributes](ConfigurationSystem.md) that are applied on type- or assembly-level and allow to change competition configuration. Second, there are standard  `[GlobalSetup]`, `[IterationSetup]`, `[GlobalCleanup]` and `[IterationCleanup]` [BenchmarkDotNet attributes](http://benchmarkdotnet.org/Advanced/SetupAndCleanup.htm) that mark setup and cleanup methods. And finally, there are `[CompetitionBenchmark]`/`[CompetitionBaseline]` attributes that allows to include methods competition. 

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

The competition result log will inform you about ignored methods with messages like this:

```
    * Run #1: Target DoSomething. Metric validation skipped as the method is marked with CompetitionBenchmarkAttribute.DoesNotCompete set to true.
```





## Annotate sources using XML file

Setting competition limits via attribute annotations does not work well with dynamically emitted or generated code. Code generation does not preserve limit annotations so you need to store the limits somewhere else. Meet the XML source annotations.



### Adding a new XML annotation file

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
> As far as I know the exact logic the resource names are generated is undocumented. You can use [this answer](https://social.msdn.microsoft.com/Forums/vstudio/en-US/632d6914-8c90-450e-8ea0-fa60d2c3b6b6/manifest-name-for-embedded-resources?forum=msbuild) as a quick reference or [check the implementation](https://github.com/Microsoft/msbuild/blob/master/src/Tasks/CreateCSharpManifestResourceName.cs) for more details.

If you want to fill metric limits automatically you should run the benchmark with `[CompetitionAnnotateSources]` attribute applied. The updated annotation file will look like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<CompetitionBenchmarks>
	<Competition Target="SimplePerfTest">
		<Baseline Baseline="true">
			<GcAllocations Min="0" Max="0" Unit="B" />
		</Baseline>
		<SlowerX3 Min="2.97" Max="3.09">
			<GcAllocations Min="0" Max="0" Unit="B" />
		</SlowerX3>
		<SlowerX5 Min="4.90" Max="5.10">
			<GcAllocations Min="0" Max="0" Unit="B" />
		</SlowerX5>
		<SlowerX7 Min="6.85" Max="7.13">
			<GcAllocations Min="0" Max="0" Unit="B" />
		</SlowerX7>
	</Competition>
</CompetitionBenchmarks>
```

By default xml annotation uses simple target type name, `Target="SimplePerfTest"`. Optionally you may force full type name usage by applying

```c#
	[CompetitionXmlAnnotation(…, UseFullTypeName = true)]
```

the result annotation will use full type name,

```xml
<Competition Target="CodeJam.Examples.PerfTests.SimplePerfTest, CodeJam.Examples">
	…
</Competition>
```



### Use single XML annotations file for multiple competitions

If you want to reuse existing XML annotations file you had to specify path to it explicitly. Here's how to do it:

Let's say you have source file `PerfTests\SimplePerfTest.cs` and the XML annotations file is located at `Assets\PerfTestAnnotations.xml`.

![XmlAnnotationsReuseFile](images/XmlAnnotationsReuseFile.png)

Use the `MetadataResourcePath` property to set path to the XML annotations file.

```c#
	[CompetitionMetadata(
		// Case-sensitive name of the manifest resource stream
		"CodeJam.Examples.Assets.PerfTestAnnotations.xml",
		// Path (should be relative to the perftest source file path)
		ResourcePath = @"..\Assets\PerfTestAnnotations.xml",
		UseFullTypeName = true)]
	public class SimplePerfTest
	{
		...
	}
```

Do not forget to apply the `UseFullTypeName = true` or 





## Setting the metric limits

### Manual source annotation

Okay, now you're familiar with basics and it's time to apply the metric limit annotations to the perftest. In most of cases you can save a plenty of time by using the auto-annotation feature (see below) but sometimes you have a strong performance contract and therefore you had to setup limits for the code. The process is pretty boring and trivial. You should annotate each competition member with expected values for each metric you want to use. For attribute annotations result should look like this:

```c#
		[CompetitionBenchmark(2.98, 3.18)]                 // ~3x slower compared to the baseline
		[GcAllocations(0), Gc0(0), Gc1(0), Gc2(0)]         // no GC; no allocations
		[ExpectedTime(271.4, 321.0, TimeUnit.Microsecond)] // ~ 27..32 ms to run
		public void SlowerX3() => Thread.SpinWait(3 * 16 * 1024);
```

Note that relative metrics (the `Scaled` metric applied via `[CompetitionBenchmark]` is one) do require a baseilne member. Choose one competition member and mark it with as a baseilne using `[CompetitionBaseline]` attribute.



### Choosing a baseline method

You can use any competition method as a baseline, but if you'll change the baseline in the future you will had to reset all relative-to-baseline metric values. 

It's a good idea to choose baseline method with small metric value distribution as it improve accuracy of metrics for all other competition memebers.

 As example, let's look at Scaled metric. If there are unpredictable delays in the baseline method body (e.g. IO or GC due to excessive allocations) the baseline timings will vary a lot. This is bad as the Scaled metric represents relative-to-baseline timings and results for other competition members will have a large distribution even if the members itself runs stable.



### Authomatic source annotations

While the auto-annotations feature is very useful and is one of the coolest features of the CodeJam.PerfTests, it has its drawbacks and therefore is disabled by default.

First and most important: **it is impossible to revert result of auto-annotation** if the old version of the file was not saved somehow. *--We have plans for implementing the source backup feature--* but it has very low priority as auto-annotations works smooth and we never have seen any issues with it. All in all, you've been warned.

> **WARNING**
>
> Be sure to use source control and to commit your changes before enabling the source annotation feature. If not you'll lose your current perftest limits as they will be overwritten without confirmation.

Second, by default Visual Studio asks you to confirm file reload after each external change. This can be suppressed via checking the "Reload modified files unless there are unsaved changes" option.

![VSOptionsReload](images/VSOptionsReload.png) 

The auto annotation feature may be enabled via config or by applying `[CompetitionAnnotateSources]` to the competition class or to the assembly. Check the [Configuration System](ConfigurationSystem) document for detailed explanation how the config modifiers work. After the feature is enabled each run of the perftest will update sources with adjusted metric limits.

In addition to the `[CompetitionAnnotateSources]` there are another modifiers

* the `[CompetitionReannotateSources]` that clears previous collected limits and replaces it with new ones
* and the `[CompetitionPreviewMetrics]` that calculates current metric limits and prints them but does not update the source.



### Merging results from previous run log file

Sometimes you have to adjust metric limits with results obtained from another machine. It's especially useful if perftests are being run on Continuous integration server (check the next section). The entire process looks like this

1. You should enable both `ContinuousIntegrationMode` and `AnnotateSources` features for runs performed on another machine. You can do it via app.config (check the [ConfigurationSystem](ConfigurationSystem.md) document for more information). If the perftests are run under continuous integration service the `ContinuousIntegrationMode` is enabled automatically. Run the perftests and ensure that they've completed without errors.

2. Next, obtain the URI to the log file that contains output of the perftests. It can be local file (or network share) path or URL to the CI build log. If the CI service does not provide one you can include the `%assemblyname%.ImportantOnly.PerfTests.log` file into build artifacts. Here's [example](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/appveyor.yml) of such config for AppVeyor build (note the `# artifacts for perftests` section). After done, save the URL to the last build log file. For example, AppVeyor url format is

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
		[CompetitionAnnotateSources(@"https://www.example.com/url.last.log")]
		public class SimplePerfTest
		{
			...
		}
```

4. Run the test locally **with enabled`AnnotateSources` feature**. If limits were adjusted there will be message like this:
```
Test completed with warnings, details below.
Warnings:
    * Run #1: The sources were updated with new annotations. Please check them before committing the changes.
Diagnostic messages:
    * Run #1: Competition metric limits were updated from log file 'url-to-log-file'.
```

5. Done!




### Source annotations and Continuous Integration service

You have two options there. 

**First**, as described in a previous section, you can enable source annotations mode, add the log into build artifacts, setup previous run log URI, and just run perftests. Metric adjustments will be applied to sources on local run. Check the previous section for more.

**Second**, you can explicitly disable the `ContinuousIntegrationMode` feature, enable source annotations and setup CI server to auto-commit sources if tests succeed. As example, here's how to do it [with AppVeyor builds](https://www.appveyor.com/docs/how-to/git-push/). We do not recommend this approach as accidental slowdowns may spoil collected metrics and the sources will be updated with these spoiled values without confirmation.
