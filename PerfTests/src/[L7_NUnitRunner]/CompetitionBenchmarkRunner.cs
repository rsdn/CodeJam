using System;

using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>NUnit runner for competition performance tests</summary>
	[PublicAPI]
	public static class CompetitionBenchmarkRunner
	{
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[CanBeNull]
		public static Summary Run<T>([NotNull] T thisReference, [CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(thisReference.GetType(), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[CanBeNull]
		public static Summary Run<T>([CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(typeof(T), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The summary for the last completed run.</returns>
		[CanBeNull]
		public static Summary RunCompetition(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			var runner = new NUnitCompetitionRunner();
			return runner.RunCompetition(benchmarkType, competitionConfig)
				?.LastRunSummary;
		}
	}
}