using System;
using System.Collections.Generic;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Type of IO metric
	/// </summary>
	public enum IoMetricSource
	{
		/// <summary>
		/// Collects information using <see cref="KernelTraceEventParser.FileIORead"/>
		/// </summary>
		FileIoRead,

		/// <summary>
		/// Collects information using <see cref="KernelTraceEventParser.FileIOWrite"/>
		/// </summary>
		FileIoWrite,
	}


	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that collects IO information for the benchmark
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	/// <seealso cref="IEtwMetricValueProvider" />
	public class FileIoMetricProvider : MetricValuesProviderBase, IEtwMetricValueProvider
	{
		private class IoData
		{
			public long FileIoRead { get; set; }
			public long FileIoWrite { get; set; }
		}
		/// <summary>The category of metric values.</summary>
		public const string Category = "IO";

		private class PerBenchmarkValues : Dictionary<Benchmark, IoData> { }

		private static readonly RunState<PerBenchmarkValues> _results = new RunState<PerBenchmarkValues>();

		/// <summary>Initializes a new instance of the <see cref="FileIoMetricProvider"/> class.</summary>
		/// <param name="metricSource">The IO metric source.</param>
		/// <param name="resultIsRelative">if set to <c>true</c> [result is relative].</param>
		public FileIoMetricProvider(IoMetricSource metricSource, bool resultIsRelative)
			: base(SingleValueMetricCalculator.Instance, resultIsRelative)
		{
			EnumCode.Defined(metricSource, nameof(metricSource));
			MetricSource = metricSource;
		}

		/// <summary>Gets the IO metric source.</summary>
		/// <value>The IO metric source.</value>
		public IoMetricSource MetricSource { get; }

		#region Overrides of MetricValuesProviderBase
		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(MetricInfo metric)
		{
			return new IDiagnoser[] { EtwDiagnoser.Instance };
		}

		/// <summary>Gets the values from benchmark report.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="summary">The summary.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary)
		{
			var totalOps = benchmarkReport.GcStats.TotalOperations;
			if (!_results[summary].TryGetValue(benchmarkReport.Benchmark, out var result)) return new double[0];
			switch (MetricSource)
			{
				case IoMetricSource.FileIoRead:
					return new[] { (double)result.FileIoRead / totalOps };
				case IoMetricSource.FileIoWrite:
					return new[] { (double)result.FileIoWrite / totalOps };
				default:
					throw CodeExceptions.UnexpectedValue(MetricSource);
			}
		}
		#endregion

		#region Implementation of IEtwMetricValueProvider
		/// <summary>Gets a value indicating whether the value provider requires kernel ETW session.</summary>
		/// <value><c>true</c> if the value provider requires kernel ETW session; otherwise, <c>false</c>.</value>
		public bool IsKernelMetric => true;

		/// <summary>Gets ETW provider keywords.</summary>
		/// <value>The ETW provider keywords.</value>
		public ulong ProviderKeywords => (ulong)KernelTraceEventParser.Keywords.FileIOInit;

		/// <summary>Gets the ETW provider unique identifier.</summary>
		/// <value>The ETW provider unique identifier.</value>
		public Guid ProviderGuid => KernelTraceEventParser.ProviderGuid;

		/// <summary>Gets ETW provider event verbosity.</summary>
		/// <value>The ETW provider event verbosity.</value>
		public TraceEventLevel EventLevel => TraceEventLevel.Informational;

		/// <summary>Attach and consume trace events.</summary>
		/// <param name="traceEventSource">The trace event source.</param>
		/// <param name="benchmark">The benchmark the event will belong to.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="filter">Filter for events that should be consumed.</param>
		/// <returns>The <see cref="IDisposable" /> to detach from metric handling.</returns>
		public IDisposable Subscribe(TraceEventSource traceEventSource, Benchmark benchmark, IConfig config, Func<TraceEvent, bool> filter)
		{
			var accumulator = _results[config].GetOrAdd(benchmark);
			var metricSource = MetricSource;
			void Handle(FileIOReadWriteTraceData e)
			{
				if (!filter(e)) return;

				switch (metricSource)
				{
					case IoMetricSource.FileIoRead:
						accumulator.FileIoRead += e.IoSize;
						break;
					case IoMetricSource.FileIoWrite:
						accumulator.FileIoWrite += e.IoSize;
						break;
					default:
						throw CodeExceptions.UnexpectedValue(metricSource);
				}
			}

			switch (metricSource)
			{
				case IoMetricSource.FileIoRead:
					accumulator.FileIoRead = 0;
					traceEventSource.Kernel.FileIORead += Handle;
					return Disposable.Create(() => traceEventSource.Kernel.FileIORead -= Handle);
				case IoMetricSource.FileIoWrite:
					accumulator.FileIoWrite = 0;
					traceEventSource.Kernel.FileIOWrite += Handle;
					return Disposable.Create(() => traceEventSource.Kernel.FileIOWrite -= Handle);
				default:
					throw CodeExceptions.UnexpectedValue(metricSource);
			}
		}
		#endregion
	}
}