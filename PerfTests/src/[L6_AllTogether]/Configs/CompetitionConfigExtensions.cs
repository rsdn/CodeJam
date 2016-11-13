using System;

using BenchmarkDotNet.Configs;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Expensions for <see cref="ICompetitionConfig"/>
	/// </summary>
	public static class CompetitionConfigExtensions
	{
		/// <summary>Applies the specified competition mode.</summary>
		/// <param name="config">The config.</param>
		/// <param name="competitionMode">The competition mode to apply.</param>
		/// <returns>Config with applied competition mode.</returns>
		public static ICompetitionConfig With(this IConfig config, CompetitionMode competitionMode) =>
			config.With(m => m.Add(competitionMode));

		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if debug builds allowed.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithAllowDebugBuilds(this IConfig config, bool value) =>
			config.With(new CompetitionMode { RunMode = { AllowDebugBuilds = value } });

		/// <summary>Enables or disables detailed logging.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if detailed logging enabled.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithDetailedLogging(this IConfig config, bool value) =>
			config.With(new CompetitionMode { RunMode = { DetailedLogging = value } });

		/// <summary>Report warnings as errors.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value"><c>true</c> if competition warnings should be reported as errors.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithReportWarningsAsErrors(this IConfig config, bool value) =>
			config.With(new CompetitionMode { RunMode = { ReportWarningsAsErrors = value } });


		/// <summary>Sets timing limit to detect too fast benchmarks.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">Timing limit to detect too fast benchmarks.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithTooFastBenchmarkLimit(this IConfig config, TimeSpan value) =>
			config.With(new CompetitionMode { Limits = { TooFastBenchmarkLimit = value } });

		/// <summary>Sets timing limit to detect long-running benchmarks.</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">Timing limit to detect long-running benchmarks.</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithLongRunningBenchmarkLimit(this IConfig config, TimeSpan value) =>
			config.With(new CompetitionMode { Limits = { LongRunningBenchmarkLimit = value } });

		/// <summary>URI of the log that contains competition limits from previous run(s).</summary>
		/// <param name="config">The config.</param>
		/// <param name="value">
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="CompetitionLimitsMode.LogAnnotations"/> to <c>true</c> to enable logged annotations.
		/// .</param>
		/// <returns>A new config with applied parameters.</returns>
		public static ICompetitionConfig WithPreviousRunLogUri(this IConfig config, string value) =>
			config.With(new CompetitionMode { SourceAnnotations = { PreviousRunLogUri = value } });

		private static ICompetitionConfig With(this IConfig config, Action<ManualCompetitionConfig> addAction)
		{
			var manualConfig = new ManualCompetitionConfig(config);
			addAction(manualConfig);
			return manualConfig.AsReadOnly();
		}
	}
}