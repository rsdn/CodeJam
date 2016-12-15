using System;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>Percentile-based competition limit provider.</summary>
	/// <seealso cref="ICompetitionLimitProvider"/>
	[PublicAPI]
	public class PercentileLimitProvider : CompetitionLimitProviderBase
	{
		private static readonly Range<int> _percentileRange = Range.Create(0, 100);

		/// <summary> Metric is based on 45..55th percentiles.</summary>
		public static readonly ICompetitionLimitProvider P50 = new PercentileLimitProvider(80, 10, 15);

		#region .ctor & properties
		/// <summary>
		/// Initializes a new instance of the <see cref="PercentileLimitProvider" /> class.
		/// </summary>
		/// <param name="meanPercentile">The mean percentile.</param>
		/// <param name="actualValuesPercentileDelta">Actual values percentile delta.</param>
		/// <param name="limitsPercentileDelta">Limits percentile delta.</param>
		public PercentileLimitProvider(
			int meanPercentile,
			int actualValuesPercentileDelta,
			int limitsPercentileDelta)
		{
			Code.InRange(meanPercentile, nameof(meanPercentile), 0, 100);
			var deltaMax = Math.Min(meanPercentile, 100 - meanPercentile);
			Code.InRange(actualValuesPercentileDelta, nameof(actualValuesPercentileDelta), 0, deltaMax);
			Code.InRange(limitsPercentileDelta, nameof(limitsPercentileDelta), 0, deltaMax);

			MeanPercentile = meanPercentile;
			ActualValuesPercentileDelta = actualValuesPercentileDelta;
			LimitsPercentileDelta = limitsPercentileDelta;
		}

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public override string ShortInfo => $"P{MeanPercentile}";

		/// <summary>The mean percentile.</summary>
		/// <value>The mean percentile.</value>
		public int MeanPercentile { get; }

		/// <summary>Actual values percentile delta.</summary>
		/// <value>Actual values percentile delta.</value>
		public int ActualValuesPercentileDelta { get; }

		/// <summary>Limits percentile delta.</summary>
		/// <value>Limits percentile deltas.</value>
		public int LimitsPercentileDelta { get; }

		#region Overrides of CompetitionLimitProviderBase
		/// <summary>Actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		public override double? TryGetMeanValue(Benchmark benchmark, Summary summary)
		{
			if (!TryGetReports(benchmark, summary, out var benchmarkReport, out var baselineReport))
				return null;

			var meanBenchmark = benchmarkReport.ResultStatistics.Percentiles.Percentile(MeanPercentile);
			var meanBaseline = baselineReport.ResultStatistics.Percentiles.Percentile(MeanPercentile);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (meanBaseline == 0)
				return null;

			return meanBenchmark / meanBaseline;
		}

		/// <summary>Metric that described deviation of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that described deviation for the benchmark or <c>null</c> if none.</returns>
		public override double? TryGetVariance(Benchmark benchmark, Summary summary) => null;

		/// <summary>Actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual values for the benchmark or empty range if none.</returns>
		public override LimitRange TryGetActualValues(Benchmark benchmark, Summary summary) =>
			TryGetValuesCore(
				benchmark, summary,
				MeanPercentile - ActualValuesPercentileDelta,
				MeanPercentile + ActualValuesPercentileDelta);

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Limits for the benchmark or empty range if none.</returns>
		public override LimitRange TryGetCompetitionLimit(Benchmark benchmark, Summary summary) =>
			TryGetValuesCore(
				benchmark, summary,
				MeanPercentile - LimitsPercentileDelta,
				MeanPercentile + LimitsPercentileDelta);

		private LimitRange TryGetValuesCore(
			Benchmark benchmark, Summary summary,
			int minPercentile, int maxPercentile)
		{
			if (!TryGetReports(benchmark, summary, out var benchmarkReport, out var baselineReport))
				return LimitRange.Empty;

			minPercentile = _percentileRange.Adjust(minPercentile);
			maxPercentile = _percentileRange.Adjust(maxPercentile);

			var minValueBaseline = baselineReport.ResultStatistics.Percentiles.Percentile(minPercentile);
			var maxValueBaseline = baselineReport.ResultStatistics.Percentiles.Percentile(maxPercentile);

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (minValueBaseline == 0 || maxValueBaseline == 0)
				// ReSharper restore CompareOfFloatsByEqualityOperator
				return LimitRange.Empty;

			var minValueBenchmark = benchmarkReport.ResultStatistics.Percentiles.Percentile(minPercentile);
			var maxValueBenchmark = benchmarkReport.ResultStatistics.Percentiles.Percentile(maxPercentile);

			var minRatio = minValueBenchmark / minValueBaseline;
			var maxRatio = maxValueBenchmark / maxValueBaseline;

			if (minRatio > maxRatio)
				minRatio = maxRatio;

			return LimitRange.CreateRatioLimit(
				Math.Min(minRatio, maxRatio),
				maxRatio);
		}
		#endregion
		#endregion
	}
}