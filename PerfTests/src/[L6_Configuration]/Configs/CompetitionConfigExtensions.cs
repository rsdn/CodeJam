using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Expensions for <see cref="ICompetitionConfig"/>
	/// </summary>
	[PublicAPI]
	public static class CompetitionConfigExtensions
	{
		/// <summary>Applies the specified competition options.</summary>
		/// <param name="config">The config.</param>
		/// <param name="optionsModifier">Competition options to apply.</param>
		/// <returns>Config with applied competition options.</returns>
		public static ICompetitionConfig WithModifier(
			this ICompetitionConfig config, CompetitionOptions optionsModifier) =>
				config.With(m => m.ApplyModifier(optionsModifier));

		/// <summary>Applies job modifier.</summary>
		/// <param name="config">The config.</param>
		/// <param name="jobModifier">Job modifier to apply.</param>
		/// <returns>Config with applied job modifier.</returns>
		public static ICompetitionConfig WithModifier(
			this ICompetitionConfig config, Job jobModifier) =>
				config.With(m => m.ApplyModifier(jobModifier));

		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if debug builds allowed.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithAllowDebugBuilds(this ICompetitionConfig config, bool value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					RunOptions =
					{
						AllowDebugBuilds = value
					}
				});

		/// <summary>Enables or disables detailed logging.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if detailed logging enabled.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithDetailedLogging(this ICompetitionConfig config, bool value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					RunOptions =
					{
						DetailedLogging = value
					}
				});

		/// <summary>Report warnings as errors.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if competition warnings should be reported as errors.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithReportWarningsAsErrors(this ICompetitionConfig config, bool value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					RunOptions =
					{
						ReportWarningsAsErrors = value
					}
				});

		/// <summary>Sets timing limit to detect too fast benchmarks.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">Timing limit to detect too fast benchmarks.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithTooFastBenchmarkLimit(this ICompetitionConfig config, TimeSpan value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					Checks =
					{
						TooFastBenchmarkLimit = value
					}
				});

		/// <summary>Sets timing limit to detect long-running benchmarks.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">Timing limit to detect long-running benchmarks.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithLongRunningBenchmarkLimit(this ICompetitionConfig config, TimeSpan value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					Checks =
					{
						LongRunningBenchmarkLimit = value
					}
				});

		/// <summary>URI of the log that contains competition limits from previous run(s).</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="CompetitionAnnotationMode.LogAnnotations"/> to <c>true</c> to enable logged annotations.
		/// </param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithPreviousRunLogUri(this ICompetitionConfig config, string value) =>
			config.WithModifier(
				new CompetitionOptions
				{
					Annotations =
					{
						PreviousRunLogUri = value
					}
				});

		private static ICompetitionConfig With(this IConfig config, Action<ManualCompetitionConfig> addAction)
		{
			var manualConfig = new ManualCompetitionConfig(config);
			addAction(manualConfig);
			return manualConfig.AsReadOnly();
		}
	}
}