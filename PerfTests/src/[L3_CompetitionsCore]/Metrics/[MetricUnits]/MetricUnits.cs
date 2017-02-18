using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Ranges;
using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Metric measurement scale
	/// </summary>
	public sealed class MetricUnits
	{
		#region Static members
		/// <summary>Empty measurement scale.</summary>
		public static readonly MetricUnits Empty = new MetricUnits();

		private static readonly Func<RuntimeTypeHandle, MetricUnits> _metricUnitsCache = Algorithms.Memoize(
			(RuntimeTypeHandle metricEnumType) => new MetricUnits(Type.GetTypeFromHandle(metricEnumType)));

		/// <summary>Creates the metric measurement scale.</summary>
		/// <param name="metricEnumType">
		/// Type of the metric unit enum.
		/// Use the <see cref="MetricUnitAttribute"/> on enum members to override display name or scaling coefficient.
		/// </param>
		/// <returns>The metric measurement scale. Empty if <paramref name="metricEnumType"/> is <c>null</c>.</returns>
		[NotNull]
		public static MetricUnits GetMetricUnits([CanBeNull] Type metricEnumType) =>
			metricEnumType == null ? Empty : _metricUnitsCache(metricEnumType.TypeHandle);

		/// <summary>Helper method that returns measuremet units from the metric unit enum.</summary>
		/// <param name="metricUnitEnumType">
		/// Type of the metric unit enum.
		/// Use the <see cref="MetricUnitAttribute"/> on enum members to override display name or scaling coefficient.
		/// </param>
		/// <returns>Composite range that describes measurement units</returns>
		private static MetricUnit[] GetMetricUnitsCore(Type metricUnitEnumType) =>
			ReflectionEnumHelper.GetFields(metricUnitEnumType).ConvertAll(
				f =>
				{
					var metricUnit = f.GetCustomAttribute<MetricUnitAttribute>();
					var enumValue = (Enum)f.GetValue(null);
					var coeff = metricUnit?.ScaleCoefficient ?? double.NaN;
					var appliesFrom = metricUnit?.AppliesFrom ?? double.NaN;
					var roundingDigits = metricUnit?.RoundingDigits;
					if (double.IsNaN(coeff))
					{
						coeff = Convert.ToDouble(enumValue, CultureInfo.InvariantCulture);
					}
					if (double.IsNaN(appliesFrom))
					{
						appliesFrom = Convert.ToDouble(enumValue, CultureInfo.InvariantCulture);
					}
					if (roundingDigits < 0)
					{
						roundingDigits = null;
					}
					return new MetricUnit(
						metricUnit?.DisplayName ?? f.Name,
						enumValue,
						coeff,
						appliesFrom,
						roundingDigits);
				});

		// DONTTOUCH: empty (double.NaN) values are handled automatically.
		private static double GetUnitSearchValue(MetricRange metricValues)
		{
			var min = Math.Abs(metricValues.Min);
			var max = Math.Abs(metricValues.Max);

			return double.IsNaN(min) ? max : Math.Min(min, max);
		}
		#endregion

		#region Fields, .ctor & properties
		private readonly CompositeRange<double, MetricUnit> _unitScale;
		private readonly IReadOnlyDictionary<Enum, MetricUnit> _unitsByEnumValue;
		private readonly IReadOnlyDictionary<string, MetricUnit> _unitsByName;

		/// <summary>Initializes a new instance of empty <see cref="MetricUnits" /> class.</summary>
		private MetricUnits()
		{
			MetricEnumType = null;

			_unitScale = CompositeRange<double, MetricUnit>.Empty;

			_unitsByEnumValue = new Dictionary<Enum, MetricUnit>();
			_unitsByName = new Dictionary<string, MetricUnit>();
		}

		/// <summary>Initializes a new instance of the <see cref="MetricUnits" /> class.</summary>
		/// <param name="metricEnumType">Type of the metric enum.
		/// Type of the metric unit enum.
		/// Use the <see cref="MetricUnitAttribute"/> on enum members to override display name or scaling coefficient.
		/// </param>
		private MetricUnits([NotNull] Type metricEnumType)
		{
			Code.NotNull(metricEnumType, nameof(metricEnumType));

			var metricUnits = GetMetricUnitsCore(metricEnumType);

			MetricEnumType = metricEnumType;

			_unitScale = metricUnits
				.ToCompositeRangeFrom(s => s.AppliesFrom)
				.ExtendFrom(RangeBoundaryFrom<double>.NegativeInfinity);

			_unitsByEnumValue = metricUnits.ToDictionary(u => u.EnumValue);
			_unitsByName = metricUnits.ToDictionary(u => u.Name, StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>Gets a value indicating whether the measurement scale is empty.</summary>
		/// <value><c>true</c> if the measurement scale is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => MetricEnumType == null;

		/// <summary>Type of the metric unit enum.</summary>
		/// <value>The type of the metric unit enum. <c>null</c> if <see cref="IsEmpty"/>.</value>
		[CanBeNull]
		public Type MetricEnumType { get; }
		#endregion

		#region Indexers
		/// <summary>Gets the best <see cref="MetricUnit" /> for the measured value.</summary>
		/// <value>The <see cref="MetricUnit" />.</value>
		/// <param name="measuredValue">The measured value.</param>
		/// <returns>The <see cref="MetricUnit" /> for the measured value.</returns>
		[NotNull]
		public MetricUnit this[double measuredValue] => 
			_unitScale.GetIntersection(measuredValue)
				.FirstOrDefault()
				.Key ?? MetricUnit.Empty;

		/// <summary>Gets the best <see cref="MetricUnit" /> for the measured value.</summary>
		/// <value>The <see cref="MetricUnit" />.</value>
		/// <param name="measuredValues">Range of measured values.</param>
		/// <returns>The <see cref="MetricUnit" /> for the measured value.</returns>
		[NotNull]
		public MetricUnit this[MetricRange measuredValues] => 
			_unitScale.GetIntersection(GetUnitSearchValue(measuredValues))
				.FirstOrDefault()
				.Key ?? MetricUnit.Empty;

		/// <summary>Gets the <see cref="MetricUnit" /> with the specified enum value.</summary>
		/// <value>The <see cref="MetricUnit" />.</value>
		/// <param name="enumValue">The enum value.</param>
		/// <returns>The <see cref="MetricUnit" /> with the specified coefficient.</returns>
		[NotNull]
		public MetricUnit this[Enum enumValue] => IsEmpty || enumValue == null 
			? MetricUnit.Empty 
			: (_unitsByEnumValue.GetValueOrDefault(enumValue) ?? MetricUnit.Empty);

		/// <summary>Gets the <see cref="MetricUnit" /> with the specified name.</summary>
		/// <value>The <see cref="MetricUnit" />.</value>
		/// <param name="name">The name.</param>
		/// <returns>The <see cref="MetricUnit" /> with the specified coefficient.</returns>
		[NotNull]
		public MetricUnit this[string name] => IsEmpty 
			? MetricUnit.Empty
			: (_unitsByName.GetValueOrDefault(name) ?? MetricUnit.Empty);
		#endregion

		/// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
		/// <returns>A <see cref="string" /> that represents this instance.</returns>
		public override string ToString() => MetricEnumType?.Name ?? MetricUnit.Empty.Name;
	}
}