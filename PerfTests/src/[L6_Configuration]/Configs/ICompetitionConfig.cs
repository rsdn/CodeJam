using System;

using BenchmarkDotNet.Configs;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition config.</summary>
	public interface ICompetitionConfig : IConfig
	{
		/// <summary>Competition parameters.</summary>
		/// <value>Competition parameters.</value>
		CompetitionOptions Options { get; }
	}
}