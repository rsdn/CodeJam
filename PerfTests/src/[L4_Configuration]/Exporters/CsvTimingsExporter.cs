using System;
using System.Linq;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Exporters
{
	/// <summary>Exporter that creates .csv files with timings for all runs in the competition.</summary>
	/// <seealso cref="BenchmarkDotNet.Exporters.IExporter"/>
	[PublicAPI]
	public class CsvTimingsExporter : ExporterBase
	{
		/// <summary>The default exporter instance</summary>
		public static readonly IExporter Default = new CsvTimingsExporter(CsvSeparator.Semicolon);

		private readonly string _separator;

		/// <summary>Initializes a new instance of the <see cref="CsvTimingsExporter"/> class.</summary>
		public CsvTimingsExporter() : this(CsvSeparator.Semicolon) { }

		/// <summary>Initializes a new instance of the <see cref="CsvTimingsExporter"/> class.</summary>
		/// <param name="separator">The separator.</param>
		public CsvTimingsExporter(CsvSeparator separator)
		{
			_separator = separator.ToRealSeparator();
		}

		/// <summary>File extension.</summary>
		/// <value>The file extension.</value>
		protected override string FileExtension => "csv";

		/// <summary>File caption.</summary>
		/// <value>The file caption.</value>
		protected override string FileCaption => "timings";

		/// <summary>Exports summary to log.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="logger">The logger.</param>
		public override void ExportToLog([NotNull] Summary summary, [NotNull] ILogger logger)
		{
			Code.NotNull(summary, nameof(summary));
			Code.NotNull(logger, nameof(logger));
			var competitionState = CompetitionCore.RunState[summary];
			var summaries = competitionState.SummaryFromAllRuns;
			if (competitionState.Completed)
			{
				Code.BugIf(summaries.LastOrDefault() != summary, "Exporter bug");
			}
			else
			{
				Code.BugIf(summaries.LastOrDefault() == summary, "Exporter bug");
				summaries = summaries.Concat(summary).ToArray();
			}

			var c = HostEnvironmentInfo.MainCultureInfo;
			var data =
				from sWithIndex in summaries.Select((s, i) => new { s, i })
				from benchmark in sWithIndex.s.GetSummaryOrderBenchmarks()
				from measurement in sWithIndex.s[benchmark].AllMeasurements.Select((m, i) => new { m, i })
				select new[]
				{
					(sWithIndex.i + 1).ToString(c),
					CsvHelper.Escape(benchmark.Job.ToString(), _separator),
					CsvHelper.Escape(benchmark.Target.MethodDisplayInfo, _separator),
					CsvHelper.Escape(benchmark.Parameters.DisplayInfo, _separator),
					measurement.m.LaunchIndex.ToString(c),
					measurement.i.ToString(c),
					measurement.m.IterationMode.ToString(),
					measurement.m.IterationIndex.ToString(c),
					measurement.m.GetAverageNanoseconds().ToString(c)
				};

			var headers = new[]
			{
				"RunNumber",
				"Job",
				"Target",
				"Parameters",
				"LaunchNumber",
				"MeasurementNumber",
				"MeasurementType",
				"MeasurementIteration",
				"MeasurementNs"
			};

			logger.WriteLine(string.Join(_separator, headers));
			foreach (var lineText in data)
			{
				logger.WriteLine(string.Join(_separator, lineText));
			}
		}
	}
}