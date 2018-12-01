using System;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Helper calculator for metrics that provide only single value instead of a multiple sample set.
	/// </summary>
	/// <seealso cref="IMetricCalculator"/>
	public sealed class SingleValueMetricCalculator : IMetricCalculator
	{
		/// <summary>Instance of the provider.</summary>
		public static readonly IMetricCalculator Instance = new SingleValueMetricCalculator();

		#region Helpers
		private static double? TryGetSingleValue(double[] values)
		{
			if (values.IsNullOrEmpty())
				return null;

			if (values.Length > 1)
				throw CodeExceptions.InvalidOperation(
					$"The {nameof(SingleValueMetricCalculator)} should be used for single item arrays only.");

			return values[0];
		}
		#endregion

		/// <summary>
		/// Prevents a default instance of the <see cref="LogNormalMetricCalculator"/> class from being created.
		/// </summary>
		private SingleValueMetricCalculator() { }

		/// <summary>Gets actual value for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetMeanValue(double[] values) => TryGetSingleValue(values);

		/// <summary>Gets actual value for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetRelativeMeanValue(double[] values, double[] baselineValues)
		{
			var x = TryGetSingleValue(values);
			var y = TryGetSingleValue(baselineValues);

			return x / y;
		}

		/// <summary>Gets variance for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetVariance(double[] values) => null;

		/// <summary>Gets variance for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		public double? TryGetRelativeVariance(double[] values, double[] baselineValues) => null;

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
		public MetricRange TryGetLimitValues(double[] values) =>
			TryGetActualValues(values);

		/// <summary>Gets expected limits range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		public MetricRange TryGetRelativeLimitValues(double[] values, double[] baselineValues) =>
			TryGetRelativeActualValues(values, baselineValues);
	}
}