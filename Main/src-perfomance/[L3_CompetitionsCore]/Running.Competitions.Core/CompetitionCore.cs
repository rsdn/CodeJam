using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Messages;
using BenchmarkDotNet.Running.Stateful;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Competitions.Core
{
	/// <summary>
	/// Reusable parts of competitions logic.
	/// </summary>
	[PublicAPI]
	public static class CompetitionCore
	{
		public static readonly RunState<CompetitionState> RunState = new RunState<CompetitionState>();

		public static CompetitionState Run(Type benchmarkType, ManualConfig competitionConfig)
		{
			if (benchmarkType == null)
				throw new ArgumentNullException(nameof(benchmarkType));

			if (competitionConfig == null)
				throw new ArgumentNullException(nameof(competitionConfig));

			var runStateSlots = competitionConfig.GetValidators().OfType<RunStateSlots>().FirstOrDefault();
			if (runStateSlots != null)
			{
				throw new ArgumentException(
					$"The competition config should not include {nameof(RunStateSlots)}",
					nameof(competitionConfig));
			}

			competitionConfig.Add(new RunStateSlots());
			var competitionState = RunState[competitionConfig];

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

		public static void FillAnalyserMessages(Summary summary, IEnumerable<IWarning> warnings)
		{
			if (summary == null)
				return;

			var competitionState = RunState[summary];
			if (competitionState.LastRun || !competitionState.RerunRequested)
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

		public static void ReportMessagesToUser(
			CompetitionState competitionState,
			Action<string> reportErrorCallback,
			Action<string> reportWarningCallback)
		{
			if (competitionState == null)
				return;

			var messages = competitionState.GetMessages();

			var errorMessages = messages
				.Where(m => m.MessageSeverity > MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => m.MessageText)
				.ToArray();
			if (errorMessages.Length > 0)
			{
				reportErrorCallback?.Invoke(
					string.Join(Environment.NewLine, errorMessages));
			}

			var validationMessages = messages
				.Where(m => m.MessageSeverity == MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => m.MessageText)
				.ToArray();
			if (validationMessages.Length > 0)
			{
				reportWarningCallback?.Invoke(
					string.Join(Environment.NewLine, validationMessages));
			}
		}

		private static void RunCore(
			CompetitionState competitionState,
			Type benchmarkType,
			IConfig competitionConfig)
		{
			// TODO: As a settable option?
			// TODO: different maxRerun in annotation mode and in validation mode?
			// TODO: extensibility points for validation and for annotation?
			const int maxRerunCount = 10;

			var logger = competitionConfig.GetCompositeLogger();
			int rerunCount = 1;
			for (int i = 0; i < rerunCount; i++)
			{
				var lastRun = i >= maxRerunCount - 1;
				competitionState.InitOnRun(lastRun);

				var runMessage = lastRun
					? $"Run #{i}, expected total: {rerunCount} (rerun limit exceeded, last run)."
					: $"Run #{i}, expected total: {rerunCount}.";
				logger.WriteInfo(runMessage);

				// Running the benchmark
				var summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);
				competitionState.RunSucceed(summary);

				if (lastRun)
					break;

				// Rerun if requested
				if (competitionState.RerunRequested)
				{
					rerunCount += 2;
					if (rerunCount > maxRerunCount)
						rerunCount = maxRerunCount;

					logger.WriteInfo("Rerun requested. Rerun count changed to {rerunCount}.");
				}
			}

			// TODO: move to somewhere else?
			if (competitionState.RunCount >= maxRerunCount)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.TestError,
					"The benchmark run count exceeded rerun count limits. Consider to adjust competition limits.");
			}
			else if (competitionState.RunCount > 1)
			{
				competitionState.WriteMessage(
					MessageSource.BenchmarkRunner, MessageSeverity.Warning,
					$"The benchmark was run {competitionState.RunCount} times. Consider to adjust competition limits.");
			}
		}
	}
}