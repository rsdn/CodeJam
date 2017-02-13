using System;

namespace CodeJam.PerfTests.Metrics
{
	// ReSharper disable once RedundantAttributeUsageProperty
	/// <summary>Options for metric attribute.</summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MetricAttributeAttribute : Attribute
	{
		/// <summary>Initializes a new instance of the <see cref="MetricAttributeAttribute"/> class.</summary>
		public MetricAttributeAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="MetricAttributeAttribute" /> class.</summary>
		/// <param name="displayName">The name of the metric.</param>
		public MetricAttributeAttribute(string displayName)
		{
			DisplayName = displayName;
		}

		/// <summary>Initializes a new instance of the <see cref="MetricAttributeAttribute"/> class.</summary>
		/// <param name="singleValueMode">How single-value annotations are threated.</param>
		public MetricAttributeAttribute(MetricSingleValueMode singleValueMode)
		{
			SingleValueMode = singleValueMode;
		}

		/// <summary>Gets or sets the name of the metric.</summary>
		/// <value>The name of the metric.</value>
		public string DisplayName { get; set; }

		/// <summary>Gets or sets metric category.</summary>
		/// <value>The metric category.</value>
		public string Category { get; set; }

		/// <summary>How single-value annotations are threated</summary>
		/// <value>How single-value annotations are threated.</value>
		public MetricSingleValueMode SingleValueMode { get; set; }

		/// <summary>The attribute should be added to the line with <see cref="CompetitionBenchmarkAttribute" />.</summary>
		/// <value>
		/// <c>true</c> if the attribute should be added to the line with <see cref="CompetitionBenchmarkAttribute" />; otherwise, <c>false</c>.</value>
		public bool AnnotateInplace { get; set; }
	}
}