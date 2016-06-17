using System;

using BenchmarkDotNet.Mathematics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.CompetitionLimitProviders
{
	/// <summary>Confidence interval-based competition limit provider..</summary>
	/// <seealso cref="ICompetitionLimitProvider"/>
	[PublicAPI]
	public class ConfidenceIntervalLimitProvider : CompetitionLimitProviderBase
	{
		/// <summary>The default instance of <see cref="ConfidenceIntervalLimitProvider"/></summary>
		public static readonly ICompetitionLimitProvider Instance = new ConfidenceIntervalLimitProvider();

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public override string ShortInfo => "CI";

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="timingRatios">Timing ratios relative to the baseline.</param>
		/// <param name="limitMode">If <c>true</c> limit values should be returned. Actual values returned otherwise.</param>
		/// <returns>Limits for the benchmark or <c>null</c> if none.</returns>
		protected override CompetitionLimit TryGetCompetitionLimitImpl(double[] timingRatios, bool limitMode)
		{
			var ci = new Statistics(timingRatios).ConfidenceInterval;
			var minRatio = limitMode
				? ci.Lower
				: ci.Mean;
			var maxRatio = limitMode
				? ci.Upper
				: ci.Mean;

			return new CompetitionLimit(minRatio, maxRatio);
		}
	}
}