using System;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Console competition runner that does not support integration with unit tests.</summary>
	[PublicAPI]
	public abstract class CompetitionApi<TRunner>
		where TRunner: CompetitionRunnerBase, new()
	{
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[NotNull]
		public static CompetitionState Run<T>([NotNull] T thisReference, [CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(thisReference.GetType(), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[NotNull]
		public static CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(typeof(T), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[NotNull]
		private static CompetitionState RunCompetition(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			var runner = new TRunner();
			return runner.RunCompetition(benchmarkType, competitionConfig);
		}
	}
}