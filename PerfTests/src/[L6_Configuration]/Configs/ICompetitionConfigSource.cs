using System;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Base interface for config provider attributes.</summary>
	public interface ICompetitionConfigSource
	{
		/// <summary>The competition config.</summary>
		/// <value>The competition config.</value>
		ICompetitionConfig Config { get; }
	}
}