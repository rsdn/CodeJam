using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;
using CodeJam.PerfTests.Running.SourceAnnotations;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
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
				var checkPassed = CheckTargets(analysis);

				CheckPostconditions(analysis);

				CompleteCheckTargets(analysis, checkPassed);
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
				analysis.WriteExecutionErrorMessage("Summary has validation errors.");
			}

			if (!summary.Benchmarks.Any())
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
				analysis.WriteExecutionErrorMessage("No reports for benchmarks: " + string.Join(", ", benchMissing));
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
					var benchmarks = tooFastReports.Length == 1 ? "Benchmark" : "Benchmarks";
					var timeMs = limitsMode.TooFastBenchmarkLimit.TotalMilliseconds.ToString(culture);
					analysis.AddWarningConclusion(
						$"{benchmarks} {string.Join(", ", tooFastReports)}: run takes less than {timeMs} ms. " +
							"Results cannot be trusted.",
						$"Hint: timing limit is configured via {CompetitionLimitsMode.TooFastBenchmarkLimitCharacteristic.FullId}.");
				}
			}

			if (limitsMode.LongRunningBenchmarkLimit > TimeSpan.Zero)
			{
				var tooSlowReports = GetTargetNames(
					analysis,
					r => r.Nanoseconds > limitsMode.LongRunningBenchmarkLimit.TotalNanoseconds());

				if (tooSlowReports.Any())
				{
					var benchmarks = tooSlowReports.Length == 1 ? "Benchmark" : "Benchmarks";
					var timeSec = limitsMode.LongRunningBenchmarkLimit.TotalSeconds.ToString(culture);
					analysis.AddWarningConclusion(
						$"{benchmarks} {string.Join(", ", tooSlowReports)}: run takes more than {timeSec} sec. " +
							"Consider to rewrite the test as peek timings will be hidden by averages.",
						$"Hint: timing limit is configured via {CompetitionLimitsMode.LongRunningBenchmarkLimitCharacteristic.FullId}.");
				}
			}

			if (!limitsMode.IgnoreExistingAnnotations)
			{
				var emptyLimits = analysis.SummaryOrderTargets()
					.Where(t => t.HasRelativeLimits && t.Limits.IsEmpty)
					.Select(rp => rp.Target.MethodDisplayInfo)
					.ToArray();
				if (emptyLimits.Any())
				{
					var benchmarks = emptyLimits.Length == 1 ? "Benchmark" : "Benchmarks";
					analysis.AddWarningConclusion(
						$"{benchmarks} {string.Join(", ", emptyLimits)}: results ignored as benchmark limits are empty.",
						"Fill limit values to include benchmarks in the competition.");
				}
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		// ReSharper disable once SuggestBaseTypeForParameter
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

			OnPrepareTargets(analysis);
			Code.BugIf(
				analysis.Targets.Any(t => t.Target.Type != analysis.RunState.BenchmarkType),
				"Trying to analyse code that does not belongs to the benchmark.");

			analysis.Targets.SetInitialized();

			return analysis.SafeToContinue;
		}

		/// <summary>Fills competition targets collection.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected virtual void OnPrepareTargets([NotNull] CompetitionAnalysis analysis)
		{
			// DONTTOUCH: DO NOT add return into the if clause.
			// The competitionTargets should be filled with empty limits anyway.
			if (analysis.Limits.IgnoreExistingAnnotations)
			{
				var ignoreCharacteristic = CompetitionLimitsMode.IgnoreExistingAnnotationsCharacteristic.FullId;
				analysis.WriteInfoMessage(
					$"Existing benchmark limits are ignored due to {ignoreCharacteristic} setting.");
			}

			// TODO: to separate analyzer?
			if (!analysis.Annotations.AdjustLimits && analysis.Annotations.PreviousRunLogUri.NotNullNorEmpty())
			{
				var adjustCharacteristic = SourceAnnotationsMode.AdjustLimitsCharacteristic.FullId;
				analysis.WriteInfoMessage(
					$"Previous run log results are ignored as {adjustCharacteristic} setting is disabled.");
			}

			var targets = analysis.Summary.GetSummaryOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct()
				.ToArray();
			if (targets.Length == 0)
				return;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(analysis.RunState.BenchmarkType);
			foreach (var target in targets)
			{
				var competitionTarget = TryParseCompetitionTarget(target, competitionMetadata, analysis);
				if (competitionTarget != null)
				{
					analysis.Targets.Add(competitionTarget);
				}
			}

			if (analysis.Targets.Any(c => c.HasRelativeLimits) &&
				!analysis.Summary.Benchmarks.Any(t => t.Target.Baseline))
			{
				analysis.WriteSetupErrorMessage(
					"No baseline method for benchmark. " +
						$"Apply {nameof(CompetitionBaselineAttribute)} to the one of benchmark methods.");
			}
		}

		[CanBeNull]
		private CompetitionTarget TryParseCompetitionTarget(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionAnalysis analysis)
		{
			var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>(true);
			if (competitionAttribute == null || competitionAttribute.DoesNotCompete)
				return null;

			LimitRange limit;
			if (analysis.Limits.IgnoreExistingAnnotations)
				limit = LimitRange.Empty;
			else if (competitionMetadata == null)
				limit = AttributeAnnotations.ParseCompetitionLimit(competitionAttribute);
			else
				limit = TryParseXmlAnnotationLimit(target, competitionMetadata, analysis);

			return new CompetitionTarget(target, limit, competitionAttribute.DoesNotCompete, competitionMetadata);
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private LimitRange TryParseXmlAnnotationLimit(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionAnalysis analysis)
		{
			var xmlAnnotationDoc = _xmlAnnotationsCache.GetOrAdd(
				competitionMetadata.MetadataResourceKey,
				key => XmlAnnotations.TryParseXmlAnnotationDoc(key, analysis.RunState));

			if (xmlAnnotationDoc == null)
				return LimitRange.Empty;

			var result = XmlAnnotations.TryParseCompetitionLimit(
				target,
				xmlAnnotationDoc,
				competitionMetadata.UseFullTypeName,
				analysis.RunState);

			if (result == null && analysis.SafeToContinue)
				analysis.WriteWarningMessage(
					$"No XML annotation for {target.MethodDisplayInfo} found. Check if the method was renamed.");

			return result.GetValueOrDefault();
		}
		#endregion

		#region CheckTargets
		private bool CheckTargets(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);

			var benchmarksByTarget = analysis.Summary
				.GetSummaryOrderBenchmarks()
				.GroupBy(b => analysis.Targets[b.Target])
				.Where(g => g.Key != null && g.Key.DoesNotCompete == false);

			var checkPassed = true;
			foreach (var benchmarks in benchmarksByTarget)
			{
				var benchmarksForTarget = benchmarks.ToArray();
				checkPassed &= OnCheckTarget(benchmarksForTarget, benchmarks.Key, analysis);

				AssertNoErrors(analysis);
			}

			return checkPassed;
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="analysis">Analyser pass results.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected virtual bool OnCheckTarget(
			[NotNull] Benchmark[] benchmarksForTarget,
			[NotNull] CompetitionTarget competitionTarget,
			[NotNull] CompetitionAnalysis analysis)
		{
			if (competitionTarget.Baseline || competitionTarget.Limits.IsEmpty)
				return true;

			var result = true;
			foreach (var benchmark in benchmarksForTarget)
			{
				result &= CheckTargetBenchmark(benchmark, competitionTarget.Limits, analysis);
			}
			return result;
		}

		private bool CheckTargetBenchmark(
			Benchmark benchmark,
			LimitRange competitionLimit,
			CompetitionAnalysis analysis)
		{
			var limitsMode = analysis.Limits;
			var summary = analysis.Summary;
			var actualValues = limitsMode.LimitProvider.TryGetActualValues(benchmark, summary);
			if (actualValues.IsEmpty)
			{
				analysis.AddTestErrorConclusion(
					$"Could not obtain competition limits for {benchmark.DisplayInfo}.",
					summary.TryGetBenchmarkReport(benchmark));

				return true;
			}

			if (competitionLimit.Contains(actualValues))
				return true;

			var targetMethodTitle = benchmark.Target.MethodDisplayInfo;

			var meanColumn = BenchmarkDotNet.Columns.StatisticColumn.Mean;
			var absoluteTime = meanColumn.GetValue(summary, benchmark);
			var absoluteTimeBaseline = meanColumn.GetValue(summary, summary.TryGetBaseline(benchmark));

			analysis.AddTestErrorConclusion(
				$"Method {targetMethodTitle} {actualValues.ToDisplayString()} does not fit into limits {competitionLimit.ToDisplayString()}.",
				summary.TryGetBenchmarkReport(benchmark));

			// TODO: better message?
			analysis.RunState.WriteVerboseHint(
				$"Method {targetMethodTitle} mean time: {absoluteTime}. Baseline mean time: {absoluteTimeBaseline}.");

			return false;
		}
		#endregion

		#region CompleteCheckTargets
		private void CompleteCheckTargets([NotNull] CompetitionAnalysis analysis, bool checkPassed)
		{
			OnCompleteCheckTargets(analysis);

			if (!checkPassed)
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
		protected virtual void OnCompleteCheckTargets([NotNull] CompetitionAnalysis analysis) { }

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