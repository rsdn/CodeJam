using System;
using System.Collections.Generic;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using CodeJam.IO;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using Microsoft.Diagnostics.Tracing.Session;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Helper class that stores ETW tracing state for current benchmark run.
	/// </summary>
	internal class EtwDiagnoserAnalysis : Analysis
	{
		/// <summary>Initializes a new instance of the <see cref="EtwDiagnoserAnalysis"/> class.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="config">The config.</param>
		/// <param name="metricProviders">The metric providers.</param>
		public EtwDiagnoserAnalysis(
			[NotNull] Benchmark benchmark,
			[NotNull] IConfig config,
			[NotNull] IEtwMetricValueProvider[] metricProviders) : base(config, MessageSource.Diagnoser)
		{
			Code.NotNull(benchmark, nameof(benchmark));
			Code.NotNull(config, nameof(config));

			RunGuid = Guid.NewGuid();
			Benchmark = benchmark;
			MetricProviders = metricProviders;
			TraceFile = TempData.CreateFile(null, $"{RunGuid}.etl");
		}

		/// <summary>An unique guid that is used to mark current session events. <see cref="DiagnoserEventSource"/> for events collected during the run.</summary>
		/// <value>The unique identifier for the current session events.</value>
		public Guid RunGuid { get; }

		/// <summary>Gets or sets the benchmark.</summary>
		/// <value>The benchmark.</value>
		[NotNull]
		public Benchmark Benchmark { get; }

		/// <summary>Gets the metric providers.</summary>
		/// <value>The metric providers.</value>
		[NotNull]
		public IReadOnlyCollection<IEtwMetricValueProvider> MetricProviders { get; }

		/// <summary>Gets or sets the name of the ETW trace file, if any.</summary>
		/// <value>The ETW trace file.</value>
		[NotNull]
		public TempData.TempFile TraceFile { get; }

		/// <summary>Gets or sets the current ETW session, if any.</summary>
		/// <value>The current ETW session.</value>
		[CanBeNull]
		public TraceEventSession TraceSession { get; set; }

		/// <summary>An unique guid that is used to mark current iteration events. <see cref="DiagnoserEventSource"/> for events collected during the run.</summary>
		/// <value>The unique identifier for the current iteration events.</value>
		public Guid IterationGuid { get; set; }
	}
}