using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Helper type that allows to reuse statistic calculation logic across different implementations
	/// </summary>
	public interface IMetricCalculator
	{
		/// <summary>Gets actual value for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		double? TryGetMeanValue([NotNull] double[] values);

		/// <summary>Gets actual value for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual value for the set of values or <c>null</c> if none.</returns>
		[Pure]
		double? TryGetRelativeMeanValue([NotNull] double[] values, [NotNull] double[] baselineValues);

		/// <summary>Gets variance for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		double? TryGetVariance([NotNull] double[] values);

		/// <summary>Gets variance for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Variance for the set of values or <c>null</c> if none.</returns>
		[Pure]
		double? TryGetRelativeVariance([NotNull] double[] values, [NotNull] double[] baselineValues);

		/// <summary>Gets actual values range for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Actual values range for for the set of values (or empty range if none).</returns>
		[Pure]
		MetricRange TryGetActualValues([NotNull] double[] values);

		/// <summary>Gets actual values range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Actual values range for the set of values (or empty range if none).</returns>
		[Pure]
		MetricRange TryGetRelativeActualValues([NotNull] double[] values, [NotNull] double[] baselineValues);

		/// <summary> Gets expected limits range for the set of values.</summary>
		/// <param name="values">Set of values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		MetricRange TryGetLimitValues([NotNull] double[] values);

		/// <summary> Gets expected limits range for the set of values (relative metric).</summary>
		/// <param name="values">Set of values.</param>
		/// <param name="baselineValues">The baseline values.</param>
		/// <returns>Limit values range for the set of values (or empty range if none).</returns>
		[Pure]
		MetricRange TryGetRelativeLimitValues([NotNull] double[] values, [NotNull] double[] baselineValues);
	}
}