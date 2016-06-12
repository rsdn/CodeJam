using System;
using System.Collections.Generic;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Competiton runner that does not support integration with unit tests.</summary>
	/// <seealso cref="CompetitionRunnerBase"/>
	[PublicAPI]
	public class SimpleCompetitionRunner : CompetitionRunnerBase
	{
		/// <summary>Host logger implementation</summary>
		protected class SimpleHostLogger : HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="SimpleHostLogger"/> class.</summary>
			/// <param name="logMode">Host logging mode.</param>
			public SimpleHostLogger(HostLogMode logMode)
				: base(ConsoleLogger.Default, logMode) { }

			/// <summary>The logger to redirect the output.</summary>
			/// <value>The logger to redirect the output.</value>
			public new ConsoleLogger WrappedLogger => (ConsoleLogger)base.WrappedLogger;
		}

		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <seealso cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new SimpleHostLogger(hostLogMode);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected override void ReportWarnings(string messages) =>
			ConsoleLogger.Default.WriteLineInfo(messages);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected override void ReportExecutionErrors(string messages) =>
			ConsoleLogger.Default.WriteLineError(messages);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected override void ReportAssertionsFailed(string messages) =>
			ConsoleLogger.Default.WriteLineError(messages);

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			// Do nothing.
		}

		#region Override config parameters
		/// <summary>Override competition exporters.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The jobs for the competition</returns>
		protected override List<IExporter> OverrideExporters(ICompetitionConfig competitionConfig)
		{
			var result = base.OverrideExporters(competitionConfig);
			result.RemoveAll(l => l != MarkdownExporter.Default);

			return result;
		}

		/// <summary>Override competition loggers.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The loggers for the competition</returns>
		protected override List<ILogger> OverrideLoggers(ICompetitionConfig competitionConfig)
		{
			var result = base.OverrideLoggers(competitionConfig);
			result.RemoveAll(l => l is ConsoleLogger);

			return result;
		}
		#endregion
	}
}