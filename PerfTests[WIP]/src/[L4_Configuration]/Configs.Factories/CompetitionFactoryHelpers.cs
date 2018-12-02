using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Loggers;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Helpers;
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
		public static Assembly GetAssembly([CanBeNull] ICustomAttributeProvider metadataSource)
		{
			var type = metadataSource as Type;
			if (type != null)
				return type.Assembly;

			var member = metadataSource as MemberInfo;
			if (member != null)
				return member.DeclaringType?.Assembly;

			return metadataSource as Assembly;
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
				(Assembly a) => new FilteringLogger(
					CreateAssemblyLevelLogger(a, ImportantOnlyLogSuffix),
					FilteringLoggerMode.PrefixedOrErrors),
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
				return new CompetitionFeatures().Freeze();

			var competitionFeatures = GetAssembly(metadataSource)
				?.FeaturesFromAppConfig()
				?.UnfreezeCopy() ??
				new CompetitionFeatures();

			if (RunsUnderContinuousIntegration())
				competitionFeatures.ContinuousIntegrationMode = true;

			foreach (var featureAttribute in metadataSource
				.GetMetadataAttributes<CompetitionFeaturesAttribute>()
				.Reverse())
			{
				competitionFeatures.Apply(featureAttribute.GetFeatures());
			}

			return competitionFeatures.Freeze();
		}
		#endregion

		#region CI detection
		// ReSharper disable CommentTypo
		// ReSharper disable StringLiteralTypo
		/// <summary>
		/// Well-known Continuous Integration services environment variables
		/// </summary>
		public static readonly IReadOnlyList<string> WellKnownCiVariables = new List<string>
		{
			"APPVEYOR",         // AppVeyor
			"CI",               // AppVeyor and Travis CI
			"JENKINS_URL",      // Jenkins
			"TEAMCITY_VERSION", // TeamCity
			"TF_BUILD",         // TFS
			"TRAVIS"            // Travis CI
		}.AsReadOnly();

		// ReSharper restore StringLiteralTypo
		// ReSharper restore CommentTypo

		/// <summary>
		/// Checks that run is performed under continuous integration.
		/// </summary>
		public static bool RunsUnderContinuousIntegration() =>
			EnvironmentHelpers.HasAnyEnvironmentVariable(WellKnownCiVariables.ToArray());
		#endregion

		#region Appcongfig support
		/// <summary>Name of the config section.</summary>
		public const string SectionName = "CodeJam.PerfTests";

		/// <summary>Reads <see cref="CompetitionFeatures"/> from assembly level options config section.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <returns>
		/// <see cref="CompetitionFeatures"/> section filled from first of app.config, <paramref name="targetAssembly"/> or CodeJam.PerfTests assembly.
		/// </returns>
		[CanBeNull]
		private static CompetitionFeatures FeaturesFromAppConfig([NotNull] this Assembly targetAssembly) =>
			FeaturesFromAppConfig(new[] { targetAssembly });

		/// <summary>Reads <see cref="CompetitionFeatures"/> from assembly level options config section.</summary>
		/// <param name="assembliesToCheck">Assemblies to check for the config section if the app.config does not contain the section.</param>
		/// <returns>
		/// <see cref="CompetitionFeatures"/> section filled from first of app.config, <paramref name="assembliesToCheck"/> or CodeJam.PerfTests assembly.
		/// </returns>
		[CanBeNull]
		public static CompetitionFeatures FeaturesFromAppConfig(params Assembly[] assembliesToCheck)
		{
			var section = EnvironmentHelpers.ParseConfigurationSection<PerfTestsSection>(
				SectionName,
				assembliesToCheck.Concat(typeof(CompetitionFactoryHelpers).Assembly));

			return section?.GetFeatures();
		}
		#endregion
	}
}