using System;
using System.IO;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Console competition runner.</summary>
	/// <seealso cref="CompetitionRunnerBase"/>
	public class ConsoleCompetitionRunner : CompetitionRunnerBase
	{
		/// <summary>Host logger implementation</summary>
		protected class ConsoleHostLogger : HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="ConsoleHostLogger"/> class.</summary>
			/// <param name="logMode">Host logging mode.</param>
			public ConsoleHostLogger(HostLogMode logMode) : base(ConsoleLogger.Default, logMode) { }
		}

		// HACK: swallow console output
		// TODO: remove after upgrade to BDN 10.3
#pragma warning disable 1591
		protected class TempLogger : HostLogger
		{
			public TempLogger(HostLogMode logMode) : base(new AccumulationLogger(), logMode) { }

			public string GetLog() => ((AccumulationLogger)WrappedLogger).GetLog();
		}
#pragma warning restore 1591

		/// <summary>Runs the competition - core implementation.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>Competition state for the run.</returns>
		protected override CompetitionState RunCore(Type benchmarkType, ICompetitionConfig competitionConfig)
		{
			// HACK: swallow console output
			// TODO: remove after upgrade to BDN 10.3
			using (BenchmarkHelpers.CaptureConsoleOutput(new StringWriter()))
			{
				return base.RunCore(benchmarkType, competitionConfig);
			}
		}

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new TempLogger(hostLogMode);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportExecutionErrors(string messages, CompetitionState competitionState)
		{
			// TODO: remove after upgrade to BDN 10.3
			var logger = competitionState.Logger;

			logger.WriteLine();
			logger.WriteLineError(messages);
		}

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportAssertionsFailed(string messages, CompetitionState competitionState)
		{
			var logger = competitionState.Logger;

			logger.WriteLine();
			logger.WriteLineError(messages);
		}

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportWarnings(string messages, CompetitionState competitionState)
		{
			var logger = competitionState.Logger;

			logger.WriteLine();
			logger.WriteLineInfo(messages);
		}

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			// TODO: remove after upgrade to BDN 10.3

			// Dumping all captured output
			var outLogger = ConsoleLogger.Default;
			var tempLogger = (TempLogger)logger;
			outLogger.Write(tempLogger.GetLog());

			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, outLogger);
			}
		}
		#endregion

		#region Override config parameters
		/// <summary>Customize competition config.</summary>
		/// <param name="competitionConfig">The competition configuration.</param>
		protected override void InitCompetitionConfigOverride(ManualCompetitionConfig competitionConfig)
		{
			base.InitCompetitionConfigOverride(competitionConfig);
			competitionConfig.Loggers.RemoveAll(l => l is ConsoleLogger);
		}
		#endregion
	}
}