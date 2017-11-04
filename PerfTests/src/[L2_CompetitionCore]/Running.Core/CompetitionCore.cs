using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.Reflection;

using JetBrains.Annotations;

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
		private static readonly TimeSpan _totalWaitTimeout = TimeSpan.FromHours(2);

		/// <summary>Total time limit for spin wait time. Set to one minute for now, may change in future.</summary>
		private static readonly TimeSpan _spinWaitRunTimeout = TimeSpan.FromMinutes(1);
		#endregion

		/// <summary>Run state slot.</summary>
		[NotNull]
		public static readonly RunStateKey<CompetitionState> RunState = new RunStateKey<CompetitionState>(
			_ =>
				throw
					CodeExceptions.InvalidOperation(
						$"The run state should be set by {nameof(CompetitionCore)}.{nameof(InitCompetitionState)}()."));

		#region Run logic
		/// <summary>Runs the benchmark for specified benchmark type.</summary>
		/// <param name="benchmarkType">Type of the benchmark.</param>
		/// <param name="competitionConfig">The competition configuration.</param>
		/// <returns>Competition state for the run.</returns>
		[NotNull]
		internal static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			var competitionState = InitCompetitionState(benchmarkType, competitionConfig);
			var runLogger = new MessageLogger(competitionState.Config, MessageSource.Runner);

			try
			{
				LogCompetitionHeader(competitionState);

				using (var mutex = new Mutex(false, $"Global\\{typeof(CompetitionCore).FullName}"))
				{
					var lockTaken = false;
					try
					{
						var timeout = competitionState.Options.RunOptions.Concurrent == ConcurrentRunBehavior.Lock
							? _totalWaitTimeout
							: TimeSpan.Zero;

						lockTaken = SpinWait(mutex, timeout, _spinWaitRunTimeout, runLogger);
						if (CheckPreconditions(competitionState, lockTaken, runLogger))
						{
							RunCore(competitionState, runLogger);
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
				runLogger.WriteExceptionMessage(
					MessageSeverity.ExecutionError,
					$"Benchmark {competitionState.BenchmarkType.Name} failed.", ex.InnerException ?? ex);
			}
			catch (Exception ex)
			{
				runLogger.WriteExceptionMessage(
					MessageSeverity.ExecutionError,
					$"Benchmark {competitionState.BenchmarkType.Name} failed.", ex);
			}
			finally
			{
				LoggerHelpers.FlushLoggers(competitionState.Config);
				competitionState.CompetitionCompleted();
			}

			return competitionState;
		}

		private static CompetitionState InitCompetitionState(
			Type benchmarkType,
			ICompetitionConfig competitionConfig)
		{
			var competitionState = new CompetitionState(benchmarkType, competitionConfig);

			var runStateSlots = competitionState.Config.GetValidators().OfType<RunStateSlots>().ToArray();
			if (runStateSlots.Length != 1)
			{
				throw CodeExceptions.Argument(
					nameof(competitionState),
					$"The competition state config should include single instance of {nameof(RunStateSlots)} validator.");
			}
			runStateSlots[0].InitSlot(RunState, competitionState);

			return competitionState;
		}

		private static void LogCompetitionHeader(CompetitionState competitionState)
		{
			var logger = competitionState.Logger;
			var benchmarkType = competitionState.BenchmarkType;

			using (LoggerHelpers.BeginImportantLogScope(competitionState.Config))
			{
				logger.WriteSeparatorLine(benchmarkType.Name, true);
				logger.WriteHelpHintLine(benchmarkType.GetShortAssemblyQualifiedName());
			}
		}

		private static void LogCompetitionRunHeader(CompetitionState competitionState)
		{
			var logger = competitionState.Logger;

			using (LoggerHelpers.BeginImportantLogScope(competitionState.Config))
			{
				var run = competitionState.RunNumber;
				var runsExpected = competitionState.RunNumber + competitionState.RunsLeft;
				var runMessage = competitionState.RunLimitExceeded
					? $"Run {run}, total runs (expected): {runsExpected} (rerun limit exceeded, last run)"
					: $"Run {run}, total runs (expected): {runsExpected}";
				logger.WriteSeparatorLine(runMessage);
			}
		}

		private static bool SpinWait(
			Mutex mutex,
			TimeSpan waitTimeout, TimeSpan spinWaitTimeout,
			IMessageLogger messageLogger)
		{
			Code.InRange(waitTimeout, nameof(spinWaitTimeout), TimeSpan.Zero, _totalWaitTimeout);
			Code.InRange(spinWaitTimeout, nameof(spinWaitTimeout), TimeSpan.Zero, _totalWaitTimeout);

			if (spinWaitTimeout > waitTimeout)
				spinWaitTimeout = waitTimeout;

			bool lockTaken = false;
			var totalSpinTime = TimeSpan.Zero;
			while (!lockTaken && totalSpinTime < waitTimeout)
			{
				try
				{
					lockTaken = mutex.WaitOne(spinWaitTimeout);

					if (!lockTaken)
					{
						messageLogger.WriteInfoMessage($"Another perftest is running, wait timeout {totalSpinTime} of {waitTimeout}.");
					}
					else if (totalSpinTime > TimeSpan.Zero)
					{
						messageLogger.WriteInfoMessage(
							$"Another perftest completed, starting. Wait timeout {totalSpinTime} of {waitTimeout}.");
					}
				}
				catch (AbandonedMutexException ex)
				{
					// It's ok to swallow abandoned mutex exception as we have no shared resources but logger output
					// and the log is protected by FileShare.Read lock
					// see https://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
					// for more detail.
					messageLogger.WriteExceptionMessage(
						MessageSeverity.Informational,
						$"Another perftest aborted, starting. Wait timeout {totalSpinTime} of {waitTimeout}.", ex);

					lockTaken = true;
				}

				totalSpinTime += spinWaitTimeout;
			}

			return lockTaken;
		}

		private static bool CheckPreconditions(
			CompetitionState competitionState, bool lockTaken,
			IMessageLogger messageLogger)
		{
			var runOptions = competitionState.Options.RunOptions;

			if (!lockTaken)
			{
				switch (runOptions.Concurrent)
				{
					case ConcurrentRunBehavior.Lock:
					case ConcurrentRunBehavior.Skip:
						messageLogger.WriteWarningMessage(
							"Competition run skipped. Competitions cannot be run in parallel, be sure to disable parallel test execution.");
						return false;
					case ConcurrentRunBehavior.Fail:
						messageLogger.WriteSetupErrorMessage(
							"Competition run failed. Competitions cannot be run in parallel, be sure to disable parallel test execution.");
						return false;
					default:
						throw CodeExceptions.UnexpectedArgumentValue(nameof(runOptions.Concurrent), runOptions.Concurrent);
				}
			}

			var benchmarkAssembly = competitionState.BenchmarkType.Assembly;
			if (!runOptions.AllowDebugBuilds && benchmarkAssembly.IsDebugAssembly())
			{
				messageLogger.WriteWarningMessage(
					$"Competition run skipped. Assembly {benchmarkAssembly.GetName().Name} was build as debug.");

				return false;
			}

			if (runOptions.ContinuousIntegrationMode)
			{
				messageLogger.WriteInfoMessage(
					"Competition is run under continuous integration service.");
			}

			return true;
		}

		// TODO: HACK: remove as the method will be public
		// ReSharper disable once AssignNullToNotNullAttribute
		private static readonly Func<Type, MethodInfo[], ReadOnlyConfig, BenchmarkRunInfo> _typeToBenchmarkHack =
			(Func<Type, MethodInfo[], ReadOnlyConfig, BenchmarkRunInfo>)Delegate.CreateDelegate(
				typeof(Func<Type, MethodInfo[], ReadOnlyConfig, BenchmarkRunInfo>),
				typeof(BenchmarkConverter).GetMethod("MethodsToBenchmarksWithFullConfig", BindingFlags.Static | BindingFlags.NonPublic));

		private static void RunCore(CompetitionState competitionState, IMessageLogger messageLogger)
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

				LogCompetitionRunHeader(competitionState);

				// Running the benchmark
				var benchmarkType = competitionState.BenchmarkType;
				var runInfo = _typeToBenchmarkHack(
					benchmarkType, benchmarkType.GetMethods(), new ReadOnlyConfig(competitionState.Config));
				var summary = BenchmarkRunnerCore.Run(
					runInfo,
					j => j.Infrastructure?.Toolchain ?? InProcessToolchain.Instance);
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
					logger.WriteHintLine("Breaking competition execution. High severity error occured.");
					break;
				}

				if (competitionState.RunLimitExceeded)
					break;

				if (competitionState.RunsLeft > 0)
				{
					logger.WriteHintLine($"Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			if (competitionState.RunLimitExceeded && competitionState.RunsLeft > 0)
			{
				messageLogger.WriteTestErrorMessage(
					$"The benchmark run limit ({runOptions.MaxRunsAllowed} runs(s)) exceeded, check log for details.");
			}
			else if (competitionState.RunNumber > 1)
			{
				messageLogger.WriteWarningMessage(
					$"The benchmark was run {competitionState.RunNumber} time(s), check log for details.");
			}
		}
		#endregion
	}
}