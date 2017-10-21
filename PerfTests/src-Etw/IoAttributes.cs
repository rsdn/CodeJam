using System;

using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Metrics.Etw;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// IO read bytes metric attribute.
	/// </summary>
	[MetricInfo(FileIoMetricProvider.Category, DefaultMinMetricValue.SameAsMax)]
	public class FileIoReadAttribute : MetricAttributeBase,
		IMetricAttribute<FileIoReadAttribute.ValuesProvider, BinarySizeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="FileIoReadAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : FileIoMetricProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(IoMetricSource.FileIoRead, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="FileIoReadAttribute"/> class.</summary>
		public FileIoReadAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="FileIoReadAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amount bytes read.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public FileIoReadAttribute(double value, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(value, binarySize) { }

		/// <summary>Initializes a new instance of the <see cref="FileIoReadAttribute"/> class.</summary>
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
		public FileIoReadAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte)
			: base(min, max, binarySize) { }

		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}

	/// <summary>
	/// IO write bytes metric attribute.
	/// </summary>
	[MetricInfo(FileIoMetricProvider.Category, DefaultMinMetricValue.SameAsMax, CompactAttributeAnnotations = true)]
	public class FileIoWriteAttribute : MetricAttributeBase,
		IMetricAttribute<FileIoWriteAttribute.ValuesProvider, BinarySizeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="FileIoWriteAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : FileIoMetricProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(IoMetricSource.FileIoWrite, false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="FileIoWriteAttribute"/> class.</summary>
		public FileIoWriteAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="FileIoWriteAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amount bytes write.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public FileIoWriteAttribute(double value, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(value, binarySize) { }

		/// <summary>Initializes a new instance of the <see cref="FileIoWriteAttribute"/> class.</summary>
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
		public FileIoWriteAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte)
			: base(min, max, binarySize) { }

		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}
}