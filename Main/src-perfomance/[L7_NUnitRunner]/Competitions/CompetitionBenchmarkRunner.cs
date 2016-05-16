using System;
using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Competitions.Core;

using JetBrains.Annotations;

using NUnit.Framework;

using static BenchmarkDotNet.Running.Competitions.Core.CompetitionRunnerCore;

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
		public static void Run<T>(T thisReference, IConfig config) where T : class =>
			RunCompetition(thisReference.GetType(), config);

		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void Run<T>(IConfig config) where T : class =>
			RunCompetition(typeof(T), config);
		#endregion

		#region Core logic
		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void RunCompetition(
			Type benchmarkType, IConfig config)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: fixing the https://github.com/nunit/nunit3-vs-adapter/issues/96
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
				}
				RunCompetitionUnderSetup(benchmarkType, config);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}

		private static void RunCompetitionUnderSetup(
			Type benchmarkType, IConfig baseConfig)
		{
			ValidateCompetitionSetup(benchmarkType);

			// Capturing the output
			var logger = InitAccumulationLogger();
			// Final config
			var competitionConfig = CreateCompetitionConfig(baseConfig, logger);

			CompetitionState competitionState = null;
			try
			{
				var parameters = competitionConfig.GetValidators()
					.OfType<CompetitionParameters>()
					.Single();
				competitionState = CompetitionCore.Run(benchmarkType, competitionConfig, parameters.MaxRerunCount);
			}
			finally
			{
				DumpOutputSummaryAtTop(competitionState?.LastRunSummary, logger);
			}

			ReportMessagesToUser(
				competitionState,
				msg => { throw new AssertionException(msg); },
				msg => { throw new IgnoreException(msg); });
		}

		private static void DumpOutputSummaryAtTop(Summary summary, AccumulationLogger logger)
		{
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, ConsoleLogger.Default);
			}

			// Dumping all captured output below the benchmark results
			ConsoleLogger.Default.WriteLine(logger.GetLog());
		}
		#endregion
	}
}