using System;

using BenchmarkDotNet.Running;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using CodeJam.Collections;
using CodeJam.Strings;
using CodeJam.PerfTests.Running.Core;
using CodeJam.Ranges;

using JetBrains.Annotations;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

using BenchmarkDotNet.Exporters;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Infrastructure diagnoser for <see cref="IEtwMetricValueProvider"/> providers.
	/// </summary>
	/// <seealso cref="IDiagnoser" />
	/// <seealso cref="IEtwMetricValueProvider" />
	public class EtwDiagnoser : IDiagnoser
	{
		#region Helper types & static members
		private class DiagnoserState
		{
			public EtwDiagnoserAnalysis Analysis { get; set; }
		}

		private static readonly RunState<DiagnoserState> _diagnoserState =
			new RunState<DiagnoserState>(clearBeforeEachRun: true);

		/// <summary>The instance ow the ETW diagnoser.</summary>
		public static readonly EtwDiagnoser Instance = new EtwDiagnoser();
		#endregion

		#region Fields & .ctor()
		// TODO: remove the field after the Stop() method will have access to the _diagnoserState.
		[CanBeNull]
		private EtwDiagnoserAnalysis _analysis;

		/// <summary>Prevents a default instance of the <see cref="EtwDiagnoser"/> class from being created.</summary>
		private EtwDiagnoser() { }
		#endregion

		#region State manipulation
		private EtwDiagnoserAnalysis CreateAnalysis(
			BenchmarkCase benchmark, IConfig config, IEtwMetricValueProvider[] metricProviders)
		{
			var diagnoserState = _diagnoserState[config];
			Code.BugIf(diagnoserState.Analysis != null, "runState.Analysis != null");
			Code.BugIf(_analysis != null, "_analysis != null");

			var analysis = new EtwDiagnoserAnalysis(benchmark, config, metricProviders);
			if (analysis.Config.KeepBenchmarkFiles)
			{
				analysis.TraceFile.SuppressDelete();
			}
			diagnoserState.Analysis = analysis;
			_analysis = analysis;
			return analysis;
		}

		private void CompleteTraceSession(EtwDiagnoserAnalysis analysis)
		{
			var traceSession = analysis.TraceSession;
			if (traceSession != null)
			{
				traceSession.Flush();
				traceSession.Dispose();
			}
		}

		private void DisposeAnalysis(EtwDiagnoserAnalysis analysis)
		{
			var config = analysis.Config;

			analysis.TraceSession?.Dispose();
			analysis.TraceFile.Dispose();

			var runState = _diagnoserState[config];

			Code.BugIf(runState.Analysis != analysis, "runState.Analysis != analysis");
			runState.Analysis = null;

			Code.BugIf(_analysis != analysis, "_analysis != analysis");
			_analysis = null;
		}
		#endregion

		#region Core logic
		private void StartTraceSession(DiagnoserActionParameters parameters)
		{
			var metricProviders = CompetitionCore.RunState[parameters.Config].Config
				.GetMetrics()
				.Select(m => m.ValuesProvider)
				.OfType<IEtwMetricValueProvider>()
				.Distinct()
				.ToArray();

			if (metricProviders.Length == 0)
			{
				_analysis = null;
				return;
			}

			var analysis = CreateAnalysis(parameters.BenchmarkCase, parameters.Config, metricProviders);

			EtwHelpers.WorkaroundEnsureNativeDlls();

			bool allOk = false;
			try
			{
				BuildTraceSession(analysis);
				allOk = true;
			}
			finally
			{
				if (!allOk)
				{
					CompleteTraceSession(analysis);
					DisposeAnalysis(analysis);
				}
			}
		}

		private static void BuildTraceSession(EtwDiagnoserAnalysis analysis)
		{
			var traceSession = new TraceEventSession(analysis.RunGuid.ToString(), analysis.TraceFile.Path)
			{
				StopOnDispose = true
			};
			analysis.TraceSession = traceSession;

			var metricsByProvider = analysis.MetricProviders.ToLookup(p => p.ProviderGuid);

			// Kernel etw provider must be the first one.
			if (metricsByProvider.Contains(KernelTraceEventParser.ProviderGuid))
			{
				var metricValueProviders = metricsByProvider[KernelTraceEventParser.ProviderGuid];
				var flags = metricValueProviders
					.Aggregate(0UL, (value, p) => value | p.ProviderKeywords);

				try
				{
					traceSession.EnableKernelProvider((KernelTraceEventParser.Keywords)flags);
				}
				catch (UnauthorizedAccessException)
				{
					var kernelMetrics = analysis.Config
						.GetMetrics()
						.Where(m => m.ValuesProvider is IEtwMetricValueProvider p && p.IsKernelMetric);

					analysis.WriteSetupErrorMessage(
						analysis.BenchmarkCase.Descriptor,
						$"The config contains kernel metric(s) {kernelMetrics.Select(m => m.DisplayName).Join(", ")} and therefore requires elevated run.",
						"Run the competition with elevated permissions (as administrator).");

					throw;
				}
			}

			traceSession.EnableProvider(DiagnoserEventSource.SourceGuid);

			// Handle all other providers
			foreach (var metricsGroup in metricsByProvider)
			{
				// Already registered
				if (metricsGroup.Key == KernelTraceEventParser.ProviderGuid)
					continue;
				if (metricsGroup.Key == DiagnoserEventSource.SourceGuid)
					continue;

				var eventLevel = TraceEventLevel.Always;
				var keywords = 0UL;
				foreach (var metricValueProvider in metricsGroup)
				{
					if (eventLevel < metricValueProvider.EventLevel)
						eventLevel = metricValueProvider.EventLevel;
					keywords |= metricValueProvider.ProviderKeywords;
				}
				traceSession.EnableProvider(metricsGroup.Key, eventLevel, keywords);
			}
		}

		private static void ProcessCapturedEvents(EtwDiagnoserAnalysis analysis)
		{
			var events = CollectDiagnoserEvents(analysis);
			var allProcesses = events.Select(e => e.ProcessId).ToHashSet();

			var timeRange = events.ToCompositeRange(
				e => e.Started ?? TimeSpan.MinValue,
				e => e.Stopped ?? TimeSpan.MaxValue);

			// ReSharper disable once ConvertToLocalFunction
			Func<TraceEvent, bool> timeAndProcessFilter = e =>
			{
				if (!allProcesses.Contains(e.ProcessID))
					return false;

				var t = TimeSpan.FromMilliseconds(e.TimeStampRelativeMSec);
				return timeRange.Intersect(t, t).SubRanges.Any(r => r.Key.ProcessId == e.ProcessID);
			};

			using (var eventSource = new ETWTraceEventSource(analysis.TraceFile.Path))
			{
				if (eventSource.EventsLost > 0)
				{
					analysis.WriteWarningMessage(
						analysis.BenchmarkCase.Descriptor,
						$"The analysis session contains {eventSource.EventsLost} lost event(s). Metric results may be inaccurate.",
						"Consider to collect less events or to place the benchmark working directory on drive with least load.");
				}

				var allHandlers = new List<IDisposable>();
				foreach (var metricProvider in analysis.MetricProviders)
				{
					// Already processed
					if (metricProvider.ProviderGuid == DiagnoserEventSource.SourceGuid)
						continue;

					var handler = metricProvider.Subscribe(eventSource, analysis.BenchmarkCase, analysis.Config, timeAndProcessFilter);
					allHandlers.Add(handler);
				}

				eventSource.Process();
				allHandlers.DisposeAll();
			}
		}

		private static DiagnoserTraceScopeEvent[] CollectDiagnoserEvents(EtwDiagnoserAnalysis analysis)
		{
			// ReSharper disable once ConvertToLocalFunction
			Func<TraceEvent, bool> currentRunFilter =
				e => (Guid)e.PayloadByName(DiagnoserEventSource.RunIdPayload) == analysis.RunGuid;

			var provider = new DiagnoserTimesProvider();
			using (var eventSource = new ETWTraceEventSource(analysis.TraceFile.Path))
			using (provider.Subscribe(eventSource, analysis.BenchmarkCase, analysis.Config, currentRunFilter))
			{
				eventSource.Process();
			}
			return provider.GetEvents(analysis.BenchmarkCase, analysis.Config).ToArray();
		}
		#endregion

		/// <summary>Called before jitting, warmup</summary>
		/// <param name="parameters">The diagnoser action parameters</param>
		private void BeforeAnythingElse(DiagnoserActionParameters parameters)
		{
			StartTraceSession(parameters);
		}

		/// <summary>Called after globalSetup, warmup and pilot but before the main run</summary>
		/// <param name="parameters">The diagnoser action parameters</param>
		private void BeforeMainRun(DiagnoserActionParameters parameters)
		{
			var analysis = _diagnoserState[parameters.Config].Analysis;
			if (analysis == null) return;

			analysis.IterationGuid = Guid.NewGuid();
			// Ensure delay before analysis start
			Thread.Sleep(100);
			DiagnoserEventSource.Instance.TraceStarted(analysis.RunGuid, analysis.IterationGuid);
		}

		/// <summary>Called after after main run, but before global Cleanup</summary>
		/// <param name="parameters">The diagnoser action parameters</param>
		private void AfterMainRun(DiagnoserActionParameters parameters)
		{
			var analysis = _diagnoserState[parameters.Config].Analysis;
			if (analysis == null) return;

			DiagnoserEventSource.Instance.TraceStopped(analysis.RunGuid, analysis.IterationGuid);
			// Ensure delay after analysis stop
			Thread.Sleep(100);
			CompleteTraceSession(analysis);
		}

		/// <summary>Called after after all (the last thing the benchmarking engine does is to fire this signal)</summary>
		/// <param name="parameters">The diagnoser action parameters</param>
		private void AfterAll(DiagnoserActionParameters parameters)
		{
			// Nothing to do here.
		}

		/// <summary>Call used to run some code independent to the benchmarked process</summary>
		/// <param name="parameters">The diagnoser action parameters</param>
		private void SeparateLogic(DiagnoserActionParameters parameters)
		{
			// Nothing to do here.
		}

		private void ProcessResultsCore()
		{
			var analysis = _analysis;
			if (analysis == null) return;

			try
			{
				ProcessCapturedEvents(analysis);
			}
			finally
			{
				DisposeAnalysis(analysis);
			}
		}

		#region Implementation of IDiagnoser
		/// <summary>Processes the results.</summary>
		/// <param name="results">The results.</param>
		public IEnumerable<Metric> ProcessResults(DiagnoserResults results)
		{
			ProcessResultsCore();
			throw new NotImplementedException();
		}

		/// <summary>Gets the column provider.</summary>
		/// <returns>The column provider.</returns>
		public IColumnProvider GetColumnProvider() => new CompositeColumnProvider();


		/// <summary>Displays the results.</summary>
		/// <param name="logger">The logger.</param>
		public void DisplayResults(ILogger logger)
		{
			// Nothing to do here.
		}

		/// <summary>Validates the specified validation parameters.</summary>
		/// <param name="validationParameters">The validation parameters.</param>
		/// <returns></returns>
		public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters)
			=> Array.Empty<ValidationError>();

		/// <summary>Gets diagnoser run mode.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>The diagnoser run mode.</returns>
		public RunMode GetRunMode(BenchmarkCase benchmark) => RunMode.ExtraRun;

		/// <summary>Handles the specified signal.</summary>
		/// <param name="signal">The signal.</param>
		/// <param name="parameters">The parameters.</param>
		/// <exception cref="NotImplementedException"></exception>
		public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
		{
			// TODO: migrate: cases
			switch (signal)
			{
				case HostSignal.BeforeAnythingElse:
					BeforeAnythingElse(parameters);
					break;
				case HostSignal.BeforeActualRun:
					BeforeMainRun(parameters);
					break;
				case HostSignal.AfterActualRun:
					AfterMainRun(parameters);
					break;
				case HostSignal.AfterAll:
					AfterAll(parameters);
					break;
				case HostSignal.SeparateLogic:
					SeparateLogic(parameters);
					break;
				case HostSignal.BeforeProcessStart:
					break;
				case HostSignal.AfterProcessExit:
					break;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(signal), signal);
			}
		}

		/// <summary>Gets the diagnoser ids.</summary>
		/// <value>The diagnoser ids.</value>
		public IEnumerable<string> Ids => new[] { nameof(EtwDiagnoser) };

		/// <summary>Gets the exporters.</summary>
		/// <value>The exporters.</value>
		public IEnumerable<IExporter> Exporters => Array<IExporter>.Empty;

		/// <summary>Gets the analysers.</summary>
		/// <value>The analysers.</value>
		public IEnumerable<IAnalyser> Analysers => Array<IAnalyser>.Empty;

		#endregion
	}
}