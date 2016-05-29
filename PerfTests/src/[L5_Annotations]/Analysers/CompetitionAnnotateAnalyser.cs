using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

namespace CodeJam.PerfTests.Analysers
{
	// TODO: needs code review
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
	internal class CompetitionAnnotateAnalyser : CompetitionAnalyser
	{
		#region Adjusted targets
		private class UpdatedCompetitionTargets : HashSet<CompetitionTarget> { }

		private class PreviousLogDocuments : Dictionary<string, XDocument[]> { }

		private static readonly RunState<UpdatedCompetitionTargets> _updatedTargets =
			new RunState<UpdatedCompetitionTargets>();

		private static readonly RunState<PreviousLogDocuments> _previousLogDocuments =
			new RunState<PreviousLogDocuments>();
		#endregion

		public bool UpdateSourceAnnotations { get; set; }
		public int RequestRerunsOnAnnotate { get; set; }
		public bool LogAnnotationResults { get; set; }
		public string PreviousLogUri { get; set; }

		protected override void InitCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary)
		{
			base.InitCompetitionTargets(competitionTargets, summary);

			if (!UpdateSourceAnnotations || string.IsNullOrEmpty(PreviousLogUri))
				return;

			var competitionState = CompetitionCore.RunState[summary];

			var docs = _previousLogDocuments[summary]
				.GetOrAdd(PreviousLogUri, uri => XmlAnnotations.TryParseBenchmarkDocsFromLog(uri, competitionState));

			competitionState.WriteMessage(
				MessageSource.Analyser, MessageSeverity.Informational,
				$"Parsing previous results ({docs.Length} doc(s)) from log {PreviousLogUri}.");

			bool updated = false;
			foreach (var resourceDoc in docs)
			{
				foreach (var competitionTarget in competitionTargets.Values)
				{
					var target2 = XmlAnnotations.TryParseCompetitionTarget(competitionTarget.Target, resourceDoc, competitionState);
					if (target2 != null)
					{
						updated |= competitionTarget.UnionWith(target2);
					}
				}
			}

			if (updated)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Benchmark limits was updated from log file {PreviousLogUri}.");
			}
			else
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					$"No benchmark limits found. Log file: {PreviousLogUri}.");
			}
		}

		protected override void ValidateSummary(Summary summary, List<IWarning> warnings)
		{
			base.ValidateSummary(summary, warnings);

			if (!UpdateSourceAnnotations)
				return;

			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];

			var hasAdjustedTargets = TryAdjustCompetitionTargets(summary, competitionTargets);

			var targetsToAnnotate = competitionTargets.Values.Where(t => t.HasUnsavedChanges).ToArray();
			AnnotateResultsCore(competitionState, targetsToAnnotate);

			RequestReruns(competitionState, hasAdjustedTargets);

			var allUpdatedTargets = _updatedTargets[summary];
			allUpdatedTargets.UnionWith(targetsToAnnotate);

			if (competitionState.LooksLikeLastRun && LogAnnotationResults)
			{
				XmlAnnotations.LogCompetitionTargets(allUpdatedTargets, competitionState);
			}
		}

		private void RequestReruns(CompetitionState competitionState, bool updated)
		{
			if (RequestRerunsOnAnnotate > 0)
			{
				if (updated)
				{
					// TODO: detailed message???
					competitionState.RequestReruns(
						RequestRerunsOnAnnotate,
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

		private static void AnnotateResultsCore(
			CompetitionState competitionState, CompetitionTarget[] targetsToAnnotate)
		{
			const int looseByPercent = 3;

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.LooseLimitsAndMarkAsSaved(looseByPercent);
			}

			AnnotateSourceHelper.TryAnnotateBenchmarkFiles(competitionState, targetsToAnnotate);
		}

		private static bool TryAdjustCompetitionTargets(
			Summary summary, CompetitionTargets competitionTargets)
		{
			bool result = false;

			foreach (var benchGroup in summary.SameConditionsBenchmarks())
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

					// No warnings required. Missing values should be checked by CompetitionAnalyser.
					if (minRatio == null || maxRatio == null)
						continue;

					if (minRatio > maxRatio)
					{
						var temp = minRatio;
						minRatio = maxRatio;
						maxRatio = temp;
					}

					var limit = new CompetitionLimit(minRatio.Value, maxRatio.Value);
					result |= competitionTarget.UnionWith(limit);
				}
			}

			return result;
		}
	}
}