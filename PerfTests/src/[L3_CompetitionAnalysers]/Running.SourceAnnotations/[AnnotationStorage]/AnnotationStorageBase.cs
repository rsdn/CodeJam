using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Base implementation of <see cref="IAnnotationStorage"/>
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.Running.SourceAnnotations.IAnnotationStorage" />
	internal abstract class AnnotationStorageBase : IAnnotationStorage
	{
		#region Static members
		#region Parse comprtition target
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
			var result = new List<CompetitionMetricValue>();

			if (storedTarget == null)
			{
				messageLogger.WriteInfoMessage(
					target,
					"Has no annotations applied, all metrics will be threated as empty.",
					"Check if the method was renamed; add annnotations for the method or enable auto-annotation feature.");

				return new CompetitionTarget(target, result.ToArray());
			}

			if (storedTarget.Baseline != null && storedTarget.Baseline != target.Baseline)
			{
				messageLogger.WriteSetupErrorMessage(
					target,
					"Baseline flag on the method and in the annotation do not match.",
					"Check if the method was renamed. Rename it back or update previous run log with new method name.");
			}

			var hasAnyMetric = storedTarget.MetricValues.Any();

			// TODO: group messages into one?
			var metricsByType = storedTarget.MetricValues.ToDictionary(m => m.MetricAttributeType);
			foreach (var metric in metrics)
			{
				var metricIsApplicable = !target.Baseline || !metric.IsRelative;
				if (metricsByType.TryGetValue(metric.AttributeType, out var storedMetric))
				{
					if (metricIsApplicable)
					{
						var metricValue = storedMetric.ToMetricValue(metric);
						if (CheckMetricValue(target, metricValue, messageLogger))
						{
							result.Add(storedMetric.ToMetricValue(metric));
						}
					}
					else if (!metric.IsPrimaryMetric)
					{
						messageLogger.WriteSetupErrorMessage(
							target,
							$"Annotation for relative metric {metric} cannot be applied as the target is baseline.",
							"Check if baseline method for the competition was changed.");
					}
				}
				else if (metricIsApplicable)
				{
					if (hasAnyMetric)
					{
						messageLogger.WriteInfoMessage(
							target,
							$"Annotation for metric {metric} not found, threated as empty.",
							"Add annnotation for the metric or enable auto-annotation feature.");
					}

					result.Add(new CompetitionMetricValue(metric));
				}
			}

			return new CompetitionTarget(target, result.ToArray());
		}

		private static bool CheckMetricValue(
			Target target,
			CompetitionMetricValue metricValue,
			IMessageLogger messageLogger)
		{
			var metric = metricValue.Metric;
			if (metric.MetricUnits.IsEmpty)
			{
				if (!metricValue.DisplayMetricUnit.IsEmpty)
				{
					messageLogger.WriteSetupErrorMessage(
						target,
						$"{metric} metric value was parsed incorrectly. Unit value should be null as metric has empty units scale.",
						"Ensure that annotation does not include metric unit.");
					return false;
				}
			}
			else if (metricValue.DisplayMetricUnit.IsEmpty)
			{
				messageLogger.WriteSetupErrorMessage(
					target,
					$"{metric} metric value was parsed incorrectly. Unit value should be not null as metric has empty units scale.",
					"Ensure that annotation does include metric unit.");
				return false;
			}
			return true;
		}
		#endregion 
		#endregion

		// TODO: notify if save method failed.

		#region Prepare metrics
		/// <summary>Fills competition targets with stored metric values.</summary>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Filled targets or empty collection if not filled.</returns>
		public CompetitionTarget[] TryGetTargets(Analysis analysis)
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

			return analysis.SafeToContinue? targets.ToArray(): Array<CompetitionTarget>.Empty;
		}

		/// <summary>Fills competition targets with empty metric values.</summary>
		/// <param name="targets">Competition targets to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsIgnoreAnnotations(List<CompetitionTarget> targets, Analysis analysis)
		{
			var ignoreCharacteristic = CompetitionAnnotationMode.IgnoreExistingAnnotationsCharacteristic.FullId;
			analysis.WriteInfoMessage(
				$"Existing metric annotations are ignored due to {ignoreCharacteristic} setting.");

			var metrics = analysis.RunState.Config.GetMetrics().ToArray();

			var targetsToFill = analysis.Summary
				.GetBenchmarkTargets()
				.Where(t => CheckCompetitionAttribute(t, analysis));
			foreach (var target in targetsToFill)
			{
				var metricValues = metrics
					.Where(m => !target.Baseline || !m.IsRelative)
					.Select(m => new CompetitionMetricValue(m))
					.ToArray();

				var competitionTarget = new CompetitionTarget(target, metricValues);
				targets.Add(competitionTarget);
			}
		}

		/// <summary>Fills competition targets with stored metric values.</summary>
		/// <param name="targets">Competition targets to fill .</param>
		/// <param name="analysis">State of the analysis.</param>
		protected virtual void FillTargetsFromAnnotations(List<CompetitionTarget> targets, Analysis analysis)
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
		public CompetitionTarget[] TrySaveTargets(
			IReadOnlyCollection<CompetitionTarget> targets,
			AnnotationContext annotationContext,
			Analysis analysis)
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
		protected abstract CompetitionTarget[] TrySaveAnnotationsCore(
			[NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets,
			[NotNull] AnnotationContext annotationContext,
			[NotNull] Analysis analysis);
	}
}