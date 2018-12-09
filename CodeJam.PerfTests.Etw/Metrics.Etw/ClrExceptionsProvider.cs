using System;
using System.Collections.Generic;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.Core;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Implementation of <see cref="IMetricValuesProvider"/> that counts amount of exceptions for the benchmark
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	/// <seealso cref="IEtwMetricValueProvider" />
	public class ClrExceptionsProvider : MetricValuesProviderBase, IEtwMetricValueProvider
	{
		/// <summary>The category of metric values.</summary>
		public const string Category = "Clr";

		private class PerBenchmarkValues : Dictionary<BenchmarkCase, long> { }

		private static readonly RunState<PerBenchmarkValues> _results = new RunState<PerBenchmarkValues>();

		/// <summary>Initializes a new instance of the <see cref="ClrExceptionsProvider"/> class.</summary>
		/// <param name="resultIsRelative"><c>true</c> if the metric is relative.</param>
		public ClrExceptionsProvider(bool resultIsRelative) : base(SingleValueMetricCalculator.Instance, resultIsRelative) { }

		#region Overrides of MetricValuesProviderBase
		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(MetricInfo metric) => new IDiagnoser[] { EtwDiagnoser.Instance };

		/// <summary>Gets the values from benchmark report.</summary>
		/// <param name="benchmarkReport">The benchmark report.</param>
		/// <param name="summary">The summary.</param>
		/// <returns>Metric values from benchmark report</returns>
		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary)
		{
			var totalOps = benchmarkReport.GcStats.TotalOperations;

			return _results[summary].TryGetValue(benchmarkReport.BenchmarkCase, out var result)
				? new[] { (double)result / totalOps }
				: new double[0];
		}
		#endregion

		#region Implementation of IEtwMetricValueProvider
		/// <summary>Gets a value indicating whether the value provider requires kernel ETW session.</summary>
		/// <value><c>true</c> if the value provider requires kernel ETW session; otherwise, <c>false</c>.</value>
		public bool IsKernelMetric => false;

		/// <summary>Gets ETW provider keywords.</summary>
		/// <value>The ETW provider keywords.</value>
		public ulong ProviderKeywords => (ulong)ClrTraceEventParser.Keywords.Exception;

		/// <summary>Gets the ETW provider unique identifier.</summary>
		/// <value>The ETW provider unique identifier.</value>
		public Guid ProviderGuid => ClrTraceEventParser.ProviderGuid;

		/// <summary>Gets ETW provider event verbosity.</summary>
		/// <value>The ETW provider event verbosity.</value>
		public TraceEventLevel EventLevel => TraceEventLevel.Informational;

		/// <summary>Attach and consume trace events.</summary>
		/// <param name="traceEventSource">The trace event source.</param>
		/// <param name="benchmark">The benchmark the event will belong to.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="filter">Filter for events that should be consumed.</param>
		/// <returns>The <see cref="IDisposable" /> to detach from metric handling.</returns>
		public IDisposable Subscribe(TraceEventSource traceEventSource, BenchmarkCase benchmark, IConfig config, Func<TraceEvent, bool> filter)
		{
			var state = _results[config];
			state[benchmark] = 0;
			void Handle(ExceptionTraceData e)
			{
				if (!filter(e)) return;
				state[benchmark]++;
			}

			traceEventSource.Clr.ExceptionStart += Handle;
			return Disposable.Create(() => traceEventSource.Clr.ExceptionStart -= Handle);
		}
		#endregion
	}
}