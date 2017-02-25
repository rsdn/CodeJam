using System;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Helpers for the <see cref="MetricInfo"/>
	/// </summary>
	public static class MetricInfoHelpers
	{
		/// <summary>Gets the name of the attribute without 'Attribute' suffix.</summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <returns>Name of the attribute without 'Attribute' suffix.</returns>
		public static string GetShortAttributeName([NotNull] this Type attributeType)
		{
			Code.NotNull(attributeType, nameof(attributeType));

			if (!typeof(Attribute).IsAssignableFrom(attributeType))
				throw CodeExceptions.Argument(
					nameof(attributeType),
					$"The {attributeType} is not a Attribute type.");

			var attributeName = attributeType.Name;
			if (attributeName.EndsWith(nameof(Attribute)))
				attributeName = attributeName.Substring(0, attributeName.Length - nameof(Attribute).Length);

			return attributeName;
		}

		private static readonly Func<Type, MetricInfoAttribute> _metricAttributesCache = Algorithms.Memoize(
			(Type t) => t.GetCustomAttribute<MetricInfoAttribute>(false),
			true);

		/// <summary>Gets metric attribute information.</summary>
		/// <param name="metricAttributeType">Type of the metric attribute.</param>
		/// <returns>Metric attribute information, if any</returns>
		[CanBeNull]
		public static MetricInfoAttribute GetMetricInfoAttribute(Type metricAttributeType)
		{
			Code.NotNull(metricAttributeType, nameof(metricAttributeType));

			if (!typeof(Attribute).IsAssignableFrom(metricAttributeType))
				throw CodeExceptions.Argument(
					nameof(metricAttributeType),
					$"The {metricAttributeType} is not a Attribute type.");

			return _metricAttributesCache(metricAttributeType);
		}

		private static MetricSingleValueMode GetSingleValueMode(Type metricAttributeType) =>
			GetMetricInfoAttribute(metricAttributeType)?.SingleValueMode
				?? MetricSingleValueMode.FromZeroToMax;

		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metricAttributeType">Type of the metric attribute.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(
			this double metricMaxValue,
			[NotNull] Type metricAttributeType) =>
				GetMinMetricValue(
					metricMaxValue,
					GetSingleValueMode(metricAttributeType));
		
		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns>Minimum metric value.</returns>
		public static double GetMinMetricValue(
			this double metricMaxValue,
			[NotNull] MetricInfo metric) =>
				GetMinMetricValue(metricMaxValue, metric.SingleValueMode);

		/// <summary>Returns minimum metric value.</summary>
		/// <param name="metricMaxValue">The maximum metric value.</param>
		/// <param name="singleValueMode">The single value treatment mode.</param>
		/// <returns>Minimum metric value.</returns>
		private static double GetMinMetricValue(
			this double metricMaxValue,
			MetricSingleValueMode singleValueMode)
		{
			if (metricMaxValue.Equals(0))
				return 0;
			switch (singleValueMode)
			{
				case MetricSingleValueMode.FromZeroToMax:
					return 0;
				case MetricSingleValueMode.FromInfinityToMax:
					return double.NegativeInfinity;
				case MetricSingleValueMode.BothMinAndMax:
					return double.IsInfinity(metricMaxValue) ? double.NegativeInfinity : metricMaxValue;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(singleValueMode), singleValueMode);
			}
		}

		/// <summary>Determines if the minimum metric value should be stored.</summary>
		/// <param name="metricRange">The metric range.</param>
		/// <param name="metricUnit">The metric unit.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns><c>true</c>, if the minimum metric value should be stored.</returns>
		public static bool ShouldStoreMinMetricValue(
			this MetricRange metricRange, MetricUnit metricUnit,
			[NotNull] MetricInfo metric) =>
				ShouldStoreMinMetricValue(metricRange, metricUnit, metric.SingleValueMode);


		/// <summary>Determines if the minimum metric value should be stored.</summary>
		/// <param name="metricRange">The metric range.</param>
		/// <param name="metricUnit">The metric unit.</param>
		/// <param name="singleValueMode">The single value treatment mode.</param>
		/// <returns><c>true</c>, if the minimum metric value should be stored.</returns>
		public static bool ShouldStoreMinMetricValue(
			this MetricRange metricRange, MetricUnit metricUnit,
			MetricSingleValueMode singleValueMode)
		{
			switch (singleValueMode)
			{
				case MetricSingleValueMode.FromZeroToMax:
					return !metricRange.Min.Equals(0);
				case MetricSingleValueMode.FromInfinityToMax:
					return !double.IsInfinity(metricRange.Min);
				case MetricSingleValueMode.BothMinAndMax:
					return double.IsInfinity(metricRange.Min) || !metricRange.MinMaxAreSame(metricUnit);
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(singleValueMode), singleValueMode);
			}
		}
	}
}