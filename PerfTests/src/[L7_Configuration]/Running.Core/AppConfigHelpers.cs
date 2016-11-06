using System;
using System.IO;
using System.Reflection;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helpers for assembly-level options
	/// </summary>
	[PublicAPI]
	public static class AppConfigHelpers
	{
		/// <summary>Name of the config section.</summary>
		public const string SectionName = "CodeJam.PerfTests";

		/// <summary>Reads assembly level options config section.</summary>
		/// <param name="assembliesToCheck">Assemblies to check for the config section if the app.config does not contain the section.</param>
		/// <returns>
		/// Configuration section from the app.config, <paramref name="assembliesToCheck"/> or CodeJam.PerfTests assembly (first wins).
		/// </returns>
		public static AppConfigOptions GetAppConfigOptions(
			params Assembly[] assembliesToCheck)
		{
			var section = BenchmarkHelpers.ParseConfigurationSection<PerfTestsSection>(
				SectionName,
				assembliesToCheck.Concat(typeof(CompetitionRunnerBase).Assembly));

			return section == null
				? null
				: new AppConfigOptions
				{
					AnnotateOnRun = section.AnnotateOnRun,
					IgnoreExistingAnnotations = section.IgnoreExistingAnnotations,
					ReportWarningsAsErrors = section.ReportWarningsAsErrors,
					TroubleshootingMode = section.TroubleshootingMode,
					Loggers = section.Loggers,
					PreviousRunLogUri = section.PreviousRunLogUri,
					TargetPlatform = section.TargetPlatform
				};
		}

		/// <summary>Reusable factory method for the config.</summary>
		/// <param name="targetAssembly">The assembly for which the config should be created.</param>
		/// <param name="createOptions">Assembly-level options.</param>
		/// <param name="job">The job for the config. Default one will be used if <c>null</c>.</param>
		/// <returns>Config for the competition.</returns>
		[NotNull]
		public static ManualCompetitionConfig CreateAppCompetitionConfig(
			[NotNull] Assembly targetAssembly,
			[CanBeNull] AppConfigOptions createOptions,
			[CanBeNull] Job job = null)
		{
			Code.NotNull(targetAssembly, nameof(targetAssembly));

			if (createOptions == null)
				return new ManualCompetitionConfig(CreateDefaultConfig(job));

			if (job == null)
				job = CreateDefaultJob(createOptions.TargetPlatform);

			ManualCompetitionConfig result;
			if (!createOptions.AnnotateOnRun)
			{
				result = CreateDefaultConfig(job);
			}
			else if (!createOptions.IgnoreExistingAnnotations)
			{
				result = CreateDefaultConfigAnnotate(job);
				result.PreviousRunLogUri = createOptions.PreviousRunLogUri;
			}
			else
			{
				result = CreateDefaultConfigReannotate(job);
			}

			if (createOptions.TroubleshootingMode)
			{
				result.DetailedLogging = true;
				result.Add(
					GetDetailedLogger(targetAssembly),
					GetImportantOnlyLogger(targetAssembly));
				result.Add(Exporters.CsvTimingsExporter.Default);
			}
			else
			{
				if (createOptions.Loggers.IsFlagSet(AppConfigLoggers.Detailed))
				{
					result.Add(GetDetailedLogger(targetAssembly));
				}
				if (createOptions.Loggers.IsFlagSet(AppConfigLoggers.ImportantOnly))
				{
					result.Add(GetImportantOnlyLogger(targetAssembly));
				}
			}

			result.ReportWarningsAsErrors = createOptions.ReportWarningsAsErrors;

			return result;
		}

		#region Loggers
		/// <summary>The detailed log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string DetailedLogSuffix = "." + nameof(AppConfigLoggers.Detailed) + ".PerfTests.log";

		/// <summary>The important only log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string ImportantOnlyLogSuffix = "." + nameof(AppConfigLoggers.ImportantOnly) + ".PerfTests.log";

		#region Logger factories
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
		#endregion

		/// <summary>Helper for custom configs: creates detailed logger for current assembly.</summary>
		/// <param name="targetAssembly">Assembly with performance tests.</param>
		/// <returns>Detailed logger for assembly.</returns>
		public static ILogger GetDetailedLogger([NotNull] Assembly targetAssembly) =>
			_detailedLoggersCache(targetAssembly);

		/// <summary>Helper for custom configs: creates important info logger for current assembly.</summary>
		/// <param name="targetAssembly">Assembly with performance tests.</param>
		/// <returns>Important info logger for assembly.</returns>
		public static ILogger GetImportantOnlyLogger([NotNull] Assembly targetAssembly) =>
			_importantOnlyLoggersCache(targetAssembly);
		#endregion
	}
}