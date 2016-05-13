using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;

// ReSharper disable CheckNamespace
using static BenchmarkDotNet.UnitTesting.CompetitionHelpers;

namespace BenchmarkDotNet.UnitTesting
{
	using CompetitionTargets = IDictionary<Target, CompetitionTarget>;

	/// <summary>
	/// Helper methods for <see cref="CompetitionTarget"/>
	/// </summary>
	internal static class CompetitionTargetHelpers
	{
		public static void InitCompetitionTargets(CompetitionTargets competitionTargets, Summary summary)
		{
			competitionTargets.Clear();
			var competitionParameters = summary.GetCompetitionParameters();

			var resourceCache = new Dictionary<string, XDocument>();

			foreach (var target in summary.Benchmarks.Select(d => d.Target).Distinct())
			{
				var competitionAttribute = target.Method.GetCustomAttribute<CompetitionBenchmarkAttribute>();

				if (competitionAttribute != null &&
					!competitionAttribute.Baseline &&
					!competitionAttribute.DoesNotCompete)
				{
					var competitionTarget = GetCompetitionTarget(
						target, competitionAttribute, resourceCache,
						competitionParameters);

					competitionTargets.Add(target, competitionTarget);
				}
			}
		}

		private static CompetitionTarget GetCompetitionTarget(
			Target target, CompetitionBenchmarkAttribute competitionAttribute,
			IDictionary<string, XDocument> resourceCache,
			CompetitionParameters competitionParameters)
		{
			string targetResourceName = null;
			var targetType = target.Type;
			while (targetType != null && targetResourceName == null)
			{
				targetResourceName = targetType.GetCustomAttribute<CompetitionMetadataAttribute>()
					?.MetadataResourceName;

				targetType = targetType.DeclaringType;
			}

			if (targetResourceName != null)
			{
				XDocument resourceDoc;
				if (!resourceCache.TryGetValue(targetResourceName, out resourceDoc))
				{
					var resourceStream = target.Type.Assembly.GetManifestResourceStream(targetResourceName);
					if (resourceStream == null)
						throw new InvalidOperationException(
							$"Method {target.Method.Name}: resource stream {targetResourceName} not found");

					resourceDoc = XDocument.Load(resourceStream);

					var rootNode = resourceDoc.Element(CompetitionBenchmarksRootNode);
					if (rootNode == null)
						throw new InvalidOperationException(
							$"Resource {targetResourceName}: root node {CompetitionBenchmarksRootNode} not found.");

					resourceCache[targetResourceName] = resourceDoc;
				}

				if (competitionParameters.IgnoreExistingAnnotations)
					return new CompetitionTarget(target, 0, 0, true);

				return GetCompetitionTargetFromResource(target, resourceDoc);
			}

			if (competitionParameters.IgnoreExistingAnnotations)
				return new CompetitionTarget(target, 0, 0, false);

			return new CompetitionTarget(
				target,
				competitionAttribute.MinRatio,
				competitionAttribute.MaxRatio,
				false);
		}

		private static CompetitionTarget GetCompetitionTargetFromResource(Target target, XDocument resourceDoc)
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
			var culture = CultureInfo.InvariantCulture;
			double.TryParse(minText, NumberStyles.Any, culture, out min);
			double.TryParse(maxText, NumberStyles.Any, culture, out max);

			// Only one attribute set
			if (minText == null && maxText != null)
			{
				min = -1; // ignore Min
			}
			else if (maxText == null && minText != null)
			{
				max = -1; // ignore Max
			}

			return new CompetitionTarget(target, min, max, true);
		}

		public static CompetitionTarget[] GetCompetitionTargetsToUpdate(
			Summary summary, CompetitionTargets competitionTargets)
		{
			var fixedMinTargets = new HashSet<CompetitionTarget>();
			var fixedMaxTargets = new HashSet<CompetitionTarget>();
			var newTargets = new Dictionary<Target, CompetitionTarget>();

			foreach (var benchGroup in summary.SameConditionBenchmarks())
			{
				var baselineBenchmark = GetBaselineBenchmark(benchGroup);
				var baselineMetricMin = summary.GetPercentile(baselineBenchmark, 0.85);
				var baselineMetricMax = summary.GetPercentile(baselineBenchmark, 0.95);

				foreach (var benchmark in benchGroup)
				{
					if (benchmark == baselineBenchmark)
						continue;

					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target, out competitionTarget))
						continue;

					var minRatio = summary.GetPercentile(benchmark, 0.85) / baselineMetricMin;
					var maxRatio = summary.GetPercentile(benchmark, 0.95) / baselineMetricMax;
					if (minRatio > maxRatio)
					{
						var temp = minRatio;
						minRatio = maxRatio;
						maxRatio = temp;
					}

					CompetitionTarget newTarget;
					if (!newTargets.TryGetValue(competitionTarget.Target, out newTarget))
					{
						newTarget = competitionTarget.Clone();
					}

					if (newTarget.UnionWithMin(minRatio))
					{
						fixedMinTargets.Add(newTarget);
						newTargets[newTarget.Target] = newTarget;
					}
					if (newTarget.UnionWithMax(maxRatio))
					{
						fixedMaxTargets.Add(newTarget);
						newTargets[newTarget.Target] = newTarget;
					}
				}
			}

			foreach (var competitionTarget in fixedMinTargets)
			{
				// min = 0.95x;
				competitionTarget.UnionWithMin(
					Math.Floor(competitionTarget.Min * 95) / 100);
			}
			foreach (var competitionTarget in fixedMaxTargets)
			{
				// max = 1.05x;
				competitionTarget.UnionWithMax(
					Math.Ceiling(competitionTarget.Max * 105) / 100);
			}

			return newTargets.Values.ToArray();
		}

		// ReSharper disable once ParameterTypeCanBeEnumerable.Local
		private static Benchmark GetBaselineBenchmark(
			IGrouping<KeyValuePair<IJob, ParameterInstances>, Benchmark> benchmarkGroup)
		{
			var baselineBenchmarks = benchmarkGroup.Where(b => b.Target.Baseline).ToArray();
			if (baselineBenchmarks.Length == 0)
				throw new InvalidOperationException("Define Baseline benchmark");
			if (baselineBenchmarks.Length != 1)
				throw new InvalidOperationException("There should be only one Baseline benchmark");

			var baselineBenchmark = baselineBenchmarks.Single();
			return baselineBenchmark;
		}

		public static void ValidateSummary(
			Summary summary, double defaultMinRatio, double defaultMaxRatio,
			CompetitionTargets competitionTargets)
		{
			// Based on 95th percentile
			const double percentileRatio = 0.95;

			var benchmarkGroups = summary.SameConditionBenchmarks();
			foreach (var benchmarkGroup in benchmarkGroups)
			{
				var baselineBenchmark = GetBaselineBenchmark(benchmarkGroup);
				var baselineMetric = summary.GetPercentile(baselineBenchmark, percentileRatio);
				if (baselineMetric <= 0)
					throw new InvalidOperationException($"Baseline benchmark {baselineBenchmark.ShortInfo} does not compute");

				foreach (var benchmark in benchmarkGroup)
				{
					if (benchmark == baselineBenchmark)
						continue;

					CompetitionTarget competitionTarget;
					if (!competitionTargets.TryGetValue(benchmark.Target, out competitionTarget))
						continue;

					var benchmarkMinRatio = defaultMinRatio;
					if (!competitionTarget.MinIsEmpty)
					{
						benchmarkMinRatio = competitionTarget.Min;
					}

					var benchmarkMaxRatio = defaultMaxRatio;
					if (!competitionTarget.MaxIsEmpty)
					{
						benchmarkMaxRatio = competitionTarget.Max;
					}

					var reportMetric = summary.GetPercentile(benchmark, percentileRatio);
					var actualRatio = Math.Round(reportMetric / baselineMetric, 2);

					ValidateBenchmark(benchmark, actualRatio, benchmarkMinRatio, benchmarkMaxRatio);
				}
			}
		}

		public static void ValidatePreconditions(Summary summary)
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

			var benchReports = summary.Reports
				.Where(r => r.ExecuteResults.Any())
				.Select(r => r.Benchmark)
				.ToHashSet();
			var benchMissing = summary.Benchmarks
				.Where(b => !benchReports.Contains(b))
				.Select(b => b.Target.Method.Name)
				.Distinct()
				.ToArray();

			if (benchMissing.Length > 0)
				throw new InvalidOperationException(
					"No reports for benchmarks: " + string.Join(", ", benchMissing));
		}

		public static void ValidatePostconditions(Summary summary)
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

			if (!summary.GetCompetitionParameters().AllowSlowBenchmarks)
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
							$"or set the {nameof(CompetitionParameters)}.{nameof(CompetitionParameters.AllowSlowBenchmarks)} to true.");
			}
		}

		private static void ValidateBenchmark(
			Benchmark benchmark,
			double actualRatio, double benchmarkMinRatio, double benchmarkMaxRatio)
		{
			var culture = CultureInfo.InvariantCulture;
			var targetMethodName = benchmark.Target.Method.Name;
			var minText = benchmarkMinRatio.ToString(culture);
			var maxText = benchmarkMaxRatio.ToString(culture);
			var actualRatioText = actualRatio.ToString(culture);

			if (benchmarkMinRatio >= 0)
			{
				if (actualRatio < benchmarkMinRatio)
					throw new InvalidOperationException(
						$"Method {targetMethodName} runs faster than {minText}x baseline. Actual ratio: {actualRatioText}x");
			}

			if (benchmarkMaxRatio >= 0)
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (benchmarkMaxRatio == 0)
					throw new InvalidOperationException(
						$"Method {targetMethodName}: max ratio not set. Actual ratio: {actualRatioText}x");

				if (actualRatio > benchmarkMaxRatio)
					throw new InvalidOperationException(
						$"Method {targetMethodName} runs slower than {maxText}x baseline. Actual ratio: {actualRatioText}x");
			}
		}
	}
}