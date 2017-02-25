using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Base interface for stored metric values.</summary>
	public interface IStoredMetricValue
	{
		/// <summary>Gets type of the attribute used for metric annotation.</summary>
		/// <value>The type of the attribute used for metric annotation.</value>
		[NotNull]
		Type MetricAttributeType { get; }

		/// <summary>Gets minimum value.</summary>
		/// <value>
		/// The minimum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity"/> returned if value is negative infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min"/> and <see cref="Max"/> values are scaled.
		/// Use the <see cref="MetricValueHelpers"/> to normalize them.
		/// </value>
		double Min { get; }

		/// <summary>Gets maximum value.</summary>
		/// <value>
		/// The maximum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.PositiveInfinity"/> returned if value is positive infinity (ignored, essentially).
		/// IMPORTANT: If the <see cref="UnitOfMeasurement"/> is not <c>null</c>
		/// both <see cref="Min"/> and <see cref="Max"/> values are scaled.
		/// Use the <see cref="MetricValueHelpers"/> to normalize them.
		/// </value>
		double Max { get; }

		/// <summary>Gets enum value for the measurement unit.</summary>
		/// <value>The enum value for the measurement unit.</value>
		[CanBeNull]
		Enum UnitOfMeasurement { get; }
	}
}