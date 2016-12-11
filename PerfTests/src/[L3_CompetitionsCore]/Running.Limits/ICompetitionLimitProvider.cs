using System;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>Interface for competition limit provider.</summary>
	public interface ICompetitionLimitProvider
	{
		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		string ShortInfo { get; }

		/// <summary>Actual value for the benchmark (averaged).</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual value for the benchmark or <c>null</c> if none.</returns>
		double? TryGetMeanValue(Benchmark benchmark, Summary summary);

		/// <summary>Metric that describes variance of the value for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric that describes variance for the benchmark or <c>null</c> if none.</returns>
		double? TryGetVariance(Benchmark benchmark, Summary summary);

		/// <summary>Actual values for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Actual values for the benchmark or empty range if none.</returns>
		LimitRange TryGetActualValues(Benchmark benchmark, Summary summary);

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Limits for the benchmark or empty range if none.</returns>
		LimitRange TryGetCompetitionLimit(Benchmark benchmark, Summary summary);
	}
}