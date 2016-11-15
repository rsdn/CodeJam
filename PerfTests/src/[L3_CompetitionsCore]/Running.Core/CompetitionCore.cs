using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Loggers.HostLogger;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helpers for performance testing infrastructure.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class CompetitionCore
	{
		#region Extension methods
		/// <summary>The message severity is setup error or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is setup error or higher.</returns>
		public static bool IsCriticalError(this MessageSeverity severity) => severity >= MessageSeverity.SetupError;

		/// <summary>The message severity is test error or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is test error or higher.</returns>
		public static bool IsTestErrorOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.TestError;

		/// <summary>The message severity is warning or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is warning or higher.</returns>
		public static bool IsWarningOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.Warning;


		/// <summary>Returns appropriate conclusion kind.</summary>
		/// <param name="severity">The severity.</param>
		/// <returns>Conclusion kind for severity.</returns>
		public static ConclusionKind ToConclusionKind(this MessageSeverity severity)
		{
			switch (severity)
			{
				case MessageSeverity.Verbose:
				case MessageSeverity.Informational:
					return ConclusionKind.Hint;
				case MessageSeverity.Warning:
				case MessageSeverity.TestError:
				case MessageSeverity.SetupError:
				case MessageSeverity.ExecutionError:
					return ConclusionKind.Warning;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(severity), severity);
			}
		}

		/// <summary>Log format for the message.</summary>
		/// <returns>Log format for the message.</returns>
		public static string ToLogString(this IMessage message) => string.Format(
			HostEnvironmentInfo.MainCultureInfo,
			"#{0}.{1,-2} {2:00.000}s, {3,-23} {4}",
			message.RunNumber,
			message.RunMessageNumber,
			message.Elapsed.TotalSeconds,
			message.MessageSeverity + "@" + message.MessageSource + ":",
			message.MessageText);
		#endregion

		#region API to use during the run
		/// <summary>Run state slot.</summary>
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		/// <summary>Reports analyser conclusion.</summary>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="analyser">The analyser the message belongs to.</param>
		/// <param name="conclusions">The list the conclusion will be added to.</param>
		/// <param name="severity">Severity of the message.</param>
		/// <param name="message">The message.</param>
		/// <param name="report">The report the message belongs to.</param>
		public static void AddAnalyserConclusion(
			[NotNull] this CompetitionState competitionState,
			[NotNull] IAnalyser analyser,
			[NotNull] List<Conclusion> conclusions,
			MessageSeverity severity,
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			Code.NotNull(competitionState, nameof(competitionState));
			Code.NotNull(analyser, nameof(analyser));
			Code.NotNull(conclusions, nameof(conclusions));
			Code.NotNullNorEmpty(message, nameof(message));

			competitionState.WriteMessage(MessageSource.Analyser, severity, message);
			var conclusion = severity.ToConclusionKind() == ConclusionKind.Hint
				? Conclusion.CreateHint(analyser.Id, message, report)
				: Conclusion.CreateWarning(analyser.Id, message, report);
			conclusions.Add(conclusion);
		}

		/// <summary>Writes the exception message.</summary>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="messageSource">Source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">The explanation for the exception.</param>
		/// <param name="ex">The exception to write.</param>
		public static void WriteExceptionMessage(
			[NotNull] this CompetitionState competitionState,
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string message,
			[NotNull] Exception ex)
		{
			Code.NotNull(competitionState, nameof(competitionState));
			Code.NotNullNorEmpty(message, nameof(message));
			Code.NotNull(ex, nameof(ex));

			competitionState.WriteMessage(
				messageSource, messageSeverity,
				$"{message} Exception: {ex.Message}.");
		}

		/// <summary>Helper method to dump the content of the message into logger.</summary>
		/// <param name="logger">The logger the message will be dumped to.</param>
		/// <param name="message">The message to log.</param>
		internal static void LogMessage([NotNull] this ILogger logger, [NotNull] IMessage message)
		{
			Code.NotNull(logger, nameof(logger));
			Code.NotNull(message, nameof(message));

			if (message.MessageSeverity.IsCriticalError())
			{
				logger.WriteLineError($"{LogImportantInfoPrefix} {message.ToLogString()}");
			}
			else if (message.MessageSeverity.IsWarningOrHigher())
			{
				logger.WriteLineInfo($"{LogImportantInfoPrefix} {message.ToLogString()}");
			}
			else
			{
				logger.WriteLineInfo($"{LogInfoPrefix} {message.ToLogString()}");
			}
		}
		#endregion

		#region Run logic
		/// <summary>Runs the benchmark for specified benchmark type.</summary>
		/// <param name="benchmarkType">Type of the benchmark.</param>
		/// <param name="benchmarkConfig">Config for the benchmark.</param>
		/// <param name="competitionOptions">Competition options.</param>
		/// <returns>Competition state for the run.</returns>
		[NotNull]
		internal static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] IConfig benchmarkConfig,
			CompetitionOptions competitionOptions)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(benchmarkConfig, nameof(benchmarkConfig));

			var runStateSlots = benchmarkConfig.GetValidators().OfType<RunStateSlots>();
			if (runStateSlots.Count() != 1)
			{
				throw CodeExceptions.Argument(
					nameof(benchmarkConfig),
					$"The competition config should include single instance of {nameof(RunStateSlots)} validator.");
			}

			var competitionState = RunState[benchmarkConfig];

			try
			{
				competitionState.FirstTimeInit(benchmarkConfig, competitionOptions);
				var logger = competitionState.Logger;

				using (BeginLogImportant(benchmarkConfig))
				{
					logger.WriteLine();
					logger.WriteSeparatorLine(benchmarkType.Name, true);
					logger.WriteLineHelp($"{LogInfoPrefix} {benchmarkType.GetShortAssemblyQualifiedName()}");
				}

				using (var mutex = new Mutex(false, $"Global\\{typeof(CompetitionCore).FullName}"))
				{
					bool lockTaken = false;
					try
					{
						var runOptions = competitionState.Options.RunOptions;
						var timeout = runOptions.Concurrent == ConcurrentRunBehavior.Lock ? CompetitionRunMode.TotalRunTimeout : TimeSpan.Zero;
						lockTaken = mutex.WaitOne(timeout);
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
				FlushLoggers(benchmarkConfig);
			}

			competitionState.CompetitionCompleted();

			return competitionState;
		}

		// TODO: as a part of CompetitionRunnerBase?
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

			return true;
		}

		private static void RunCore(Type benchmarkType, CompetitionState competitionState)
		{
			var logger = competitionState.Logger;
			var runOptions = competitionState.Options.RunOptions;

			Code.InRange(runOptions.MaxRunsAllowed, nameof(runOptions.MaxRunsAllowed), 0, CompetitionRunMode.MaxRunLimit);

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

				// TODO: toolchainProvider to base.
				Func<Job, IToolchain> toolchainProvider = j => j.Infrastructure?.Toolchain ?? InProcessToolchain.Instance;

				// Running the benchmark
				var summary = BenchmarkRunnerCore.Run(
					BenchmarkConverter.TypeToBenchmarks(benchmarkType, competitionState.Config),
					competitionState.Config,
					toolchainProvider);
				competitionState.RunCompleted(summary);

				// TODO: dump them before analyser run?
				WriteValidationMessages(competitionState);

				if (competitionState.HasCriticalErrorsInRun)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix} Breaking competition execution. High severity error occured.");
					break;
				}

				if (competitionState.RunLimitExceeded)
					break;

				if (competitionState.RunsLeft > 0)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix} Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			// TODO: notify analysers for last run? // Will need to define custom interface, of course.
			// TODO: move to somewhere else?
			if (competitionState.RunLimitExceeded && competitionState.RunsLeft > 0)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.TestError,
					$"The benchmark run limit ({runOptions.MaxRunsAllowed} runs(s)) exceeded (read log for details). Try to loose competition limits.");
			}
			else if (competitionState.RunNumber > 1)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunNumber} time(s) (read log for details). Try to loose competition limits.");
			}
		}

		private static void WriteValidationMessages(CompetitionState competitionState)
		{
			if (competitionState.LastRunSummary == null)
				return;

			foreach (var validationError in competitionState.LastRunSummary.ValidationErrors)
			{
				var severity = validationError.IsCritical ? MessageSeverity.SetupError : MessageSeverity.Warning;
				var message = validationError.Benchmark == null
					? validationError.Message
					: $"Benchmark {validationError.Benchmark.DisplayInfo}:{Environment.NewLine}\t{validationError.Message}";

				competitionState.WriteMessage(MessageSource.Validator, severity, message);
			}
		}
		#endregion
	}
}