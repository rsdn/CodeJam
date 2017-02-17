using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary> Extension methods for <see cref="MetricUnit"/> and <see cref="MetricUnits"/>. </summary>
	public static class MetricExtensions
	{
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
		
		#region GetMinMetricValue
		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metricInfo">The metric information.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(this double metricMaxValue, [NotNull] CompetitionMetricInfo metricInfo) =>
			GetMinMetricValue(metricMaxValue, metricInfo.SingleValueMode);

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
		#endregion

		#region GetRoundingDigits
		private static int GetRoundingDigits(double value, MetricUnit metricUnit) =>
			metricUnit.RoundingDigits ??
				BenchmarkHelpers.GetRoundingDigits(value);

		private static int GetRoundingDigits(double min, double max, MetricUnit metricUnit) =>
			metricUnit.RoundingDigits ??
				Math.Max(
					BenchmarkHelpers.GetRoundingDigits(min),
					BenchmarkHelpers.GetRoundingDigits(max));
		#endregion

		#region Scaled
		/// <summary>Scales range of metric values using the metric measurement unit.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Scaled range of metric values.</returns>
		public static MetricRange ToScaledValuesRounded(this MetricRange metricValues, [NotNull] MetricUnit metricUnit)
		{
			var min = metricValues.Min.ToScaledValue(metricUnit);
			var max = metricValues.Max.ToScaledValue(metricUnit);
			var roundingDigits = GetRoundingDigits(min, max, metricUnit);

			return MetricRange.Create(
				Math.Round(min, roundingDigits, MidpointRounding.AwayFromZero),
				Math.Round(max, roundingDigits, MidpointRounding.AwayFromZero));
		}

		private static double ToScaledValue(this double metricValue, [NotNull] MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? metricValue : metricValue / metricUnit.ScaleCoefficient;

		/// <summary>Normalizes range of metric values using the metric measurement unit.</summary>
		/// <param name="scaledMetricValues">Scaled range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>Normalized range of metric values.</returns>
		public static MetricRange ToNormalizedMetricValues(
			this MetricRange scaledMetricValues, [NotNull] MetricUnit metricUnit) =>
				metricUnit.IsEmpty
					? scaledMetricValues
					: MetricRange.Create(
						scaledMetricValues.Min.ToNormalizedMetricValue(metricUnit),
						scaledMetricValues.Max.ToNormalizedMetricValue(metricUnit));

		private static double ToNormalizedMetricValue(this double scaledMetricValue, [NotNull] MetricUnit metricUnit) =>
			metricUnit.IsEmpty ? scaledMetricValue : scaledMetricValue * metricUnit.ScaleCoefficient;
		#endregion

		#region ToString
		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this double metricValue, [NotNull] MetricUnits metricUnits) =>
				ToString(metricValue, metricUnits[metricValue]);

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this double metricValue, [NotNull] MetricUnit metricUnit)
		{
			metricValue = metricValue.ToScaledValue(metricUnit);
			var roundingDigits = GetRoundingDigits(metricValue, metricUnit);
			var displayFormat = "F" + roundingDigits;

			var formattedValue = metricValue.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			return double.IsNaN(metricValue) || metricUnit.IsEmpty
				? formattedValue
				: formattedValue + " " + metricUnit.Name;
		}

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnits">The metric units.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this MetricRange metricValues, [NotNull] MetricUnits metricUnits) =>
				ToString(metricValues, metricUnits[metricValues]);

		/// <summary>Returns a <see cref="string" /> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>A <see cref="string" /> that represents the metric value.</returns>
		public static string ToString(
			this MetricRange metricValues, [NotNull] MetricUnit metricUnit)
		{
			var min = metricValues.Min.ToScaledValue(metricUnit);
			var max = metricValues.Max.ToScaledValue(metricUnit);
			var roundingDigits = GetRoundingDigits(min, max, metricUnit);
			var displayFormat = "F" + roundingDigits;

			var formattedValue = metricValues.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			return metricValues.IsEmpty || metricUnit.IsEmpty
				? formattedValue
				: formattedValue + " " + metricUnit.Name;
		}

		/// <summary>Returns a <see cref="string" /> representation of min and max parts of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <param name="minString">String representation of min part of a metric value.</param>
		/// <param name="maxString">String representation of max part of a metric value.</param>
		public static void GetStringMinMax(
			this MetricRange metricValues, [NotNull] MetricUnit metricUnit, out string minString, out string maxString)
		{
			var min = metricValues.Min.ToScaledValue(metricUnit);
			var max = metricValues.Max.ToScaledValue(metricUnit);
			var roundingDigits = GetRoundingDigits(min, max, metricUnit);
			var displayFormat = "F" + roundingDigits;

			minString = min.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			maxString = max.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
		}
		#endregion
	}
}