using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Description of metric unit of measurement.</summary>
	// ReSharper disable RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	// ReSharper restore RedundantAttributeUsageProperty
	[PublicAPI, MeansImplicitUse]
	public class MetricUnitAttribute : Attribute
	{
		private double _scaleCoefficient;
		private int _roundingDigits = -1;

		/// <summary>Initializes a new instance of the <see cref="MetricUnitAttribute"/> class.</summary>
		public MetricUnitAttribute() : this(null) { }

		/// <summary>Initializes a new instance of the <see cref="MetricUnitAttribute"/> class.</summary>
		/// <param name="displayName">The display name.</param>
		public MetricUnitAttribute([CanBeNull] string displayName)
		{
			DisplayName = displayName;
			ScaleCoefficient = double.NaN;
			AppliesFrom = double.NaN;
			RoundingDigits = -1;
		}

		/// <summary>
		/// Gets display name of the measurement unit.
		/// If is <c>null</c> the name of the enum member is used as a coefficient.
		/// </summary>
		/// <value>The display name of the measurement unit.</value>
		[CanBeNull]
		public string DisplayName { get; }

		/// <summary>
		/// Gets or sets scale coefficient for the measurement unit.
		/// If equals to <see cref="double.NaN"/> the value of the enum member is used as a coefficient.
		/// </summary>
		/// <value>The scale coefficient for the measurement unit.</value>
		public double ScaleCoefficient
		{
			get
			{
				return _scaleCoefficient;
			}
			set
			{
				if (!double.IsNaN(value))
				{
					Code.InRange(value, nameof(value), 0, double.MaxValue);
				}
				_scaleCoefficient = value;
			}
		}

		/// <summary>
		/// Gets or sets apply threshold for the measurement unit.
		/// If equals to <see cref="double.NaN"/> the value of the enum member is used.
		/// </summary>
		/// <value>The apply threshold for the measurement unit.</value>
		public double AppliesFrom { get; set; }

		/// <summary>
		/// Gets or sets number of fractional digits for storing and comparing metric values.
		/// If value is <c>-1</c> number of fractional digits detected automatically.
		/// </summary>
		/// <value>Number of fractional digits for storing and comparing metric values.</value>
		public int RoundingDigits
		{
			get
			{
				return _roundingDigits;
			}
			set
			{
				Code.InRange(value, nameof(value), -1, MetricUnit.MaxRoundingDigits);
				_roundingDigits = value;
			}
		}
	}
}