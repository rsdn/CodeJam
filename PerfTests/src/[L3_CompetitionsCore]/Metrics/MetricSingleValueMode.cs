using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>How single-value annotations are threated.</summary>
	public enum MetricSingleValueMode
	{
		/// <summary>The value is max limit, min limit is negative infinity.</summary>
		FromInfinityToMax,

		/// <summary>Exact match: value specifies both min and max limits.</summary>
		BothMinAndMax
	}
}