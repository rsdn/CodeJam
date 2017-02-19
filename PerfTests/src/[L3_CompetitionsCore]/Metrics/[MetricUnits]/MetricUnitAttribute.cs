using System;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Description of metric unit of measurement.</summary>
	/// <seealso cref="System.Attribute"/>
	[AttributeUsage(AttributeTargets.Field)]
	public class MetricUnitAttribute : Attribute
	{
		private double _scaleCoefficient;
		private int _roundingDigits = -1;

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
		/// Gets the name of the metric unit. If is <c>null</c> the name of the enum member is used as a coefficient.
		/// </summary>
		/// <value>The name of the metric unit.</value>
		[CanBeNull]
		public string DisplayName { get; }

		/// <summary>
		/// Gets or sets the scale coefficient for the metric unit. If equals to <see cref="double.NaN"/>
		/// the value of the enum member is used as a coefficient.
		/// </summary>
		/// <value>The scale coefficient for the metric unit.</value>
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
		/// Gets or sets apply threshold for the metric unit. If equals to <see cref="double.NaN"/>
		/// the value of the enum member is used.
		/// </summary>
		/// <value>The apply threshold for the metric unit.</value>
		public double AppliesFrom { get; set; }

		/// <summary>
		/// Gets or sets number of fractional digits for storing and comparing metric values.
		/// Use the <c>-1</c> to use auto-rounding feature
		/// (used by default, behavior matches to <see cref="BenchmarkHelpers.GetRoundingDigits"/>)
		/// </summary>
		/// <value>Count of rounding digits.</value>
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

		/// <summary>Gets number of fractional digits for storing and comparing metric values.</summary>
		/// <returns>
		/// Number of fractional digits for storing and comparing metric values
		/// or <c>null</c> if auto-rounding feature shold be used.
		/// </returns>
		public int? GetRoundingDigitsNullable() => _roundingDigits > 0 ? _roundingDigits : default(int?);
	}
}