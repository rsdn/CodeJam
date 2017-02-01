using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Time measurement units
	/// </summary>
	public enum TimeUnit
	{
		/// <summary>Time measured in nanoseconds.</summary>
		[MetricUnit("ns")]
		Nanosecond = 1,
		/// <summary>Time measured in microseconds.</summary>
		[MetricUnit("us")]
		Microsecond = Nanosecond * 1000,
		/// <summary>Time measured in milliseconds.</summary>
		[MetricUnit("ms")]
		Millisecond = Microsecond * 1000,
		/// <summary>Time measured in seconds.</summary>
		[MetricUnit("sec")]
		Second = Millisecond * 1000,
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
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
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
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="timeUnit">The time unit.</param>
		public ExpectedTimeAttribute(double min, double max, TimeUnit timeUnit = TimeUnit.Nanosecond) : base(min, max, timeUnit)
		{
		}

		public new TimeUnit UnitOfMeasurement => (TimeUnit?)base.UnitOfMeasurement ?? TimeUnit.Nanosecond;
	}
}