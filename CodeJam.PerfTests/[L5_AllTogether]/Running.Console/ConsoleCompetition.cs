using System;
using System.Collections.Generic;
using System.Reflection;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Console
{
	/// <summary>Console competition runner.</summary>
	[PublicAPI]
	public static class ConsoleCompetition
	{
		private static CompetitionRunnerBase Runner => new ConsoleCompetitionRunner();

		#region Public API (expose these via Competiton classes)

		#region With competition features
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([CanBeNull] CompetitionFeatures competitionFeatures = null)
			where T : class =>
				Runner.Run<T>(competitionFeatures);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
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
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
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
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run<T>([NotNull] ICompetitionConfig competitionConfig)
			where T : class =>
				Runner.Run<T>(competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
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
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public static CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(benchmarkType, competitionConfig);
		#endregion

		#endregion

		#region Advanced public API (expose these if you wish)

		#region With competition features
		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Assembly assembly,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				Runner.Run(assembly, competitionFeatures);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">BenchmarkCase classes to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Type[] benchmarkTypes,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				Runner.Run(benchmarkTypes, competitionFeatures);
		#endregion

		#region With config
		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Assembly assembly,
			[NotNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(assembly, competitionConfig);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">BenchmarkCase classes to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public static IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Type[] benchmarkTypes,
			[NotNull] ICompetitionConfig competitionConfig) =>
				Runner.Run(benchmarkTypes, competitionConfig);
		#endregion

		#endregion
	}
}