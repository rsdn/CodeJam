using System;

using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>Absolute time metric attribute based on 95th percentile.</summary>
	[MetricInfo(TimeMetricValuesProvider.Category)]
	public class ExpectedTimeAttribute : MetricAttributeBase,
		IMetricAttribute<ExpectedTimeAttribute.ValuesProvider, TimeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="ExpectedTimeAttribute"/>.
		/// that returns timings in nanoseconds.
		/// </summary>
		internal class ValuesProvider : TimeMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(PercentileMetricCalculator.P95, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		public ExpectedTimeAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="timeUnit">The time unit.</param>
		public ExpectedTimeAttribute(double max, TimeUnit timeUnit = TimeUnit.Nanosecond) : base(max, timeUnit) { }

		/// <summary>Initializes a new instance of the <see cref="ExpectedTimeAttribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity"/> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="timeUnit">The time unit.</param>
		public ExpectedTimeAttribute(double min, double max, TimeUnit timeUnit = TimeUnit.Nanosecond)
			: base(min, max, timeUnit) { }

		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		public new TimeUnit UnitOfMeasurement => (TimeUnit?)base.UnitOfMeasurement ?? TimeUnit.Nanosecond;
	}
}