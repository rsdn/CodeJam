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
		/// <summary>Initializes a new instance of the <see cref="MetricInfoAttribute" /> class.</summary>
		/// <param name="category">The category of the metric.</param>
		public MetricInfoAttribute([NotNull] string category)
			: this(category, MetricSingleValueMode.FromInfinityToMax) { }

		/// <summary>Initializes a new instance of the <see cref="MetricInfoAttribute"/> class.</summary>
		/// <param name="category">The category of the metric.</param>
		/// <param name="singleValueMode">The single value treatment mode.</param>
		public MetricInfoAttribute([NotNull] string category, MetricSingleValueMode singleValueMode)
		{
			Code.NotNullNorEmpty(category, nameof(category));

			Category = category;
			SingleValueMode = singleValueMode;
			MetricColumns = MetricValueColumns.Default;
		}

		/// <summary>Gets category of the metric.</summary>
		/// <value>The category of the metric.</value>
		[CanBeNull]
		public string Category { get; }

		/// <summary>Gets single value treatment mode.</summary>
		/// <value>The single value treatment mode.</value>
		public MetricSingleValueMode SingleValueMode { get; }

		/// <summary>Gets or sets display name of the metric.</summary>
		/// <value>The display name of the metric.</value>
		[CanBeNull]
		public string DisplayName { get; set; }

		/// <summary>Gets columns to include into summary output.</summary>
		/// <value>The columns to include into summary output.</value>
		public MetricValueColumns MetricColumns { get; set; }

		/// <summary>
		/// Gets or sets in-place annotation mode (all in-place attributes for same category will be placed at the same line).
		/// </summary>
		/// <value>
		/// <c>true</c> if the in-place annotation mode is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool AnnotateInPlace { get; set; }
	}
}