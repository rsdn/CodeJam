using System;
using System.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.CompetitionLimitProviders
{
	/// <summary>Base class for metrics providers.</summary>
	/// <seealso cref="ICompetitionLimitProvider"/>
	[PublicAPI]
	public abstract class CompetitionLimitProviderBase : ICompetitionLimitProvider
	{
		/// <summary>Reports for the benchmark and the baseline.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="baselineReport">The baseline report.</param>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <returns><c>true</c> if all is ok.</returns>
		protected static bool TryGetReports(
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

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public abstract string ShortInfo { get; }

		/// <summary>Actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual values for the benchmark or <c>null</c> if none.</returns>
		public CompetitionLimit TryGetActualValues(Benchmark benchmark, Summary summary) =>
			TryGetCompetitionLimit(benchmark, summary, false);

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Limits for the benchmark or <c>null</c> if none.</returns>
		public CompetitionLimit TryGetLimitForActualValues(Benchmark benchmark, Summary summary) =>
			TryGetCompetitionLimit(benchmark, summary, true);

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="limitMode">If <c>true</c> limit values should be returned. Actual values returned otherwise.</param>
		/// <returns>Limits for the benchmark or <c>null</c> if none.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual CompetitionLimit TryGetCompetitionLimit(Benchmark benchmark, Summary summary, bool limitMode)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			BenchmarkReport baselineReport, benchmarkReport;
			if (!TryGetReports(benchmark, summary, out baselineReport, out benchmarkReport))
				return null;

			var timingRatios = benchmarkReport
				.GetResultRuns()
				.Zip(
					baselineReport.GetResultRuns(),
					(r1, r2) => r1.GetAverageNanoseconds() / r2.GetAverageNanoseconds())
				.Where(r => !double.IsNaN(r) && !double.IsInfinity(r))
				.ToArray();

			if (timingRatios.Length == 0)
				return null;

			return TryGetCompetitionLimitImpl(timingRatios, limitMode);
		}

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="timingRatios">Timing ratios relative to the baseline.</param>
		/// <param name="limitMode">If <c>true</c> limit values should be returned. Actual values returned otherwise.</param>
		/// <returns>Limits for the benchmark or <c>null</c> if none.</returns>
		protected abstract CompetitionLimit TryGetCompetitionLimitImpl(double[] timingRatios, bool limitMode);
	}
}