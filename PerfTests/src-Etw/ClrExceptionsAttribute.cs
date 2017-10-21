using System;

using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Metrics.Etw;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// CLR exceptions thrown attribute.
	/// </summary>
	[MetricInfo(ClrExceptionsProvider.Category, DefaultMinMetricValue.SameAsMax)]
	public class ClrExceptionsAttribute : MetricAttributeBase,
		IMetricAttribute<ClrExceptionsAttribute.ValuesProvider>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="ClrExceptionsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : ClrExceptionsProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="ClrExceptionsAttribute"/> class.</summary>
		public ClrExceptionsAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="ClrExceptionsAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amount bytes read.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		public ClrExceptionsAttribute(double value) : base(value) { }

		/// <summary>Initializes a new instance of the <see cref="ClrExceptionsAttribute"/> class.</summary>
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
		public ClrExceptionsAttribute(double min, double max)
			: base(min, max) { }
	}
}