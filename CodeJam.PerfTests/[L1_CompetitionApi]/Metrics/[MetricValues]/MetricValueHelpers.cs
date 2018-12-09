using System;

using BenchmarkDotNet.Environments;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary> Extension methods for <see cref="MetricUnit"/> and <see cref="MetricUnitScale"/>. </summary>
	public static class MetricValueHelpers
	{
		#region MetricRange factory methods
		/// <summary>Creates a new metric range.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="MetricRange.EmptyMetricValue"/>  marks the range as unset but updateable during the annotation.
		/// Use <c>null</c> or <seealso cref="MetricRange"/> to set min value to negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// Use <see cref="MetricRange.EmptyMetricValue"/>
		/// to mark the range as unset but updateable during the annotation.
		/// Use <c>null</c> or <seealso cref="MetricRange.ToPositiveInfinity"/> to set max value to positive infinity (ignored, essentially).
		/// </param>
		/// <returns>Metrics range.</returns>
		public static MetricRange CreateMetricRange(double? min, double? max) => CreateMetricRange(min, max, MetricUnit.Empty);

		/// <summary>Creates a new metric range.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="MetricRange.EmptyMetricValue"/> marks the range as unset but updateable during the annotation.
		/// Use <c>null</c> or <seealso cref="MetricRange.EmptyMetricValue"/> to set min value to negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// Use <see cref="MetricRange"/>
		/// to mark the range as unset but updateable during the annotation.
		/// Use <c>null</c> or <seealso cref="MetricRange"/> to set max value to positive infinity (ignored, essentially).
		/// </param>
		/// <param name="metricUnit">The metric unit that was used to store metric range.</param>
		/// <returns>Metrics range.</returns>
		public static MetricRange CreateMetricRange(double? min, double? max, [NotNull] MetricUnit metricUnit)
		{
			var minValue = min ?? MetricRange.FromNegativeInfinity;
			var maxValue = max ?? MetricRange.ToPositiveInfinity;

			if (!metricUnit.IsEmpty)
			{
				minValue *= metricUnit.ScaleCoefficient;
				maxValue *= metricUnit.ScaleCoefficient;
			}

			return new MetricRange(minValue, maxValue);
		}
		#endregion

		#region Formatting & rounding
		private const double ScaleCoefficient = 1 / 1.89; // Empirically-found value.

		private static int GetRoundingDigits(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				return 0;

			// Same logic for positive & negative values.
			value = Math.Abs(value);

			// Corner cases
			if (value >= 100)
				return 1;
			if (value <= 0 || value >= 1)
				return 2;

			// Make value smaller to get additional fractional digits
			// for values with normalized mantissa less than 1.9;
			// as example,
			//  0.2123 will be printed as 0.21
			//  0.1812 will be printed as 0.181
			value *= ScaleCoefficient;

			// Get exponent part of the value. As the value expected to be <<1 result should be negated.
			var valuePow = -Math.Log10(value);

			// Add two extra digits.
			var roundingDigits = (int)Math.Floor(valuePow) + 2;

			// If there's no decimal places - use zero.
			return Math.Max(0, roundingDigits);
		}

		private static int GetRoundingDigits(double scaledMetricValue, MetricUnit metricUnit) =>
			metricUnit.RoundingDigits ??
				GetRoundingDigits(scaledMetricValue);

		private static int GetRoundingDigits(MetricRange scaledMetricValues, MetricUnit metricUnit) =>
			metricUnit.RoundingDigits ??
				Math.Max(
					GetRoundingDigits(scaledMetricValues.Min),
					GetRoundingDigits(scaledMetricValues.Max));

		/// <summary>Gets the autoscaled format for the value.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Autoscaled format for the value</returns>
		public static string GetAutoscaledFormat(double value) =>
			"F" + GetRoundingDigits(value);

		private static string GetFormatForScaled(double scaledMetricValue, MetricUnit metricUnit) =>
			"F" + GetRoundingDigits(scaledMetricValue, metricUnit);

		private static string GetFormatForScaled(MetricRange scaledMetricValues, MetricUnit metricUnit) =>
			"F" + GetRoundingDigits(scaledMetricValues, metricUnit);
		#endregion

		#region Scaled
		/// <summary>
		/// Determines whether the range contains another one.
		/// The check is performed using same rounding that will be used to store the <paramref name="metricValues"/>.
		/// </summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="other">The metric range to check.</param>
		/// <param name="metricUnit">
		/// The metric measurement unit that will be used to store the <paramref name="metricValues"/>.
		/// </param>
		/// <returns><c>true</c>, if the range contains another one.</returns>
		public static bool ContainsWithRounding(
			this MetricRange metricValues, MetricRange other, [NotNull] MetricUnit metricUnit)
		{
			var scaledMetricValues = metricValues.ToScaledValues(metricUnit);
			var scaledOtherMetricValues = other.ToScaledValues(metricUnit);

			var roundDigits = GetRoundingDigits(scaledMetricValues, metricUnit);

			scaledMetricValues = scaledMetricValues.Round(roundDigits);
			scaledOtherMetricValues = scaledOtherMetricValues.Round(roundDigits);

			return scaledMetricValues.Contains(scaledOtherMetricValues);
		}

		/// <summary>
		/// Determines whether the range can be represented as a single point range
		/// (scaled and rounded min and max are the same).
		/// </summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">
		/// The metric measurement unit that will be used to store the <paramref name="metricValues"/>.
		/// </param>
		/// <returns><c>true</c>, if the range can be represented as a single point range.</returns>
		public static bool MinMaxAreSame(this MetricRange metricValues, [NotNull] MetricUnit metricUnit)
		{
			if (metricValues.IsEmpty)
				return true;

			var scaledMetricValues = metricValues.ToScaledValues(metricUnit);
			var roundDigits = GetRoundingDigits(scaledMetricValues, metricUnit);
			scaledMetricValues = scaledMetricValues.Round(roundDigits);

			return scaledMetricValues.Min.Equals(scaledMetricValues.Max);
		}

		private static MetricRange ToScaledValues(this MetricRange metricValues, MetricUnit metricUnit) =>
			CreateMetricRange(
				metricValues.Min.ToScaledValue(metricUnit),
				metricValues.Max.ToScaledValue(metricUnit));

		private static double ToScaledValue(this double metricValue, [NotNull] MetricUnit metricUnit) =>
			(double.IsNaN(metricValue) || metricUnit.IsEmpty)
				? metricValue
				: metricValue / metricUnit.ScaleCoefficient;

		private static MetricRange Round(this MetricRange metricValues, int roundingDigits) =>
			CreateMetricRange(
				metricValues.Min.Round(roundingDigits),
				metricValues.Max.Round(roundingDigits));

		// HACK: decimal rounding is used as it match the rounding that
		// double.ToString(format) uses undercover.
		private static double Round(this double metricValue, int roundingDigits) =>
			(double.IsNaN(metricValue) || double.IsInfinity(metricValue))
				? metricValue
				: (double)Math.Round((decimal)metricValue, roundingDigits, MidpointRounding.AwayFromZero);
		#endregion

		#region ToString
		/// <summary>Returns a <see cref="string"/> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnitScale">The metric measurement scale.</param>
		/// <returns>A <see cref="string"/> that represents the metric value.</returns>
		public static string ToString(this double metricValue, [NotNull] MetricUnitScale metricUnitScale) =>
			ToString(metricValue, metricUnitScale[metricValue]);

		/// <summary>Returns a <see cref="string"/> representation of a metric value.</summary>
		/// <param name="metricValue">The metric value.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>A <see cref="string"/> that represents the metric value.</returns>
		public static string ToString(this double metricValue, [NotNull] MetricUnit metricUnit)
		{
			metricValue = metricValue.ToScaledValue(metricUnit);
			var displayFormat = GetFormatForScaled(metricValue, metricUnit);

			var formattedValue = metricValue.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			return (double.IsNaN(metricValue) || metricUnit.IsEmpty || string.IsNullOrEmpty(metricUnit.DisplayName))
				? formattedValue
				: formattedValue + " " + metricUnit.DisplayName;
		}

		/// <summary>Returns a <see cref="string"/> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnitScale">The metric measurement scale.</param>
		/// <returns>A <see cref="string"/> that represents the metric value.</returns>
		public static string ToString(this MetricRange metricValues, [NotNull] MetricUnitScale metricUnitScale) =>
			ToString(metricValues, metricUnitScale[metricValues]);

		/// <summary>Returns a <see cref="string"/> representation of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <returns>A <see cref="string"/> that represents the metric value.</returns>
		public static string ToString(this MetricRange metricValues, [NotNull] MetricUnit metricUnit)
		{
			metricValues = ToScaledValues(metricValues, metricUnit);
			var displayFormat = GetFormatForScaled(metricValues, metricUnit);

			var formattedValue = metricValues.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			return (metricValues.IsEmpty || metricUnit.IsEmpty || string.IsNullOrEmpty(metricUnit.DisplayName))
				? formattedValue
				: formattedValue + " " + metricUnit.DisplayName;
		}

		/// <summary>Returns a <see cref="string"/> representation of min and max parts of a metric value.</summary>
		/// <param name="metricValues">Range of metric values.</param>
		/// <param name="metricUnit">The metric measurement unit.</param>
		/// <param name="minString">String representation of min part of a metric value.</param>
		/// <param name="maxString">String representation of max part of a metric value.</param>
		public static void GetMinMaxString(
			this MetricRange metricValues, [NotNull] MetricUnit metricUnit,
			out string minString, out string maxString)
		{
			metricValues = ToScaledValues(metricValues, metricUnit);
			var displayFormat = GetFormatForScaled(metricValues, metricUnit);

			minString = metricValues.Min.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
			maxString = metricValues.Max.ToString(displayFormat, HostEnvironmentInfo.MainCultureInfo);
		}
		#endregion
	}
}