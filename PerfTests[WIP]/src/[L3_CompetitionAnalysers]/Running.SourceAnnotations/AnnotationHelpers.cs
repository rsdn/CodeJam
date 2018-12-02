using System;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helpers for <see cref="IAnnotationStorage"/> implementations
	/// </summary>
	internal static class AnnotationHelpers
	{
		/// <summary>Converts <see cref="IStoredMetricValue"/> to the competition metric value.</summary>
		/// <param name="storedMetric">The stored metric.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns>The competition metric value.</returns>
		public static CompetitionMetricValue ToMetricValue(
			this IStoredMetricValue storedMetric,
			MetricInfo metric)
		{
			Code.BugIf(
				storedMetric.MetricAttributeType != metric.AttributeType,
				"storedMetric.MetricAttributeType != metric.AttributeType");

			var metricUnit = metric.MetricUnits[storedMetric.UnitOfMeasurement];

			var metricValues = MetricValueHelpers.CreateMetricRange(storedMetric.Min, storedMetric.Max, metricUnit);
			return new CompetitionMetricValue(
				metric,
				metricValues,
				metricUnit);
		}
	}
}