using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Metric measurement unit.</summary>
	public class MetricUnit
	{
		/// <summary>The maximum supported number of fractional digits for storing and comparing metric values.</summary>
		public const int MaxRoundingDigits = 15;

		/// <summary>Empty measurement unit.</summary>
		public static readonly MetricUnit Empty = new MetricUnit();

		#region Fields & .ctor
		private const string EmptyUnitName = "(" + nameof(Empty) + ")";

		/// <summary>Initializes a new instance of the <see cref="MetricUnit"/> class.</summary>
		private MetricUnit()
		{
			DisplayName = EmptyUnitName;
			ScaleCoefficient = 1;
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
		internal MetricUnit(
			[NotNull] string name,
			[NotNull] Enum enumValue,
			double scaleCoefficient,
			double appliesFrom,
			[CanBeNull] int? roundingDigits)
		{
			Code.NotNullNorEmpty(name, nameof(name));
			Code.NotNull(enumValue, nameof(enumValue));
			Code.AssertArgument(
				scaleCoefficient > 0 && scaleCoefficient <= double.MaxValue,
				nameof(scaleCoefficient),
				"The scale coefficient has to be non-zero positive numeric value.");
			Code.AssertArgument(
				appliesFrom >= 0 && appliesFrom <= double.MaxValue,
				nameof(appliesFrom),
				"The applies from value has to be zero or positive numeric value.");

			Code.InRange(roundingDigits ?? 0, nameof(roundingDigits), 0, MaxRoundingDigits);

			DisplayName = name;
			IsEmpty = false;
			EnumValue = enumValue;
			ScaleCoefficient = scaleCoefficient;
			AppliesFrom = appliesFrom;
			RoundingDigits = roundingDigits;
		}
		#endregion

		#region Properties
		/// <summary>Gets a value indicating whether this instance is empty measurement unit.</summary>
		/// <value>
		/// <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty { get; }

		/// <summary>Gets display name of the measurement unit.</summary>
		/// <value>The display name of the measurement unit.</value>
		[NotNull]
		public string DisplayName { get; }

		/// <summary>Gets enum value for the measurement unit.</summary>
		/// <value>The enum value for the measurement unit.</value>
		[CanBeNull]
		public Enum EnumValue { get; }

		/// <summary>Gets scale coefficient for the measurement unit.</summary>
		/// <value>The scale coefficient for the measurement unit.</value>
		public double ScaleCoefficient { get; }

		/// <summary> Gets apply threshold for the measurement unit.</summary>
		/// <value>The apply threshold for the measurement unit.</value>
		public double AppliesFrom { get; }

		/// <summary>
		/// Gets number of fractional digits for storing and comparing metric values.
		/// If value is <c>null</c> number of fractional digits detected automatically.
		/// </summary>
		/// <value>Number of fractional digits for storing and comparing metric values.</value>
		public int? RoundingDigits { get; }
		#endregion

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => DisplayName;
	}
}