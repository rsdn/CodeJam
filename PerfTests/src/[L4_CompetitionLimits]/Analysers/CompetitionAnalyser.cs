using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	internal class CompetitionAnalyser : IAnalyser
	{
		// TODO: notify if not saved

		#region Properties
		/// <summary>Returns the identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;
		#endregion

		[AssertionMethod]
		private void AssertNoErrors(CompetitionAnalysis analysis) =>
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

			if (analysis.SafeToContinue && analysis.RunState.FirstRun)
			{
				PrepareTargets(analysis);

				PreparePreviousRunLog(analysis);

				Code.BugIf(
					analysis.Targets.Any(t => t.Target.Type != analysis.RunState.BenchmarkType),
					"Trying to analyse code that does not belong to the benchmark.");

				if (analysis.Targets.SelectMany(t => t.MetricValues).Any(m => m.Metric.IsRelative) &&
					!GetBenchmarkTargets(analysis).Any(t => t.Baseline))
				{
					analysis.WriteSetupErrorMessage(
						"No baseline method for benchmark. " +
							$"Apply {nameof(CompetitionBaselineAttribute)} to the one of benchmark methods.");
				}
			}

			if (analysis.SafeToContinue)
			{
				bool checkPassed = CheckTargets(analysis);

				CheckPostconditions(analysis);

				CompleteCheckTargets(analysis, checkPassed);
			}

			if (analysis.RunState.LooksLikeLastRun)
			{
				if (analysis.Annotations.LogAnnotations || analysis.Targets.HasUnsavedChanges)
				{
					XmlAnnotations.LogXmlAnnotationDoc(analysis.Targets, analysis.RunState);
				}
			}

			return analysis.Conclusions.ToArray();
		}

		private void PreparePreviousRunLog(CompetitionAnalysis analysis)
		{
			if (!analysis.SafeToContinue)
				return;

			var logUri = analysis.Annotations.PreviousRunLogUri;
			if (string.IsNullOrEmpty(logUri))
				return;

			var xmlAnnotationDocs = ReadXmlAnnotationDocsFromLog(logUri, analysis);
			if (xmlAnnotationDocs.Length == 0 || !analysis.SafeToContinue)
				return;

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (TryFillCompetitionTargetsFromLog(analysis, xmlAnnotationDocs))
			{
				analysis.WriteInfoMessage($"Competition limits were updated from log file '{logUri}'.");
			}
			else if (analysis.SafeToContinue)
			{
				analysis.WriteInfoMessage($"Competition limits do not require update. Log file: '{logUri}'.");
			}
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private XDocument[] ReadXmlAnnotationDocsFromLog(string logUri, CompetitionAnalysis analysis)
		{
			analysis.RunState.WriteVerbose($"Reading XML annotation documents from log '{logUri}'.");

			var xmlAnnotationDocs = XmlAnnotations.TryParseXmlAnnotationDocsFromLog(logUri, analysis.RunState);

			if (xmlAnnotationDocs == null)
				return Array<XDocument>.Empty;

			if (xmlAnnotationDocs.Length == 0 && analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage($"No XML annotation documents in the log '{logUri}'.");
			}

			return xmlAnnotationDocs;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private bool TryFillCompetitionTargetsFromLog(
			CompetitionAnalysis analysis, XDocument[] xmlAnnotationDocs)
		{
			analysis.RunState.WriteVerbose($"Parsing XML annotations ({xmlAnnotationDocs.Length} doc(s)) from log.");

			var updated = false;
			foreach (var competitionTarget in analysis.Targets)
			{
				var metricsByType = competitionTarget.MetricValues.ToDictionary(m => m.Metric.AttributeType);
				var hasAnnotations = false;

				foreach (var doc in xmlAnnotationDocs)
				{
					var parsedMetrics = XmlAnnotations.TryParseMetrics(
						competitionTarget.Target, doc, true, analysis.RunState);
					foreach (var storedMetricValue in parsedMetrics)
					{
						if (!metricsByType.TryGetValue(storedMetricValue.MetricAttributeType, out var metricValue))
							continue;

						hasAnnotations = true;

						updated |= metricValue.UnionWith(ToMetricValue(storedMetricValue, metricValue.Metric), true);
					}
				}

				if (!hasAnnotations && analysis.SafeToContinue && metricsByType.Any())
				{
					analysis.WriteWarningMessage(
						$"No logged XML annotation for {competitionTarget.Target.MethodDisplayInfo} found. Check if the method was renamed.");
				}
			}

			return updated;
		}

		#region Skip logic
		private bool PerformAdjustment(CompetitionAnalysis analysis) =>
			analysis.Adjustments.AdjustLimits &&
				analysis.RunState.RunNumber > analysis.Adjustments.SkipRunsBeforeAdjustment;

		private bool PerformAdjustment(CompetitionAnalysis analysis, CompetitionMetricValue metricValue)
		{
			if (analysis.Adjustments.AdjustLimits
				|| (metricValue.ValuesRange.IsEmpty && analysis.Adjustments.ForceEmptyLimitsAdjustment))
				return analysis.RunState.RunNumber > analysis.Adjustments.SkipRunsBeforeAdjustment;
			return false;
		}
		#endregion

		#region Prepare targets
		private void PrepareTargets(CompetitionAnalysis analysis)
		{
			// TODO: xmlAnnotationsDoc to lazy?
			// -or- separate loop for ignore existing annotations
			// TODO: cache stored metrics!

			// DONTTOUCH: DO NOT add return into the if clause.
			// The competitionTargets should be filled with empty limits anyway.
			var ignoreExistingAnnotations = analysis.Annotations.IgnoreExistingAnnotations;
			if (ignoreExistingAnnotations)
			{
				var ignoreCharacteristic = CompetitionAnnotationMode.IgnoreExistingAnnotationsCharacteristic.FullId;
				analysis.WriteInfoMessage(
					$"Existing benchmark limits are ignored due to {ignoreCharacteristic} setting.");
			}

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(analysis.RunState.BenchmarkType);
			analysis.Targets.CompetitionMetadata = competitionMetadata;

			var xmlAnnotationsDoc = ignoreExistingAnnotations || competitionMetadata == null
				? null
				: XmlAnnotations.TryParseXmlAnnotationDoc(
					competitionMetadata.MetadataResourceKey,
					analysis.RunState);

			foreach (var target in GetBenchmarkTargets(analysis))
			{
				var competitionTarget = TryParseCompetitionTarget(target, xmlAnnotationsDoc, analysis);
				if (competitionTarget != null)
				{
					analysis.Targets.Add(competitionTarget);
				}
			}

			analysis.Targets.SetInitialized();
		}

		[CanBeNull]
		private CompetitionTarget TryParseCompetitionTarget(
			[NotNull] Target target,
			[CanBeNull] XDocument xmlAnnotationsDoc,
			[NotNull] CompetitionAnalysis analysis)
		{
			var benchmarkAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
			if (benchmarkAttribute == null || benchmarkAttribute.DoesNotCompete)
				return null;

			if (analysis.Annotations.IgnoreExistingAnnotations)
			{
				var metricValues = analysis.RunState.Config.GetMetrics()
					.Where(m => !target.Baseline || !m.IsRelative)
					.Select(m => new CompetitionMetricValue(m))
					.ToArray();
				return new CompetitionTarget(target, metricValues);
			}

			var result = new List<CompetitionMetricValue>();

			var competitionMetadata = analysis.Targets.CompetitionMetadata;
			var storedMetrics =
				competitionMetadata == null
					? GetAttributeMetrics(target, analysis)
					: GetXmlAnnotationDocMetrics(target, xmlAnnotationsDoc, competitionMetadata, analysis);

			bool hasMetrics = storedMetrics.Any();

			var metricsByType = storedMetrics.ToDictionary(m => m.MetricAttributeType);
			foreach (var metricInfo in analysis.RunState.Config.GetMetrics())
			{
				bool relativeApplicable = !target.Baseline || !metricInfo.IsRelative;
				if (metricsByType.TryGetValue(metricInfo.AttributeType, out var storedMetric))
				{
					if (relativeApplicable)
					{
						result.Add(ToMetricValue(storedMetric, metricInfo));
					}
					else if (!metricInfo.IsPrimaryMetric)
					{
						analysis.WriteSetupErrorMessage(
							$"Trying to apply relative metric {metricInfo.Name} to the baseline method {target.MethodDisplayInfo}. Skipped.");
					}
				}
				else if (relativeApplicable)
				{
					if (hasMetrics)
					{
						analysis.WriteInfoMessage(
							$"Annotation for {target.MethodDisplayInfo}, metric {metricInfo} not found, threated as empty.");
					}

					result.Add(new CompetitionMetricValue(metricInfo));
				}
			}

			return new CompetitionTarget(target, result.ToArray());
		}

		private static CompetitionMetricValue ToMetricValue(
			IStoredMetricSource storedMetric,
			CompetitionMetricInfo metricInfo)
		{
			Code.BugIf(
				storedMetric.MetricAttributeType != metricInfo.AttributeType,
				"storedMetric.MetricAttributeType != metricInfo.AttributeType");

			var metricUnit = metricInfo.MetricUnits[storedMetric.UnitOfMeasurement];
			var scaledMetricValues = MetricRange.Create(storedMetric.Min, storedMetric.Max);
			return new CompetitionMetricValue(
				metricInfo,
				scaledMetricValues.ToNormalizedMetricValues(metricUnit),
				metricUnit);
		}

		private static Target[] GetBenchmarkTargets(CompetitionAnalysis analysis) =>
			analysis.Summary.GetSummaryOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct()
				.ToArray();

		private static IStoredMetricSource[] GetAttributeMetrics(Target target, CompetitionAnalysis analysis)
		{
			var metricsByType = analysis.RunState.Config.GetMetrics().ToDictionary(m => m.AttributeType);
			var result = new List<IStoredMetricSource>(metricsByType.Count);

			foreach (var metricAttribute in target.Method.GetMetadataAttributes<IStoredMetricSource>(true).ToArray())
			{
				if (!metricsByType.TryGetValue(metricAttribute.MetricAttributeType, out var metricInfo))
				{
					if (metricAttribute.MetricAttributeType != AttributeAnnotations.PrimaryMetric.AttributeType)
					{
						analysis.WriteInfoMessage(
							$"Annotation for {target.MethodDisplayInfo}, unknown metric {metricAttribute.MetricAttributeType.Name}, skipped.");
					}
					continue;
				}

				// validates unit value.
				AttributeAnnotations.ParseUnitValue(target, metricAttribute.UnitOfMeasurement, metricInfo, analysis.RunState);
				result.Add(metricAttribute);
			}
			return result.ToArray();
		}

		private static IStoredMetricSource[] GetXmlAnnotationDocMetrics(
			Target target, XDocument xmlAnnotationsDoc, CompetitionMetadata competitionMetadata, CompetitionAnalysis analysis)
		{
			if (xmlAnnotationsDoc == null)
				return Array<IStoredMetricSource>.Empty;

			var result = XmlAnnotations.TryParseMetrics(
				target, xmlAnnotationsDoc,
				competitionMetadata.UseFullTypeName,
				analysis.RunState);

			if (result.Length == 0 && analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage(
					$"No XML annotation for {target.MethodDisplayInfo} found. Check if the method was renamed.");
			}

			return result;
		}
		#endregion

		#region CheckTargets
		private bool CheckTargets(CompetitionAnalysis analysis)
		{
			AssertNoErrors(analysis);

			var benchmarksByTarget = analysis.Summary
				.GetSummaryOrderBenchmarks()
				.GroupBy(b => analysis.Targets[b.Target])
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

		private bool OnCheckTarget(
			[NotNull] CompetitionTarget competitionTarget,
			[NotNull] Benchmark[] benchmarksForTarget,
			[NotNull] CompetitionAnalysis analysis)
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

		private bool CheckBenchmark(
			Benchmark benchmark,
			CompetitionMetricValue metricValue,
			CompetitionAnalysis analysis)
		{
			var targetMethodTitle = benchmark.Target.MethodDisplayInfo;
			var summary = analysis.Summary;
			var metric = metricValue.Metric;

			var actualValues = metric.ValuesProvider.TryGetActualValues(benchmark, summary);
			if (actualValues.IsEmpty)
			{
				analysis.AddTestErrorConclusion(
					$"Method {targetMethodTitle}: could not obtain {metric} metric values for {benchmark.DisplayInfo}.",
					summary[benchmark]);

				return true;
			}

			var scaledMetricValues = metricValue.ValuesRange.ToScaledValuesRounded(metricValue.DisplayMetricUnit);
			var actualScaledMetricValues = actualValues.ToScaledValuesRounded(metricValue.DisplayMetricUnit);

			if (scaledMetricValues.Contains(actualScaledMetricValues))
				return true;

			if (metric == CompetitionMetricInfo.GcAllocations)
			{
				analysis.RunState.WriteVerboseHint(
					$"Debug {metric}: {analysis.Summary[benchmark].GcStats}.");
			}

			bool checkPassed;
			if (metricValue.ValuesRange.IsEmpty)
			{
				// Check passed if empty & adjustment is disabled.
				checkPassed = !analysis.Adjustments.AdjustLimits && !analysis.Adjustments.ForceEmptyLimitsAdjustment;
			}
			else
			{
				analysis.AddTestErrorConclusion(
					$"{targetMethodTitle}, {metric} {actualValues.ToString(metric.MetricUnits)} is out of limit {metricValue}.",
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

		private void CompleteCheckTargets(CompetitionAnalysis analysis, bool checkPassed)
		{
			if (!analysis.Annotations.DontSaveUpdatedLimits)
			{
				AnnotateTargets(analysis);
			}

			if (!checkPassed)
			{
				RequestReruns(analysis);
			}
			else if (analysis.Conclusions.Count == 0 && analysis.Targets.Any(t => t.MetricValues.Any()))
			{
				if (analysis.Targets.HasUnsavedChanges)
				{
					analysis.WriteWarningMessage(
						"There are competition metrics unsaved. Check the log for details please.");
				}
				else
				{
					analysis.WriteInfoMessage($"{GetType().Name}: All competition metrics are ok.");
				}
			}
		}

		private void AnnotateTargets(CompetitionAnalysis analysis)
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

			if (analysis.Annotations.DontSaveUpdatedLimits)
			{
				var message =
					$"Source annotation skipped due to {CompetitionAnnotationMode.DontSaveUpdatedLimitsCharacteristic.FullId} setting.";
				if (analysis.RunState.FirstRun)
				{
					analysis.RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.Informational, message);
				}
				else
				{
					analysis.RunState.WriteVerbose(message);
				}
				foreach (var metric in analysis.Targets.SelectMany(t => t.MetricValues))
				{
					if (metric.HasUnsavedChanges)
						metric.MarkAsSaved();
				}
			}
			else
			{
				var annotatedTargets = SourceAnnotationsHelper.TryAnnotateBenchmarkFiles(
					analysis.Targets.ToArray(), analysis.Targets.CompetitionMetadata, analysis.RunState);

				if (annotatedTargets.Any())
				{
					analysis.WriteWarningMessage(
						"The sources were updated with new annotations. Please check them before commiting the changes.");
				}

				foreach (var metric in annotatedTargets.SelectMany(t => t.MetricValues))
				{
					if (metric.HasUnsavedChanges)
						metric.MarkAsSaved();
				}
			}
		}

		/// <summary>Requests reruns for the competition.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected virtual void RequestReruns(CompetitionAnalysis analysis)
		{
			var adjustmentMode = analysis.Adjustments;

			if (PerformAdjustment(analysis) && adjustmentMode.RerunsIfAdjusted > 0)
			{
				analysis.RunState.RequestReruns(adjustmentMode.RerunsIfAdjusted, "Metrics were adjusted.");
			}
			else if (analysis.RunState.RunNumber < analysis.Checks.RerunsIfValidationFailed)
			{
				analysis.RunState.RequestReruns(1, "Metrics check failed.");
			}
		}
		#endregion

		private void CheckPostconditions(CompetitionAnalysis analysis)
		{
			// TODO: improve
			if (!analysis.Annotations.IgnoreExistingAnnotations)
			{
				var emptyMetrics = analysis.GetSummaryOrderTargets()
					.Where(t => t.MetricValues.Any(m => m.ValuesRange.IsEmpty))
					.Select(rp => rp.Target.MethodDisplayInfo)
					.ToArray();
				if (emptyMetrics.Any())
				{
					var benchmarks = emptyMetrics.Length == 1 ? "Benchmark" : "Benchmarks";
					analysis.AddWarningConclusion(
						$"{benchmarks} {emptyMetrics.Join(", ")}: results ignored as benchmark metric limits are empty.",
						"Fill metric limits to include benchmarks in the competition.");
				}
			}
		}
	}
}