using System;

using BenchmarkDotNet.Loggers;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Exporters;

using JetBrains.Annotations;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam
{
	/// <summary>
	/// Use this to run fast but inaccurate measures
	/// OPTIONAL: Updates source files with actual min..max ratio for [CompetitionBenchmark]
	/// </summary>
	[PublicAPI]
	public sealed class AssemblyWideConfig : ReadOnlyCompetitionConfig
	{
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.AnnotateOnRun=true in app.config
		/// to enable auto-annotation of benchmark methods
		/// </summary>
		public static readonly bool AnnotateOnRun = AppSwitch.GetAssemblySwitch(() => AnnotateOnRun);

		/// <summary>
		/// OPTIONAL: log timings fort troubleshooting
		/// </summary>
		public static readonly bool TroubleshootingMode = AppSwitch.GetAssemblySwitch(() => TroubleshootingMode);

		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.IgnoreAnnotatedLimits=true in app.config
		/// to enable ignoring existing limits on auto-annotation of benchmark methods
		/// </summary>
		public static readonly new bool IgnoreExistingAnnotations =
			AppSwitch.GetAssemblySwitch(() => IgnoreExistingAnnotations);

		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.ReportWarningsAsErrors=true in app.config
		/// to enable reporting warnings as errors.
		/// </summary>
		public static readonly new bool ReportWarningsAsErrors = AppSwitch.GetAssemblySwitch(() => ReportWarningsAsErrors);

		/// <summary>
		/// Instance of the config
		/// </summary>
		public static ICompetitionConfig RunConfig => new AssemblyWideConfig();

		private static readonly Lazy<ILogger> _detailedLogger =
			new Lazy<ILogger>(CreateDetailedLogger);
		private static readonly Lazy<ILogger> _importantInfoLogger =
			new Lazy<ILogger>(CreateImportantInfoLogger);

		/// <summary>
		/// Constructor
		/// </summary>
		[UsedImplicitly]
		public AssemblyWideConfig() : base(Create()) { }

		private static ManualCompetitionConfig Create()
		{
			ManualCompetitionConfig result;
			if (!AnnotateOnRun)
			{
				result = CreateRunConfig();
			}
			else if (IgnoreExistingAnnotations)
			{
				result = CreateRunConfigReAnnotate();
			}
			else
			{
				result = CreateRunConfigAnnotate();
#if !CI_Build
				result.PreviousRunLogUri =
					@"https://ci.appveyor.com/api/projects/andrewvk/codejam/artifacts/CodeJam-Tests.Performance.Short.AllPerfTests.log";
#endif
			}

			result.ReportWarningsAsErrors = ReportWarningsAsErrors;
			if (TroubleshootingMode)
			{
				result.Add(TimingsExporter.Instance);
				result.Add(_detailedLogger.Value);
			}
			result.Add(_importantInfoLogger.Value);

			return result;
		}
	}
}