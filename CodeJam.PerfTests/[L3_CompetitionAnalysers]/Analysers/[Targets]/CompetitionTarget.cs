using System;
using System.Collections.Generic;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Competition descriptor.</summary>
	/// <seealso cref=".Descriptor"/>
	public class CompetitionTarget
	{
		#region Fields & .ctor
		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget" /> class.</summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <param name="metricValues">Competition metric values for the descriptor.</param>
		public CompetitionTarget(
			[NotNull] Descriptor descriptor,
			[NotNull] IReadOnlyList<CompetitionMetricValue> metricValues)
		{
			Code.NotNull(descriptor, nameof(descriptor));
			Code.NotNull(metricValues, nameof(metricValues));

			Descriptor = descriptor;
			MetricValues = metricValues;
		}
		#endregion

		#region Properties
		/// <summary>Gets benchmark descriptor.</summary>
		/// <value>The benchmark descriptor.</value>
		[NotNull]
		public Descriptor Descriptor { get; }

		/// <summary>Gets metric values for the descriptor.</summary>
		/// <value>The metric values for the descriptor.</value>
		[NotNull]
		public IReadOnlyList<CompetitionMetricValue> MetricValues { get; }

		/// <summary>Determines whether the descriptor is baseline.</summary>
		/// <value><c>true</c> if the benchmark descriptor is baseline.</value>
		public bool Baseline => Descriptor.Baseline;
		#endregion
	}
}