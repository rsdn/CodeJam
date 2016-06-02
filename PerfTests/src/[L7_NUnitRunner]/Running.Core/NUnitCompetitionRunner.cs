using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using NUnit.Framework;

namespace CodeJam.PerfTests.Running.Core
{
	[SuppressMessage("ReSharper", "ConvertToExpressionBodyWhenPossible")]
	internal class NUnitCompetitionRunner : CompetitionRunnerBase
	{
		protected class NUnitHostLogger : HostLogger
		{
			public NUnitHostLogger(HostLogMode logMode)
				: base(new AccumulationLogger(), logMode) { }

			protected new AccumulationLogger WrappedLogger => (AccumulationLogger)base.WrappedLogger;

			public string GetLog() => WrappedLogger.GetLog();
		}

		#region Overrides of CompetitionRunnerBase
		public override CompetitionState RunCompetition(Type benchmarkType, ICompetitionConfig competitionConfig)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: https://github.com/nunit/nunit3-vs-adapter/issues/38
				// NUnit 3.0 does not alter current directory at all.
				// So, we had to do it ourselves.
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.WorkDirectory;
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
		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new NUnitHostLogger(hostLogMode);

		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			var outLogger = ConsoleLogger.Default;
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, outLogger);
				outLogger.WriteLine();
				outLogger.WriteLine();
				outLogger.WriteLine(new string('=', 40));
				outLogger.WriteLine();
			}

			// Dumping all captured output below the benchmark results
			var nUnitLogger = (NUnitHostLogger)logger;
			outLogger.Write(nUnitLogger.GetLog());
		}

		protected override void ReportExecutionErrors(string messages)
		{
			throw new AssertionException(messages);
		}

		protected override void ReportAssertionsFailed(string messages)
		{
			throw new AssertionException(messages);
		}

		protected override void ReportWarnings(string messages)
		{
			throw new IgnoreException(messages);
		}
		#endregion

		#region Override config parameters
		// TODO: do not filter the exporters?
		protected override List<IExporter> OverrideExporters(ICompetitionConfig baseConfig)
		{
			var result = base.OverrideExporters(baseConfig);
			result.RemoveAll(l => l != MarkdownExporter.Default);

			return result;
		}

		protected override List<ILogger> OverrideLoggers(ICompetitionConfig baseConfig)
		{
			var result = base.OverrideLoggers(baseConfig);
			result.RemoveAll(l => l is ConsoleLogger);

			return result;
		}
		#endregion
	}
}