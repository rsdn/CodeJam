using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Lists well known ETW competition metrics
	/// </summary>
	public static class WellKnownEtwMetrics
	{
		/// <summary>The CLR exceptions thrown metric. </summary>
		public static readonly MetricInfo<ClrExceptionsAttribute> ClrExceptions =
			MetricInfo.FromAttribute<ClrExceptionsAttribute>();

		/// <summary>The CLR exceptions thrown metric. </summary>
		public static readonly MetricInfo<FileIoReadAttribute> FileIoRead =
			MetricInfo.FromAttribute<FileIoReadAttribute>();

		/// <summary>The CLR exceptions thrown metric. </summary>
		public static readonly MetricInfo<FileIoWriteAttribute> FileIoWrite =
			MetricInfo.FromAttribute<FileIoWriteAttribute>();
	}
}