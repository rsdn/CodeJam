using System;
using System.Collections.Generic;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Competition target.</summary>
	/// <seealso cref="Target"/>
	public class CompetitionTarget
	{
		#region Fields & .ctor
		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget" /> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="metricValues">Competition metric values for the target.</param>
		public CompetitionTarget(
			[NotNull] Target target,
			[NotNull] IReadOnlyList<CompetitionMetricValue> metricValues)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNull(metricValues, nameof(metricValues));

			Target = target;
			MetricValues = metricValues;
		}
		#endregion

		#region Properties
		/// <summary>Gets benchmark target.</summary>
		/// <value>The benchmark target.</value>
		[NotNull]
		public Target Target { get; }

		/// <summary>Gets metric values for the target.</summary>
		/// <value>The metric values for the target.</value>
		[NotNull]
		public IReadOnlyList<CompetitionMetricValue> MetricValues { get; }

		/// <summary>Determines whether the target is baseline.</summary>
		/// <value><c>true</c> if the benchmark target is baseline.</value>
		public bool Baseline => Target.Baseline;
		#endregion
	}
}