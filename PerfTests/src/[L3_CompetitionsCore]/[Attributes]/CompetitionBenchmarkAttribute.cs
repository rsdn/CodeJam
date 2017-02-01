using System;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Metrics.MetricRange;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for competition benchmark.</summary>
	/// <seealso cref="BenchmarkAttribute"/>
	// DONTTOUCH: DO NOT change Inherited = false as it will break annotation system
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionBenchmarkAttribute : BenchmarkAttribute,
		IMetricAttribute<CompetitionBenchmarkAttribute.RelativeTimeValuesProvider>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="CompetitionBenchmarkAttribute"/>.
		/// </summary>
		internal class RelativeTimeValuesProvider : TimeMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="RelativeTimeValuesProvider"/> class.</summary>
			public RelativeTimeValuesProvider() : base(LogNormalMetricCalculator.Instance, true) { }
		}

		/// <summary>Marks the competition benchmark.</summary>
		public CompetitionBenchmarkAttribute() : this(EmptyMetricValue, EmptyMetricValue)
		{
		}

		/// <summary>Marks the competition benchmark.</summary>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN" /> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity" /> returned if value is positive infinity (ignored, essentially).
		/// </param>
		public CompetitionBenchmarkAttribute(double max): this(FromNegativeInfinity, max)
		{
		}

		/// <summary>Marks the competition benchmark.</summary>
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
		public CompetitionBenchmarkAttribute(double min, double max)
		{
			Min = min;
			Max = max;
			UnitOfMeasurement = null;
		}

		/// <summary>Exclude the benchmark from competition.</summary>
		/// <value>
		/// <c>true</c> if the benchmark does not take part in competition
		/// and should not be validated.
		/// </value>
		public bool DoesNotCompete { get; set; }

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

		/// <summary>The value that represents measurement unit for the metric value.</summary>
		/// <value>The value that represents measurement unit for the metric value.</value>
		public Enum UnitOfMeasurement { get; set; }

		/// <summary>Gets the type of the attribute used for metric annotation.</summary>
		/// <value>The type of the attribute used for metric annotation.</value>
		Type IStoredMetricSource.MetricAttributeType => typeof(CompetitionBenchmarkAttribute);
	}
}