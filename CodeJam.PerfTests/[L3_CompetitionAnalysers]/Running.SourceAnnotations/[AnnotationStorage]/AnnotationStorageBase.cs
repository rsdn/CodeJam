using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Running;
using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
		/// <summary>Creates empty competition descriptor.</summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <param name="metrics">The metrics.</param>
		/// <returns>A new empty competition descriptor.</returns>
		protected static CompetitionTarget CreateEmptyCompetitionTarget(Descriptor descriptor, MetricInfo[] metrics)
		{
			var metricValues = metrics
				.Where(m => !descriptor.Baseline || !m.IsRelative)
				.Select(m => new CompetitionMetricValue(m))
				.ToArray();

			return new CompetitionTarget(descriptor, metricValues);
		}

		#region Parse competition descriptor
		/// <summary>Tries to parse competition descriptor.</summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <param name="metrics">The metrics to parse.</param>
		/// <param name="storedTarget">The stored descriptor.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>A competition descriptor with metrics that were parsed.</returns>
		[NotNull]
		protected static CompetitionTarget ParseCompetitionTarget(
					[NotNull] Descriptor descriptor,
					[NotNull] MetricInfo[] metrics,
					[CanBeNull] StoredTargetInfo storedTarget,
					[NotNull] IMessageLogger messageLogger)
		{
			if (storedTarget == null)
			{
				messageLogger.WriteInfoMessage(
					descriptor,
					"Has no annotations applied, all metrics will be treated as empty.",
					"Check if the method was renamed; add annotations for the method or enable auto-annotation feature.");

				return CreateEmptyCompetitionTarget(descriptor, metrics);
			}

			if (storedTarget.Baseline != null && storedTarget.Baseline != descriptor.Baseline)
			{
				messageLogger.WriteSetupErrorMessage(
					descriptor,
					"Baseline flag on the method and in the annotation do not match.",
					"Check if the method was renamed. Rename it back or update previous run log with new method name.");
			}

			var parseEvents = new List<(MetricInfo metric, MetricParseEvent parseEvent)>();
			var result = ParseCompetitionMetricValues(descriptor, metrics, storedTarget, parseEvents);

			ReportParseEventSummary(descriptor, parseEvents, messageLogger);

			return new CompetitionTarget(descriptor, result.ToArray());
		}

		private static List<CompetitionMetricValue> ParseCompetitionMetricValues(
			Descriptor descriptor,
			MetricInfo[] metrics,
			StoredTargetInfo storedTarget,
			List<(MetricInfo, MetricParseEvent)> parseEvents)
		{
			var result = new List<CompetitionMetricValue>();
			var metricsByType = storedTarget.MetricValues.ToLookup(m => m.MetricAttributeType);
			foreach (var metric in metrics)
			{
				var metricIsApplicable = !descriptor.Baseline || !metric.IsRelative;
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
			Descriptor descriptor,
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
							descriptor,
							$"Annotation for {metricQuantifier} {names} not found, treated as empty.",
							$"Add annotation for missing {metricQuantifier} or enable auto-annotation feature.");
						break;
					case MetricParseEvent.NotApplicableToBaseline:
						messageLogger.WriteInfoMessage(
							descriptor,
							$"The relative {metricQuantifier} {names} cannot be applied to the descriptor as the descriptor is baseline.",
							"Check if the baseline of the competition was accidentally changed. If not, remove the annotations.");
						break;
					case MetricParseEvent.UnitValueMissing:
						messageLogger.WriteSetupErrorMessage(
							descriptor,
							$"Parsing error for {names} {metricQuantifier}. Unit value should be not null as metric' units scale is not empty.",
							"Ensure that annotation does include metric unit.");
						break;
					case MetricParseEvent.UnitValueNotRequired:
						messageLogger.WriteSetupErrorMessage(
							descriptor,
							$"Parsing error for {names} {metricQuantifier}. Unit value should be null as metric' units scale is empty.",
							"Ensure that annotation does not include metric unit.");
						break;
					case MetricParseEvent.MultipleAnnotations:
						messageLogger.WriteWarningMessage(
							descriptor,
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
		/// <summary>Fills competition descriptors with stored metric values.</summary>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Filled descriptors or empty collection if not filled.</returns>
		public CompetitionTarget[] TryGetTargets(SummaryAnalysis analysis)
		{
			Code.NotNull(analysis, nameof(analysis));

			// TODO: cache stored metrics!
			var descriptors = new List<CompetitionTarget>();
			if (analysis.Options.Annotations.IgnoreExistingAnnotations)
			{
				FillTargetsIgnoreAnnotations(descriptors, analysis);
			}
			else
			{
				FillTargetsFromAnnotations(descriptors, analysis);
			}

			return analysis.SafeToContinue ? descriptors.ToArray() : Array<CompetitionTarget>.Empty;
		}

		/// <summary>Fills competition descriptors with empty metric values.</summary>
		/// <param name="descriptors">Competition descriptors to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsIgnoreAnnotations(List<CompetitionTarget> descriptors, SummaryAnalysis analysis)
		{
			var ignoreCharacteristic = CompetitionAnnotationMode.IgnoreExistingAnnotationsCharacteristic.FullId;
			analysis.WriteInfoMessage(
				$"Existing source annotations are ignored due to {ignoreCharacteristic} setting.");

			var metrics = analysis.RunState.Config.GetMetrics().ToArray();

			var targetsToFill = analysis.Summary
				.GetBenchmarkDescriptors()
				.Where(t => CheckCompetitionAttribute(t, analysis));
			foreach (var descriptor in targetsToFill)
			{
				var competitionTarget = CreateEmptyCompetitionTarget(descriptor, metrics);
				descriptors.Add(competitionTarget);
			}
		}

		/// <summary>Fills competition descriptors with stored metric values.</summary>
		/// <param name="descriptors">Competition descriptors to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsFromAnnotations(List<CompetitionTarget> descriptors, SummaryAnalysis analysis)
		{
			var targetsToFill = analysis.Summary.GetBenchmarkDescriptors()
				.Where(t => CheckCompetitionAttribute(t, analysis))
				.ToArray();
			var metrics = analysis.Config.GetMetrics().ToArray();

			var storedMetrics = GetStoredTargets(targetsToFill, analysis);

			if (analysis.SafeToContinue)
			{
				foreach (var descriptor in targetsToFill)
				{
					var targetInfo = storedMetrics.GetValueOrDefault(descriptor);

					var competitionTarget = ParseCompetitionTarget(descriptor, metrics, targetInfo, analysis);
					descriptors.Add(competitionTarget);
				}
			}
		}

		private static bool CheckCompetitionAttribute(Descriptor descriptor, IMessageLogger messageLogger)
		{
			var benchmarkAttribute = descriptor.WorkloadMethod.GetCustomAttribute<CompetitionBenchmarkAttribute>(false);

			if (benchmarkAttribute == null)
			{
				messageLogger.WriteInfoMessage(
					descriptor,
					$"Metric validation skipped as the method is not marked with {nameof(CompetitionBenchmarkAttribute)}.");
				return false;
			}

			if (benchmarkAttribute.DoesNotCompete)
			{
				messageLogger.WriteInfoMessage(
					descriptor,
					"Metric validation skipped as the method is marked with " +
						$"{nameof(CompetitionBenchmarkAttribute)}.{nameof(CompetitionBenchmarkAttribute.DoesNotCompete)} set to true.");
				return false;
			}

			return true;
		}
		/// <summary>Retrieves stored info for competition descriptors.</summary>
		/// <param name="descriptors">Competition descriptors the metrics are retrieved for.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Stored info for competition descriptors.</returns>
		[NotNull]
		protected abstract IReadOnlyDictionary<Descriptor, StoredTargetInfo> GetStoredTargets(
			[NotNull] Descriptor[] descriptors, [NotNull] Analysis analysis);
		#endregion

		/// <summary>Saves stored metrics from competition descriptors.</summary>
		/// <param name="descriptors">Competition descriptors with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved descriptors, if any.</returns>
		public CompetitionTarget[] TrySaveTargets(IReadOnlyCollection<CompetitionTarget> descriptors, AnnotationContext annotationContext, SummaryAnalysis analysis)
		{
			Code.NotNull(descriptors, nameof(descriptors));
			Code.NotNull(analysis, nameof(analysis));
			Code.BugIf(
				descriptors.Any(t => t.Descriptor.Type != analysis.RunState.BenchmarkType),
				"Trying to annotate code that does not belongs to the benchmark.");

			var result = TrySaveAnnotationsCore(descriptors, annotationContext, analysis);
			if (result.Any())
			{
				annotationContext.Save();
			}
			return result;
		}

		/// <summary>Saves stored metrics from competition descriptors.</summary>
		/// <param name="competitionTargets">Competition descriptors with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved descriptors, if any.</returns>
		protected abstract CompetitionTarget[] TrySaveAnnotationsCore([NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets, [NotNull] AnnotationContext annotationContext, [NotNull] SummaryAnalysis analysis);
	}
}