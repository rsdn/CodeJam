using System;

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

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new ConsoleHostLogger(hostLogMode);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportExecutionErrors(string messages, CompetitionState competitionState)
		{
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
			// Do nothing. logger dumps output to the console.
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