using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
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
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	internal class CompetitionAnalyser : IAnalyser
	{
		#region Competition targets
		/// <summary>Storage class for competition targets.</summary>
		protected sealed class CompetitionTargets
		{
			private readonly Dictionary<MethodInfo, CompetitionTarget> _targets = new Dictionary<MethodInfo, CompetitionTarget>();

			/// <summary>Gets the <see cref="CompetitionTarget"/> for the specified target.</summary>
			/// <value>The <see cref="CompetitionTarget"/>.</value>
			/// <param name="target">The target.</param>
			/// <returns>Competition target</returns>
			[CanBeNull]
			public CompetitionTarget this[Target target] => _targets.GetValueOrDefault(target.Method);

			/// <summary>Returns all targets.</summary>
			/// <value>The targets.</value>
			public IReadOnlyCollection<CompetitionTarget> Targets => _targets.Values;

			/// <summary>Gets a value indicating whether the targets are initialized.</summary>
			/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
			public bool Initialized { get; private set; }

			/// <summary>Gets a value indicating whether there are any targets.</summary>
			/// <value><c>true</c> if there are any targets; otherwise, <c>false</c>.</value>
			public bool HasTargets => _targets.Count > 0;

			/// <summary>Clears this instance.</summary>
			public void Clear()
			{
				_targets.Clear();
				Initialized = false;
			}

			/// <summary>Adds the specified competition target.</summary>
			/// <param name="competitionTarget">The competition target.</param>
			public void Add(CompetitionTarget competitionTarget) =>
				_targets.Add(competitionTarget.Target.Method, competitionTarget);

			/// <summary>Marks as initialized.</summary>
			public void SetInitialized() => Initialized = true;
		}

		/// <summary>Run state slot for the competition targets.</summary>
		protected static readonly RunState<CompetitionTargets> TargetsSlot =
			new RunState<CompetitionTargets>();
		#endregion

		#region Cached resources with XML annotations.
		// DONTTOUCH: DO NOT replace with Memoize as the value reading depends on CompetitionState
		// and should be called from callee thread only.
		private static readonly ConcurrentDictionary<ResourceKey, XDocument> _xmlAnnotationsCache =
			new ConcurrentDictionary<ResourceKey, XDocument>();
		#endregion

		#region Properties
		/// <summary>Returns the identifier of the analyser.</summary>
		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;
		#endregion

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			var competitionState = CompetitionCore.RunState[summary];
			var limitsMode = competitionState.Options.Limits;
			var competitionTargets = TargetsSlot[summary];
			var conclusions = new List<Conclusion>();

			if (CheckPreconditions(summary, competitionState))
			{
				AnalyseCore(summary, competitionTargets, competitionState, conclusions);
			}

			if (competitionState.LooksLikeLastRun && limitsMode.LogAnnotations)
			{
				XmlAnnotations.LogXmlAnnotationDoc(competitionTargets.Targets, competitionState);
			}

			return conclusions.ToArray();
		}

		#region Analyse core
		private void AnalyseCore(
			Summary summary, CompetitionTargets competitionTargets,
			CompetitionState competitionState, List<Conclusion> conclusions)
		{
			if (competitionState.HasCriticalErrorsInRun)
				return;

			if (!TryInitialize(summary, competitionTargets, competitionState))
				return;

			var checkPassed = CheckCompetitionLimits(summary, competitionTargets, competitionState, conclusions);

			CheckPostconditions(summary, competitionState, conclusions);

			OnLimitsCheckCompleted(summary, competitionState, checkPassed);

			if (conclusions.Count == 0 && competitionTargets.Targets.Any(c => c.CheckLimiths))
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"{GetType().Name}: All competition limits are ok.");
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
		#endregion

		#region Pre- & postconditions
		private bool CheckPreconditions(Summary summary, CompetitionState competitionState)
		{
			// DONTTOUCH: DO NOT add return into the if clause.
			// All preconditions should be checked.
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
				.Select(b => b.Target.MethodDisplayInfo)
				.Distinct()
				.ToArray();

			if (benchMissing.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
			}

			var limitsMode = competitionState.Options.Limits;
			if (limitsMode.LimitProvider == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"The {CompetitionLimitsMode.LimitProviderCharacteristic.FullId} should be not null.");
			}

			return !competitionState.HasCriticalErrorsInRun;
		}

		private void CheckPostconditions(Summary summary, CompetitionState competitionState, List<Conclusion> conclusions)
		{
			var culture = HostEnvironmentInfo.MainCultureInfo;
			var limitsMode = competitionState.Options.Limits;

			if (limitsMode.TooFastBenchmarkLimit > TimeSpan.Zero)
			{
				var tooFastReports = GetReportNames(
					summary,
					r => r.Nanoseconds < limitsMode.TooFastBenchmarkLimit.TotalNanoseconds());

				if (tooFastReports.Any())
				{
					competitionState.AddAnalyserConclusion(
						this, conclusions, MessageSeverity.Warning,
						"The benchmark(s) " + string.Join(", ", tooFastReports) +
							$" run faster than {limitsMode.TooFastBenchmarkLimit.TotalMilliseconds.ToString(culture)} ms. Results cannot be trusted.");
				}
			}

			if (limitsMode.LongRunningBenchmarkLimit > TimeSpan.Zero)
			{
				var tooSlowReports = GetReportNames(
					summary,
					r => r.Nanoseconds > limitsMode.LongRunningBenchmarkLimit.TotalNanoseconds());

				if (tooSlowReports.Any())
				{
					competitionState.AddAnalyserConclusion(
						this, conclusions, MessageSeverity.Warning,
						"The benchmark(s) " + string.Join(", ", tooSlowReports) +
							$" run longer than {limitsMode.LongRunningBenchmarkLimit.TotalSeconds.ToString(culture)} sec." +
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
				.Select(rp => rp.Benchmark.Target.MethodDisplayInfo)
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
			var limitsMode = competitionState.Options.Limits;
			if (limitsMode.IgnoreExistingAnnotations)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser,
					MessageSeverity.Informational,
					$"Existing benchmark limits are ignored due to {CompetitionLimitsMode.IgnoreExistingAnnotationsCharacteristic.FullId} setting.");
			}

			bool hasBaseline = false;
			competitionTargets.Clear();

			var targets = summary.GetExecutionOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct()
				.ToArray();

			if (targets.Length == 0)
				return;

			var targetType = targets[0].Type;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(targetType);
			foreach (var target in targets)
			{
				hasBaseline |= target.Baseline;

				var competitionTarget = TryParseCompetitionTarget(target, competitionMetadata, competitionState);
				if (competitionTarget != null)
				{
					competitionTargets.Add(competitionTarget);
				}
			}

			if (!hasBaseline && competitionTargets.HasTargets)
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
			var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>(true);
			if (competitionAttribute == null)
				return null;

			var fallbackLimit = CompetitionLimit.Empty;

			CompetitionTarget result;
			if (competitionMetadata == null)
			{
				var limit = TryParseCompetitionLimit(competitionAttribute, competitionState.Options)
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
			CompetitionBenchmarkAttribute competitionAttribute,
			CompetitionOptions competitionOptions) =>
				competitionOptions.Limits.IgnoreExistingAnnotations
					? null
					: AttributeAnnotations.ParseCompetitionLimit(competitionAttribute);

		private CompetitionLimit TryParseCompetitionLimit(
			Target target,
			CompetitionMetadata competitionMetadata,
			CompetitionState competitionState)
		{
			CompetitionLimit result = null;

			var limitsMode = competitionState.Options.Limits;

			// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
			var resourceKey = new ResourceKey(
				target.Type.Assembly,
				competitionMetadata.MetadataResourceName,
				competitionMetadata.UseFullTypeName);

			var xmlAnnotationDoc = _xmlAnnotationsCache.GetOrAdd(
				resourceKey,
				r => XmlAnnotations.TryParseXmlAnnotationDoc(r.Item1, r.Item2, r.Item3, competitionState));

			if (!limitsMode.IgnoreExistingAnnotations && xmlAnnotationDoc != null)
			{
				var parsedLimit = XmlAnnotations.TryParseCompetitionLimit(
					target,
					xmlAnnotationDoc,
					competitionState);

				if (parsedLimit == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						$"No XML annotation for {target.MethodDisplayInfo} found. Check if the method was renamed.");
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
		private bool CheckCompetitionLimits(
			Summary summary, CompetitionTargets competitionTargets,
			CompetitionState competitionState, List<Conclusion> conclusions)
		{
			var result = true;

			var benchmarksByTarget = summary
				.GetExecutionOrderBenchmarks()
				.GroupBy(b => b.Target);

			foreach (var benchmarks in benchmarksByTarget)
			{
				var competitionTarget = competitionTargets[benchmarks.Key];

				if (competitionTarget != null)
				{
					result &= CheckCompetitionTargetLimits(
						competitionTarget, benchmarks.ToArray(),
						summary, competitionState,
						conclusions);
				}
			}

			return result;
		}

		/// <summary>Check competition target limits.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="benchmarksForTarget">Benchmarks for the target.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="conclusions">The warnings.</param>
		/// <returns><c>true</c> if competition limits are ok.</returns>
		protected virtual bool CheckCompetitionTargetLimits(
			CompetitionTarget competitionTarget,
			Benchmark[] benchmarksForTarget,
			Summary summary,
			CompetitionState competitionState,
			List<Conclusion> conclusions)
		{
			if (!competitionTarget.CheckLimiths)
				return true;

			var result = true;
			foreach (var benchmark in benchmarksForTarget)
			{
				result &= CheckCompetitionLimitsCore(
					benchmark,
					summary,
					competitionTarget,
					competitionState,
					conclusions);
			}

			return result;
		}

		private bool CheckCompetitionLimitsCore(
			Benchmark benchmark,
			Summary summary,
			CompetitionLimit competitionLimit,
			CompetitionState competitionState,
			List<Conclusion> conclusions)
		{
			var limitsMode = competitionState.Options.Limits;
			var actualValues = limitsMode.LimitProvider.TryGetActualValues(benchmark, summary);
			if (actualValues == null)
			{
				competitionState.AddAnalyserConclusion(
					this, conclusions, MessageSeverity.ExecutionError,
					$"Could not obtain competition limits for {benchmark.DisplayInfo}.",
					summary.TryGetBenchmarkReport(benchmark));

				return true;
			}

			if (competitionLimit.CheckLimitsFor(actualValues))
				return true;

			var targetMethodTitle = benchmark.Target.MethodDisplayInfo;
			var message = competitionLimit.IsEmpty
				? $"Method {targetMethodTitle} {actualValues} has empty limit. Please fill it."
				: $"Method {targetMethodTitle} {actualValues} does not fit into limits {competitionLimit}.";

			competitionState.AddAnalyserConclusion(
				this, conclusions, MessageSeverity.TestError,
				message, summary.TryGetBenchmarkReport(benchmark));

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

			var limitsMode = competitionState.Options.Limits;
			if (competitionState.RunNumber < limitsMode.RerunsIfValidationFailed)
			{
				competitionState.RequestReruns(1, "Limit checking failed.");
			}
		}
		#endregion
	}
}