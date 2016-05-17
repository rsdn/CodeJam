using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Configs;
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
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Run<T>(T thisReference) where T : class =>
			RunCompetition(thisReference.GetType(), null);

		/// <summary>
		/// Runs the competition benchmark from a type of a callee
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Run<T>(T thisReference, ICompetitionConfig config) where T : class =>
			RunCompetition(thisReference.GetType(), config);

		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void Run<T>(ICompetitionConfig config) where T : class =>
			RunCompetition(typeof(T), config);
		#endregion

		#region Core logic
		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void RunCompetition(
			Type benchmarkType, ICompetitionConfig config)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: fixing the https://github.com/nunit/nunit3-vs-adapter/issues/96
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
				}

				var runner = new NUnitCompetitionRunner();
				runner.RunCompetition(benchmarkType, config);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion
	}
}