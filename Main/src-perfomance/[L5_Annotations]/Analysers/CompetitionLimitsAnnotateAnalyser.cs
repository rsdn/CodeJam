using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
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

		protected override void ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets, List<IWarning> warnings)
		{
			base.ValidateSummary(summary, competitionTargets, warnings);

			if (!AnnotateOnRun)
				return;

			var competitionState = CompetitionCore.RunState[summary];
			var logger = summary.Config.GetCompositeLogger();

			var targetsToAnnotate = GetTargetsToAnnotate(summary, competitionTargets);
			if (targetsToAnnotate.Length != 0)
			{
				targetsToAnnotate = AnnotateSourceHelper.TryAnnotateBenchmarkFiles(
					targetsToAnnotate, warnings, logger);

				foreach (var competitionTarget in targetsToAnnotate)
				{
					competitionTargets[competitionTarget.Target.Method] = competitionTarget;
				}
			}

			if (AdditionalRunsOnAnnotate > 0)
			{
				if (targetsToAnnotate.Length == 0)
				{
					competitionState.RequestReruns(
						0,
						"All competition benchmarks do not require annotation.");
				}
				else
				{
					// TODO: detailed message???
					competitionState.RequestReruns(
						AdditionalRunsOnAnnotate,
						"Annotations updated.");
				}
			}

			if (competitionState.LooksLikeLastRun && LogAnnotationResults)
			{
				ExportAnnotationResultsCore(competitionTargets, logger);
			}
		}

		private void ExportAnnotationResultsCore(CompetitionTargets competitionTargets, ILogger logger)
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

			var xDoc = AnnotateSourceHelper.CreateXmlAnnotationDoc(true);
			foreach (var competitionTarget in targets)
			{
				AnnotateSourceHelper.UpdateXmlAnnotation(competitionTarget, xDoc);
			}

			logger.WriteLineInfo(CompetitionLimitConstants.LogAnnotationStart);

			var tmp = new StringBuilder();
			var writerSettings = AnnotateSourceHelper.GetXmlWriterSettings();
			using (var writer = XmlWriter.Create(tmp, writerSettings))
			{
				xDoc.Save(writer);
			}
			logger.WriteLineInfo(tmp.ToString());

			logger.WriteLineInfo(CompetitionLimitConstants.LogAnnotationEnd);
		}

		private static CompetitionTarget[] GetTargetsToAnnotate(
			Summary summary, CompetitionTargets competitionTargets)
		{
			var fixedMinTargets = new HashSet<CompetitionTarget>();
			var fixedMaxTargets = new HashSet<CompetitionTarget>();
			var newTargets = new Dictionary<Target, CompetitionTarget>();

			foreach (var benchGroup in summary.SameConditionBenchmarks())
			{
				foreach (var benchmark in benchGroup)
				{
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

					CompetitionTarget newTarget;
					if (!newTargets.TryGetValue(competitionTarget.Target, out newTarget))
					{
						newTarget = competitionTarget.Clone();
					}

					if (newTarget.UnionWithMin(minRatio.Value))
					{
						fixedMinTargets.Add(newTarget);
						newTargets[newTarget.Target] = newTarget;
					}
					if (newTarget.UnionWithMax(maxRatio.Value))
					{
						fixedMaxTargets.Add(newTarget);
						newTargets[newTarget.Target] = newTarget;
					}
				}
			}

			foreach (var competitionTarget in fixedMinTargets)
			{
				// min = 0.95x;
				competitionTarget.UnionWithMin(
					Math.Floor(competitionTarget.Min * 95) / 100);
			}
			foreach (var competitionTarget in fixedMaxTargets)
			{
				// max = 1.05x;
				competitionTarget.UnionWithMax(
					Math.Ceiling(competitionTarget.Max * 105) / 100);
			}

			return newTargets.Values.ToArray();
		}
	}
}