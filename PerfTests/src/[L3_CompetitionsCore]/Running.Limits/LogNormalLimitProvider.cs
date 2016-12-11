using System;
using System.Linq;

using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CodeJam.Collections;

using JetBrains.Annotations;

using static System.Math;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>
	/// Competition limit provider for microbenchmarks. Uses lognormal distribution for results estimation.
	/// </summary>
	/// <seealso cref="CompetitionLimitProviderBase"/>
	[PublicAPI]
	public class LogNormalLimitProvider : CompetitionLimitProviderBase
	{
		/// <summary>Instance of the provider.</summary>
		public static readonly ICompetitionLimitProvider Instance = new LogNormalLimitProvider();

		#region .ctor & properties
		/// <summary>
		/// Prevents a default instance of the <see cref="LogNormalLimitProvider"/> class from being created.
		/// </summary>
		private LogNormalLimitProvider() { }

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public override string ShortInfo => "Lnml";
		#endregion

		/// <summary>Actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		public override double? TryGetMeanValue(Benchmark benchmark, Summary summary)
		{
			if (!TryGetTimings(benchmark, summary, out var benchmarkTimings, out var baselineTimings))
				return null;

			// μ = exp([ln a0 + ln a1 + … + ln aN] / N)
			benchmarkTimings = benchmarkTimings.ConvertAll(a => a <= 0 ? 0 : Log(a));
			baselineTimings = baselineTimings.ConvertAll(a => a <= 0 ? 0 : Log(a));

			// μBenchmark / μBaseline
			return Exp(benchmarkTimings.Average() - baselineTimings.Average());
		}

		/// <summary>Metric that described deviation of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that described deviation for the benchmark or <c>null</c> if none.</returns>
		// THANKSTO: http://stats.stackexchange.com/questions/21735/what-are-the-mean-and-variance-of-the-ratio-of-two-lognormal-variables
		public override double? TryGetVariance(Benchmark benchmark, Summary summary)
		{
			double Sqr(double d) => d * d;
			if (!TryGetTimings(benchmark, summary, out var benchmarkTimings, out var baselineTimings))
				return null;

			benchmarkTimings = benchmarkTimings.ConvertAll(a => a <= 0 ? 0 : Log(a));
			baselineTimings = baselineTimings.ConvertAll(a => a <= 0 ? 0 : Log(a));

			var benchmarkStat = new Statistics(benchmarkTimings);
			var baselineStat = new Statistics(baselineTimings);

			// μZ = μBenchmark - μBaseline
			var resultMean = benchmarkStat.Mean - baselineStat.Mean;
			// σZ^2 = σBenchmark^2+σBaseline^2-covariance // assuming covariance=0 as vars are independent
			var resultVariance = Sqr(benchmarkStat.StandardDeviation) + Sqr(baselineStat.StandardDeviation);

			// Variance(e^Z)=exp{2μZ+2σZ^2}−exp{2μZ+σZ^2}
			return Exp(2 * resultMean + 2 * resultVariance) - Exp(2 * resultMean + resultVariance);
		}

		/// <summary>Actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual values for the benchmark or empty range if none.</returns>
		public override LimitRange TryGetActualValues(Benchmark benchmark, Summary summary)
		{
			var result = TryGetMeanValue(benchmark, summary);
			return LimitRange.CreateRatioLimit(result, result);
		}

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Limits for the benchmark or empty range if none.</returns>
		public override LimitRange TryGetCompetitionLimit(Benchmark benchmark, Summary summary)
		{
			var result = TryGetMeanValue(benchmark, summary);
			if (result == null)
				return LimitRange.Empty;

			var minRatio = result * 0.98; // 0.99*0.99 accuracy
			var maxRatio = result * 1.02; // 1.01*1.01 accuracy
			return LimitRange.CreateRatioLimit(minRatio, maxRatio);
		}
	}
}