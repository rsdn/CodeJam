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

		protected const int LooseByPercent = 3;

		#region Properties
		/// <summary>Try to annotate source with actual competition limits.</summary>
		/// <value><c>true</c> if the analyser should update source annotations; otherwise, <c>false</c>. </value>
		public bool UpdateSourceAnnotations { get; set; }

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// If <see cref="UpdateSourceAnnotations"/> set to <c>true</c>, the annotations will be updated with limits from the log.
		/// Enable the <seealso cref="CompetitionAnalyser.LogCompetitionLimits"/> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri { get; set; }

		/// <summary>
		/// Count of additional runs performed after updating the limits annotations.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>The count of additional runs performed after updating the limits annotations.</value>
		public int AdditionalRerunsOnAnnotate { get; set; }
		#endregion

		/// <summary>
		/// Refills the competition targets collection and fills competition limits from the <seealso cref="PreviousRunLogUri"/>.
		/// </summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		protected override void InitCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary)
		{
			base.InitCompetitionTargets(competitionTargets, summary);

			if (!UpdateSourceAnnotations || string.IsNullOrEmpty(PreviousRunLogUri))
				return;

			var competitionState = CompetitionCore.RunState[summary];
			competitionState.WriteMessage(
				MessageSource.Analyser, MessageSeverity.Informational,
				$"Reading annotations from log {PreviousRunLogUri}.");

			var benchmarkDocs = _documentsFromLog[summary]
				.GetOrAdd(PreviousRunLogUri, uri => XmlAnnotations.TryParseBenchmarkDocsFromLog(uri, competitionState));

			if (benchmarkDocs.IsNullOrEmpty())
				return;

			competitionState.WriteMessage(
				MessageSource.Analyser, MessageSeverity.Informational,
				$"Parsing previous results ({benchmarkDocs.Length} doc(s)) from log {PreviousRunLogUri}.");

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
					$"Benchmark limits were updated from log file {PreviousRunLogUri}.");
			}
			else
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"All benchmnarks are in log limits. Log file: {PreviousRunLogUri}.");
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

			var adjustedTargets = TryAdjustCompetitionTargets(competitionTargets, summary);
			foreach (var competitionTarget in adjustedTargets)
			{
				competitionTarget.LooseLimits(LooseByPercent);
			}
			RequestReruns(adjustedTargets.Any(), competitionState);

			var targetsToAnnotate = competitionTargets.Values.Where(t => t.HasUnsavedChanges).ToArray();
			var annotatedTargets = AnnotateSourceHelper.TryAnnotateBenchmarkFiles(targetsToAnnotate, competitionState);
			if (annotatedTargets.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					"The sources were updated with a new annotations. Please check them before commiting the changes.");
			}

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.MarkAsSaved();
			}
		}

		private static CompetitionTarget[] TryAdjustCompetitionTargets(CompetitionTargets competitionTargets, Summary summary)
		{
			var adjusted = new List<CompetitionTarget>();

			foreach (var benchGroup in summary.SameConditionsBenchmarks())
			{
				foreach (var benchmark in benchGroup)
				{
					if (benchmark.Target.Baseline)
						continue;

					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target.Method, out competitionTarget))
						continue;

					// todo: single ratio
					var actualRatioMin = summary.TryGetScaledConfidenceIntervalLower(benchmark);
					var actualRatioMax = summary.TryGetScaledConfidenceIntervalLower(benchmark);

					// No warnings required. Missing values should be checked by base class.
					if (actualRatioMin == null || actualRatioMax == null)
						continue;

					if (actualRatioMin > actualRatioMax)
					{
						Algorithms.Swap(ref actualRatioMin, ref actualRatioMax);
					}

					var limit = new CompetitionLimit(actualRatioMin.Value, actualRatioMax.Value);

					if (competitionTarget.UnionWith(limit))
					{
						adjusted.Add(competitionTarget);
					}
				}
			}

			return adjusted.ToArray();
		}

		private void RequestReruns(bool updated, CompetitionState competitionState)
		{
			if (AdditionalRerunsOnAnnotate > 0)
			{
				if (updated)
				{
					competitionState.RequestReruns(AdditionalRerunsOnAnnotate, "Annotations were updated.");
				}
				else
				{
					competitionState.RequestReruns(0, "All competition benchmarks do not require annotation.");
				}
			}
		}
	}
}