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
		/// <param name="competitionOptions">The competition options to apply.</param>
		/// <returns>Config with applied competition options.</returns>
		public static ICompetitionConfig WithCompetitionOptions(
			this ICompetitionConfig config, CompetitionOptions competitionOptions) =>
				config.With(m => m.ApplyCompetitionOptions(competitionOptions));

		/// <summary>Applies job modifier.</summary>
		/// <param name="config">The config.</param>
		/// <param name="jobModifier">The modifier to apply to the config' jobs.</param>
		/// <param name="preserveId">if set to <c>true</c> job ids are preserved.</param>
		/// <returns>Config with applied job modifier.</returns>
		public static ICompetitionConfig WithJobModifier(
			this ICompetitionConfig config, Job jobModifier, bool preserveId = false) =>
				config.With(m => m.ApplyToJobs(jobModifier, preserveId));

		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if debug builds allowed.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithAllowDebugBuilds(this ICompetitionConfig config, bool value) =>
			config.WithCompetitionOptions(
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
			config.WithCompetitionOptions(
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
			config.WithCompetitionOptions(
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
			config.WithCompetitionOptions(
				new CompetitionOptions
				{
					Limits =
					{
						TooFastBenchmarkLimit = value
					}
				});

		/// <summary>Sets timing limit to detect long-running benchmarks.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">Timing limit to detect long-running benchmarks.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithLongRunningBenchmarkLimit(this ICompetitionConfig config, TimeSpan value) =>
			config.WithCompetitionOptions(
				new CompetitionOptions
				{
					Limits =
					{
						LongRunningBenchmarkLimit = value
					}
				});

		/// <summary>URI of the log that contains competition limits from previous run(s).</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="CompetitionLimitsMode.LogAnnotations"/> to <c>true</c> to enable logged annotations.
		/// </param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithPreviousRunLogUri(this ICompetitionConfig config, string value) =>
			config.WithCompetitionOptions(
				new CompetitionOptions
				{
					SourceAnnotations =
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