using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Metric value columns to return.</summary>
	[Flags]
	public enum MetricValueColumns
	{
		/// <summary>No columns.</summary>
		None = 0x0,

		/// <summary>
		/// List of columns is determined by metric values provider.
		/// The flag is ignored when combined with others.
		/// </summary>
		Auto = 0x1,

		/// <summary>Mean value column.</summary>
		Mean = 0x2,

		/// <summary>Standard deviation column.</summary>
		StdDev = 0x4,

		/// <summary>Min value column.</summary>
		Min = 0x8,

		/// <summary>Max value column.</summary>
		Max = 0x10,

		/// <summary>The value and standard deviation columns.</summary>
		ValueAndStdDev = Mean | StdDev,

		/// <summary>The min and max values columns.</summary>
		MinAndMax = Min | Max,

		/// <summary>
		/// Default (mean only)
		/// </summary>
		Default = Mean
	}
}