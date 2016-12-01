using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	using ResourceKey = ValueTuple<Assembly, string, bool>;

	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	internal class CompetitionAnalyser : IAnalyser
	{
		#region Static members.
		// DONTTOUCH: DO NOT replace with Memoize as the value reading depends on CompetitionState
		// and should be called from callee thread only.
		private static readonly ConcurrentDictionary<ResourceKey, XDocument> _xmlAnnotationsCache =
			new ConcurrentDictionary<ResourceKey, XDocument>();
		#endregion

		#region Properties
		/// <summary>Returns the identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;
		#endregion

		[AssertionMethod]
		private void AssertNoErrors(Analysis analysis) =>
			Code.BugIf(
				!analysis.SafeToContinue,
				"Bug: Trying to analyse failed competition run.");

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var analysis = new CompetitionAnalysis(Id, summary);

			if (CheckPreconditions(analysis) && PrepareTargets(analysis))
			{
				CheckTargets(analysis);

				CheckPostconditions(analysis);

				CompleteCheckTargets(analysis);
			}

			if (analysis.RunState.LooksLikeLastRun)
			{
				if (analysis.Limits.LogAnnotations || analysis.Targets.Any(t => t.HasUnsavedChanges))
				{
					XmlAnnotations.LogXmlAnnotationDoc(analysis.Targets, analysis.RunState);
				}
			}

			return analysis.Conclusions.ToArray();
		}

		#region Pre- & postconditions
		private bool CheckPreconditions(CompetitionAnalysis analysis)
		{
			// DONTTOUCH: DO NOT add return into the if clause.
			// All preconditions should be checked.
			var summary = analysis.Summary;
			if (summary.HasCriticalValidationErrors)
			{
				analysis.WriteExecutonErrorMessage("Summary has validation errors.");
			}

			if (!summary.Benchmarks.Any())
			{
				analysis.WriteSetupErrorMessage("No methods to benchmark. Add methods into competition.");
			}

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
				analysis.WriteExecutonErrorMessage("No reports for benchmarks: " + string.Join(", ", benchMissing));
			}

			if (analysis.Limits.LimitProvider == null)
			{
				var providerProperty = CompetitionLimitsMode.LimitProviderCharacteristic.FullId;
				analysis.WriteSetupErrorMessage($"The {providerProperty} should be not null.");
			}

			return analysis.SafeToContinue;
		}

		private void CheckPostconditions(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);

			var culture = HostEnvironmentInfo.MainCultureInfo;
			var limitsMode = analysis.Limits;

			if (limitsMode.TooFastBenchmarkLimit > TimeSpan.Zero)
			{
				var tooFastReports = GetTargetNames(
					analysis,
					r => r.Nanoseconds < limitsMode.TooFastBenchmarkLimit.TotalNanoseconds());

				if (tooFastReports.Any())
				{
					var benchmarks = tooFastReports.Length == 1 ? "benchmark" : "benchmarks";
					var timeMs = limitsMode.TooFastBenchmarkLimit.TotalMilliseconds.ToString(culture);
					analysis.AddWarningConclusion(
						$"The {benchmarks} {string.Join(", ", tooFastReports)} run faster than {timeMs} ms. Results cannot be trusted.");
				}
			}

			if (limitsMode.LongRunningBenchmarkLimit > TimeSpan.Zero)
			{
				var tooSlowReports = GetTargetNames(
					analysis,
					r => r.Nanoseconds > limitsMode.LongRunningBenchmarkLimit.TotalNanoseconds());

				if (tooSlowReports.Any())
				{
					var benchmarks = tooSlowReports.Length == 1 ? "benchmark" : "benchmarks";
					var timeSec = limitsMode.LongRunningBenchmarkLimit.TotalSeconds.ToString(culture);
					analysis.AddWarningConclusion(
						$"The {benchmarks} {string.Join(", ", tooSlowReports)} run longer than {timeSec} sec." +
							" Consider to rewrite the test as the peek timings will be hidden by averages" +
							" or enable long running benchmarks support in the config.");
				}
			}

			var emptyLimits = analysis.SummaryOrderTargets()
				.Where(t => t.HasRelativeLimits && t.IsEmpty)
				.Select(rp => rp.Target.MethodDisplayInfo)
				.ToArray();
			if (emptyLimits.Any())
			{
				var benchmarks = emptyLimits.Length == 1 ? "benchmark" : "benchmarks";
				var noLimit = emptyLimits.Length == 1 ? "it has empty limit" : "their limits are empty";
				analysis.AddWarningConclusion(
					$"The {benchmarks} {string.Join(", ", emptyLimits)} ignored as {noLimit}. " +
						$"Update limits to include {benchmarks} in the competition.");
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private string[] GetTargetNames(
			CompetitionAnalysis analysis,
			Func<Measurement, bool> measurementFilter) =>
			analysis.Summary.GetSummaryOrderBenchmarks()
				.Select(b => analysis.Summary[b])
				.Where(rp => rp.GetResultRuns().Any(measurementFilter))
				.Select(rp => rp.Benchmark.Target.MethodDisplayInfo)
				.Distinct()
				.ToArray();
		#endregion

		#region PrepareTargets
		private bool PrepareTargets(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);
			if (analysis.Targets.Initialized)
				return true;

			PrepareTargetsOverride(analysis);
			analysis.Targets.SetInitialized();

			return analysis.SafeToContinue;
		}

		/// <summary>Fills competition targets collection.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected virtual void PrepareTargetsOverride([NotNull] CompetitionAnalysis analysis)
		{
			// DONTTOUCH: DO NOT add return into the if clause.
			// The competitionTargets should be filled with empty limits anyway.
			if (analysis.Limits.IgnoreExistingAnnotations)
			{
				var ignoreProperty = CompetitionLimitsMode.IgnoreExistingAnnotationsCharacteristic.FullId;
				analysis.WriteInfoMessage(
					$"Existing benchmark limits are ignored due to {ignoreProperty} setting.");
			}

			var targets = analysis.Summary.GetExecutionOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct()
				.ToArray();
			if (targets.Length == 0)
				return;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(targets[0].Type);
			foreach (var target in targets)
			{
				var competitionTarget = TryParseCompetitionTarget(target, competitionMetadata, analysis);
				if (competitionTarget != null)
				{
					analysis.Targets.Add(competitionTarget);
				}
			}

			if (analysis.Targets.Count > 0 && !analysis.Targets.Any(t => t.Baseline))
			{
				analysis.WriteSetupErrorMessage(
					"The benchmark has no baseline method. " +
						$"Apply {nameof(CompetitionBaselineAttribute)} to the one of the benchmark methods.");
			}
		}

		[CanBeNull]
		private CompetitionTarget TryParseCompetitionTarget(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionAnalysis analysis)
		{
			var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>(true);
			if (competitionAttribute == null)
				return null;

			CompetitionLimit limit;
			if (analysis.Limits.IgnoreExistingAnnotations)
				limit = CompetitionLimit.Empty;
			else if (competitionMetadata == null)
				limit = AttributeAnnotations.ParseCompetitionLimit(competitionAttribute);
			else
				limit = TryParseXmlAnnotationLimit(target, competitionMetadata, analysis) ?? CompetitionLimit.Empty;

			return new CompetitionTarget(target, limit, competitionAttribute.DoesNotCompete, competitionMetadata);
		}

		[CanBeNull]
		private CompetitionLimit TryParseXmlAnnotationLimit(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionAnalysis analysis)
		{
			var resourceKey = new ResourceKey(
				target.Type.Assembly,
				competitionMetadata.MetadataResourceName,
				competitionMetadata.UseFullTypeName);

			var xmlAnnotationDoc = _xmlAnnotationsCache.GetOrAdd(
				resourceKey,
				r => XmlAnnotations.TryParseXmlAnnotationDoc(r.Item1, r.Item2, r.Item3, analysis.RunState));

			if (xmlAnnotationDoc == null)
				return null;

			var result = XmlAnnotations.TryParseCompetitionLimit(target, xmlAnnotationDoc, analysis.RunState);

			if (result == null)
				analysis.WriteWarningMessage(
					$"No XML annotation for {target.MethodDisplayInfo} found. Check if the method was renamed.");

			return result;
		}
		#endregion

		#region CheckTargets
		private void CheckTargets(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);

			var benchmarksByTarget = analysis.Summary
				.GetSummaryOrderBenchmarks()
				.GroupBy(b => analysis.Targets[b.Target])
				.Where(g => !g.Key?.DoesNotCompete ?? false);

			var checkPassed = true;
			foreach (var benchmarks in benchmarksByTarget)
			{
				var benchmarksForTarget = benchmarks.ToArray();
				checkPassed &= CheckTargetOverride(benchmarksForTarget, benchmarks.Key, analysis);

				AssertNoErrors(analysis);
			}

			if (!checkPassed)
			{
				analysis.MarkForRerun();
			}
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="analysis">Analyser pass results.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected virtual bool CheckTargetOverride(
			[NotNull] Benchmark[] benchmarksForTarget,
			[NotNull] CompetitionTarget competitionTarget,
			[NotNull] CompetitionAnalysis analysis)
		{
			if (competitionTarget.Baseline || competitionTarget.IsEmpty)
				return true;

			var result = true;
			foreach (var benchmark in benchmarksForTarget)
			{
				result &= CheckTargetBenchmark(benchmark, competitionTarget, analysis);
			}
			return result;
		}

		private bool CheckTargetBenchmark(
			Benchmark benchmark,
			CompetitionLimit competitionLimit,
			CompetitionAnalysis analysis)
		{
			var limitsMode = analysis.Limits;
			var actualValues = limitsMode.LimitProvider.TryGetActualValues(benchmark, analysis.Summary);
			if (actualValues == null)
			{
				analysis.AddTestErrorConclusion(
					$"Could not obtain competition limits for {benchmark.DisplayInfo}.",
					analysis.Summary.TryGetBenchmarkReport(benchmark));

				return true;
			}

			if (competitionLimit.CheckLimitsFor(actualValues))
				return true;

			var targetMethodTitle = benchmark.Target.MethodDisplayInfo;

			var absoluteTime = BenchmarkDotNet.Columns.StatisticColumn.Mean.GetValue(analysis.Summary, benchmark);
			var absoluteTimeBaseline = BenchmarkDotNet.Columns.StatisticColumn.Mean.GetValue(analysis.Summary, analysis.Summary.TryGetBaseline(benchmark));

			analysis.AddTestErrorConclusion(
				$"Method {targetMethodTitle} {actualValues} does not fit into limits {competitionLimit}.",
				analysis.Summary.TryGetBenchmarkReport(benchmark));

			// TODO: better message?
			analysis.RunState.WriteVerboseHint(
				$"Method {targetMethodTitle} mean time: {absoluteTime}. Baseline mean time: {absoluteTimeBaseline}.");

			return false;
		}
		#endregion

		#region CompleteCheckTargets
		private void CompleteCheckTargets([NotNull] CompetitionAnalysis analysis)
		{
			CompleteCheckTargetsOverride(analysis);

			if (analysis.RerunRequested)
			{
				RequestReruns(analysis);
			}
			else if (analysis.Conclusions.Count == 0 && analysis.Targets.Any(c => c.HasRelativeLimits))
			{
				if (analysis.Targets.Any(c => c.HasUnsavedChanges))
				{
					analysis.WriteWarningMessage("There are competition limits unsaved. Check the log for details please.");
				}
				else
				{
					analysis.WriteInfoMessage($"{GetType().Name}: All competition limits are ok.");
				}
			}
		}

		/// <summary>Complete analysis.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected virtual void CompleteCheckTargetsOverride([NotNull] CompetitionAnalysis analysis)
		{
		}

		/// <summary>Requests reruns for the competition.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected virtual void RequestReruns(CompetitionAnalysis analysis)
		{
			if (analysis.RunState.RunNumber < analysis.Limits.RerunsIfValidationFailed)
			{
				analysis.RunState.RequestReruns(1, "Limit checking failed.");
			}
		}
		#endregion
	}
}