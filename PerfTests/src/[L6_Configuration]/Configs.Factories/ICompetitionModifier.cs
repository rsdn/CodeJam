using System;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>Competition config modifier.</summary>
	public interface ICompetitionModifier
	{
		/// <summary>Updates competition config.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		void Modify(ManualCompetitionConfig competitionConfig);
	}
}