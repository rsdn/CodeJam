using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary> Extension methods for <see cref="MetricUnit"/> and <see cref="MetricUnits"/>. </summary>
	public static class MetricExtensions
	{
		// DONTTOUCH: empty (double.NaN) values are handled automatically.
		/// <summary>Returns value to use for metric unit search.</summary>
		/// <param name="metricValues">The metric values.</param>
		/// <returns>Value to use for metric unit search.</returns>
		public static double GetUnitSearchValue(this MetricRange metricValues) =>
			double.IsInfinity(metricValues.Min)
				? metricValues.Max
				: metricValues.Min;

		/// <summary>
		/// Determines whether the value is a special metric value
		/// (one of <see cref="MetricRange.Empty"/>, <see cref="MetricRange.FromNegativeInfinity"/>, <see cref="MetricRange.ToPositiveInfinity"/>).
		/// </summary>
		/// <param name="metricValue">The metric value.</param>
		/// <returns>
		/// <c>true</c> if the value is a special metric value; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsSpecialMetricValue(this double metricValue) =>
			double.IsInfinity(metricValue) || double.IsNaN(metricValue);

		/// <summary>Scales metric value using the metric measurement unit.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Scaled metric value.</returns>
		public static double ToScaledValue(this double metricValue, MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? metricValue : metricValue / metricUnit.ScaleCoefficient;

		/// <summary>Scales metric value using the metric measurement unit.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>Scaled metric value.</returns>
		public static double ToScaledValue(this double metricValue, MetricUnits metricUnits) =>
			ToScaledValue(metricValue, metricUnits[metricValue]);

		/// <summary>Scales range of metric values using the metric measurement unit.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Scaled range of metric values.</returns>
		public static MetricRange ToScaledValues(this MetricRange metricValues, MetricUnit metricUnit) =>
			metricUnit.IsEmpty
			? metricValues
			: MetricRange.Create(
				metricValues.Min.ToScaledValue(metricUnit),
				metricValues.Max.ToScaledValue(metricUnit));

		/// <summary>Scales range of metric values using the metric measurement unit.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>Scaled range of metric values.</returns>
		public static MetricRange ToScaledValues(this MetricRange metricValues, MetricUnits metricUnits) =>
			ToScaledValues(metricValues, metricUnits[metricValues]);

		/// <summary>Normalizes metric value using the metric measurement unit.</summary>
		/// <param name="scaledMetricValue">Scaled metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Normalized metric value.</returns>
		public static double ToNormalizedMetricValue(this double scaledMetricValue, MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? scaledMetricValue : scaledMetricValue * metricUnit.ScaleCoefficient;

		/// <summary>Normalizes range of metric values using the metric measurement unit.</summary>
		/// <param name="scaledMetricValues">Scaled range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Normalized range of metric values.</returns>
		public static MetricRange ToNormalizedMetricValues(this MetricRange scaledMetricValues, MetricUnit metricUnit) =>
			metricUnit.IsEmpty
			? scaledMetricValues
			: MetricRange.Create(
				scaledMetricValues.Min.ToNormalizedMetricValue(metricUnit),
				scaledMetricValues.Max.ToNormalizedMetricValue(metricUnit));

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <param name="withoutUnitName">if set to <c>true</c> the name of the measurement unit will be omitted.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this double metricValue, MetricUnit metricUnit, bool withoutUnitName = false)
		{
			metricValue = metricValue.ToScaledValue(metricUnit);

			var displayFormat = BenchmarkHelpers.GetAutoscaledFormat(metricValue);
			var formattedValue = metricValue.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);

			return double.IsNaN(metricValue) || metricUnit.IsEmpty || withoutUnitName
				? formattedValue
				: formattedValue + " " + metricUnit.Name;
		}

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <param name="withoutUnitName">if set to <c>true</c> the name of the measurement unit will be omitted.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(this double metricValue, [NotNull] MetricUnits metricUnits, bool withoutUnitName = false) =>
			ToString(metricValue, metricUnits[metricValue], withoutUnitName);


		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <param name="withoutUnitName">if set to <c>true</c> the name of the measurement unit will be omitted.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this MetricRange metricValues, MetricUnit metricUnit, bool withoutUnitName = false)
		{
			metricValues = metricValues.ToScaledValues(metricUnit);

			var displayFormat = BenchmarkHelpers.GetAutoscaledFormat(metricValues.GetUnitSearchValue());
			var formattedValue = metricValues.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);

			return metricValues.IsEmpty || metricUnit.IsEmpty || withoutUnitName
				? formattedValue
				: formattedValue + " " + metricUnit.Name;
		}

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <param name="withoutUnitName">if set to <c>true</c> the name of the measurement unit will be omitted.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(this MetricRange metricValues,
			[NotNull] MetricUnits metricUnits, bool withoutUnitName = false) =>
			ToString(metricValues, metricUnits[metricValues], withoutUnitName);
	}
}