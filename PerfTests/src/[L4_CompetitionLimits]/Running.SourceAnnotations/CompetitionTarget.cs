using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Analysers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Stores metrics for benchmark target
	/// </summary>
	/// <seealso cref="Target"/>
	internal class CompetitionTarget
	{
		#region Fields & .ctor
		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="metricValues">Competition metric values for the target.</param>
		public CompetitionTarget(
			[NotNull] Target target,
			[NotNull] IReadOnlyCollection<CompetitionMetricValue> metricValues)
		{
			Target = target;
			TargetKey = new TargetCacheKey(target.Type.TypeHandle, target.Method.MethodHandle);
			MetricValues = metricValues.ToArray();
		}
		#endregion

		#region Properties
		/// <summary>The benchmark target.</summary>
		/// <value>The benchmark target.</value>
		[NotNull]
		public Target Target { get; }

		/// <summary>The target cache key.</summary>
		/// <value>The target cache key.</value>
		public TargetCacheKey TargetKey { get; }

		/// <summary>The metric values for the target.</summary>
		/// <value>The metric values for the target.</value>
		public IReadOnlyList<CompetitionMetricValue> MetricValues { get; }

		/// <summary>The benchmark target is baseline.</summary>
		/// <value><c>true</c> if the benchmark target is baseline.</value>
		public bool Baseline => Target.Baseline;
		#endregion
	}
}