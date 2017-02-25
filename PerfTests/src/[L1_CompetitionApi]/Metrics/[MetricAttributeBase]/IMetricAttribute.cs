using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Interface for custom metric attributes that has no unit of measurement.
	/// </summary>
	/// <typeparam name="TMetricProvider">The type of the metric provider.</typeparam>
	/// <seealso cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>
	/// <seealso cref="IStoredMetricValue"/>
	[PublicAPI]
	public interface IMetricAttribute<[UsedImplicitly] TMetricProvider> : IStoredMetricValue
		where TMetricProvider : IMetricValuesProvider, new() { }

	/// <summary>Interface for custom metric attributes that has unit of measurement.</summary>
	/// <typeparam name="TMetricProvider">The type of the metric provider.</typeparam>
	/// <typeparam name="TUnitOfMeasurement">The type of the unit of measurement.</typeparam>
	/// <seealso cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>
	/// <seealso cref="IStoredMetricValue"/>
	// ReSharper disable once TypeParameterCanBeVariant
	public interface IMetricAttribute<TMetricProvider, TUnitOfMeasurement> : IMetricAttribute<TMetricProvider>
		where TMetricProvider : IMetricValuesProvider, new()
		where TUnitOfMeasurement : struct
	{
		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		new TUnitOfMeasurement UnitOfMeasurement { get; }
	}
}