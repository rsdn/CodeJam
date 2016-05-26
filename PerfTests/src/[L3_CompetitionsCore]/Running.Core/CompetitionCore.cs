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
	/// Reusable parts of competitions logic.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public static class CompetitionCore
	{
		#region API to use during the run
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		public static bool IsWarningOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.Warning;
		public static bool IsCriticalError(this MessageSeverity severity) => severity > MessageSeverity.TestError;

		public static void AddAnalyserWarning(
			this CompetitionState competitionState,
			List<IWarning> warnings,
			MessageSeverity severity,
			string message,
			BenchmarkReport report = null)
		{
			competitionState.WriteMessage(MessageSource.Analyser, severity, message);
			warnings.Add(new Warning(severity.ToString(), message, report));
		}

		public static void FillAnalyserMessages(Summary summary, IEnumerable<IWarning> warnings)
		{
			if (summary == null)
				return;

			var competitionState = RunState[summary];
			foreach (var warning in warnings)
			{
				MessageSeverity severity;
				// TODO: another way to mark warnings as logged???
				if (Enum.TryParse(warning.Kind, out severity))
				{
					// Skipping as these should be logged already.
					continue;
				}

				competitionState.WriteMessage(MessageSource.Analyser, MessageSeverity.Warning, warning.Message);
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

		internal static void LogMessage(this ILogger logger, IMessage message)
		{
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
		internal static CompetitionState Run(
			Type benchmarkType,
			IConfig competitionConfig,
			int maxRunsAllowed)
		{
			if (benchmarkType == null)
				throw new ArgumentNullException(nameof(benchmarkType));

			if (competitionConfig == null)
				throw new ArgumentNullException(nameof(competitionConfig));

			var runStateSlots = competitionConfig.GetValidators().OfType<RunStateSlots>();
			if (!runStateSlots.Any())
			{
				throw new ArgumentException(
					$"The competition config should include {nameof(RunStateSlots)} validator",
					nameof(competitionConfig));
			}

			var competitionState = RunState[competitionConfig];
			try
			{
				RunCore(
					competitionState, maxRunsAllowed,
					benchmarkType, competitionConfig);
			}
			catch (Exception ex)
			{
				competitionState.WriteMessage(MessageSource.BenchmarkRunner, MessageSeverity.ExecutionError, ex.ToString());
				competitionConfig.GetCompositeLogger().WriteLineError(ex.ToString());
			}
			FillMessagesAfterLastRun(competitionState);

			return competitionState;
		}

		private static void RunCore(
			CompetitionState competitionState, int maxRunsAllowed,
			Type benchmarkType, IConfig competitionConfig)
		{
			var logger = competitionConfig.GetCompositeLogger();
			competitionState.FirstTimeInit(maxRunsAllowed, logger);

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

				if (competitionState.GetMessages().Any(m => m.MessageSeverity.IsCriticalError()))
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix}Breaking the run. High severity error occured.");
					break;
				}

				if (competitionState.RunsLeft > 0)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix}Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			// TODO: move to somewhere else?
			if (competitionState.RunLimitExceeded)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.TestError,
					"The benchmark run count exceeded max rerun limits (read log for details). Consider to adjust competition setup.");
			}
			else if (competitionState.RunNumber > 1)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunNumber} times (read log for details). Consider to adjust competition setup.");
			}
		}
		#endregion
	}
}