using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// <summary>Transport class for stored target info values.</summary>
	/// </summary>
	internal class StoredTargetInfo
	{
		/// <summary>Initializes a new instance of the <see cref="StoredTargetInfo"/> class.</summary>
		/// <param name="metricValues">The stored metric values.</param>
		public StoredTargetInfo(
			[NotNull] IReadOnlyList<StoredMetricValue> metricValues): this(metricValues, null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="StoredTargetInfo"/> class.</summary>
		/// <param name="metricValues">The stored metric values.</param>
		/// <param name="baseline">
		/// <c>true</c> if the target is baseline; 
		/// <c>null</c> if baseline info is not persisted; 
		/// otherwise, <c>false</c>.
		/// </param>
		public StoredTargetInfo(
			[NotNull] IReadOnlyList<StoredMetricValue> metricValues,
			bool? baseline)
		{
			Code.NotNull(metricValues, nameof(metricValues));
			MetricValues = metricValues;
			Baseline = baseline;
		}

		/// <summary>Gets stored metric values.</summary>
		/// <value>The stored metric values.</value>
		public IReadOnlyList<StoredMetricValue> MetricValues { get; }

		/// <summary>Gets a value indicating whether the target is a baseline.</summary>
		/// <value>
		/// <c>true</c> if the target is baseline; 
		/// <c>null</c> if baseline info is not persisted; 
		/// otherwise, <c>false</c>.
		/// </value>
		public bool? Baseline { get; }
	}
}