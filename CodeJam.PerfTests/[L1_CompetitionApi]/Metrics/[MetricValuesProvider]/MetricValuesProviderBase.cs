using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Columns;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Base implementation of the <see cref="IMetricValuesProvider"/>
	/// </summary>
	/// <seealso cref="IMetricValuesProvider"/>
	[PublicAPI]
	public abstract class MetricValuesProviderBase : IMetricValuesProvider
	{
		private static readonly IReadOnlyDictionary<MetricValueColumns, MetricValueColumn.Kind> _columnKinds =
			new Dictionary<MetricValueColumns, MetricValueColumn.Kind>
			{
				{ MetricValueColumns.Mean, MetricValueColumn.Kind.Mean },
				{ MetricValueColumns.StdDev, MetricValueColumn.Kind.StdDev },
				{ MetricValueColumns.Min, MetricValueColumn.Kind.Min },
				{ MetricValueColumns.Max, MetricValueColumn.Kind.Max }
			};

		#region Helpers
		/// <summary>Creates the column provider for the metric columns.</summary>
		/// <param name="metric">The metric to get columns for.</param>
		/// <param name="columns">The columns to include.</param>
		/// <returns>Column provider for the metric columns</returns>
		protected static IColumnProvider CreateColumnProvider([NotNull] MetricInfo metric, MetricValueColumns columns)
		{
			var resultColumns = _columnKinds
				.Where(p => columns.IsFlagSet(p.Key))
				.Select(p => (IColumn)new MetricValueColumn(metric, p.Value))
				.ToArray();

			return new SimpleColumnProvider(resultColumns);
		}

		/// <summary>Gets report for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <returns><c>true</c> if benchmark report is available.</returns>
		protected static bool TryGetReport(
			[NotNull] BenchmarkCase benchmark, [NotNull] Summary summary,
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
			[NotNull] BenchmarkCase benchmark, [NotNull] Summary summary,
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
			Code.NotNull(calculator, nameof(calculator));

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
		public double? TryGetMeanValue(BenchmarkCase benchmark, Summary summary)
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
		public double? TryGetVariance(BenchmarkCase benchmark, Summary summary)
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
		public MetricRange TryGetActualValues(BenchmarkCase benchmark, Summary summary)
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
		public MetricRange TryGetLimitValues(BenchmarkCase benchmark, Summary summary)
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
		/// <param name="metric">The metric to get columns for.</param>
		/// <param name="columns">The columns to include.</param>
		/// <returns>Column provider for the metric values</returns>
		public IColumnProvider GetColumnProvider(MetricInfo metric, MetricValueColumns columns)
		{
			Code.NotNull(metric, nameof(metric));
			EnumCode.FlagsDefined(columns, nameof(columns));
			Code.AssertArgument(
				metric.ValuesProvider == this, nameof(metric),
				"Passed ValuesProvider does not match to this one.");

			return GetColumnProviderOverride(metric, columns);
		}

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		public IDiagnoser[] GetDiagnosers(MetricInfo metric)
		{
			Code.NotNull(metric, nameof(metric));
			Code.AssertArgument(
				metric.ValuesProvider == this, nameof(metric),
				"Passed ValuesProvider does not match to this one.");

			return GetDiagnosersOverride(metric);
		}
		#endregion

		#region Default implementation (overridable)
		/// <summary>Gets column provider for the metric values.</summary>
		/// <param name="metric">The metric to get columns for.</param>
		/// <param name="columns">The columns to include.</param>
		/// <returns>Column provider for the metric values</returns>
		protected virtual IColumnProvider GetColumnProviderOverride(MetricInfo metric, MetricValueColumns columns)
		{
			if (columns == MetricValueColumns.Auto)
				columns = metric.MetricColumns;

			if (columns == MetricValueColumns.Auto)
				columns = MetricValueColumns.Mean;

			return CreateColumnProvider(metric, columns);
		}

		// TODO: better name
		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		[NotNull]
		protected abstract IDiagnoser[] GetDiagnosersOverride(MetricInfo metric);

		/// <summary>Tries to get values for the relative metric.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="metricValues">The metric values.</param>
		/// <returns><c>True</c> if values calculated successfully.</returns>
		protected virtual bool TryGetValues(
			[NotNull] BenchmarkCase benchmark, [NotNull] Summary summary,
			out double[] metricValues)
		{
			metricValues = null;

			if (!TryGetReport(benchmark, summary, out var benchmarkReport))
				return false;

			metricValues = GetValuesFromReport(benchmarkReport, summary);

			return true;
		}

		/// <summary>Tries to get values for the relative metric.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="metricValues">The metric values.</param>
		/// <param name="baselineMetricValues">The baseline metric values.</param>
		/// <returns><c>True</c> if values calculated successfully.</returns>
		protected virtual bool TryGetRelativeValues(
			[NotNull] BenchmarkCase benchmark, [NotNull] Summary summary,
			out double[] metricValues, out double[] baselineMetricValues)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(summary, nameof(summary));

			metricValues = null;
			baselineMetricValues = null;

			if (!TryGetReports(benchmark, summary, out var benchmarkReport, out var baselineReport))
				return false;

			metricValues = GetValuesFromReport(benchmarkReport, summary);
			baselineMetricValues = GetValuesFromReport(baselineReport, summary);

			return true;
		}
		#endregion

		/// <summary>Gets the values from benchmark report.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="summary">The summary.</param>
		/// <returns>Metric values from benchmark report</returns>
		[NotNull]
		protected abstract double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary);
	}
}