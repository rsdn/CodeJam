using System;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>Base interface for <see cref="ICompetitionModifier"/> attributes.</summary>
	public interface ICompetitionModifierSource
	{
		/// <summary>The competition config modifier.</summary>
		/// <value>The competition config modifier.</value>
		ICompetitionModifier Modifier { get; }
	}
}