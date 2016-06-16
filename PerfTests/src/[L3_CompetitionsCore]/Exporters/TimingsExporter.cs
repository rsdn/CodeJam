using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Exporters
{
	public class TimingsExporter : IExporter
	{
		public void ExportToLog([NotNull] Summary summary, [NotNull] ILogger logger)
		{
			// do nothing;
		}

		public IEnumerable<string> ExportToFiles([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var summaries = CompetitionCore.RunState[summary].SummaryFromAllRuns;

			var summaryIdx = summaries.Select((s, i) => new { s, i }).ToDictionary(p => p.s, p => p.i);

			var data =
				from sWithIndex in summaries.Select((s, i) => new { s, i })
				from benchmark in sWithIndex.s.Benchmarks
				from measurement in sWithIndex.s[benchmark].AllMeasurements.Select((m, i) => new { m, i })
				select new
				{
					SummaryNum = sWithIndex.i + 1,
					Summary = sWithIndex.s,
					Benchmark = benchmark,
					Target = benchmark.Target,
					Job = benchmark.Job,
					Parameters = benchmark.Parameters,
					MeasurementType = measurement.m.IterationMode,
					MeasurementNum = measurement.i,
					MeasurementLaunch = measurement.m.LaunchIndex,
					MeasurementIteration = measurement.m.IterationIndex,
					MeasurementNs = measurement.m.Nanoseconds
				};

			var fileName = summary.Benchmarks[0].Target.Type.Name + "-timings.csv";

			using (var writer = File.CreateText(fileName))
			{
				writer.WriteLine("RunNumber;Target;Job;Parameters;MNum;MType;MLaunch;MIteration;MNs");
				foreach (var d in data)
				{
					writer.WriteLine(
						$"{d.SummaryNum};{d.Target.MethodTitle};{d.Job.ToString()};{d.Parameters.FullInfo};"+
						$"{d.MeasurementNum};{d.MeasurementType};{d.MeasurementLaunch};{d.MeasurementIteration};{d.MeasurementNs}");
				}
			}
			yield return fileName;
		}
	}
}