using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Default metric min value.</summary>
	public enum DefaultMinMetricValue
	{
		/// <summary>By default min limit is <c>0</c>.</summary>
		Zero,

		/// <summary>By default min limit is negative infinity.</summary>
		NegativeInfinity,

		/// <summary>Exact match: max limit equals to min limit.</summary>
		SameAsMax
	}
}