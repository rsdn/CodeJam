using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>The single value treatment mode.</summary>
	public enum MetricSingleValueMode
	{
		/// <summary>The value is max limit, min limit is negative infinity.</summary>
		FromInfinityToMax,

		/// <summary>The value is max limit, min limit is <c>0</c>.</summary>
		FromZeroToMax,

		/// <summary>Exact match: value specifies both min and max limits</summary>
		BothMinAndMax
	}
}