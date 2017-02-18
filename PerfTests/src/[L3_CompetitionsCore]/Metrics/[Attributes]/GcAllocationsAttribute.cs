using System;

using BenchmarkDotNet.Engines;

using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Binary size units
	/// </summary>
	public enum BinarySizeUnit : long
	{
		/// <summary>Binary size in bytes.</summary>
		[MetricUnit("B", AppliesFrom = 0, RoundingDigits = 0)]
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

	/// <summary>
	/// Gc allocations metric attribute.
	/// As perftests may be run inprocess and there can be accidental allocation from test enfine, first page allocation is ignored.
	/// If <see cref="GcStats.AllocatedBytes"/> less than or equal to <see cref="GcMetricValuesProvider.MinimalGcAllocation"/> zero allocation is reported.
	/// </summary>
	[MetricAttribute(MetricSingleValueMode.BothMinAndMax, Category = GcMetricValuesProvider.Category)]
	public class GcAllocationsAttribute : MetricBaseAttribute, IMetricAttribute<GcAllocationsAttribute.ValuesProvider, BinarySizeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="GcAllocationsAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : GcMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(GcMetricSource.BytesAllocatedPerOperationIgnoreNoise, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
		public GcAllocationsAttribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="GcAllocationsAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amout of allocations.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double value, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(value, binarySize)
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
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public GcAllocationsAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(min, max, binarySize)
		{
		}

		/// <summary>Gets the unit of measurement for the metric.</summary>
		/// <value>The unit of measurement.</value>
		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}

	/// <summary>GC 0 count per 1000 operations metric attribute.</summary>
	[MetricAttribute(AnnotateInplace=true, Category = GcMetricValuesProvider.Category)]
	public class Gc0Attribute : MetricBaseAttribute, IMetricAttribute<Gc0Attribute.ValuesProvider>
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
		public Gc0Attribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc0Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc0Attribute(double value) : base(value)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc0Attribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc0Attribute(double min, double max) : base(min, max)
		{
		}
	}

	/// <summary>GC 1 count per 1000 operations metric attribute.</summary>
	[MetricAttribute(AnnotateInplace = true, Category = GcMetricValuesProvider.Category)]
	public class Gc1Attribute : MetricBaseAttribute, IMetricAttribute<Gc1Attribute.ValuesProvider>
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
		public Gc1Attribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc1Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc1Attribute(double value) : base(value)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc1Attribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc1Attribute(double min, double max) : base(min, max)
		{
		}
	}

	/// <summary>GC 2 count per 1000 operations metric attribute.</summary>
	[MetricAttribute(AnnotateInplace = true, Category = GcMetricValuesProvider.Category)]
	public class Gc2Attribute : MetricBaseAttribute, IMetricAttribute<Gc2Attribute.ValuesProvider>
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
		public Gc2Attribute()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc2Attribute"/> class.</summary>
		/// <param name="value">
		/// Count of GC per 1000 operations.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc2Attribute(double value) : base(value)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="Gc2Attribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity" /> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> if value is positive infinity (ignored, essentially).
		/// </param>
		public Gc2Attribute(double min, double max) : base(min, max)
		{
		}
	}
}