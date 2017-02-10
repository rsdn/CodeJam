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
		/// <summary>The allocated bytes</summary>
		AllocatedBytes,
		/// <summary>The bytes allocated per operation</summary>
		BytesAllocatedPerOperation,
		/// <summary>The gen0 collections</summary>
		Gen0Collections,
		/// <summary>The gen1 collections</summary>
		Gen1Collections,
		/// <summary>The gen2 collections</summary>
		Gen2Collections
	}

	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that returns various values from Benchmark's GcStats
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	public class GcMetricValuesProvider : MetricValuesProviderBase
	{
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

		private long GetGcValuesFromReport(GcStats gcStats, GcMetricSource metricSource)
		{
			switch (metricSource)
			{
				case GcMetricSource.AllocatedBytes:
					return gcStats.AllocatedBytes;
				case GcMetricSource.BytesAllocatedPerOperation:
					return gcStats.BytesAllocatedPerOperation;
				case GcMetricSource.Gen0Collections:
					return gcStats.Gen0Collections;
				case GcMetricSource.Gen1Collections:
					return gcStats.Gen1Collections;
				case GcMetricSource.Gen2Collections:
					return gcStats.Gen2Collections;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(metricSource), metricSource);
			}
		}

		/// <summary>Timings for the benchmark report in nanoseconds.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport) =>
			new double[] { GetGcValuesFromReport(benchmarkReport.GcStats, MetricSource)};

		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metricInfo">The competition metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(CompetitionMetricInfo metricInfo) =>
			new[] { MemoryDiagnoser.Default };
	}
}