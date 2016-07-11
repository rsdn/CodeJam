using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

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

		/// <summary>Log format for the message.</summary>
		/// <returns>Log format for the message.</returns>
		public static string ToLogString(this IMessage message)
		{
			var m = message;
			return $"#{m.RunNumber}.{m.RunMessageNumber,-2} {m.Elapsed.TotalSeconds:00.000}s" +
				$", {m.MessageSeverity + "@" + m.MessageSource + ":",-23} {m.MessageText}";
		}
		#endregion

		#region API to use during the run
		/// <summary>Run state slot.</summary>
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		/// <summary>Reports analyser warning.</summary>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="warnings">The list the warnings will be added to.</param>
		/// <param name="severity">Severity of the message.</param>
		/// <param name="message">The message.</param>
		/// <param name="report">The report the message belongs to.</param>
		public static void AddAnalyserWarning(
			[NotNull] this CompetitionState competitionState,
			[NotNull] List<IWarning> warnings,
			MessageSeverity severity,
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			Code.NotNull(competitionState, nameof(competitionState));
			Code.NotNull(warnings, nameof(warnings));
			Code.NotNullNorEmpty(message, nameof(message));

			competitionState.WriteMessage(MessageSource.Analyser, severity, message);
			warnings.Add(new Warning(severity.ToString(), message, report));
		}

		/// <summary>Writes the setup exception message.</summary>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="messageSource">The source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="origin">The prefix to be used in message.</param>
		/// <param name="ex">The exception to write.</param>
		public static void WriteExceptionMessage(
			[NotNull] this CompetitionState competitionState,
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string origin,
			[NotNull] Exception ex)
		{
			Code.NotNull(competitionState, nameof(competitionState));
			Code.NotNullNorEmpty(origin, nameof(origin));
			Code.NotNull(ex, nameof(ex));

			competitionState.WriteMessage(
				messageSource, messageSeverity,
				$"{origin}. Exception: {ex.Message}.");
			competitionState.Logger.WriteLineError(ex.ToString());
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
		private static int _runCount;

		/// <summary>Runs the benchmark for specified benchmark type.</summary>
		/// <param name="benchmarkType">The type of the benchmark.</param>
		/// <param name="competitionConfig">The config for the benchmark.</param>
		/// <param name="maxRunsAllowed">Total count of reruns allowed.</param>
		/// <param name="allowDebugBuilds">Allow debug builds. If <c>false</c> the benchmark will be skipped on debug builds.</param>
		/// <returns>Competition state for the run.</returns>
		[NotNull]
		internal static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] IConfig competitionConfig,
			int maxRunsAllowed,
			bool allowDebugBuilds)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(competitionConfig, nameof(competitionConfig));

			var runStateSlots = competitionConfig.GetValidators().OfType<RunStateSlots>();
			if (runStateSlots.Count() != 1)
			{
				throw CodeExceptions.Argument(
					nameof(competitionConfig),
					$"The competition config should include single instance of {nameof(RunStateSlots)} validator");
			}

			var competitionState = RunState[competitionConfig];

			try
			{
				competitionState.FirstTimeInit(maxRunsAllowed, competitionConfig);
				var logger = competitionState.Logger;

				using (BeginLogImportant(competitionConfig))
				{
					logger.WriteLine();
					logger.WriteSeparatorLine(benchmarkType.Name, true);
					logger.WriteLine(LogKind.Help, $"{LogInfoPrefix} {benchmarkType.GetShortAssemblyQualifiedName()}");
				}

				try
				{
					var runCount = Interlocked.Increment(ref _runCount);
					if (CheckPreconditions(benchmarkType, runCount, allowDebugBuilds, competitionState))
					{
						RunCore(benchmarkType, competitionState, maxRunsAllowed);
					}
				}
				finally
				{
					Interlocked.Decrement(ref _runCount);
				}
			}
			catch (Exception ex)
			{
				competitionState.WriteExceptionMessage(
					MessageSource.Runner, MessageSeverity.ExecutionError,
					$"Benchmark {benchmarkType.Name}", ex);
			}
			finally
			{
				FlushLoggers(competitionConfig);
			}

			competitionState.CompetitionCompleted();

			return competitionState;
		}

		private static bool CheckPreconditions(
			[NotNull] Type benchmarkType,
			int runCount, bool allowDebugBuilds,
			CompetitionState competitionState)
		{
			if (runCount > 1)
			{
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.SetupError,
					"Competitions cannot be run in parallel. Competition run skipped.");
				return false;
			}

			if (!allowDebugBuilds && benchmarkType.Assembly.IsDebugAssembly())
			{
				var assembly = benchmarkType.Assembly;
				competitionState.WriteMessage(
					MessageSource.Runner, MessageSeverity.Warning,
					$"Please run as a release build. Assembly {assembly.GetName().Name} was build as debug.");
				return false;
			}

			return true;
		}

		private static void RunCore(
			Type benchmarkType,
			CompetitionState competitionState, int maxRunsAllowed)
		{
			var logger = competitionState.Logger;

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

				// Running the benchmark
				var summary = BenchmarkRunner.Run(benchmarkType, competitionState.Config);
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
					$"The benchmark run limit ({competitionState.MaxRunsAllowed} runs(s)) exceeded (read log for details). Try to loose competition limits.");
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
					: $"Benchmark {validationError.Benchmark.ShortInfo}:{Environment.NewLine}\t{validationError.Message}";

				competitionState.WriteMessage(MessageSource.Validator, severity, message);
			}
		}
		#endregion
	}
}