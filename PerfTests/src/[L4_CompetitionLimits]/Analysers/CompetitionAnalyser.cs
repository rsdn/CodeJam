using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.CompetitionLimitProviders;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal class CompetitionAnalyser : IAnalyser
	{
		#region Competition targets
		/// <summary>Storage class for competition targets.</summary>
		protected class CompetitionTargets : Dictionary<MethodInfo, CompetitionTarget> { }

		/// <summary>Run state slot for the competition targets.</summary>
		protected static readonly RunState<CompetitionTargets> TargetsSlot =
			new RunState<CompetitionTargets>();
		#endregion

		#region Properties
		/// <summary>The analyser should ignore existing limit annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing limit annotations.</value>
		public bool IgnoreExistingAnnotations { get; set; }

		/// <summary>Timing limit to detect too fast benchmarks.</summary>
		/// <value>The timing limit to detect too fast benchmarks.</value>
		public TimeSpan TooFastBenchmarkLimit { get; set; }

		/// <summary>Timing limit to detect long-running benchmarks.</summary>
		/// <value>The timing limit to detect long-running benchmarks.</value>
		public TimeSpan LongRunningBenchmarkLimit { get; set; }

		/// <summary>
		/// Maximum count of retries performed if the limit checking failed.
		/// Set this to greater than one to detect case when limits are too tight and the benchmark fails occasionally.
		/// </summary>
		/// <value>Maximum count of retries performed if the validation failed.</value>
		public int MaxRerunsIfValidationFailed { get; set; }

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits { get; set; }

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider..</value>
		public ICompetitionLimitProvider CompetitionLimitProvider { get; set; }
		#endregion

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<IWarning> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];
			var warnings = new List<IWarning>();

			if (CheckPreconditions(summary))
			{
				bool hasTargets = competitionTargets.Any() ||
					FillCompetitionTargets(competitionTargets, summary);

				if (hasTargets)
				{
					CheckCompetitionLimits(summary, warnings);

					CheckPostconditions(summary, warnings);
				}
			}

			if (competitionState.LooksLikeLastRun && LogCompetitionLimits)
			{
				XmlAnnotations.LogCompetitionTargets(competitionTargets.Values, competitionState);
			}

			return warnings.ToArray();
		}

		#region Pre- & postconditions
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private bool CheckPreconditions(Summary summary)
		{
			var competitionState = CompetitionCore.RunState[summary];
			if (summary.HasCriticalValidationErrors)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"Summary has validation errors.");
			}

			var benchmarksWithReports = summary.Reports
				.Where(r => r.ExecuteResults.Any())
				.Select(r => r.Benchmark);

			var benchMissing = summary.GetSummaryOrderBenchmarks()
				.Except(benchmarksWithReports)
				.Select(b => b.Target.MethodTitle)
				.Distinct()
				.ToArray();

			if (benchMissing.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
			}

			if (CompetitionLimitProvider == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					$"The {nameof(CompetitionLimitProvider)} should be not null.");
			}

			return !competitionState.HasCriticalErrorsInRun;
		}

		private void CheckPostconditions(Summary summary, List<IWarning> warnings)
		{
			var culture = EnvironmentInfo.MainCultureInfo;
			var competitionState = CompetitionCore.RunState[summary];

			if (TooFastBenchmarkLimit > TimeSpan.Zero)
			{
				var tooFastReports = GetReportNames(
					summary,
					r => r.GetAverageNanoseconds() < TooFastBenchmarkLimit.TotalNanoseconds());

				if (tooFastReports.Any())
				{
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.Warning,
						"The benchmarks " + string.Join(", ", tooFastReports) +
							$" run faster than {TooFastBenchmarkLimit.TotalMilliseconds.ToString(culture)}ms. Results cannot be trusted.");
				}
			}

			if (LongRunningBenchmarkLimit > TimeSpan.Zero)
			{
				var tooSlowReports = GetReportNames(
					summary,
					r => r.GetAverageNanoseconds() > LongRunningBenchmarkLimit.TotalNanoseconds());

				if (tooSlowReports.Any())
				{
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.Warning,
						"The benchmarks " + string.Join(", ", tooSlowReports) +
							$" run longer than {LongRunningBenchmarkLimit.TotalSeconds.ToString(culture)}s." +
							" Consider to rewrite the test as the peek timings will be hidden by averages" +
							" or enable long running benchmarks support in the config.");
				}
			}

			if (warnings.Count == 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"{GetType().Name}: All competition limits are ok.");
			}
		}

		private string[] GetReportNames(Summary summary, Func<Measurement, bool> measurementFilter) =>
			summary.GetSummaryOrderBenchmarks()
				.Select(b => summary[b])
				.Where(rp => rp.GetResultRuns().Any(measurementFilter))
				.Select(rp => rp.Benchmark.Target.MethodTitle)
				.Distinct()
				.ToArray();
		#endregion

		#region Parsing competition target info
		/// <summary>Refills the competition targets collection.</summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		protected virtual bool FillCompetitionTargets(
			CompetitionTargets competitionTargets,
			Summary summary)
		{
			competitionTargets.Clear();

			var competitionState = CompetitionCore.RunState[summary];

			// DONTTOUCH: DO NOT add return into the if clause.
			// The competitionTargets should be filled with empty limits if IgnoreExistingAnnotations set to false
			if (IgnoreExistingAnnotations)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser,
					MessageSeverity.Informational,
					$"Existing benchmark limits are ignored due to {nameof(IgnoreExistingAnnotations)} setting.");
			}

			var xmlAnnotationDocs = new Dictionary<string, XDocument>();
			bool hasBaseline = false;

			var targets = summary.GetExecutionOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct();

			foreach (var target in targets)
			{
				hasBaseline |= target.Baseline;

				var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
				if (competitionAttribute != null &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(
						target,
						competitionAttribute,
						xmlAnnotationDocs,
						competitionState);

					competitionTargets.Add(target.Method, competitionTarget);
				}
			}

			if (!hasBaseline && competitionTargets.Count > 0)
			{
				var target = summary.Benchmarks[0].Target;
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"The benchmark {target.Type.Name} has no baseline.");
			}

			return !competitionState.HasCriticalErrorsInRun;
		}

		private CompetitionTarget GetCompetitionTarget(
			Target target,
			CompetitionBenchmarkAttribute competitionAttribute,
			IDictionary<string, XDocument> xmlAnnotationDocs,
			CompetitionState competitionState)
		{
			var fallbackLimit = CompetitionLimit.Empty;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(target);
			if (competitionMetadata == null)
			{
				var limit = IgnoreExistingAnnotations
					? fallbackLimit
					: AttributeAnnotations.ParseCompetitionLimit(competitionAttribute);

				return new CompetitionTarget(target, limit);
			}
			else
			{
				// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
				var resourceDoc = xmlAnnotationDocs.GetOrAdd(
					competitionMetadata.MetadataResourceName,
					resourceName =>
						XmlAnnotations.TryParseXmlAnnotationDoc(
							target.Type.Assembly,
							resourceName,
							competitionState));

				var limit = fallbackLimit;

				if (resourceDoc != null && !IgnoreExistingAnnotations)
				{
					var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(
						target,
						resourceDoc,
						competitionState);

					if (parsedLimit == null)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Informational,
							$"XML anotations for {target.MethodTitle}: no annotation exists.");
					}
					else
					{
						limit = parsedLimit;
					}
				}

				return new CompetitionTarget(target, limit, true, competitionMetadata.MetadataResourcePath);
			}
		}
		#endregion

		#region Core logic
		/// <summary>Check competition limits.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="warnings">List of warnings for the benchmarks.</param>
		protected virtual void CheckCompetitionLimits(Summary summary, List<IWarning> warnings)
		{
			bool limitsAreFine = true;

			var competitionTargets = TargetsSlot[summary];

			var benchmarksByTarget = summary.GetExecutionOrderBenchmarks()
				.GroupBy(b => b.Target);
			foreach (var benchmarksForTarget in benchmarksByTarget)
			{
				var target = benchmarksForTarget.First().Target;

				CompetitionTarget competitionTarget;
				if (!competitionTargets.TryGetValue(target.Method, out competitionTarget))
					continue;

				limitsAreFine &= CheckCompetitionTargetLimits(
					competitionTarget, benchmarksForTarget, summary, warnings);
			}

			var competitionState = CompetitionCore.RunState[summary];
			if (!limitsAreFine && competitionState.RunNumber < MaxRerunsIfValidationFailed)
			{
				competitionState.RequestReruns(1, "Limit checking failed.");
			}
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="warnings">The warnings.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected virtual bool CheckCompetitionTargetLimits(
			CompetitionTarget competitionTarget,
			IEnumerable<Benchmark> benchmarksForTarget,
			Summary summary,
			List<IWarning> warnings)
		{
			bool succeed = true;
			
			foreach (var benchmark in benchmarksForTarget)
			{
				succeed &= CheckCompetitionLimitsCore(
					summary,
					benchmark,
					competitionTarget,
					warnings);
			}

			return succeed;
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private bool CheckCompetitionLimitsCore(
			Summary summary,
			Benchmark benchmark,
			CompetitionLimit competitionLimit,
			List<IWarning> warnings)
		{
			if (benchmark.Target.Baseline)
				return true;

			var competitionState = CompetitionCore.RunState[summary];

			var actualValues = CompetitionLimitProvider.TryGetActualValues(benchmark, summary);
			if (actualValues == null)
			{
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Could not obtain competition limits for {benchmark.ShortInfo}.",
					summary.TryGetBenchmarkReport(benchmark));

				return false;
			}

			if (!competitionLimit.CheckLimitsFor(actualValues))
			{
				var targetMethodTitle = benchmark.Target.MethodTitle;
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Method {targetMethodTitle} {actualValues} does not fit into limits {competitionLimit}",
					summary.TryGetBenchmarkReport(benchmark));

				return false;
			}

			return true;
		}
		#endregion
	}
}