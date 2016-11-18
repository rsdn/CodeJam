using System;

using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Exporters;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Features for competition.</summary>
	public sealed class CompetitionFeatures
	{
		#region Environment
		/// <summary>Specifies target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		public Platform? TargetPlatform { get; set; }
		#endregion

		#region Annotations
		/// <summary>Enables source annotations feature.</summary>
		/// <value><c>true</c> if source annotations feature should be enabled.</value>
		public bool AnnotateSources { get; set; }

		/// <summary>
		/// Ignores existing annotations if <see cref="AnnotateSources"/> is enabled.
		/// Value of <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if reannotation feature should be enabled.</value>
		public bool IgnoreExistingAnnotations { get; set; }

		/// <summary>Sets the <see cref="SourceAnnotationsMode.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="SourceAnnotationsMode.PreviousRunLogUri"/>.</value>
		public string PreviousRunLogUri { get; set; }
		#endregion

		#region Troubleshooting
		/// <summary>Fails tests if there are any warnings.</summary>
		/// <value>
		/// <c>true</c> if <see cref="CompetitionRunMode.ReportWarningsAsErrors"/> should be set to true.
		/// </value>
		public bool ReportWarningsAsErrors { get; set; }

		/// <summary>
		/// Enables <see cref="CompetitionRunMode.DetailedLogging"/> and <see cref="CompetitionRunMode.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Adds important info and detailed info loggers.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
		public bool TroubleshootingMode { get; set; }

		/// <summary>Enables important info logger.</summary>
		/// <value><c>true</c> if important info logger should be used.</value>
		public bool ImportantInfoLogger { get; set; }

		/// <summary>Enables detailed logger.</summary>
		/// <value><c>true</c> if detailed logger should be used.</value>
		public bool DetailedLogger { get; set; }
		#endregion
	}
}