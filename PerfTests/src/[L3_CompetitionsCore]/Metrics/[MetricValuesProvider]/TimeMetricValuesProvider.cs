using System;
using System.Linq;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that returns timings 
	/// (in nanoseconds if the <see cref="MetricValuesProviderBase.ResultIsRelative"/> is <c>false</c>).
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	public class TimeMetricValuesProvider : MetricValuesProviderBase
	{
		/// <summary>Initializes a new instance of the <see cref="TimeMetricValuesProvider" /> class.</summary>
		/// <param name="calculator">The metric calculator.</param>
		/// <param name="resultIsRelative"><c>true</c> if the metric is relative.</param>
		public TimeMetricValuesProvider([NotNull] IMetricCalculator calculator, bool resultIsRelative) : base(calculator, resultIsRelative) { }

		/// <summary>
		/// Timings for the benchmark in nanoseconds.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkTimings">The benchmark timings.</param>
		///  <returns><c>true</c> if benchmark timings are available.</returns>
		protected override bool TryGetValues(Benchmark benchmark, Summary summary, out double[] benchmarkTimings)
		{
			benchmarkTimings = null;

			if (!TryGetReport(benchmark, summary, out var benchmarkReport))
				return false;

			benchmarkTimings = benchmarkReport.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds())
				.ToArray();

			return true;
		}

		/// <summary>
		/// Timings for the benchmark and the baseline in nanoseconds.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkTimings">The benchmark timings.</param>
		/// <param name="baselineTimings">The baseline timings.</param>
		/// <returns><c>true</c> if both benchmark and baseline timings are available.</returns>
		protected override bool TryGetRelativeValues(Benchmark benchmark, Summary summary, out double[] benchmarkTimings, out double[] baselineTimings)
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
	}


}