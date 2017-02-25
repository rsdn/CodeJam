using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>Host API for in-process benchmarks.</summary>
	/// <seealso cref="IHostApi"/>
	public sealed class InProcessHostApi : IHostApi
	{
		#region Fields & .ctor
		[NotNull]
		private readonly Benchmark _benchmark;

		[CanBeNull]
		private readonly ILogger _logger;

		[CanBeNull]
		private readonly IDiagnoser _diagnoser;

		[CanBeNull]
		private readonly Process _currentProcess;

		/// <summary>Creates a new instance of <see cref="InProcessHostApi"/>.</summary>
		/// <param name="benchmark">Current benchmark.</param>
		/// <param name="logger">Optional logger for informational output.</param>
		/// <param name="diagnoser">Diagnosers, if attached.</param>
		public InProcessHostApi(
			[NotNull] Benchmark benchmark, [CanBeNull] ILogger logger, [CanBeNull] IDiagnoser diagnoser)
		{
			if (benchmark == null)
				throw new ArgumentNullException(nameof(benchmark));

			_benchmark = benchmark;
			_logger = logger;
			_diagnoser = diagnoser;
			IsDiagnoserAttached = diagnoser != null;

			if (diagnoser != null)
				_currentProcess = Process.GetCurrentProcess();
		}
		#endregion

		#region Properties
		/// <summary><c>True</c> if there are diagnosers attached.</summary>
		/// <value><c>True</c> if there are diagnosers attached.</value>
		public bool IsDiagnoserAttached { get; }

		/// <summary>Results of the run.</summary>
		/// <value>Results of the run.</value>
		public RunResults RunResults { get; private set; }
		#endregion

		/// <summary>Passes text to the host.</summary>
		/// <param name="message">Text to write.</param>
		public void Write(string message) => _logger?.Write(message);

		/// <summary>Passes new line to the host.</summary>
		public void WriteLine() => _logger?.WriteLine();

		/// <summary>Passes text (new line appended) to the host.</summary>
		/// <param name="message">Text to write.</param>
		public void WriteLine(string message) => _logger?.WriteLine(message);

		/// <summary>Writes formatted to the host.</summary>
		/// <param name="messageFormat">Format string.</param>
		/// <param name="args">Format args.</param>
		public void WriteLine(string messageFormat, params object[] args)
			=> _logger?.WriteLine(string.Format(messageFormat, args));

		/// <summary>Sends BeforeAnythingElse notification.</summary>
		public void BeforeAnythingElse()
		{
			_diagnoser?.BeforeAnythingElse(_currentProcess, _benchmark);
			WriteLine(Engine.Signals.BeforeAnythingElse);
		}

		/// <summary>Sends AfterSetup notification.</summary>
		public void AfterSetup()
		{
			WriteLine(Engine.Signals.AfterSetup);
			_diagnoser?.AfterSetup(_currentProcess, _benchmark);
		}

		/// <summary>Sends BeforeCleanup notification.</summary>
		public void BeforeCleanup()
		{
			_diagnoser?.BeforeCleanup();
			WriteLine(Engine.Signals.BeforeCleanup);
		}

		/// <summary>Sends AfterAnythingElse notification.</summary>
		public void AfterAnythingElse() => WriteLine("AfterAnythingElse");

		/// <summary>Submits run results to the host.</summary>
		/// <param name="runResults">The run results.</param>
		public void Print(RunResults runResults)
		{
			RunResults = runResults;

			if (_logger != null)
			{
				using (var w = new StringWriter())
				{
					foreach (Measurement measurement in runResults.GetMeasurements())
						_logger.WriteLine(measurement.ToOutputLine());

					var ops = (long)typeof(RunResults)
						.GetField("totalOperationsCount", BindingFlags.Instance | BindingFlags.NonPublic)
						.GetValue(runResults);

					var s = runResults.GCStats.WithTotalOperations(ops);
					var totalOpsOutput = string.Format(
						"{0} {1} {2} {3} {4} {5}", (object)"GC: ",
						s.Gen0Collections, s.Gen1Collections, s.Gen2Collections,
						s.AllocatedBytes, s.TotalOperations);
					_logger.WriteLine(totalOpsOutput);
					_logger.WriteLine();

					_logger.Write(w.GetStringBuilder().ToString());
				}
			}
		}
	}
}