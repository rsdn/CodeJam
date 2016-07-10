using System;
using System.Collections.Generic;
using System.Reflection;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Console
{
	/// <summary>Console competition runner that does not support integration with unit tests.</summary>
	[PublicAPI]
	public static class ConsoleCompetition
	{
		private static CompetitionRunnerBase Runner => new ConsoleCompetitionRunner();

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([NotNull] T thisReference, [CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				Runner.Run(thisReference, competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				Runner.Run<T>(competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(benchmarkType, competitionConfig);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> RunCore(
			[NotNull] Assembly assembly,
			[CanBeNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(assembly, competitionConfig);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">Benchmark classes to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> RunCore(
			[NotNull] Type[] benchmarkTypes,
			[CanBeNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(benchmarkTypes, competitionConfig);
	}
}