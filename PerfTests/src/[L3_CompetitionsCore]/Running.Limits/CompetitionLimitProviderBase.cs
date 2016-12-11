using System;
using System.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>Base class for metrics providers.</summary>
	/// <seealso cref="ICompetitionLimitProvider"/>
	[PublicAPI]
	public abstract class CompetitionLimitProviderBase : ICompetitionLimitProvider
	{
		#region Helpers
		/// <summary>Reports for the benchmark and the baseline.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="baselineReport">The baseline report.</param>
		/// <returns><c>true</c> if both benchmark and baseline reports are available.</returns>
		protected static bool TryGetReports(
			[NotNull] Benchmark benchmark, [NotNull] Summary summary,
			out BenchmarkReport benchmarkReport, out BenchmarkReport baselineReport)
		{
			benchmarkReport = null;
			baselineReport = null;

			var baselineBenchmark = summary.TryGetBaseline(benchmark);
			if (baselineBenchmark == null)
				return false;

			benchmarkReport = summary.TryGetBenchmarkReport(benchmark);
			if (benchmarkReport?.ResultStatistics == null)
				return false;

			baselineReport = summary.TryGetBenchmarkReport(baselineBenchmark);
			if (baselineReport?.ResultStatistics == null)
				return false;

			return true;
		}

		/// <summary>
		/// Timings for the benchmark and the baseline.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkTimings">The benchmark timings.</param>
		/// <param name="baselineTimings">The baseline timings.</param>
		/// <returns><c>true</c> if both benchmark and baseline timings are available.</returns>
		protected static bool TryGetTimings(
			[NotNull] Benchmark benchmark, [NotNull] Summary summary, 
			out double[] benchmarkTimings, out double[] baselineTimings)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			benchmarkTimings = null;
			baselineTimings = null;

			if (!TryGetReports(benchmark, summary, out var benchmarkReport, out var baselineReport))
				return false;

			benchmarkTimings = benchmarkReport.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds())
				.ToArray();
			baselineTimings = baselineReport.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds())
				.ToArray();

			return true;
		}

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public abstract string ShortInfo { get; }
		#endregion

		#region Methods to implement
		/// <summary>Actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		public abstract double? TryGetMeanValue(Benchmark benchmark, Summary summary);

		/// <summary>Metric that described deviation of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that described deviation for the benchmark or <c>null</c> if none.</returns>
		public abstract double? TryGetVariance(Benchmark benchmark, Summary summary);

		/// <summary>Actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual values for the benchmark or empty range if none.</returns>
		public abstract LimitRange TryGetActualValues(Benchmark benchmark, Summary summary);

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Limits for the benchmark or empty range if none.</returns>
		public abstract LimitRange TryGetCompetitionLimit(Benchmark benchmark, Summary summary); 
		#endregion
	}
}