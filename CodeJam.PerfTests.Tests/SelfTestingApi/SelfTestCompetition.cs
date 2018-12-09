using System;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	internal static class SelfTestCompetition
	{
		private static CompetitionRunnerBase Runner => new SelfTestCompetitionRunner();

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		public static CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig = null)
			where T : class =>
				Runner.Run<T>(competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		public static CompetitionState Run<T>(
			[NotNull] T thisReference,
			[CanBeNull] ICompetitionConfig competitionConfig = null)
			where T : class =>
				Runner.Run(thisReference, competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig = null) =>
				Runner.Run(benchmarkType, competitionConfig);
	}
}