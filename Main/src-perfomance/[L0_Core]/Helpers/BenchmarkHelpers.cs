using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
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
		#region Validate environment
		// ReSharper disable HeapView.DelegateAllocation
		private static readonly IReadOnlyDictionary<string, Func<IJob, EnvironmentInfo, string>> _validationRules = new Dictionary
			<string, Func<IJob, EnvironmentInfo, string>>()
		{
			{ nameof(IJob.Affinity), NoValidation },
			{ nameof(IJob.Framework), ValidateFramework },
			{ nameof(IJob.IterationTime), NoValidation },
			{ nameof(IJob.Jit), ValidateJit },
			{ nameof(IJob.LaunchCount), NoValidation },
			{ nameof(IJob.Mode), NoValidation },
			{ nameof(IJob.Platform), ValidatePlatform },
			{ nameof(IJob.Runtime), ValidateRuntime },
			{ nameof(IJob.TargetCount), NoValidation },
			{ nameof(IJob.Toolchain), NoValidation },
			{ nameof(IJob.WarmupCount), NoValidation },
			// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/179
			// TODO: remove as fixed
			{ "Warmup", NoValidation },
			{ "Target", NoValidation },
			{ "Process", NoValidation }
		};

		// ReSharper restore HeapView.DelegateAllocation

		private static string NoValidation(IJob job, EnvironmentInfo env) => null;

		// TODO: Detect framework
		private static string ValidateFramework(IJob job, EnvironmentInfo env)
		{
			switch (job.Framework)
			{
				case Framework.Host:
					return null;
				case Framework.V40:
				case Framework.V45:
				case Framework.V451:
				case Framework.V452:
				case Framework.V46:
				case Framework.V461:
				case Framework.V462:
					return $"Should be set to {nameof(Framework.Host)}.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Framework));
			}
		}

		private static string ValidateJit(IJob job, EnvironmentInfo env)
		{
			bool isX64 = env.Architecture == "64-bit";
			switch (job.Jit)
			{
				case Jit.Host:
					return null;
				case Jit.LegacyJit:
					return !isX64 || !env.HasRyuJit
						? null
						: "The current setup does not support legacy jit.";
				case Jit.RyuJit:
					return env.HasRyuJit
						? null
						: "The current setup does not support RyuJit.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Jit));
			}
		}

		private static string ValidatePlatform(IJob job, EnvironmentInfo env)
		{
			bool isX64 = env.Architecture == "64-bit";
			switch (job.Platform)
			{
				case Platform.Host:
				case Platform.AnyCpu:
					return null;
				case Platform.X86:
					return !isX64
						? null
						: "The current process is not run as x86.";
				case Platform.X64:
					return isX64
						? null
						: "The current process is not run as x64.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Platform));
			}
		}

		// TODO: Detect runtime
		private static string ValidateRuntime(IJob job, EnvironmentInfo env)
		{
			switch (job.Runtime)
			{
				case Runtime.Host:
					return null;
				case Runtime.Clr:
				case Runtime.Mono:
				case Runtime.Dnx:
				case Runtime.Core:
					return $"Should be set to {nameof(Runtime.Host)}.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Runtime));
			}
		}

		// TODO: check that analyzers can run in-process
		// TODO: check that the target is not static class
		public static void ValidateEnvironment(Benchmark benchmark, ILogger logger)
		{
			var result = new StringBuilder();
			var env = EnvironmentInfo.GetCurrent();
			var job = benchmark.Job;
			foreach (var jobProperty in job.AllProperties)
			{
				Func<IJob, EnvironmentInfo, string> validationRule;
				if (!_validationRules.TryGetValue(jobProperty.Name, out validationRule))
				{
					var prefix = $"Property {jobProperty.Name}: ";
					logger.WriteLineError(prefix + "no validation rule specified");
					result.AppendFormat(prefix + "no validation rule specified").AppendLine();
				}
				else
				{
					var message = validationRule(job, env);
					if (!string.IsNullOrEmpty(message))
					{
						var prefix = $"Property {jobProperty.Name}: ";
						logger.WriteLineError(prefix + message);
						result.AppendLine(prefix + message);
					}
				}
			}

			if (result.Length > 0)
				throw new InvalidOperationException(result.ToString());
		}
		#endregion

		#region Statistics-related
		/// <summary>
		/// Returns the baseline for the benchmark
		/// </summary>
		public static Benchmark GetBaseline(this Summary summary, Benchmark benchmark)
			=>
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

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double? TryGetScaledPercentile(
			this Summary summary, Benchmark benchmark, int baselinePercentile, int benchmarkPercentile)
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
		public static double? TryGetPercentile(this Summary summary, Benchmark benchmark, int percentile)
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