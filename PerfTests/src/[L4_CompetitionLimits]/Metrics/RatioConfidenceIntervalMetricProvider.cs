using System;
using System.Linq;

using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Percentile metric provider.</summary>
	/// <seealso cref="CodeJam.PerfTests.Metrics.ILimitMetricProvider"/>
	[PublicAPI]
	public class RatioConfidenceIntervalMetricProvider : MetricProviderBase
	{
		public static readonly ILimitMetricProvider Instance = new RatioConfidenceIntervalMetricProvider();

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public override string ShortInfo => "CI";

		protected override bool TryGetMetricsImpl(
			BenchmarkReport benchmarkReport, BenchmarkReport baselineReport,
			out double lowerBoundary, out double upperBoundary)
		{
			var samples = benchmarkReport
				.GetResultRuns()
				.Zip(
					baselineReport.GetResultRuns(),
					(r1, r2) => r1.GetAverageNanoseconds() / r2.GetAverageNanoseconds());

			var ci = new Statistics(samples).ConfidenceInterval;

			lowerBoundary = ci.Mean;
			upperBoundary = ci.Mean;
			return true;
		}

		protected override bool TryGetBoundaryMetricsImpl(
			BenchmarkReport benchmarkReport, BenchmarkReport baselineReport,
			out double lowerBoundary, out double upperBoundary)
		{
			var samples = benchmarkReport
				.GetResultRuns()
				.Zip(
					baselineReport.GetResultRuns(),
					(r1, r2) => r1.GetAverageNanoseconds() / r2.GetAverageNanoseconds());

			var ci = new Statistics(samples).ConfidenceInterval;
			lowerBoundary = ci.Lower;
			upperBoundary = ci.Upper;
			return true;
		}
	}
}