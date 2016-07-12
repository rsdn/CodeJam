using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BenchmarkDotNet.Exporters;
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
	public class NUnitCompetitionRunner : CompetitionRunnerBase
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

		#region Override test running behavior
		/// <summary>Returns output directory that should be used for running the test.</summary>
		/// <param name="targetAssembly">The target assembly tests will be run for.</param>
		/// <returns>Output directory that should be used for running the test or <c>null</c> if the current directory should be used.</returns>
		protected override string GetOutputDirectory(Assembly targetAssembly)
		{
			if (TestContext.CurrentContext.WorkDirectory != null)
			{
				return TestContext.CurrentContext.TestDirectory;
			}
			return base.GetOutputDirectory(targetAssembly);
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
			}

			// Dumping all captured output below the benchmark results
			var nUnitLogger = (NUnitHostLogger)logger;
			outLogger.Write(nUnitLogger.GetLog());
		}

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportExecutionErrors(string messages, CompetitionState competitionState) =>
			Assert.Fail(messages);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportAssertionsFailed(string messages, CompetitionState competitionState) =>
			Assert.Fail(messages);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void ReportWarnings(string messages, CompetitionState competitionState) =>
			Assert.Ignore(messages);
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