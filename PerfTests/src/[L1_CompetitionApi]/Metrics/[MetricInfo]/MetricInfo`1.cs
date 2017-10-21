using System;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Typed metric description.
	/// Use <see cref="MetricInfo.FromAttribute{TAttribute}"/> to create instance of the objec
	/// </summary>
	/// <typeparam name="TAttribute">
	/// Type of the attribute used for metric annotation.
	/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or
	/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
	/// you can use <see cref="MetricAttributeBase"/> as a base implementation.
	/// </typeparam>
	/// <remarks>
	/// Instances of this type are cached to enable equality by reference semantic.
	/// DO NOT expose API that enables creation of multiple instances of the same metric.
	/// </remarks>
	// DONTTOUCH: see <remarks/>.
	public sealed class MetricInfo<TAttribute> : MetricInfo
		where TAttribute : Attribute, IStoredMetricValue
	{
		/// <summary>Initializes a new instance of the <see cref="MetricInfo{TAttribute}"/> class.</summary>
		internal MetricInfo() : base(typeof(TAttribute)) { }
	}
}