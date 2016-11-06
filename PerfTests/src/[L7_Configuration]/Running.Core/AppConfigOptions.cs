using System;
using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Exporters;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Assembly-level options for the <see cref="AssemblyCompetitionConfig"/>.</summary>
	public class AppConfigOptions
	{
		/// <summary>Enables <see cref="ICompetitionConfig.UpdateSourceAnnotations"/> option.</summary>
		/// <value><c>true</c> if <see cref="ICompetitionConfig.UpdateSourceAnnotations"/> should be enabled.</value>
		public bool AnnotateOnRun { get; set; }

		/// <summary>
		/// Enables <see cref="ICompetitionConfig.IgnoreExistingAnnotations"/> option.
		/// If set to <c>true</c> the <see cref="PreviousRunLogUri"/> value will be ignored.
		/// </summary>
		/// <value>
		/// <c>true</c> if <see cref="ICompetitionConfig.IgnoreExistingAnnotations"/> should be enabled.
		/// </value>
		public bool IgnoreExistingAnnotations { get; set; }

		/// <summary>Enables <see cref="ICompetitionConfig.ReportWarningsAsErrors"/> option.</summary>
		/// <value><c>true</c> if <see cref="ICompetitionConfig.ReportWarningsAsErrors"/> should be enabled.</value>
		public bool ReportWarningsAsErrors { get; set; }

		/// <summary>
		/// Enables <see cref="ICompetitionConfig.DetailedLogging"/> and <see cref="ICompetitionConfig.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Also, the <see cref="Loggers"/> will be threated as if set to <seealso cref="AppConfigLoggers.Both"/>.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
		public bool TroubleshootingMode { get; set; }

		/// <summary>
		/// Assembly-level loggers that should be used. Check the <see cref="AppConfigLoggers"/> for possible values.
		/// </summary>
		/// <value>Assembly-level loggers that should be used.</value>
		public AppConfigLoggers Loggers { get; set; }

		/// <summary>Sets the <see cref="ICompetitionConfig.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="ICompetitionConfig.PreviousRunLogUri"/>.</value>
		public string PreviousRunLogUri { get; set; }

		/// <summary>Target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		public Platform? TargetPlatform { get; set; }
	}
}