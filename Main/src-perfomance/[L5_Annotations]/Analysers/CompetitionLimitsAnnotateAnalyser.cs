using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Competitions.SourceAnnotations;

namespace BenchmarkDotNet.Analysers
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	internal class CompetitionLimitsAnnotateAnalyser : CompetitionLimitsAnalyser
	{
		public bool AnnotateOnRun { get; set; } = true;
		public int AdditionalRunsOnAnnotate { get; set; } = 2;

		protected override void ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets, List<IWarning> warnings)
		{
			base.ValidateSummary(summary, competitionTargets, warnings);

			if (!AnnotateOnRun)
				return;

			var targetsToAnnotate = GetTargetsToAnnotate(summary, competitionTargets);
			if (targetsToAnnotate.Length == 0)
			{
				CompetitionCore.RunState[summary].RequestReruns(
					0,
					"All competition benchmarks do not require annotation.");
			}
			else
			{
				var logger = summary.Config.GetCompositeLogger();
				var annotatedTargets = AnnotateSourceHelper.TryAnnotateBenchmarkFiles(
					targetsToAnnotate, warnings, logger);

				foreach (var competitionTarget in annotatedTargets)
				{
					competitionTargets[competitionTarget.Target.Method] = competitionTarget;
				}

				if (annotatedTargets.Length > 0 && AdditionalRunsOnAnnotate > 0)
				{
					// TODO: detailed message???
					CompetitionCore.RunState[summary].RequestReruns(
						AdditionalRunsOnAnnotate,
						"Annotations updated.");
				}
			}
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