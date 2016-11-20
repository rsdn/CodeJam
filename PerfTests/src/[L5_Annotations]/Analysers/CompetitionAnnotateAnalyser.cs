using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

namespace CodeJam.PerfTests.Analysers
{
	// TODO: rename to adjust limits analyser?
	/// <summary>Competition analyser that updates source annotations if competition limit checking failed.</summary>
	/// <seealso cref="CompetitionAnalyser"/>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	internal class CompetitionAnnotateAnalyser : CompetitionAnalyser
	{
		#region Cached XML annotations from log
		// DONTTOUCH: Each run should use results from previous run log.
		// DO NOT cache log output in static field
		// as test can be run multiple times in same appdomain.
		private sealed class LoggedXmlAnnotations : Dictionary<string, XDocument[]> { }

		private static readonly RunState<LoggedXmlAnnotations> _annotationsFromLogCacheSlot =
			new RunState<LoggedXmlAnnotations>();
		#endregion

		#region Parsing competition target info
		/// <summary>
		/// Refills the competition targets collection and fills competition limits from the <see cref="SourceAnnotationsMode.PreviousRunLogUri"/>.
		/// </summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		protected override void FillCompetitionTargets(
			CompetitionTargets competitionTargets, Summary summary, CompetitionState competitionState)
		{
			base.FillCompetitionTargets(competitionTargets, summary, competitionState);

			var annotationsMode = competitionState.Options.SourceAnnotations;
			var logUri = annotationsMode.PreviousRunLogUri;

			if (!annotationsMode.AdjustLimits || string.IsNullOrEmpty(logUri))
				return;

			var xmlAnnotationDocs = ReadXmlAnnotationDocsFromLog(logUri, summary, competitionState);
			if (xmlAnnotationDocs.Length == 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					$"No XML annotation documents in the log '{logUri}'.");

				return;
			}

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (TryFillCompetitionTargetsFromLog(competitionTargets, xmlAnnotationDocs, competitionState))
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Competition limits were updated from log file '{logUri}'.");
			}
			else
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Competition limits do not require update. Log file: '{logUri}'.");
			}
		}

		private XDocument[] ReadXmlAnnotationDocsFromLog(string logUri, Summary summary, CompetitionState competitionState)
		{
			competitionState.WriteVerbose(
				$"Reading XML annotation documents from log '{logUri}'.");

			var xmlAnnotationDocs = _annotationsFromLogCacheSlot[summary].GetOrAdd(
				logUri,
				uri => XmlAnnotations.TryParseXmlAnnotationDocsFromLog(uri, competitionState));

			return xmlAnnotationDocs;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private bool TryFillCompetitionTargetsFromLog(
			CompetitionTargets competitionTargets, XDocument[] xmlAnnotationDocs, CompetitionState competitionState)
		{
			competitionState.WriteVerbose(
				$"Parsing XML annotations ({xmlAnnotationDocs.Length} doc(s)) from log.");

			bool updated = false;
			foreach (var competitionTarget in competitionTargets.Targets)
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
						$"No logged XML annotation for {competitionTarget.Target.MethodDisplayInfo} found. Check if the method was renamed.");
				}
			}

			return updated;
		}
		#endregion

		#region Checking competition limits
		private static bool SkipAnnotation(bool checkPassed, CompetitionState competitionState)
		{
			var annotationsMode = competitionState.Options.SourceAnnotations;

			return checkPassed ||
				!annotationsMode.AdjustLimits ||
				competitionState.RunNumber < annotationsMode.AnnotateSourcesOnRun;
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="conclusions">The warnings.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected override bool CheckCompetitionTargetLimits(
			CompetitionTarget competitionTarget,
			Benchmark[] benchmarksForTarget,
			Summary summary,
			CompetitionState competitionState,
			List<Conclusion> conclusions)
		{
			var checkPassed = base.CheckCompetitionTargetLimits(
				competitionTarget, benchmarksForTarget,
				summary, competitionState,
				conclusions);

			if (SkipAnnotation(checkPassed, competitionState))
			{
				return checkPassed;
			}

			foreach (var benchmark in benchmarksForTarget)
			{
				if (benchmark.Target.Baseline)
					continue;

				var limitsMode = competitionState.Options.Limits;
				var limit = limitsMode.LimitProvider.TryGetCompetitionLimit(benchmark, summary);
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
			var annotationsMode = competitionState.Options.SourceAnnotations;
			AnnotateTargets(summary, competitionState);

			if (SkipAnnotation(checkPassed, competitionState) ||
				annotationsMode.AdditionalRerunsIfAnnotationsUpdated <= 0)
			{
				base.OnLimitsCheckCompleted(summary, competitionState, checkPassed);
			}
			else
			{
				competitionState.RequestReruns(
					annotationsMode.AdditionalRerunsIfAnnotationsUpdated,
					"Limits were adjusted.");
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void AnnotateTargets(Summary summary, CompetitionState competitionState)
		{
			var competitionTargets = TargetsSlot[summary];
			var targetsToAnnotate = competitionTargets.Targets
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

			if (competitionState.Options.SourceAnnotations.DontSaveAdjustedLimits)
			{
				var message =
					$"Source annotation skipped due to {SourceAnnotationsMode.DontSaveAdjustedLimitsCharacteristic.FullId} setting.";
				if (competitionState.FirstRun)
				{
					competitionState.WriteMessage(MessageSource.Analyser, MessageSeverity.Informational, message);
				}
				else
				{
					competitionState.WriteVerbose(message);
				}
			}
			else
			{
				var annotatedTargets = SourceAnnotationsHelper.TryAnnotateBenchmarkFiles(
					targetsToAnnotate, competitionState);

				if (annotatedTargets.Any())
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						"The sources were updated with new annotations. Please check them before commiting the changes.");
				}
			}

			foreach (var competitionTarget in targetsToAnnotate)
			{
				competitionTarget.MarkAsSaved();
			}
		}
		#endregion
	}
}