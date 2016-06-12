using System;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Base class for metrics providers.</summary>
	/// <seealso cref="CodeJam.PerfTests.Metrics.ILimitMetricProvider" />
	public abstract class MetricProviderBase : ILimitMetricProvider
	{
		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public abstract string ShortInfo { get; }

		/// <summary>Tries to obtain metric that can be used to compare the target with the baseline.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="lowerBoundary">The lower boundary of the metric.</param>
		/// <param name="upperBoundary">The upper boundary of the metric.</param>
		/// <returns><c>true</c> if <paramref name="summary"/> contains metrics for the benchmark.</returns>
		public bool TryGetMetrics(
			Benchmark benchmark, Summary summary,
			out double lowerBoundary, out double upperBoundary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			lowerBoundary = 0;
			upperBoundary = 0;

			var baselineBenchmark = summary.TryGetBaseline(benchmark);
			if (baselineBenchmark == null)
				return false;

			var baselineReport = summary.TryGetBenchmarkReport(baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return false;

			var benchmarkReport = summary.TryGetBenchmarkReport(benchmark);
			if (benchmarkReport?.ResultStatistics == null)
				return false;

			var baselineMetricLower = TryGetMetricLower(benchmarkReport, baselineReport);
			var baselineMetricUpper = TryGetMetricUpper(benchmarkReport, baselineReport);
			if (baselineMetricLower == null || baselineMetricUpper == null)
				return false;

			lowerBoundary = baselineMetricLower.Value;
			upperBoundary = baselineMetricUpper.Value;

			if (lowerBoundary > upperBoundary)
			{
				lowerBoundary = upperBoundary;
			}

			return true;
		}

		/// <summary>Tries the get lower metric for the benchmark.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="baselineReport">The baseline report.</param>
		/// <returns>The lower metric for the benchmark or <c>null</c> if none.</returns>
		protected abstract double? TryGetMetricLower(
			[NotNull] BenchmarkReport benchmarkReport,
			[NotNull] BenchmarkReport baselineReport);

		/// <summary>Tries the get upper metric for the benchmark.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="baselineReport">The baseline report.</param>
		/// <returns>The upper metric for the benchmark or <c>null</c> if none.</returns>
		protected abstract double? TryGetMetricUpper(
			[NotNull] BenchmarkReport benchmarkReport,
			[NotNull] BenchmarkReport baselineReport);
	}
}