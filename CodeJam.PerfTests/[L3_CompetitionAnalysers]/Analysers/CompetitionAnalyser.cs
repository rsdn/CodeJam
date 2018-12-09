using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.SourceAnnotations;
using CodeJam.Reflection;
using CodeJam.Strings;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	internal sealed class CompetitionAnalyser : IAnalyser
	{
		#region Static members
		/// <summary>The instance of <see cref="ValidatorMessagesAnalyser"/>.</summary>
		public static readonly CompetitionAnalyser Instance = new CompetitionAnalyser();

		[AssertionMethod]
		private static void AssertNoErrors(Analysis analysis) =>
			Code.BugIf(
				!analysis.SafeToContinue,
				"Bug: Trying to analyse failed competition run.");

		private static void PrepareTargets(CompetitionAnalysis analysis)
		{
			var competitionTargets = analysis.Targets;
			Code.BugIf(competitionTargets.Initialized, "competitionTargets.Initialized");
			Code.BugIf(competitionTargets.Any(), "competitionTargets.Any()");

			var annotationsStorage =
				analysis.RunState.BenchmarkType
					.TryGetMetadataAttribute<IAnnotationStorageSource>()
					?.AnnotationStorage
				?? new SourceAnnotationStorage();

			var loadedTargets = annotationsStorage.TryGetTargets(analysis);
			if (analysis.SafeToContinue)
			{
				foreach (var loadedTarget in loadedTargets)
				{
					competitionTargets.Add(loadedTarget);
				}
				competitionTargets.SetInitialized(annotationsStorage);
			}
		}

		private static void PreparePreviousRunLog(CompetitionAnalysis analysis)
		{
			if (!analysis.SafeToContinue)
				return;

			var logUri = analysis.Options.Annotations.PreviousRunLogUri;
			if (string.IsNullOrEmpty(logUri))
				return;

			var xmlAnnotationDocs = XmlAnnotationStorage.ReadXmlAnnotationDocsFromLog(logUri, analysis);

			if (xmlAnnotationDocs.Length == 0 && analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage($"No XML annotation documents in the log '{logUri}'.");
			}
			if (xmlAnnotationDocs.Length == 0 || !analysis.SafeToContinue)
				return;

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (XmlAnnotationStorage.TryFillCompetitionTargetsFromLog(
				analysis.Targets,
				xmlAnnotationDocs,
				analysis))
			{
				analysis.WriteInfoMessage($"Competition metric limits were updated from log file '{logUri}'.");
			}
			else if (analysis.SafeToContinue)
			{
				analysis.WriteInfoMessage($"Competition metric limits do not require update. Log file: '{logUri}'.");
			}
		}

		private static void CheckPrepairedTargets(CompetitionAnalysis analysis)
		{
			if (!analysis.SafeToContinue)
				return;

			Code.BugIf(
				analysis.Targets.Any(t => t.Descriptor.Type != analysis.RunState.BenchmarkType),
				"Trying to analyse code that does not belong to the benchmark.");

			if (analysis.Targets.SelectMany(t => t.MetricValues).Any(m => m.Metric.IsRelative) &&
				!analysis.Summary.GetBenchmarkDescriptors().Any(t => t.Baseline))
			{
				analysis.WriteSetupErrorMessage(
					"No baseline member found. " +
						$"Apply {nameof(CompetitionBaselineAttribute)} to the one of benchmark methods.");
			}
		}


		#region Skip logic
		private static bool PerformAdjustment(Analysis analysis) =>
			analysis.Options.Adjustments.AdjustMetrics &&
				analysis.RunState.RunNumber > analysis.Options.Adjustments.SkipRunsBeforeAdjustment;

		private static bool PerformAdjustment(Analysis analysis, CompetitionMetricValue metricValue)
		{
			var adjustmentMode = analysis.Options.Adjustments;

			var adjustEmpty = metricValue.ValuesRange.IsEmpty && adjustmentMode.ForceEmptyMetricsAdjustment;
			if (adjustmentMode.AdjustMetrics || adjustEmpty)
				return analysis.RunState.RunNumber > adjustmentMode.SkipRunsBeforeAdjustment;

			return false;
		}
		#endregion

		#region CheckTargets
		private static bool CheckTargets(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);

			var benchmarksByTarget = analysis.Summary
				.GetSummaryOrderBenchmarksCases()
				.GroupBy(b => analysis.Targets[b.Descriptor])
				.Where(g => g.Key != null);

			var checkPassed = true;
			foreach (var benchmarks in benchmarksByTarget)
			{
				var benchmarksForTarget = benchmarks.ToArray();
				checkPassed &= OnCheckTarget(benchmarks.Key, benchmarksForTarget, analysis);

				AssertNoErrors(analysis);
			}

			return checkPassed;
		}

		private static bool OnCheckTarget(
			[NotNull] CompetitionTarget competitionTarget,
			[NotNull] BenchmarkCase[] benchmarksForTarget,
			[NotNull] SummaryAnalysis analysis)
		{
			var result = true;
			foreach (var metricValue in competitionTarget.MetricValues)
			{
				foreach (var benchmark in benchmarksForTarget)
				{
					result &= CheckBenchmark(benchmark, metricValue, analysis);
				}
			}
			return result;
		}

		private static bool CheckBenchmark(
			BenchmarkCase benchmark,
			CompetitionMetricValue metricValue,
			SummaryAnalysis analysis)
		{
			var summary = analysis.Summary;
			var metric = metricValue.Metric;

			var actualValues = metric.ValuesProvider.TryGetActualValues(benchmark, summary);
			if (actualValues.IsEmpty)
			{
				analysis.AddTestErrorConclusion(
					benchmark.Descriptor,
					$"Could not obtain {metric} metric values for {benchmark.DisplayInfo}.",
					summary[benchmark]);

				return true;
			}

			if (metricValue.ValuesRange.ContainsWithRounding(actualValues, metricValue.DisplayMetricUnit))
				return true;

			bool checkPassed;
			if (metricValue.ValuesRange.IsEmpty)
			{
				// Check passed if empty & adjustment is disabled.
				checkPassed = !analysis.Options.Adjustments.AdjustMetrics && !analysis.Options.Adjustments.ForceEmptyMetricsAdjustment;
			}
			else
			{
				analysis.AddTestErrorConclusion(
					benchmark.Descriptor,
					$"Metric {metric} {actualValues.ToString(metric.MetricUnits)} is out of limit {metricValue}.",
					summary[benchmark]);

				checkPassed = false;
			}

			if (PerformAdjustment(analysis, metricValue))
			{
				var limitValues = metric.ValuesProvider.TryGetLimitValues(benchmark, analysis.Summary);
				metricValue.UnionWith(
					new CompetitionMetricValue(
						metric,
						limitValues,
						metric.MetricUnits[limitValues]),
					false);
			}

			return checkPassed;
		}

		private static void CompleteCheckTargets(CompetitionAnalysis analysis, bool checkPassed)
		{
			// TODO: improve
			if (!analysis.Options.Annotations.IgnoreExistingAnnotations)
			{
				var emptyMetricsNames =
					(from t in analysis.Targets
					 from m in t.MetricValues
					 where m.ValuesRange.IsEmpty
					 group m.Metric.DisplayName by t.Descriptor.WorkloadMethod
					 into g
					 select g.Key + ": " + g.Join(", "))
					.ToArray();

				if (emptyMetricsNames.Any())
				{
					analysis.AddWarningConclusion(
						$"Some benchmark metrics are empty and were ignored. Empty metrics are: {emptyMetricsNames.Join("; ")}.",
						"Fill metric limits to include benchmarks in the competition.");
				}
			}

			if (!analysis.Options.Annotations.DontSaveUpdatedAnnotations)
			{
				AnnotateTargets(analysis);
			}

			if (!checkPassed)
			{
				RequestReruns(analysis);
			}
			else if (analysis.Conclusions.Count == 0 && analysis.Targets.Any(t => t.MetricValues.Any()))
			{
				if (analysis.RunState.LooksLikeLastRun && analysis.Targets.HasUnsavedChanges)
				{
					analysis.WriteWarningMessage(
						"There are competition metrics unsaved. Check the log for details please.");
				}
				else
				{
					analysis.WriteInfoMessage("All competition metrics are ok.");
				}
			}
		}

		private static void AnnotateTargets(CompetitionAnalysis analysis)
		{
			// TODO: messaging?
			if (!analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage(
					"Source annotation skipped as there are critical errors in the run. Check the log please.");
				return;
			}

			if (!analysis.Targets.HasUnsavedChanges)
				return;

			if (analysis.Options.Annotations.DontSaveUpdatedAnnotations)
			{
				var dontSaveCharacteristic = CompetitionAnnotationMode.DontSaveUpdatedAnnotationsCharacteristic.FullId;
				var message = $"Source annotation skipped due to {dontSaveCharacteristic} setting.";
				if (analysis.RunState.IsFirstRun)
				{
					analysis.WriteInfoMessage(message);
				}
				else
				{
					analysis.Logger.WriteVerboseLine(message);
				}
				foreach (var metric in analysis.Targets.SelectMany(t => t.MetricValues))
				{
					if (metric.HasUnsavedChanges)
						metric.MarkAsSaved();
				}
			}
			else
			{
				Code.BugIf(
					analysis.Targets.AnnotationStorage == null,
					"analysis.Targets.AnnotationStorage == null");

				var annotatedTargets = analysis.RunInAnnotationContext(ctx =>
					analysis.Targets.AnnotationStorage.TrySaveTargets(
						analysis.Targets, ctx, analysis));

				if (annotatedTargets.Any())
				{
					analysis.WriteWarningMessage(
						"The sources were updated with new annotations. Please check them before committing the changes.");
				}

				foreach (var metric in annotatedTargets.SelectMany(t => t.MetricValues))
				{
					if (metric.HasUnsavedChanges)
						metric.MarkAsSaved();
				}
			}
		}

		private static void RequestReruns(Analysis analysis)
		{
			var adjustmentMode = analysis.Options.Adjustments;

			if (PerformAdjustment(analysis) && adjustmentMode.RerunsIfAdjusted > 0)
			{
				analysis.RunState.RequestReruns(adjustmentMode.RerunsIfAdjusted, "Metrics were adjusted.");
			}
			else if (analysis.RunState.RunNumber < analysis.Options.Checks.RerunsIfValidationFailed)
			{
				analysis.RunState.RequestReruns(1, "Metrics check failed.");
			}
		}
		#endregion
		#endregion

		/// <summary>Prevents a default instance of the <see cref="CompetitionAnalyser"/> class from being created.</summary>
		private CompetitionAnalyser() { }

		/// <summary>Gets identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;

		/// <summary>Competition metric limit checks and adjustments.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			var analysis = new CompetitionAnalysis(Id, summary);

			if (analysis.SafeToContinue && analysis.RunState.IsFirstRun)
			{
				PrepareTargets(analysis);

				PreparePreviousRunLog(analysis);

				CheckPrepairedTargets(analysis);
			}

			if (analysis.SafeToContinue)
			{
				bool checkPassed = CheckTargets(analysis);

				CompleteCheckTargets(analysis, checkPassed);
			}

			if (analysis.RunState.LooksLikeLastRun)
			{
				if (analysis.Options.Annotations.LogAnnotations ||
					analysis.Targets.HasUnsavedChanges ||
					!analysis.SafeToContinue)
				{
					XmlAnnotationStorage.LogXmlAnnotationDoc(analysis.Targets, analysis);
				}
			}

			return analysis.Conclusions.ToArray();
		}
	}
}