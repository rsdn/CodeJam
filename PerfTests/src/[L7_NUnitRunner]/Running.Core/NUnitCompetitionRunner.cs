using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using NUnit.Framework;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Nunit competition performance tests runner.</summary>
	/// <seealso cref="CompetitionRunnerBase"/>
	[SuppressMessage("ReSharper", "ConvertToExpressionBodyWhenPossible")]
	internal class NUnitCompetitionRunner : CompetitionRunnerBase
	{
		/// <summary>Host logger implementation</summary>
		protected class NUnitHostLogger : HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="NUnitHostLogger"/> class.</summary>
			/// <param name="logMode">Host logging mode.</param>
			public NUnitHostLogger(HostLogMode logMode) : base(new AccumulationLogger(), logMode) { }

			/// <summary>Get string with the log content.</summary>
			/// <returns>String with the log content.</returns>
			public string GetLog() => ((AccumulationLogger)WrappedLogger).GetLog();
		}

		#region Overrides of CompetitionRunnerBase
		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		public override CompetitionState RunCompetition(Type benchmarkType, ICompetitionConfig competitionConfig)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: https://github.com/nunit/nunit3-vs-adapter/issues/38
				// NUnit 3.0 does not alter current directory at all.
				// So we had to do it ourselves.
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
				}

				return base.RunCompetition(benchmarkType, competitionConfig);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="CompetitionRunnerBase.HostLogger"/></returns>
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new NUnitHostLogger(hostLogMode);

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			var outLogger = ConsoleLogger.Default;
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, outLogger);

				outLogger.WriteLine();
				outLogger.WriteSeparatorLine();
				outLogger.WriteLine();
			}

			// Dumping all captured output below the benchmark results
			var nUnitLogger = (NUnitHostLogger)logger;
			outLogger.Write(nUnitLogger.GetLog());
		}

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportExecutionErrors(string messages, HostLogger hostLogger) => Assert.Fail(messages);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportAssertionsFailed(string messages, HostLogger hostLogger) => Assert.Fail(messages);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected override void ReportWarnings(string messages, HostLogger hostLogger) => Assert.Ignore(messages);
		#endregion

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