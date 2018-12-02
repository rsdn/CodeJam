using System;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;

using CodeJam.PerfTests.Running.Helpers;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Metric description.
	/// </summary>
	/// <remarks>
	/// Instances of this type are cached to enable equality by reference semantic.
	/// DO NOT expose API that enables creation of multiple instances of the same metric.
	/// </remarks>
	// DONTTOUCH: see <remarks/>.
	public class MetricInfo
	{
		#region Factory methods
		private static readonly Func<RuntimeTypeHandle, MetricInfo> _metricInfoCache = Algorithms.Memoize(
			(RuntimeTypeHandle metricAttributeTypeHandle) =>
				(MetricInfo)Activator.CreateInstance(
					typeof(MetricInfo<>).MakeGenericType(Type.GetTypeFromHandle(metricAttributeTypeHandle)),
					true),
			true);

		/// <summary>Creates instance of the <see cref="MetricInfo{TAttribute}"/> class.</summary>
		/// <typeparam name="TAttribute">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricAttributeBase"/> as a base implementation.
		/// </typeparam>
		/// <returns>Instance of the <see cref="MetricInfo{TAttribute}"/>.</returns>
		public static MetricInfo<TAttribute> FromAttribute<TAttribute>()
			where TAttribute : Attribute, IStoredMetricValue =>
				(MetricInfo<TAttribute>)_metricInfoCache(typeof(TAttribute).TypeHandle);

		/// <summary>Creates instance of the <see cref="MetricInfo{TAttribute}"/> class.</summary>
		/// <param name="metricAttributeType">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricAttributeBase"/> as a base implementation..
		/// </param>
		/// <returns>Instance of the <see cref="MetricInfo{TAttribute}"/>.</returns>
		internal static MetricInfo FromAttribute(Type metricAttributeType) =>
			_metricInfoCache(metricAttributeType.TypeHandle);
		#endregion

		/// <summary>The primary metric.</summary>
		public static readonly MetricInfo<CompetitionBenchmarkAttribute> PrimaryMetric =
			FromAttribute<CompetitionBenchmarkAttribute>();

		#region Helpers
		/// <summary>Gets metric attribute information.</summary>
		/// <param name="metricAttributeType">Type of the metric attribute.</param>
		/// <returns>Metric attribute information, if any</returns>
		[CanBeNull]
		private static MetricInfoAttribute TryGetMetricInfoAttribute(Type metricAttributeType)
		{
			Code.NotNull(metricAttributeType, nameof(metricAttributeType));

			if (!typeof(Attribute).IsAssignableFrom(metricAttributeType))
				throw CodeExceptions.Argument(
					nameof(metricAttributeType),
					$"The {metricAttributeType} is not a Attribute type.");

			return metricAttributeType.GetCustomAttribute<MetricInfoAttribute>(false);
		}

		[CanBeNull]
		private static Type[] TryGetMetricAttributeTypeArgs(Type attributeType)
		{
			Type[] typeArgs = null;

			var attGenericInterfaces = attributeType.GetInterfaces()
				.Where(t => t.IsGenericType)
				.ToArray();

			var metricAttribute =
				attGenericInterfaces.FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(IMetricAttribute<,>))
					?? attGenericInterfaces.FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(IMetricAttribute<>));

			if (metricAttribute != null)
			{
				typeArgs = metricAttribute.GenericTypeArguments;

				Code.BugIf(typeArgs.Length < 1 || typeArgs.Length > 2, "typeArgs.Length < 1 || typeArgs.Length > 2");
				Code.BugIf(typeArgs.Any(t => t != Type.GetTypeFromHandle(t.TypeHandle)), "Incorrect instance of type.");
			}
			return typeArgs;
		}
		#endregion

		#region Fields & .ctor
		/// <summary>Initializes a new instance of the <see cref="MetricInfo"/> class.</summary>
		/// <param name="metricAttributeType">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricAttributeBase"/> as a base implementation.
		/// </param>
		/// <returns>Composite range that describes measurement units</returns>
		internal MetricInfo([NotNull] Type metricAttributeType)
		{
			// Performs arg validation
			var metricMeta = TryGetMetricInfoAttribute(metricAttributeType);

			if (typeof(CompetitionBenchmarkAttribute).IsAssignableFrom(metricAttributeType) &&
				metricAttributeType != typeof(CompetitionBenchmarkAttribute))
				throw CodeExceptions.Argument(
					nameof(metricAttributeType),
					$"Attributes derived from {nameof(CompetitionBenchmarkAttribute)} are not supported");

			var typeArgs = TryGetMetricAttributeTypeArgs(metricAttributeType);
			if (typeArgs == null)
				throw CodeExceptions.Argument(
					nameof(metricAttributeType),
					$"The {metricAttributeType} should implement {typeof(IMetricAttribute<>)} interface.");

			var displayName = metricMeta?.DisplayName;
			if (displayName.IsNullOrEmpty())
				displayName = metricAttributeType.GetShortAttributeName();
			DisplayName = displayName;

			AttributeType = metricAttributeType;
			IsPrimaryMetric = AttributeType == typeof(CompetitionBenchmarkAttribute);

			ValuesProvider = (IMetricValuesProvider)Activator.CreateInstance(typeArgs[0]);
			var enumType = typeArgs.Length < 2 ? null : typeArgs[1];
			MetricUnits = MetricUnitScale.FromEnumValues(enumType);

			if (metricMeta != null)
			{
				Category = metricMeta.Category;
				DefaultMinValue = metricMeta.DefaultMinValue;
				MetricColumns = metricMeta.MetricColumns;
				CompactAttributeAnnotations = metricMeta.CompactAttributeAnnotations;
			}
		}
		#endregion

		#region Values
		/// <summary>Gets display name of the metric.</summary>
		/// <value>The display name of the metric.</value>
		[NotNull]
		public string DisplayName { get; }

		/// <summary>Gets type of the metric attribute.</summary>
		/// <value>The type of the metric attribute.</value>
		[NotNull]
		public Type AttributeType { get; }

		/// <summary>Gets metric measurement scale.</summary>
		/// <value>The metric measurement scale.</value>
		[NotNull]
		public MetricUnitScale MetricUnits { get; }

		/// <summary>Gets metric values provider.</summary>
		/// <value>The metric values provider.</value>
		[NotNull]
		public IMetricValuesProvider ValuesProvider { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is a primary metric
		/// (a metric for <see cref="CompetitionBenchmarkAttribute"/>).
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is primary metric; otherwise, <c>false</c>.
		/// </value>
		public bool IsPrimaryMetric { get; }

		/// <summary>Gets a value indicating whether the metric is relative.</summary>
		/// <value> <c>true</c> if the metric is relative; otherwise, <c>false</c>. </value>
		public bool IsRelative => ValuesProvider.ResultIsRelative;

		/// <summary>Gets category of the metric.</summary>
		/// <value>The category of the metric.</value>
		[CanBeNull]
		public string Category { get; }

		/// <summary>Gets single value treatment mode.</summary>
		/// <value>The single value treatment mode.</value>
		public DefaultMinMetricValue DefaultMinValue { get; }

		/// <summary>Gets columns to include into summary output.</summary>
		/// <value>The columns to include into summary output.</value>
		public MetricValueColumns MetricColumns { get; }

		/// <summary>
		/// Place attribute annotation on a same line with same category attributes.
		/// </summary>
		/// <value>
		/// <c>true</c> if the attribute should be placed on same line with other attributes that belongs to the <see cref="Category"/>; otherwise, <c>false</c>.
		/// </value>
		public bool CompactAttributeAnnotations { get; }
		#endregion

		/// <summary>Gets column provider for the metric values.</summary>
		/// <returns>Column provider for the metric values</returns>
		[CanBeNull]
		public IColumnProvider GetColumnProvider() => ValuesProvider.GetColumnProvider(this, MetricValueColumns.Auto);

		/// <summary>Gets column provider for the metric values.</summary>
		/// <param name="columns">The columns to include.</param>
		/// <returns>Column provider for the metric values</returns>
		[CanBeNull]
		public IColumnProvider GetColumnProvider(MetricValueColumns columns) =>
			ValuesProvider.GetColumnProvider(this, columns);

		/// <summary>Gets diagnosers for the metric values.</summary>
		/// <returns>Diagnosers for the metric values</returns>
		[NotNull]
		public IDiagnoser[] GetDiagnosers() => ValuesProvider.GetDiagnosers(this);

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => DisplayName;
	}
}