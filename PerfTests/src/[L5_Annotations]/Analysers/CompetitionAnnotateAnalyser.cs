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
	/// <summary>Analyser class that enables limits validation and annotation for competition benchmarks.</summary>
	/// <seealso cref="CodeJam.PerfTests.Analysers.CompetitionAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
	internal class CompetitionAnnotateAnalyser : CompetitionAnalyser
	{
		#region Adjusted targets
		private class PreviousRunDocuments : Dictionary<string, XDocument[]> { }

		private static readonly RunState<PreviousRunDocuments> _documentsFromLog =
			new RunState<PreviousRunDocuments>();
		#endregion

		#region Properties
		/// <summary>Try to annotate source with actual competition limits.</summary>
		/// <value><c>true</c> if the analyser should update source annotations; otherwise, <c>false</c>. </value>
		public bool UpdateSourceAnnotations { get; set; }

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Enable the <seealso cref="CompetitionAnalyser.LogAnnotationResults"/> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousLogUri { get; set; }

		/// <summary>
		/// Count of additional runs performed after updating the limits annotations.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>The count of additional runs performed after updating the limits annotations.</value>
		public int RequestRerunsOnAnnotate { get; set; }
		#endregion

		/// <summary>
		/// Refills the competition targets collection and fills competition limits from the <seealso cref="PreviousLogUri"/>.
		/// </summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		protected override void InitCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary)
		{
			base.InitCompetitionTargets(competitionTargets, summary);

			if (!UpdateSourceAnnotations || string.IsNullOrEmpty(PreviousLogUri))
				return;

			var competitionState = CompetitionCore.RunState[summary];

			var benchmarkDocs = _documentsFromLog[summary]
				.GetOrAdd(PreviousLogUri, uri => XmlAnnotations.TryParseBenchmarkDocsFromLog(uri, competitionState));

			competitionState.WriteMessage(
				MessageSource.Analyser, MessageSeverity.Informational,
				$"Parsing previous results ({benchmarkDocs.Length} doc(s)) from log {PreviousLogUri}.");

			bool updated = false;
			foreach (var competitionTarget in competitionTargets.Values)
			{
				bool hasAnnotations = false;
				foreach (var resourceDoc in benchmarkDocs)
				{
					var target2 = XmlAnnotations.TryParseCompetitionTarget(competitionTarget.Target, resourceDoc, competitionState);
					if (target2 != null)
					{
						hasAnnotations = true;
						updated |= competitionTarget.UnionWith(target2);
					}
				}
				if (!hasAnnotations)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Informational,
						$"Logged anotations for {competitionTarget.Target.Type.Name}.{competitionTarget.Target.Method.Name}: no annotation exists.");
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

		/// <summary>
		/// Performs limit validation for competition benchmarks. Updates the annotations with actual limits.
		/// </summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="warnings">List of warnings for the benchmarks.</param>
		protected override void ValidateSummary(Summary summary, List<IWarning> warnings)
		{
			base.ValidateSummary(summary, warnings);

			if (!UpdateSourceAnnotations)
				return;

			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];

			var hasAdjustedTargets = TryAdjustCompetitionTargets(competitionTargets, summary);

			var targetsToAnnotate = competitionTargets.Values.Where(t => t.HasUnsavedChanges).ToArray();
			AnnotateResultsCore(targetsToAnnotate, competitionState);

			RequestReruns(hasAdjustedTargets, competitionState);
		}

		private static bool TryAdjustCompetitionTargets(CompetitionTargets competitionTargets, Summary summary)
		{
			bool adjusted = false;

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

					// No warnings required. Missing values should be checked by base class.
					if (minRatio == null || maxRatio == null)
						continue;

					if (minRatio > maxRatio)
					{
						Algorithms.Swap(ref minRatio, ref maxRatio);
					}

					var limit = new CompetitionLimit(minRatio ?? 0, maxRatio ?? 0);
					adjusted |= competitionTarget.UnionWith(limit);
				}
			}

			return adjusted;
		}

		private static void AnnotateResultsCore(CompetitionTarget[] targetsToAnnotate, CompetitionState competitionState)
		{
			const int looseByPercent = 5;

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.LooseLimitsAndMarkAsSaved(looseByPercent);
			}

			AnnotateSourceHelper.TryAnnotateBenchmarkFiles(targetsToAnnotate, competitionState);
		}

		private void RequestReruns(bool updated, CompetitionState competitionState)
		{
			if (RequestRerunsOnAnnotate > 0)
			{
				if (updated)
				{
					competitionState.RequestReruns(RequestRerunsOnAnnotate, "Annotations were updated.");
				}
				else
				{
					competitionState.RequestReruns(0, "All competition benchmarks do not require annotation.");
				}
			}
		}
	}
}