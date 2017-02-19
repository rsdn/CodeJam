using System;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>The metric measurement unit.</summary>
	public class MetricUnit
	{
		/// <summary>The maximum supported number of fractional digits for storing and comparing metric values.</summary>
		public const int MaxRoundingDigits = 15;

		/// <summary>Empty metric measurement unit.</summary>
		public static readonly MetricUnit Empty = new MetricUnit();

		#region Fields & .ctor
		private const string EmptyMetricName = "(" + nameof(Empty) + ")";

		/// <summary>Initializes a new instance of the <see cref="MetricUnit"/> class.</summary>
		private MetricUnit()
		{
			Name = EmptyMetricName;
			IsEmpty = true;
		}

		/// <summary>Initializes a new instance of the <see cref="MetricUnit"/> class.</summary>
		/// <param name="name">The name of the metric measurement unit.</param>
		/// <param name="enumValue">Enum value for the measurement unit.</param>
		/// <param name="scaleCoefficient">The scale coefficient for the metric measurement unit.</param>
		/// <param name="appliesFrom">The apply threshold for the metric unit.</param>
		/// <param name="roundingDigits">
		/// Number of fractional digits for storing and comparing metric values
		/// or <c>null</c> if auto-rounding feature shold be used.
		/// </param>
		public MetricUnit(
			[NotNull] string name, [NotNull] Enum enumValue,
			double scaleCoefficient, double appliesFrom,
			[CanBeNull] int? roundingDigits)
		{
			Code.NotNullNorEmpty(name, nameof(name));
			Code.NotNull(enumValue, nameof(enumValue));
			Code.AssertArgument(
				scaleCoefficient > 0 && !scaleCoefficient.IsSpecialMetricValue(),
				nameof(scaleCoefficient),
				"The scale coefficient has to be non-zero positive numeric value.");
			Code.AssertArgument(
				appliesFrom >= 0 && !appliesFrom.IsSpecialMetricValue(),
				nameof(appliesFrom),
				"The applies from value has to be zero or positive numeric value.");
			Code.InRange(roundingDigits ?? 0, nameof(roundingDigits), 0, MaxRoundingDigits);

			Name = name;
			IsEmpty = false;
			EnumValue = enumValue;
			ScaleCoefficient = scaleCoefficient;
			AppliesFrom = appliesFrom;
			RoundingDigits = roundingDigits;
		}
		#endregion

		#region Properties
		/// <summary>Gets a value indicating whether this instance is empty metric unit.</summary>
		/// <value>
		/// <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty { get; }

		/// <summary>Gets the name of the metric measurement unit.</summary>
		/// <value>The name of the metric measurement unit.</value>
		[NotNull]
		public string Name { get; }

		/// <summary>Gets the enum value for the measurement unit.</summary>
		/// <value>The enum value for the measurement unit.</value>
		[CanBeNull]
		public Enum EnumValue { get; }

		/// <summary>Gets the scale coefficient for the metric measurement unit.</summary>
		/// <value>The scale coefficient for the metric measurement unit.</value>
		public double ScaleCoefficient { get; }

		/// <summary> Gets apply threshold for the metric unit.</summary>
		/// <value>The apply threshold for the metric unit.</value>
		public double AppliesFrom { get; }

		/// <summary>
		/// Gets number of fractional digits for storing and comparing metric values.
		/// If value is <c>null</c> auto-rounding feature used instead
		/// (behavior matches to <see cref="BenchmarkHelpers.GetRoundingDigits"/>)
		/// </summary>
		/// <value>Count of rounding digits.</value>
		public int? RoundingDigits { get; }
		#endregion

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => Name;
	}
}