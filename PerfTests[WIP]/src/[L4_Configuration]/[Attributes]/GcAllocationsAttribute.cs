using System;

using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Binary size units
	/// </summary>
	public enum BinarySizeUnit : long
	{
		/// <summary>Binary size in bytes, B.</summary>
		[MetricUnit("B", RoundingDigits = 0)]
		Byte = 1,

		/// <summary>Binary size in kilobytes, KB.</summary>
		[MetricUnit("KB", AppliesFrom = (long)Kilobyte * 0.75)]
		Kilobyte = Byte * 1024,

		/// <summary>Binary size in megabytes, MB.</summary>
		[MetricUnit("MB", AppliesFrom = (long)Megabyte * 0.75)]
		Megabyte = Kilobyte * 1024,

		/// <summary>Binary size in gigabytes, GB.</summary>
		[MetricUnit("GB", AppliesFrom = (long)Gigabyte * 0.75)]
		Gigabyte = Megabyte * 1024,

#if !DOCFX
		/// <summary>Binary size in petabytes, PB.</summary>
		[MetricUnit("PB", AppliesFrom = (long)Petabyte * 0.75)]
		Petabyte = Gigabyte * 1024
#endif
	}

	/// <summary>
	/// Gc allocations metric attribute.
	/// NOTE: Noise allocations (total bytes allocated less than minimum gc allocation quantum) are reported as <c>0</c>
	/// </summary>
	[MetricInfo(GcMetricValuesProvider.Category, DefaultMinMetricValue.SameAsMax)]
	public class GcAllocationsAttribute : MetricAttributeBase,
		IMetricAttribute<GcAllocationsAttribute.ValuesProvider, BinarySizeUnit>
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
		public GcAllocationsAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amount of allocations.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double value, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(value, binarySize) { }

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
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
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte)
			: base(min, max, binarySize) { }

		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}

	/// <summary>GC 0 count per 1000 operations metric attribute.</summary>
	[MetricInfo(GcMetricValuesProvider.Category, CompactAttributeAnnotations = true)]
	public class Gc0Attribute : MetricAttributeBase, IMetricAttribute<Gc0Attribute.ValuesProvider>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="GcAllocationsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : GcMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(GcMetricSource.Gen0CollectionsPer1000, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="Gc0Attribute"/> class.</summary>
		public Gc0Attribute() { }

		/// <summary>Initializes a new instance of the <see cref="Gc0Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc0Attribute(double value) : base(value) { }

		/// <summary>Initializes a new instance of the <see cref="Gc0Attribute"/> class.</summary>
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
		public Gc0Attribute(double min, double max) : base(min, max) { }
	}

	/// <summary>GC 1 count per 1000 operations metric attribute.</summary>
	[MetricInfo(GcMetricValuesProvider.Category, CompactAttributeAnnotations = true)]
	public class Gc1Attribute : MetricAttributeBase, IMetricAttribute<Gc1Attribute.ValuesProvider>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="GcAllocationsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : GcMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(GcMetricSource.Gen1CollectionsPer1000, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="Gc1Attribute"/> class.</summary>
		public Gc1Attribute() { }

		/// <summary>Initializes a new instance of the <see cref="Gc1Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc1Attribute(double value) : base(value) { }

		/// <summary>Initializes a new instance of the <see cref="Gc1Attribute"/> class.</summary>
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
		public Gc1Attribute(double min, double max) : base(min, max) { }
	}

	/// <summary>GC 2 count per 1000 operations metric attribute.</summary>
	[MetricInfo(GcMetricValuesProvider.Category, CompactAttributeAnnotations = true)]
	public class Gc2Attribute : MetricAttributeBase, IMetricAttribute<Gc2Attribute.ValuesProvider>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="GcAllocationsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : GcMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(GcMetricSource.Gen2CollectionsPer1000, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="Gc2Attribute"/> class.</summary>
		public Gc2Attribute() { }

		/// <summary>Initializes a new instance of the <see cref="Gc2Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc2Attribute(double value) : base(value) { }

		/// <summary>Initializes a new instance of the <see cref="Gc2Attribute"/> class.</summary>
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
		public Gc2Attribute(double min, double max) : base(min, max) { }
	}
}