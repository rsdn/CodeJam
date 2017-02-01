using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Interface for metric values provider</summary>
	public interface IMetricValuesProvider
	{
		// TODO: diagnosers, columns?

		/// <summary>Gets a value indicating whether this instance is relative metric provider.</summary>
		/// <value> <c>true</c> if this instance is relative metric provider; otherwise, <c>false</c>. </value>
		bool ResultIsRelative { get; }

		/// <summary>Gets actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		double? TryGetMeanValue(Benchmark benchmark, Summary summary);

		/// <summary>Gets metric that describes variance of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that describes variance for the benchmark or <c>null</c> if none.</returns>
		double? TryGetVariance(Benchmark benchmark, Summary summary);

		/// <summary>Gets range that describes actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Range that describes actual values for the benchmark (or empty range if none).</returns>
		MetricRange TryGetActualValues(Benchmark benchmark, Summary summary);

		/// <summary>
		/// Gets range that describes expected limits for the benchmark. Should be wider than <see cref="TryGetActualValues"/>.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Range that describes expected limits for the benchmark (or empty range if none).</returns>
		MetricRange TryGetLimitValues(Benchmark benchmark, Summary summary);
	}
}