using System;

using static CodeJam.PerfTests.Metrics.MetricRange;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Base metric attribute implementation.
	/// </summary>
	/// <seealso cref="System.Attribute" />
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public abstract class MetricBaseAttribute : Attribute, IStoredMetricSource
	{
		// TODO: metric ignore rule enum? (AFAIR rejected)

		/// <summary>Marks the metric range as empty (unset but updateable during the annotation).</summary>
		protected MetricBaseAttribute(): this (EmptyMetricValue, EmptyMetricValue, null)
		{
		}

		/// <summary>
		/// Sets max value of the metric range. The min value set to negative infinity.
		/// </summary>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
		/// IMPORTANT: If the <paramref name="unitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min" /> and <see cref="Max" /> values are scaled.
		/// Use the <see cref="MetricUnits" /> to normalize them.
		/// </param>
		/// <param name="unitOfMeasurement">The value that represents measurement unit for the scale.</param>
		protected MetricBaseAttribute(double max, Enum unitOfMeasurement = null): this(FromNegativeInfinity, max, unitOfMeasurement)
		{
		}

		/// <summary>Sets range values of the attribute.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> should be used if value is negative infinity (ignored, essentially).
		/// IMPORTANT: If the <paramref name="unitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min" /> and <see cref="Max" /> values are scaled.
		/// Use the <see cref="MetricUnits" /> to normalize them.
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> should be used if value is positive infinity (ignored, essentially).
		/// IMPORTANT: If the <paramref name="unitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min" /> and <see cref="Max" /> values are scaled.
		/// Use the <see cref="MetricUnits" /> to normalize them.
		/// </param>
		/// <param name="unitOfMeasurement">The value that represents measurement unit for the scale.</param>
		protected MetricBaseAttribute(double min, double max, Enum unitOfMeasurement = null)
		{
			Min = min;
			Max = max;
			UnitOfMeasurement = unitOfMeasurement;
		}

		/// <summary>Minimum value.</summary>
		/// <value>
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> returned if value is negative infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement" /> is not <c>null</c>
		/// both <see cref="Min" /> and <see cref="Max" /> values are scaled.
		/// Use the <see cref="MetricUnits" /> to normalize them.
		/// </value>
		public double Min { get; }

		/// <summary>Maximum value.</summary>
		/// <value>
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement" /> is not <c>null</c>
		/// both <see cref="Min" /> and <see cref="Max" /> values are scaled.
		/// Use the <see cref="MetricUnits" /> to normalize them.
		/// </value>
		public double Max { get; }

		/// <summary>The value that represents measurement unit for the scale.</summary>
		/// <value>The value that represents measurement unit for the scale.</value>
		public Enum UnitOfMeasurement { get; }

		/// <summary>Gets the type of the attribute used for metric annotation.</summary>
		/// <value>The type of the attribute used for metric annotation.</value>
		Type IStoredMetricSource.MetricAttributeType => GetType();
	}
}