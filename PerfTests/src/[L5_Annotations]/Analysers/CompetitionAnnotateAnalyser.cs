using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

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
		#region Static members
		private sealed class LoggedXmlAnnotations : Dictionary<string, XDocument[]> { }

		// DONTTOUCH: Each run should use results from previous run log.
		// Some testrunners reuse appdomains for multiple test runs.
		// So static variable cache should be cleared after each test run.
		// Run state cache does exactly this.
		private static readonly RunState<LoggedXmlAnnotations> _annotationsFromLogCacheSlot =
			new RunState<LoggedXmlAnnotations>();
		#endregion

		#region PrepareTargets
		/// <summary>
		/// Fills competition targets collection.
		/// Adds support for <see cref="SourceAnnotationsMode.PreviousRunLogUri"/>.
		/// </summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected override void OnPrepareTargets(CompetitionAnalysis analysis)
		{
			base.OnPrepareTargets(analysis);

			if (!analysis.SafeToContinue)
				return;

			var annotationsMode = analysis.Annotations;
			var logUri = annotationsMode.PreviousRunLogUri;

			if (!annotationsMode.AdjustLimits || string.IsNullOrEmpty(logUri))
				return;

			var xmlAnnotationDocs = ReadXmlAnnotationDocsFromLog(logUri, analysis);
			if (xmlAnnotationDocs.Length == 0 || !analysis.SafeToContinue)
				return;

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (TryFillCompetitionTargetsFromLog(analysis, xmlAnnotationDocs))
			{
				analysis.WriteInfoMessage($"Competition limits were updated from log file '{logUri}'.");
			}
			else if (analysis.SafeToContinue)
			{
				analysis.WriteInfoMessage($"Competition limits do not require update. Log file: '{logUri}'.");
			}
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private XDocument[] ReadXmlAnnotationDocsFromLog(string logUri, CompetitionAnalysis analysis)
		{
			analysis.RunState.WriteVerbose($"Reading XML annotation documents from log '{logUri}'.");

			var xmlAnnotationDocs = _annotationsFromLogCacheSlot[analysis.Summary].GetOrAdd(
				logUri,
				uri => XmlAnnotations.TryParseXmlAnnotationDocsFromLog(uri, analysis.RunState));

			if (xmlAnnotationDocs == null)
			{
				return Array<XDocument>.Empty;
			}
			if (xmlAnnotationDocs.Length == 0 && analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage($"No XML annotation documents in the log '{logUri}'.");
			}

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
					var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(
						competitionTarget.Target, doc, true, analysis.RunState);
					if (parsedLimit != null)
					{
						hasAnnotations = true;
						updated |= competitionTarget.UnionWith(parsedLimit.GetValueOrDefault());
					}
				}

				if (!hasAnnotations && analysis.SafeToContinue)
				{
					analysis.WriteWarningMessage(
						$"No logged XML annotation for {competitionTarget.Target.MethodDisplayInfo} found. Check if the method was renamed.");
				}
			}

			return updated;
		}
		#endregion

		#region CheckTargets
		private bool SkipAnnotation(CompetitionAnalysis analysis) =>
			!analysis.Annotations.AdjustLimits ||
				analysis.RunState.RunNumber < analysis.Annotations.AnnotateSourcesOnRun;

		/// <summary>Check competition target limits.</summary>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="analysis">Analyser pass results.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected override bool OnCheckTarget(
			Benchmark[] benchmarksForTarget,
			CompetitionTarget competitionTarget,
			CompetitionAnalysis analysis)
		{
			var checkPassed = base.OnCheckTarget(benchmarksForTarget, competitionTarget, analysis);

			if (competitionTarget.Baseline)
				return checkPassed;

			if (checkPassed && !competitionTarget.Limits.IsEmpty)
				return true;

			if (SkipAnnotation(analysis))
				return checkPassed;

			var limitProvider = analysis.Limits.LimitProvider;
			foreach (var benchmark in benchmarksForTarget)
			{
				var limit = limitProvider.TryGetCompetitionLimit(benchmark, analysis.Summary);
				if (limit.IsEmpty)
					// No warnings required. Missing values should be checked by base implementation.
					continue;

				competitionTarget.UnionWith(limit);
			}

			return false;
		}
		#endregion

		#region CompleteCheckTargets
		/// <summary>Called after limits check completed.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected override void OnCompleteCheckTargets(CompetitionAnalysis analysis)
		{
			AnnotateTargets(analysis);

			base.OnCompleteCheckTargets(analysis);
		}

		/// <summary>Requests reruns for the competition.</summary>
		/// <param name="analysis">Analyser pass results.</param>
		protected override void RequestReruns(CompetitionAnalysis analysis)
		{
			var annotationsMode = analysis.Annotations;
			if (SkipAnnotation(analysis) || annotationsMode.AdditionalRerunsIfAnnotationsUpdated <= 0)
			{
				base.RequestReruns(analysis);
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
			// TODO: messaging?
			if (!analysis.SafeToContinue)
			{
				analysis.WriteWarningMessage(
					"Source annotation skipped as there are critical errors in the run. Check the log please.");
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
				foreach (var competitionTarget in targetsToAnnotate)
				{
					competitionTarget.MarkAsSaved();
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

				foreach (var competitionTarget in annotatedTargets)
				{
					competitionTarget.MarkAsSaved();
				}
			}
		}
		#endregion
	}
}