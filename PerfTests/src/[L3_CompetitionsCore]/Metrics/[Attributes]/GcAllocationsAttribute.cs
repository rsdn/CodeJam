using System;

using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Binary size units
	/// </summary>
	public enum BinarySizeUnit: long
	{
		/// <summary>Binary size in bytes.</summary>
		[MetricUnit("B", AppliesFrom = 0)]
		Byte = 1,
		/// <summary>Binary size in kilobytes.</summary>
		[MetricUnit("KB", AppliesFrom = (long)Kilobyte / 2.0)]
		Kilobyte = Byte * 1024,
		/// <summary>Binary size in megabytes.</summary>
		[MetricUnit("MB", AppliesFrom = (long)Megabyte / 2.0)]
		Megabyte = Kilobyte * 1024,
		/// <summary>Binary size in gigabytes.</summary>
		[MetricUnit("GB", AppliesFrom = (long)Gigabyte / 2.0)]
		Gigabyte = Megabyte * 1024,
		/// <summary>Binary size in petabytes.</summary>
		[MetricUnit("PB", AppliesFrom = (long)Petabyte / 2.0)]
		Petabyte = Gigabyte * 1024
	}

	/// <summary>Gc allocations metric attribute.</summary>
	public class GcAllocationsAttribute : MetricBaseAttribute, IMetricAttribute<GcAllocationsAttribute.ValuesProvider, BinarySizeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="GcAllocationsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : GcMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(GcMetricSource.BytesAllocatedPerOperation, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
		public GcAllocationsAttribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(max, binarySize)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
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
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(min, max, binarySize)
		{
		}

		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}
}