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
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;
using CodeJam.Strings;

namespace CodeJam.PerfTests.Analysers
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
		public bool AllowSlowBenchmarks { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }
		public int MaxRerunsIfValidationFailed { get; set; }
		#endregion

		public IEnumerable<IWarning> Analyse(Summary summary)
		{
			ValidatePreconditions(summary);

			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];
			if (competitionTargets.Count == 0 && !competitionState.HasCriticalErrorsInRun)
			{
				InitCompetitionTargets(competitionTargets, summary);
			}

			var warnings = new List<IWarning>();
			if (!competitionState.HasCriticalErrorsInRun)
			{
				ValidateSummary(summary, warnings);

				ValidatePostconditions(summary, warnings);
			}

			return warnings.ToArray();
		}

		#region Parsing competition target info
		protected virtual void InitCompetitionTargets(
			CompetitionTargets competitionTargets,
			Summary summary)
		{
			competitionTargets.Clear();

			var competitionState = CompetitionCore.RunState[summary];
			if (IgnoreExistingAnnotations)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser,
					MessageSeverity.Informational,
					$"Existing benchmark limits are ignored due to {nameof(IgnoreExistingAnnotations)} setting.");
			}

			var resourceCache = new Dictionary<string, XDocument>();
			bool hasBaseline = false;
			foreach (var target in summary.GetTargets())
			{
				hasBaseline |= target.Baseline;

				var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
				if (competitionAttribute != null &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(
						target, competitionAttribute, competitionState, resourceCache);

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

			var targetResourceName = AttributeAnnotations.TryGetTargetResourceName(target);
			if (targetResourceName.IsNullOrEmpty())
			{
				if (IgnoreExistingAnnotations)
					return new CompetitionTarget(target, fallbackLimit, false);

				return AttributeAnnotations.ParseCompetitionTarget(target, competitionAttribute);
			}

			// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
			var resourceDoc = resourceCache.GetOrAdd(
				targetResourceName,
				r => XmlAnnotations.TryParseBenchmarksDocFromResource(target, r, competitionState));

			if (resourceDoc == null || IgnoreExistingAnnotations)
				return new CompetitionTarget(target, fallbackLimit, true);

			var result = XmlAnnotations.TryParseCompetitionTarget(target, resourceDoc, competitionState);
			return result ??
				new CompetitionTarget(target, fallbackLimit, true);
		}
		#endregion

		#region Validation
		protected virtual void ValidateSummary(Summary summary, List<IWarning> warnings)
		{
			var competitionState = CompetitionCore.RunState[summary];
			var competitionTargets = TargetsSlot[summary];
			var validated = true;
			foreach (var benchmarkGroup in summary.SameConditionsBenchmarks())
			{
				foreach (var benchmark in benchmarkGroup)
				{
					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target.Method, out competitionTarget))
						continue;

					validated &= ValidateBenchmark(
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
		private bool ValidateBenchmark(
			Summary summary, Benchmark benchmark,
			CompetitionLimit competitionLimit, List<IWarning> warnings)
		{
			if (benchmark.Target.Baseline)
				return true;

			var competitionState = CompetitionCore.RunState[summary];

			const int percentile = 95;

			var targetMethodName = benchmark.Target.Method.Name;
			var actualRatio = summary.TryGetScaledPercentile(benchmark, percentile);
			if (actualRatio == null)
			{
				var baselineBenchmark = summary.TryGetBaseline(benchmark);
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Baseline benchmark {baselineBenchmark?.ShortInfo} does not compute",
					summary.TryGetBenchmarkReport(benchmark));
				return false;
			}

			bool validated = true;

			var actualRatioText = actualRatio.Value.ToString(
				CompetitionLimit.ActualRatioFormat,
				EnvironmentInfo.MainCultureInfo);

			if (!competitionLimit.MinRatioIsOk(actualRatio.Value))
			{
				validated = false;
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Method {targetMethodName} runs faster than {competitionLimit.MinRatioText}x baseline. Actual ratio: {actualRatioText}x",
					summary.TryGetBenchmarkReport(benchmark));
			}

			if (!competitionLimit.MaxRatioIsOk(actualRatio.Value))
			{
				validated = false;
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.TestError,
					$"Method {targetMethodName} runs slower than {competitionLimit.MaxRatioText}x baseline. Actual ratio: {actualRatioText}x",
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
				.Select(b => b.Target.Method.Name)
				.Distinct()
				.ToArray();

			if (benchMissing.Length > 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.ExecutionError,
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
			}
		}

		private void ValidatePostconditions(Summary summary, List<IWarning> warnings)
		{
			var competitionState = CompetitionCore.RunState[summary];
			var tooFastReports = summary.Reports
				.Where(rp => rp.GetResultRuns().Any(r => r.GetAverageNanoseconds() < 400))
				.Select(rp => rp.Benchmark.Target.Method.Name)
				.ToArray();
			if (tooFastReports.Length > 0)
			{
				competitionState.AddAnalyserWarning(
					warnings, MessageSeverity.Warning,
					"The benchmarks " + string.Join(", ", tooFastReports) +
						" run faster than 400 nanoseconds. Results cannot be trusted.");
			}

			if (!AllowSlowBenchmarks)
			{
				var tooSlowReports = summary.Reports
					.Where(rp => rp.GetResultRuns().Any(r => r.GetAverageNanoseconds() > 500 * 1000 * 1000))
					.Select(rp => rp.Benchmark.Target.Method.Name)
					.ToArray();

				if (tooSlowReports.Length > 0)
				{
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.Warning,
						"The benchmarks " + string.Join(", ", tooSlowReports) +
							" run longer than 0.5 sec. Consider to rewrite the test as the peek timings will be hidden by averages" +
							$" or set the {nameof(AllowSlowBenchmarks)} to true.");
				}
			}

			if (warnings.Count == 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Analyser {GetType().Name}: no warnings.");
			}
		}
		#endregion
	}
}