using System;

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
		/// <summary>The bytes allocated per operation</summary>
		BytesAllocatedPerOperation,
		/// <summary>The bytes allocated per operation. Allocations less than 4kb (single page allocations) returned as <c>0</c></summary>
		BytesAllocatedPerOperationIgnoreFirstPage,
		/// <summary>The gen0 collections per 1000 ops.</summary>
		Gen0CollectionsPer1000,
		/// <summary>The gen1 collections per 1000 ops.</summary>
		Gen1CollectionsPer1000,
		/// <summary>The gen2 collections per 1000 ops.</summary>
		Gen2CollectionsPer1000
	}

	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that returns various values from Benchmark's GcStats
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	public class GcMetricValuesProvider : MetricValuesProviderBase
	{
		/// <summary>The category of metric values.</summary>
		public const string Category = "GcMemory";

		/// <summary>Initializes a new instance of the <see cref="TimeMetricValuesProvider" /> class.</summary>
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
			const long SinglePage = 4096;
			switch (metricSource)
			{
				case GcMetricSource.BytesAllocatedPerOperation:
					return gcStats.BytesAllocatedPerOperation;
				case GcMetricSource.BytesAllocatedPerOperationIgnoreFirstPage:
					return gcStats.AllocatedBytes <= SinglePage ? 0 : gcStats.BytesAllocatedPerOperation;
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
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport) =>
			new[] { GetGcValuesFromReport(benchmarkReport.GcStats, MetricSource) };

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(CompetitionMetricInfo metricInfo) =>
			new IDiagnoser[] { MemoryDiagnoser.Default };
	}
}