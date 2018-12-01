using System;
using System.Collections.Generic;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition config.</summary>
	public interface ICompetitionConfig : IConfig
	{
		/// <summary>Competition parameters.</summary>
		/// <value>Competition parameters.</value>
		[NotNull] CompetitionOptions Options { get; }

		/// <summary>Gets competition metrics.</summary>
		/// <returns>Competition metrics.</returns>
		[NotNull] IEnumerable<MetricInfo> GetMetrics();
	}
}