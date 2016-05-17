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

namespace BenchmarkDotNet.Running.Competitions.Core
{
	/// <summary>
	/// Reusable parts of competitions logic.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public static class CompetitionCore
	{
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
			if (competitionState.LastRun || competitionState.AdditionalRunsRequested == 0)
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

		private static void FillMessagesAfterLastRun(Summary summary)
		{
			if (summary == null)
				return;

			var competitionState = RunState[summary];
			foreach (var validationError in summary.ValidationErrors)
			{
				var severity = validationError.IsCritical ? MessageSeverity.TestError : MessageSeverity.Warning;
				var message = validationError.Benchmark == null
					? validationError.Message
					: $"Benchmark {validationError.Benchmark.ShortInfo}:{Environment.NewLine}\t{validationError.Message}";

				competitionState.WriteMessage(MessageSource.Validator, severity, message);
			}
		}

		internal static CompetitionState Run(
			Type benchmarkType,
			IConfig competitionConfig,
			int maxRunCount)
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
			competitionState.FirstTimeInit(maxRunCount);

			try
			{
				RunCore(competitionState, benchmarkType, competitionConfig);
			}
			catch (Exception ex)
			{
				competitionState.WriteMessage(MessageSource.BenchmarkRunner, MessageSeverity.ExecutionError, ex.ToString());
			}
			FillMessagesAfterLastRun(competitionState.LastRunSummary);

			return competitionState;
		}

		private static void RunCore(
			CompetitionState competitionState,
			Type benchmarkType,
			IConfig competitionConfig)
		{
			var logger = competitionConfig.GetCompositeLogger();
			int runsLeft = 1;
			int run = 0;
			while (runsLeft > 0)
			{
				run++;
				runsLeft--;

				var runMessage = competitionState.LastRun
					? $"// !Run {run}, runs requested: {run + runsLeft} (rerun limit exceeded, last run)."
					: $"// !Run {run}, runs requested: {run + runsLeft}.";
				logger.WriteLineInfo(runMessage);

				competitionState.PrepareForRun();

				// Running the benchmark
				var summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);
				competitionState.RunCompleted(summary);

				if (competitionState.LastRun)
					break;

				if (competitionState.AdditionalRunsRequested > 0)
				{
					runsLeft = Math.Max(runsLeft, competitionState.AdditionalRunsRequested);

					logger.WriteLineInfo($"// !Rerun requested. Runs left: {runsLeft}.");
				}
			}

			// TODO: move to somewhere else?
			if (competitionState.LastRun)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.TestError,
					"The benchmark run count exceeded max rerun limits (read log for details). Consider to adjust competition setup.");
			}
			else if (competitionState.RunCount > 1)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunCount} times (read log for details). Consider to adjust competition setup.");
			}
		}
	}
}