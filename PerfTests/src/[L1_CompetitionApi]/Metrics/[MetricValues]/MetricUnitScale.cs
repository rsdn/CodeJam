using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Ranges;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Metric measurement scale.</summary>
	public sealed class MetricUnitScale
	{
		#region Static members
		/// <summary>Empty measurement scale.</summary>
		public static readonly MetricUnitScale Empty = new MetricUnitScale();

		private static readonly Func<RuntimeTypeHandle, MetricUnitScale> _unitScalesCache = Algorithms.Memoize(
			(RuntimeTypeHandle metricEnumType) => new MetricUnitScale(Type.GetTypeFromHandle(metricEnumType)),
			true);

		/// <summary>Creates metric measurement scale.</summary>
		/// <param name="metricEnumType">
		/// Type of enum that defines metric unit values.
		/// Apply <see cref="MetricUnitAttribute"/> to the enum members to override metric unit values.
		/// </param>
		/// <returns>
		/// The metric measurement scale. Empty if <paramref name="metricEnumType"/> is <c>null</c>.
		/// </returns>
		[NotNull]
		public static MetricUnitScale FromEnumValues([CanBeNull] Type metricEnumType) =>
			metricEnumType == null ? Empty : _unitScalesCache(metricEnumType.TypeHandle);

		[NotNull]
		private static MetricUnit[] GetMetricUnits(Type metricEnumType)
		{
			var result = new List<MetricUnit>();
			var fields = EnumHelper.GetEnumValues(metricEnumType)
				.OrderBy(f => Convert.ToDouble(f.Value, CultureInfo.InvariantCulture));

			MetricUnit previousUnit = null;
			foreach (var field in fields)
			{
				var unit = GetMetricUnit(field.UnderlyingField);

				if (previousUnit != null)
				{
					Code.AssertArgument(
						previousUnit.AppliesFrom < unit.AppliesFrom,
						nameof(metricEnumType),
						$"The applies from value of {metricEnumType.Name}.{unit.EnumValue} ({unit.AppliesFrom.ToInvariantString()}) " +
							$"should be greater than prev unit {metricEnumType.Name}.{previousUnit.EnumValue} value ({previousUnit.AppliesFrom.ToInvariantString()})");
				}
				previousUnit = unit;

				result.Add(unit);
			}

			Code.AssertArgument(
				result.Count > 0,
				nameof(metricEnumType),
				$"The enum {metricEnumType} should be not empty.");

			return result.ToArray();
		}

		[NotNull]
		private static MetricUnit GetMetricUnit(FieldInfo metricUnitField)
		{
			var metricInfo = metricUnitField.GetCustomAttribute<MetricUnitAttribute>();

			var enumValue = (Enum)metricUnitField.GetValue(null);
			var coeff = metricInfo?.ScaleCoefficient ?? double.NaN;
			var appliesFrom = metricInfo?.AppliesFrom ?? double.NaN;
			var roundingDigits = metricInfo?.RoundingDigits;

			if (double.IsNaN(coeff))
			{
				coeff = Convert.ToDouble(enumValue, CultureInfo.InvariantCulture);
				if (coeff.Equals(0))
					coeff = 1;
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
				metricInfo?.DisplayName ?? metricUnitField.Name,
				enumValue,
				coeff,
				appliesFrom,
				roundingDigits);
		}
		#endregion

		#region Fields, .ctor & properties
		private readonly CompositeRange<double, MetricUnit> _unitScale;
		private readonly IReadOnlyDictionary<Enum, MetricUnit> _unitsByEnumValue;
		private readonly IReadOnlyDictionary<string, MetricUnit> _unitsByName;

		private MetricUnitScale()
		{
			MetricEnumType = null;

			_unitScale = CompositeRange<double, MetricUnit>.Empty;

			_unitsByEnumValue = new Dictionary<Enum, MetricUnit>();
			_unitsByName = new Dictionary<string, MetricUnit>();
		}

		private MetricUnitScale([NotNull] Type metricEnumType)
		{
			Code.NotNull(metricEnumType, nameof(metricEnumType));

			var metricUnits = GetMetricUnits(metricEnumType);

			MetricEnumType = metricEnumType;

			_unitScale = metricUnits
				.ToCompositeRangeFrom(s => s.AppliesFrom)
				.ExtendFrom(0); // Support for values less than first enum value.

			_unitsByEnumValue = metricUnits.ToDictionary(u => u.EnumValue);
			_unitsByName = metricUnits.ToDictionary(u => u.DisplayName, StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>Gets a value indicating whether the measurement scale is empty.</summary>
		/// <value><c>true</c> if the measurement scale is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => MetricEnumType == null;

		/// <summary>Gets type of the metric unit enum.</summary>
		/// <value>The type of the metric unit enum. <c>null</c> if <see cref="IsEmpty"/>.</value>
		[CanBeNull]
		public Type MetricEnumType { get; }
		#endregion

		#region Indexers
		/// <summary>Finds best applicable <see cref="MetricUnit"/> for measured value.</summary>
		/// <value>The <see cref="MetricUnit"/>.</value>
		/// <param name="measuredValue">The measured value.</param>
		/// <returns>The <see cref="MetricUnit"/> for measured value.</returns>
		[NotNull]
		public MetricUnit this[double measuredValue]
		{
			get
			{
				measuredValue = double.IsNaN(measuredValue) ? 0 : Math.Abs(measuredValue);

				return _unitScale.GetIntersection(measuredValue)
					.FirstOrDefault()
					.Key ?? MetricUnit.Empty;
			}
		}

		/// <summary>Finds best applicable <see cref="MetricUnit"/> for measured values.</summary>
		/// <value>The <see cref="MetricUnit"/>.</value>
		/// <param name="measuredValues">Range of measured values.</param>
		/// <returns>The <see cref="MetricUnit"/> for measured values.</returns>
		[NotNull]
		public MetricUnit this[MetricRange measuredValues]
		{
			get
			{
				var min = Math.Abs(measuredValues.Min);
				var max = Math.Abs(measuredValues.Max);

				return this[Math.Min(min, max)];
			}
		}

		/// <summary>Gets the <see cref="MetricUnit"/> with the specified enum value.</summary>
		/// <value>The <see cref="MetricUnit"/>.</value>
		/// <param name="enumValue">The enum value.</param>
		/// <returns>The <see cref="MetricUnit"/> with the specified coefficient.</returns>
		[NotNull]
		public MetricUnit this[Enum enumValue] =>
			IsEmpty || enumValue == null
				? MetricUnit.Empty
				: (_unitsByEnumValue.GetValueOrDefault(enumValue) ?? MetricUnit.Empty);

		/// <summary>Gets the <see cref="MetricUnit"/> with the specified name.</summary>
		/// <value>The <see cref="MetricUnit"/>.</value>
		/// <param name="displayName">The metric unit display name.</param>
		/// <returns>The <see cref="MetricUnit"/> with the specified coefficient.</returns>
		[NotNull]
		public MetricUnit this[string displayName] =>
			IsEmpty || displayName == null
				? MetricUnit.Empty
				: (_unitsByName.GetValueOrDefault(displayName) ?? MetricUnit.Empty);
		#endregion

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => MetricEnumType?.Name ?? MetricUnit.Empty.DisplayName;
	}
}