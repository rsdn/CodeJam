using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;

using CodeJam.Strings;
using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	internal class CompetitionPreconditionsAnalyser : IAnalyser
	{
		/// <summary>Returns the identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var analysis = new CompetitionAnalysis(Id, summary);

			if (!analysis.RunState.FirstRun || CheckSetup(summary, analysis))
				CheckExecution(summary, analysis);

			return analysis.Conclusions.ToArray();
		}

		#region Check setup
		private static bool CheckSetup(Summary summary, CompetitionAnalysis analysis)
		{
			// DONTTOUCH: DO NOT add return into the if clauses.
			// All conditions should be checked
			if (summary.HasCriticalValidationErrors)
			{
				analysis.WriteExecutionErrorMessage("Summary has validation errors.");
			}

			if (summary.Benchmarks.IsNullOrEmpty())
			{
				analysis.WriteSetupErrorMessage(
					$"No methods in benchmark. Apply one of {nameof(CompetitionBenchmarkAttribute)}, " +
						$"{nameof(CompetitionBaselineAttribute)} or {nameof(BenchmarkAttribute)} to the benchmark methods.");
			}

			if (summary.Config.GetJobs().Skip(1).Any())
			{
				analysis.WriteSetupErrorMessage(
					"Benchmark configuration includes multiple jobs. " +
						"This is not supported as there's no way to store separate competition limits per each job.");
			}

			if (summary.Benchmarks.Select(b => b.Parameters).Distinct().Skip(1).Any())
			{
				analysis.WriteInfoMessage(
					"Benchmark configuration includes multiple parameters. " +
						"Note that results of runs for each parameters set will be merged.");
			}

			CheckMetrics(analysis);

			return analysis.SafeToContinue;
		}

		private static void CheckMetrics(CompetitionAnalysis analysis)
		{
			var metrics = analysis.RunState.Config.GetMetrics().ToArray();
			if (metrics.Any())
			{
				Code.BugIf(
					metrics.DistinctBy(m => m.AttributeType).Count() != metrics.Length,
					"Duplicate metrics (by attribute type) were not removed during cofig preparation.");
				Code.BugIf(
					metrics.DistinctBy(m => m.Name).Count() != metrics.Length,
					"Duplicate metrics (by name) were not removed during cofig preparation.");
			}
			else
			{
				analysis.WriteSetupErrorMessage(
					$"There's no metrics for the competition. Specify them via {nameof(ICompetitionConfig)}");
			}
		}
		#endregion

		#region Check execution
		private void CheckExecution(Summary summary, CompetitionAnalysis analysis)
		{
			// DONTTOUCH: DO NOT add return into the if clauses.
			// All conditions should be checked

			var benchmarksWithReports = summary.Reports
				.Where(r => r.ExecuteResults.Any())
				.Select(r => r.Benchmark);

			var benchMissing = summary.GetSummaryOrderBenchmarks()
				.Except(benchmarksWithReports)
				.Select(b => b.Target.MethodDisplayInfo)
				.Distinct()
				.ToArray();

			if (benchMissing.Any())
			{
				analysis.WriteExecutionErrorMessage("No reports for benchmarks: " + benchMissing.Join(", "));
			}

			var checksMode = analysis.Checks;
			if (checksMode.CheckLimits)
			{
				var timeUnits = MetricUnits.GetMetricUnits(typeof(TimeUnit));

				if (checksMode.TooFastBenchmarkLimit > TimeSpan.Zero)
				{
					var tooFastReports = GetTargetNames(
						analysis,
						r => r.GetResultRuns().Average(run => run.Nanoseconds) < checksMode.TooFastBenchmarkLimit.TotalNanoseconds());

					if (tooFastReports.Any())
					{
						var benchmarks = tooFastReports.Length == 1 ? "Benchmark" : "Benchmarks";
						var time = checksMode.TooFastBenchmarkLimit
							.TotalNanoseconds()
							.ToString(timeUnits);
						analysis.AddWarningConclusion(
							$"{benchmarks} {tooFastReports.Join(", ")}: run takes less than {time}. " +
								"Results cannot be trusted.",
							$"Hint: timing limit is configured via {CompetitionCheckMode.TooFastBenchmarkLimitCharacteristic.FullId}.");
					}
				}

				if (checksMode.LongRunningBenchmarkLimit > TimeSpan.Zero)
				{

					var tooSlowReports = GetTargetNames(
						analysis,
						r => r.GetResultRuns().Average(run => run.Nanoseconds) > checksMode.LongRunningBenchmarkLimit.TotalNanoseconds());

					if (tooSlowReports.Any())
					{
						var benchmarks = tooSlowReports.Length == 1 ? "Benchmark" : "Benchmarks";
						var time = checksMode.LongRunningBenchmarkLimit
							.TotalNanoseconds()
							.ToString(timeUnits);
						analysis.AddWarningConclusion(
							$"{benchmarks} {string.Join(", ", tooSlowReports)}: run takes more than {time}. " +
								"Consider to rewrite the test as peek timings will be hidden by averages.",
							$"Hint: timing limit is configured via {CompetitionCheckMode.LongRunningBenchmarkLimitCharacteristic.FullId}.");
					}
				}
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		// ReSharper disable once SuggestBaseTypeForParameter
		private string[] GetTargetNames(
			CompetitionAnalysis analysis,
			Func<BenchmarkReport, bool> benchmarkReportFilter) =>
				analysis.Summary.GetSummaryOrderBenchmarks()
					.Select(b => analysis.Summary.TryGetBenchmarkReport(b))
					.Where(r => r != null && benchmarkReportFilter(r))
					.Select(r => r.Benchmark.Target.MethodDisplayInfo)
					.Distinct()
					.ToArray();
		#endregion
	}
}