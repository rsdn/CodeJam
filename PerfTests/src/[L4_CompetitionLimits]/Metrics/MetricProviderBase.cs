using System;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Base class for metrics providers.</summary>
	/// <seealso cref="CodeJam.PerfTests.Metrics.ILimitMetricProvider"/>
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

			BenchmarkReport baselineReport, benchmarkReport;
			if (!TryGetReports(benchmark, summary, out baselineReport, out benchmarkReport))
				return false;

			if (!TryGetMetricsImpl(benchmarkReport, baselineReport, out lowerBoundary, out upperBoundary))
				return false;

			if (lowerBoundary > upperBoundary)
			{
				Algorithms.Swap(ref lowerBoundary, ref upperBoundary);
			}

			return true;
		}

		/// <summary>Tries to obtain metric that can be used to describ boundaries of the value.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="lowerBoundary">The lower boundary of the metric.</param>
		/// <param name="upperBoundary">The upper boundary of the metric.</param>
		/// <returns><c>true</c> if <paramref name="summary"/> contains metrics for the benchmark.</returns>
		public bool TryGetBoundaryMetrics(
			Benchmark benchmark, Summary summary,
			out double lowerBoundary, out double upperBoundary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			lowerBoundary = 0;
			upperBoundary = 0;

			BenchmarkReport baselineReport, benchmarkReport;
			if (!TryGetReports(benchmark, summary, out baselineReport, out benchmarkReport))
				return false;

			if (!TryGetBoundaryMetricsImpl(benchmarkReport, baselineReport, out lowerBoundary, out upperBoundary))
				return false;

			if (lowerBoundary > upperBoundary)
			{
				Algorithms.Swap(ref lowerBoundary, ref upperBoundary);
			}

			return true;
		}

		private static bool TryGetReports(
			Benchmark benchmark, Summary summary,
			out BenchmarkReport baselineReport, out BenchmarkReport benchmarkReport)
		{
			baselineReport = null;
			benchmarkReport = null;

			var baselineBenchmark = summary.TryGetBaseline(benchmark);
			if (baselineBenchmark == null)
				return false;

			baselineReport = summary.TryGetBenchmarkReport(baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return false;

			benchmarkReport = summary.TryGetBenchmarkReport(benchmark);
			if (benchmarkReport?.ResultStatistics == null)
				return false;

			return true;
		}

		protected abstract bool TryGetMetricsImpl(
			BenchmarkReport benchmarkReport, BenchmarkReport baselineReport,
			out double lowerBoundary, out double upperBoundary);

		protected abstract bool TryGetBoundaryMetricsImpl(
			BenchmarkReport benchmarkReport, BenchmarkReport baselineReport,
			out double lowerBoundary, out double upperBoundary);
	}
}