using System;

using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Exporters;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Helper interface to sync members and xml documentation across competition feature sources
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal interface ICompetitionFeatures
	{
		/// <summary>
		/// Performs single run per measurement.
		/// Recommended for use if single call time >> than timer resolution (recommended minimum is 1000 ns).
		/// </summary>
		/// <value>.Descriptor platform for the competition.</value>
		bool BurstMode { get; }

		/// <summary>
		/// The code is being run on a CI server.
		/// <seealso cref="CompetitionAnnotationMode.LogAnnotations"/>,
		/// <seealso cref="CompetitionAnnotationMode.DontSaveUpdatedAnnotations"/>
		/// and <see cref="CompetitionRunMode.ContinuousIntegrationMode"/> are enabled,
		/// <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value>
		/// <c>true</c> if the code is being run on a CI server.
		/// </value>
		bool ContinuousIntegrationMode { get; }

		/// <summary>Specifies descriptor platform for the competition.</summary>
		/// <value>.Descriptor platform for the competition.</value>
		Platform Platform { get; }

		/// <summary>Enables source annotations feature.</summary>
		/// <value><c>true</c> if source annotations feature should be enabled.</value>
		bool AnnotateSources { get; }

		/// <summary>
		/// Ignores existing annotations if <see cref="AnnotateSources"/> is enabled.
		/// Value of <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if reannotation feature should be enabled.</value>
		bool IgnoreExistingAnnotations { get; }

		/// <summary>Sets the <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/>.</value>
		string PreviousRunLogUri { get; }

		/// <summary>Fails tests if there are any warnings.</summary>
		/// <value>
		/// <c>true</c> if <see cref="CompetitionRunMode.ReportWarningsAsErrors"/> should be set to true.
		/// </value>
		bool ReportWarningsAsErrors { get; }

		/// <summary>
		/// Enables <see cref="CompetitionRunMode.DetailedLogging"/> and <see cref="CompetitionRunMode.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Adds important info and detailed info loggers.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
		bool TroubleshootingMode { get; }

		/// <summary>Enables important info logger.</summary>
		/// <value><c>true</c> if important info logger should be used.</value>
		bool ImportantInfoLogger { get; }

		/// <summary>Enables detailed logger.</summary>
		/// <value><c>true</c> if detailed logger should be used.</value>
		bool DetailedLogger { get; }
	}
}