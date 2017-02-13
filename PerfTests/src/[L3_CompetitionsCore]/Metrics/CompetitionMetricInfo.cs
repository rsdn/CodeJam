using System;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Helpers;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>
	/// Competition metric description.
	/// </summary>
	public class CompetitionMetricInfo
	{
		#region Factory methods
		private static readonly Func<RuntimeTypeHandle, CompetitionMetricInfo> _metricsCache = Algorithms.Memoize(
			(RuntimeTypeHandle t) => (CompetitionMetricInfo)Activator.CreateInstance(Type.GetTypeFromHandle(t), true));

		/// <summary>Creates instance of the <see cref="CompetitionMetricInfo{TAttribute}" /> class.</summary>
		/// <typeparam name="TAttribute">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or 
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricBaseAttribute"/> as a base implementation.
		/// </typeparam>
		/// <returns>Instance of the <see cref="CompetitionMetricInfo{TAttribute}" />.</returns>
		public static CompetitionMetricInfo<TAttribute> Create<TAttribute>() where TAttribute : Attribute, IStoredMetricSource =>
			(CompetitionMetricInfo<TAttribute>)_metricsCache(typeof(CompetitionMetricInfo<TAttribute>).TypeHandle);
		#endregion

		// TODO: to L7 all together
		#region Known metrics
		/// <summary>The relative time metric. </summary>
		public static readonly CompetitionMetricInfo<CompetitionBenchmarkAttribute> RelativeTime =
			Create<CompetitionBenchmarkAttribute>();

		/// <summary>The absolute time metic, in nanoseconds.</summary>
		public static readonly CompetitionMetricInfo<ExpectedTimeAttribute> AbsoluteTime =
			Create<ExpectedTimeAttribute>();

		/// <summary>Gc allocations metic, in bytes.</summary>
		public static readonly CompetitionMetricInfo<GcAllocationsAttribute> GcAllocations =
			Create<GcAllocationsAttribute>();

		/// <summary>GC 0 count metric.</summary>
		public static readonly CompetitionMetricInfo<Gc0Attribute> Gc0 = Create<Gc0Attribute>();

		/// <summary>GC 1 count metric.</summary>
		public static readonly CompetitionMetricInfo<Gc1Attribute> Gc1 = Create<Gc1Attribute>();

		/// <summary>GC 2 count metric.</summary>
		public static readonly CompetitionMetricInfo<Gc2Attribute> Gc2 = Create<Gc2Attribute>();
		#endregion


		#region Helpers
		private static Type[] GetMetricAttributeTypeArgs(Type attributeType)
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
		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricInfo"/> class.</summary>
		/// <param name="attributeType">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or 
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricBaseAttribute"/> as a base implementation.
		/// </param>
		/// <returns>Composite range that describes measurement units</returns>
		internal CompetitionMetricInfo([NotNull] Type attributeType)
		{
			Code.NotNull(attributeType, nameof(attributeType));

			if (!typeof(Attribute).IsAssignableFrom(attributeType))
				throw CodeExceptions.Argument(
					nameof(attributeType), $"The {nameof(attributeType)} should be derived from {typeof(Attribute)}");

			var typeArgs = GetMetricAttributeTypeArgs(attributeType);
			if (typeArgs == null)
				throw CodeExceptions.Argument(
					nameof(attributeType),
					$"The {attributeType} should implement {typeof(IMetricAttribute<>)} interface.");

			var metricMeta = attributeType.GetCustomAttribute<MetricAttributeAttribute>();

			var name = metricMeta?.DisplayName;
			if (name.IsNullOrEmpty())
				name = attributeType.GetAttributeName();
			Name = name;

			AttributeType = attributeType;
			IsPrimaryMetric = AttributeType == typeof(CompetitionBenchmarkAttribute);

			ValuesProvider = (IMetricValuesProvider)Activator.CreateInstance(typeArgs[0]);
			var enumType = typeArgs.Length < 2 ? null : typeArgs[1];
			MetricUnits = enumType == null
				? MetricUnits.Empty
				: MetricUnits.TryCreate(enumType);

			if (metricMeta != null)
			{
				SingleValueMode = metricMeta.SingleValueMode;
				Category = metricMeta.Category;
				AnnotateInplace = metricMeta.AnnotateInplace;
			}
		}
		#endregion

		#region Values
		/// <summary>Gets the metric name.</summary>
		/// <value>The metric name.</value>
		public string Name { get; }

		/// <summary>Gets the type of the metric attribute.</summary>
		/// <value>The type of the metric attribute.</value>
		[NotNull]
		public Type AttributeType { get; }

		/// <summary>Gets or sets metric category.</summary>
		/// <value>The metric category.</value>
		public string Category { get; set; }

		/// <summary>Gets the metric measurement scale.</summary>
		/// <value>The metric measurement scale.</value>
		[NotNull]
		public MetricUnits MetricUnits { get; }

		/// <summary>Gets the metric values provider.</summary>
		/// <value>The metric values provider.</value>
		[NotNull]
		public IMetricValuesProvider ValuesProvider { get; }

		/// <summary>How single-value annotations are threated</summary>
		/// <value>How single-value annotations are threated.</value>
		public MetricSingleValueMode SingleValueMode { get; }

		/// <summary>The attribute should be added to the line with <see cref="CompetitionBenchmarkAttribute" />.</summary>
		/// <value>
		/// <c>true</c> if the attribute should be added to the line with <see cref="CompetitionBenchmarkAttribute" />; otherwise, <c>false</c>.</value>
		public bool AnnotateInplace { get; }

		/// <summary>Gets a value indicating whether the metric is relative.</summary>
		/// <value> <c>true</c> if the metric is relative; otherwise, <c>false</c>. </value>
		public bool IsRelative => ValuesProvider.ResultIsRelative;

		/// <summary>
		/// Gets a value indicating whether this instance is a primary metric 
		/// (used to annotate the <see cref="CompetitionBenchmarkAttribute"/>).
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is primary metric; otherwise, <c>false</c>.
		/// </value>
		public bool IsPrimaryMetric { get; }
		#endregion

		/// <summary>Gets column provider for the metric values.</summary>
		/// <returns>Column provider for the metric values</returns>
		[CanBeNull]
		public IColumnProvider GetColumnProvider() => ValuesProvider.GetColumnProvider(this);

		/// <summary>Gets diagnosers for the metric values.</summary>
		/// <returns>Diagnosers for the metric values</returns>
		[NotNull]
		public IDiagnoser[] GetDiagnosers() => ValuesProvider.GetDiagnosers(this);

		/// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString() => Name;
	}

	/// <summary>Typed competition metric description.</summary>
	/// <typeparam name="TAttribute">
	/// Type of the attribute used for metric annotation.
	/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or 
	/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
	/// you can use <see cref="MetricBaseAttribute"/> as a base implementation.
	/// </typeparam>
	public sealed class CompetitionMetricInfo<TAttribute> : CompetitionMetricInfo
		where TAttribute : Attribute, IStoredMetricSource
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricInfo{TAttribute}" /> class.</summary>
		internal CompetitionMetricInfo() : base(typeof(TAttribute)) { }
	}
}