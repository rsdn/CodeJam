using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Helpers
{
	// TODO: move to different classes
	/// <summary>
	/// Helper methods for benchmark infrastructure
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class BenchmarkHelpers
	{
		#region Benchmark-related

		#region Selects
		/// <summary>
		/// Returns the baseline for the benchmark
		/// </summary>
		public static Benchmark TryGetBaseline(this Summary summary, Benchmark benchmark) =>
			summary.Benchmarks.Where(b => b.Job == benchmark.Job && b.Parameters == benchmark.Parameters)
				.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>
		/// Returns the report for the benchmark
		/// </summary>
		public static BenchmarkReport TryGetBenchmarkReport(this Summary summary, Benchmark benchmark) =>
			summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);

		/// <summary>
		/// Groups benchmarks being run under same conditions (job+parameters)
		/// </summary>
		public static ILookup<KeyValuePair<IJob, ParameterInstances>, Benchmark> SameConditionBenchmarks(
			this Summary summary) =>
				summary.Benchmarks.ToLookup(b => new KeyValuePair<IJob, ParameterInstances>(b.Job, b.Parameters));

		/// <summary>
		/// Returns targets for the summary
		/// </summary>
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static IOrderedEnumerable<Target> GetTargets(this Summary summary) =>
			summary.Benchmarks
				.Select(d => d.Target)
				.Distinct()
				.OrderBy(d => d.FullInfo);

		/// <summary>
		/// Returns jobs usede in the benchmarks
		/// </summary>
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static IOrderedEnumerable<IJob> GetJobs(this IEnumerable<Benchmark> benchmarks) =>
			benchmarks
				.Select(d => d.Job)
				.Distinct()
				.OrderBy(d => d.GetShortInfo());
		#endregion

		#region Percentiles
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
			this Summary summary, Benchmark benchmark, int percentile) =>
				TryGetScaledPercentile(summary, benchmark, percentile, percentile);

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		private static double? TryGetScaledPercentile(
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

			var baselineMetric = baselineReport.ResultStatistics.Percentiles.Percentile(baselinePercentile);
			var benchmarkMetric = benchmarkReport.ResultStatistics.Percentiles.Percentile(benchmarkPercentile);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (baselineMetric == 0)
				return null;

			return benchmarkMetric / baselineMetric;
		}
		#endregion

		#endregion

		#region Reflection
		/// <summary>
		/// Checks that the assembly is build in debug mode.
		/// </summary>
		public static bool IsDebugAssembly(this Assembly assembly)
		{
			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}
		#endregion

		#region Process
		/// <summary>
		/// Tries to change the priority of the process
		/// </summary>
		public static void SetPriority(
			this Process process, ProcessPriorityClass priority, ILogger logger)
		{
			try
			{
				process.PriorityClass = priority;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					string.Format(
						"Failed to set up priority {1}. Make sure you have the right permissions. Message: {0}", ex.Message,
						priority));
			}
		}

		/// <summary>
		/// Tries to change the priority of the process
		/// </summary>
		public static void SetAffinity(
			this Process process, IntPtr processorAffinity, ILogger logger)
		{
			try
			{
				process.ProcessorAffinity = processorAffinity;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					string.Format(
						"Failed to set up processor affinity 0x{1:X}. Make sure you have the right permissions. Message: {0}",
						ex.Message,
						(long)processorAffinity));
			}
		}
		#endregion

		#region IO
		/// <summary>
		/// Writes file content without empty line at the end
		/// </summary>
		// THANKSTO: http://stackoverflow.com/a/11689630
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

		public static TextReader TryGetTextFromUri(string uri)
		{
			var uriInst = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uriInst.IsAbsoluteUri && !uriInst.IsFile)
			{
				try
				{
					using (var webClient = new WebClient())
					{
						return new StringReader(webClient.DownloadString(uriInst));
					}
				}
				catch (WebException)
				{
					return null;
				}
			}

			if (!File.Exists(uri))
				return null;

			return File.OpenText(uriInst.IsAbsoluteUri ? uriInst.LocalPath : uri);
		}
		#endregion
	}
}