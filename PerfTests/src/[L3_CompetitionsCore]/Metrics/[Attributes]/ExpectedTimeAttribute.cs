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

	/// <summary>Absolute time metric attribute.</summary>
	public class ExpectedTimeAttribute : MetricBaseAttribute, IMetricAttribute<ExpectedTimeAttribute.ValuesProvider, TimeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="ExpectedTimeAttribute"/>.
		/// that returns timings in nanoseconds.
		/// </summary>
		internal class ValuesProvider : TimeMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(PercentileMetricCalculator.P85, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		public ExpectedTimeAttribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="timeUnit">The time unit.</param>
		public ExpectedTimeAttribute(double max, TimeUnit timeUnit = TimeUnit.Nanosecond) : base(max, timeUnit)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="timeUnit">The time unit.</param>
		public ExpectedTimeAttribute(double min, double max, TimeUnit timeUnit = TimeUnit.Nanosecond) : base(min, max, timeUnit)
		{
		}

		/// <summary>Gets the unit of measurement for the metric.</summary>
		/// <value>The unit of measurement.</value>
		public new TimeUnit UnitOfMeasurement => (TimeUnit?)base.UnitOfMeasurement ?? TimeUnit.Nanosecond;
	}
}