using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary> Extension methods for <see cref="MetricUnit"/> and <see cref="MetricUnits"/>. </summary>
	public static class MetricExtensions
	{
		#region ToMinValue
		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="singleValueMode">How single-value annotations are threated.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(this double metricMaxValue, MetricSingleValueMode singleValueMode) =>
			 metricMaxValue.Equals(0)
				? 0
				: (singleValueMode == MetricSingleValueMode.BothMinAndMax && !double.IsPositiveInfinity(metricMaxValue)
					? metricMaxValue
					: MetricRange.FromNegativeInfinity);

		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metricInfo">The metric information.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(this double metricMaxValue, [NotNull] CompetitionMetricInfo metricInfo) =>
			GetMinMetricValue(metricMaxValue, metricInfo.SingleValueMode);

		/// <summary>Returns minimum metric value to be stored as a source annotation.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="singleValueMode">How single-value annotations are threated.</param>
		/// <returns>Minimum metric value to be stored as a source annotation.</returns>
		public static double? GetMinMetricValueToStore(this MetricRange metricValues, MetricSingleValueMode singleValueMode)
		{
			Code.AssertArgument(metricValues.IsNotEmpty, nameof(metricValues), "The metric values range should be not empty.");
			if (metricValues.Min.Equals(0) && metricValues.Max.Equals(0))
				return 0;

			switch (singleValueMode)
			{
				case MetricSingleValueMode.FromInfinityToMax:
					return double.IsNegativeInfinity(metricValues.Min) ? null : (double?)metricValues.Min;
				case MetricSingleValueMode.BothMinAndMax:
					return metricValues.Min.Equals(metricValues.Max) ? null : (double?)metricValues.Min;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(singleValueMode), singleValueMode);
			}
		}

		/// <summary>Returns minimum metric value to be stored as a source annotation.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricInfo">The metric information.</param>
		/// <returns>Minimum metric value to be stored as a source annotation.</returns>
		public static double? GetMinMetricValueToStore(this MetricRange metricValues, [NotNull] CompetitionMetricInfo metricInfo) =>
			GetMinMetricValueToStore(metricValues, metricInfo.SingleValueMode);
		#endregion

		#region Scaled
		// DONTTOUCH: empty (double.NaN) values are handled automatically.
		/// <summary>Returns value to use for metric unit search.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <returns>Value to use for metric unit search.</returns>
		public static double GetUnitSearchValue(this double metricValue) =>
			metricValue.IsSpecialMetricValue() ? 0 : metricValue;

		// DONTTOUCH: empty (double.NaN) values are handled automatically.
		/// <summary>Returns value to use for metric unit search.</summary>
		/// <param name="metricValues">The metric values.</param>
		/// <returns>Value to use for metric unit search.</returns>
		public static double GetUnitSearchValue(this MetricRange metricValues)
		{
			var min = Math.Abs(metricValues.Min);
			var max = Math.Abs(metricValues.Max);

			return min.IsSpecialMetricValue() ? max : Math.Min(min, max);
		}

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
		public static double ToScaledValue(this double metricValue, [NotNull] MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? metricValue : metricValue / metricUnit.ScaleCoefficient;

		/// <summary>Scales metric value using the metric measurement unit.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>Scaled metric value.</returns>
		public static double ToScaledValue(this double metricValue, [NotNull] MetricUnits metricUnits) =>
			ToScaledValue(metricValue, metricUnits[metricValue]);

		/// <summary>Scales range of metric values using the metric measurement unit.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Scaled range of metric values.</returns>
		public static MetricRange ToScaledValues(this MetricRange metricValues, [NotNull] MetricUnit metricUnit) =>
			metricUnit.IsEmpty
			? metricValues
			: MetricRange.Create(
				metricValues.Min.ToScaledValue(metricUnit),
				metricValues.Max.ToScaledValue(metricUnit));

		/// <summary>Scales range of metric values using the metric measurement unit.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>Scaled range of metric values.</returns>
		public static MetricRange ToScaledValues(this MetricRange metricValues, [NotNull] MetricUnits metricUnits) =>
			ToScaledValues(metricValues, metricUnits[metricValues]);

		/// <summary>Normalizes metric value using the metric measurement unit.</summary>
		/// <param name="scaledMetricValue">Scaled metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Normalized metric value.</returns>
		public static double ToNormalizedMetricValue(this double scaledMetricValue, [NotNull]  MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? scaledMetricValue : scaledMetricValue * metricUnit.ScaleCoefficient;

		/// <summary>Normalizes range of metric values using the metric measurement unit.</summary>
		/// <param name="scaledMetricValues">Scaled range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Normalized range of metric values.</returns>
		public static MetricRange ToNormalizedMetricValues(this MetricRange scaledMetricValues, [NotNull] MetricUnit metricUnit) =>
			metricUnit.IsEmpty
			? scaledMetricValues
			: MetricRange.Create(
				scaledMetricValues.Min.ToNormalizedMetricValue(metricUnit),
				scaledMetricValues.Max.ToNormalizedMetricValue(metricUnit));
		#endregion

		#region ToString
		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <param name="withoutUnitName">if set to <c>true</c> the name of the measurement unit will be omitted.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this double metricValue, [NotNull] MetricUnit metricUnit, bool withoutUnitName = false)
		{
			metricValue = metricValue.ToScaledValue(metricUnit);

			var displayFormat = metricUnit.DisplayFormat ?? BenchmarkHelpers.GetAutoscaledFormat(metricValue);
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
			this MetricRange metricValues, [NotNull] MetricUnit metricUnit, bool withoutUnitName = false)
		{
			metricValues = metricValues.ToScaledValues(metricUnit);

			var displayFormat = metricUnit.DisplayFormat ?? BenchmarkHelpers.GetAutoscaledFormat(metricValues.GetUnitSearchValue());
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
		public static string ToString(
			this MetricRange metricValues, [NotNull] MetricUnits metricUnits, bool withoutUnitName = false) =>
			ToString(metricValues, metricUnits[metricValues], withoutUnitName);
		#endregion
	}
}