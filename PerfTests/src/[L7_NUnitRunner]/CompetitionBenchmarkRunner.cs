using System;

using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Runner for competition performance tests
	/// </summary>
	[PublicAPI]
	public static class CompetitionBenchmarkRunner
	{
		#region Public API
		/// <summary>
		/// Runs the competition benchmark from a type of a callee
		/// </summary>
		public static Summary Run<T>(T thisReference, ICompetitionConfig config) where T : class =>
			RunCompetition(thisReference.GetType(), config);

		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static Summary Run<T>(ICompetitionConfig config) where T : class =>
			RunCompetition(typeof(T), config);
		#endregion

		#region Core logic
		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static Summary RunCompetition(
			Type benchmarkType, ICompetitionConfig config)
		{
			var runner = new NUnitCompetitionRunner();
			return runner.RunCompetition(benchmarkType, config)
				?.LastRunSummary;
		}
		#endregion
	}
}