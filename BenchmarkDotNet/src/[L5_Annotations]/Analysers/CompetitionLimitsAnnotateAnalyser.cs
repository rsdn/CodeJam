using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Competitions.SourceAnnotations;

namespace BenchmarkDotNet.Analysers
{
	// TODO: needs code review
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	internal class CompetitionLimitsAnnotateAnalyser : CompetitionLimitsAnalyser
	{
		#region Adjusted targets
		private class AdjustedCompetitionTargets : HashSet<CompetitionTarget> { }

		private static readonly RunState<AdjustedCompetitionTargets> _adjustedTargets =
			new RunState<AdjustedCompetitionTargets>();
		#endregion

		public int AdditionalRunsOnAnnotate { get; set; }
		public bool AnnotateOnRun { get; set; }
		public bool LogAnnotationResults { get; set; }
		public string PreviousLogUri { get; set; }

		#region Overrides of CompetitionLimitsAnalyser
		protected override void InitCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary, List<IWarning> warnings)
		{
			base.InitCompetitionTargets(competitionTargets, summary, warnings);

			if (!AnnotateOnRun || string.IsNullOrEmpty(PreviousLogUri))
				return;

			var docs = XmlAnnotations.GetDocumentsFromLog(PreviousLogUri);

			foreach (var resourceDoc in docs)
			{
				foreach (var competitionTarget in competitionTargets.Values)
				{
					var target2 = XmlAnnotations.TryParseCompetitionTarget(
						competitionTarget.Target,
						resourceDoc);
					if (target2 != null)
					{
						competitionTarget.UnionWith(target2);
					}
				}
			}
		}
		#endregion

		protected override void ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets,
			List<IWarning> warnings)
		{
			base.ValidateSummary(summary, competitionTargets, warnings);

			if (!AnnotateOnRun)
				return;

			var competitionState = CompetitionCore.RunState[summary];
			var adjustedTargets = _adjustedTargets[summary];
			var logger = summary.Config.GetCompositeLogger();

			var targetsToAnnotate = AdjustCompetitionTargets(summary, competitionTargets);

			AnnotateResultsCore(targetsToAnnotate, logger, warnings);
			RequestReruns(targetsToAnnotate.Any(), competitionState);

			adjustedTargets.UnionWith(targetsToAnnotate);

			if (competitionState.LooksLikeLastRun && LogAnnotationResults)
			{
				XmlAnnotations.LogAdjustedCompetitionTargets(adjustedTargets, logger);
			}
		}

		private void RequestReruns(bool updated, CompetitionState competitionState)
		{
			if (AdditionalRunsOnAnnotate > 0)
			{
				if (updated)
				{
					// TODO: detailed message???
					competitionState.RequestReruns(
						AdditionalRunsOnAnnotate,
						"Annotations updated.");
				}
				else
				{
					competitionState.RequestReruns(
						0,
						"All competition benchmarks do not require annotation.");
				}
			}
		}

		private void AnnotateResultsCore(
			CompetitionTarget[] targetsToAnnotate, ILogger logger,
			List<IWarning> warnings)
		{
			const int looseByPercent = 3;

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.LooseLimitsAndMarkAsSaved(looseByPercent);
			}

			AnnotateSourceHelper.TryAnnotateBenchmarkFiles(targetsToAnnotate, warnings, logger);
		}
		

		private static CompetitionTarget[] AdjustCompetitionTargets(
			Summary summary, CompetitionTargets competitionTargets)
		{
			var result = new List<CompetitionTarget>();

			foreach (var benchGroup in summary.SameConditionBenchmarks())
			{
				foreach (var benchmark in benchGroup)
				{
					if (benchmark.Target.Baseline)
						continue;

					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target.Method, out competitionTarget))
						continue;

					var minRatio = summary.TryGetScaledPercentile(benchmark, 85);
					var maxRatio = summary.TryGetScaledPercentile(benchmark, 95);

					// No warnings required. Missing values should be checked by CompetitionLimitsAnalyser.
					if (minRatio == null || maxRatio == null)
						continue;

					if (minRatio > maxRatio)
					{
						var temp = minRatio;
						minRatio = maxRatio;
						maxRatio = temp;
					}

					var limit = new CompetitionLimit(minRatio.Value, maxRatio.Value);
					if (competitionTarget.UnionWith(limit))
					{
						result.Add(competitionTarget);
					}
				}
			}

			return result.ToArray();
		}
	}
}