using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.FilteringLogger;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helpers for performance testing infrastructure.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class CompetitionCore
	{
		#region Run limit values
		/// <summary>The maximum run limit.</summary>
		internal const int MaxRunLimit = 100;

		/// <summary>Total time limit for competition run. Set to two hours for now, may change in future.</summary>
		internal static readonly TimeSpan TotalWaitTimeout = TimeSpan.FromHours(2);

		/// <summary>Total time limit for spin wait time. Set to one minute for now, may change in future.</summary>
		internal static readonly TimeSpan SpinWaitRunTimeout = TimeSpan.FromMinutes(1);
		#endregion

		/// <summary>Run state slot.</summary>
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		#region Run logic
		/// <summary>Runs the benchmark for specified benchmark type.</summary>
		/// <param name="benchmarkType">Type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>Competition state for the run.</returns>
		[NotNull]
		internal static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(competitionConfig, nameof(competitionConfig));
			var runStateSlots = competitionConfig.GetValidators().OfType<RunStateSlots>();
			if (runStateSlots.Count() != 1)
			{
				throw CodeExceptions.Argument(
					nameof(competitionConfig),
					$"The competition config should include single instance of {nameof(RunStateSlots)} validator.");
			}

			var competitionState = RunState[competitionConfig];

			try
			{
				competitionState.FirstTimeInit(benchmarkType, competitionConfig);
				var logger = competitionState.Logger;

				using (BeginLogImportant(competitionConfig))
				{
					logger.WriteSeparatorLine(benchmarkType.Name, true);
					logger.WriteLineHelp($"{LogInfoPrefix} {benchmarkType.GetShortAssemblyQualifiedName()}");
				}

				using (var mutex = new Mutex(false, $"Global\\{typeof(CompetitionCore).FullName}"))
				{
					var lockTaken = false;
					try
					{
						var timeout = competitionState.Options.RunOptions.Concurrent == ConcurrentRunBehavior.Lock
							? TotalWaitTimeout
							: TimeSpan.Zero;

						lockTaken = SpinWait(mutex, timeout, SpinWaitRunTimeout, competitionState);
						if (CheckPreconditions(benchmarkType, lockTaken, competitionState))
						{
							RunCore(benchmarkType, competitionState);
						}
					}
					finally
					{
						if (lockTaken)
							mutex.ReleaseMutex();
					}
				}
			}
			catch (TargetInvocationException ex)
			{
				competitionState.WriteExceptionMessage(
					MessageSource.Runner, MessageSeverity.ExecutionError,
					$"Benchmark {benchmarkType.Name} failed.", ex.InnerException ?? ex);
			}
			catch (Exception ex)
			{
				competitionState.WriteExceptionMessage(
					MessageSource.Runner, MessageSeverity.ExecutionError,
					$"Benchmark {benchmarkType.Name} failed.", ex);
			}
			finally
			{
				LoggerHelpers.FlushLoggers(competitionConfig);
			}

			competitionState.CompetitionCompleted();

			return competitionState;
		}

		private static bool SpinWait(
			Mutex mutex,
			TimeSpan waitTimeout, TimeSpan spinWaitTimeout,
			CompetitionState competitionState)
		{
			if (spinWaitTimeout > waitTimeout)
				spinWaitTimeout = waitTimeout;

			bool result;

			var totalSpinTime = TimeSpan.Zero;
			var waitStopwatch = Stopwatch.StartNew();
			do
			{
				try
				{
					result = mutex.WaitOne(spinWaitTimeout);

					if (!result)
					{
						competitionState.WriteMessage(
							MessageSource.Runner, MessageSeverity.Informational,
							$"Another perftest is running, wait timeout {totalSpinTime} of {waitTimeout}.");
					}
					else if (totalSpinTime > TimeSpan.Zero)
					{
						competitionState.WriteMessage(
							MessageSource.Runner, MessageSeverity.Informational,
							$"Another perftest completed, starting. Wait timeout {totalSpinTime} of {waitTimeout}.");
					}
				}
				catch (AbandonedMutexException ex)
				{
					// It's ok to swallow abandoned mutex exception as we have no shared resources but logger output
					// and the log is protected by FileShare.Read lock
					// see https://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
					// for more detail.
					competitionState.WriteExceptionMessage(
						MessageSource.Runner, MessageSeverity.Informational,
						$"Another perftest aborted, starting. Wait timeout {totalSpinTime} of {waitTimeout}.", ex);

					result = true;
					break;
				}

				if (result)
					break;

				totalSpinTime += spinWaitTimeout;
			}
			// double check, exit by first condition
			while (totalSpinTime < waitTimeout && waitStopwatch.Elapsed < waitTimeout);

			return result;
		}

		private static bool CheckPreconditions(
			[NotNull] Type benchmarkType,
			bool lockTaken,
			CompetitionState competitionState)
		{
			var runOptions = competitionState.Options.RunOptions;

			if (!lockTaken)
			{
				switch (runOptions.Concurrent)
				{
					case ConcurrentRunBehavior.Lock:
					case ConcurrentRunBehavior.Skip:
						competitionState.WriteMessage(
							MessageSource.Runner, MessageSeverity.Warning,
							"Competition run skipped. Competitions cannot be run in parallel, be sure to disable parallel test execution.");
						return false;
					case ConcurrentRunBehavior.Fail:
						competitionState.WriteMessage(
							MessageSource.Runner, MessageSeverity.SetupError,
							"Competition run failed. Competitions cannot be run in parallel, be sure to disable parallel test execution.");
						return false;
					default:
						throw CodeExceptions.UnexpectedArgumentValue(nameof(runOptions.Concurrent), runOptions.Concurrent);
				}
			}

			if (!runOptions.AllowDebugBuilds && benchmarkType.Assembly.IsDebugAssembly())
			{
				var assembly = benchmarkType.Assembly;
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.Warning,
					$"Competition run skipped. Assembly {assembly.GetName().Name} was build as debug.");

				return false;
			}

			if (runOptions.ContinuousIntegrationMode)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.Informational,
					"Competition is run under continuous integration service.");
			}

			return true;
		}

		private static void RunCore(Type benchmarkType, CompetitionState competitionState)
		{
			var logger = competitionState.Logger;
			var runOptions = competitionState.Options.RunOptions;

			Code.InRange(
				runOptions.MaxRunsAllowed,
				CompetitionRunMode.MaxRunsAllowedCharacteristic.FullId,
				0, MaxRunLimit);

			while (competitionState.RunsLeft > 0)
			{
				competitionState.PrepareForRun();

				var run = competitionState.RunNumber;
				var runsExpected = competitionState.RunNumber + competitionState.RunsLeft;
				var runMessage = competitionState.RunLimitExceeded
					? $"Run {run}, total runs (expected): {runsExpected} (rerun limit exceeded, last run)"
					: $"Run {run}, total runs (expected): {runsExpected}";

				using (BeginLogImportant(competitionState.Config))
				{
					logger.WriteSeparatorLine(runMessage);
				}

				// TODO: toolchainProvider to base (???).
				Func<Job, IToolchain> toolchainProvider = j => j.Infrastructure?.Toolchain ?? InProcessToolchain.Instance;

				// Running the benchmark
				var summary = BenchmarkRunnerCore.Run(
					BenchmarkConverter.TypeToBenchmarks(benchmarkType, competitionState.Config),
					competitionState.Config,
					toolchainProvider);
				competitionState.RunCompleted(summary);

				// Dump messages if analysis was not run and there is a validation analyser.
				if (summary.HasCriticalValidationErrors)
				{
					var validationAnalyser = competitionState.Config
						.GetAnalysers()
						.OfType<ValidatorMessagesAnalyser>()
						.FirstOrDefault();
					validationAnalyser?.Analyse(summary);
				}

				if (competitionState.HasCriticalErrorsInRun)
				{
					competitionState.Logger.WriteVerboseHint("Breaking competition execution. High severity error occured.");
					break;
				}

				if (competitionState.RunLimitExceeded)
					break;

				if (competitionState.RunsLeft > 0)
				{
					competitionState.Logger.WriteVerboseHint($"Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			if (competitionState.RunLimitExceeded && competitionState.RunsLeft > 0)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.TestError,
					$"The benchmark run limit ({runOptions.MaxRunsAllowed} runs(s)) exceeded, check log for details.");
			}
			else if (competitionState.RunNumber > 1)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunNumber} time(s), check log for details.");
			}
		}
		#endregion
	}
}