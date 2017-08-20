using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Lists well known competition metrics
	/// </summary>
	public static class WellKnownMetrics
	{
		/// <summary>The relative time metric. </summary>
		public static readonly MetricInfo<CompetitionBenchmarkAttribute> RelativeTime =
			MetricInfo.PrimaryMetric;

		/// <summary>The expected execution time metric based on 95th percentile.</summary>
		public static readonly MetricInfo<ExpectedTimeAttribute> ExpectedTime =
			MetricInfo.FromAttribute<ExpectedTimeAttribute>();

		/// <summary>The mean execution time metric.</summary>
		public static readonly MetricInfo<MeanTimeAttribute> MeanTime =
			MetricInfo.FromAttribute<MeanTimeAttribute>();

		/// <summary>GC allocations metric, in bytes.</summary>
		public static readonly MetricInfo<GcAllocationsAttribute> GcAllocations =
			MetricInfo.FromAttribute<GcAllocationsAttribute>();

		/// <summary>GC 0 count metric.</summary>
		public static readonly MetricInfo<Gc0Attribute> Gc0 =
			MetricInfo.FromAttribute<Gc0Attribute>();

		/// <summary>GC 1 count metric.</summary>
		public static readonly MetricInfo<Gc1Attribute> Gc1 =
			MetricInfo.FromAttribute<Gc1Attribute>();

		/// <summary>GC 2 count metric.</summary>
		public static readonly MetricInfo<Gc2Attribute> Gc2 =
			MetricInfo.FromAttribute<Gc2Attribute>();
	}
}