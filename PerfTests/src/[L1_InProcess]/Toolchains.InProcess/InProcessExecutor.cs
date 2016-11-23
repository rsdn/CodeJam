using System;
using System.Collections.Generic;
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

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>
	/// Implementation of <see cref="IExecutor"/> for in-process benchmarks.
	/// </summary>
	[PublicAPI]
	public class InProcessExecutor : IExecutor
	{
		private static readonly TimeSpan _debugTimeout = TimeSpan.FromDays(1);

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

			var runnableBenchmark = new RunnableBenchmark();
			var factory = benchmark.Job.ResolveValue(
				InfrastructureMode.EngineFactoryCharacteristic,
				InfrastructureResolver.Instance);
			var factoryType = factory.GetType();

			var outputStream = new MemoryStream(80 * 1000);
			var affinity = benchmark.Job.ResolveValueAsNullable(EnvMode.AffinityCharacteristic);
			var runThread = new Thread(
				() =>
				{
					using (BenchmarkHelpers.SetupHighestPriorityScope(affinity, logger))
					{
						RunCore(runnableBenchmark, benchmark, factoryType, outputStream, false, logger);
					}
				});

			if (benchmark.Target.Method.GetCustomAttributes(false).OfType<STAThreadAttribute>().Any())
			{
				runThread.SetApartmentState(ApartmentState.STA);
			}
			runThread.IsBackground = true;

			var timeout = HostEnvironmentInfo.GetCurrent().HasAttachedDebugger ?
				_debugTimeout : ExecutionTimeout;

			runThread.Start();
			if (!runThread.Join(timeout))
				throw new InvalidOperationException(
					$"Benchmark {benchmark.DisplayInfo} takes to long to run. " +
						"Prefer to use out-of-process toolchains for long-running benchmarks.");

			CodeJam.Code.BugIf(runThread.IsAlive, "The runThread.Join() did not work as expected.");

			return ParseExecutionResult(outputStream, logger);
		}

		private void RunCore(
			RunnableBenchmark runnableBenchmark,
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
				// ReSharper disable ConvertIfStatementToSwitchStatement
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
				// ReSharper restore ConvertIfStatementToSwitchStatement
			}

			return new ExecuteResult(true, 0, lines.ToArray(), linesWithOutput.ToArray());
		}
	}
}