using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Helpers for the <see cref="MetricInfo"/>
	/// </summary>
	public static class MetricInfoHelpers
	{
		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metricAttributeType">Type of the metric attribute.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(this double metricMaxValue, [NotNull] Type metricAttributeType) =>
			GetMinMetricValue(metricMaxValue, MetricInfo.FromAttribute(metricAttributeType));

		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(this double metricMaxValue, [NotNull] MetricInfo metric) =>
			GetMinMetricValue(metricMaxValue, metric.DefaultMinValue);

		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="defaultMinValue">>Min value to be used by default.</param>
		/// <returns>Minimum metric value.</returns>
		private static double GetMinMetricValue(this double metricMaxValue, DefaultMinMetricValue defaultMinValue)
		{
			if (metricMaxValue.Equals(0))
				return 0;

			switch (defaultMinValue)
			{
				case DefaultMinMetricValue.Zero:
					return 0;
				case DefaultMinMetricValue.NegativeInfinity:
					return double.NegativeInfinity;
				case DefaultMinMetricValue.SameAsMax:
					return double.IsInfinity(metricMaxValue) ? double.NegativeInfinity : metricMaxValue;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(defaultMinValue), defaultMinValue);
			}
		}

		/// <summary>Determines if the minimum metric value should be stored.</summary>
		/// <param name="metricRange">The metric range.</param>
		/// <param name="metricUnit">The metric unit.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns><c>true</c>, if the minimum metric value should be stored.</returns>
		public static bool ShouldStoreMinMetricValue(
			this MetricRange metricRange, MetricUnit metricUnit,
			[NotNull] MetricInfo metric) =>
				ShouldStoreMinMetricValue(metricRange, metricUnit, metric.DefaultMinValue);

		/// <summary>Determines if the minimum metric value should be stored.</summary>
		/// <param name="metricRange">The metric range.</param>
		/// <param name="metricUnit">The metric unit.</param>
		/// <param name="defaultMinValue">>Min value to be used by default.</param>
		/// <returns><c>true</c>, if the minimum metric value should be stored.</returns>
		public static bool ShouldStoreMinMetricValue(
			this MetricRange metricRange, MetricUnit metricUnit,
			DefaultMinMetricValue defaultMinValue)
		{
			switch (defaultMinValue)
			{
				case DefaultMinMetricValue.Zero:
					return !metricRange.Min.Equals(0);
				case DefaultMinMetricValue.NegativeInfinity:
					return !double.IsInfinity(metricRange.Min);
				case DefaultMinMetricValue.SameAsMax:
					return double.IsInfinity(metricRange.Min) || !metricRange.MinMaxAreSame(metricUnit);
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(defaultMinValue), defaultMinValue);
			}
		}
	}
}