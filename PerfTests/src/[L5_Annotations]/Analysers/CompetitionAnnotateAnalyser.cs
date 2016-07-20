using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

using static CodeJam.PerfTests.Loggers.HostLogger;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Competition analyser that updates source annotations if competition limit checking failed.</summary>
	/// <seealso cref="CompetitionAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
	internal class CompetitionAnnotateAnalyser : CompetitionAnalyser
	{
		#region Cached XML annotations from log
		// DONTTOUCH: Each run should use results from previous run log.
		// DO NOT cache log output in static field 
		// as test can be run multiple times in same appdomain.
		private class LoggedXmlAnnotations : Dictionary<string, XDocument[]> { }

		private static readonly RunState<LoggedXmlAnnotations> _annotationsCacheSlot =
			new RunState<LoggedXmlAnnotations>();
		#endregion

		#region Properties
		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="CompetitionAnalyser.LogCompetitionLimits"/> to <c>true</c> to log the limits.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri { get; set; }

		/// <summary>
		/// Count of runs skipped before source annotations will be applied.
		/// Set this to non-zero positive value to skip some runs before first annotation applied.
		/// Should be used together with <see cref="CompetitionAnalyser.MaxRerunsIfValidationFailed"/>
		/// when run on unstable environments such as virtual machines or low-end notebooks.
		/// </summary>
		/// <value>The count of runs performed before updating the limits annotations.</value>
		public int SkipRunsBeforeApplyingAnnotations { get; set; }

		/// <summary>
		/// Count of additional runs performed after updating source annotations.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>The count of additional runs performed after updating the limits annotations.</value>
		public int AdditionalRerunsIfAnnotationsUpdated { get; set; }
		#endregion

		#region Parsing competition target info
		/// <summary>
		/// Refills the competition targets collection and fills competition limits from the <see cref="PreviousRunLogUri"/>.
		/// </summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void FillCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary, CompetitionState competitionState)
		{
			base.FillCompetitionTargets(competitionTargets, summary, competitionState);

			if (string.IsNullOrEmpty(PreviousRunLogUri))
				return;

			var xmlAnnotationDocs = ReadXmlAnnotationDocsFromLog(summary, competitionState);
			if (xmlAnnotationDocs.Length == 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					$"No XML annotation documents in the log '{PreviousRunLogUri}'.");

				return;
			}

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (TryFillCompetitionTargetsFromLog(competitionTargets, xmlAnnotationDocs, competitionState))
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Competition limits were updated from log file '{PreviousRunLogUri}'.");
			}
			else
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Competition limits do not require update. Log file: '{PreviousRunLogUri}'.");
			}
		}

		private XDocument[] ReadXmlAnnotationDocsFromLog(Summary summary, CompetitionState competitionState)
		{
			competitionState.Logger.WriteLineInfo(
				$"{LogVerbosePrefix} Reading XML annotation documents from log '{PreviousRunLogUri}'.");

			var xmlAnnotationDocs = _annotationsCacheSlot[summary].GetOrAdd(
				PreviousRunLogUri,
				uri => XmlAnnotations.TryParseXmlAnnotationDocsFromLog(uri, competitionState));

			return xmlAnnotationDocs;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private bool TryFillCompetitionTargetsFromLog(
			CompetitionTargets competitionTargets, XDocument[] xmlAnnotationDocs, CompetitionState competitionState)
		{
			competitionState.Logger.WriteLineInfo(
				$"{LogVerbosePrefix} Parsing XML annotations ({xmlAnnotationDocs.Length} doc(s)) from log '{PreviousRunLogUri}'.");

			bool updated = false;
			foreach (var competitionTarget in competitionTargets.Values)
			{
				bool hasAnnotations = false;

				foreach (var doc in xmlAnnotationDocs)
				{
					var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(competitionTarget.Target, doc, competitionState);
					if (parsedLimit != null)
					{
						hasAnnotations = true;
						updated |= competitionTarget.UnionWith(parsedLimit);
					}
				}

				if (!hasAnnotations)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						$"No logged XML annotation for {competitionTarget.Target.MethodTitle} found. Check if the method was renamed.");
				}
			}

			return updated;
		}
		#endregion

		#region Checking competition limits
		/// <summary>Check competition target limits.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="warnings">The warnings.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected override bool CheckCompetitionTargetLimits(
			CompetitionTarget competitionTarget,
			Benchmark[] benchmarksForTarget,
			Summary summary,
			CompetitionState competitionState,
			List<IWarning> warnings)
		{
			var checkPassed = base.CheckCompetitionTargetLimits(
				competitionTarget, benchmarksForTarget,
				summary, competitionState,
				warnings);

			if (checkPassed || competitionState.RunNumber <= SkipRunsBeforeApplyingAnnotations)
				return checkPassed;

			foreach (var benchmark in benchmarksForTarget)
			{
				if (benchmark.Target.Baseline)
					continue;

				var limit = CompetitionLimitProvider.TryGetCompetitionLimit(benchmark, summary);
				if (limit == null)
				{
					// No warnings required. Missing values should be checked by base CheckCompetitionTargetLimits() method.
					continue;
				}

				competitionTarget.UnionWith(limit);
			}

			return false;
		}

		/// <summary>Called after limits check completed.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="checkPassed"><c>true</c> if competition check passed.</param>
		protected override void OnLimitsCheckCompleted(Summary summary, CompetitionState competitionState, bool checkPassed)
		{
			AnnotateTargets(summary, competitionState);

			if (AdditionalRerunsIfAnnotationsUpdated <= 0 ||
				competitionState.RunNumber <= SkipRunsBeforeApplyingAnnotations)
			{
				base.OnLimitsCheckCompleted(summary, competitionState, checkPassed);
			}
			else if (!checkPassed)
			{
				competitionState.RequestReruns(
					AdditionalRerunsIfAnnotationsUpdated,
					"Annotations were updated.");
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void AnnotateTargets(Summary summary, CompetitionState competitionState)
		{
			var competitionTargets = TargetsSlot[summary];
			var targetsToAnnotate = competitionTargets.Values
				.Where(t => t.HasUnsavedChanges)
				.ToArray();

			if (targetsToAnnotate.Length == 0)
				return;

			if (competitionState.HasCriticalErrorsInRun)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					"Source annotation skipped as there are critical errors in the run. Check the log please.");
				return;
			}

			var annotatedTargets = SourceAnnotationsHelper.TryAnnotateBenchmarkFiles(
				targetsToAnnotate, competitionState);

			if (annotatedTargets.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					"The sources were updated with new annotations. Please check them before commiting the changes.");
			}

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.MarkAsSaved();
			}
		}
		#endregion
	}
}