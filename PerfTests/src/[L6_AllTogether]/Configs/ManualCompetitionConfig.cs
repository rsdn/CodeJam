using System;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Running.CompetitionLimitProviders;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Class for competition config config creation</summary>
	[PublicAPI]
	public sealed class ManualCompetitionConfig : ManualConfig, ICompetitionConfig
	{
		/// <summary>Merges two configs.</summary>
		/// <param name="globalConfig">The global config.</param>
		/// <param name="localConfig">The local config.</param>
		/// <returns>Merged config instance.</returns>
		public static ManualCompetitionConfig Union(
			ICompetitionConfig globalConfig, IConfig localConfig)
		{
			var competitionConfig = new ManualCompetitionConfig();
			switch (localConfig.UnionRule)
			{
				case ConfigUnionRule.AlwaysUseLocal:
					competitionConfig.Add(localConfig);
					break;
				case ConfigUnionRule.AlwaysUseGlobal:
					competitionConfig.Add(globalConfig);
					break;
				case ConfigUnionRule.Union:
					competitionConfig.Add(globalConfig);
					competitionConfig.Add(localConfig);
					break;
			}
			return competitionConfig;
		}

		#region Ctor & Add()
		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		public ManualCompetitionConfig() { }

		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to init from.</param>
		public ManualCompetitionConfig([CanBeNull] IConfig config)
		{
			if (config != null)
			{
				Add(config);
			}
		}

		// TODO: as override
		/// <summary>Fills properties from the specified config.</summary>
		/// <param name="config">The config to init from.</param>
		public new void Add(IConfig config)
		{
			var competitionConfig = config as ICompetitionConfig;
			if (competitionConfig != null)
			{
				base.Add(config);
				AddCompetitionProperties(competitionConfig);
			}
			else
			{
				base.Add(config);
			}
		}

		private void AddCompetitionProperties(ICompetitionConfig config)
		{
			//Runner config - troubleshooting
			AllowDebugBuilds = config.AllowDebugBuilds;
			DetailedLogging = config.DetailedLogging;

			// Runner config
			MaxRunsAllowed = config.MaxRunsAllowed;
			ReportWarningsAsErrors = config.ReportWarningsAsErrors;

			// Validation config
			DontCheckAnnotations = config.DontCheckAnnotations;
			IgnoreExistingAnnotations = config.IgnoreExistingAnnotations;
			AllowLongRunningBenchmarks = config.AllowLongRunningBenchmarks;
			RerunIfLimitsFailed = config.RerunIfLimitsFailed;
			LogCompetitionLimits = config.LogCompetitionLimits;
			CompetitionLimitProvider = config.CompetitionLimitProvider;

			// Annotation config
			UpdateSourceAnnotations = config.UpdateSourceAnnotations;
			PreviousRunLogUri = config.PreviousRunLogUri;
		}
		#endregion

		#region Runner config - troubleshooting
		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <value><c>true</c> if debug builds allowed; otherwise, <c>false</c>.</value>
		public bool AllowDebugBuilds { get; set; }

		/// <summary>Enable detailed logging.</summary>
		/// <value><c>true</c> if detailed logging enabled.</value>
		public bool DetailedLogging { get; set; }
		#endregion

		#region Runner config
		/// <summary>Total count of reruns allowed. Set this to zero to use default limit (10 runs).</summary>
		/// <value>The upper limits of rerun count.</value>
		public int MaxRunsAllowed { get; set; }

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		public bool ReportWarningsAsErrors { get; set; }
		#endregion

		#region Validation config
		/// <summary>Do not validate competition limits.</summary>
		/// <value><c>true</c> if competition limits should not be validated.</value>
		public bool DontCheckAnnotations { get; set; }

		/// <summary>The analyser should ignore existing limit annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing limit annotations.</value>
		public bool IgnoreExistingAnnotations { get; set; }

		/// <summary>The analyser should warn on benchmarks that take longer than 0.5 sec to complete.</summary>
		/// <value>True if the analyser should warn on benchmarks that take longer than 0.5 sec to complete.</value>
		public bool AllowLongRunningBenchmarks { get; set; }

		/// <summary>Perform reruns if competition limits check failed.</summary>
		/// <value><c>true</c> if reruns should be performed if competition limits check failed.</value>
		public bool RerunIfLimitsFailed { get; set; }

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits { get; set; }

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		public ICompetitionLimitProvider CompetitionLimitProvider { get; set; }
		#endregion

		#region Annotation config
		/// <summary>Try to update source annotations if competition limits check failed.</summary>
		/// <value>
		/// <c>true</c> if the analyser should update source annotations if competition limits check failed; otherwise, <c>false</c>.
		/// </value>
		public bool UpdateSourceAnnotations { get; set; }

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Set <see cref="LogCompetitionLimits"/> <c>true</c> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri { get; set; }
		#endregion

		/// <summary>Returns read-only wrapper for the config.</summary>
		/// <returns>Read-only wrapper for the config</returns>
		public ICompetitionConfig AsReadOnly() => new ReadOnlyCompetitionConfig(this);
	}
}