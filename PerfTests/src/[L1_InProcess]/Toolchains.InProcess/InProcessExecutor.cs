using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

using CodeJam;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>
	/// Implementation of <see cref="IExecutor"/> for in-process benchmarks.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_while")]
	public class InProcessExecutor : IExecutor
	{
		/// <summary>Initializes a new instance of the <see cref="InProcessExecutor"/> class.</summary>
		/// <param name="timeout">Timeout for the run.</param>
		/// <param name="logOutput"><c>true</c> if the output should be logged.</param>
		public InProcessExecutor(TimeSpan timeout, bool logOutput)
		{
			ExecutionTimeout = timeout;
			LogOutput = logOutput;
		}

		/// <summary>Timeout for the run.</summary>
		/// <value>The timeout for the run.</value>
		public TimeSpan ExecutionTimeout { get; }

		/// <summary>Gets a value indicating whether the output should be logged.</summary>
		/// <value><c>true</c> if the output should be logged; otherwise, <c>false</c>.</value>
		public bool LogOutput { get; }

		// TODO: replace outputStream with something better?
		// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/177
		/// <summary>Executes the specified benchmark.</summary>
		/// <param name="buildResult">The build result.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="resolver">The resolver.</param>
		/// <param name="diagnoser">The diagnoser.</param>
		/// <returns>Execution result.</returns>
		public ExecuteResult Execute(
			BuildResult buildResult, Benchmark benchmark, ILogger logger, IResolver resolver, IDiagnoser diagnoser = null)
		{
			// TODO: with diagnoser.
			if (diagnoser != null)
				throw new NotSupportedException("Inline toolchain does not support diagnosers for now.");

			var runnableBenchmark = RunnableBenchmarkFactory.Create(benchmark);
			var factory = benchmark.Job.ResolveValue(
				InfrastructureMode.EngineFactoryCharacteristic,
				InfrastructureResolver.Instance);
			var factoryType = factory.GetType();

			var outputStream = new MemoryStream(80 * 1000);
			var runThread = new Thread(
				() => RunCore(runnableBenchmark, benchmark, factoryType, outputStream, false, logger))
			{
				IsBackground = true,
				Priority = ThreadPriority.Highest
			};

			if (benchmark.Target.Method.GetCustomAttributes(false).OfType<STAThreadAttribute>().Any())
			{
				runThread.SetApartmentState(ApartmentState.STA);
			}

			using (ProcessPriorityScope(
				ProcessPriorityClass.RealTime,
				benchmark.Job.ResolveValueAsNullable(EnvMode.AffinityCharacteristic),
				logger))
			{
				runThread.Start();
				if (!runThread.Join(ExecutionTimeout))
					throw new InvalidOperationException(
						"Benchmark takes to long to run. " +
							"Prefer to use out-of-process toolchains for long-running benchmarks.");
			}

			int retryCount = 0;
			while (runThread.IsAlive && retryCount++ < 100)
			{
				Thread.Sleep(10);
			}

			return ParseExecutionResult(outputStream, logger);
		}

		private void RunCore(
			IRunnableBenchmark runnableBenchmark,
			Benchmark benchmark,
			Type factoryType,
			Stream outputStream,
			bool isDiagnoserAttached,
			ILogger logger)
		{
			var outputWriter = new StreamWriter(outputStream);
			try
			{
				runnableBenchmark.Init(benchmark, factoryType, outputWriter, isDiagnoserAttached);
				runnableBenchmark.Run();
			}
			catch (Exception ex)
			{
				logger.WriteLineError($"// ! {GetType().Name}, exception: {ex}");
			}
			finally
			{
				outputWriter.Flush();
			}
		}

		private ExecuteResult ParseExecutionResult(MemoryStream outputStream, ILogger logger)
		{
			outputStream.Position = 0;
			var outputReader = new StreamReader(outputStream);
			var lines = new List<string>();
			var linesWithOutput = new List<string>();
			string line;
			while ((line = outputReader.ReadLine()) != null)
			{
				if (LogOutput)
				{
					logger.WriteLine(line);
				}

				if (string.IsNullOrEmpty(line))
					continue;

				// TODO: diagnoser support.
				if (!line.StartsWith("//"))
				{
					lines.Add(line);
				}
				else if (line == Engine.Signals.BeforeAnythingElse)
				{
					//diagnoser?.BeforeAnythingElse(process, benchmark);
				}
				else if (line == Engine.Signals.AfterSetup)
				{
					//diagnoser?.AfterSetup(process, benchmark);
				}
				else if (line == Engine.Signals.BeforeCleanup)
				{
					//diagnoser?.BeforeCleanup();
				}
				else
				{
					linesWithOutput.Add(line);
				}
			}

			return new ExecuteResult(true, 0, lines.ToArray(), linesWithOutput.ToArray());
		}

		private static IDisposable ProcessPriorityScope(
			ProcessPriorityClass priority, IntPtr? affinity, ILogger logger)
		{
			var process = Process.GetCurrentProcess();
			var oldPriority = process.PriorityClass;
			var oldAffinity = process.ProcessorAffinity;

			process.SetPriority(priority, logger);
			if (affinity.HasValue)
			{
				process.SetAffinity(affinity.Value, logger);
			}

			return Disposable.Create(
				() =>
				{
					if (affinity.HasValue)
					{
						process.SetAffinity(oldAffinity, logger);
					}
					process.SetPriority(oldPriority, logger);
				});
		}
	}
}