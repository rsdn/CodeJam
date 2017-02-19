using System;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Columns;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Base implementation of the <see cref="IMetricValuesProvider"/>
	/// </summary>
	/// <seealso cref="IMetricValuesProvider"/>
	public abstract class MetricValuesProviderBase : IMetricValuesProvider
	{
		#region Helpers
		/// <summary>Gets report for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <returns><c>true</c> if benchmark report is available.</returns>
		protected static bool TryGetReport(
			[NotNull] Benchmark benchmark, [NotNull] Summary summary,
			out BenchmarkReport benchmarkReport)
		{
			benchmarkReport = summary[benchmark];
			return benchmarkReport?.ResultStatistics != null;
		}

		/// <summary>Gets reports for the benchmark and the baseline.</summary>
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

			benchmarkReport = summary[benchmark];
			if (benchmarkReport?.ResultStatistics == null)
				return false;

			baselineReport = summary[baselineBenchmark];
			if (baselineReport?.ResultStatistics == null)
				return false;

			return true;
		}
		#endregion

		#region .ctor & properties
		/// <summary>Initializes a new instance of the <see cref="MetricValuesProviderBase"/> class.</summary>
		/// <param name="calculator">The metric calculator.</param>
		/// <param name="resultIsRelative"><c>true</c> if the metric is relative.</param>
		protected MetricValuesProviderBase(
			[NotNull] IMetricCalculator calculator,
			bool resultIsRelative)
		{
			MetricCalculator = calculator;
			ResultIsRelative = resultIsRelative;
		}

		/// <summary>Gets the metric calculator.</summary>
		/// <value>The metric calculator.</value>
		[NotNull]
		protected IMetricCalculator MetricCalculator { get; }

		/// <summary>Gets a value indicating whether this instance is relative metric provider.</summary>
		/// <value> <c>true</c> if this instance is relative metric provider; otherwise, <c>false</c>. </value>
		public bool ResultIsRelative { get; }
		#endregion

		#region IMetricValuesProvider implementation
		/// <summary>Gets actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		public double? TryGetMeanValue(Benchmark benchmark, Summary summary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			if (ResultIsRelative)
			{
				if (TryGetRelativeValues(benchmark, summary, out var benchmarkValues, out var baselineValues))
					return MetricCalculator.TryGetRelativeMeanValue(benchmarkValues, baselineValues);
			}
			else if (TryGetValues(benchmark, summary, out var values))
			{
				return MetricCalculator.TryGetMeanValue(values);
			}
			return null;
		}

		/// <summary>Gets metric that describes variance of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that describes variance for the benchmark or <c>null</c> if none.</returns>
		public double? TryGetVariance(Benchmark benchmark, Summary summary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			if (ResultIsRelative)
			{
				if (TryGetRelativeValues(benchmark, summary, out var benchmarkValues, out var baselineValues))
					return MetricCalculator.TryGetRelativeVariance(benchmarkValues, baselineValues);
			}
			else if (TryGetValues(benchmark, summary, out var values))
			{
				return MetricCalculator.TryGetVariance(values);
			}
			return null;
		}

		/// <summary>Gets range that describes actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Range that describes actual values for the benchmark (or empty range if none).</returns>
		public MetricRange TryGetActualValues(Benchmark benchmark, Summary summary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			if (ResultIsRelative)
			{
				if (TryGetRelativeValues(benchmark, summary, out var benchmarkValues, out var baselineValues))
					return MetricCalculator.TryGetRelativeActualValues(benchmarkValues, baselineValues);
			}
			else if (TryGetValues(benchmark, summary, out var values))
			{
				return MetricCalculator.TryGetActualValues(values);
			}
			return MetricRange.Empty;
		}

		/// <summary>
		/// Gets range that describes expected limits for the benchmark. Should be wider than <see cref="IMetricValuesProvider.TryGetActualValues"/>.
		/// </summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Range that describes expected limits for the benchmark (or empty range if none).</returns>
		public MetricRange TryGetLimitValues(Benchmark benchmark, Summary summary)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			if (ResultIsRelative)
			{
				if (TryGetRelativeValues(benchmark, summary, out var benchmarkValues, out var baselineValues))
					return MetricCalculator.TryGetRelativeLimitValues(benchmarkValues, baselineValues);
			}
			else if (TryGetValues(benchmark, summary, out var values))
			{
				return MetricCalculator.TryGetLimitValues(values);
			}
			return MetricRange.Empty;
		}

		/// <summary>Gets column provider for the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get column for.</param>
		/// <returns>Column provider for the metric values</returns>
		public IColumnProvider GetColumnProvider(CompetitionMetricInfo metricInfo)
		{
			Code.NotNull(metricInfo, nameof(metricInfo));
			Code.AssertArgument(
				metricInfo.ValuesProvider == this, nameof(metricInfo),
				"Passed ValuesProvider does not match to this one.");

			return GetColumnProviderOverride(metricInfo);
		}

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		public IDiagnoser[] GetDiagnosers(CompetitionMetricInfo metricInfo)
		{
			Code.NotNull(metricInfo, nameof(metricInfo));
			Code.AssertArgument(
				metricInfo.ValuesProvider == this, nameof(metricInfo),
				"Passed ValuesProvider does not match to this one.");

			return GetDiagnosersOverride(metricInfo);
		}
		#endregion

		#region Default implementation (overridable)
		/// <summary>Gets column provider for the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get column for.</param>
		/// <returns>Column provider for the metric values</returns>
		protected virtual IColumnProvider GetColumnProviderOverride(CompetitionMetricInfo metricInfo) =>
			new SimpleColumnProvider(
				new CompetitionMetricColumn(metricInfo, CompetitionMetricColumn.Kind.Value),
				new CompetitionMetricColumn(metricInfo, CompetitionMetricColumn.Kind.Variance));

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected virtual IDiagnoser[] GetDiagnosersOverride(CompetitionMetricInfo metricInfo) => Array<IDiagnoser>.Empty;

		/// <summary>Tries to get values for the relative metric.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="metricValues">The metric values.</param>
		/// <returns><c>True</c> if values calculated successfully.</returns>
		protected virtual bool TryGetValues(
			[NotNull] Benchmark benchmark, [NotNull] Summary summary,
			out double[] metricValues)
		{
			metricValues = null;

			if (!TryGetReport(benchmark, summary, out var benchmarkReport))
				return false;

			metricValues = GetValuesFromReport(benchmarkReport);

			return true;
		}

		/// <summary>Tries to get values for the relative metric.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="metricValues">The metric values.</param>
		/// <param name="baselineMetricValues">The baseline metric values.</param>
		/// <returns><c>True</c> if values calculated successfully.</returns>
		protected virtual bool TryGetRelativeValues(
			[NotNull] Benchmark benchmark, [NotNull] Summary summary,
			out double[] metricValues, out double[] baselineMetricValues)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			metricValues = null;
			baselineMetricValues = null;

			if (!TryGetReports(benchmark, summary, out var benchmarkReport, out var baselineReport))
				return false;

			metricValues = GetValuesFromReport(benchmarkReport);
			baselineMetricValues = GetValuesFromReport(baselineReport);

			return true;
		}
		#endregion

		/// <summary>Gets the values from benchmark report.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected abstract double[] GetValuesFromReport(BenchmarkReport benchmarkReport);
	}
}