using System;
using System.Linq;

using BenchmarkDotNet.Mathematics;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Metric calculator for relative metrics (especially for microbenchmarks).
	/// Uses lognormal distribution for results estimation.
	/// </summary>
	/// <seealso cref="IMetricCalculator"/>
	public sealed class LogNormalMetricCalculator : IMetricCalculator
	{
		/// <summary>Instance of the provider.</summary>
		public static readonly IMetricCalculator Instance = new LogNormalMetricCalculator();

		#region Helpers
		private static bool TryPrepareLogValues(ref double[] values)
		{
			if (values.IsNullOrEmpty())
				return false;

			values = values.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));
			return true;
		}

		private static bool TryPrepareLogValues(ref double[] values, ref double[] baselineValues)
		{
			if (values.IsNullOrEmpty() || baselineValues.IsNullOrEmpty())
				return false;

			values = values.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));
			baselineValues = baselineValues.ConvertAll(a => a <= 0 ? 0 : Math.Log(a));
			return true;
		}
		#endregion

		/// <summary>
		/// Prevents a default instance of the <see cref="LogNormalMetricCalculator"/> class from being created.
		/// </summary>
		private LogNormalMetricCalculator() { }

		/// <summary>Gets actual value for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetMeanValue(double[] values)
		{
			if (!TryPrepareLogValues(ref values))
				return null;

			// μ = exp([ln a0 + ln a1 + … + ln aN] / N)
			return Math.Exp(values.Average());
		}

		/// <summary>Gets actual value for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetRelativeMeanValue(double[] values, double[] baselineValues)
		{
			if (!TryPrepareLogValues(ref values, ref baselineValues))
				return null;

			// μValues / μBaselineValues
			return Math.Exp(values.Average() - baselineValues.Average());
		}

		/// <summary>Gets variance for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetVariance(double[] values)
		{
			if (!TryPrepareLogValues(ref values))
				return null;

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
			if (!TryPrepareLogValues(ref values, ref baselineValues))
				return null;

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
		public MetricRange TryGetActualValues(double[] values)
		{
			var result = TryGetMeanValue(values);
			return result == null ? MetricRange.Empty : MetricValueHelpers.CreateMetricRange(result, result);
		}

		/// <summary>Gets actual values range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeActualValues(double[] values, double[] baselineValues)
		{
			var result = TryGetRelativeMeanValue(values, baselineValues);
			return result == null ? MetricRange.Empty : MetricValueHelpers.CreateMetricRange(result, result);
		}

		/// <summary>Gets expected limits range for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetLimitValues(double[] values)
		{
			var result = TryGetMeanValue(values);
			if (result == null)
				return MetricRange.Empty;

			var minRatio = result * 0.99; // 0.99 accuracy
			var maxRatio = result * 1.01; // 1.01 accuracy
			return MetricValueHelpers.CreateMetricRange(minRatio, maxRatio);
		}

		/// <summary>Gets expected limits range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeLimitValues(double[] values, double[] baselineValues)
		{
			var result = TryGetRelativeMeanValue(values, baselineValues);
			if (result == null)
				return MetricRange.Empty;

			var minRatio = result * 0.98; // 0.99*0.99 accuracy
			var maxRatio = result * 1.02; // 1.01*1.01 accuracy
			return MetricValueHelpers.CreateMetricRange(minRatio, maxRatio);
		}
	}
}