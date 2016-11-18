using System;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;

using CodeJam.Collections;
using CodeJam.PerfTests.Loggers;
using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>
	/// Reusable API for creating competition config.
	/// </summary>
	[PublicAPI]
	public static class CompetitionFactoryHelpers
	{
		#region Helpers
		/// <summary>Returns assembly for the metadata source.</summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <returns>Assembly for the metadata source</returns>
		[CanBeNull]
		public static Assembly GetAssembly([CanBeNull]ICustomAttributeProvider metadataSource)
		{
			var type = metadataSource as Type;
			if (type != null)
				return type.Assembly;

			var member = metadataSource as MemberInfo;
			if (member != null)
				return member.DeclaringType?.Assembly;

			return metadataSource as Assembly;
		}

		private static void InitFrom(this CompetitionFeatures result, CompetitionFeaturesAttribute source)
		{
			result.TargetPlatform = source.TargetPlatform ?? result.TargetPlatform;

			result.AnnotateSources = source.AnnotateSources ?? result.AnnotateSources;
			result.IgnoreExistingAnnotations = source.IgnoreExistingAnnotations ?? result.IgnoreExistingAnnotations;
			result.PreviousRunLogUri = source.PreviousRunLogUri ?? result.PreviousRunLogUri;

			result.ReportWarningsAsErrors = source.ReportWarningsAsErrors ?? result.ReportWarningsAsErrors;

			result.TroubleshootingMode = source.TroubleshootingMode ?? result.TroubleshootingMode;
			result.ImportantInfoLogger = source.ImportantInfoLogger ?? result.ImportantInfoLogger;
			result.DetailedLogger = source.DetailedLogger ?? result.DetailedLogger;
		}

		private static void InitFrom(this CompetitionFeatures result, PerfTestsSection source)
		{
			result.TargetPlatform = source.TargetPlatform;

			result.AnnotateSources = source.AnnotateSources;
			result.IgnoreExistingAnnotations = source.IgnoreExistingAnnotations;
			result.PreviousRunLogUri = source.PreviousRunLogUri;

			result.ReportWarningsAsErrors = source.ReportWarningsAsErrors;

			result.TroubleshootingMode = source.TroubleshootingMode;
			result.ImportantInfoLogger = source.ImportantInfoLogger;
			result.DetailedLogger = source.DetailedLogger;
		}

		private static void InitFrom(this CompetitionFeatures result, CompetitionFeatures source)
		{
			result.TargetPlatform = source.TargetPlatform;

			result.AnnotateSources = source.AnnotateSources;
			result.IgnoreExistingAnnotations = source.IgnoreExistingAnnotations;
			result.PreviousRunLogUri = source.PreviousRunLogUri;

			result.ReportWarningsAsErrors = source.ReportWarningsAsErrors;

			result.TroubleshootingMode = source.TroubleshootingMode;
			result.ImportantInfoLogger = source.ImportantInfoLogger;
			result.DetailedLogger = source.DetailedLogger;
		}
		#endregion

		#region Loggers
		/// <summary>The detailed log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string DetailedLogSuffix = ".Detailed.PerfTests.log";

		/// <summary>The important only log extension suffix. Can be used as a condition for CI build artifacts</summary>
		public const string ImportantOnlyLogSuffix = ".ImportantOnly.PerfTests.log";

		/// <summary>Gets the important information logger.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <returns>Important information logger for the assembly</returns>
		[NotNull]
		public static ILogger GetImportantInfoLogger([NotNull] Assembly targetAssembly)
		{
			Code.NotNull(targetAssembly, nameof(targetAssembly));
			return _importantOnlyLoggersCache(targetAssembly);
		}

		/// <summary>Gets the detailed logger.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <returns>Detailed logger for the assembly.</returns>
		[NotNull]
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

		#region CompetitionFeatures
		/// <summary>
		/// Creates competition features object.
		/// If <paramref name="metadataSource"/> is <c>null</c>, new instance of competition features is returned.
		/// Else, competition features are obtained from app.config, if any
		/// and are updated with values from <see cref="CompetitionFeaturesAttribute"/>, if any.
		/// </summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <returns>A new instance of competition features object</returns>
		[NotNull]
		public static CompetitionFeatures GetCompetitionFeatures(
			[CanBeNull] ICustomAttributeProvider metadataSource)
		{
			if (metadataSource == null)
				return new CompetitionFeatures();

			var competitionFeatures = GetAssembly(metadataSource)?.FeaturesFromAppConfig() ??
				new CompetitionFeatures();

			foreach (var featureAttribute in metadataSource
				.GetMetadataAttributes<CompetitionFeaturesAttribute>()
				.Reverse())
			{
				competitionFeatures.InitFrom(featureAttribute);
			}

			return competitionFeatures;
		}


		/// <summary>Creates copy of the competition features.</summary>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Copy of the competition features.</returns>
		[NotNull]
		public static CompetitionFeatures CopyCompetitionFeatures(CompetitionFeatures competitionFeatures)
		{
			var result = new CompetitionFeatures();
			result.InitFrom(competitionFeatures);
			return result;
		}
		#endregion

		#region Appcongfig support
		/// <summary>Name of the config section.</summary>
		public const string SectionName = "CodeJam.PerfTests";

		/// <summary>Reads <see cref="CompetitionFeatures"/> from assembly level options config section.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <returns>
		///  <see cref="CompetitionFeatures"/> section filled from first of app.config, <paramref name="targetAssembly"/> or CodeJam.PerfTests assembly.
		/// </returns>
		[CanBeNull]
		private static CompetitionFeatures FeaturesFromAppConfig([NotNull] this Assembly targetAssembly) =>
			FeaturesFromAppConfig(new[] { targetAssembly });

		/// <summary>Reads <see cref="CompetitionFeatures"/> from assembly level options config section.</summary>
		/// <param name="assembliesToCheck">Assemblies to check for the config section if the app.config does not contain the section.</param>
		/// <returns>
		///  <see cref="CompetitionFeatures"/> section filled from first of app.config, <paramref name="assembliesToCheck"/> or CodeJam.PerfTests assembly.
		/// </returns>
		[CanBeNull]
		public static CompetitionFeatures FeaturesFromAppConfig(params Assembly[] assembliesToCheck)
		{
			var section = BenchmarkHelpers.ParseConfigurationSection<PerfTestsSection>(
				SectionName,
				assembliesToCheck.Concat(typeof(CompetitionFactoryHelpers).Assembly));

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

	}
}