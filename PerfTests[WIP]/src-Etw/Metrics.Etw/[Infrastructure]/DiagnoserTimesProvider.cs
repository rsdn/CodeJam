using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;

using Microsoft.Diagnostics.Tracing;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Helper provider that collects start/stop times for diagnoser runs
	/// </summary>
	/// <seealso cref="MetricValuesProviderBase" />
	/// <seealso cref="IEtwMetricValueProvider" />
	internal class DiagnoserTimesProvider : MetricValuesProviderBase, IEtwMetricValueProvider
	{
		#region Static members
		private class PerBenchmarkValues : Dictionary<Benchmark, Dictionary<Guid, DiagnoserTraceScopeEvent>> { }

		private static readonly RunState<PerBenchmarkValues> _results = new RunState<PerBenchmarkValues>(true);
		#endregion

		/// <summary>Initializes a new instance of the <see cref="DiagnoserTimesProvider"/> class.</summary>
		public DiagnoserTimesProvider() : base(PercentileMetricCalculator.P50, false) { }

		/// <summary>Returns <see cref="DiagnoserTraceScopeEvent"/> events for the <paramref name="benchmark"/>.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="config">The configuration.</param>
		/// <returns><see cref="DiagnoserTraceScopeEvent"/> events for the <paramref name="benchmark"/>.</returns>
		public IEnumerable<DiagnoserTraceScopeEvent> GetEvents(Benchmark benchmark, IConfig config) =>
			_results[config].GetValueOrDefault(benchmark)?.Values.ToArray();

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
			if (!_results[summary].TryGetValue(benchmarkReport.Benchmark, out var result))
				return new double[0];

			return result.Values
				.Where(t => t.Stopped != null)
				.Select(t => (t.Stopped.Value - (t.Started ?? TimeSpan.Zero)).TotalNanoseconds())
				.ToArray();
		}
		#endregion

		#region Implementation of IEtwMetricValueProvider
		/// <summary>Gets a value indicating whether the value provider requires kernel ETW session.</summary>
		/// <value><c>true</c> if the value provider requires kernel ETW session; otherwise, <c>false</c>.</value>
		public bool IsKernelMetric => false;

		/// <summary>Gets ETW provider keywords.</summary>
		/// <value>The ETW provider keywords.</value>
		public ulong ProviderKeywords => 0;

		/// <summary>Gets the ETW provider unique identifier.</summary>
		/// <value>The ETW provider unique identifier.</value>
		public Guid ProviderGuid => DiagnoserEventSource.SourceGuid;

		/// <summary>Gets ETW provider event verbosity.</summary>
		/// <value>The ETW provider event verbosity.</value>
		public TraceEventLevel EventLevel => TraceEventLevel.Critical;

		/// <summary>Attach and consume trace events.</summary>
		/// <param name="traceEventSource">The trace event source.</param>
		/// <param name="benchmark">The benchmark the event will belong to.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="filter">Filter for events that should be consumed.</param>
		/// <returns>The <see cref="IDisposable" /> to detach from metric handling.</returns>
		public IDisposable Subscribe(
			TraceEventSource traceEventSource,
			Benchmark benchmark,
			IConfig config,
			Func<TraceEvent, bool> filter)
		{
			var events = _results[config].GetOrAdd(benchmark);

			void HandleStart(TraceEvent e)
			{
				if (filter(e))
				{
					var runId = (Guid)e.PayloadByName(DiagnoserEventSource.IterationIdPayload);
					var evt = events.GetOrAdd(runId, g => new DiagnoserTraceScopeEvent(g, e.ProcessID));
					evt.Started = TimeSpan.FromMilliseconds(e.TimeStampRelativeMSec);
				}
			}

			void HandleStop(TraceEvent e)
			{
				if (filter(e))
				{
					var runId = (Guid)e.PayloadByName(DiagnoserEventSource.IterationIdPayload);
					var evt = events.GetOrAdd(runId, g => new DiagnoserTraceScopeEvent(g, e.ProcessID));
					evt.Stopped = TimeSpan.FromMilliseconds(e.TimeStampRelativeMSec);
				}
			}

			traceEventSource.Dynamic.AddCallbackForProviderEvent(
				DiagnoserEventSource.SourceName, nameof(DiagnoserEventSource.TraceStarted), HandleStart);
			traceEventSource.Dynamic.AddCallbackForProviderEvent(
				DiagnoserEventSource.SourceName, nameof(DiagnoserEventSource.TraceStopped), HandleStop);

			return Disposable.Create(
				() =>
				{
					traceEventSource.Dynamic.RemoveCallback<TraceEvent>(HandleStart);
					traceEventSource.Dynamic.RemoveCallback<TraceEvent>(HandleStop);
				});
		}
		#endregion
	}
}