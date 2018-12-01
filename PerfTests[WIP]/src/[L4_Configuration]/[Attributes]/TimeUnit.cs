using System;

using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Time measurement units
	/// </summary>
	public enum TimeUnit
	{
		/// <summary>Time measured in nanoseconds, ns.</summary>
		[MetricUnit("ns", AppliesFrom = 0)]
		Nanosecond = 1,

		/// <summary>Time measured in microseconds, us.</summary>
		[MetricUnit("us", AppliesFrom = (int)Microsecond * 0.8)]
		Microsecond = Nanosecond * 1000,

		/// <summary>Time measured in milliseconds, ms.</summary>
		[MetricUnit("ms", AppliesFrom = (int)Millisecond * 0.8)]
		Millisecond = Microsecond * 1000,

		/// <summary>Time measured in seconds, sec.</summary>
		[MetricUnit("sec", AppliesFrom = (int)Second * 0.8)]
		Second = Millisecond * 1000
	}
}