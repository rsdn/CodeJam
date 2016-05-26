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

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.PerfTests.Running.SourceAnnotations;

namespace CodeJam.PerfTests.Analysers
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal class CompetitionLimitsAnalyser : IAnalyser
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
			var warnings = new List<IWarning>();

			var competitionState = CompetitionCore.RunState[summary];
			ValidatePreconditions(summary);

			var competitionTargets = TargetsSlot[summary];
			if (competitionTargets.Count == 0 && !competitionState.HasCriticalErrorsInRun)
			{
				InitCompetitionTargets(competitionTargets, summary);
			}

			if (!competitionState.HasCriticalErrorsInRun)
			{
				ValidateSummary(summary, competitionTargets, warnings);

				ValidatePostconditions(summary, warnings);
			}

			CompetitionCore.FillAnalyserMessages(summary, warnings);
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
					!competitionAttribute.Baseline &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(competitionState, target, competitionAttribute, resourceCache);

					competitionTargets.Add(target.Method, competitionTarget);
				}
			}

			if (!hasBaseline && competitionTargets.Count > 0)
			{
				competitionState.WriteMessage(MessageSource.Analyser, MessageSeverity.SetupError, "The competition has no baseline");
			}
		}

		private CompetitionTarget GetCompetitionTarget(
			CompetitionState competitionState,
			Target target, CompetitionBenchmarkAttribute competitionAttribute,
			IDictionary<string, XDocument> resourceCache)
		{
			var emptyLimit = CompetitionLimit.Empty;

			var targetResourceName = AttributeAnnotations.TryGetTargetResourceName(target);
			if (targetResourceName == null)
			{
				if (IgnoreExistingAnnotations)
					return new CompetitionTarget(target, emptyLimit, false);

				return AttributeAnnotations.ParseCompetitionTarget(target, competitionAttribute);
			}

			// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
			XDocument resourceDoc;
			if (!resourceCache.TryGetValue(targetResourceName, out resourceDoc))
			{
				resourceDoc = XmlAnnotations.TryLoadResourceDoc(target.Type, targetResourceName, competitionState);
				resourceCache[targetResourceName] = resourceDoc;
			}

			if (resourceDoc == null || IgnoreExistingAnnotations)
				return new CompetitionTarget(target, emptyLimit, true);

			var result = XmlAnnotations.TryParseCompetitionTarget(resourceDoc, target, competitionState);
			return result ??
				new CompetitionTarget(target, emptyLimit, true);
		}
		#endregion

		#region Validation
		protected virtual void ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets,
			List<IWarning> warnings)
		{
			var validated = true;
			foreach (var benchmarkGroup in summary.SameConditionBenchmarks())
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

			var competitionState = CompetitionCore.RunState[summary];
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

			var actualRatioText = actualRatio.GetValueOrDefault().ToString(
				CompetitionLimit.ActualRatioFormat,
				EnvironmentInfo.MainCultureInfo);

			if (!competitionLimit.IgnoreMin)
			{
				if (actualRatio < competitionLimit.Min)
				{
					validated = false;
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.TestError,
						$"Method {targetMethodName} runs faster than {competitionLimit.MinText}x baseline. Actual ratio: {actualRatioText}x",
						summary.TryGetBenchmarkReport(benchmark));
				}
			}

			if (!competitionLimit.IgnoreMax)
			{
				if (actualRatio > competitionLimit.Max)
				{
					validated = false;
					competitionState.AddAnalyserWarning(
						warnings, MessageSeverity.TestError,
						$"Method {targetMethodName} runs slower than {competitionLimit.MaxText}x baseline. Actual ratio: {actualRatioText}x",
						summary.TryGetBenchmarkReport(benchmark));
				}
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
						" runs faster than 400 nanoseconds. Results cannot be trusted.");
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
							" runs longer than half a second. Consider to rewrite the test as the peek timings will be hidden by averages" +
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