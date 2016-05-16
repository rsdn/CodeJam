using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.SourceAnnotations;

namespace BenchmarkDotNet.Analysers
{
	internal class CompetitionLimitsAnnotateAnalyser : CompetitionLimitsAnalyser
	{
		public bool AnnotateOnRun { get; set; } = true;

		protected override bool ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets, List<IWarning> warnings)
		{
			var result = base.ValidateSummary(summary, competitionTargets, warnings);

			if (!AnnotateOnRun)
				return result;

			var logger = summary.Config.GetCompositeLogger();

			var targetsToAnnotate = GetTargetsToAnnotate(summary, competitionTargets);
			if (targetsToAnnotate.Length == 0)
			{
				logger.WriteLineInfo("All competition benchmarks do not require annotation. Skipping.");
			}
			else
			{
				var annotatedTargets = AnnotateSourceHelper.TryAnnotateBenchmarkFiles(
					targetsToAnnotate, warnings, logger);

				foreach (var competitionTarget in annotatedTargets)
				{
					competitionTargets[competitionTarget.Target] = competitionTarget;
				}
			}

			return result;
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
					if (!competitionTargets.TryGetValue(benchmark.Target, out competitionTarget))
						continue;

					var minRatio = summary.TryGetScaledPercentile(benchmark, 85);
					var maxRatio = summary.TryGetScaledPercentile(benchmark, 95);
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

					if (newTarget.UnionWithMin(minRatio ?? 0))
					{
						fixedMinTargets.Add(newTarget);
						newTargets[newTarget.Target] = newTarget;
					}
					if (newTarget.UnionWithMax(maxRatio ?? 0))
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