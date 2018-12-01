using System;

using BenchmarkDotNet.Mathematics;

using CodeJam.Collections;
using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Metric calculator for absolute metrics (especially for long-runing benchmarks).
	/// Uses percentiles for results estimation.
	/// </summary>
	/// <seealso cref="IMetricCalculator"/>
	// TODO: better variance.
	[PublicAPI]
	public sealed class PercentileMetricCalculator : IMetricCalculator
	{
		private static readonly Range<int> _percentileRange = Range.Create(0, 100);

		/// <summary> Metric is based on 45..55th percentiles.</summary>
		public static readonly IMetricCalculator P50 = new PercentileMetricCalculator(50, 5, 10);

		/// <summary> Metric is based on 85th percentiles.</summary>
		public static readonly IMetricCalculator P85 = new PercentileMetricCalculator(85, 0, 1);

		/// <summary> Metric is based on 95th percentiles.</summary>
		public static readonly IMetricCalculator P95 = new PercentileMetricCalculator(95, 0, 1);

		#region Helpers
		private static bool CheckValues(double[] values) => values.NotNullNorEmpty();

		private static bool CheckValues(double[] values, double[] baselineValues) =>
			values.NotNullNorEmpty() && baselineValues.NotNullNorEmpty();

		private static MetricRange TryGetMetricRange(
			double[] values,
			int minPercentile, int maxPercentile)
		{
			Code.BugIf(minPercentile > maxPercentile, "minPercentile>maxPercentile");
			if (!CheckValues(values))
				return MetricRange.Empty;

			minPercentile = _percentileRange.Clamp(minPercentile);
			maxPercentile = _percentileRange.Clamp(maxPercentile);

			var statValues = new Statistics(values);
			var minValue = statValues.Percentiles.Percentile(minPercentile);
			var maxValue = statValues.Percentiles.Percentile(maxPercentile);

			// NB: min may be less then max due to rounding errors
			if (minValue > maxValue)
				minValue = maxValue;

			return MetricValueHelpers.CreateMetricRange(minValue, maxValue);
		}

		private static MetricRange TryGetMetricRange(
			double[] values, double[] baselineValues,
			int minPercentile, int maxPercentile)
		{
			Code.BugIf(minPercentile > maxPercentile, "minPercentile>maxPercentile");
			if (!CheckValues(values, baselineValues))
				return MetricRange.Empty;

			minPercentile = _percentileRange.Clamp(minPercentile);
			maxPercentile = _percentileRange.Clamp(maxPercentile);

			var statBaseline = new Statistics(baselineValues);
			var minBaseline = statBaseline.Percentiles.Percentile(minPercentile);
			var maxBaseline = statBaseline.Percentiles.Percentile(maxPercentile);

			if (minBaseline.Equals(0) || maxBaseline.Equals(0))
				return MetricRange.Empty;

			var statValues = new Statistics(values);
			var minValue = statValues.Percentiles.Percentile(minPercentile);
			var maxValue = statValues.Percentiles.Percentile(maxPercentile);

			// NB: min may be less then max due to rounding errors
			if (minValue > maxValue)
				minValue = maxValue;

			var minRatio = minValue / minBaseline;
			var maxRatio = maxValue / maxBaseline;

			if (minRatio > maxRatio)
				minRatio = maxRatio;

			return MetricValueHelpers.CreateMetricRange(minRatio, maxRatio);
		}
		#endregion

		#region .ctor & properties
		/// <summary>Initializes a new instance of the <see cref="PercentileMetricCalculator"/> class.</summary>
		/// <param name="meanPercentile">The mean percentile.</param>
		/// <param name="actualValuesPercentileDelta">Actual values percentile delta.</param>
		/// <param name="limitValuesPercentileDelta">Limits percentile delta.</param>
		public PercentileMetricCalculator(
			int meanPercentile,
			int actualValuesPercentileDelta,
			int limitValuesPercentileDelta)
		{
			Code.InRange(meanPercentile, nameof(meanPercentile), 0, 100);
			Code.InRange(actualValuesPercentileDelta, nameof(actualValuesPercentileDelta), 0, 100);
			Code.InRange(limitValuesPercentileDelta, nameof(limitValuesPercentileDelta), 0, 100);

			MeanPercentile = meanPercentile;
			ActualValuesPercentileDelta = actualValuesPercentileDelta;
			LimitValuesPercentileDelta = limitValuesPercentileDelta;
		}

		/// <summary>The mean percentile.</summary>
		/// <value>The mean percentile.</value>
		public int MeanPercentile { get; }

		/// <summary>Actual values percentile delta.</summary>
		/// <value>Actual values percentile delta.</value>
		public int ActualValuesPercentileDelta { get; }

		/// <summary>Limits percentile delta.</summary>
		/// <value>Limits percentile deltas.</value>
		public int LimitValuesPercentileDelta { get; }
		#endregion

		/// <summary>Gets actual value for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetMeanValue(double[] values)
		{
			if (!CheckValues(values))
				return null;

			return new Statistics(values).Percentiles.Percentile(MeanPercentile);
		}

		/// <summary>Gets actual value for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetRelativeMeanValue(double[] values, double[] baselineValues)
		{
			if (!CheckValues(values, baselineValues))
				return null;

			var meanValues = new Statistics(values).Percentiles.Percentile(MeanPercentile);
			var meanBaseline = new Statistics(baselineValues).Percentiles.Percentile(MeanPercentile);

			if (meanBaseline.Equals(0))
				return null;

			return meanValues / meanBaseline;
		}

		/// <summary>Gets variance for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetVariance(double[] values)
		{
			if (values.IsNullOrEmpty())
				return null;

			values = values.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));

			// σValues
			return Math.Exp(new Statistics(values).StandardDeviation);
		}

		/// <summary>Gets variance for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		// THANKSTO: http://stats.stackexchange.com/questions/21735/what-are-the-mean-and-variance-of-the-ratio-of-two-lognormal-variables
		public double? TryGetRelativeVariance(double[] values, double[] baselineValues)
		{
			if (values.IsNullOrEmpty() || baselineValues.IsNullOrEmpty())
				return null;

			values = values.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));
			baselineValues = baselineValues.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));

			var benchmarkStat = new Statistics(values);
			var baselineStat = new Statistics(baselineValues);

			// μZ = μValues - μBaselineValues
			var resultMean = benchmarkStat.Mean - baselineStat.Mean;
			// σZ^2 = σValues^2 + σBaselineValues^2 - covariance // assuming covariance=0 as vars are independent
			var resultVariance = benchmarkStat.Variance + baselineStat.Variance;

			// Variance(e^Z) = exp{2μZ+2σZ^2} − exp{2μZ+σZ^2}
			// STDev = sqr(Variance)
			return Math.Sqrt(Math.Exp(2 * resultMean + 2 * resultVariance) - Math.Exp(2 * resultMean + resultVariance));
		}

		/// <summary>Gets actual values range for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual values range for for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetActualValues(double[] values) =>
			TryGetMetricRange(
				values,
				MeanPercentile - ActualValuesPercentileDelta,
				MeanPercentile + ActualValuesPercentileDelta);

		/// <summary>Gets actual values range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeActualValues(double[] values, double[] baselineValues) =>
			TryGetMetricRange(
				values, baselineValues,
				MeanPercentile - ActualValuesPercentileDelta,
				MeanPercentile + ActualValuesPercentileDelta);

		/// <summary>Gets expected limits range for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetLimitValues(double[] values) =>
			TryGetMetricRange(
				values,
				MeanPercentile - LimitValuesPercentileDelta,
				MeanPercentile + LimitValuesPercentileDelta);

		/// <summary>Gets expected limits range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeLimitValues(double[] values, double[] baselineValues) =>
			TryGetMetricRange(
				values, baselineValues,
				MeanPercentile - LimitValuesPercentileDelta,
				MeanPercentile + LimitValuesPercentileDelta);
	}
}