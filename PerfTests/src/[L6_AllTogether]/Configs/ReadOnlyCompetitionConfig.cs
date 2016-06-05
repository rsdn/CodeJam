using System;

using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Wrapper class for readonly competition config.</summary>
	/// <seealso cref="BenchmarkDotNet.Configs.ReadOnlyConfig"/>
	/// <seealso cref="CodeJam.PerfTests.Configs.ICompetitionConfig"/>
	public class ReadOnlyCompetitionConfig : ReadOnlyConfig, ICompetitionConfig
	{
		#region Fields & .ctor
		private readonly ICompetitionConfig _config;

		/// <summary>Initializes a new instance of the <see cref="ReadOnlyCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to wrap.</param>
		public ReadOnlyCompetitionConfig([NotNull] ICompetitionConfig config) : base(config)
		{
			_config = config;
		}
		#endregion

		#region Runner config - troubleshooting
		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <value><c>true</c> if debug builds allowed; otherwise, <c>false</c>.</value>
		public bool AllowDebugBuilds => _config.AllowDebugBuilds;

		/// <summary>Enable detailed logging.</summary>
		/// <value><c>true</c> if detailed logging enabled.</value>
		public bool DetailedLogging => _config.DetailedLogging;
		#endregion

		#region Runner config
		/// <summary>Total count of reruns allowed. Set this to zero to use default limit (10 runs).</summary>
		/// <value>The upper limits of rerun count.</value>
		public int MaxRunsAllowed => _config.MaxRunsAllowed;

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		public bool ReportWarningsAsErrors => _config.ReportWarningsAsErrors;
		#endregion

		#region Validation config
		/// <summary>Do not validate competition limits.</summary>
		/// <value><c>true</c> if competition limits should not be validated.</value>
		public bool DontCheckAnnotations => _config.DontCheckAnnotations;

		/// <summary>The analyser should ignore existing annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing annotations.</value>
		public bool IgnoreExistingAnnotations => _config.IgnoreExistingAnnotations;

		/// <summary>The analyser should warn on benchmarks that take longer than 0.5 sec to complete.</summary>
		/// <value>True if the analyser should warn on benchmarks that take longer than 0.5 sec to complete.</value>
		public bool AllowLongRunningBenchmarks => _config.AllowLongRunningBenchmarks;

		/// <summary>Perform reruns if competition limits failed or updated.</summary>
		/// <value><c>true</c> if reruns should be performed if competition limits failed or updated.</value>
		public bool RerunIfLimitsFailed => _config.RerunIfLimitsFailed;

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits => _config.LogCompetitionLimits;
		#endregion

		#region Annotation config
		/// <summary>Try to annotate source with actual competition limits.</summary>
		/// <value><c>true</c> if the analyser should update source annotations; otherwise, <c>false</c>. </value>
		public bool UpdateSourceAnnotations => _config.UpdateSourceAnnotations;

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Set <seealso cref="LogCompetitionLimits"/> <c>true</c> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri => _config.PreviousRunLogUri;
		#endregion
	}
}