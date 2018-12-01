using System;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core.Xunit;

using Xunit;
using Xunit.Sdk;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>xUnit competition performance tests runner.</summary>
	/// <seealso cref="CompetitionRunnerBase"/>
	public class XunitCompetitionRunner : CompetitionRunnerBase
	{
		/// <summary>Host logger implementation</summary>
		protected class XunitHostLogger : HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="XunitHostLogger"/> class.</summary>
			/// <param name="logMode">Host logging mode.</param>
			public XunitHostLogger(FilteringLoggerMode logMode) : base(new AccumulationLogger(), logMode) { }

			/// <summary>Get string with the log content.</summary>
			/// <returns>String with the log content.</returns>
			public string GetLog() => ((AccumulationLogger)WrappedLogger).GetLog();
		}

		#region Override test running behavior
		/// <summary>Gets a value indicating whether the last run summary should be dumped into host logger.</summary>
		/// <value>
		/// <c>true</c> if the last run summary should be dumped into host logger; otherwise, <c>false</c>.
		/// </value>
		protected override bool DumpSummaryToHostLogger => false;
		#endregion

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(FilteringLoggerMode hostLogMode) =>
			new XunitHostLogger(hostLogMode);

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			var outLogger = new LazySynchronizedStreamLogger(() => CompetitionFactTestCase.Output);
			if (summary != null)
			{
				var state = CompetitionCore.RunState[summary];
				if (!state.CompletedWithoutWarnings)
				{
					outLogger.WriteLine();
				}
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, outLogger);
			}

			// Dumping all captured output below the benchmark results
			var xunitLogger = (XunitHostLogger)logger;
			outLogger.Write(xunitLogger.GetLog());
		}

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportExecutionErrors(string messages, CompetitionState competitionState)
		{
			throw new XunitException(messages + Environment.NewLine);
		}

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportAssertionsFailed(string messages, CompetitionState competitionState)
		{
			throw new XunitException(messages + Environment.NewLine);
		}

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportWarnings(string messages, CompetitionState competitionState)
		{
			throw new SkipTestException(messages + Environment.NewLine);
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