using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using NUnit.Framework;

namespace BenchmarkDotNet.Running.Competitions.Core
{
	[SuppressMessage("ReSharper", "ConvertToExpressionBodyWhenPossible")]
	internal class NUnitCompetitionRunner : CompetitionRunnerBase
	{
		private class FakeConsoleLogger : AccumulationLogger { }

		private static FakeConsoleLogger InitFakeConsoleLogger()
		{
			var logger = new FakeConsoleLogger();
			logger.WriteLine();
			logger.WriteLine();
			logger.WriteLine(new string('=', 40));
			logger.WriteLine();
			return logger;
		}

		private static void DumpOutputSummaryAtTop(Summary summary, FakeConsoleLogger logger)
		{
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, ConsoleLogger.Default);
			}

			// Dumping all captured output below the benchmark results
			ConsoleLogger.Default.WriteLine(logger.GetLog());
		}

		#region Override config
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

			result.Insert(0, InitFakeConsoleLogger());

			return result;
		}
		#endregion

		#region Core logic
		protected override void OnAfterRun(bool runSucceed, IConfig competitionConfig, Summary summary)
		{
			base.OnAfterRun(runSucceed, competitionConfig, summary);

			var fakeConsoleLogger = competitionConfig.GetLoggers().OfType<FakeConsoleLogger>().Single();
			DumpOutputSummaryAtTop(summary, fakeConsoleLogger);
		}

		protected override void ReportAsError(string messages)
		{
			throw new AssertionException(messages);
		}

		protected override void ReportAsWarning(string messages)
		{
			throw new IgnoreException(messages);
		}
		#endregion
	}
}