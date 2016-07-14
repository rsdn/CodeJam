using System;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Wrapper class for readonly competition config.</summary>
	/// <seealso cref="ReadOnlyConfig"/>
	/// <seealso cref="ICompetitionConfig"/>
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
		/// <summary>
		/// Total count of reruns allowed. Set this to zero to use default limit of 10 runs.
		/// (limit value can be overriden by <see cref="CodeJam.PerfTests.Running.Core.CompetitionRunnerBase"/> implementation).
		/// </summary>
		/// <value>The upper limits of rerun count.</value>
		public int MaxRunsAllowed => _config.MaxRunsAllowed;

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		public bool ReportWarningsAsErrors => _config.ReportWarningsAsErrors;

		/// <summary>Behavior for concurrent competition runs.</summary>
		/// <value>Behavior for concurrent competition runs.</value>
		public ConcurrentRunBehavior ConcurrentRunBehavior => _config.ConcurrentRunBehavior;
		#endregion

		#region Validation config
		/// <summary>The analyser should ignore existing limit annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing limit annotations.</value>
		public bool IgnoreExistingAnnotations => _config.IgnoreExistingAnnotations;

		/// <summary>
		/// The analyser should not warn on benchmark runs that take longer than 0.5 sec to complete.
		/// (limit value can be overriden by <see cref="CodeJam.PerfTests.Running.Core.CompetitionRunnerBase"/> implementation).
		/// </summary>
		/// <value>
		/// True if the analyser should not warn on benchmark runs that take longer than 0.5 sec to complete.
		/// </value>
		public bool AllowLongRunningBenchmarks => _config.AllowLongRunningBenchmarks;

		/// <summary>Rerun competition if competition limits check failed.</summary>
		/// <value><c>true</c> if reruns should be performed if competition limits check failed.</value>
		public bool RerunIfLimitsFailed => _config.RerunIfLimitsFailed;

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits => _config.LogCompetitionLimits;

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		public ICompetitionLimitProvider CompetitionLimitProvider => _config.CompetitionLimitProvider;
		#endregion

		#region Annotation config
		/// <summary>Try to update source annotations if competition limits check failed.</summary>
		/// <value>
		/// <c>true</c> if the analyser should update source annotations if competition limits check failed; otherwise, <c>false</c>.
		/// </value>
		public bool UpdateSourceAnnotations => _config.UpdateSourceAnnotations;

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, absolute paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Set <see cref="LogCompetitionLimits"/> <c>true</c> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri => _config.PreviousRunLogUri;
		#endregion
	}
}