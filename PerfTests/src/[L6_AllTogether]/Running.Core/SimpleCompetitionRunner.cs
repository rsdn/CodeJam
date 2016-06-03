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
	[PublicAPI]
	public class SimpleCompetitionRunner : CompetitionRunnerBase
	{
		protected class SimpleHostLogger : HostLogger
		{
			public SimpleHostLogger(HostLogMode logMode)
				: base(ConsoleLogger.Default, logMode) { }

			public new ConsoleLogger WrappedLogger => (ConsoleLogger)base.WrappedLogger;
		}

		protected override HostLogger CreateHostLogger(HostLogMode hostLogMode) =>
			new SimpleHostLogger(hostLogMode);

		protected override void ReportWarnings(string messages) => ConsoleLogger.Default.WriteLineInfo(messages);

		protected override void ReportExecutionErrors(string messages) => ConsoleLogger.Default.WriteLineError(messages);

		protected override void ReportAssertionsFailed(string messages) => ConsoleLogger.Default.WriteLineError(messages);

		protected override void ReportHostLogger(HostLogger logger, Summary summary)
		{
			// Do nothing.
		}

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