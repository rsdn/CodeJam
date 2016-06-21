using System;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Running.CompetitionLimitProviders;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition config.</summary>
	public interface ICompetitionConfig : IConfig
	{
		#region Runner config - troubleshooting
		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <value><c>true</c> if debug builds allowed; otherwise, <c>false</c>.</value>
		bool AllowDebugBuilds { get; }

		/// <summary>Enable detailed logging.</summary>
		/// <value><c>true</c> if detailed logging enabled.</value>
		bool DetailedLogging { get; }
		#endregion

		#region Runner config
		/// <summary>Total count of reruns allowed. Set this to zero to use default limit (10 runs).</summary>
		/// <value>The upper limits of rerun count.</value>
		int MaxRunsAllowed { get; }

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		bool ReportWarningsAsErrors { get; }
		#endregion

		#region Validation config
		/// <summary>The analyser should ignore existing limit annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing limit annotations.</value>
		bool IgnoreExistingAnnotations { get; }

		/// <summary>The analyser should not warn on benchmark runs that take longer than 0.5 sec to complete.</summary>
		/// <value>
		/// True if the analyser should not warn on benchmark runs that take longer than 0.5 sec to complete.
		/// </value>
		bool AllowLongRunningBenchmarks { get; }

		/// <summary>Rerun competition if competition limits check failed.</summary>
		/// <value><c>true</c> if reruns should be performed if competition limits check failed.</value>
		bool RerunIfLimitsFailed { get; }

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		bool LogCompetitionLimits { get; }

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		ICompetitionLimitProvider CompetitionLimitProvider { get; }
		#endregion

		#region Annotation config
		/// <summary>Try to update source annotations if competition limits check failed.</summary>
		/// <value>
		/// <c>true</c> if the analyser should update source annotations if competition limits check failed; otherwise, <c>false</c>.
		/// </value>
		bool UpdateSourceAnnotations { get; }

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, absolute paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Set <see cref="LogCompetitionLimits"/> <c>true</c> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		string PreviousRunLogUri { get; }
		#endregion
	}
}