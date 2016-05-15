using System;

using BenchmarkDotNet.Competitions.RunState;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions
{
	[PublicAPI]
	public static class CompetitionRunnerBase
	{
		public static CompetitionState RunCore(
			Type benchmarkType, ManualConfig competitionConfig)
		{
			// TODO: refactor outside?
			competitionConfig.Add(new StateSlots());
			var logger = competitionConfig.GetCompositeLogger();
			var competitionState = CompetitionState.StateSlot[competitionConfig];

			try
			{
				RunCore(benchmarkType, competitionConfig, logger);
			}
			catch (Exception ex)
			{
				competitionState.WriteMessage(MessageSource.BenchmarkRunner, MessageSeverity.ExecutionError, ex.ToString());
			}

			if (competitionState.LastRunSummary != null)
			{
				FillMessages(competitionState);
			}

			return competitionState;
		}

		private static void FillMessages(CompetitionState competitionState)
		{
			if (competitionState.LastRunSummary == null)
				return;

			foreach (var validationError in competitionState.LastRunSummary.ValidationErrors)
			{
				var message = validationError.Benchmark == null
					? validationError.Message
					: $"Benchmark {validationError.Benchmark.ShortInfo}:{Environment.NewLine}\t{validationError.Message}";

				competitionState.WriteMessage(
					MessageSource.Validator,
					validationError.IsCritical ? MessageSeverity.TestError : MessageSeverity.Warning,
					message);
			}
		}

		private static Summary RunCore(
			Type benchmarkType,
			IConfig competitionConfig,
			ILogger logger)
		{
			// TODO: As a settable option?
			// TODO: different maxRerun in annotation mode and in validation mode?
			// TODO: extensibility points for validation and for annotation?
			const int MaxRerunCount = 10;

			var competitionState = CompetitionState.StateSlot[competitionConfig];

			Summary summary = null;
			int rerunCount = 1;
			for (int i = 0; i < rerunCount; i++)
			{
				var lastRun = i >= MaxRerunCount - 1;
				competitionState.InitOnRun(lastRun);

				if (lastRun)
					logger.WriteInfo($"Run #{i}, expected total: {rerunCount} (rerun limit exceeded, last run).");
				else
					logger.WriteInfo($"Run #{i}, expected total: {rerunCount}.");

				// Running the benchmark
				summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);
				competitionState.RunSucceed(summary);
				if (lastRun)
					break;

				// Rerun if annotated
				if (competitionState.RerunRequested)
				{
					rerunCount += 2;
					if (rerunCount > MaxRerunCount)
						rerunCount = MaxRerunCount;

					logger.WriteInfo("Rerun requested. Rerun count changed to {rerunCount}.");
				}
			}

			// TODO: move to somewhere else?
			if (competitionState.RunCount >= MaxRerunCount)
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

			return summary;
		}
	}
}