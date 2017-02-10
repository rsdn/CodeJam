using System;
using System.Linq;

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
		// TODO: reference relative -> absolute metric? We need it to troubleshoot relative time adjustments.

		#region Known metrics
		/// <summary>The relative time metric. </summary>
		public static readonly CompetitionMetricInfo<CompetitionBenchmarkAttribute> RelativeTime =
			new CompetitionMetricInfo<CompetitionBenchmarkAttribute>(nameof(RelativeTime));

		/// <summary>The absolute time metic, in nanoseconds.</summary>
		public static readonly CompetitionMetricInfo<ExpectedTimeAttribute> AbsoluteTime =
			new CompetitionMetricInfo<ExpectedTimeAttribute>();

		/// <summary>Gc allocations metic, in bytes.</summary>
		public static readonly CompetitionMetricInfo<GcAllocationsAttribute> GcAllocations =
			new CompetitionMetricInfo<GcAllocationsAttribute>();
		#endregion

		#region Helpers
		private static Type[] GetMetricAttributeTypeArgs(Type attributeType)
		{
			Type[] typeArgs = null;

			var attGenericInterfaces = attributeType.GetInterfaces()
				.Where(t=>t.IsGenericType)
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
		/// <param name="name">The metric name. If <c>null</c>, name of <paramref name="attributeType"/> is used.</param>
		/// <param name="attributeType">
		/// Type of the attribute used for metric annotation.
		/// Should implement <see cref="IMetricAttribute{TMetricProvider}"/> or 
		/// <see cref="IMetricAttribute{TMetricProvider, TUnitOfMeasurement}"/>;
		/// you can use <see cref="MetricBaseAttribute"/> as a base implementation.
		/// </param>
		/// <returns>Composite range that describes measurement units</returns>
		protected CompetitionMetricInfo(
			[CanBeNull] string name,
			[NotNull] Type attributeType)
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

			if (name.IsNullOrEmpty())
			{
				name = attributeType.GetAttributeName();
			}
			Name = name;

			AttributeType = attributeType;

			ValuesProvider = (IMetricValuesProvider)Activator.CreateInstance(typeArgs[0]);

			var enumType = typeArgs.Length < 2 ? null : typeArgs[1];
			MetricUnits = enumType == null
				? MetricUnits.Empty
				: MetricUnits.TryCreate(enumType);
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

		/// <summary>Gets the metric measurement scale.</summary>
		/// <value>The metric measurement scale.</value>
		[NotNull]
		public MetricUnits MetricUnits { get; }

		/// <summary>Gets the metric values provider.</summary>
		/// <value>The metric values provider.</value>
		[NotNull]
		public IMetricValuesProvider ValuesProvider { get; }

		/// <summary>Gets a value indicating whether the metric is relative.</summary>
		/// <value> <c>true</c> if the metric is relative; otherwise, <c>false</c>. </value>
		public bool IsRelative => ValuesProvider.ResultIsRelative;
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
		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricInfo"/> class.</summary>
		/// <returns>Composite range that describes measurement units</returns>
		public CompetitionMetricInfo()
			: base(null, typeof(TAttribute)) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricInfo"/> class.</summary>
		/// <param name="name">The metric name. If <c>null</c>, name of <typeparamref name="TAttribute"/> is used.</param>
		/// <returns>Composite range that describes measurement units</returns>
		public CompetitionMetricInfo([CanBeNull] string name)
			: base(name, typeof(TAttribute)) { }
	}
}