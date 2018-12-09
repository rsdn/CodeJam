using System;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs.Factories
{
	/// <summary>Competition config factory</summary>
	public interface ICompetitionConfigFactory
	{
		/// <summary>Creates competition config for type.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		ICompetitionConfig Create(
			[CanBeNull] Assembly targetAssembly,
			[CanBeNull] CompetitionFeatures competitionFeatures);

		/// <summary>Creates competition config for type.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		ICompetitionConfig Create(
			[CanBeNull] Type benchmarkType,
			[CanBeNull] CompetitionFeatures competitionFeatures);
	}
}