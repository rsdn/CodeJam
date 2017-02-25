using System;
using System.Collections.Generic;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Wrapper class for readonly competition config.</summary>
	/// <seealso cref="ReadOnlyConfig"/>
	/// <seealso cref="ICompetitionConfig"/>
	public class ReadOnlyCompetitionConfig : ReadOnlyConfig, ICompetitionConfig
	{
		#region Fields & .ctor
		private readonly ICompetitionConfig _config;

		/// <summary>Initializes a new instance of the <see cref="ReadOnlyCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to wrap.</param>
		public ReadOnlyCompetitionConfig([NotNull] ICompetitionConfig config) : base(config)
		{
			Code.NotNull(config, nameof(config));

			_config = config;
		}
		#endregion

		/// <summary>Competition parameters.</summary>
		/// <value>Competition parameters.</value>
		public CompetitionOptions Options => _config.Options;

		/// <summary>Gets competition metrics.</summary>
		/// <returns>Competition metrics.</returns>
		public IEnumerable<MetricInfo> GetMetrics() => _config.GetMetrics();
	}
}