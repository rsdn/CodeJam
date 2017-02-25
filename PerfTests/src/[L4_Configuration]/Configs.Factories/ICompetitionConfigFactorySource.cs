using System;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>Base interface for <see cref="ICompetitionConfigFactory"/> attributes.</summary>
	public interface ICompetitionConfigFactorySource
	{
		/// <summary>The competition config factory.</summary>
		/// <value>The competition config factory.</value>
		ICompetitionConfigFactory ConfigFactory { get; }
	}
}