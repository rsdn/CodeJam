using System;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Interface for metrics provider.</summary>
	public interface ILimitMetricProvider
	{
		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		string ShortInfo { get; }

		/// <summary>Tries to obtain metric that can be used to compare the target with the baseline.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="lowerBoundary">The lower boundary of the metric.</param>
		/// <param name="upperBoundary">The upper boundary of the metric.</param>
		/// <returns><c>true</c> if <paramref name="summary"/> contains metrics for the benchmark.</returns>
		bool TryGetMetrics(Benchmark benchmark, Summary summary, out double lowerBoundary, out double upperBoundary);

		/// <summary>Tries to obtain metric that can be used to describ boundaries of the value.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="lowerBoundary">The lower boundary of the metric.</param>
		/// <param name="upperBoundary">The upper boundary of the metric.</param>
		/// <returns><c>true</c> if <paramref name="summary"/> contains metrics for the benchmark.</returns>
		bool TryGetBoundaryMetrics(Benchmark benchmark, Summary summary, out double lowerBoundary, out double upperBoundary);
	}
}