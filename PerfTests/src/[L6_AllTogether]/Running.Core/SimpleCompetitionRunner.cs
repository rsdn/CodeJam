using System;
using System.Collections.Generic;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Competition runner that does not support integration with unit tests.</summary>
	/// <seealso cref="CompetitionRunnerBase"/>
	[PublicAPI]
	public class SimpleCompetitionRunner : CompetitionRunnerBase
	{
		/// <summary>Host logger implementation</summary>
		protected class SimpleHostLogger : HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="SimpleHostLogger"/> class.</summary>
			/// <param name="logMode">Host logging mode.</param>
			public SimpleHostLogger(HostLogMode logMode) : base(ConsoleLogger.Default, logMode) { }
		}

		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new SimpleHostLogger(hostLogMode);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportExecutionErrors(string messages, HostLogger hostLogger)
		{
			hostLogger.WrappedLogger.WriteSeparatorLine();
			hostLogger.WrappedLogger.WriteLineError(messages);
		}

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportAssertionsFailed(string messages, HostLogger hostLogger)
		{
			hostLogger.WrappedLogger.WriteSeparatorLine();
			hostLogger.WrappedLogger.WriteLineError(messages);
		}

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportWarnings(string messages, HostLogger hostLogger)
		{
			hostLogger.WrappedLogger.WriteSeparatorLine();
			hostLogger.WrappedLogger.WriteLineInfo(messages);
		}

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			if (logger.LogMode != HostLogMode.AllMessages && summary != null)
			{
				// Dumping the benchmark results to console
				var outLogger = logger.WrappedLogger;

				outLogger.WriteSeparatorLine();
				MarkdownExporter.Console.ExportToLog(summary, outLogger);
			}
		}

		#region Override config parameters
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