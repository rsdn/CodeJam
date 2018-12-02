using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Columns;
using CodeJam.PerfTests.Metrics;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Configs.Factories.CompetitionFactoryHelpers;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>
	/// Reusable API for creating competition config.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public class CompetitionConfigFactory : ICompetitionConfigFactory
	{
		/// <summary>Columns for default config. </summary>
		public static IColumnProvider DefaultColumns = new CompositeColumnProvider(
			DefaultColumnProviders.Target,
			CompetitionOptionsColumnProvider.Instance,
			DefaultColumnProviders.Job,
			DefaultColumnProviders.Params,
			DefaultColumnProviders.Diagnosers,
			new SimpleColumnProvider(
				StatisticColumn.Mean,
				StatisticColumn.StdDev));

		/// <summary>Columns for troubleshooting mode config. </summary>
		public static IColumnProvider TroubleshootingModeColumns = new CompositeColumnProvider(
			DefaultColumnProviders.Target,
			CompetitionOptionsColumnProvider.Instance,
			DefaultColumnProviders.Job,
			DefaultColumnProviders.Params,
			DefaultColumnProviders.Diagnosers,
			DefaultColumnProviders.Statistics,
			new SimpleColumnProvider(
				StatisticColumn.Mean,
				StatisticColumn.StdDev,
				BaselineScaledColumn.Scaled,
				BaselineScaledColumn.ScaledStdDev,
				StatisticColumn.Min,
				StatisticColumn.Max));

		/// <summary>Columns for troubleshooting mode config. </summary>
		public static IReadOnlyList<MetricInfo> DefaultMetrics = new MetricInfo[]
		{
			WellKnownMetrics.RelativeTime,
			WellKnownMetrics.GcAllocations
		};

		/// <summary>Creates competition config for type.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		public static ICompetitionConfig FindFactoryAndCreate(
			[CanBeNull] Assembly targetAssembly,
			[CanBeNull] CompetitionFeatures competitionFeatures)
		{
			var factory = targetAssembly?.TryGetMetadataAttribute<ICompetitionConfigFactorySource>()?.ConfigFactory
				?? FallbackInstance;

			return factory.Create(targetAssembly, competitionFeatures);
		}

		/// <summary>Creates competition config for type.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		public static ICompetitionConfig FindFactoryAndCreate(
			[CanBeNull] Type benchmarkType,
			[CanBeNull] CompetitionFeatures competitionFeatures)
		{
			var factory = benchmarkType?.TryGetMetadataAttribute<ICompetitionConfigFactorySource>()?.ConfigFactory
				?? FallbackInstance;

			return factory.Create(benchmarkType, competitionFeatures);
		}

		#region Static members
		/// <summary>Default competition config job.</summary>
		public static readonly Job DefaultJob = new Job("Competition")
		{
			Env =
			{
				Gc =
				{
					Force = false
				}
			},
			Run =
			{
				LaunchCount = 1,
				WarmupCount = 128,
				TargetCount = 256,
				RunStrategy = RunStrategy.Throughput,
				UnrollFactor = 16,
				InvocationCount = 256
			},
			Infrastructure =
			{
				Toolchain = InProcessToolchain.DontLogOutput
			}
		}.Freeze();

		/// <summary>The modifier for burst mode jobs.</summary>
		public static readonly Job BurstModeModifier = new Job
		{
			Run =
			{
				UnrollFactor = 1,
				InvocationCount = 1
			}
		}.Freeze();

		/// <summary>Default competition config options.</summary>
		public static readonly CompetitionOptions DefaultCompetitionOptions = new CompetitionOptions("Competition")
			.ApplyAndFreeze(CompetitionOptions.Default);

		/// <summary>Default instance of <see cref="CompetitionConfigFactory"/></summary>
		protected static readonly CompetitionConfigFactory FallbackInstance = new CompetitionConfigFactory(DefaultJob.Id);
		#endregion

		#region .ctor & properties
		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigFactory"/> class.</summary>
		/// <param name="configId">The config identifier.</param>
		public CompetitionConfigFactory(string configId)
		{
			ConfigId = configId;
		}

		/// <summary>The config identifier.</summary>
		/// <value>The config identifier.</value>
		public string ConfigId { get; }
		#endregion

		/// <summary>Creates competition config for type.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		public ICompetitionConfig Create(
			Assembly targetAssembly,
			CompetitionFeatures competitionFeatures) =>
				CreateCore(targetAssembly, competitionFeatures);

		/// <summary>Creates competition config for type.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		public ICompetitionConfig Create(
			Type benchmarkType,
			CompetitionFeatures competitionFeatures) =>
				CreateCore(benchmarkType, competitionFeatures);

		/// <summary>Creates competition config for type.</summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		protected ICompetitionConfig CreateCore(
			[CanBeNull] ICustomAttributeProvider metadataSource,
			[CanBeNull] CompetitionFeatures competitionFeatures)
		{
			// 1. Check for existing config
			var config = metadataSource?.TryGetMetadataAttribute<ICompetitionConfigSource>()?.Config;
			if (config != null)
				return CompleteConfig(new ManualCompetitionConfig(config));

			// 2. Get competition features
			// TODO: redesign? Do not lost features Id
			competitionFeatures = competitionFeatures?.UnfreezeCopy()
				?? CreateCompetitionFeaturesUnfrozen(ConfigId, metadataSource);
			competitionFeatures = CompleteFeatures(competitionFeatures);

			// 3. Create config stub
			var result = CreateEmptyConfig(metadataSource, competitionFeatures);

			// 4. Create job
			result.Add(CreateJobUnfrozen(ConfigId, metadataSource, competitionFeatures));

			// 5. Create competition options
			result.Set(CreateCompetitionOptionsUnfrozen(metadataSource, competitionFeatures));

			// 6. Apply config sources.
			ApplyConfigSources(result, metadataSource);

			// 7. Apply competition modifiers
			ApplyCompetitionModifiers(result, metadataSource);

			return CompleteConfig(result);
		}

		/// <summary>
		/// Creates competition features. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="metadataSource">The metadata source.</param>
		/// <returns>
		/// New competition features. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </returns>
		protected virtual CompetitionFeatures CreateCompetitionFeaturesUnfrozen(
			[CanBeNull] string jobId,
			[CanBeNull] ICustomAttributeProvider metadataSource) =>
				new CompetitionFeatures(jobId, GetCompetitionFeatures(metadataSource));

		/// <summary>Completes competition features creation.</summary>
		/// <param name="competitionFeatures">Current competition features.</param>
		/// <returns>Frozen competition features.</returns>
		protected virtual CompetitionFeatures CompleteFeatures(
			[NotNull] CompetitionFeatures competitionFeatures) =>
				competitionFeatures.Freeze();

		/// <summary>Creates empty competition config without job and competition options applied.</summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>New empty competition config.</returns>
		[NotNull]
		protected virtual ManualCompetitionConfig CreateEmptyConfig(
			[CanBeNull] ICustomAttributeProvider metadataSource,
			[NotNull] CompetitionFeatures competitionFeatures)
		{
			var result = new ManualCompetitionConfig(BenchmarkDotNet.Configs.DefaultConfig.Instance);

			// DONTTOUCH: competition should not use default
			// Jobs, Diagnosers, Exporters and Loggers.
			// These omitted intentionally.
			result.Jobs.Clear();
			result.Diagnosers.Clear();
			result.Exporters.Clear();
			result.Loggers.Clear();

			// TODO: better columns.
			// TODO: columns for options.
			result.ColumnProviders.Clear();
			result.Add(competitionFeatures.TroubleshootingMode ? TroubleshootingModeColumns : DefaultColumns);

			var targetAssembly = GetAssembly(metadataSource);
			if (targetAssembly != null)
			{
				if (competitionFeatures.TroubleshootingMode)
				{
					result.Add(
						GetImportantInfoLogger(targetAssembly),
						GetDetailedLogger(targetAssembly));

					result.Add(Exporters.CsvTimingsExporter.Default);
				}
				else
				{
					if (competitionFeatures.ImportantInfoLogger || competitionFeatures.ContinuousIntegrationMode)
					{
						result.Add(GetImportantInfoLogger(targetAssembly));
					}
					if (competitionFeatures.DetailedLogger)
					{
						result.Add(GetDetailedLogger(targetAssembly));
					}
				}
			}

			result.Add(DefaultMetrics.ToArray());

			return result;
		}

		/// <summary>
		/// Creates job for the competition. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>
		/// New job for the competition. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </returns>
		protected virtual Job CreateJobUnfrozen(
			[CanBeNull] string jobId,
			[CanBeNull] ICustomAttributeProvider metadataSource,
			[NotNull] CompetitionFeatures competitionFeatures)
		{
			var platform = competitionFeatures.ResolveValueAsNullable(CompetitionFeatures.PlatformCharacteristic);

			if (jobId != null)
				jobId += platform;

			var job = new Job(jobId, DefaultJob);
			if (platform != null)
			{
				job.Env.Platform = platform.GetValueOrDefault();
			}
			if (competitionFeatures.BurstMode)
			{
				job.Apply(BurstModeModifier);
				job.Run.TargetCount *= 2;
			}

			if (competitionFeatures.TroubleshootingMode)
			{
				job.Apply(new InfrastructureMode { Toolchain = InProcessToolchain.Instance });
			}

			return job;
		}

		// TODO: pass id?
		/// <summary>
		/// Creates options for the competition. The <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>
		/// Options for the competition. The <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </returns>
		protected virtual CompetitionOptions CreateCompetitionOptionsUnfrozen(
			[CanBeNull] ICustomAttributeProvider metadataSource,
			[NotNull] CompetitionFeatures competitionFeatures)
		{
			var result = new CompetitionOptions(DefaultCompetitionOptions);

			if (competitionFeatures.AnnotateSources)
			{
				result.Adjustments.AdjustMetrics = true;

				if (competitionFeatures.IgnoreExistingAnnotations)
				{
					result.Annotations.IgnoreExistingAnnotations = true;
					result.Annotations.PreviousRunLogUri = null;
				}
				else
				{
					result.Annotations.PreviousRunLogUri = competitionFeatures.PreviousRunLogUri;
				}
			}

			if (Debugger.IsAttached)
			{
				result.RunOptions.AllowDebugBuilds = true;
			}

			if (competitionFeatures.TroubleshootingMode)
			{
				result.RunOptions.AllowDebugBuilds = true;
				result.RunOptions.ReportWarningsAsErrors = false;
				result.RunOptions.DetailedLogging = true;
			}
			else if (competitionFeatures.ReportWarningsAsErrors)
			{
				result.RunOptions.ReportWarningsAsErrors = true;
			}

			if (competitionFeatures.ContinuousIntegrationMode)
			{
				result.RunOptions.ContinuousIntegrationMode = true;
				result.Annotations.LogAnnotations = true;
				result.Annotations.PreviousRunLogUri = null;
				result.Annotations.DontSaveUpdatedAnnotations = true;
			}

			return result;
		}

		/// <summary>Applies <see cref="IConfigSource"/> attribute modifiers.</summary>
		/// <param name="competitionConfig">The competition configuration.</param>
		/// <param name="metadataSource">The metadata source.</param>
		protected virtual void ApplyConfigSources(ManualCompetitionConfig competitionConfig, ICustomAttributeProvider metadataSource)
		{
			var config = new ManualConfig();
			var add = false;
			if (metadataSource != null)
			{
				foreach (var modifierSource in metadataSource
					.GetMetadataAttributes<IConfigSource>()
					.Reverse())
				{
					config = ManualConfig.Union(config, modifierSource.Config);
					add = true;
				}
			}
			if (add)
			{
				competitionConfig.Add(config);
			}
		}

		/// <summary>Applies <see cref="ICompetitionModifierSource"/> attribute modifiers.</summary>
		/// <param name="competitionConfig">The competition configuration.</param>
		/// <param name="metadataSource">The metadata source.</param>
		protected virtual void ApplyCompetitionModifiers(
			[NotNull] ManualCompetitionConfig competitionConfig,
			[CanBeNull] ICustomAttributeProvider metadataSource)
		{
			if (metadataSource != null)
			{
				foreach (var modifierSource in metadataSource
					.GetMetadataAttributes<ICompetitionModifierSource>()
					.Reverse())
				{
					modifierSource.Modifier.Modify(competitionConfig);
				}
			}
		}

		/// <summary>Completes competition config creation.</summary>
		/// <param name="competitionConfig">Current competition config.</param>
		/// <returns>Read-only competition config.</returns>
		protected virtual ICompetitionConfig CompleteConfig(
			[NotNull] ManualCompetitionConfig competitionConfig) =>
				competitionConfig.AsReadOnly();
	}
}