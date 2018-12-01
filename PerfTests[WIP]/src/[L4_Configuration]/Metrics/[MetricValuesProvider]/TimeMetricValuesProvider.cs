using System;
using System.Linq;

using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that returns timings
	/// (in nanoseconds if the <see cref="MetricValuesProviderBase.ResultIsRelative"/> is <c>false</c>).
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase"/>
	public class TimeMetricValuesProvider : MetricValuesProviderBase
	{
		/// <summary>The category of metric values.</summary>
		public const string Category = "Time";

		/// <summary>Initializes a new instance of the <see cref="TimeMetricValuesProvider"/> class.</summary>
		/// <param name="calculator">The metric calculator.</param>
		/// <param name="resultIsRelative"><c>true</c> if the metric is relative.</param>
		public TimeMetricValuesProvider([NotNull] IMetricCalculator calculator, bool resultIsRelative)
			: base(calculator, resultIsRelative) { }

		/// <summary>Timings for the benchmark report in nanoseconds.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="summary">The summary.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary) =>
			benchmarkReport.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds())
				.ToArray();

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(MetricInfo metric) =>
			Array<IDiagnoser>.Empty;
	}
}