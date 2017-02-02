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
	public sealed class PercentileMetricCalculator : IMetricCalculator
	{
		private static readonly Range<int> _percentileRange = Range.Create(0, 100);

		/// <summary> Metric is based on 45..55th percentiles.</summary>
		public static readonly IMetricCalculator P50 = new PercentileMetricCalculator(50, 5, 15);

		/// <summary> Metric is based on 85th percentiles.</summary>
		public static readonly IMetricCalculator P85 = new PercentileMetricCalculator(85, 0, 10);

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

			return MetricRange.Create(minValue, maxValue);
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

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (minBaseline == 0 || maxBaseline == 0)
				// ReSharper restore CompareOfFloatsByEqualityOperator
				return MetricRange.Empty;

			var statValues = new Statistics(values);
			var minValues = statValues.Percentiles.Percentile(minPercentile);
			var maxValues = statValues.Percentiles.Percentile(maxPercentile);

			var minRatio = minValues / minBaseline;
			var maxRatio = maxValues / maxBaseline;

			if (minRatio > maxRatio)
				minRatio = maxRatio;

			return MetricRange.Create(minRatio, maxRatio);
		} 
		#endregion

		#region .ctor & properties
		/// <summary>
		/// Initializes a new instance of the <see cref="PercentileMetricCalculator" /> class.
		/// </summary>
		/// <param name="meanPercentile">The mean percentile.</param>
		/// <param name="actualValuesPercentileDelta">Actual values percentile delta.</param>
		/// <param name="limitsValuesPercentileDelta">Limits percentile delta.</param>
		public PercentileMetricCalculator(
			int meanPercentile,
			int actualValuesPercentileDelta,
			int limitsValuesPercentileDelta)
		{
			Code.InRange(meanPercentile, nameof(meanPercentile), 0, 100);
			Code.InRange(actualValuesPercentileDelta, nameof(actualValuesPercentileDelta), 0, 100);
			Code.InRange(limitsValuesPercentileDelta, nameof(limitsValuesPercentileDelta), 0, 100);

			MeanPercentile = meanPercentile;
			ActualValuesPercentileDelta = actualValuesPercentileDelta;
			LimitsValuesPercentileDelta = limitsValuesPercentileDelta;
		}

		/// <summary>The mean percentile.</summary>
		/// <value>The mean percentile.</value>
		public int MeanPercentile { get; }

		/// <summary>Actual values percentile delta.</summary>
		/// <value>Actual values percentile delta.</value>
		public int ActualValuesPercentileDelta { get; }

		/// <summary>Limits percentile delta.</summary>
		/// <value>Limits percentile deltas.</value>
		public int LimitsValuesPercentileDelta { get; }
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

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (meanBaseline == 0)
				return null;

			return meanValues / meanBaseline;
		}

		/// <summary>Gets variance for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetVariance(double[] values) => null; // TODO: !!! variance?

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
				MeanPercentile - LimitsValuesPercentileDelta,
				MeanPercentile + LimitsValuesPercentileDelta);

		/// <summary>Gets expected limits range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeLimitValues(double[] values, double[] baselineValues) =>
			TryGetMetricRange(
				values, baselineValues,
				MeanPercentile - LimitsValuesPercentileDelta,
				MeanPercentile + LimitsValuesPercentileDelta);
	}
}