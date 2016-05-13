using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	/// <summary>
	/// Helper methods for benchmark infrastructure
	/// </summary>
	public static class BenchmarkHelpers
	{
		#region Statistics-related
		/// <summary>
		/// Returns the baseline for the benchmark
		/// </summary>
		public static Benchmark GetBaseline(this Summary summary, Benchmark benchmark) =>
			summary.Benchmarks
				.Where(b => b.Job == benchmark.Job && b.Parameters == benchmark.Parameters)
				.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>
		/// Groups benchmarks being run under same conditions (job+parameters)
		/// </summary>
		public static ILookup<KeyValuePair<IJob, ParameterInstances>, Benchmark> SameConditionBenchmarks(this Summary summary)
			=> summary.Benchmarks.ToLookup(b => new KeyValuePair<IJob, ParameterInstances>(b.Job, b.Parameters));

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetScaledPercentile(
			this Summary summary, Benchmark benchmark,
			int baselinePercentile,
			int benchmarkPercentile)
		{
			var baselineBenchmark = summary.GetBaseline(benchmark);
			if (baselineBenchmark == null)
				return null;

			var benchmarkReport = summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);
			if (benchmarkReport?.ResultStatistics == null)
				return null;

			var baselineReport = summary.Reports.SingleOrDefault(r => r.Benchmark == baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return null;

			var baselineMetric = benchmarkReport.ResultStatistics.Percentiles.Percentile(baselinePercentile);
			var benchmarkMetric = baselineReport.ResultStatistics.Percentiles.Percentile(benchmarkPercentile);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (baselineMetric == 0)
				return null;

			return benchmarkMetric / baselineMetric;
		}

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetPercentile(this Summary summary, Benchmark benchmark,int percentile)
		{
			var benchmarkReport = summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

			return benchmarkReport?.ResultStatistics?.Percentiles?.Percentile(percentile);
		}

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetScaledPercentile(this Summary summary, Benchmark benchmark, double percentileRatio)
		{
			var baselineBenchmark = summary.GetBaseline(benchmark);

			if (baselineBenchmark == null)
				return null;
			var benchmarkReport = summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

			if (benchmarkReport?.ResultStatistics == null)
				return null;

			var baselineReport = summary.Reports.SingleOrDefault(r => r.Benchmark == baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return null;

			var prc = (int)(percentileRatio * 100);
			var baselineMetric = benchmarkReport.ResultStatistics.Percentiles.Percentile(prc);
			var currentMetric = baselineReport.ResultStatistics.Percentiles.Percentile(prc);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (baselineMetric == 0)
				return null;

			return currentMetric / baselineMetric;
		}
		#endregion
	}
}