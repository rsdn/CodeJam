using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Description of metric unit of measurement.</summary>
	/// <seealso cref="System.Attribute"/>
	[AttributeUsage(AttributeTargets.Field)]
	public class MetricUnitAttribute : Attribute
	{
		private double _scaleCoefficient;

		/// <summary>Initializes a new instance of the <see cref="MetricUnitAttribute"/> class.</summary>
		/// <param name="displayName">The display name.</param>
		public MetricUnitAttribute([CanBeNull] string displayName)
		{
			DisplayName = displayName;
			ScaleCoefficient = double.NaN;
			AppliesFrom = double.NaN;
		}

		/// <summary>
		/// Gets the name of the metric unit. If is <c>null</c> the name of the enum member is used as a coefficient.
		/// </summary>
		/// <value>The name of the metric unit.</value>
		public string DisplayName { get; }

		/// <summary>
		/// Gets the scale coefficient for the metric unit. If equals to <see cref="double.NaN"/>
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
		/// Gets apply threshold for the metric unit. If equals to <see cref="double.NaN"/>
		/// the value of the enum member is used.
		/// </summary>
		/// <value>The apply threshold for the metric unit.</value>
		public double AppliesFrom { get; set; }
	}
}