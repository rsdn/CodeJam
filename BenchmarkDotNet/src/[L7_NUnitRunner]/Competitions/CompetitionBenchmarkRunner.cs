using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Competitions.Core;

using JetBrains.Annotations;

using NUnit.Framework;

namespace BenchmarkDotNet.Competitions
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
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: https://github.com/nunit/nunit3-vs-adapter/issues/38
				// NUnit 3.0 does not alter current directory at all.
				// So, we had to do it ourselves.
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.WorkDirectory;
				}

				var runner = new NUnitCompetitionRunner();
				return runner.RunCompetition(benchmarkType, config);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion
	}
}