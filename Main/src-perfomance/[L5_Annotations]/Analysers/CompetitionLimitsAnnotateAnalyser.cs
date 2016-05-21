using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Competitions.SourceAnnotations;

namespace BenchmarkDotNet.Analysers
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	internal class CompetitionLimitsAnnotateAnalyser : CompetitionLimitsAnalyser
	{

		public int AdditionalRunsOnAnnotate { get; set; }
		public bool AnnotateOnRun { get; set; }
		public bool LogAnnotationResults { get; set; }
		public string PreviousLogUri { get; set; }

		#region Overrides of CompetitionLimitsAnalyser
		protected override void InitCompetitionTargets(CompetitionTargets competitionTargets, Summary summary, List<IWarning> warnings)
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
						competitionTarget.MergeWith(target2);
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
			var logger = summary.Config.GetCompositeLogger();

			bool updated = MergeTargetsToAnnotate(summary, competitionTargets);

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

			if (competitionState.LooksLikeLastRun)
			{
				AnnotateResultsCore(competitionTargets, logger, warnings);

				if (LogAnnotationResults)
				{
					ExportAnnotationResultsCore(competitionTargets, logger, warnings);
				}
			}
		}

		private void AnnotateResultsCore(
			CompetitionTargets competitionTargets, ILogger logger,
			List<IWarning> warnings)
		{
			const int looseByPercent = 3;

			var targetsToAnnotate = competitionTargets.Values
				.Where(t => t.WasUpdated)
				.ToArray();
			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.LooseLimits(looseByPercent);
			}

			AnnotateSourceHelper.TryAnnotateBenchmarkFiles(
			   targetsToAnnotate, warnings, logger);
		}

		private void ExportAnnotationResultsCore(
			CompetitionTargets competitionTargets, ILogger logger,
			List<IWarning> warnings)
		{
			var targets = competitionTargets.Values
				.Where(t => t.WasUpdated)
				.ToArray();
			if (targets.Length == 0)
			{
				logger.WriteLineInfo(
					"// No competition benchmark annotations were updated, nothing to export.");
				return;
			}

			var xDoc = XmlAnnotations.CreateEmptyResourceDoc(true);
			foreach (var competitionTarget in targets)
			{
				XmlAnnotations.SaveCompetitionTarget(competitionTarget, xDoc);
			}

			logger.WriteLineInfo(XmlAnnotations.LogAnnotationStart);
			var tmp = XmlAnnotations.SaveToString(xDoc);
			logger.WriteLineInfo(tmp.ToString());

			logger.WriteLineInfo(XmlAnnotations.LogAnnotationEnd);
		}

		private static bool MergeTargetsToAnnotate(
			Summary summary, CompetitionTargets competitionTargets)
		{
			bool result = false;

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
					result |= competitionTarget.MergeWith(limit);
				}
			}

			return result;
		}
	}
}