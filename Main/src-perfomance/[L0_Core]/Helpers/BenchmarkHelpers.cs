using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

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
		public static Benchmark TryGetBaseline(this Summary summary, Benchmark benchmark) =>
			summary.Benchmarks.Where(b => b.Job == benchmark.Job && b.Parameters == benchmark.Parameters)
				.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>
		/// Groups benchmarks being run under same conditions (job+parameters)
		/// </summary>
		public static ILookup<KeyValuePair<IJob, ParameterInstances>, Benchmark> SameConditionBenchmarks(this Summary summary)
			=> summary.Benchmarks.ToLookup(b => new KeyValuePair<IJob, ParameterInstances>(b.Job, b.Parameters));

		// ReSharper disable once ParameterTypeCanBeEnumerable.Local
		public static Benchmark TryGetBaseline(
			this IGrouping<KeyValuePair<IJob, ParameterInstances>, Benchmark> benchmarkGroup)
			=> benchmarkGroup.SingleOrDefault(b => b.Target.Baseline);

		public static BenchmarkReport TryGetBenchmarkReport(this Summary summary, Benchmark benchmark) =>
			summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetScaledPercentile(
			this Summary summary, Benchmark benchmark, int baselinePercentile, int benchmarkPercentile)
		{
			var baselineBenchmark = summary.TryGetBaseline(benchmark);
			if (baselineBenchmark == null)
				return null;

			var benchmarkReport = summary.TryGetBenchmarkReport(benchmark);
			if (benchmarkReport?.ResultStatistics == null)
				return null;

			var baselineReport = summary.TryGetBenchmarkReport(baselineBenchmark);
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
		public static double? TryGetPercentile(this Summary summary, Benchmark benchmark, int percentile)
		{
			var benchmarkReport = summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

			return benchmarkReport?.ResultStatistics?.Percentiles?.Percentile(percentile);
		}

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetScaledPercentile(
			this Summary summary, Benchmark benchmark, int percentile)
		{
			var baselineBenchmark = summary.TryGetBaseline(benchmark);

			if (baselineBenchmark == null)
				return null;
			var benchmarkReport = summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

			if (benchmarkReport?.ResultStatistics == null)
				return null;

			var baselineReport = summary.Reports.SingleOrDefault(r => r.Benchmark == baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return null;

			var baselineMetric = baselineReport.ResultStatistics.Percentiles.Percentile(percentile);
			var currentMetric = benchmarkReport.ResultStatistics.Percentiles.Percentile(percentile);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (baselineMetric == 0)
				return null;

			return currentMetric / baselineMetric;
		}

		public static IOrderedEnumerable<Target> GetTargets(this Summary summary) =>
			summary.Benchmarks
				.Select(d => d.Target)
				.Distinct()
				.OrderBy(d => d.FullInfo);

		public static IOrderedEnumerable<IJob> GetJobs(this IEnumerable<Benchmark> benchmarks) =>
			benchmarks
				.Select(d => d.Job)
				.Distinct()
				.OrderBy(d => d.GetShortInfo());
		#endregion

		/// <summary>
		/// Checks that the assembly is build in debug mode.
		/// </summary>
		public static bool IsDebugAssembly(this Assembly assembly)
		{
			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}

		#region IO
		/// <summary>
		/// Writes file content without empty line at the end
		/// </summary>
		// BASEDON: http://stackoverflow.com/a/11689630
		public static void WriteFileContent(string path, string[] lines)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));

			using (var writer = File.CreateText(path))
			{
				if (lines.Length > 0)
				{
					for (var i = 0; i < lines.Length - 1; i++)
					{
						writer.WriteLine(lines[i]);
					}
					writer.Write(lines[lines.Length - 1]);
				}
			}
		}
		#endregion
	}
}