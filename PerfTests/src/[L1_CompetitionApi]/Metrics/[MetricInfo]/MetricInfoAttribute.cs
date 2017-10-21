using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	// ReSharper disable once RedundantAttributeUsageProperty
	/// <summary>Options for metric attribute.</summary>
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MetricInfoAttribute : Attribute
	{
		/// <summary>Initializes a new instance of the <see cref="MetricInfoAttribute"/> class.</summary>
		/// <param name="category">The category of the metric.</param>
		public MetricInfoAttribute([NotNull] string category)
			: this(category, DefaultMinMetricValue.NegativeInfinity) { }

		/// <summary>Initializes a new instance of the <see cref="MetricInfoAttribute"/> class.</summary>
		/// <param name="category">The category of the metric.</param>
		/// <param name="defaultMinValue">Min value to be used by default.</param>
		public MetricInfoAttribute([NotNull] string category, DefaultMinMetricValue defaultMinValue)
		{
			Code.NotNullNorEmpty(category, nameof(category));

			Category = category;
			DefaultMinValue = defaultMinValue;
			MetricColumns = MetricValueColumns.Default;
		}

		/// <summary>Gets category of the metric.</summary>
		/// <value>The category of the metric.</value>
		[NotNull]
		public string Category { get; }

		/// <summary>Gets default min value behavior.</summary>
		/// <value>The default min value behavior.</value>
		public DefaultMinMetricValue DefaultMinValue { get; }

		/// <summary>Gets or sets display name of the metric.</summary>
		/// <value>The display name of the metric.</value>
		[CanBeNull]
		public string DisplayName { get; set; }

		/// <summary>Gets columns to include into summary output.</summary>
		/// <value>The columns to include into summary output.</value>
		public MetricValueColumns MetricColumns { get; set; }

		/// <summary>
		/// Place attribute annotation on a same line with same category attributes.
		/// </summary>
		/// <value>
		/// <c>true</c> if the attribute should be placed on same line with other attributes that belongs to the <see cref="Category"/>; otherwise, <c>false</c>.
		/// </value>
		public bool CompactAttributeAnnotations { get; set; }
	}
}