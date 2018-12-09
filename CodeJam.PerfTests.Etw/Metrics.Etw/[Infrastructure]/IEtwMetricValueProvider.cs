using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

using Microsoft.Diagnostics.Tracing;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	///  Base interface for <see cref="IMetricValuesProvider"/> that do provide values via ETW
	/// </summary>
	public interface IEtwMetricValueProvider : IMetricValuesProvider
	{
		/// <summary>Gets a value indicating whether the value provider requires kernel ETW session.</summary>
		/// <value><c>true</c> if the value provider requires kernel ETW session; otherwise, <c>false</c>.</value>
		bool IsKernelMetric { get; }

		/// <summary>Gets the ETW provider unique identifier.</summary>
		/// <value>The ETW provider unique identifier.</value>
		Guid ProviderGuid { get; }

		/// <summary>Gets ETW provider keywords.</summary>
		/// <value>The ETW provider keywords.</value>
		ulong ProviderKeywords { get; }

		/// <summary>Gets ETW provider event verbosity.</summary>
		/// <value>The ETW provider event verbosity.</value>
		TraceEventLevel EventLevel { get; }

		/// <summary>Attach and consume trace events.</summary>
		/// <param name="traceEventSource">The trace event source.</param>
		/// <param name="benchmark">The benchmark the event will belong to.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="filter">Filter for events that should be consumed.</param>
		/// <returns>The <see cref="IDisposable"/> to detach from metric handling.</returns>
		IDisposable Subscribe(
			[NotNull]TraceEventSource traceEventSource,
			[NotNull]Benchmark benchmark,
			[NotNull]IConfig config,
			[NotNull]Func<TraceEvent, bool> filter);
	}
}