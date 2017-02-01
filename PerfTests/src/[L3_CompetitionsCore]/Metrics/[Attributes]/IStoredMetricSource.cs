using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Base interface for stored metrics.</summary>
	public interface IStoredMetricSource
	{
		/// <summary>Gets the type of the attribute used for metric annotation.</summary>
		/// <value>The type of the attribute used for metric annotation.</value>
		[NotNull]
		Type MetricAttributeType { get; }

		/// <summary>Minimum value.</summary>
		/// <value>
		/// The minimum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity"/> returned if value is negative infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min"/> and <see cref="Max"/> values are scaled.
		/// Use the <see cref="MetricUnits"/> to normalize them.
		/// </value>
		double Min { get; }

		/// <summary>Maximum value.</summary>
		/// <value>
		/// The maximum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> returned if value is positive infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min"/> and <see cref="Max"/> values are scaled.
		/// Use the <see cref="MetricUnits"/> to normalize them.
		/// </value>
		double Max { get; }

		/// <summary>The value that represents measurement unit for the metric value.</summary>
		/// <value>The value that represents measurement unit for the metric value.</value>
		[CanBeNull]
		Enum UnitOfMeasurement { get; }
	}
}