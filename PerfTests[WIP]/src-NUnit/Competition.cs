using System;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>NUnit runner for competition performance tests.</summary>
	[PublicAPI]
	public static class Competition
	{
		private static CompetitionRunnerBase Runner => new NUnitCompetitionRunner();

		#region Public API (expose these via Competiton classes)

		#region With competition features
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([CanBeNull] CompetitionFeatures competitionFeatures = null)
			where T : class =>
				Runner.Run<T>(competitionFeatures);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>(
			[NotNull] T thisReference,
			[CanBeNull] CompetitionFeatures competitionFeatures = null)
			where T : class =>
				Runner.Run(thisReference, competitionFeatures);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				Runner.Run(benchmarkType, competitionFeatures);
		#endregion

		#region With config
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([NotNull] ICompetitionConfig competitionConfig)
			where T : class =>
				Runner.Run<T>(competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>(
			[NotNull] T thisReference,
			[NotNull] ICompetitionConfig competitionConfig)
			where T : class =>
				Runner.Run(thisReference, competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(benchmarkType, competitionConfig);
		#endregion

		#endregion
	}
}