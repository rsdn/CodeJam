using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Messages;
using BenchmarkDotNet.Running.Stateful;

using static BenchmarkDotNet.Competitions.CompetitionLimitConstants;

namespace BenchmarkDotNet.Analysers
{
	internal class CompetitionLimitsAnalyser : IAnalyser
	{
		protected class CompetitionTargets : Dictionary<Target, CompetitionTarget> { }

		protected static readonly RunState<CompetitionTargets> TargetsSlot =
			new RunState<CompetitionTargets>();

		#region Parse competition targets helpers
		private static string TryGetTargetResourceName(Target target)
		{
			string targetResourceName = null;
			var targetType = target.Type;
			while (targetType != null && targetResourceName == null)
			{
				targetResourceName = targetType
					.GetCustomAttribute<CompetitionMetadataAttribute>()
					?.MetadataResourceName;

				targetType = targetType.DeclaringType;
			}
			return targetResourceName;
		}

		private static XDocument TryLoadResourceDoc(
			Target target, string targetResourceName, List<IWarning> warnings)
		{
			var resourceStream = target.Type.Assembly.GetManifestResourceStream(targetResourceName);

			if (resourceStream == null)
			{
				var message =
					$"Method {target.Method.Name}: resource stream {targetResourceName} not found";
				warnings.Add(new Warning(nameof(MessageSeverity.SetupError), message, null));
				return null;
			}

			var resourceDoc = XDocument.Load(resourceStream);
			var rootNode = resourceDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				var message =
					$"Resource {targetResourceName}: root node {CompetitionBenchmarksRootNode} not found.";
				warnings.Add(new Warning(nameof(MessageSeverity.SetupError), message, null));
				return null;
			}

			return resourceDoc;
		}

		private static CompetitionTarget GetCompetitionTargetFromAttribute(
			Target target, CompetitionBenchmarkAttribute competitionAttribute) =>
				new CompetitionTarget(
					target,
					competitionAttribute.MinRatio,
					competitionAttribute.MaxRatio,
					false);

		private static CompetitionTarget GetCompetitionTargetFromResource(
			Target target, XDocument resourceDoc)
		{
			var competitionName = target.Type.Name;
			var candidateName = target.Method.Name;

			var matchingNodes =
				// ReSharper disable once PossibleNullReferenceException
				from competition in resourceDoc.Root.Elements(CompetitionNode)
				where competition.Attribute(TargetAttribute)?.Value == competitionName
				from candidate in competition.Elements(CandidateNode)
				where candidate.Attribute(TargetAttribute)?.Value == candidateName
				select candidate;

			var resultNode = matchingNodes.SingleOrDefault();
			var minText = resultNode?.Attribute(MinRatioAttribute)?.Value;
			var maxText = resultNode?.Attribute(MaxRatioAttribute)?.Value;

			double min;
			double max;
			var culture = EnvironmentInfo.MainCultureInfo;
			double.TryParse(minText, NumberStyles.Any, culture, out min);
			double.TryParse(maxText, NumberStyles.Any, culture, out max);

			// Only one attribute set
			if (minText == null && maxText != null)
			{
				min = IgnoreValue;
			}
			else if (maxText == null && minText != null)
			{
				max = IgnoreValue;
			}
			else
			{
				min = EmptyValue;
				max = EmptyValue;
			}

			return new CompetitionTarget(target, min, max, true);
		}
		#endregion

		public bool IgnoreExistingAnnotations { get; set; }
		public bool RerunIfValidationFailed { get; set; }
		public bool AllowSlowBenchmarks { get; set; }
		public CompetitionLimit DefaultCompetitionLimit { get; set; }

		public IEnumerable<IWarning> Analyse(Summary summary)
		{
			// TODO: as warnings
			ValidatePreconditions(summary);

			var warnings = new List<IWarning>();
			var competitionTargets = TargetsSlot[summary];
			if (competitionTargets.Count == 0)
			{
				InitCompetitionTargets(competitionTargets, summary, warnings);
			}

			bool validated = ValidateSummary(summary, competitionTargets, warnings);

			if (!validated && RerunIfValidationFailed)
			{
				// TODO: detailed message???
				CompetitionCore.RunState[summary].RequestRerun(
					"Competition validation failed, requesting rerun.");
			}

			ValidatePostconditions(summary);

			var result = warnings.ToArray();
			CompetitionCore.FillAnalyserMessages(summary, warnings);
			return result;
		}

		#region Parsing competition target info
		private void InitCompetitionTargets(
			CompetitionTargets competitionTargets,
			Summary summary,
			List<IWarning> warnings)
		{
			competitionTargets.Clear();

			var resourceCache = new Dictionary<string, XDocument>();
			foreach (var target in summary.GetTargets())
			{
				if (warnings.Count > 0)
					break;

				var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();
				if (competitionAttribute != null &&
					!competitionAttribute.Baseline &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(
						target, competitionAttribute,
						resourceCache, warnings);

					competitionTargets.Add(target, competitionTarget);
				}
			}
		}

		private CompetitionTarget GetCompetitionTarget(
			Target target, CompetitionBenchmarkAttribute competitionAttribute,
			IDictionary<string, XDocument> resourceCache,
			List<IWarning> warnings)
		{
			var fallbackLimit = DefaultCompetitionLimit ?? CompetitionLimit.NoLimit;
			var targetResourceName = TryGetTargetResourceName(target);
			if (targetResourceName == null)
			{
				if (IgnoreExistingAnnotations)
					return new CompetitionTarget(target, fallbackLimit, false);

				return GetCompetitionTargetFromAttribute(target, competitionAttribute);
			}

			// DONTTOUCH: the doc should be loaded for validation even if IgnoreExistingAnnotations = true
			XDocument resourceDoc;
			if (!resourceCache.TryGetValue(targetResourceName, out resourceDoc))
			{
				resourceDoc = TryLoadResourceDoc(target, targetResourceName, warnings);
				resourceCache[targetResourceName] = resourceDoc;
			}

			if (resourceDoc == null || IgnoreExistingAnnotations)
				return new CompetitionTarget(target, fallbackLimit, true);

			return GetCompetitionTargetFromResource(target, resourceDoc);
		}
		#endregion

		protected virtual bool ValidateSummary(
			Summary summary, CompetitionTargets competitionTargets,
			List<IWarning> warnings)
		{
			var validated = true;
			foreach (var benchmarkGroup in summary.SameConditionBenchmarks())
			{
				foreach (var benchmark in benchmarkGroup)
				{
					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target, out competitionTarget))
						continue;

					validated &= ValidateBenchmark(
						summary, benchmark,
						competitionTarget, warnings);
				}
			}

			return validated;
		}

		private static bool ValidateBenchmark(
			Summary summary, Benchmark benchmark,
			CompetitionTarget competitionLimit, List<IWarning> warnings)
		{
			const int percentile = 95;

			var targetMethodName = benchmark.Target.Method.Name;
			var actualRatio = summary.TryGetScaledPercentile(benchmark, percentile);
			if (actualRatio == null)
			{
				var baselineBenchmark = summary.TryGetBaseline(benchmark);
				warnings.Add(
					new Warning(
						nameof(MessageSeverity.SetupError),
						$"Baseline benchmark {baselineBenchmark?.ShortInfo} does not compute",
						summary.TryGetBenchmarkReport(benchmark)));
				return false;
			}

			bool validated = true;

			var actualRatioText = actualRatio.GetValueOrDefault().ToString(
				RatioFormat,
				EnvironmentInfo.MainCultureInfo);

			if (!competitionLimit.IgnoreMin)
			{
				if (actualRatio < competitionLimit.Min)
				{
					validated = false;
					warnings.Add(
						new Warning(
							nameof(MessageSeverity.TestError),
							$"Method {targetMethodName} runs faster than {competitionLimit.MinText}x baseline. Actual ratio: {actualRatioText}x",
							summary.TryGetBenchmarkReport(benchmark)));
				}
			}

			if (!competitionLimit.IgnoreMax)
			{
				if (actualRatio > competitionLimit.Max)
				{
					validated = false;
					warnings.Add(
						new Warning(
							nameof(MessageSeverity.TestError),
							$"Method {targetMethodName} runs slower than {competitionLimit.MaxText}x baseline. Actual ratio: {actualRatioText}x",
							summary.TryGetBenchmarkReport(benchmark)));
				}
			}

			return validated;
		}

		private void ValidatePreconditions(Summary summary)
		{
			if (summary.HasCriticalValidationErrors)
			{
				var messages = summary.ValidationErrors
					.Where(err => err.IsCritical)
					.Select(
						err =>
							(err.Benchmark == null ? null : err.Benchmark.ShortInfo + ": ") +
								err.Message)
					.ToArray();

				if (messages.Length > 0)
					throw new InvalidOperationException(
						"Validation failed:\r\n" + string.Join(Environment.NewLine, messages));

				throw new InvalidOperationException("Benchmark validation failed.");
			}

			var runnedBenchmarks = new HashSet<Benchmark>(
				summary.Reports
					.Where(r => r.ExecuteResults.Any())
					.Select(r => r.Benchmark));

			var benchMissing = summary.Benchmarks
				.Where(b => !runnedBenchmarks.Contains(b))
				.Select(b => b.Target.Method.Name)
				.Distinct()
				.ToArray();

			if (benchMissing.Length > 0)
				throw new InvalidOperationException(
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
		}

		public void ValidatePostconditions(Summary summary)
		{
			var tooFastReports = summary.Reports
				.Where(
					rp => rp.GetResultRuns().Any(
						r => r.GetAverageNanoseconds() < 400))
				.Select(rp => rp.Benchmark.Target.Method.Name)
				.ToArray();
			if (tooFastReports.Length > 0)
				throw new InvalidOperationException(
					"The benchmarks " + string.Join(", ", tooFastReports) +
						"runs faster than 400 nanoseconds. Results cannot be trusted.");

			if (!AllowSlowBenchmarks)
			{
				var tooSlowReports = summary.Reports
					.Where(
						rp => rp.GetResultRuns().Any(
							r => r.GetAverageNanoseconds() > 500 * 1000 * 1000))
					.Select(rp => rp.Benchmark.Target.Method.Name)
					.ToArray();

				if (tooSlowReports.Length > 0)
					throw new InvalidOperationException(
						"The benchmarks " + string.Join(", ", tooSlowReports) +
							"runs longer than half a second. Consider to rewrite the test as the peek timings will be hidden by averages " +
							$"or set the {nameof(AllowSlowBenchmarks)} to true.");
			}
		}
	}
}