using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;
using CodeJam.Strings;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Base implementation of <see cref="IAnnotationStorage"/>
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.Running.SourceAnnotations.IAnnotationStorage" />
	internal abstract class AnnotationStorageBase : IAnnotationStorage
	{
		private enum MetricParseEvent
		{
			TreatedAsEmpty,
			NotApplicableToBaseline,
			UnitValueMissing,
			UnitValueNotRequired,
			MultipleAnnotations
		}

		#region Static members
		/// <summary>Creates empty competition target.</summary>
		/// <param name="target">The target.</param>
		/// <param name="metrics">The metrics.</param>
		/// <returns>A new empty competition target.</returns>
		protected static CompetitionTarget CreateEmptyCompetitionTarget(Target target, MetricInfo[] metrics)
		{
			var metricValues = metrics
				.Where(m => !target.Baseline || !m.IsRelative)
				.Select(m => new CompetitionMetricValue(m))
				.ToArray();

			return new CompetitionTarget(target, metricValues);
		}

		#region Parse competition target
		/// <summary>Tries to parse competition target.</summary>
		/// <param name="target">The target.</param>
		/// <param name="metrics">The metrics to parse.</param>
		/// <param name="storedTarget">The stored target.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>A competition target with metrics that were parsed.</returns>
		[NotNull]
		protected static CompetitionTarget ParseCompetitionTarget(
					[NotNull] Target target,
					[NotNull] MetricInfo[] metrics,
					[CanBeNull] StoredTargetInfo storedTarget,
					[NotNull] IMessageLogger messageLogger)
		{
			if (storedTarget == null)
			{
				messageLogger.WriteInfoMessage(
					target,
					"Has no annotations applied, all metrics will be treated as empty.",
					"Check if the method was renamed; add annotations for the method or enable auto-annotation feature.");

				return CreateEmptyCompetitionTarget(target, metrics);
			}

			if (storedTarget.Baseline != null && storedTarget.Baseline != target.Baseline)
			{
				messageLogger.WriteSetupErrorMessage(
					target,
					"Baseline flag on the method and in the annotation do not match.",
					"Check if the method was renamed. Rename it back or update previous run log with new method name.");
			}

			var parseEvents = new List<(MetricInfo metric, MetricParseEvent parseEvent)>();
			var result = ParseCompetitionMetricValues(target, metrics, storedTarget, parseEvents);

			ReportParseEventSummary(target, parseEvents, messageLogger);

			return new CompetitionTarget(target, result.ToArray());
		}

		private static List<CompetitionMetricValue> ParseCompetitionMetricValues(
			Target target,
			MetricInfo[] metrics,
			StoredTargetInfo storedTarget,
			List<(MetricInfo, MetricParseEvent)> parseEvents)
		{
			var result = new List<CompetitionMetricValue>();
			var metricsByType = storedTarget.MetricValues.ToLookup(m => m.MetricAttributeType);
			foreach (var metric in metrics)
			{
				var metricIsApplicable = !target.Baseline || !metric.IsRelative;
				var storedMetrics = metricsByType[metric.AttributeType].ToArray();
				if (storedMetrics.Length > 0)
				{
					if (metricIsApplicable)
					{
						var metricValue = storedMetrics[0].ToMetricValue(metric);
						if (CheckMetricValue(metricValue, parseEvents))
						{
							result.Add(metricValue);
						}
					}
					else if (!metric.IsPrimaryMetric)
					{
						parseEvents.Add((metric, MetricParseEvent.NotApplicableToBaseline));
					}

					if (storedMetrics.Length > 1)
					{
						parseEvents.Add((metric, MetricParseEvent.MultipleAnnotations));
					}
				}
				else if (metricIsApplicable)
				{
					parseEvents.Add((metric, MetricParseEvent.TreatedAsEmpty));
					result.Add(new CompetitionMetricValue(metric));
				}
			}
			return result;
		}

		private static bool CheckMetricValue(
			CompetitionMetricValue metricValue,
			List<(MetricInfo, MetricParseEvent)> parseEvents)
		{
			var metric = metricValue.Metric;
			var hasDisplayMetricUnit = !metricValue.DisplayMetricUnit.IsEmpty;
			if (metric.MetricUnits.IsEmpty)
			{
				if (hasDisplayMetricUnit)
				{
					parseEvents.Add((metric, MetricParseEvent.UnitValueNotRequired));
					return false;
				}
			}
			else if (!hasDisplayMetricUnit && !metricValue.ValuesRange.IsEmpty)
			{
				parseEvents.Add((metric, MetricParseEvent.UnitValueMissing));
				return false;
			}
			return true;
		}

		private static void ReportParseEventSummary(
			Target target,
			List<(MetricInfo metric, MetricParseEvent parseEvent)> parseEvents,
			IMessageLogger messageLogger)
		{
			var eventsGrouped = parseEvents
				.GroupBy(g => g.parseEvent, g => g.metric)
				.OrderBy(g => (int)g.Key)
				.Select(g => (parseEvent: g.Key, metricNames: g.Select(m => m.DisplayName).ToArray()));

			foreach (var g in eventsGrouped)
			{
				var names = g.metricNames.Join(", ");
				var metricQuantifier = g.metricNames.Length == 1 ? "metric" : "metrics";

				switch (g.parseEvent)
				{
					case MetricParseEvent.TreatedAsEmpty:
						messageLogger.WriteInfoMessage(
							target,
							$"Annotation for {metricQuantifier} {names} not found, treated as empty.",
							$"Add annotation for missing {metricQuantifier} or enable auto-annotation feature.");
						break;
					case MetricParseEvent.NotApplicableToBaseline:
						messageLogger.WriteInfoMessage(
							target,
							$"The relative {metricQuantifier} {names} cannot be applied to the target as the target is baseline.",
							"Check if the baseline of the competition was accidentally changed. If not, remove the annotations.");
						break;
					case MetricParseEvent.UnitValueMissing:
						messageLogger.WriteSetupErrorMessage(
							target,
							$"Parsing error for {names} {metricQuantifier}. Unit value should be not null as metric' units scale is not empty.",
							"Ensure that annotation does include metric unit.");
						break;
					case MetricParseEvent.UnitValueNotRequired:
						messageLogger.WriteSetupErrorMessage(
							target,
							$"Parsing error for {names} {metricQuantifier}. Unit value should be null as metric' units scale is empty.",
							"Ensure that annotation does not include metric unit.");
						break;
					case MetricParseEvent.MultipleAnnotations:
						messageLogger.WriteWarningMessage(
							target,
							$"Multiple annotations found for {names} {metricQuantifier}. Only first annotation was applied, others were ignored.",
							"Remove annotation duplicates.");
						break;
					default:
						throw CodeExceptions.UnexpectedValue(g.parseEvent);
				}
			}
		}
		#endregion
		#endregion

		// TODO: notify if save method failed.

		#region Prepare metrics
		/// <summary>Fills competition targets with stored metric values.</summary>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Filled targets or empty collection if not filled.</returns>
		public CompetitionTarget[] TryGetTargets(SummaryAnalysis analysis)
		{
			Code.NotNull(analysis, nameof(analysis));

			// TODO: cache stored metrics!
			var targets = new List<CompetitionTarget>();
			if (analysis.Options.Annotations.IgnoreExistingAnnotations)
			{
				FillTargetsIgnoreAnnotations(targets, analysis);
			}
			else
			{
				FillTargetsFromAnnotations(targets, analysis);
			}

			return analysis.SafeToContinue ? targets.ToArray() : Array<CompetitionTarget>.Empty;
		}

		/// <summary>Fills competition targets with empty metric values.</summary>
		/// <param name="targets">Competition targets to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsIgnoreAnnotations(List<CompetitionTarget> targets, SummaryAnalysis analysis)
		{
			var ignoreCharacteristic = CompetitionAnnotationMode.IgnoreExistingAnnotationsCharacteristic.FullId;
			analysis.WriteInfoMessage(
				$"Existing source annotations are ignored due to {ignoreCharacteristic} setting.");

			var metrics = analysis.RunState.Config.GetMetrics().ToArray();

			var targetsToFill = analysis.Summary
				.GetBenchmarkTargets()
				.Where(t => CheckCompetitionAttribute(t, analysis));
			foreach (var target in targetsToFill)
			{
				var competitionTarget = CreateEmptyCompetitionTarget(target, metrics);
				targets.Add(competitionTarget);
			}
		}

		/// <summary>Fills competition targets with stored metric values.</summary>
		/// <param name="targets">Competition targets to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsFromAnnotations(List<CompetitionTarget> targets, SummaryAnalysis analysis)
		{
			var targetsToFill = analysis.Summary.GetBenchmarkTargets()
				.Where(t => CheckCompetitionAttribute(t, analysis))
				.ToArray();
			var metrics = analysis.Config.GetMetrics().ToArray();

			var storedMetrics = GetStoredTargets(targetsToFill, analysis);

			if (analysis.SafeToContinue)
			{
				foreach (var target in targetsToFill)
				{
					var targetInfo = storedMetrics.GetValueOrDefault(target);

					var competitionTarget = ParseCompetitionTarget(target, metrics, targetInfo, analysis);
					targets.Add(competitionTarget);
				}
			}
		}

		private static bool CheckCompetitionAttribute(Target target, IMessageLogger messageLogger)
		{
			var benchmarkAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>(false);

			if (benchmarkAttribute == null)
			{
				messageLogger.WriteInfoMessage(
					target,
					$"Metric validation skipped as the method is not marked with {nameof(CompetitionBenchmarkAttribute)}.");
				return false;
			}

			if (benchmarkAttribute.DoesNotCompete)
			{
				messageLogger.WriteInfoMessage(
					target,
					"Metric validation skipped as the method is marked with " +
						$"{nameof(CompetitionBenchmarkAttribute)}.{nameof(CompetitionBenchmarkAttribute.DoesNotCompete)} set to true.");
				return false;
			}

			return true;
		}
		/// <summary>Retrieves stored info for competition targets.</summary>
		/// <param name="targets">Competition targets the metrics are retrieved for.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Stored info for competition targets.</returns>
		[NotNull]
		protected abstract IReadOnlyDictionary<Target, StoredTargetInfo> GetStoredTargets(
			[NotNull] Target[] targets, [NotNull] Analysis analysis);
		#endregion

		/// <summary>Saves stored metrics from competition targets.</summary>
		/// <param name="targets">Competition targets with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved targets, if any.</returns>
		public CompetitionTarget[] TrySaveTargets(IReadOnlyCollection<CompetitionTarget> targets, AnnotationContext annotationContext, SummaryAnalysis analysis)
		{
			Code.NotNull(targets, nameof(targets));
			Code.NotNull(analysis, nameof(analysis));
			Code.BugIf(
				targets.Any(t => t.Target.Type != analysis.RunState.BenchmarkType),
				"Trying to annotate code that does not belongs to the benchmark.");

			var result = TrySaveAnnotationsCore(targets, annotationContext, analysis);
			if (result.Any())
			{
				annotationContext.Save();
			}
			return result;
		}

		/// <summary>Saves stored metrics from competition targets.</summary>
		/// <param name="competitionTargets">Competition targets with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved targets, if any.</returns>
		protected abstract CompetitionTarget[] TrySaveAnnotationsCore([NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets, [NotNull] AnnotationContext annotationContext, [NotNull] SummaryAnalysis analysis);
	}
}