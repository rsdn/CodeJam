using System;

using BenchmarkDotNet.Mathematics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.CompetitionLimitProviders
{
	// TODO: something better than BoundaryLoosedBy?
	/// <summary>Percentile-based competition limit provider.</summary>
	/// <seealso cref="ICompetitionLimitProvider"/>
	[PublicAPI]
	public class PercentileLimitProvider : CompetitionLimitProviderBase
	{
		/// <summary> Metric is based on 20 (lower boundary) and 80 (upper boundary) percentiles.</summary>
		public static readonly ICompetitionLimitProvider P20To80 = new PercentileLimitProvider(40, 60, 20);

		/// <summary>Initializes a new instance of the <see cref="PercentileLimitProvider"/> class.</summary>
		/// <param name="minRatioPercentile">The percentile for the minimum timing ratio.</param>
		/// <param name="maxRatioPercentile">>The percentile for the maximum timing ratio.</param>
		/// <param name="limitModeDelta">
		/// Delta to loose percentiles by. Used for <see cref="ICompetitionLimitProvider.TryGetCompetitionLimit "/>.
		/// </param>
		public PercentileLimitProvider(
			int minRatioPercentile,
			int maxRatioPercentile,
			int limitModeDelta)
		{
			Code.InRange(minRatioPercentile, nameof(minRatioPercentile), 0, maxRatioPercentile);
			Code.InRange(maxRatioPercentile, nameof(maxRatioPercentile), minRatioPercentile, 99);
			Code.InRange(limitModeDelta, nameof(limitModeDelta), 0, 99);

			MinRatioPercentile = minRatioPercentile;
			MaxRatioPercentile = maxRatioPercentile;
			LimitModeDelta = limitModeDelta;
		}

		/// <summary>Short description for the provider.</summary>
		/// <value>The short description for the provider.</value>
		public override string ShortInfo =>
			MinRatioPercentile == MaxRatioPercentile
				? $"P{MinRatioPercentile}(±{LimitModeDelta})"
				: $"P{MinRatioPercentile - LimitModeDelta}..{MaxRatioPercentile + LimitModeDelta}";

		/// <summary>Percentile for the minimum timing ratio.</summary>
		/// <value>The percentile for the minimum timing ratio.</value>
		public int MinRatioPercentile { get; }

		/// <summary>Percentile for the maximum timing ratio.</summary>
		/// <value>The percentile for the maximum timing ratio.</value>
		public int MaxRatioPercentile { get; }

		/// <summary>
		/// Delta to loose percentiles by. Used for <see cref="ICompetitionLimitProvider.TryGetCompetitionLimit "/>.
		/// </summary>
		/// <value>The delta to loose percentiles by.</value>
		public int LimitModeDelta { get; }

		/// <summary>Limits for the benchmark.</summary>
		/// <param name="timingRatios">Timing ratios relative to the baseline.</param>
		/// <param name="limitMode">If <c>true</c> limit values should be returned. Actual values returned otherwise.</param>
		/// <returns>Limits for the benchmark or <c>null</c> if none.</returns>
		protected override CompetitionLimit TryGetCompetitionLimitImpl(double[] timingRatios, bool limitMode)
		{
			var percentiles = new Statistics(timingRatios).Percentiles;
			var minPercentile = limitMode
				? Math.Max(0, MinRatioPercentile - LimitModeDelta)
				: MinRatioPercentile;
			var maxPercentile = limitMode
				? Math.Min(99, MaxRatioPercentile + LimitModeDelta)
				: MaxRatioPercentile;

			var minRatio = percentiles.Percentile(minPercentile);
			var maxRatio = percentiles.Percentile(maxPercentile);

			return new CompetitionLimit(minRatio, maxRatio);
		}
	}
}