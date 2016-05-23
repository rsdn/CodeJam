using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Messages;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.HostLogger;

namespace BenchmarkDotNet.Running.Competitions.Core
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

		public static void AddWarning(
			this List<IWarning> warnings,
			MessageSeverity severity, string message,
			BenchmarkReport report = null) =>
				warnings.Add(
					new Warning(severity.ToString(), message, report));

		public static void FillAnalyserMessages(Summary summary, IEnumerable<IWarning> warnings)
		{
			if (summary == null)
				return;

			var competitionState = RunState[summary];
			if (competitionState.LooksLikeLastRun)
			{
				foreach (var warning in warnings)
				{
					MessageSeverity severity;
					if (!Enum.TryParse(warning.Kind, out severity))
					{
						severity = MessageSeverity.Warning;
					}

					competitionState.WriteMessage(MessageSource.Analyser, severity, warning.Message);
				}
			}
		}

		private static void FillMessagesAfterLastRun(CompetitionState competitionState)
		{
			if (competitionState.LastRunSummary == null)
				return;

			foreach (var validationError in competitionState.LastRunSummary.ValidationErrors)
			{
				var severity = validationError.IsCritical ? MessageSeverity.TestError : MessageSeverity.Warning;
				var message = validationError.Benchmark == null
					? validationError.Message
					: $"Benchmark {validationError.Benchmark.ShortInfo}:{Environment.NewLine}\t{validationError.Message}";

				competitionState.WriteMessage(MessageSource.Validator, severity, message);
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
			}
			FillMessagesAfterLastRun(competitionState);

			return competitionState;
		}

		private static void RunCore(
			CompetitionState competitionState, int maxRunsAllowed,
			Type benchmarkType, IConfig competitionConfig)
		{
			var logger = competitionConfig.GetCompositeLogger();
			competitionState.FirstTimeInit(maxRunsAllowed);
			while (competitionState.RunsLeft > 0)
			{
				competitionState.PrepareForRun();

				var run = competitionState.RunNumber;
				var runsExpected = competitionState.RunNumber + competitionState.RunsLeft;
				var runMessage = competitionState.LastRun
					? $"{LogImportantInfoPrefix}Run {run}, total runs (expected): {runsExpected} (rerun limit exceeded, last run)."
					: $"{LogImportantInfoPrefix}Run {run}, total runs (expected): {runsExpected}.";
				logger.WriteLine();
				logger.WriteLineInfo(runMessage);
				logger.WriteLine();

				// TODO: validate summary after run.
				// Running the benchmark
				var summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);
				competitionState.RunCompleted(summary);

				if (competitionState.LastRun)
					break;

				if (competitionState.RunsLeft > 0)
				{
					logger.WriteLineInfo($"{LogImportantInfoPrefix}Rerun requested. Runs left: {competitionState.RunsLeft}.");
				}
			}

			// TODO: move to somewhere else?
			if (competitionState.LastRun)
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