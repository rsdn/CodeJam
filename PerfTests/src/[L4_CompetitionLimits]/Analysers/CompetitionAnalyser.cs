using System;
using System.Collections.Concurrent;
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
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	using ResourceKey = ValueTuple<Assembly, string, bool>;

	/// <summary>Basic competition analyser.</summary>
	/// <seealso cref="IAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal class CompetitionAnalyser : IAnalyser
	{
		#region Competition targets
		/// <summary>Storage class for competition targets.</summary>
		protected class CompetitionTargets : Dictionary<MethodInfo, CompetitionTarget>
		{
			/// <summary>Gets a value indicating whether the targets are initialized.</summary>
			/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
			public bool Initialized { get; private set; }

			/// <summary>Marks as initialized.</summary>
			public void SetInitialized() => Initialized = true;
		}

		/// <summary>Run state slot for the competition targets.</summary>
		protected static readonly RunState<CompetitionTargets> TargetsSlot =
			new RunState<CompetitionTargets>();
		#endregion

		#region Cached resources with XML annotations.
		private static readonly ConcurrentDictionary<ResourceKey, XDocument> _xmlAnnotationsCache =
			new ConcurrentDictionary<ResourceKey, XDocument>();
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
		/// Set this to non-zero positive value to detect case when limits are too tight and the benchmark fails occasionally.
		/// </summary>
		/// <value>Maximum count of retries performed if the validation failed.</value>
		public int MaxRerunsIfValidationFailed { get; set; }

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits { get; set; }

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
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

			if (CheckPreconditions(summary, competitionState))
			{
				AnalyseCore(summary, competitionTargets, competitionState, warnings);
			}

			if (competitionState.LooksLikeLastRun && LogCompetitionLimits)
			{
				XmlAnnotations.LogCompetitionTargets(competitionTargets.Values, competitionState);
			}

			return warnings.ToArray();
		}

		#region Analyse core
		private void AnalyseCore(
			Summary summary, CompetitionTargets competitionTargets,
			CompetitionState competitionState, List<IWarning> warnings)
		{
			if (competitionState.HasCriticalErrorsInRun)
				return;

			if (!TryInitialize(summary, competitionTargets, competitionState))
			{
				return;
			}

			if (competitionTargets.Any())
			{
				CheckCompetitionLimitsCore(summary, competitionState, warnings);
			}
			else
			{
				CheckPostconditions(summary, competitionState, warnings);
			}
		}

		private bool TryInitialize(Summary summary, CompetitionTargets competitionTargets, CompetitionState competitionState)
		{
			if (competitionTargets.Initialized)
				return true;

			FillCompetitionTargets(competitionTargets, summary, competitionState);
			competitionTargets.SetInitialized();

			return !competitionState.HasCriticalErrorsInRun;
		}

		private void CheckCompetitionLimitsCore(Summary summary, CompetitionState competitionState, List<IWarning> warnings)
		{
			var checkPassed = CheckCompetitionLimits(summary, competitionState, warnings);

			CheckPostconditions(summary, competitionState, warnings);

			OnLimitsCheckCompleted(summary, competitionState, checkPassed);

			if (warnings.Count == 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"{GetType().Name}: All competition limits are ok.");
			}
		}
		#endregion

		#region Pre- & postconditions
		private bool CheckPreconditions(Summary summary, CompetitionState competitionState)
		{
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
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"The {nameof(CompetitionLimitProvider)} should be not null.");
			}

			return !competitionState.HasCriticalErrorsInRun;
		}

		private void CheckPostconditions(Summary summary, CompetitionState competitionState, List<IWarning> warnings)
		{
			var culture = HostEnvironmentInfo.MainCultureInfo;

			if (TooFastBenchmarkLimit > TimeSpan.Zero)
			{
				var tooFastReports = GetReportNames(
					summary,
					r => r.GetAverageNanoseconds() < TooFastBenchmarkLimit.TotalNanoseconds());

				if (tooFastReports.Any())
				{
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.Warning,
						"The benchmark(s) " + string.Join(", ", tooFastReports) +
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
						"The benchmark(s) " + string.Join(", ", tooSlowReports) +
							$" run longer than {LongRunningBenchmarkLimit.TotalSeconds.ToString(culture)}s." +
							" Consider to rewrite the test as the peek timings will be hidden by averages" +
							" or enable long running benchmarks support in the config.");
				}
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
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
		/// <param name="competitionState">State of the run.</param>
		protected virtual void FillCompetitionTargets(
			CompetitionTargets competitionTargets,
			Summary summary,
			CompetitionState competitionState)
		{
			// DONTTOUCH: DO NOT add return into the if clause.
			// The competitionTargets should be filled with empty limits if IgnoreExistingAnnotations set to false
			if (IgnoreExistingAnnotations)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser,
					MessageSeverity.Informational,
					$"Existing benchmark limits are ignored due to {nameof(IgnoreExistingAnnotations)} setting.");
			}

			bool hasBaseline = false;
			competitionTargets.Clear();

			var targets = summary.GetExecutionOrderBenchmarks()
				.Select(b => b.Target)
				.ToHashSet();

			if (targets.Count == 0)
				return;

			var targetType = targets.First().Type;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(targetType);
			foreach (var target in targets)
			{
				hasBaseline |= target.Baseline;

				var competitionTarget = TryParseCompetitionTarget(target, competitionMetadata, competitionState);
				if (competitionTarget != null)
				{
					competitionTargets.Add(target.Method, competitionTarget);
				}
			}

			if (!hasBaseline && competitionTargets.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"The benchmark {targetType.Name} has no baseline.");
			}
		}

		[CanBeNull]
		private CompetitionTarget TryParseCompetitionTarget(
			Target target, CompetitionMetadata competitionMetadata,
			CompetitionState competitionState)
		{
			var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
			if (competitionAttribute == null)
				return null;

			var fallbackLimit = CompetitionLimit.Empty;

			CompetitionTarget result;
			if (competitionMetadata == null)
			{
				var limit = TryParseCompetitionLimit(competitionAttribute)
					?? fallbackLimit;

				result = new CompetitionTarget(target, limit, competitionAttribute.DoesNotCompete);
			}
			else
			{
				var limit = TryParseCompetitionLimit(target, competitionMetadata, competitionState)
					?? fallbackLimit;

				result = new CompetitionTarget(target, limit, competitionAttribute.DoesNotCompete, competitionMetadata);
			}

			return result;
		}

		private CompetitionLimit TryParseCompetitionLimit(
			CompetitionBenchmarkAttribute competitionAttribute) =>
				IgnoreExistingAnnotations
					? null
					: AttributeAnnotations.ParseCompetitionLimit(competitionAttribute);

		private CompetitionLimit TryParseCompetitionLimit(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionState competitionState)
		{
			CompetitionLimit result = null;

			// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
			var resourceKey = new ResourceKey(
				target.Type.Assembly,
				competitionMetadata.MetadataResourceName,
				competitionMetadata.UseFullTypeName);

			var xmlAnnotationDoc = _xmlAnnotationsCache.GetOrAdd(
				resourceKey,
				r => XmlAnnotations.TryParseXmlAnnotationDoc(r.Item1, r.Item2, r.Item3, competitionState));

			if (!IgnoreExistingAnnotations && xmlAnnotationDoc != null)
			{
				var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(
					target,
					xmlAnnotationDoc,
					competitionState);

				if (parsedLimit == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						$"No XML annotation for {target.MethodTitle} found. Check if the method was renamed.");
				}
				else
				{
					result = parsedLimit;
				}
			}

			return result;
		}
		#endregion

		#region Checking competition limits
		private bool CheckCompetitionLimits(Summary summary, CompetitionState competitionState, List<IWarning> warnings)
		{
			var result = true;
			var competitionTargets = TargetsSlot[summary];

			var benchmarksByTarget = summary.GetExecutionOrderBenchmarks()
				.GroupBy(b => b.Target)
				.Where(g => competitionTargets.ContainsKey(g.Key.Method));

			foreach (var benchmarks in benchmarksByTarget)
			{
				var competitionTarget = competitionTargets[benchmarks.Key.Method];

				result &= CheckCompetitionTargetLimits(
					competitionTarget, benchmarks.ToArray(),
					summary, competitionState,
					warnings);
			}

			return result;
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="warnings">The warnings.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected virtual bool CheckCompetitionTargetLimits(
			CompetitionTarget competitionTarget,
			Benchmark[] benchmarksForTarget,
			Summary summary,
			CompetitionState competitionState,
			List<IWarning> warnings)
		{
			if (competitionTarget.Target.Baseline || competitionTarget.DoesNotCompete)
				return true;

			var result = true;
			foreach (var benchmark in benchmarksForTarget)
			{
				result &= CheckCompetitionLimitsCore(
					benchmark,
					summary,
					competitionTarget,
					competitionState,
					warnings);
			}

			return result;
		}

		private bool CheckCompetitionLimitsCore(
			Benchmark benchmark,
			Summary summary,
			CompetitionLimit competitionLimit,
			CompetitionState competitionState,
			List<IWarning> warnings)
		{
			var actualValues = CompetitionLimitProvider.TryGetActualValues(benchmark, summary);
			if (actualValues == null)
			{
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.ExecutionError,
					$"Could not obtain competition limits for {benchmark.ShortInfo}.",
					summary.TryGetBenchmarkReport(benchmark));

				return true;
			}

			if (competitionLimit.CheckLimitsFor(actualValues))
				return true;

			var targetMethodTitle = benchmark.Target.MethodTitle;
			var message = competitionLimit.IsEmpty
				? $"Method {targetMethodTitle} {actualValues} has empty limit. Please fill it."
				: $"Method {targetMethodTitle} {actualValues} does not fit into limits {competitionLimit}.";

			competitionState.AddAnalyserWarning(
				warnings, MessageSeverity.TestError, message, summary.TryGetBenchmarkReport(benchmark));

			return false;
		}

		/// <summary>Called after limits check completed.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="checkPassed"><c>true</c> if competition check passed.</param>
		protected virtual void OnLimitsCheckCompleted(
			Summary summary,
			CompetitionState competitionState, bool checkPassed)
		{
			if (checkPassed)
				return;

			if (competitionState.RunNumber < MaxRerunsIfValidationFailed)
			{
				competitionState.RequestReruns(1, "Limit checking failed.");
			}
		}
		#endregion
	}
}