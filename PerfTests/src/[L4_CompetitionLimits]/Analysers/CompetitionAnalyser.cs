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
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Analyser class that enables limits validation for competition benchmarks.</summary>
	/// <seealso cref="IAnalyser"/>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal class CompetitionAnalyser : IAnalyser
	{
		#region Competition targets
		// TODO: to readonly dict + api to update the targets?
		protected class CompetitionTargets : Dictionary<MethodInfo, CompetitionTarget> { }

		protected static readonly RunState<CompetitionTargets> TargetsSlot =
			new RunState<CompetitionTargets>();
		#endregion

		#region Properties
		/// <summary>The analyser should ignore existing annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing annotations.</value>
		public bool IgnoreExistingAnnotations { get; set; }

		/// <summary>Timing limit to detect too fast benchmarks.</summary>
		/// <value>The timing limit to detect too fast benchmarks></value>
		public TimeSpan TooFastBenchmarkLimit { get; set; }

		/// <summary>Timing limit to detect long-running benchmarks.</summary>
		/// <value>The timing limit to detect long-running benchmarks.</value>
		public TimeSpan LongRunningBenchmarkLimit { get; set; }

		/// <summary>
		/// Total count of retries performed if the validation failed.
		/// Used to detect the case when limits are too tight and the benchmark fails them occasionally.
		/// </summary>
		/// <value>Total count of retries performed if the validation failed.</value>
		public int MaxRerunsIfValidationFailed { get; set; }

		/// <summary>Log competition limits.</summary>
		/// <value><c>true</c> if competition limits should be logged; otherwise, <c>false</c>.</value>
		public bool LogCompetitionLimits { get; set; }

		/// <summary>Provider for benchmark limit metrics.</summary>
		/// <value>The provider for benchmark limit metrics.</value>
		public ILimitMetricProvider LimitMetricProvider { get; set; }
		#endregion

		/// <summary>Performs limit validation for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<IWarning> Analyse([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			ValidatePreconditions(summary);

			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];
			if (competitionTargets.Count == 0 &&
				!competitionState.HasCriticalErrorsInRun)
			{
				InitCompetitionTargets(competitionTargets, summary);
			}

			var warnings = new List<IWarning>();
			if (!competitionState.HasCriticalErrorsInRun)
			{
				ValidateSummary(summary, warnings);

				ValidatePostconditions(summary, warnings);
			}

			if (competitionState.LooksLikeLastRun && LogCompetitionLimits)
			{
				XmlAnnotations.LogCompetitionTargets(competitionTargets.Values, competitionState);
			}

			return warnings.ToArray();
		}

		#region Parsing competition target info
		/// <summary>Refills the competition targets collection.</summary>
		/// <param name="competitionTargets">The collection to be filled with competition targets.</param>
		/// <param name="summary">Summary for the run.</param>
		protected virtual void InitCompetitionTargets(
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

			var resourceCache = new Dictionary<string, XDocument>();
			bool hasBaseline = false;

			foreach (var target in summary.GetExecutionOrderTargets())
			{
				hasBaseline |= target.Baseline;

				var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
				if (competitionAttribute != null &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(
						target, competitionAttribute,
						competitionState, resourceCache);

					competitionTargets.Add(target.Method, competitionTarget);
				}
			}

			if (!hasBaseline && competitionTargets.Count > 0)
			{
				competitionState.WriteMessage(MessageSource.Analyser, MessageSeverity.SetupError, "The competition has no baseline");
			}
		}

		private CompetitionTarget GetCompetitionTarget(
			Target target, CompetitionBenchmarkAttribute competitionAttribute,
			CompetitionState competitionState,
			IDictionary<string, XDocument> resourceCache)
		{
			var fallbackLimit = CompetitionLimit.Empty;

			var competitionMetadata = AttributeAnnotations.TryGetCompetitionMetadata(target);
			if (competitionMetadata == null)
			{
				var limit = IgnoreExistingAnnotations
					? fallbackLimit
					: AttributeAnnotations.ParseAnnotation(competitionAttribute);

				return new CompetitionTarget(target, limit, false, null);
			}
			else
			{
				// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
				var resourceDoc = resourceCache.GetOrAdd(
					competitionMetadata.MetadataResourceName,
					r => XmlAnnotations.TryParseBenchmarksDocFromResource(target, r, competitionState));

				var limit = fallbackLimit;

				if (resourceDoc != null && !IgnoreExistingAnnotations)
				{
					var parsedLimit = XmlAnnotations.TryParseAnnotation(target, resourceDoc, competitionState);
					if (parsedLimit == null)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Informational,
							$"Xml anotations for {target.MethodTitle}: no annotation exists.");
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

		#region Validation
		/// <summary>Performs limit validation for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="warnings">List of warnings for the benchmarks.</param>
		protected virtual void ValidateSummary(Summary summary, List<IWarning> warnings)
		{
			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];

			if (LimitMetricProvider == null)
			{
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.SetupError,
					$"The {nameof(LimitMetricProvider)} should be not null.");
				return;
			}

			// TODO: remove grouping
			bool validated = true;
			foreach (var benchmarkGroup in summary.SameConditionsBenchmarks())
			{
				foreach (var benchmark in benchmarkGroup)
				{
					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target.Method, out competitionTarget))
						continue;

					validated &= ValidateBenchmarkCore(
						summary, benchmark,
						competitionTarget, warnings);
				}
			}

			if (!validated && MaxRerunsIfValidationFailed > competitionState.RunNumber)
			{
				// TODO: detailed message???
				competitionState.RequestReruns(1, "Competition validation failed.");
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private bool ValidateBenchmarkCore(
			Summary summary, Benchmark benchmark,
			CompetitionLimit competitionLimit, List<IWarning> warnings)
		{
			if (benchmark.Target.Baseline)
				return true;

			var competitionState = CompetitionCore.RunState[summary];

			double actualRatioMin, actualRatioMax;
			if (!LimitMetricProvider.TryGetMetrics(
				benchmark, summary,
				out actualRatioMin, out actualRatioMax))
			{
				var baselineBenchmark = summary.TryGetBaseline(benchmark);
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Baseline benchmark {baselineBenchmark?.ShortInfo} does not compute.",
					summary.TryGetBenchmarkReport(benchmark));
				return false;
			}

			bool validated = true;
			var targetMethodTitle = benchmark.Target.MethodTitle;

			if (!competitionLimit.MinRatioIsOk(actualRatioMin))
			{
				var actualRatioText = CompetitionLimit.GetActualRatioText(actualRatioMin);
				validated = false;
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Method {targetMethodTitle} runs faster than {competitionLimit.MinRatioText}x baseline. Actual ratio: {actualRatioText}x",
					summary.TryGetBenchmarkReport(benchmark));
			}

			if (!competitionLimit.MaxRatioIsOk(actualRatioMax))
			{
				var actualRatioText = CompetitionLimit.GetActualRatioText(actualRatioMax);
				validated = false;
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Method {targetMethodTitle} runs slower than {competitionLimit.MaxRatioText}x baseline. Actual ratio: {actualRatioText}x",
					summary.TryGetBenchmarkReport(benchmark));
			}

			return validated;
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void ValidatePreconditions(Summary summary)
		{
			var competitionState = CompetitionCore.RunState[summary];
			if (summary.HasCriticalValidationErrors)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"Summary has validation errors.");
			}

			var reportedBenchmarks = new HashSet<Benchmark>(
				summary.Reports
					.Where(r => r.ExecuteResults.Any())
					.Select(r => r.Benchmark));

			var benchMissing = summary.Benchmarks
				.Where(b => !reportedBenchmarks.Contains(b))
				.Select(b => b.Target.MethodTitle)
				.Distinct()
				.ToArray();

			if (benchMissing.Any())
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
			}
		}

		private void ValidatePostconditions(Summary summary, List<IWarning> warnings)
		{
			var culture = EnvironmentInfo.MainCultureInfo;
			var competitionState = CompetitionCore.RunState[summary];
			if (TooFastBenchmarkLimit > TimeSpan.Zero)
			{
				var tooFastReports = summary.Reports
					.Where(rp => rp.GetResultRuns().Any(r => r.GetAverageNanoseconds() < TooFastBenchmarkLimit.TotalNanoseconds()))
					.Select(rp => rp.Benchmark.Target.MethodTitle)
					.ToArray();
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
				var tooSlowReports = summary.Reports
					.Where(rp => rp.GetResultRuns().Any(r => r.GetAverageNanoseconds() > LongRunningBenchmarkLimit.TotalNanoseconds()))
					.Select(rp => rp.Benchmark.Target.MethodTitle)
					.ToArray();

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
		#endregion
	}
}