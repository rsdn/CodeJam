using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Description of metric unit of measurement.</summary>
	/// <seealso cref="System.Attribute"/>
	[AttributeUsage(AttributeTargets.Field)]
	public class MetricUnitAttribute : Attribute
	{
		/// <summary>Initializes a new instance of the <see cref="MetricUnitAttribute"/> class.</summary>
		/// <param name="displayName">The display name.</param>
		public MetricUnitAttribute([CanBeNull] string displayName)
		{
			DisplayName = displayName;
			ScaleCoefficient = double.NaN;
		}

		/// <summary>Initializes a new instance of the <see cref="MetricUnitAttribute"/> class.</summary>
		/// <param name="displayName">
		/// The name of the metric unit. If is <c>null</c> the name of the enum member is used.
		/// </param>
		/// <param name="scaleCoefficient">
		/// The scale coefficient for the metric unit. If equals to <see cref="double.NaN"/>
		/// the value of the enum member is used as a coefficient.
		/// </param>
		public MetricUnitAttribute([CanBeNull] string displayName, double scaleCoefficient)
		{
			if (!double.IsNaN(scaleCoefficient))
			{
				Code.InRange(scaleCoefficient, nameof(scaleCoefficient), 0, double.PositiveInfinity);
				Code.AssertArgument(
					!scaleCoefficient.IsSpecialMetricValue(),
					nameof(scaleCoefficient),
					"The scale coefficient has to be a valid value.");
			}

			DisplayName = displayName;
			ScaleCoefficient = scaleCoefficient;
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
		// TODO: to nullable?
		public double ScaleCoefficient { get; }
	}
}