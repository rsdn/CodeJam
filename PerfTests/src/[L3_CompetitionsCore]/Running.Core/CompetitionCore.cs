using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
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
	public static class CompetitionCore
	{
		#region API to use during the run
		/// <summary>Run state slot.</summary>
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		/// <summary>The message severity is warning or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>True</c> if the severity is warning or higher.</returns>
		public static bool IsWarningOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.Warning;

		/// <summary>The message severity is setup error or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>True</c> if the severity is setup error or higher.</returns>
		public static bool IsCriticalError(this MessageSeverity severity) => severity >= MessageSeverity.SetupError;

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

		/// <summary>Helper method to dump the content of the message into logger.</summary>
		/// <param name="logger">The logger the message will be dumped to.</param>
		/// <param name="message">The message to log.</param>
		internal static void LogMessage([NotNull] this ILogger logger, IMessage message)
		{
			Code.NotNull(logger, nameof(logger));

			if (message.MessageSeverity.IsCriticalError())
			{
				logger.WriteLineError($"{LogImportantInfoPrefix} {message.ToString()}");
			}
			else if (message.MessageSeverity.IsWarningOrHigher())
			{
				logger.WriteLineInfo($"{LogImportantInfoPrefix} {message.ToString()}");
			}
			else
			{
				logger.WriteLineInfo($"{LogInfoPrefix} {message.ToString()}");
			}
		}
		#endregion

		#region Run logic
		/// <summary>Runs the benchmark for specified benchmark type.</summary>
		/// <param name="benchmarkType">The type of the benchmark.</param>
		/// <param name="competitionConfig">The config for the benchmark.</param>
		/// <param name="maxRunsAllowed">The maximum runs limit.</param>
		/// <returns>A competition state for the run.</returns>
		internal static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] IConfig competitionConfig,
			int maxRunsAllowed)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(competitionConfig, nameof(competitionConfig));

			var runStateSlots = competitionConfig.GetValidators().OfType<RunStateSlots>();
			if (!runStateSlots.Any())
			{
				throw CodeExceptions.Argument(
					nameof(competitionConfig),
					$"The competition config should include {nameof(RunStateSlots)} validator");
			}

			var competitionState = RunState[competitionConfig];
			var logger = competitionConfig.GetCompositeLogger();
			competitionState.FirstTimeInit(maxRunsAllowed, logger);

			try
			{
				RunCore(
					benchmarkType, competitionConfig,
					competitionState, maxRunsAllowed);
			}
			catch (Exception ex)
			{
				competitionState.Logger.WriteLineError(ex.ToString());
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner,
					MessageSeverity.ExecutionError,
					ex.Message);
			}

			FillMessagesAfterLastRun(competitionState);

			return competitionState;
		}

		private static void RunCore(
			Type benchmarkType, IConfig competitionConfig,
			CompetitionState competitionState, int maxRunsAllowed)
		{
			var logger = competitionState.Logger;

			while (competitionState.RunsLeft > 0)
			{
				competitionState.PrepareForRun();

				var run = competitionState.RunNumber;
				var runsExpected = competitionState.RunNumber + competitionState.RunsLeft;
				var runMessage = competitionState.RunLimitExceeded
					? $"{LogImportantInfoPrefix}Run {run}, total runs (expected): {runsExpected} (rerun limit exceeded, last run)."
					: $"{LogImportantInfoPrefix}Run {run}, total runs (expected): {runsExpected}.";
				logger.WriteLine();
				logger.WriteLineInfo(runMessage);
				logger.WriteLine();

				// TODO: validate summary after run.
				// Running the benchmark
				var summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);
				competitionState.RunCompleted(summary);

				if (competitionState.RunLimitExceeded)
					break;

				if (competitionState.HasCriticalErrorsInRun)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix}Breaking the run. High severity error occured.");
					break;
				}

				if (competitionState.RunsLeft > 0)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix}Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			// TODO: notify analysers for last run? // Will need to define custom interface, of course.
			// TODO: move to somewhere else?
			if (competitionState.RunLimitExceeded)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.TestError,
					$"The benchmark run count limit ({competitionState.MaxRunsAllowed} runs(s)) exceeded (read log for details). Consider to adjust competition setup.");
			}
			else if (competitionState.RunNumber > 1)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunNumber} times (read log for details). Consider to adjust competition setup.");
			}
		}

		private static void FillMessagesAfterLastRun(CompetitionState competitionState)
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