using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Competition preconditions analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	internal sealed class CompetitionPreconditionsAnalyser : IAnalyser
	{
		#region Static members
		/// <summary>The instance of <see cref="ValidatorMessagesAnalyser"/>.</summary>
		public static readonly CompetitionPreconditionsAnalyser Instance = new CompetitionPreconditionsAnalyser();

		#region Helpers
		private static readonly IReadOnlyList<Type> _knownUniqueMemberLevelAttributes = new[]
		{
			typeof(BenchmarkAttribute),
			typeof(GlobalSetupAttribute),
			typeof(GlobalCleanupAttribute),
			typeof(IterationSetupAttribute),
			typeof(IterationCleanupAttribute)
		};
		private static readonly IReadOnlyList<Type> _knownUniqueTypeLevelAttributes = new[]
		{
			typeof(GlobalSetupAttribute),
			typeof(GlobalCleanupAttribute),
			typeof(IterationSetupAttribute),
			typeof(IterationCleanupAttribute)
		};


		private static IEnumerable<MethodInfo> GetInvokedMethods(Target target)
		{
			yield return target.Method;

			if (target.GlobalSetupMethod != null)
				yield return target.GlobalSetupMethod;
			if (target.IterationSetupMethod != null)
				yield return target.IterationSetupMethod;
			if (target.IterationCleanupMethod != null)
				yield return target.IterationCleanupMethod;
			if (target.GlobalCleanupMethod != null)
				yield return target.GlobalCleanupMethod;
		}

		private static string[] GetTargetNames(
			SummaryAnalysis analysis,
			Func<BenchmarkReport, bool> benchmarkReportFilter) =>
				analysis.Summary.GetSummaryOrderBenchmarks()
					.Select(b => analysis.Summary[b])
					.Where(r => r != null && r.ExecuteResults.Any() && benchmarkReportFilter(r))
					.Select(r => r.Benchmark.Target.MethodDisplayInfo)
					.Distinct()
					.ToArray();

		private static string[] GetDuplicates<T, TKey>(
			IEnumerable<T> source,
			Func<T, TKey> keySelector,
			Func<T, string> fullNameSelector) =>
				source
					.GroupBy(keySelector)
					.Where(g => g.Skip(1).Any())
					.Select(g => $"{g.Key} {g.Select(fullNameSelector).Join("; ")}")
					.ToArray();
		#endregion

		#region Check setup
		private static bool CheckSetup(SummaryAnalysis analysis)
		{
			var summary = analysis.Summary;

			// DONTTOUCH: DO NOT add return into if clauses.
			// All conditions should be checked
			if (summary.HasCriticalValidationErrors)
			{
				analysis.WriteExecutionErrorMessage("Summary has validation errors.");
			}

			if (!summary.HasCriticalValidationErrors && summary.Benchmarks.IsNullOrEmpty())
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

			CheckMembers(analysis);

			CheckMetrics(analysis);

			return analysis.SafeToContinue;
		}

		private static void CheckMembers(SummaryAnalysis analysis)
		{
			var summary = analysis.Summary;

			// No duplicate names
			var targets = summary.GetBenchmarkTargets();
			var duplicateTargets = GetDuplicates(
				targets,
				t => t.Method.Name,
				t => $"\r\n\t\t  {t.Method.DeclaringType}.{t.Method.Name}()");

			if (duplicateTargets.NotNullNorEmpty())
			{
				analysis.WriteSetupErrorMessage(
					$"There are multiple methods with same name: {duplicateTargets.Join(",\r\n\t\t")}.",
					"Rename methods to avoid duplicates.");
			}

			// No conflict on attributes
			var targetMethodsWithAttributes = targets
				.SelectMany(GetInvokedMethods)
				.Distinct()
				.SelectMany(
					m =>
						m.GetCustomAttributes(true)
							.Select(a =>
								(method: m,
								 attributeType: a.GetType(),
								 baseAttribute: _knownUniqueMemberLevelAttributes.FirstOrDefault(ka => ka.IsInstanceOfType(a)),
								 target: (a as TargetedAttribute)?.Target))
							.Where(t => t.baseAttribute != null))
				.ToArray();

			var conflictingAttributes = GetDuplicates(
				targetMethodsWithAttributes,
				t => $"{t.method.DeclaringType}.{t.method.Name}({t.target})",
				t => $"\r\n\t\t  {t.attributeType.FullName}");

			if (conflictingAttributes.NotNullNorEmpty())
			{
				analysis.WriteSetupErrorMessage(
					$"There are conflicting attributes: {conflictingAttributes.Join(",\r\n\t\t")}.",
					"There can be only one.");
			}

			// No multiple methods for an attribute
			var conflictingMethods = GetDuplicates(
				targetMethodsWithAttributes.Where(t => _knownUniqueTypeLevelAttributes.Contains(t.baseAttribute)),
				t => $"{t.baseAttribute.FullName}({t.target})",
				t => $"\r\n\t\t  {t.method.DeclaringType}.{t.method.Name}() ({t.attributeType.FullName})");

			if (conflictingMethods.NotNullNorEmpty())
			{
				analysis.WriteSetupErrorMessage(
					$"There are conflicting methods: {conflictingMethods.Join(",\r\n\t\t")}.",
					"Leave only one method for each attribute.");
			}
		}

		private static void CheckMetrics(Analysis analysis)
		{
			var metrics = analysis.RunState.Config.GetMetrics().ToArray();
			if (metrics.Any())
			{
				Code.BugIf(
					metrics.DistinctBy(m => m.AttributeType).Count() != metrics.Length,
					"Duplicate metrics (by attribute type) were not removed during config preparation.");

				var duplicateMetrics = GetDuplicates(
						metrics,
						m => m.DisplayName,
						m => "\r\n\t\t  " + m.AttributeType.FullName);

				if (duplicateMetrics.NotNullNorEmpty())
				{
					analysis.WriteSetupErrorMessage(
						$"There are multiple metrics with same display name: {duplicateMetrics.Join(",\r\n\t\t")}.",
						$"Remove metric duplicates from {nameof(ICompetitionConfig)}.");
				}
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
		private static void CheckExecution(SummaryAnalysis analysis)
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
				.Distinct().
				ToArray();

			if (benchMissing.Any())
			{
				var benchmarks = benchMissing.Length == 1 ? "benchmark" : "benchmarks";
				analysis.WriteExecutionErrorMessage(
					$"No result reports for {benchmarks}: {benchMissing.Join(", ")}.",
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
							$"{benchmarks} {tooFastReports.Join(", ")}: measured run time is less than {time}. " +
								"Timings are imprecise as they are too close to the timer resolution.",
							$"Timing limit for this warning is configured via {CompetitionCheckMode.TooFastBenchmarkLimitCharacteristic.FullId}.");
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
							$"{benchmarks} {string.Join(", ", tooSlowReports)}: measured run time is greater than {time}. " +
								"There's a risk the peak timings were hidden by averages. " +
								"Consider to reduce the number of iterations performed per each measurement.",
							$"Timing limit for this warning is configured via {CompetitionCheckMode.LongRunningBenchmarkLimitCharacteristic.FullId}.");
					}
				}
			}
		}
		#endregion
		#endregion

		/// <summary>Prevents a default instance of the <see cref="CompetitionPreconditionsAnalyser"/> class from being created.</summary>
		private CompetitionPreconditionsAnalyser() { }

		/// <summary>Gets identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;

		/// <summary>Checks preconditions for competition analysis.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var analysis = new SummaryAnalysis(Id, summary);
			var checkExecution = true;

			if (analysis.RunState.IsFirstRun)
				checkExecution = CheckSetup(analysis);

			if (checkExecution)
				CheckExecution(analysis);

			return analysis.Conclusions.ToArray();
		}
	}
}