# CodeJam.PerfTests Configuration System

> **META-NOTE**
>
> Places to update are marked with *--…--*.
>

[TOC]

> **WARNING**
>
> Please do not use the BenchmarkDotNet config attributes for competitions. *--As it is for now--* they are unsupported and result behavior is undefined.

CodeJam.PerfTests configuration uses almost same approach the BenchmarkDotNet does. However, there are additions aimed to ease configuration of large projects with hundreds or thousands of perftests. Here's how it works:



## Attribute annotations

Almost all configuration features rely on attribute annotations. Attributes are checked in following order:

1. Attributes applied to the competition class or to it's base types.
2. Attributes applied to the container types or to container type's base types (if the competition class is a nested type).
3. Attributes applied to the assembly.

If the configuration system expects only one attribute (as with `CompetitionConfigAttribute`), first found attribute wins.

In cases when multiple attributes allowed (`CompetitionFeaturesAttribute` as example), they are applied in reversed order, overriding previous ones: assembly level attributes go first, container type attributes are applied next and the competition class attributes are the last ones.

> **IMPORTANT**
>
> There's no ordering for attributes applied at the same level. As example, if there are multiple attributes applied to the type or to the assembly, they may be enumerated in random order.





## 1. Run competition with explicit config

>  **NOTE**
>
>  Explicit config passing is an advanced technique and should be used only when you want to have a perfect control over the configuration. It skips entire configuration pipeline and therefore it's up to you to pass correct config into competition.

Competition config stores all settings that apply to the competition. It's derived from BenchmarkDotNet's `IConfig` and adds some new members (`Options` property and `GetMetrics()` method, as example) *--TODO: link to competition options help page--*.



### 1.1 Pass config as a competition arg

It just works. No additional adjustments are performed, competition will use the config you're passing to the test. Use it like this

```c#
		[Test]
		public void RunSimplePerfTest()
		{
			// Obtain the default config for the executing assembly.
			var config = CompetitionHelpers.ConfigForAssembly;
			// Create updated config
			config = config
				.WithAllowDebugBuilds(true) // No warning when run on default builds
				.WithDetailedLogging(true); // Enable detailed test console output

			Competition.Run(this, config); // Pass updated config to the competition
		}
```



### 1.2 Use custom config attribute

If you do want to reuse the config you can define custom config attribute

```c#
	public class MyCompetitionAttribute : CompetitionConfigAttribute
	{
		public MyCompetitionAttribute() : base(Create)
		{ }

		private static ICompetitionConfig Create()
		{
			// Create a config and fill it with defaults 
			var config = new ManualCompetitionConfig(CompetitionHelpers.DefaultConfig);

			// Override some competition options
			config.ApplyModifier(new CompetitionOptions
			{
				// Fail on warnings
				RunOptions = { ReportWarningsAsErrors = true },
				// No long runs allowed
				Checks = { LongRunningBenchmarkLimit = TimeSpan.FromSeconds(5) } 
			});

			// Override some job properties
			config.ApplyModifier(new Job()
			{
				// Run twice
				Run = { LaunchCount = 2 }
			});

			return config;
		}
	}
```

and apply it to the competition class, it's container class (if the competition class is a nested type) or to the competition's assembly:

```c#
	// Use config for the SimplePerfTest class
	// and for all nested or derived classes
	// (if they do exist and do not have their own config attribute applied).
	[MyCompetition]
	// -or-
	// Use config for entire assembly.
	[assembly:MyCompetition]
	public class SimplePerfTest
	{
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// ...
	}
```

When the test is run the configuration system will check the competition type, it's container type (if any) and competition's assembly for the `CompetitionConfigAttribute`. First found attribute wins.





## 2. Declarative approach. Use competition features

> **NOTE**
>
> All declarative config annotations do apply only if the config was not passed explicitly (as a `Competition.Run()` argument or via `CompetitionConfigAttribute`).

It should be obvious for now that CodeJam.PerfTests has very complex configuration system. At the same time most end-user use cases are simple. You may want to enable/disable source annotations or specify target platform or enable troubleshooting mode. You do not want to know anything about the configs or what properties should be changed to enable particular scenario. Meet the `CompetitionFeatures`.



### 2.1 Pass competition features explicitly

As with explicit config scenario, features should be passed explicitly only when you want to override all other ways to set up a competition feature. Here's how to do it:

```c#
		[Test]
		public void RunSimplePerfTest()
		{
			Competition.Run(
				this,
				new CompetitionFeatures
				{
					// Detailed logging, allow debug builds, export measurements and so on
					TroubleshootingMode = true,
					// We do not care whether the benchmark is run as x86 or x64
					Platform = Platform.AnyCpu
				});
		}
```

> **NOTE**
>
> all competition feature annotations are ignored if the features are passed explicitly as an arg of the `Competition.Run()`.



### 2.2 Set default competition features via app.config

If you want to set up competition features for entire assembly you can add it into default app.config, perftest assembly's app.config or into `CodeJam.PerfTests.dll.config`. First found config with `<CodeJam.PerfTests/>` section wins.

The syntax is following:

```xml
<configuration>
	<!-- <configSections> must be the first child element of the <configuration> element. -->
	<configSections>
		<section
			name="CodeJam.PerfTests"
			type="CodeJam.PerfTests.Configs.PerfTestsSection, CodeJam.PerfTests"/>
	</configSections>

	<!-- ... -->

	<CodeJam.PerfTests
		Platform="X64"
		AnnotateSources="true"
		IgnoreExistingAnnotations="false"
		ReportWarningsAsErrors="false"
		TroubleshootingMode="false"
		ImportantInfoLogger="true"
 		DetailedLogger="false"/>
</configuration>
```



### 2.3 Update default competition features from the environment

> ***--THINGS TO CHANGE--***
>
> There can be more auto-detected features in the future, documentation will be updated.

CodeJam.PerfTests detects if it is running under Continuous Integration service and sets `CompetitionFeatures.ContinuousIntegrationMode` to true. This setting adjusts competition options so that source annotation feature will work even if sources are not available. Check [Source Annotations](SourceAnnotations.md) for more information.

Current CI auto-detection uses very naïve approach and checks if the current process has any of the following environment variables applied:

```
Environment variable |   Defined by
---------------------|----------------------------------
 APPVEYOR            | AppVeyor Continuous Integration
 CI                  | AppVeyor CI or Travis CI
 JENKINS_URL         | Jenkins
 TEAMCITY_VERSION    | JetBrains TeamCity
 TF_BUILD            | MS Team Foundation Server
 TRAVIS              | Travis CI
```

So if you have a CI service not supported yet and want CI auto-detection feature to work, just add one of the environment variables to your test setup (`CI` looks like a best choice).

Want to add CI service or have a idea how to make the feature better? [Create issue for it!](https://github.com/rsdn/CodeJam/issues)



### 2.4 Set competition features via attributes 

While default features can be good for most perftests there always are tests that require own feature set. If you want to add (or disable) some particular features, apply the `[CompetitionFeatures]` attribute (or any derived attribute) to the competition class, container type (if the competition class is a nested type) or to the competition's assembly. Check the *--Attribute annotations TODO: link--* section for explanation how the attributes are applied.

Here's example that covers possible annotations for the competition features.

```c#
	// Assembly-level defaults: enables Detailed and ImportantInfo loggers
	[assembly:CompetitionFeatures(DetailedLogger=true, ImportantInfoLogger = true)]

	// Container type level: Disables DetailedLogger and adds target platform=X64
	[CompetitionFeatures(DetailedLogger = false, Platform = Platform.X64)]
	public class ContainerType
	{
		// Container type level: Overrides the target platform
		[CompetitionPlatform(Platform.X86)]
 		// BADCODE: DO NOT do this, random (X86|X64) platform will be applied
		[CompetitionFeatures(Platform = Platform.X64)]
		public class AnotherContainerType
		{
			// Perftest class: enables source annotations and disables target platorm check
			[CompetitionAnnotateSources]
			public class SimplePerfTest
			{
				// Runs the competition. Resulting features are
				// * DetailedLogger = false
				// * ImportantInfoLogger = true
				// * Platform = <unknown> (one of X86|X64 will be applied)
				// * AnnotateSources = true
				[Test]
				public void RunSimplePerfTest() => Competition.Run(this);

				// ...
			}
		}
	}
```





## 3. Declarative approach. Modify resulting config

> **NOTE**
>
> All declarative config annotations do apply only if the config was not passed explicitly (as a `Competition.Run()` argument or via `CompetitionConfigAttribute`).

Okay, you've set up competition features but you do want to change some options that are not exposed as a competition features. CodeJam.PerfTests provide `ICompetitionModifier` interface for competition config modifiers and `CompetitionModifierAttribute` attribute to apply the modifiers to the competitions. As example:

```c#
	public class MyCompetitionModifier : ICompetitionModifier
	{
		// ICompetitionModifier implementation
		public void Modify(ManualCompetitionConfig competitionConfig)
		{
			// Override some competition options
			competitionConfig.ApplyModifier(new CompetitionOptions
			{
				// Fail on warnings
				RunOptions = { ReportWarningsAsErrors = true },
				// No long runs allowed
				Checks = { LongRunningBenchmarkLimit = TimeSpan.FromSeconds(5) }
			});

			// Override some job properties
			competitionConfig.ApplyModifier(new Job()
			{
				// Run twice
				Run = { LaunchCount = 2 }
			});
		}
	}
```

If you need to pass some args to the modifier, here's how to do it:

```c#
	// Custom modifier attribute
	public sealed class CompetitionRerunsModifierAttribute : CompetitionModifierAttribute
	{
		// ICompetitionModifier implementation
		private class ModifierImpl : ICompetitionModifier
		{
			private readonly int _rerunsIfValidationFailed;

			public ModifierImpl(int rerunsIfValidationFailed)
			{
				_rerunsIfValidationFailed = rerunsIfValidationFailed;
			}
 
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.ApplyModifier(
					new CompetitionOptions
					{
						Checks = { RerunsIfValidationFailed = _rerunsIfValidationFailed }
					});
		}

		// Passing arg(s) to the implementation.
		public CompetitionRerunsModifierAttribute(int rerunsIfValidationFailed)
			: base(() => new ModifierImpl(rerunsIfValidationFailed)) { }
	}
```

Modifiers can be applied to the competition class, it's container class (if the competition class is a nested type) or to the competition's assembly:

```c#
	// Modifier will be applied to all competition classes in the assembly.
	[assembly: CompetitionModifier(typeof(AnotherCompetitionModifier))]

	// Appling modifiers to the SimplePerfTest class
	// and to all nested or derived classes (if they do exist).
	[CompetitionModifier(typeof(MyCompetitionModifier))]
	[CompetitionRerunsModifier(5)]
	public class SimplePerfTest
	{
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// ...
	}
```

As with `CompetitionFeaturesAttribute`, modifiers can be combined together. Check the *--Attribute annotations TODO: link--* section for explanation how modifier attributes are applied.





## 4. Get magic under control. Custom competition config factories

> **NOTE**
>
> All declarative config annotations do apply only if the config was not passed explicitly (as a `Competition.Run()` argument or via `CompetitionConfigAttribute`).

> **NOTE**
>
> As with explicit config passing, this is an advanced feature and it is recommended to check for existing implementations and study them at first. There's no safety net anymore.

If all of the above is not enough there's a backdoor: you can override entire config factory pipeline. Implement `ICompetitionConfigFactory` or derive from existing one:

```c#
	public class MyCompetitionFactory : CompetitionConfigFactory
	{
		public MyCompetitionFactory(string configId) : base(configId) { }

		protected override CompetitionFeatures CompleteFeatures(
			CompetitionFeatures competitionFeatures)
		{
			// Disable CI support.
			competitionFeatures.ContinuousIntegrationMode = false;

			return base.CompleteFeatures(competitionFeatures);
		}

		protected override ICompetitionConfig CompleteConfig(
			ManualCompetitionConfig competitionConfig)
		{
			// No idea what to do here. Let's sort something
			competitionConfig.Analysers.Sort((IAnalyser a, IAnalyser b) =>
				String.Compare(a.Id, b.Id, StringComparison.Ordinal));

			// and remove some stuff.
			competitionConfig.Exporters.Clear();

			return base.CompleteConfig(competitionConfig);
		}
	}	
```

and apply it to the competition class, it's container class (if the competition class is a nested type) or to the competition's assembly:

```c#
	// Use config factory for the SimplePerfTest class
	// and for all nested or derived classes
	// (if they do exist and do not have their own config factory attribute applied).
	[CompetitionConfigFactory(typeof(MyCompetitionFactory))]
	// -or-
	// Use config factory for entire assembly.
	[assembly: CompetitionConfigFactory(typeof(MyCompetitionFactory))]
	public class SimplePerfTest
	{
		[Test]
		public void RunSimplePerfTest() => Competition.Run(this);

		// ...
	}
```

When the test is run the configuration system will check the competition type, it's container type (if any) and competition's assembly for the `CompetitionConfigFactoryAttribute`. First found attribute wins.

