using System;
using System.IO;
using System.Reflection;

using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.Collections;
using CodeJam.PerfTests.Loggers;
using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Reusable API for creating competition config.
	/// </summary>
	[PublicAPI]
	public static class CompetitionConfigFactory
	{
		#region Appcongfig support
		/// <summary>Name of the config section.</summary>
		public const string SectionName = "CodeJam.PerfTests";

		/// <summary>Reads <see cref="CompetitionFeatures" /> from assembly level options config section.</summary>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <param name="targetAssembly">Assembly to check. If <c>null</c>, the result is <c>null</c></param>
		/// <returns>
		///   <see cref="CompetitionFeatures" /> section filled from first of app.config, <paramref name="targetAssembly" /> or CodeJam.PerfTests assembly.
		/// </returns>
		public static CompetitionFeatures CreateCompetitionFeatures(
			CompetitionFeatures competitionFeatures,
			Assembly targetAssembly)
		{
			if (competitionFeatures != null)
				return competitionFeatures;

			if (targetAssembly != null)
				competitionFeatures = FeaturesFromAppConfig(targetAssembly);

			return competitionFeatures ?? new CompetitionFeatures();
		}

		/// <summary>Reads <see cref="CompetitionFeatures"/> from assembly level options config section.</summary>
		/// <param name="assembliesToCheck">Assemblies to check for the config section if the app.config does not contain the section.</param>
		/// <returns>
		///  <see cref="CompetitionFeatures"/> section filled from first of app.config, <paramref name="assembliesToCheck"/> or CodeJam.PerfTests assembly.
		/// </returns>
		public static CompetitionFeatures FeaturesFromAppConfig(
			params Assembly[] assembliesToCheck)
		{
			var section = BenchmarkHelpers.ParseConfigurationSection<PerfTestsSection>(
				SectionName,
				assembliesToCheck.Concat(typeof(CompetitionConfigFactory).Assembly));

			return section == null
				? null
				: new CompetitionFeatures
				{
					TargetPlatform = section.TargetPlatform,
					AnnotateSources = section.AnnotateSources,
					IgnoreExistingAnnotations = section.IgnoreExistingAnnotations,
					PreviousRunLogUri = section.PreviousRunLogUri,
					ReportWarningsAsErrors = section.ReportWarningsAsErrors,
					TroubleshootingMode = section.TroubleshootingMode,
					ImportantInfoLogger = section.ImportantInfoLogger,
					DetailedLogger = section.DetailedLogger
				};
		}
		#endregion

		#region Loggers
		/// <summary>The detailed log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string DetailedLogSuffix = ".Detailed.PerfTests.log";

		/// <summary>The important only log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string ImportantOnlyLogSuffix = ".ImportantOnly.PerfTests.log";

		/// <summary>Gets the important information logger.</summary>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <returns>Important information logger for the assembly</returns>
		public static ILogger GetImportantInfoLogger([NotNull] Assembly targetAssembly)
		{
			Code.NotNull(targetAssembly, nameof(targetAssembly));
			return _importantOnlyLoggersCache(targetAssembly);
		}

		/// <summary>Gets the detailed logger.</summary>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <returns>Detailed logger for the assembly.</returns>
		public static ILogger GetDetailedLogger([NotNull] Assembly targetAssembly)
		{
			Code.NotNull(targetAssembly, nameof(targetAssembly));
			return _detailedLoggersCache(targetAssembly);
		}

		private static readonly Func<Assembly, ILogger> _detailedLoggersCache =
			Algorithms.Memoize(
				(Assembly a) => CreateAssemblyLevelLogger(a, DetailedLogSuffix),
				true);

		private static readonly Func<Assembly, ILogger> _importantOnlyLoggersCache =
			Algorithms.Memoize(
				(Assembly a) => new HostLogger(
					CreateAssemblyLevelLogger(a, ImportantOnlyLogSuffix),
					HostLogMode.PrefixedOnly),
				true);

		private static LazySynchronizedStreamLogger CreateAssemblyLevelLogger(Assembly assembly, string suffix)
		{
			var fileName = assembly.GetAssemblyPath();
			fileName = Path.ChangeExtension(fileName, suffix);
			return new LazySynchronizedStreamLogger(
				() => new StreamWriter(
					new FileStream(
						fileName,
						FileMode.Create, FileAccess.Write, FileShare.Read)));
		}
		#endregion

		#region Jobs
		/// <summary>Default competition config job.</summary>
		public static readonly Job DefaultJob = new Job("CompetitionConfig")
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
				WarmupCount = 100,
				TargetCount = 300,
				UnrollFactor = 1,
				RunStrategy = RunStrategy.Throughput,
				InvocationCount = 1
			},
			Infrastructure =
			{
				Toolchain = InProcessToolchain.Instance,
				EngineFactory = BurstModeEngineFactory.Instance
			},
			Accuracy =
			{
				AnalyzeLaunchVariance = false,
				EvaluateOverhead = false,
				RemoveOutliers = false
			}
		}.Freeze();

		/// <summary>Creates job for the competition.</summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>New job for the competition</returns>
		public static Job CreateJob(string jobId, CompetitionFeatures competitionFeatures)
		{
			var platform = competitionFeatures?.TargetPlatform;

			if (jobId == null && platform == null)
				return DefaultJob;

			if (jobId != null)
				jobId += platform;

			var job = new Job(jobId, DefaultJob);
			if (platform != null)
			{
				job.Env.Platform = platform.GetValueOrDefault();
			}

			return job.Freeze();
		}
		#endregion

		#region Competition options
		/// <summary>Default competition config options.</summary>
		public static readonly CompetitionOptions DefaultCompetitionOptions = CompetitionOptions.Default;

		private static CompetitionOptions CreateCompetitionOptions(CompetitionFeatures competitionFeatures)
		{
			if (competitionFeatures == null)
				return DefaultCompetitionOptions;

			var result = new CompetitionOptions(DefaultCompetitionOptions);

			if (competitionFeatures.AnnotateSources)
			{
				result.SourceAnnotations.UpdateSources = true;
				if (competitionFeatures.IgnoreExistingAnnotations)
				{
					result.Limits.IgnoreExistingAnnotations = true;
					result.SourceAnnotations.PreviousRunLogUri = null;
				}
				else
				{
					result.SourceAnnotations.PreviousRunLogUri = competitionFeatures.PreviousRunLogUri;
				}
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

			return result.Freeze();
		}
		#endregion

		#region Factory methods
		/// <summary>Creates mutable comnpetition config.</summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <param name="competitionFeatures">
		/// The competition features.
		/// if is <c>null</c> and the <paramref name="targetAssembly"/> set,
		/// features are taken from appconfig.</param>
		/// <returns>New mutable comnpetition config.</returns>
		public static ManualCompetitionConfig Create(
			string jobId, Assembly targetAssembly, CompetitionFeatures competitionFeatures)
		{
			competitionFeatures = CreateCompetitionFeatures(competitionFeatures, targetAssembly);

			var result = CreateCompetitionConfigStub(targetAssembly, competitionFeatures);
			result.Add(CreateJob(jobId, competitionFeatures));
			result.Set(CreateCompetitionOptions(competitionFeatures));

			return result;
		}

		/// <summary>Creates competition config without job and options applied.</summary>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>New competition config without job and options applied.</returns>
		public static ManualCompetitionConfig CreateCompetitionConfigStub(
			Assembly targetAssembly,
			CompetitionFeatures competitionFeatures)
		{
			var result = new ManualCompetitionConfig(BenchmarkDotNet.Configs.DefaultConfig.Instance);

			// DONTTOUCH: competition should not use default
			// Exporters, Loggers and Jobs.
			// These are omitted intentionally.
			result.Jobs.Clear();
			result.Loggers.Clear();
			result.Exporters.Clear();

			if (competitionFeatures != null && targetAssembly != null)
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
					if (competitionFeatures.ImportantInfoLogger)
					{
						result.Add(GetImportantInfoLogger(targetAssembly));
					}
					if (competitionFeatures.DetailedLogger)
					{
						result.Add(GetDetailedLogger(targetAssembly));
					}
				}
			}

			return result;
		}
		#endregion
	}
}