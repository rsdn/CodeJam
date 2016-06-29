using System;
using System.Linq;

using BenchmarkDotNet.Exporters;
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
	public class TimingsExporter : ExporterBase
	{
		/// <summary>The default instance of the exporter.</summary>
		public static readonly TimingsExporter Instance = new TimingsExporter();

		/// <summary>Prevents a default instance of the <see cref="TimingsExporter"/> class from being created.</summary>
		private TimingsExporter()
		{ }

		/// <summary>File extension.</summary>
		/// <value>The file extension.</value>
		protected override string FileExtension => "csv";

		/// <summary>File caption.</summary>
		/// <value>The file caption.</value>
		protected override string FileCaption => "timings";

		/// <summary>Exports to log.</summary>
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
				Code.AssertState(summaries.LastOrDefault() == summary, "Exporter bug");
			}
			else
			{
				Code.AssertState(summaries.LastOrDefault() != summary, "Exporter bug");
				summaries = summaries.Concat(summary).ToArray();
			}

			var data =
				from sWithIndex in summaries.Select((s, i) => new { s, i })
				from benchmark in sWithIndex.s.GetSummaryOrderBenchmarks()
				from measurement in sWithIndex.s[benchmark].AllMeasurements.Select((m, i) => new { m, i })
				select new
				{
					RunNumber = sWithIndex.i + 1,
					RunSummary = sWithIndex.s,
					Benchmark = benchmark,
					benchmark.Target,
					benchmark.Job,
					benchmark.Parameters,
					MeasurementType = measurement.m.IterationMode,
					MeasurementNum = measurement.i,
					MeasurementLaunch = measurement.m.LaunchIndex,
					MeasurementIteration = measurement.m.IterationIndex,
					MeasurementNs = measurement.m.GetAverageNanoseconds()
				};

			logger.WriteLine(
				"RunNumber;Job;Target;Parameters;" +
					"LaunchNumber;MeasurementNumber;MeasurementType;MeasurementIteration;MeasurementNs");
			foreach (var d in data)
			{
				logger.WriteLine(
					$"{d.RunNumber};{d.Job};{d.Target.MethodTitle};{d.Parameters.FullInfo};" +
						$"{d.MeasurementLaunch};{d.MeasurementNum};{d.MeasurementType};{d.MeasurementIteration};{d.MeasurementNs}");
			}
		}
	}
}