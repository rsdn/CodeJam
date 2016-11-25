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
using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	// TODO: rename to adjust limits analyser?
	/// <summary>Competition analyser that updates source annotations if competition limit checking failed.</summary>
	/// <seealso cref="CompetitionAnalyser"/>
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
		/// Refills the competition targets collection and fills competition limits from the
		/// <see cref="SourceAnnotationsMode.PreviousRunLogUri"/>.
		/// </summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected override void FillCompetitionTargets(CompetitionAnalysis analysis)
		{
			base.FillCompetitionTargets(analysis);

			var annotationsMode = analysis.Annotations;
			var logUri = annotationsMode.PreviousRunLogUri;

			if (!annotationsMode.AdjustLimits || string.IsNullOrEmpty(logUri))
				return;

			var xmlAnnotationDocs = ReadXmlAnnotationDocsFromLog(logUri, analysis);
			if (xmlAnnotationDocs.Length == 0)
			{
				analysis.WriteWarningMessage($"No XML annotation documents in the log '{logUri}'.");
				return;
			}

			if (TryFillCompetitionTargetsFromLog(analysis, xmlAnnotationDocs))
			{
				analysis.WriteInfoMessage($"Competition limits were updated from log file '{logUri}'.");
				return;
			}

			analysis.WriteInfoMessage($"Competition limits do not require update. Log file: '{logUri}'.");
		}

		[NotNull]
		private XDocument[] ReadXmlAnnotationDocsFromLog(string logUri, CompetitionAnalysis analysis)
		{
			analysis.RunState.WriteVerbose($"Reading XML annotation documents from log '{logUri}'.");

			var xmlAnnotationDocs = _annotationsFromLogCacheSlot[analysis.Summary].GetOrAdd(
				logUri,
				uri => XmlAnnotations.TryParseXmlAnnotationDocsFromLog(uri, analysis.RunState));

			return xmlAnnotationDocs;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private bool TryFillCompetitionTargetsFromLog(CompetitionAnalysis analysis, XDocument[] xmlAnnotationDocs)
		{
			analysis.RunState.WriteVerbose($"Parsing XML annotations ({xmlAnnotationDocs.Length} doc(s)) from log.");

			var updated = false;
			foreach (var competitionTarget in analysis.Targets)
			{
				var hasAnnotations = false;

				foreach (var doc in xmlAnnotationDocs)
				{
					var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(competitionTarget.Target, doc, analysis.RunState);
					if (parsedLimit != null)
					{
						hasAnnotations = true;
						updated |= competitionTarget.UnionWith(parsedLimit);
					}
				}

				if (!hasAnnotations)
				{
					analysis.WriteWarningMessage(
						$"No logged XML annotation for {competitionTarget.Target.MethodDisplayInfo} found. Check if the method was renamed.");
				}
			}

			return updated;
		}
		#endregion

		#region Checking competition limits
		private bool SkipAnnotation(bool checkPassed, CompetitionAnalysis analysis) =>
			checkPassed ||
			!analysis.Annotations.AdjustLimits ||
			analysis.RunState.RunNumber < analysis.Annotations.AnnotateSourcesOnRun;

		/// <summary>Check competition target limits.</summary>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="analysis">Analyser pass results.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected override bool CheckTargetLimits(
			Benchmark[] benchmarksForTarget,
			CompetitionTarget competitionTarget,
			CompetitionAnalysis analysis)
		{
			var checkPassed = base.CheckTargetLimits(benchmarksForTarget, competitionTarget, analysis);

			// TODO better 4 empty.
			checkPassed &= !competitionTarget.CheckLimits || !competitionTarget.IsEmpty;
			if (SkipAnnotation(checkPassed, analysis))
			{
				return checkPassed;
			}

			var limitProvider = analysis.Limits.LimitProvider;
			foreach (var benchmark in benchmarksForTarget)
			{
				if (benchmark.Target.Baseline)
					continue;

				var limit = limitProvider.TryGetCompetitionLimit(benchmark, analysis.Summary);
				if (limit == null)
					// No warnings required. Missing values should be checked by base implementation.
					continue;

				competitionTarget.UnionWith(limit);
			}

			return false;
		}

		/// <summary>Called after limits check completed.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		/// <param name="checkPassed"><c>true</c> if competition check passed.</param>
		protected override void OnLimitsCheckCompleted(CompetitionAnalysis analysis, bool checkPassed)
		{
			// TODO: at limit checking???
			AnnotateTargets(analysis);

			var annotationsMode = analysis.Annotations;
			if (SkipAnnotation(checkPassed, analysis) ||
				annotationsMode.AdditionalRerunsIfAnnotationsUpdated <= 0)
			{
				base.OnLimitsCheckCompleted(analysis, checkPassed);
			}
			else
			{
				analysis.RunState.RequestReruns(
					annotationsMode.AdditionalRerunsIfAnnotationsUpdated,
					"Limits were adjusted.");
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void AnnotateTargets(CompetitionAnalysis analysis)
		{
			// TODO: move to check limits.
			if (!analysis.Passed)
			{
				analysis.WriteWarningMessage("Source annotation skipped as there are critical errors in the run. Check the log please.");
				return;
			}

			var targetsToAnnotate = analysis.Targets
				.Where(t => t.HasUnsavedChanges)
				.ToArray();

			if (targetsToAnnotate.Length == 0)
				return;

			if (analysis.Annotations.DontSaveAdjustedLimits)
			{
				var message =
					$"Source annotation skipped due to {SourceAnnotationsMode.DontSaveAdjustedLimitsCharacteristic.FullId} setting.";
				if (analysis.RunState.FirstRun)
				{
					analysis.RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.Informational, message);
				}
				else
				{
					analysis.RunState.WriteVerbose(message);
				}
			}
			else
			{
				var annotatedTargets = SourceAnnotationsHelper.TryAnnotateBenchmarkFiles(
					targetsToAnnotate, analysis.RunState);

				if (annotatedTargets.Any())
				{
					analysis.WriteWarningMessage(
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