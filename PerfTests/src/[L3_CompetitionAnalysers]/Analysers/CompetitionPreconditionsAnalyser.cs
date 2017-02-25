using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	internal sealed class CompetitionPreconditionsAnalyser : IAnalyser
	{
		#region Static members
		/// <summary>The instance of <see cref="ValidatorMessagesAnalyser"/>.</summary>
		public static readonly CompetitionPreconditionsAnalyser Instance = new CompetitionPreconditionsAnalyser();

		#region Check setup
		private static bool CheckSetup(Analysis analysis)
		{
			var summary = analysis.Summary;

			// DONTTOUCH: DO NOT add return into if clauses.
			// All conditions should be checked
			if (summary.HasCriticalValidationErrors)
			{
				analysis.WriteExecutionErrorMessage("Summary has validation errors.");
			}

			if (summary.Benchmarks.IsNullOrEmpty())
			{
				analysis.WriteSetupErrorMessage(
					"Nothing to check as there is no methods in benchmark.",
					$"Apply one of {nameof(CompetitionBenchmarkAttribute)}, {nameof(CompetitionBaselineAttribute)} or {nameof(BenchmarkAttribute)} to the benchmark methods.");
			}

			if (summary.Config.GetJobs().Skip(1).Any())
			{
				analysis.WriteSetupErrorMessage(
					"Benchmark configuration includes multiple jobs. " +
						"This is not supported as there's no way to store metric annotations individually per each job.",
					"Ensure that the config contains only one job.");
			}

			if (summary.Benchmarks.Select(b => b.Parameters).Distinct().Skip(1).Any())
			{
				analysis.WriteInfoMessage(
					"Benchmark configuration includes multiple parameters. " +
						"Note that results for each parameter set will be merged.");
			}

			CheckMetrics(analysis);

			return analysis.SafeToContinue;
		}

		private static void CheckMetrics(Analysis analysis)
		{
			var metrics = analysis.RunState.Config.GetMetrics().ToArray();
			if (metrics.Any())
			{
				Code.BugIf(
					metrics.DistinctBy(m => m.AttributeType).Count() != metrics.Length,
					"Duplicate metrics (by attribute type) were not removed during cofig preparation.");
				Code.BugIf(
					metrics.DistinctBy(m => m.DisplayName).Count() != metrics.Length,
					"Duplicate metrics (by name) were not removed during cofig preparation.");
			}
			else
			{
				analysis.WriteSetupErrorMessage(
					"There's no metrics for the competition.",
					$"Specify metrics to measure via {nameof(ICompetitionConfig)}.");
			}
		}
		#endregion

		#region Check execution
		private static void CheckExecution(Analysis analysis)
		{
			var summary = analysis.Summary;

			// DONTTOUCH: DO NOT add return into if clauses.
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
				var benchmarks = benchMissing.Length == 1 ? "benchmark" : "benchmarks";
				analysis.WriteExecutionErrorMessage(
					$"No reports for {benchmarks}: {benchMissing.Join(", ")}.",
					"Ensure that benchmarks were run successfully and did not throw any exceptions.");
			}

			var checksMode = analysis.Options.Checks;
			if (checksMode.CheckMetrics)
			{
				var timeUnits = MetricUnitScale.FromEnumValues(typeof(TimeUnit));

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
							$"Timing limit is configured via {CompetitionCheckMode.TooFastBenchmarkLimitCharacteristic.FullId}.");
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
							$"Timing limit is configured via {CompetitionCheckMode.LongRunningBenchmarkLimitCharacteristic.FullId}.");
					}
				}
			}
		}

		private static string[] GetTargetNames(
			Analysis analysis,
			Func<BenchmarkReport, bool> benchmarkReportFilter) =>
				analysis.Summary.GetSummaryOrderBenchmarks()
					.Select(b => analysis.Summary[b])
					.Where(r => r != null && benchmarkReportFilter(r))
					.Select(r => r.Benchmark.Target.MethodDisplayInfo)
					.Distinct()
					.ToArray();
		#endregion 
		#endregion

		/// <summary>Prevents a default instance of the <see cref="CompetitionPreconditionsAnalyser"/> class from being created.</summary>
		private CompetitionPreconditionsAnalyser() { }

		/// <summary>Gets identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var analysis = new Analysis(Id, summary);
			var checkExecution = true;

			if (analysis.RunState.IsFirstRun)
				checkExecution = CheckSetup(analysis);

			if (checkExecution)
				CheckExecution(analysis);

			return analysis.Conclusions.ToArray();
		}
	}
}