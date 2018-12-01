using System;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Interface for metric values provider</summary>
	public interface IMetricValuesProvider
	{
		/// <summary>Gets a value indicating whether this instance is relative metric provider.</summary>
		/// <value> <c>true</c> if this instance is relative metric provider; otherwise, <c>false</c>. </value>
		bool ResultIsRelative { get; }

		/// <summary>Gets actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		double? TryGetMeanValue([NotNull] Benchmark benchmark, [NotNull] Summary summary);

		/// <summary>Gets metric that describes variance of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that describes variance for the benchmark or <c>null</c> if none.</returns>
		double? TryGetVariance([NotNull] Benchmark benchmark, [NotNull] Summary summary);

		/// <summary>Gets range that describes actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Range that describes actual values for the benchmark (or empty range if none).</returns>
		MetricRange TryGetActualValues([NotNull] Benchmark benchmark, [NotNull] Summary summary);

		/// <summary>
		/// Gets range that describes expected limits for the benchmark. Should be wider than <see cref="TryGetActualValues"/>.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Range that describes expected limits for the benchmark (or empty range if none).</returns>
		MetricRange TryGetLimitValues([NotNull] Benchmark benchmark, [NotNull] Summary summary);

		/// <summary>Gets column provider for the metric values.</summary>
		/// <param name="metric">The metric to get columns for.</param>
		/// <param name="columns">The columns to include.</param>
		/// <returns>Column provider for the metric values</returns>
		[CanBeNull]
		IColumnProvider GetColumnProvider([NotNull] MetricInfo metric, MetricValueColumns columns);

		/// <summary>Gets diagnosers for the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		[NotNull]
		IDiagnoser[] GetDiagnosers([NotNull] MetricInfo metric);
	}
}