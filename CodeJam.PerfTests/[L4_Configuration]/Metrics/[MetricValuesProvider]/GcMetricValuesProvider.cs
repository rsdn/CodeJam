using System;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Property of the <see cref="GcStats"/> to be used as a GC metric value
	/// </summary>
	public enum GcMetricSource
	{
		/// <summary>
		/// Bytes allocated per operation.
		/// Noise allocations (total bytes allocated less than gc allocation quantum are returned as <c>0</c>.
		/// </summary>
		BytesAllocatedPerOperation,

		/// <summary>
		/// Bytes allocated per operation including noise allocations (those with total bytes allocated less than gc allocation quantum).
		/// </summary>
		BytesAllocatedPerOperationRaw,

		/// <summary>Gen0 collections per 1000 ops.</summary>
		Gen0CollectionsPer1000,

		/// <summary>Gen1 collections per 1000 ops.</summary>
		Gen1CollectionsPer1000,

		/// <summary>Gen2 collections per 1000 ops.</summary>
		Gen2CollectionsPer1000
	}

	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that returns various values from BenchmarkCase's GcStats
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase"/>
	public class GcMetricValuesProvider : MetricValuesProviderBase
	{
		/// <summary>
		/// Memory diagnoser that does not report any columns.
		/// </summary>
		/// <seealso cref="BenchmarkDotNet.Diagnosers.MemoryDiagnoser" />
		/// <seealso cref="BenchmarkDotNet.Diagnosers.IDiagnoser" />
		private sealed class MemoryDiagnoserNoColumns : MemoryDiagnoser, IDiagnoser
		{
			/// <summary>Instance of the memory diagnoser.</summary>
			public static readonly new MemoryDiagnoserNoColumns Default = new MemoryDiagnoserNoColumns();

			/// <summary>Prevents a default instance of the <see cref="MemoryDiagnoserNoColumns"/> class from being created.</summary>
			private MemoryDiagnoserNoColumns() { }

			/// <summary>Gets the column provider.</summary>
			/// <returns>The column provider.</returns>
			public new IColumnProvider GetColumnProvider() =>
				// no columns, uses implementation from values provider;
				new SimpleColumnProvider();
		}

		/// <summary>The category of metric values.</summary>
		public const string Category = "GcMemory";

		/// <summary>Initializes a new instance of the <see cref="TimeMetricValuesProvider"/> class.</summary>
		/// <param name="metricSource">Property of the <see cref="GcStats"/> to be used as a GC metric value.</param>
		/// <param name="resultIsRelative"><c>true</c> if the metric is relative.</param>
		public GcMetricValuesProvider(GcMetricSource metricSource, bool resultIsRelative) :
			base(SingleValueMetricCalculator.Instance, resultIsRelative)
		{
			EnumCode.Defined(metricSource, nameof(metricSource));
			MetricSource = metricSource;
		}

		/// <summary>Property of the <see cref="GcStats"/> to be used as a GC metric value.</summary>
		/// <value>Property of the <see cref="GcStats"/> to be used as a GC metric value.</value>
		public GcMetricSource MetricSource { get; }

		private double GetGcValuesFromReport(GcStats gcStats, GcMetricSource metricSource)
		{
			switch (metricSource)
			{
				case GcMetricSource.BytesAllocatedPerOperation:
					return gcStats.BytesAllocatedPerOperation;
				case GcMetricSource.BytesAllocatedPerOperationRaw:
					return gcStats.GetTotalAllocatedBytes(false) * 1.0 / gcStats.TotalOperations;
				case GcMetricSource.Gen0CollectionsPer1000:
					return 1000.0 * gcStats.Gen0Collections / gcStats.TotalOperations;
				case GcMetricSource.Gen1CollectionsPer1000:
					return 1000.0 * gcStats.Gen1Collections / gcStats.TotalOperations;
				case GcMetricSource.Gen2CollectionsPer1000:
					return 1000.0 * gcStats.Gen2Collections / gcStats.TotalOperations;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(metricSource), metricSource);
			}
		}

		/// <summary>Timings for the benchmark report in nanoseconds.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="summary">The summary.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary) =>
			new[] { GetGcValuesFromReport(benchmarkReport.GcStats, MetricSource) };

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(MetricInfo metric) =>
			new IDiagnoser[] { MemoryDiagnoserNoColumns.Default };
	}
}